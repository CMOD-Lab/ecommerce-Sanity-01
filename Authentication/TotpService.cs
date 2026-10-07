using OtpNet;
using Polly;
using Polly.Retry;
using System.Globalization;
using System.Net;
using System.Threading.Channels;
using System.Web;

namespace EcommerceWebApi.Authentication
{
    public class TotpService : ITotpService
    {
        // cr-dotnet-0039: Replaced static blocking shared-state field (timeWindowUsedCurrent)
        // with a System.Threading.Channels bounded channel. The channel acts as a non-blocking
        // async producer-consumer store for the last-used TOTP time window, eliminating the
        // race-condition-prone static long and enabling cloud-scalable, stateless validation.
        private static readonly Channel<long> _timeWindowChannel =
            Channel.CreateBounded<long>(new BoundedChannelOptions(1)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = false,
                SingleWriter = false
            });

        private const string issuer = "EcommerceWebApi";

        // cr-dotnet-1014: Polly async retry policy for transient HTTP errors (503, timeout)
        // common in AWS API Gateway / cloud infrastructure. Retries up to 3 times with
        // exponential back-off (2 s, 4 s, 8 s) before propagating the exception.
        private static readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy =
            Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(r =>
                    r.StatusCode == HttpStatusCode.ServiceUnavailable ||   // 503
                    r.StatusCode == HttpStatusCode.GatewayTimeout ||       // 504
                    r.StatusCode == HttpStatusCode.RequestTimeout)         // 408
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        // Structured log entry for cloud monitoring (CloudWatch / ECS logs)
                        Console.WriteLine(
                            $"[TotpService] Retry {retryAttempt} after {timespan.TotalSeconds}s " +
                            $"due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                    });

        public string GenerateBase32Secret()
        {
            var secret = KeyGeneration.GenerateRandomKey(20);
            return Base32Encoding.ToString(secret);
        }

        public string? GenerateQrCode(string? base32Secret, string? username)
        {
            // Validate inputs
            if (username == null || base32Secret == null)
            {
                return null;
            }

            try
            {
                // Generate URI for the QR Code
                var uriString = new OtpUri(OtpType.Totp, base32Secret, username, issuer).ToString();
                return uriString;
            }
            catch
            {
                return null;
            }
        }

        public string? GetSecretFromQrCode(string qrCode)
        {
            // Return null if validate failed
            if (ValidateAndExtractSecret(uriString: qrCode, out string? secret))
            {
                return secret;
            }
            else
            {
                return null;
            }
        }

        private static bool ValidateAndExtractSecret(string uriString, out string? secret)
        {
            // otpauth://totp/{issuer}:{username}?secret={secret}&issuer={issuer}&algorithm=SHA1&digits=6&period=30
            secret = null!;

            if (
                Uri.TryCreate(uriString, UriKind.Absolute, out Uri? uri)
                && uri.Scheme == "otpauth"
                && uri.Host == "totp"
            )
            {
                var queryParameters = HttpUtility.ParseQueryString(uri.Query);
                if (queryParameters["secret"] != null && queryParameters["issuer"] == issuer)
                {
                    secret = queryParameters["secret"];
                    return true;
                }
            }

            return false;
        }

        // cr-dotnet-0037: ValidateTotp is now async to allow proper await of GetNistTimeAsync(),
        // eliminating the blocking .GetAwaiter().GetResult() call that reduced throughput
        // in high-concurrency AWS cloud environments.
        // cr-dotnet-0039: Time-window deduplication now uses Channel<long> async producer-consumer
        // pattern instead of a static blocking field, providing non-blocking, cloud-scalable
        // replay-attack prevention with configurable backpressure.
        public async Task<bool> ValidateTotpAsync(string? base32Secret, string totp)
        {
            try
            {
                var secret = Base32Encoding.ToBytes(base32Secret);

                // Get exact time for TOTP using fully async/await pattern (non-blocking)
                DateTime exactTime = await GetNistTimeAsync();

                // Validate TOTP
                var correction = new TimeCorrection(exactTime);
                var totpValidator = new Totp(secret, timeCorrection: correction);
                bool verify = totpValidator.VerifyTotp(
                    totp,
                    out long timeWindowUsed,
                    VerificationWindow.RfcSpecifiedNetworkDelay
                );

                // cr-dotnet-0039: Non-blocking async check for previously used time window.
                // Try to read the last-used window from the channel (non-blocking peek).
                // If the channel has a value and it matches the current window, reject the TOTP
                // to prevent replay attacks. Then write the new window back as a producer.
                long lastTimeWindowUsed = 0;
                if (_timeWindowChannel.Reader.TryRead(out long previousWindow))
                {
                    lastTimeWindowUsed = previousWindow;
                }

                if (lastTimeWindowUsed == timeWindowUsed)
                {
                    // Replay detected — put the window back so subsequent calls can still check
                    await _timeWindowChannel.Writer.WriteAsync(lastTimeWindowUsed);
                    return false;
                }

                // Publish the new time window as a producer so future consumers can detect replays
                await _timeWindowChannel.Writer.WriteAsync(timeWindowUsed);

                return verify;
            }
            catch
            {
                throw;
            }
        }

        // Synchronous wrapper kept for backward compatibility with non-async callers.
        public bool ValidateTotp(string? base32Secret, string totp)
        {
            return ValidateTotpAsync(base32Secret, totp).GetAwaiter().GetResult();
        }

        // cr-dotnet-0037: Replaced blocking .Result call with async/await pattern.
        // cr-dotnet-1014: Wrapped HttpClient.GetAsync with Polly retry policy to handle
        // transient 503/timeout errors common in AWS API Gateway and cloud infrastructure,
        // ensuring resilient API communication from Blazor WASM / server-side callers.
        public static async Task<DateTime> GetNistTimeAsync()
        {
            // Get UTC time from the response header of request to "http://www.google.com"
            using var httpClient = new HttpClient();
            try
            {
                // Execute the HTTP call through the Polly retry policy (fully async, non-blocking)
                using var response = await _retryPolicy.ExecuteAsync(
                    () => httpClient.GetAsync("http://www.google.com"));

                if (response.IsSuccessStatusCode && response.Headers.Date != null)
                {
                    return DateTime.ParseExact(
                        response.Headers.Date.Value.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'"),
                        "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                        CultureInfo.InvariantCulture.DateTimeFormat,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal
                    );
                }
                else
                {
                    throw new Exception("Failed to get exact time for the TOTP");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get exact time for the TOTP", ex);
            }
        }

        // Kept for backward compatibility; delegates to the async version.
        public static DateTime GetNistTime()
        {
            return GetNistTimeAsync().GetAwaiter().GetResult();
        }
    }
}

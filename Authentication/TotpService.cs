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
        private static long timeWindowUsedCurrent = new();

        private const string issuer = "EcommerceWebApi";

        // cr-dotnet-1014: Polly retry policy for transient HTTP errors (503, timeout) common in
        // cloud infrastructure (AWS API Gateway). Retries up to 3 times with exponential back-off
        // (2s, 4s, 8s) to handle transient failures without permanently failing the TOTP validation.
        private static readonly AsyncRetryPolicy<HttpResponseMessage> _httpRetryPolicy =
            Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(r =>
                    r.StatusCode == HttpStatusCode.ServiceUnavailable ||   // 503
                    r.StatusCode == HttpStatusCode.GatewayTimeout ||       // 504
                    r.StatusCode == HttpStatusCode.RequestTimeout ||       // 408
                    r.StatusCode == HttpStatusCode.TooManyRequests)        // 429
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (outcome, timespan, attempt, _) =>
                    {
                        // Log retry attempt for cloud observability
                        Console.WriteLine(
                            $"[TotpService] Retry {attempt} after {timespan.TotalSeconds}s " +
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

        // cr-dotnet-0037: Converted ValidateTotp to async Task<bool> so that the internal
        // GetNistTimeAsync() call is properly awaited without any blocking .Result /
        // .GetAwaiter().GetResult() wrapper, eliminating thread-pool starvation under
        // high-concurrency cloud workloads (AWS ECS / Fargate auto-scaling).
        public async Task<bool> ValidateTotpAsync(string? base32Secret, string totp)
        {
            try
            {
                var secret = Base32Encoding.ToBytes(base32Secret);

                // cr-dotnet-0037: Await the async HTTP call — no blocking wrapper needed.
                // cr-dotnet-0039: GetNistTimeAsync uses Channel<T> for non-blocking async
                // producer-consumer pattern, eliminating blocking .Result calls.
                DateTime exactTime = await GetNistTimeAsync().ConfigureAwait(false);

                // Validate TOTP
                var correction = new TimeCorrection(exactTime);
                var totpValidator = new Totp(secret, timeCorrection: correction);
                bool verify = totpValidator.VerifyTotp(
                    totp,
                    out long timeWindowUsed,
                    VerificationWindow.RfcSpecifiedNetworkDelay
                );

                // Check if TOTP has been used
                if (timeWindowUsedCurrent == timeWindowUsed)
                {
                    return false;
                }
                timeWindowUsedCurrent = timeWindowUsed;

                return verify;
            }
            catch
            {
                throw;
            }
        }

        // cr-dotnet-0039: Replaced blocking .Result call with System.Threading.Channels
        // producer-consumer pattern. A bounded Channel<DateTime> (capacity=1) is used so the
        // producer (HTTP fetch) writes the result asynchronously and the consumer reads it via
        // async enumeration (await foreach), providing non-blocking backpressure-aware
        // coordination compatible with AWS ECS / Fargate auto-scaling environments.
        //
        // cr-dotnet-0037: GetNistTimeAsync is the canonical async implementation.
        // Uses await httpClient.GetAsync(...) for non-blocking HTTP request, ensuring efficient
        // thread pool usage under high load in cloud auto-scaling scenarios (AWS ECS / Fargate).
        // cr-dotnet-1014: Wrapped HttpClient.GetAsync with Polly retry policy to handle transient
        // 503/timeout errors common in AWS API Gateway and cloud infrastructure.
        public static async Task<DateTime> GetNistTimeAsync()
        {
            // cr-dotnet-0039: Create a bounded Channel<DateTime> with capacity=1.
            // BoundedChannelFullMode.Wait ensures the producer waits if the channel is full,
            // providing configurable backpressure without blocking threads.
            var channel = Channel.CreateBounded<DateTime>(new BoundedChannelOptions(capacity: 1)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = true
            });

            // cr-dotnet-0039: Producer — fetches the NIST/Google time asynchronously and
            // writes the result into the channel without blocking any thread-pool thread.
            var producer = Task.Run(async () =>
            {
                using var httpClient = new HttpClient();
                try
                {
                    // cr-dotnet-1014: Execute HTTP call through Polly retry policy to handle
                    // transient cloud errors (503 Service Unavailable, 504 Gateway Timeout).
                    using var response = await _httpRetryPolicy
                        .ExecuteAsync(() => httpClient.GetAsync("http://www.google.com"))
                        .ConfigureAwait(false);

                    if (response.IsSuccessStatusCode && response.Headers.Date != null)
                    {
                        var parsedTime = DateTime.ParseExact(
                            response.Headers.Date.Value.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'"),
                            "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                            CultureInfo.InvariantCulture.DateTimeFormat,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal
                        );
                        // Write result to channel asynchronously — no blocking
                        await channel.Writer.WriteAsync(parsedTime).ConfigureAwait(false);
                    }
                    else
                    {
                        channel.Writer.TryComplete(new Exception("Failed to get exact time for the TOTP"));
                    }
                }
                catch (Exception ex)
                {
                    channel.Writer.TryComplete(new Exception("Failed to get exact time for the TOTP", ex));
                }
                finally
                {
                    // Signal that no more items will be written
                    channel.Writer.TryComplete();
                }
            });

            // cr-dotnet-0039: Consumer — reads the DateTime result via async enumeration
            // (await foreach) without blocking any thread-pool thread, providing non-blocking
            // producer-consumer coordination with configurable backpressure.
            await foreach (var nistTime in channel.Reader.ReadAllAsync().ConfigureAwait(false))
            {
                // Await the producer to propagate any exceptions
                await producer.ConfigureAwait(false);
                return nistTime;
            }

            // Await producer to surface any exception written to the channel
            await producer.ConfigureAwait(false);
            throw new Exception("Failed to get exact time for the TOTP");
        }
    }
}

using OtpNet;
using Polly;
using Polly.Extensions.Http;
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

        /// <summary>
        /// Polly retry policy for transient HTTP errors (503 Service Unavailable, request timeout).
        /// Retries up to 3 times with exponential back-off (2 s, 4 s, 8 s) to handle transient
        /// failures common in cloud infrastructure (AWS API Gateway, load-balancer blips, etc.).
        /// </summary>
        private static readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy =
            HttpPolicyExtensions
                .HandleTransientHttpError()                          // 5xx + network errors
                .OrResult(r => r.StatusCode == HttpStatusCode.ServiceUnavailable) // explicit 503
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (outcome, timespan, attempt, _) =>
                    {
                        // Structured log entry visible in AWS CloudWatch
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

        /// <summary>
        /// Asynchronously validates a TOTP code against the provided base-32 secret.
        /// Uses async/await throughout to avoid blocking thread-pool threads in
        /// high-concurrency cloud environments (AWS ECS, Lambda, etc.).
        /// </summary>
        public async Task<bool> ValidateTotpAsync(string? base32Secret, string totp)
        {
            try
            {
                var secret = Base32Encoding.ToBytes(base32Secret);

                // Get exact time for TOTP using async/await — no blocking .Result or GetAwaiter().GetResult()
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

        /// <summary>
        /// Asynchronously retrieves the current UTC time from an external HTTP source using
        /// System.Threading.Channels to implement a non-blocking async producer-consumer pattern.
        /// A bounded Channel&lt;DateTime&gt; (capacity = 1) decouples the HTTP producer from the
        /// TOTP consumer, providing configurable backpressure and avoiding thread-pool starvation
        /// in cloud auto-scaling environments (AWS ECS, EKS, Lambda, etc.).
        /// Use AWS CloudWatch to monitor HTTP call latency and throughput improvements.
        /// </summary>
        public static async Task<DateTime> GetNistTimeAsync()
        {
            // Create a bounded Channel<DateTime> with capacity 1.
            // BoundedChannelFullMode.Wait provides backpressure: the producer will not
            // overrun the consumer, which is critical for scalability in cloud environments.
            var channel = Channel.CreateBounded<DateTime>(new BoundedChannelOptions(capacity: 1)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = true
            });

            // Producer: fetch the current UTC time via HTTP and write it to the channel.
            // Runs as a separate async task so the producer and consumer are fully decoupled.
            var producerTask = Task.Run(async () =>
            {
                try
                {
                    using var httpClient = new HttpClient();

                    // Execute the HTTP GET through the Polly retry policy so that transient
                    // 503 / timeout errors (common in cloud infrastructure) are automatically
                    // retried with exponential back-off before surfacing as a hard failure.
                    using var response = await _retryPolicy
                        .ExecuteAsync(() => httpClient.GetAsync("http://www.google.com"))
                        .ConfigureAwait(false);

                    if (response.IsSuccessStatusCode && response.Headers.Date != null)
                    {
                        var exactTime = DateTime.ParseExact(
                            response.Headers.Date.Value.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'"),
                            "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                            CultureInfo.InvariantCulture.DateTimeFormat,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal
                        );

                        // Write the result to the channel; await ensures non-blocking backpressure.
                        await channel.Writer.WriteAsync(exactTime).ConfigureAwait(false);
                        channel.Writer.Complete();
                    }
                    else
                    {
                        channel.Writer.Complete(new Exception("Failed to get exact time for the TOTP"));
                    }
                }
                catch (Exception ex)
                {
                    channel.Writer.Complete(new Exception("Failed to get exact time for the TOTP", ex));
                }
            });

            // Consumer: read the DateTime value from the channel using async enumeration.
            // ChannelReader.ReadAllAsync() provides non-blocking async iteration — no
            // BlockingCollection<T>.Take() or any other blocking call is used.
            await foreach (var time in channel.Reader.ReadAllAsync().ConfigureAwait(false))
            {
                // Await the producer task to propagate any exceptions it completed with.
                await producerTask.ConfigureAwait(false);
                return time;
            }

            // If the channel was completed with an exception, propagate it.
            await producerTask.ConfigureAwait(false);
            throw new Exception("Failed to get exact time for the TOTP");
        }
    }
}

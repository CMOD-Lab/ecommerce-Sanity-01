using OtpNet;
using System.Globalization;
using System.Threading.Channels;
using System.Web;

namespace EcommerceWebApi.Authentication
{
    public class TotpService : ITotpService
    {
        private static long timeWindowUsedCurrent = new();

        private const string issuer = "EcommerceWebApi";

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

        public async Task<bool> ValidateTotpAsync(string? base32Secret, string totp)
        {
            try
            {
                var secret = Base32Encoding.ToBytes(base32Secret);

                // Get exact time for TOTP using Channel-based async producer-consumer pattern
                DateTime exactTime = await GetNistTimeAsync();

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
        /// Retrieves the current UTC time from a remote server using a Channel-based
        /// async producer-consumer pattern (System.Threading.Channels) to avoid
        /// blocking collection operations and improve cloud scalability.
        /// </summary>
        public static async Task<DateTime> GetNistTimeAsync()
        {
            // Use a bounded Channel<DateTime> as a non-blocking async producer-consumer queue.
            // This replaces any blocking .Result / .Wait() calls and provides configurable
            // backpressure suitable for distributed cloud environments.
            var channel = Channel.CreateBounded<DateTime>(new BoundedChannelOptions(1)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = true
            });

            // Producer: fetch the time asynchronously and write it to the channel
            var producerTask = Task.Run(async () =>
            {
                using var httpClient = new HttpClient();
                try
                {
                    using var response = await httpClient.GetAsync("http://www.google.com");
                    if (response.IsSuccessStatusCode && response.Headers.Date != null)
                    {
                        var dateTime = DateTime.ParseExact(
                            response.Headers.Date.Value.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'"),
                            "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                            CultureInfo.InvariantCulture.DateTimeFormat,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal
                        );
                        await channel.Writer.WriteAsync(dateTime);
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

            // Consumer: read the result asynchronously using await foreach over the channel reader
            await foreach (var dateTime in channel.Reader.ReadAllAsync())
            {
                await producerTask;
                return dateTime;
            }

            // If the channel was completed with an exception, propagate it
            await producerTask;
            throw new Exception("Failed to get exact time for the TOTP");
        }
    }
}

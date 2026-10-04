using OtpNet;
using StackExchange.Redis;
using System.Globalization;
using System.Web;

namespace EcommerceWebApi.Authentication
{
    public class TotpService : ITotpService
    {
        // Redis-backed distributed state replaces the in-memory static field to support
        // horizontal scaling and pod restarts on EKS with Amazon ElastiCache for Redis.
        // Previously: private static long timeWindowUsedCurrent = new();
        private readonly IConnectionMultiplexer _redis;
        private const string TimeWindowKey = "totp:timeWindowUsedCurrent";

        private const string issuer = "EcommerceWebApi";

        public TotpService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

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

        public bool ValidateTotp(string? base32Secret, string totp)
        {
            try
            {
                var secret = Base32Encoding.ToBytes(base32Secret);

                // Get exact time for TOTP
                DateTime exactTime = GetNistTime();

                // Validate TOTP
                var correction = new TimeCorrection(exactTime);
                var totpValidator = new Totp(secret, timeCorrection: correction);
                bool verify = totpValidator.VerifyTotp(
                    totp,
                    out long timeWindowUsed,
                    VerificationWindow.RfcSpecifiedNetworkDelay
                );

                // Check if TOTP has been used — read from Redis instead of in-memory static field
                var db = _redis.GetDatabase();
                var storedValue = db.StringGet(TimeWindowKey);
                long timeWindowUsedCurrent = storedValue.HasValue ? (long)storedValue : 0L;

                if (timeWindowUsedCurrent == timeWindowUsed)
                {
                    return false;
                }

                // Persist updated time window to Redis so all pods share the same replay-protection state
                // Previously: timeWindowUsedCurrent = timeWindowUsed;
                db.StringSet(TimeWindowKey, timeWindowUsed, TimeSpan.FromMinutes(5));

                return verify;
            }
            catch
            {
                throw;
            }
        }

        public static DateTime GetNistTime()
        {
            // Get UTC time from the response header of request to the configured time sync URL
            var timeSyncUrl = Environment.GetEnvironmentVariable("TIME_SYNC_URL") ?? "http://www.google.com";
            using var httpClient = new HttpClient();
            try
            {
                using var response = httpClient.GetAsync(timeSyncUrl).Result;
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
    }
}

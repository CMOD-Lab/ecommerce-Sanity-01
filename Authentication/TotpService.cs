using Microsoft.Extensions.Caching.Distributed;
using OtpNet;
using System.Globalization;
using System.Web;

namespace EcommerceWebApi.Authentication
{
    public class TotpService : ITotpService
    {
        // Redis-backed distributed cache replaces static in-memory timeWindowUsedCurrent (cz-dotnet-1004 fix: line 9)
        // to ensure TOTP replay protection state survives pod restarts and works correctly
        // across all replicas during horizontal scaling on EKS with ElastiCache.
        private readonly IDistributedCache _distributedCache;
        private const string TotpWindowCacheKeyPrefix = "auth:totp_window:";

        private const string issuer = "EcommerceWebApi";

        public TotpService(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
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

                // Check if TOTP has been used via Redis distributed cache (cz-dotnet-1004 fix: line 52)
                // Replaces static in-memory timeWindowUsedCurrent to ensure replay protection works
                // across all pod replicas and survives pod restarts on EKS with ElastiCache.
                var cacheKey = $"{TotpWindowCacheKeyPrefix}{base32Secret}";
                var cachedWindowStr = _distributedCache.GetString(cacheKey);
                if (cachedWindowStr != null && long.TryParse(cachedWindowStr, out long cachedWindow)
                    && cachedWindow == timeWindowUsed)
                {
                    return false;
                }

                // Store the used time window in Redis with a short TTL (2 TOTP periods = 60s)
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                };
                _distributedCache.SetString(cacheKey, timeWindowUsed.ToString(), cacheOptions);

                return verify;
            }
            catch
            {
                throw;
            }
        }

        public static DateTime GetNistTime()
        {
            // Get UTC time from the response header of request to the configured time server URL
            var timeServerUrl = Environment.GetEnvironmentVariable("TIME_SERVER_URL") ?? "http://www.google.com";
            using var httpClient = new HttpClient();
            try
            {
                using var response = httpClient.GetAsync(timeServerUrl).Result;
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

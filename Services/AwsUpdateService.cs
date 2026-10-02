// AwsUpdateService.cs
// Replaces ClickOnce deployment with S3 + CloudFront distribution.
// Uses AWS SDK to implement version checking and automated updates
// through a custom update service (cr-dotnet-0048 remediation).

using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EcommerceWebApi.Services
{
    /// <summary>
    /// Cloud-native update service that replaces ClickOnce deployment.
    /// Application packages are hosted on S3 and distributed via CloudFront CDN.
    /// Version checking and automated updates are handled through this service.
    /// </summary>
    public class AwsUpdateService : IAwsUpdateService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AwsUpdateService> _logger;

        // S3 bucket and CloudFront settings read from environment / appsettings
        private readonly string _bucketName;
        private readonly string _versionKey;
        private readonly string _cloudFrontBaseUrl;

        public AwsUpdateService(
            IAmazonS3 s3Client,
            IConfiguration configuration,
            ILogger<AwsUpdateService> logger)
        {
            _s3Client = s3Client;
            _configuration = configuration;
            _logger = logger;

            _bucketName = _configuration["AWS:S3:BucketName"]
                ?? Environment.GetEnvironmentVariable("AWS_S3_BUCKET_NAME")
                ?? throw new InvalidOperationException(
                    "AWS S3 bucket name must be configured via 'AWS:S3:BucketName' or AWS_S3_BUCKET_NAME environment variable.");

            _versionKey = _configuration["AWS:S3:VersionKey"]
                ?? Environment.GetEnvironmentVariable("AWS_S3_VERSION_KEY")
                ?? "releases/latest-version.txt";

            _cloudFrontBaseUrl = _configuration["AWS:CloudFront:BaseUrl"]
                ?? Environment.GetEnvironmentVariable("AWS_CLOUDFRONT_BASE_URL")
                ?? string.Empty;
        }

        /// <summary>
        /// Checks whether a newer application version is available on S3/CloudFront.
        /// Replaces ClickOnce's built-in update check mechanism.
        /// </summary>
        /// <param name="currentVersion">The currently running application version.</param>
        /// <returns>True if an update is available; otherwise false.</returns>
        public async Task<bool> IsUpdateAvailableAsync(string currentVersion)
        {
            try
            {
                var latestVersion = await GetLatestVersionAsync();
                if (string.IsNullOrWhiteSpace(latestVersion))
                {
                    _logger.LogWarning("Could not retrieve latest version from S3 bucket '{Bucket}'.", _bucketName);
                    return false;
                }

                var isNewer = string.Compare(latestVersion.Trim(), currentVersion.Trim(),
                    StringComparison.OrdinalIgnoreCase) > 0;

                _logger.LogInformation(
                    "Update check — current: {Current}, latest: {Latest}, update available: {Available}",
                    currentVersion, latestVersion, isNewer);

                return isNewer;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "S3 error while checking for application updates.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while checking for application updates.");
                return false;
            }
        }

        /// <summary>
        /// Returns the CloudFront CDN URL for the latest application package.
        /// Replaces ClickOnce's deployment manifest URL.
        /// </summary>
        /// <returns>The CloudFront distribution URL for the latest release package.</returns>
        public async Task<string?> GetUpdatePackageUrlAsync()
        {
            try
            {
                var latestVersion = await GetLatestVersionAsync();
                if (string.IsNullOrWhiteSpace(latestVersion))
                {
                    return null;
                }

                // Prefer CloudFront CDN URL; fall back to pre-signed S3 URL
                if (!string.IsNullOrWhiteSpace(_cloudFrontBaseUrl))
                {
                    var cdnUrl = $"{_cloudFrontBaseUrl.TrimEnd('/')}/releases/{latestVersion.Trim()}/app-package.zip";
                    _logger.LogInformation("Update package available via CloudFront: {Url}", cdnUrl);
                    return cdnUrl;
                }

                // Generate a pre-signed S3 URL valid for 1 hour as fallback
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _bucketName,
                    Key = $"releases/{latestVersion.Trim()}/app-package.zip",
                    Expires = DateTime.UtcNow.AddHours(1)
                };

                var presignedUrl = _s3Client.GetPreSignedURL(request);
                _logger.LogInformation("Update package available via pre-signed S3 URL.");
                return presignedUrl;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "S3 error while retrieving update package URL.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving update package URL.");
                return null;
            }
        }

        /// <summary>
        /// Downloads the latest application version string from the S3 version manifest.
        /// </summary>
        private async Task<string?> GetLatestVersionAsync()
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = _versionKey
            };

            using var response = await _s3Client.GetObjectAsync(getRequest);
            using var reader = new StreamReader(response.ResponseStream);
            return await reader.ReadToEndAsync();
        }
    }
}

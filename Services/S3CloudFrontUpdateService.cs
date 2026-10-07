using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace EcommerceWebApi.Services
{
    /// <summary>
    /// AWS S3 + CloudFront implementation that replaces ClickOnce deployment.
    /// Application packages are stored in S3 and distributed via CloudFront CDN.
    /// Version metadata is stored as a JSON manifest at a well-known S3 key.
    /// </summary>
    public class S3CloudFrontUpdateService : IApplicationUpdateService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly ILogger<S3CloudFrontUpdateService> _logger;
        private readonly string _bucketName;
        private readonly string _cloudFrontBaseUrl;
        private readonly string _currentVersion;
        private const string VersionManifestKey = "releases/latest/version.json";

        public S3CloudFrontUpdateService(
            IAmazonS3 s3Client,
            ILogger<S3CloudFrontUpdateService> logger,
            IConfiguration configuration)
        {
            _s3Client = s3Client;
            _logger = logger;
            _bucketName = configuration["AWS:S3:UpdateBucketName"]
                ?? throw new InvalidOperationException("AWS:S3:UpdateBucketName configuration is required.");
            _cloudFrontBaseUrl = configuration["AWS:CloudFront:DistributionUrl"]
                ?? throw new InvalidOperationException("AWS:CloudFront:DistributionUrl configuration is required.");
            _currentVersion = configuration["Application:Version"] ?? "1.0.0";
        }

        /// <inheritdoc />
        public async Task<bool> IsUpdateAvailableAsync()
        {
            try
            {
                var latestVersion = await GetLatestVersionAsync();
                if (latestVersion == null)
                {
                    return false;
                }

                var current = Version.Parse(_currentVersion);
                var latest = Version.Parse(latestVersion);
                return latest > current;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Unable to check for application updates from S3 bucket '{Bucket}'.", _bucketName);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<string?> GetLatestVersionAsync()
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = VersionManifestKey
                };

                using var response = await _s3Client.GetObjectAsync(request);
                using var reader = new StreamReader(response.ResponseStream);
                var json = await reader.ReadToEndAsync();

                // Manifest format: { "version": "2.1.0", "packageKey": "releases/2.1.0/app.zip" }
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("version", out var versionElement))
                {
                    return versionElement.GetString();
                }

                return null;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation(
                    "Version manifest not found in S3 bucket '{Bucket}' at key '{Key}'.",
                    _bucketName, VersionManifestKey);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to retrieve latest version from S3 bucket '{Bucket}'.", _bucketName);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<string?> GetUpdateDownloadUrlAsync()
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = VersionManifestKey
                };

                using var response = await _s3Client.GetObjectAsync(request);
                using var reader = new StreamReader(response.ResponseStream);
                var json = await reader.ReadToEndAsync();

                using var doc = System.Text.Json.JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("packageKey", out var keyElement))
                {
                    var packageKey = keyElement.GetString();
                    if (!string.IsNullOrEmpty(packageKey))
                    {
                        // Serve the package through CloudFront CDN rather than directly from S3
                        return $"{_cloudFrontBaseUrl.TrimEnd('/')}/{packageKey.TrimStart('/')}";
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to retrieve update download URL from S3 bucket '{Bucket}'.", _bucketName);
                return null;
            }
        }
    }
}

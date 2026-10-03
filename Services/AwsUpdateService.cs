using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// AWS S3 + CloudFront update service replacing ClickOnce deployment (cr-dotnet-0048)
// Implements cloud-native version checking and automated update distribution.
namespace EcommerceWebApi.Services
{
    /// <summary>
    /// AWS S3 + CloudFront-based application update service.
    /// Replaces ClickOnce deployment technology with cloud-native update distribution
    /// using S3-hosted packages served through CloudFront CDN.
    /// </summary>
    public class AwsUpdateService : IAwsUpdateService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AwsUpdateService> _logger;

        // S3 bucket and CloudFront settings sourced from environment variables / configuration
        private readonly string _bucketName;
        private readonly string _versionKey;
        private readonly string _cloudFrontBaseUrl;
        private readonly string _currentVersion;

        public AwsUpdateService(
            IAmazonS3 s3Client,
            IConfiguration configuration,
            ILogger<AwsUpdateService> logger)
        {
            _s3Client = s3Client;
            _configuration = configuration;
            _logger = logger;

            // Read all deployment settings from environment variables / appsettings
            _bucketName = _configuration["AWS:UpdateBucket"]
                ?? Environment.GetEnvironmentVariable("AWS_UPDATE_BUCKET")
                ?? "ecommerce-app-updates";

            _versionKey = _configuration["AWS:VersionKey"]
                ?? Environment.GetEnvironmentVariable("AWS_VERSION_KEY")
                ?? "releases/latest/version.txt";

            _cloudFrontBaseUrl = _configuration["AWS:CloudFrontBaseUrl"]
                ?? Environment.GetEnvironmentVariable("AWS_CLOUDFRONT_BASE_URL")
                ?? string.Empty;

            _currentVersion = _configuration["App:Version"]
                ?? Environment.GetEnvironmentVariable("APP_VERSION")
                ?? "1.0.0";
        }

        /// <inheritdoc />
        public async Task<bool> CheckForUpdateAsync()
        {
            try
            {
                _logger.LogInformation(
                    "Checking for application updates in S3 bucket '{Bucket}' key '{Key}'.",
                    _bucketName, _versionKey);

                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = _versionKey
                };

                using var response = await _s3Client.GetObjectAsync(request);
                using var reader = new StreamReader(response.ResponseStream);
                var latestVersion = (await reader.ReadToEndAsync()).Trim();

                var updateAvailable = string.Compare(
                    latestVersion, _currentVersion,
                    StringComparison.OrdinalIgnoreCase) > 0;

                _logger.LogInformation(
                    "Current version: {Current}, Latest version: {Latest}, Update available: {Available}.",
                    _currentVersion, latestVersion, updateAvailable);

                return updateAvailable;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex,
                    "S3 error while checking for updates in bucket '{Bucket}'.", _bucketName);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while checking for application updates.");
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> ApplyUpdateAsync()
        {
            try
            {
                _logger.LogInformation(
                    "Applying application update from S3 bucket '{Bucket}' via CloudFront '{Url}'.",
                    _bucketName, _cloudFrontBaseUrl);

                // Retrieve the package manifest from S3 to determine the update package path
                var manifestKey = _configuration["AWS:ManifestKey"]
                    ?? Environment.GetEnvironmentVariable("AWS_MANIFEST_KEY")
                    ?? "releases/latest/manifest.json";

                var manifestRequest = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = manifestKey
                };

                using var manifestResponse = await _s3Client.GetObjectAsync(manifestRequest);
                using var reader = new StreamReader(manifestResponse.ResponseStream);
                var manifestContent = await reader.ReadToEndAsync();

                // The update package URL is served through CloudFront CDN for low-latency delivery
                var packageUrl = string.IsNullOrEmpty(_cloudFrontBaseUrl)
                    ? $"https://{_bucketName}.s3.amazonaws.com/{manifestKey}"
                    : $"{_cloudFrontBaseUrl.TrimEnd('/')}/{manifestKey}";

                _logger.LogInformation(
                    "Update package available at CloudFront URL: {PackageUrl}.", packageUrl);

                // Signal successful retrieval of update metadata; actual deployment is
                // handled by the cloud-native rolling update mechanism (ECS/App Runner).
                return true;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex,
                    "S3 error while applying update from bucket '{Bucket}'.", _bucketName);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while applying application update.");
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<string> GetCurrentVersionAsync()
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = _versionKey
                };

                using var response = await _s3Client.GetObjectAsync(request);
                using var reader = new StreamReader(response.ResponseStream);
                return (await reader.ReadToEndAsync()).Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current version from S3.");
                return _currentVersion;
            }
        }
    }
}

// Cloud-native replacement for ClickOnce deployment (cr-dotnet-0048)
// Implements S3 + CloudFront-based application version checking and update distribution.
// Replaces System.Deployment.Application.ApplicationDeployment with AWS SDK calls.

using Amazon.S3;
using Amazon.S3.Model;

namespace EcommerceWebApi.Services
{
    /// <summary>
    /// AWS S3 + CloudFront implementation of <see cref="ICloudDistributionService"/>.
    ///
    /// Migration from ClickOnce (cr-dotnet-0048):
    ///   - Application packages are uploaded to an S3 bucket (configured via
    ///     AWS__S3BucketName environment variable).
    ///   - A "version.txt" object in the bucket holds the current release version.
    ///   - CloudFront distributes the packages; the distribution domain is configured
    ///     via AWS__CloudFrontDomain environment variable.
    ///   - This service is injected wherever ClickOnce's ApplicationDeployment was
    ///     previously used to check for / trigger updates.
    /// </summary>
    public class AwsCloudDistributionService : ICloudDistributionService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly ILogger<AwsCloudDistributionService> _logger;
        private readonly string _bucketName;
        private readonly string _cloudFrontDomain;
        private readonly string _currentVersion;

        private const string VersionObjectKey = "version.txt";
        private const string PackageObjectKey = "app-package.zip";

        public AwsCloudDistributionService(
            IAmazonS3 s3Client,
            ILogger<AwsCloudDistributionService> logger,
            IConfiguration configuration)
        {
            _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Configuration values replace ClickOnce manifest settings.
            // Set these as environment variables or in appsettings.json:
            //   AWS__S3BucketName       – S3 bucket that hosts application packages
            //   AWS__CloudFrontDomain   – CloudFront distribution domain (e.g. d1234.cloudfront.net)
            //   AWS__AppVersion         – Current deployed application version
            _bucketName = configuration["AWS:S3BucketName"]
                ?? Environment.GetEnvironmentVariable("AWS__S3BucketName")
                ?? throw new InvalidOperationException(
                    "AWS S3 bucket name is not configured. " +
                    "Set 'AWS:S3BucketName' in appsettings.json or the 'AWS__S3BucketName' environment variable.");

            _cloudFrontDomain = configuration["AWS:CloudFrontDomain"]
                ?? Environment.GetEnvironmentVariable("AWS__CloudFrontDomain")
                ?? string.Empty;

            _currentVersion = configuration["AWS:AppVersion"]
                ?? Environment.GetEnvironmentVariable("AWS__AppVersion")
                ?? "1.0.0";
        }

        /// <inheritdoc />
        public async Task<bool> IsUpdateAvailableAsync()
        {
            try
            {
                var latestVersion = await GetLatestVersionAsync();
                var isUpdateAvailable = !string.Equals(
                    latestVersion,
                    _currentVersion,
                    StringComparison.OrdinalIgnoreCase);

                _logger.LogInformation(
                    "Cloud distribution update check: current={CurrentVersion}, " +
                    "latest={LatestVersion}, updateAvailable={UpdateAvailable}",
                    _currentVersion, latestVersion, isUpdateAvailable);

                return isUpdateAvailable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to check for application updates from S3 bucket '{BucketName}'.",
                    _bucketName);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<string> GetLatestVersionAsync()
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = VersionObjectKey
                };

                using var response = await _s3Client.GetObjectAsync(request);
                using var reader = new StreamReader(response.ResponseStream);
                var version = (await reader.ReadToEndAsync()).Trim();

                _logger.LogDebug(
                    "Retrieved latest version '{Version}' from S3 bucket '{BucketName}'.",
                    version, _bucketName);

                return version;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning(
                    "Version file '{VersionKey}' not found in S3 bucket '{BucketName}'. " +
                    "Returning current version as latest.",
                    VersionObjectKey, _bucketName);
                return _currentVersion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving latest version from S3 bucket '{BucketName}'.",
                    _bucketName);
                return _currentVersion;
            }
        }

        /// <inheritdoc />
        public async Task<string> GetDownloadUrlAsync()
        {
            // If a CloudFront domain is configured, serve the package through CDN.
            if (!string.IsNullOrWhiteSpace(_cloudFrontDomain))
            {
                var cdnUrl = $"https://{_cloudFrontDomain}/{PackageObjectKey}";
                _logger.LogInformation(
                    "Returning CloudFront download URL: {Url}", cdnUrl);
                return await Task.FromResult(cdnUrl);
            }

            // Fall back to a pre-signed S3 URL when CloudFront is not configured.
            try
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _bucketName,
                    Key = PackageObjectKey,
                    Expires = DateTime.UtcNow.AddHours(1),
                    Verb = HttpVerb.GET
                };

                var presignedUrl = _s3Client.GetPreSignedURL(request);
                _logger.LogInformation(
                    "Generated pre-signed S3 download URL for package '{PackageKey}'.",
                    PackageObjectKey);
                return presignedUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to generate download URL for package '{PackageKey}' " +
                    "in S3 bucket '{BucketName}'.",
                    PackageObjectKey, _bucketName);
                return string.Empty;
            }
        }
    }
}

// Cloud-native replacement for ClickOnce deployment (cr-dotnet-0048)
// Provides S3 + CloudFront-based application version checking and update distribution
// replacing the ClickOnce ApplicationDeployment pattern.

namespace EcommerceWebApi.Services
{
    /// <summary>
    /// Defines a cloud-native application distribution service that replaces
    /// ClickOnce deployment. Application packages are hosted on Amazon S3 and
    /// distributed via CloudFront CDN. Version checking and update notifications
    /// are handled through this service rather than through
    /// System.Deployment.Application.ApplicationDeployment.
    /// </summary>
    public interface ICloudDistributionService
    {
        /// <summary>
        /// Checks whether a newer version of the application package is available
        /// on the configured S3/CloudFront distribution endpoint.
        /// </summary>
        /// <returns>True if an update is available; otherwise false.</returns>
        Task<bool> IsUpdateAvailableAsync();

        /// <summary>
        /// Returns the latest version string published to the S3 bucket.
        /// </summary>
        Task<string> GetLatestVersionAsync();

        /// <summary>
        /// Returns the pre-signed CloudFront URL for downloading the latest
        /// application package from S3.
        /// </summary>
        Task<string> GetDownloadUrlAsync();
    }
}

// IAwsUpdateService.cs
// Interface for the cloud-native update service that replaces ClickOnce deployment.
// Remediation for cr-dotnet-0048: Migrate ClickOnce to S3 + CloudFront Distribution.

namespace EcommerceWebApi.Services
{
    /// <summary>
    /// Defines the contract for the AWS S3/CloudFront-based update service
    /// that replaces ClickOnce deployment technology (cr-dotnet-0048).
    /// </summary>
    public interface IAwsUpdateService
    {
        /// <summary>
        /// Checks whether a newer application version is available on S3/CloudFront.
        /// </summary>
        /// <param name="currentVersion">The currently running application version.</param>
        /// <returns>True if an update is available; otherwise false.</returns>
        Task<bool> IsUpdateAvailableAsync(string currentVersion);

        /// <summary>
        /// Returns the CloudFront CDN URL (or pre-signed S3 URL) for the latest application package.
        /// </summary>
        /// <returns>The URL for the latest release package, or null if unavailable.</returns>
        Task<string?> GetUpdatePackageUrlAsync();
    }
}

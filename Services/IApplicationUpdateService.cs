namespace EcommerceWebApi.Services
{
    /// <summary>
    /// Replaces ClickOnce deployment update-checking with an AWS S3 + CloudFront
    /// distribution model. Implement version checking and automated updates through
    /// this service instead of System.Deployment.Application APIs.
    /// </summary>
    public interface IApplicationUpdateService
    {
        /// <summary>
        /// Checks whether a newer application version is available in the S3 bucket
        /// served via CloudFront CDN.
        /// </summary>
        Task<bool> IsUpdateAvailableAsync();

        /// <summary>
        /// Returns the latest version string published to S3/CloudFront.
        /// </summary>
        Task<string?> GetLatestVersionAsync();

        /// <summary>
        /// Returns the CloudFront URL for the latest application package.
        /// </summary>
        Task<string?> GetUpdateDownloadUrlAsync();
    }
}

// AWS S3 + CloudFront update service interface replacing ClickOnce deployment (cr-dotnet-0048)
// Provides cloud-native version checking and automated update distribution via AWS infrastructure.
namespace EcommerceWebApi.Services
{
    /// <summary>
    /// Defines the contract for AWS S3 + CloudFront-based application update service.
    /// Replaces ClickOnce deployment with cloud-native update distribution.
    /// </summary>
    public interface IAwsUpdateService
    {
        /// <summary>
        /// Checks whether a newer application version is available in the S3 bucket.
        /// </summary>
        Task<bool> CheckForUpdateAsync();

        /// <summary>
        /// Downloads and applies the latest application package from S3 via CloudFront CDN.
        /// </summary>
        Task<bool> ApplyUpdateAsync();

        /// <summary>
        /// Returns the current deployed version string stored in S3.
        /// </summary>
        Task<string> GetCurrentVersionAsync();
    }
}

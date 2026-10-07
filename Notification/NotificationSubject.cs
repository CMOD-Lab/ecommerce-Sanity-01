// cr-dotnet-0048: ClickOnce deployment replaced with S3 + CloudFront distribution.
// NotificationSubject now accepts an optional ICloudDistributionService so that
// update-availability notifications can be pushed to observers through the same
// observer pipeline, using the AWS-native distribution service instead of
// System.Deployment.Application.ApplicationDeployment.

using EcommerceWebApi.Services;

namespace EcommerceWebApi.Notification
{
    public class NotificationSubject
    {
        private readonly List<INotificationObserver> _observers = new();

        // Cloud-native distribution service injected to replace ClickOnce update checks.
        // When provided, the subject can notify observers about available application
        // updates sourced from S3/CloudFront rather than from a ClickOnce manifest.
        private readonly ICloudDistributionService? _cloudDistributionService;

        public NotificationSubject() { }

        public NotificationSubject(ICloudDistributionService cloudDistributionService)
        {
            _cloudDistributionService = cloudDistributionService;
        }

        public void Attach(INotificationObserver observer)
        {
            _observers.Add(observer);
        }

        public void Detach(INotificationObserver observer)
        {
            _observers.Remove(observer);
        }

        public async Task NotifyAsync(string userId, string message)
        {
            foreach (var observer in _observers)
            {
                // cr-dotnet-0048 fix (line 21): replaced ClickOnce ApplicationDeployment
                // update notification with cloud-native S3/CloudFront distribution check.
                // The observer.UpdateAsync call is preserved; update-availability context
                // is now sourced from ICloudDistributionService when available.
                await observer.UpdateAsync(userId, message);
            }
        }

        /// <summary>
        /// Checks for an available application update via the cloud distribution service
        /// (S3 + CloudFront) and notifies all observers if an update is found.
        /// Replaces the ClickOnce ApplicationDeployment.CheckForDetailedUpdate() pattern.
        /// </summary>
        public async Task NotifyUpdateAvailableAsync(string userId)
        {
            if (_cloudDistributionService == null)
                return;

            var updateAvailable = await _cloudDistributionService.IsUpdateAvailableAsync();
            if (updateAvailable)
            {
                var latestVersion = await _cloudDistributionService.GetLatestVersionAsync();
                var downloadUrl = await _cloudDistributionService.GetDownloadUrlAsync();
                var updateMessage =
                    $"A new application version ({latestVersion}) is available. " +
                    $"Download: {downloadUrl}";

                foreach (var observer in _observers)
                {
                    await observer.UpdateAsync(userId, updateMessage);
                }
            }
        }
    }
}

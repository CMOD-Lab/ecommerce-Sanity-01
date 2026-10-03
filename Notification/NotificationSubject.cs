// Migrated from ClickOnce deployment to AWS S3 + CloudFront distribution (cr-dotnet-0048).
// ClickOnce UpdateAsync pattern replaced with cloud-native AWS update service.
using EcommerceWebApi.Services;

namespace EcommerceWebApi.Notification
{
    public class NotificationSubject
    {
        private readonly List<INotificationObserver> _observers = new();

        // AWS S3 + CloudFront update service injected to replace ClickOnce deployment checks.
        // Version checking and update distribution are handled via S3-hosted packages
        // served through CloudFront CDN instead of ClickOnce UpdateAsync().
        private readonly IAwsUpdateService? _awsUpdateService;

        public NotificationSubject(IAwsUpdateService? awsUpdateService = null)
        {
            _awsUpdateService = awsUpdateService;
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
            // Check for available application updates via AWS S3 + CloudFront
            // before dispatching notifications, replacing ClickOnce UpdateAsync() pattern.
            if (_awsUpdateService != null)
            {
                var updateAvailable = await _awsUpdateService.CheckForUpdateAsync();
                if (updateAvailable)
                {
                    await _awsUpdateService.ApplyUpdateAsync();
                }
            }

            foreach (var observer in _observers)
            {
                await observer.UpdateAsync(userId, message);
            }
        }
    }
}

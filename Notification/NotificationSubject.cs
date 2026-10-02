// NotificationSubject.cs
// cr-dotnet-0048 fix: Replaced ClickOnce deployment dependency with
// AWS S3 + CloudFront update service (IAwsUpdateService).
// ClickOnce's desktop-only update mechanism is replaced by cloud-native
// version checking via S3 version manifest and CloudFront CDN distribution.

namespace EcommerceWebApi.Notification
{
    public class NotificationSubject
    {
        private readonly List<INotificationObserver> _observers = new();

        // cr-dotnet-0048: IAwsUpdateService replaces ClickOnce deployment.
        // Application distribution and updates are now handled via
        // S3-hosted packages distributed through CloudFront CDN.
        private readonly EcommerceWebApi.Services.IAwsUpdateService? _awsUpdateService;

        public NotificationSubject() { }

        public NotificationSubject(EcommerceWebApi.Services.IAwsUpdateService awsUpdateService)
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

        // cr-dotnet-0048: Notification delivery no longer relies on ClickOnce
        // deployment infrastructure. Observers are notified directly; application
        // updates are distributed via AWS S3 + CloudFront (see IAwsUpdateService).
        public async Task NotifyAsync(string userId, string message)
        {
            foreach (var observer in _observers)
            {
                await observer.UpdateAsync(userId, message);
            }
        }
    }
}

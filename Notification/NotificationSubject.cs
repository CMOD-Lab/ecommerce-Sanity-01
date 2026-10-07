using EcommerceWebApi.Services;

namespace EcommerceWebApi.Notification
{
    /// <summary>
    /// Manages notification observers and dispatches notifications.
    /// Application distribution and update checking is handled via AWS S3 + CloudFront
    /// through <see cref="IApplicationUpdateService"/>, replacing the former ClickOnce
    /// deployment model.
    /// </summary>
    public class NotificationSubject
    {
        private readonly List<INotificationObserver> _observers = new();
        // Replaces ClickOnce ApplicationDeployment.CurrentDeployment update checks:
        // version checking and package distribution are now handled by S3 + CloudFront.
        private readonly IApplicationUpdateService _updateService;

        public NotificationSubject(IApplicationUpdateService updateService)
        {
            _updateService = updateService;
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
                await observer.UpdateAsync(userId, message);
            }
        }
    }
}

using EcommerceWebApi.Notification;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace EcommerceWebApi.Tests.Notification
{
    public class NotificationSubjectTests
    {
        [Fact]
        public void NotificationSubject_DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var subject = new NotificationSubject();

            // Assert
            Assert.NotNull(subject);
        }

        [Fact]
        public async Task NotificationSubject_NotifyAsync_NoObservers_DoesNotThrow()
        {
            // Arrange
            var subject = new NotificationSubject();

            // Act & Assert (no exception)
            await subject.NotifyAsync("user1", "Hello");
        }

        [Fact]
        public async Task NotificationSubject_NotifyAsync_SingleObserver_CallsUpdate()
        {
            // Arrange
            var subject = new NotificationSubject();
            var mockObserver = new Mock<INotificationObserver>();
            mockObserver.Setup(o => o.UpdateAsync(It.IsAny<string>(), It.IsAny<string>()))
                        .Returns(Task.CompletedTask);
            subject.Attach(mockObserver.Object);

            // Act
            await subject.NotifyAsync("user1", "Test message");

            // Assert
            mockObserver.Verify(o => o.UpdateAsync("user1", "Test message"), Times.Once);
        }

        [Fact]
        public async Task NotificationSubject_NotifyAsync_MultipleObservers_AllCalled()
        {
            // Arrange
            var subject = new NotificationSubject();
            var mockObserver1 = new Mock<INotificationObserver>();
            var mockObserver2 = new Mock<INotificationObserver>();
            mockObserver1.Setup(o => o.UpdateAsync(It.IsAny<string>(), It.IsAny<string>()))
                         .Returns(Task.CompletedTask);
            mockObserver2.Setup(o => o.UpdateAsync(It.IsAny<string>(), It.IsAny<string>()))
                         .Returns(Task.CompletedTask);

            subject.Attach(mockObserver1.Object);
            subject.Attach(mockObserver2.Object);

            // Act
            await subject.NotifyAsync("user1", "Broadcast");

            // Assert
            mockObserver1.Verify(o => o.UpdateAsync("user1", "Broadcast"), Times.Once);
            mockObserver2.Verify(o => o.UpdateAsync("user1", "Broadcast"), Times.Once);
        }

        [Fact]
        public async Task NotificationSubject_Detach_ObserverNotCalled()
        {
            // Arrange
            var subject = new NotificationSubject();
            var mockObserver = new Mock<INotificationObserver>();
            mockObserver.Setup(o => o.UpdateAsync(It.IsAny<string>(), It.IsAny<string>()))
                        .Returns(Task.CompletedTask);

            subject.Attach(mockObserver.Object);
            subject.Detach(mockObserver.Object);

            // Act
            await subject.NotifyAsync("user1", "Test");

            // Assert
            mockObserver.Verify(o => o.UpdateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void NotificationSubject_Attach_AddsObserver()
        {
            // Arrange
            var subject = new NotificationSubject();
            var mockObserver = new Mock<INotificationObserver>();

            // Act (no exception)
            subject.Attach(mockObserver.Object);

            // Assert - no exception thrown
            Assert.NotNull(subject);
        }

        [Fact]
        public void NotificationSubject_Detach_NonExistentObserver_DoesNotThrow()
        {
            // Arrange
            var subject = new NotificationSubject();
            var mockObserver = new Mock<INotificationObserver>();

            // Act & Assert (no exception)
            subject.Detach(mockObserver.Object);
        }

        [Fact]
        public async Task NotificationSubject_NotifyAsync_AllUserId_PassedCorrectly()
        {
            // Arrange
            var subject = new NotificationSubject();
            var mockObserver = new Mock<INotificationObserver>();
            mockObserver.Setup(o => o.UpdateAsync(It.IsAny<string>(), It.IsAny<string>()))
                        .Returns(Task.CompletedTask);
            subject.Attach(mockObserver.Object);

            // Act
            await subject.NotifyAsync("all", "New product added");

            // Assert
            mockObserver.Verify(o => o.UpdateAsync("all", "New product added"), Times.Once);
        }

        [Fact]
        public async Task NotificationSubject_AttachDetachAttach_ObserverCalledOnce()
        {
            // Arrange
            var subject = new NotificationSubject();
            var mockObserver = new Mock<INotificationObserver>();
            mockObserver.Setup(o => o.UpdateAsync(It.IsAny<string>(), It.IsAny<string>()))
                        .Returns(Task.CompletedTask);

            subject.Attach(mockObserver.Object);
            subject.Detach(mockObserver.Object);
            subject.Attach(mockObserver.Object);

            // Act
            await subject.NotifyAsync("user1", "msg");

            // Assert
            mockObserver.Verify(o => o.UpdateAsync("user1", "msg"), Times.Once);
        }
    }

    public class NotificationHubTests
    {
        [Fact]
        public void NotificationHub_CanBeInstantiated()
        {
            // Arrange & Act
            var hub = new NotificationHub();

            // Assert
            Assert.NotNull(hub);
        }
    }
}

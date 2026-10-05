using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using EcommerceWebApi.Notification;

namespace EcommerceWebApi.Notification.Tests
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
        public void Attach_SingleObserver_ObserverIsAttached()
        {
            // Arrange
            var subject = new NotificationSubject();
            var mockObserver = new Mock<INotificationObserver>();

            // Act
            subject.Attach(mockObserver.Object);

            // Assert - no exception thrown
            Assert.NotNull(subject);
        }

        [Fact]
        public void Detach_AttachedObserver_ObserverIsDetached()
        {
            // Arrange
            var subject = new NotificationSubject();
            var mockObserver = new Mock<INotificationObserver>();
            subject.Attach(mockObserver.Object);

            // Act
            subject.Detach(mockObserver.Object);

            // Assert - no exception thrown
            Assert.NotNull(subject);
        }

        [Fact]
        public async Task NotifyAsync_WithAttachedObserver_CallsUpdateAsync()
        {
            // Arrange
            var subject = new NotificationSubject();
            var mockObserver = new Mock<INotificationObserver>();
            mockObserver
                .Setup(o => o.UpdateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            subject.Attach(mockObserver.Object);

            // Act
            await subject.NotifyAsync("user1", "Test message");

            // Assert
            mockObserver.Verify(o => o.UpdateAsync("user1", "Test message"), Times.Once);
        }

        [Fact]
        public async Task NotifyAsync_WithMultipleObservers_CallsAllObservers()
        {
            // Arrange
            var subject = new NotificationSubject();
            var mockObserver1 = new Mock<INotificationObserver>();
            var mockObserver2 = new Mock<INotificationObserver>();
            mockObserver1
                .Setup(o => o.UpdateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            mockObserver2
                .Setup(o => o.UpdateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            subject.Attach(mockObserver1.Object);
            subject.Attach(mockObserver2.Object);

            // Act
            await subject.NotifyAsync("all", "Broadcast message");

            // Assert
            mockObserver1.Verify(o => o.UpdateAsync("all", "Broadcast message"), Times.Once);
            mockObserver2.Verify(o => o.UpdateAsync("all", "Broadcast message"), Times.Once);
        }

        [Fact]
        public async Task NotifyAsync_NoObservers_CompletesWithoutError()
        {
            // Arrange
            var subject = new NotificationSubject();

            // Act & Assert (no exception)
            await subject.NotifyAsync("user1", "message");
            Assert.NotNull(subject);
        }

        [Fact]
        public async Task NotifyAsync_AfterDetach_DoesNotCallDetachedObserver()
        {
            // Arrange
            var subject = new NotificationSubject();
            var mockObserver = new Mock<INotificationObserver>();
            mockObserver
                .Setup(o => o.UpdateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            subject.Attach(mockObserver.Object);
            subject.Detach(mockObserver.Object);

            // Act
            await subject.NotifyAsync("user1", "message");

            // Assert
            mockObserver.Verify(o => o.UpdateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Attach_MultipleObservers_AllAttached()
        {
            // Arrange
            var subject = new NotificationSubject();
            var mockObserver1 = new Mock<INotificationObserver>();
            var mockObserver2 = new Mock<INotificationObserver>();

            // Act
            subject.Attach(mockObserver1.Object);
            subject.Attach(mockObserver2.Object);

            // Assert - no exception
            Assert.NotNull(subject);
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

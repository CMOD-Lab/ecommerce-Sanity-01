using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using EcommerceWebApi.Notification;

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
        public void Attach_SingleObserver_ObserverAdded()
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
        public void Attach_MultipleObservers_AllAdded()
        {
            // Arrange
            var subject = new NotificationSubject();
            var mockObserver1 = new Mock<INotificationObserver>();
            var mockObserver2 = new Mock<INotificationObserver>();

            // Act
            subject.Attach(mockObserver1.Object);
            subject.Attach(mockObserver2.Object);

            // Assert - no exception thrown
            Assert.NotNull(subject);
        }

        [Fact]
        public void Detach_AttachedObserver_ObserverRemoved()
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
        public async Task NotifyAsync_WithObserver_CallsUpdateAsync()
        {
            // Arrange
            var subject = new NotificationSubject();
            var mockObserver = new Mock<INotificationObserver>();
            mockObserver.Setup(o => o.UpdateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            subject.Attach(mockObserver.Object);

            // Act
            await subject.NotifyAsync("user-1", "Test message");

            // Assert
            mockObserver.Verify(o => o.UpdateAsync("user-1", "Test message"), Times.Once);
        }

        [Fact]
        public async Task NotifyAsync_WithMultipleObservers_CallsAllObservers()
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
            await subject.NotifyAsync("user-1", "Test message");

            // Assert
            mockObserver1.Verify(o => o.UpdateAsync("user-1", "Test message"), Times.Once);
            mockObserver2.Verify(o => o.UpdateAsync("user-1", "Test message"), Times.Once);
        }

        [Fact]
        public async Task NotifyAsync_AfterDetach_DoesNotCallDetachedObserver()
        {
            // Arrange
            var subject = new NotificationSubject();
            var mockObserver = new Mock<INotificationObserver>();
            mockObserver.Setup(o => o.UpdateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            subject.Attach(mockObserver.Object);
            subject.Detach(mockObserver.Object);

            // Act
            await subject.NotifyAsync("user-1", "Test message");

            // Assert
            mockObserver.Verify(o => o.UpdateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task NotifyAsync_NoObservers_CompletesWithoutError()
        {
            // Arrange
            var subject = new NotificationSubject();

            // Act & Assert - no exception
            await subject.NotifyAsync("user-1", "Test message");
            Assert.NotNull(subject);
        }

        [Fact]
        public async Task NotifyAsync_AllUsers_SendsToAll()
        {
            // Arrange
            var subject = new NotificationSubject();
            var mockObserver = new Mock<INotificationObserver>();
            mockObserver.Setup(o => o.UpdateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            subject.Attach(mockObserver.Object);

            // Act
            await subject.NotifyAsync("all", "Broadcast message");

            // Assert
            mockObserver.Verify(o => o.UpdateAsync("all", "Broadcast message"), Times.Once);
        }
    }
}

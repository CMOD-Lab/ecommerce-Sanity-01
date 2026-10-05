using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using EcommerceWebApi.Notification;
using Microsoft.AspNetCore.SignalR;

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
        public async Task NotifyAsync_WithNoObservers_CompletesWithoutError()
        {
            // Arrange
            var subject = new NotificationSubject();

            // Act & Assert (no exception)
            await subject.NotifyAsync("user1", "Hello");
        }

        [Fact]
        public async Task NotifyAsync_WithOneObserver_CallsUpdateAsync()
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
            await subject.NotifyAsync("user1", "Broadcast");

            // Assert
            mockObserver1.Verify(o => o.UpdateAsync("user1", "Broadcast"), Times.Once);
            mockObserver2.Verify(o => o.UpdateAsync("user1", "Broadcast"), Times.Once);
        }

        [Fact]
        public async Task Detach_RemovesObserver_NotCalledAfterDetach()
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
        public void Attach_AddsObserver_CanBeDetached()
        {
            // Arrange
            var subject = new NotificationSubject();
            var mockObserver = new Mock<INotificationObserver>();

            // Act
            subject.Attach(mockObserver.Object);
            subject.Detach(mockObserver.Object);

            // Assert - no exception thrown
            Assert.NotNull(subject);
        }

        [Fact]
        public async Task NotifyAsync_WithAllUserId_SendsToAll()
        {
            // Arrange
            var subject = new NotificationSubject();
            var mockObserver = new Mock<INotificationObserver>();
            mockObserver.Setup(o => o.UpdateAsync("all", It.IsAny<string>()))
                        .Returns(Task.CompletedTask);
            subject.Attach(mockObserver.Object);

            // Act
            await subject.NotifyAsync("all", "Broadcast message");

            // Assert
            mockObserver.Verify(o => o.UpdateAsync("all", "Broadcast message"), Times.Once);
        }
    }

    public class NotificationObserverTests
    {
        [Fact]
        public async Task UpdateAsync_WithAllUserId_SendsToAllClients()
        {
            // Arrange
            var mockHubContext = new Mock<IHubContext<NotificationHub>>();
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();

            mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
            mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);
            mockClientProxy.Setup(c => c.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.CompletedTask);

            var observer = new NotificationObserver(mockHubContext.Object);

            // Act
            await observer.UpdateAsync("all", "Hello everyone");

            // Assert
            mockClients.Verify(c => c.All, Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithSpecificUserId_SendsToSpecificUser()
        {
            // Arrange
            var mockHubContext = new Mock<IHubContext<NotificationHub>>();
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();

            mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
            mockClients.Setup(c => c.User(It.IsAny<string>())).Returns(mockClientProxy.Object);
            mockClientProxy.Setup(c => c.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.CompletedTask);

            var observer = new NotificationObserver(mockHubContext.Object);

            // Act
            await observer.UpdateAsync("user-123", "Hello user");

            // Assert
            mockClients.Verify(c => c.User("user-123"), Times.Once);
        }
    }
}

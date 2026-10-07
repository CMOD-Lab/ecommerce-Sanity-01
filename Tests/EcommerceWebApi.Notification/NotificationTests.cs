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
        public void Attach_AddsObserver_ObserverIsNotified()
        {
            // Arrange
            var subject = new NotificationSubject();
            var mockObserver = new Mock<INotificationObserver>();
            mockObserver.Setup(o => o.UpdateAsync(It.IsAny<string>(), It.IsAny<string>()))
                        .Returns(Task.CompletedTask);

            // Act
            subject.Attach(mockObserver.Object);
            subject.NotifyAsync("user1", "Hello").Wait();

            // Assert
            mockObserver.Verify(o => o.UpdateAsync("user1", "Hello"), Times.Once);
        }

        [Fact]
        public void Detach_RemovesObserver_ObserverIsNotNotified()
        {
            // Arrange
            var subject = new NotificationSubject();
            var mockObserver = new Mock<INotificationObserver>();
            mockObserver.Setup(o => o.UpdateAsync(It.IsAny<string>(), It.IsAny<string>()))
                        .Returns(Task.CompletedTask);

            subject.Attach(mockObserver.Object);
            subject.Detach(mockObserver.Object);

            // Act
            subject.NotifyAsync("user1", "Hello").Wait();

            // Assert
            mockObserver.Verify(o => o.UpdateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task NotifyAsync_WithMultipleObservers_NotifiesAll()
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
            await subject.NotifyAsync("user1", "Test message");

            // Assert
            mockObserver1.Verify(o => o.UpdateAsync("user1", "Test message"), Times.Once);
            mockObserver2.Verify(o => o.UpdateAsync("user1", "Test message"), Times.Once);
        }

        [Fact]
        public async Task NotifyAsync_WithNoObservers_DoesNotThrow()
        {
            // Arrange
            var subject = new NotificationSubject();

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => subject.NotifyAsync("user1", "Hello"));
            Assert.Null(exception);
        }

        [Fact]
        public async Task NotifyAsync_WithAllUserId_SendsToAll()
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

        [Fact]
        public void Attach_MultipleObservers_AllAttached()
        {
            // Arrange
            var subject = new NotificationSubject();
            var mockObserver1 = new Mock<INotificationObserver>();
            var mockObserver2 = new Mock<INotificationObserver>();
            var mockObserver3 = new Mock<INotificationObserver>();

            mockObserver1.Setup(o => o.UpdateAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            mockObserver2.Setup(o => o.UpdateAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            mockObserver3.Setup(o => o.UpdateAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            // Act
            subject.Attach(mockObserver1.Object);
            subject.Attach(mockObserver2.Object);
            subject.Attach(mockObserver3.Object);
            subject.NotifyAsync("user", "msg").Wait();

            // Assert
            mockObserver1.Verify(o => o.UpdateAsync("user", "msg"), Times.Once);
            mockObserver2.Verify(o => o.UpdateAsync("user", "msg"), Times.Once);
            mockObserver3.Verify(o => o.UpdateAsync("user", "msg"), Times.Once);
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
            await observer.UpdateAsync("all", "Broadcast message");

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
            await observer.UpdateAsync("user123", "Personal message");

            // Assert
            mockClients.Verify(c => c.User("user123"), Times.Once);
        }
    }
}

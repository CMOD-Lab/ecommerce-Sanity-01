using System;
using Xunit;
using EcommerceWebApi;

namespace EcommerceWebApi.Tests
{
    public class AppSettingsTests
    {
        [Fact]
        public void AppSettings_DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var settings = new AppSettings();

            // Assert
            Assert.NotNull(settings);
        }

        [Fact]
        public void AppSettings_SetSecret_ReturnsCorrectValue()
        {
            // Arrange
            var settings = new AppSettings();

            // Act
            settings.Secret = "my-super-secret-key-for-jwt-token";

            // Assert
            Assert.Equal("my-super-secret-key-for-jwt-token", settings.Secret);
        }

        [Fact]
        public void AppSettings_SecretIsString_TypeIsCorrect()
        {
            // Arrange
            var settings = new AppSettings();
            settings.Secret = "test-secret";

            // Assert
            Assert.IsType<string>(settings.Secret);
        }

        [Fact]
        public void AppSettings_SetEmptySecret_ReturnsEmptyString()
        {
            // Arrange
            var settings = new AppSettings();

            // Act
            settings.Secret = string.Empty;

            // Assert
            Assert.Equal(string.Empty, settings.Secret);
        }

        [Fact]
        public void AppSettings_SetLongSecret_ReturnsCorrectValue()
        {
            // Arrange
            var settings = new AppSettings();
            var longSecret = new string('x', 256);

            // Act
            settings.Secret = longSecret;

            // Assert
            Assert.Equal(longSecret, settings.Secret);
            Assert.Equal(256, settings.Secret.Length);
        }
    }

    public class ChangeSetTests
    {
        [Fact]
        public void ChangeSet_DefaultConstructor_ActionsIsEmpty()
        {
            // Arrange & Act
            var changeSet = new ChangeSet();

            // Assert
            Assert.NotNull(changeSet.Actions);
            Assert.Empty(changeSet.Actions);
        }

        [Fact]
        public void ChangeSet_AddAction_ActionIsAdded()
        {
            // Arrange
            var changeSet = new ChangeSet();
            bool executed = false;
            Action action = () => { executed = true; };

            // Act
            changeSet.Actions.Add(action);

            // Assert
            Assert.Single(changeSet.Actions);
        }

        [Fact]
        public void ChangeSet_AddMultipleActions_AllActionsAdded()
        {
            // Arrange
            var changeSet = new ChangeSet();

            // Act
            changeSet.Actions.Add(() => { });
            changeSet.Actions.Add(() => { });
            changeSet.Actions.Add(() => { });

            // Assert
            Assert.Equal(3, changeSet.Actions.Count);
        }

        [Fact]
        public void ChangeSet_ExecuteActions_AllActionsRun()
        {
            // Arrange
            var changeSet = new ChangeSet();
            int counter = 0;
            changeSet.Actions.Add(() => counter++);
            changeSet.Actions.Add(() => counter++);

            // Act
            foreach (var action in changeSet.Actions)
            {
                action();
            }

            // Assert
            Assert.Equal(2, counter);
        }
    }

    public class UnitOfWorkTests
    {
        [Fact]
        public void UnitOfWork_StartTransaction_IsTransactionInProgressReturnsTrue()
        {
            // Arrange
            var uow = new UnitOfWork();

            // Act
            uow.StartTransaction();

            // Assert
            Assert.True(uow.IsTransactionInProgress());

            // Cleanup
            uow.AbortTransaction();
            uow.Dispose();
        }

        [Fact]
        public void UnitOfWork_NoTransaction_IsTransactionInProgressReturnsFalse()
        {
            // Arrange
            var uow = new UnitOfWork();

            // Act & Assert
            Assert.False(uow.IsTransactionInProgress());

            // Cleanup
            uow.Dispose();
        }

        [Fact]
        public void UnitOfWork_StartTransaction_WhenAlreadyInProgress_ThrowsInvalidOperationException()
        {
            // Arrange
            var uow = new UnitOfWork();
            uow.StartTransaction();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => uow.StartTransaction());

            // Cleanup
            uow.AbortTransaction();
            uow.Dispose();
        }

        [Fact]
        public void UnitOfWork_AbortTransaction_WhenNoTransaction_ThrowsInvalidOperationException()
        {
            // Arrange
            var uow = new UnitOfWork();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => uow.AbortTransaction());

            // Cleanup
            uow.Dispose();
        }

        [Fact]
        public void UnitOfWork_CommitTransaction_WhenNoTransaction_ThrowsInvalidOperationException()
        {
            // Arrange
            var uow = new UnitOfWork();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => uow.CommitTransaction());

            // Cleanup
            uow.Dispose();
        }

        [Fact]
        public void UnitOfWork_AbortTransaction_AfterStart_IsTransactionInProgressReturnsFalse()
        {
            // Arrange
            var uow = new UnitOfWork();
            uow.StartTransaction();

            // Act
            uow.AbortTransaction();

            // Assert
            Assert.False(uow.IsTransactionInProgress());

            // Cleanup
            uow.Dispose();
        }

        [Fact]
        public void UnitOfWork_CommitTransaction_AfterStart_IsTransactionInProgressReturnsFalse()
        {
            // Arrange
            var uow = new UnitOfWork();
            uow.StartTransaction();

            // Act
            uow.CommitTransaction();

            // Assert
            Assert.False(uow.IsTransactionInProgress());

            // Cleanup
            uow.Dispose();
        }

        [Fact]
        public void UnitOfWork_AddToTransaction_WhenNoTransaction_ThrowsInvalidOperationException()
        {
            // Arrange
            var uow = new UnitOfWork();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => uow.AddToTransaction(() => { }));

            // Cleanup
            uow.Dispose();
        }

        [Fact]
        public void UnitOfWork_AddToTransaction_WhenTransactionInProgress_AddsAction()
        {
            // Arrange
            var uow = new UnitOfWork();
            uow.StartTransaction();

            // Act & Assert (no exception)
            uow.AddToTransaction(() => { });
            Assert.True(uow.IsTransactionInProgress());

            // Cleanup
            uow.AbortTransaction();
            uow.Dispose();
        }

        [Fact]
        public void UnitOfWork_Dispose_CanBeCalledMultipleTimes()
        {
            // Arrange
            var uow = new UnitOfWork();

            // Act & Assert (no exception)
            uow.Dispose();
            uow.Dispose();
        }

        [Fact]
        public void UnitOfWork_UsersRepository_IsNotNull()
        {
            // Arrange
            var uow = new UnitOfWork();

            // Assert
            Assert.NotNull(uow.Users);

            // Cleanup
            uow.Dispose();
        }

        [Fact]
        public void UnitOfWork_ProductsRepository_IsNotNull()
        {
            // Arrange
            var uow = new UnitOfWork();

            // Assert
            Assert.NotNull(uow.Products);

            // Cleanup
            uow.Dispose();
        }

        [Fact]
        public void UnitOfWork_OrdersRepository_IsNotNull()
        {
            // Arrange
            var uow = new UnitOfWork();

            // Assert
            Assert.NotNull(uow.Orders);

            // Cleanup
            uow.Dispose();
        }
    }
}

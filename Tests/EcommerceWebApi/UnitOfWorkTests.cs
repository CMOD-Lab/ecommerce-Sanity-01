using System;
using Xunit;
using EcommerceWebApi;

namespace EcommerceWebApi.Tests
{
    public class UnitOfWorkTests
    {
        [Fact]
        public void UnitOfWork_DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var uow = new UnitOfWork();

            // Assert
            Assert.NotNull(uow);
            uow.Dispose();
        }

        [Fact]
        public void UnitOfWork_DefaultConstructor_UsersNotNull()
        {
            // Arrange & Act
            var uow = new UnitOfWork();

            // Assert
            Assert.NotNull(uow.Users);
            uow.Dispose();
        }

        [Fact]
        public void UnitOfWork_DefaultConstructor_ProductsNotNull()
        {
            // Arrange & Act
            var uow = new UnitOfWork();

            // Assert
            Assert.NotNull(uow.Products);
            uow.Dispose();
        }

        [Fact]
        public void UnitOfWork_DefaultConstructor_OrdersNotNull()
        {
            // Arrange & Act
            var uow = new UnitOfWork();

            // Assert
            Assert.NotNull(uow.Orders);
            uow.Dispose();
        }

        [Fact]
        public void IsTransactionInProgress_Initially_ReturnsFalse()
        {
            // Arrange
            var uow = new UnitOfWork();

            // Act
            var result = uow.IsTransactionInProgress();

            // Assert
            Assert.False(result);
            uow.Dispose();
        }

        [Fact]
        public void StartTransaction_WhenNoTransactionInProgress_StartsSuccessfully()
        {
            // Arrange
            var uow = new UnitOfWork();

            // Act
            uow.StartTransaction();

            // Assert
            Assert.True(uow.IsTransactionInProgress());
            uow.AbortTransaction();
            uow.Dispose();
        }

        [Fact]
        public void StartTransaction_WhenTransactionAlreadyInProgress_ThrowsException()
        {
            // Arrange
            var uow = new UnitOfWork();
            uow.StartTransaction();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => uow.StartTransaction());
            uow.AbortTransaction();
            uow.Dispose();
        }

        [Fact]
        public void AbortTransaction_WhenTransactionInProgress_AbortsSuccessfully()
        {
            // Arrange
            var uow = new UnitOfWork();
            uow.StartTransaction();

            // Act
            uow.AbortTransaction();

            // Assert
            Assert.False(uow.IsTransactionInProgress());
            uow.Dispose();
        }

        [Fact]
        public void AbortTransaction_WhenNoTransactionInProgress_ThrowsException()
        {
            // Arrange
            var uow = new UnitOfWork();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => uow.AbortTransaction());
            uow.Dispose();
        }

        [Fact]
        public void CommitTransaction_WhenNoTransactionInProgress_ThrowsException()
        {
            // Arrange
            var uow = new UnitOfWork();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => uow.CommitTransaction());
            uow.Dispose();
        }

        [Fact]
        public void CommitTransaction_WhenTransactionInProgress_CommitsSuccessfully()
        {
            // Arrange
            var uow = new UnitOfWork();
            uow.StartTransaction();

            // Act
            uow.CommitTransaction();

            // Assert
            Assert.False(uow.IsTransactionInProgress());
            uow.Dispose();
        }

        [Fact]
        public void Dispose_CanBeCalledMultipleTimes_NoException()
        {
            // Arrange
            var uow = new UnitOfWork();

            // Act & Assert - should not throw
            uow.Dispose();
            uow.Dispose();
        }

        [Fact]
        public void StartTransaction_AfterAbort_CanStartAgain()
        {
            // Arrange
            var uow = new UnitOfWork();
            uow.StartTransaction();
            uow.AbortTransaction();

            // Act
            uow.StartTransaction();

            // Assert
            Assert.True(uow.IsTransactionInProgress());
            uow.AbortTransaction();
            uow.Dispose();
        }
    }

    public class ChangeSetTests
    {
        [Fact]
        public void ChangeSet_DefaultConstructor_CreatesEmptyActionsList()
        {
            // Arrange & Act
            var changeSet = new ChangeSet();

            // Assert
            Assert.NotNull(changeSet.Actions);
            Assert.Empty(changeSet.Actions);
        }

        [Fact]
        public void ChangeSet_AddAction_ActionAddedToList()
        {
            // Arrange
            var changeSet = new ChangeSet();
            bool actionExecuted = false;
            Action action = () => { actionExecuted = true; };

            // Act
            changeSet.Actions.Add(action);

            // Assert
            Assert.Single(changeSet.Actions);
        }

        [Fact]
        public void ChangeSet_ExecuteActions_ActionsRun()
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

        [Fact]
        public void ChangeSet_MultipleActions_AllStoredCorrectly()
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
    }

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
        public void AppSettings_Secret_CanBeEmptyString()
        {
            // Arrange
            var settings = new AppSettings();

            // Act
            settings.Secret = "";

            // Assert
            Assert.Equal("", settings.Secret);
        }
    }
}

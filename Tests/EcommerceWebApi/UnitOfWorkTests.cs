using System;
using Xunit;
using EcommerceWebApi;

namespace EcommerceWebApi.Tests
{
    public class UnitOfWorkTests
    {
        [Fact]
        public void UnitOfWork_IsTransactionInProgress_ReturnsFalseInitially()
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
        public void UnitOfWork_StartTransaction_SetsTransactionInProgress()
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
        public void UnitOfWork_StartTransaction_WhenAlreadyInProgress_ThrowsInvalidOperationException()
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
        public void UnitOfWork_CommitTransaction_WhenNoTransaction_ThrowsInvalidOperationException()
        {
            // Arrange
            var uow = new UnitOfWork();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => uow.CommitTransaction());

            uow.Dispose();
        }

        [Fact]
        public void UnitOfWork_AbortTransaction_WhenNoTransaction_ThrowsInvalidOperationException()
        {
            // Arrange
            var uow = new UnitOfWork();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => uow.AbortTransaction());

            uow.Dispose();
        }

        [Fact]
        public void UnitOfWork_AbortTransaction_ClearsTransaction()
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
        public void UnitOfWork_CommitTransaction_ExecutesActionsAndClearsTransaction()
        {
            // Arrange
            var uow = new UnitOfWork();
            uow.StartTransaction();
            bool actionExecuted = false;
            uow.AddToTransaction(() => actionExecuted = true);

            // Act
            uow.CommitTransaction();

            // Assert
            Assert.True(actionExecuted);
            Assert.False(uow.IsTransactionInProgress());

            uow.Dispose();
        }

        [Fact]
        public void UnitOfWork_AddToTransaction_WhenNoTransaction_ThrowsInvalidOperationException()
        {
            // Arrange
            var uow = new UnitOfWork();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => uow.AddToTransaction(() => { }));

            uow.Dispose();
        }

        [Fact]
        public void UnitOfWork_Users_IsNotNull()
        {
            // Arrange
            var uow = new UnitOfWork();

            // Assert
            Assert.NotNull(uow.Users);

            uow.Dispose();
        }

        [Fact]
        public void UnitOfWork_Products_IsNotNull()
        {
            // Arrange
            var uow = new UnitOfWork();

            // Assert
            Assert.NotNull(uow.Products);

            uow.Dispose();
        }

        [Fact]
        public void UnitOfWork_Orders_IsNotNull()
        {
            // Arrange
            var uow = new UnitOfWork();

            // Assert
            Assert.NotNull(uow.Orders);

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
        public void ChangeSet_DefaultConstructor_HasEmptyActions()
        {
            // Arrange & Act
            var changeSet = new ChangeSet();

            // Assert
            Assert.NotNull(changeSet.Actions);
            Assert.Empty(changeSet.Actions);
        }

        [Fact]
        public void ChangeSet_AddAction_IncreasesCount()
        {
            // Arrange
            var changeSet = new ChangeSet();

            // Act
            changeSet.Actions.Add(() => { });
            changeSet.Actions.Add(() => { });

            // Assert
            Assert.Equal(2, changeSet.Actions.Count);
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
        public void AppSettings_Secret_CanBeUpdated()
        {
            // Arrange
            var settings = new AppSettings { Secret = "initial-secret" };

            // Act
            settings.Secret = "updated-secret";

            // Assert
            Assert.Equal("updated-secret", settings.Secret);
        }
    }
}

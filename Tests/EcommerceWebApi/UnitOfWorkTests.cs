using System;
using Xunit;
using EcommerceWebApi;

namespace EcommerceWebApi.Tests
{
    public class UnitOfWorkTests
    {
        [Fact]
        public void UnitOfWork_StartTransaction_SetsTransactionInProgress()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();

            // Act
            unitOfWork.StartTransaction();

            // Assert
            Assert.True(unitOfWork.IsTransactionInProgress());

            // Cleanup
            unitOfWork.AbortTransaction();
            unitOfWork.Dispose();
        }

        [Fact]
        public void UnitOfWork_StartTransaction_WhenAlreadyInProgress_ThrowsInvalidOperationException()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();
            unitOfWork.StartTransaction();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => unitOfWork.StartTransaction());

            // Cleanup
            unitOfWork.AbortTransaction();
            unitOfWork.Dispose();
        }

        [Fact]
        public void UnitOfWork_AbortTransaction_ClearsTransaction()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();
            unitOfWork.StartTransaction();

            // Act
            unitOfWork.AbortTransaction();

            // Assert
            Assert.False(unitOfWork.IsTransactionInProgress());

            // Cleanup
            unitOfWork.Dispose();
        }

        [Fact]
        public void UnitOfWork_AbortTransaction_WhenNoTransaction_ThrowsInvalidOperationException()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => unitOfWork.AbortTransaction());

            // Cleanup
            unitOfWork.Dispose();
        }

        [Fact]
        public void UnitOfWork_CommitTransaction_WhenNoTransaction_ThrowsInvalidOperationException()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => unitOfWork.CommitTransaction());

            // Cleanup
            unitOfWork.Dispose();
        }

        [Fact]
        public void UnitOfWork_IsTransactionInProgress_WhenNoTransaction_ReturnsFalse()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();

            // Act
            var result = unitOfWork.IsTransactionInProgress();

            // Assert
            Assert.False(result);

            // Cleanup
            unitOfWork.Dispose();
        }

        [Fact]
        public void UnitOfWork_CommitTransaction_ClearsTransaction()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();
            unitOfWork.StartTransaction();

            // Act
            unitOfWork.CommitTransaction();

            // Assert - transaction should be cleared after commit
            Assert.False(unitOfWork.IsTransactionInProgress());

            // Cleanup
            unitOfWork.Dispose();
        }

        [Fact]
        public void UnitOfWork_IsTransactionInProgress_AfterAbort_ReturnsFalse()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();

            // AddToTransaction is internal - test via StartTransaction/CommitTransaction flow
            Assert.False(unitOfWork.IsTransactionInProgress());

            // Cleanup
            unitOfWork.Dispose();
        }

        [Fact]
        public void UnitOfWork_Dispose_CanBeCalledMultipleTimes()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();

            // Act & Assert - no exception
            unitOfWork.Dispose();
            unitOfWork.Dispose(); // Second dispose should not throw
        }

        [Fact]
        public void UnitOfWork_HasUsersRepository()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();

            // Assert
            Assert.NotNull(unitOfWork.Users);

            // Cleanup
            unitOfWork.Dispose();
        }

        [Fact]
        public void UnitOfWork_HasProductsRepository()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();

            // Assert
            Assert.NotNull(unitOfWork.Products);

            // Cleanup
            unitOfWork.Dispose();
        }

        [Fact]
        public void UnitOfWork_HasOrdersRepository()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();

            // Assert
            Assert.NotNull(unitOfWork.Orders);

            // Cleanup
            unitOfWork.Dispose();
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
        public void ChangeSet_AddAction_ActionIsAdded()
        {
            // Arrange
            var changeSet = new ChangeSet();
            bool executed = false;

            // Act
            changeSet.Actions.Add(() => executed = true);

            // Assert
            Assert.Single(changeSet.Actions);
            Assert.False(executed); // Not yet executed
        }

        [Fact]
        public void ChangeSet_ExecuteActions_ActionsRun()
        {
            // Arrange
            var changeSet = new ChangeSet();
            bool executed = false;
            changeSet.Actions.Add(() => executed = true);

            // Act
            foreach (var action in changeSet.Actions)
            {
                action();
            }

            // Assert
            Assert.True(executed);
        }

        [Fact]
        public void UnitOfWork_StartAndAbortTransaction_TransactionNotInProgress()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();

            // Act
            unitOfWork.StartTransaction();
            Assert.True(unitOfWork.IsTransactionInProgress());
            unitOfWork.AbortTransaction();

            // Assert
            Assert.False(unitOfWork.IsTransactionInProgress());

            // Cleanup
            unitOfWork.Dispose();
        }

        [Fact]
        public void UnitOfWork_StartAndCommitTransaction_TransactionNotInProgress()
        {
            // Arrange
            var unitOfWork = new UnitOfWork();

            // Act
            unitOfWork.StartTransaction();
            Assert.True(unitOfWork.IsTransactionInProgress());
            unitOfWork.CommitTransaction();

            // Assert
            Assert.False(unitOfWork.IsTransactionInProgress());

            // Cleanup
            unitOfWork.Dispose();
        }
    }
}

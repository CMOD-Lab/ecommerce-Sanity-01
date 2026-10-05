using System;
using System.Collections.Generic;
using Xunit;
using EcommerceWebApi.Entities;

namespace EcommerceWebApi.Entities.Tests
{
    public class OrderTests
    {
        [Fact]
        public void Order_DefaultConstructor_IdIsGuid()
        {
            // Arrange & Act
            var order = new Order();

            // Assert
            Assert.NotNull(order.Id);
            Assert.True(Guid.TryParse(order.Id, out _));
        }

        [Fact]
        public void Order_DefaultConstructor_StatusIsPending()
        {
            // Arrange & Act
            var order = new Order();

            // Assert
            Assert.Equal(OrderStatus.Pending, order.Status);
        }

        [Fact]
        public void Order_SetUserId_ReturnsCorrectValue()
        {
            // Arrange
            var order = new Order();

            // Act
            order.UserId = "user-123";

            // Assert
            Assert.Equal("user-123", order.UserId);
        }

        [Fact]
        public void Order_SetProductList_ReturnsCorrectValue()
        {
            // Arrange
            var order = new Order();
            var productList = new Dictionary<int, int> { { 1, 2 }, { 3, 4 } };

            // Act
            order.ProductList = productList;

            // Assert
            Assert.Equal(productList, order.ProductList);
            Assert.Equal(2, order.ProductList.Count);
        }

        [Fact]
        public void Order_SetCreated_ReturnsCorrectValue()
        {
            // Arrange
            var order = new Order();
            var now = DateTime.UtcNow;

            // Act
            order.Created = now;

            // Assert
            Assert.Equal(now, order.Created);
        }

        [Fact]
        public void Order_SetUpdated_ReturnsCorrectValue()
        {
            // Arrange
            var order = new Order();
            var updated = DateTime.UtcNow;

            // Act
            order.Updated = updated;

            // Assert
            Assert.Equal(updated, order.Updated);
        }

        [Fact]
        public void Order_SetStatusSuccessed_ReturnsCorrectValue()
        {
            // Arrange
            var order = new Order();

            // Act
            order.Status = OrderStatus.Successed;

            // Assert
            Assert.Equal(OrderStatus.Successed, order.Status);
        }

        [Fact]
        public void Order_SetStatusCanceled_ReturnsCorrectValue()
        {
            // Arrange
            var order = new Order();

            // Act
            order.Status = OrderStatus.Canceled;

            // Assert
            Assert.Equal(OrderStatus.Canceled, order.Status);
        }

        [Fact]
        public void Order_TwoInstances_HaveDifferentIds()
        {
            // Arrange & Act
            var order1 = new Order();
            var order2 = new Order();

            // Assert
            Assert.NotEqual(order1.Id, order2.Id);
        }

        [Fact]
        public void Order_SetId_OverridesDefault()
        {
            // Arrange
            var order = new Order();
            var customId = "custom-order-id";

            // Act
            order.Id = customId;

            // Assert
            Assert.Equal(customId, order.Id);
        }

        [Fact]
        public void Order_FullyPopulated_AllPropertiesCorrect()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var productList = new Dictionary<int, int> { { 1, 3 } };

            // Act
            var order = new Order
            {
                Id = "order-001",
                UserId = "user-001",
                ProductList = productList,
                Created = now,
                Updated = now,
                Status = OrderStatus.Pending
            };

            // Assert
            Assert.Equal("order-001", order.Id);
            Assert.Equal("user-001", order.UserId);
            Assert.Equal(productList, order.ProductList);
            Assert.Equal(now, order.Created);
            Assert.Equal(now, order.Updated);
            Assert.Equal(OrderStatus.Pending, order.Status);
        }
    }

    public class OrderStatusTests
    {
        [Fact]
        public void OrderStatus_Pending_HasValueZero()
        {
            Assert.Equal(0, (int)OrderStatus.Pending);
        }

        [Fact]
        public void OrderStatus_Successed_HasValueOne()
        {
            Assert.Equal(1, (int)OrderStatus.Successed);
        }

        [Fact]
        public void OrderStatus_Canceled_HasValueTwo()
        {
            Assert.Equal(2, (int)OrderStatus.Canceled);
        }

        [Fact]
        public void OrderStatus_AllValues_AreDistinct()
        {
            var values = Enum.GetValues<OrderStatus>();
            Assert.Equal(3, values.Length);
        }
    }
}

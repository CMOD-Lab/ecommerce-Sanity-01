using System;
using System.Collections.Generic;
using Xunit;
using EcommerceWebApi.Entities;

namespace EcommerceWebApi.Entities.Tests
{
    public class OrderTests
    {
        [Fact]
        public void Order_DefaultConstructor_GeneratesGuidId()
        {
            // Arrange & Act
            var order = new Order();

            // Assert
            Assert.NotNull(order.Id);
            Assert.NotEmpty(order.Id);
            Assert.True(Guid.TryParse(order.Id, out _));
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
        public void Order_SetStatus_Pending_ReturnsCorrectValue()
        {
            // Arrange
            var order = new Order();

            // Act
            order.Status = OrderStatus.Pending;

            // Assert
            Assert.Equal(OrderStatus.Pending, order.Status);
        }

        [Fact]
        public void Order_SetStatus_Successed_ReturnsCorrectValue()
        {
            // Arrange
            var order = new Order();

            // Act
            order.Status = OrderStatus.Successed;

            // Assert
            Assert.Equal(OrderStatus.Successed, order.Status);
        }

        [Fact]
        public void Order_SetStatus_Canceled_ReturnsCorrectValue()
        {
            // Arrange
            var order = new Order();

            // Act
            order.Status = OrderStatus.Canceled;

            // Assert
            Assert.Equal(OrderStatus.Canceled, order.Status);
        }

        [Fact]
        public void Order_FullyPopulated_AllPropertiesCorrect()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var productList = new Dictionary<int, int> { { 1, 2 } };

            // Act
            var order = new Order
            {
                UserId = "user-abc",
                ProductList = productList,
                Created = now,
                Status = OrderStatus.Pending
            };

            // Assert
            Assert.Equal("user-abc", order.UserId);
            Assert.Equal(productList, order.ProductList);
            Assert.Equal(now, order.Created);
            Assert.Equal(OrderStatus.Pending, order.Status);
        }
    }

    public class OrderStatusTests
    {
        [Fact]
        public void OrderStatus_Pending_HasCorrectValue()
        {
            Assert.Equal(0, (int)OrderStatus.Pending);
        }

        [Fact]
        public void OrderStatus_Successed_HasCorrectValue()
        {
            Assert.Equal(1, (int)OrderStatus.Successed);
        }

        [Fact]
        public void OrderStatus_Canceled_HasCorrectValue()
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

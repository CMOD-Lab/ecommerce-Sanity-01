using System;
using System.Collections.Generic;
using Xunit;
using EcommerceWebApi.Entities;

namespace EcommerceWebApi.Tests.Entities
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
            order.UserId = "user123";

            // Assert
            Assert.Equal("user123", order.UserId);
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
        public void Order_TwoInstances_HaveDifferentIds()
        {
            // Arrange & Act
            var order1 = new Order();
            var order2 = new Order();

            // Assert
            Assert.NotEqual(order1.Id, order2.Id);
        }

        [Fact]
        public void OrderStatus_Enum_HasCorrectValues()
        {
            // Assert
            Assert.Equal(0, (int)OrderStatus.Pending);
            Assert.Equal(1, (int)OrderStatus.Successed);
            Assert.Equal(2, (int)OrderStatus.Canceled);
        }

        [Fact]
        public void Order_FullInitialization_AllPropertiesSet()
        {
            // Arrange
            var productList = new Dictionary<int, int> { { 1, 3 } };
            var created = DateTime.UtcNow;

            // Act
            var order = new Order
            {
                UserId = "user-abc",
                ProductList = productList,
                Created = created,
                Status = OrderStatus.Pending
            };

            // Assert
            Assert.Equal("user-abc", order.UserId);
            Assert.Equal(productList, order.ProductList);
            Assert.Equal(created, order.Created);
            Assert.Equal(OrderStatus.Pending, order.Status);
        }
    }
}

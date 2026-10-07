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
        public void Order_SetUserId_ReturnsCorrectUserId()
        {
            // Arrange
            var order = new Order();
            var userId = Guid.NewGuid().ToString();

            // Act
            order.UserId = userId;

            // Assert
            Assert.Equal(userId, order.UserId);
        }

        [Fact]
        public void Order_SetProductList_ReturnsCorrectProductList()
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
        public void Order_SetCreated_ReturnsCorrectDate()
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
        public void Order_SetUpdated_ReturnsCorrectDate()
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
        public void Order_SetStatus_Pending_ReturnsCorrectStatus()
        {
            // Arrange
            var order = new Order();

            // Act
            order.Status = OrderStatus.Pending;

            // Assert
            Assert.Equal(OrderStatus.Pending, order.Status);
        }

        [Fact]
        public void Order_SetStatus_Successed_ReturnsCorrectStatus()
        {
            // Arrange
            var order = new Order();

            // Act
            order.Status = OrderStatus.Successed;

            // Assert
            Assert.Equal(OrderStatus.Successed, order.Status);
        }

        [Fact]
        public void Order_SetStatus_Canceled_ReturnsCorrectStatus()
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
        public void Order_FullyPopulated_AllPropertiesCorrect()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var productList = new Dictionary<int, int> { { 1, 3 } };
            var created = DateTime.UtcNow;

            // Act
            var order = new Order
            {
                UserId = userId,
                ProductList = productList,
                Created = created,
                Status = OrderStatus.Pending
            };

            // Assert
            Assert.Equal(userId, order.UserId);
            Assert.Equal(productList, order.ProductList);
            Assert.Equal(created, order.Created);
            Assert.Equal(OrderStatus.Pending, order.Status);
        }

        [Theory]
        [InlineData(OrderStatus.Pending)]
        [InlineData(OrderStatus.Successed)]
        [InlineData(OrderStatus.Canceled)]
        public void OrderStatus_AllValues_AreValid(OrderStatus status)
        {
            // Arrange
            var order = new Order();

            // Act
            order.Status = status;

            // Assert
            Assert.Equal(status, order.Status);
        }

        [Fact]
        public void Order_ProductList_EmptyDictionary_IsValid()
        {
            // Arrange
            var order = new Order();

            // Act
            order.ProductList = new Dictionary<int, int>();

            // Assert
            Assert.NotNull(order.ProductList);
            Assert.Empty(order.ProductList);
        }
    }
}

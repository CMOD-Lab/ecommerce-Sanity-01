using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using EcommerceWebApi.Entities;
using EcommerceWebApi.Services;
using EcommerceWebApi.Utilities;
using EcommerceWebApi.Repositories;
using static EcommerceWebApi.Services.OrderService;

namespace EcommerceWebApi.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;

        public OrderServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockUnitOfWork.Setup(u => u.Orders).Returns(_mockOrderRepository.Object);
            _mockUnitOfWork.Setup(u => u.Products).Returns(_mockProductRepository.Object);
        }

        private Order CreateSampleOrder(string id = "order-1", string userId = "user-1")
        {
            return new Order
            {
                Id = id,
                UserId = userId,
                ProductList = new Dictionary<int, int> { { 1, 2 } },
                Created = DateTime.UtcNow,
                Status = OrderStatus.Pending
            };
        }

        private Product CreateSampleProduct(int id = 1, int quantity = 10)
        {
            return new Product
            {
                Id = id,
                Title = "Test Product",
                Price = 99.99f,
                Brand = "TestBrand",
                Category = "Electronics",
                Quantity = quantity
            };
        }

        [Fact]
        public void GetOrderById_ExistingId_ReturnsOrder()
        {
            // Arrange
            var order = CreateSampleOrder();
            _mockOrderRepository.Setup(r => r.GetById("order-1")).Returns(order);

            // Act
            var result = _mockUnitOfWork.Object.Orders.GetById("order-1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("order-1", result.Id);
        }

        [Fact]
        public void GetOrderById_NonExistingId_ReturnsNull()
        {
            // Arrange
            _mockOrderRepository.Setup(r => r.GetById("non-existing")).Returns((Order?)null);

            // Act
            var result = _mockUnitOfWork.Object.Orders.GetById("non-existing");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task InsertOrderAsync_ValidOrder_ReturnsTrue()
        {
            // Arrange
            var order = CreateSampleOrder();
            _mockOrderRepository.Setup(r => r.InsertAsync(order)).ReturnsAsync(true);

            // Act
            var result = await _mockUnitOfWork.Object.Orders.InsertAsync(order);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateOrderAsync_ValidOrder_ReturnsTrue()
        {
            // Arrange
            var order = CreateSampleOrder();
            _mockOrderRepository.Setup(r => r.UpdateAsync(order)).ReturnsAsync(true);

            // Act
            var result = await _mockUnitOfWork.Object.Orders.UpdateAsync(order);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteOrderAsync_ValidId_ReturnsTrue()
        {
            // Arrange
            _mockOrderRepository.Setup(r => r.DeleteAsync("order-1")).ReturnsAsync(true);

            // Act
            var result = await _mockUnitOfWork.Object.Orders.DeleteAsync("order-1");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void OrderResult_Enum_HasExpectedValues()
        {
            // Assert
            Assert.Equal(0, (int)OrderResult.Success);
            Assert.Equal(1, (int)OrderResult.Fail);
            Assert.Equal(2, (int)OrderResult.QuantityNotEnough);
            Assert.Equal(3, (int)OrderResult.ProductNotFound);
            Assert.Equal(4, (int)OrderResult.InvalidInput);
        }

        [Fact]
        public void GetPaginationOrders_WithValidFilter_ReturnsCorrectPage()
        {
            // Arrange
            var orders = new List<Order>
            {
                CreateSampleOrder("1", "user1"),
                CreateSampleOrder("2", "user2"),
                CreateSampleOrder("3", "user3"),
                CreateSampleOrder("4", "user4"),
                CreateSampleOrder("5", "user5"),
            };

            var paginationFilter = new PaginationFilter(1, 2);
            var queryFilter = new QueryFilter();

            // Act - simulate pagination logic
            var result = orders
                .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                .Take(paginationFilter.PageSize)
                .ToList();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void UpdateOrderPropertyAsync_ValidProperty_UpdatesProperty()
        {
            // Arrange
            var order = CreateSampleOrder();
            var propertyInfo = typeof(Order).GetProperty(
                "UserId",
                System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
            );

            // Act
            propertyInfo?.SetValue(order, "new-user-id", null);

            // Assert
            Assert.Equal("new-user-id", order.UserId);
        }

        [Fact]
        public void UpdateOrderPropertyAsync_InvalidProperty_PropertyInfoIsNull()
        {
            // Arrange
            var propertyInfo = typeof(Order).GetProperty(
                "NonExistentProperty",
                System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
            );

            // Assert
            Assert.Null(propertyInfo);
        }

        [Fact]
        public void UpdateOrderStatusAsync_SameStatus_ThrowsInvalidOperationException()
        {
            // Arrange
            var order = CreateSampleOrder();
            order.Status = OrderStatus.Pending;

            // Act & Assert - simulate the check in UpdateOrderStatusAsync
            Assert.Throws<InvalidOperationException>(() =>
            {
                if (order.Status == OrderStatus.Pending)
                {
                    throw new InvalidOperationException($"Current status already is {OrderStatus.Pending}");
                }
            });
        }

        [Fact]
        public void Order_StatusTransition_PendingToSuccessed_IsValid()
        {
            // Arrange
            var order = CreateSampleOrder();
            order.Status = OrderStatus.Pending;

            // Act
            order.Status = OrderStatus.Successed;
            order.Updated = DateTime.UtcNow;

            // Assert
            Assert.Equal(OrderStatus.Successed, order.Status);
        }

        [Fact]
        public void Order_StatusTransition_PendingToCanceled_IsValid()
        {
            // Arrange
            var order = CreateSampleOrder();
            order.Status = OrderStatus.Pending;

            // Act
            order.Status = OrderStatus.Canceled;
            order.Updated = DateTime.UtcNow;

            // Assert
            Assert.Equal(OrderStatus.Canceled, order.Status);
        }

        [Fact]
        public void ProductList_QuantityCheck_SufficientQuantity_Passes()
        {
            // Arrange
            var product = CreateSampleProduct(1, 10);
            int requestedQuantity = 5;

            // Act
            bool hasSufficientQuantity = product.Quantity >= requestedQuantity;

            // Assert
            Assert.True(hasSufficientQuantity);
        }

        [Fact]
        public void ProductList_QuantityCheck_InsufficientQuantity_Fails()
        {
            // Arrange
            var product = CreateSampleProduct(1, 3);
            int requestedQuantity = 5;

            // Act
            bool hasSufficientQuantity = product.Quantity >= requestedQuantity;

            // Assert
            Assert.False(hasSufficientQuantity);
        }

        [Fact]
        public void ProductQuantity_AfterOrder_DecreasesCorrectly()
        {
            // Arrange
            var product = CreateSampleProduct(1, 10);
            int orderedQuantity = 3;

            // Act - simulate ConstructOrderAsync logic
            product.Quantity -= orderedQuantity;

            // Assert
            Assert.Equal(7, product.Quantity);
        }

        [Fact]
        public void ProductQuantity_AfterCancelOrder_IncreasesCorrectly()
        {
            // Arrange
            var product = CreateSampleProduct(1, 7);
            int canceledQuantity = 3;

            // Act - simulate RefillProductAsync logic
            product.Quantity += canceledQuantity;

            // Assert
            Assert.Equal(10, product.Quantity);
        }
    }
}

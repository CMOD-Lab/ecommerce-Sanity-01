using EcommerceWebApi.Entities;
using EcommerceWebApi.Repositories;
using EcommerceWebApi.Services;
using EcommerceWebApi.Tests.Helpers;
using EcommerceWebApi.Utilities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
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
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(u => u.Orders).Returns(_mockOrderRepository.Object);
            _mockUnitOfWork.Setup(u => u.Products).Returns(_mockProductRepository.Object);
        }

        private Order CreateTestOrder(string id = "order-1", string userId = "user-1")
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

        private Product CreateTestProduct(int id = 1, int quantity = 10)
        {
            return new Product
            {
                Id = id,
                Title = $"Product {id}",
                Price = 50f,
                Rating = 4f,
                Brand = "Brand",
                Category = "Cat",
                Thumbnail = new Uri("https://example.com/img.jpg"),
                Quantity = quantity
            };
        }

        [Fact]
        public void GetOrderById_ExistingId_ReturnsOrder()
        {
            // Arrange
            var order = CreateTestOrder("order-1");
            _mockOrderRepository.Setup(r => r.GetById("order-1")).Returns(order);
            var service = CreateOrderService();

            // Act
            var result = service.GetOrderById("order-1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("order-1", result!.Id);
        }

        [Fact]
        public void GetOrderById_NonExistingId_ReturnsNull()
        {
            // Arrange
            _mockOrderRepository.Setup(r => r.GetById("nonexistent")).Returns((Order?)null);
            var service = CreateOrderService();

            // Act
            var result = service.GetOrderById("nonexistent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetAllOrders_ReturnsAllOrders()
        {
            // Arrange
            var orders = new List<Order>
            {
                CreateTestOrder("order-1"),
                CreateTestOrder("order-2")
            };
            var mockCollection = new MockDocumentCollection<Order>(orders);
            _mockOrderRepository.Setup(r => r.GetAll()).Returns(mockCollection);
            var service = CreateOrderService();

            // Act
            var result = service.GetAllOrders();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetPaginationOrders_ReturnsCorrectPage()
        {
            // Arrange
            var orders = Enumerable.Range(1, 10)
                .Select(i => CreateTestOrder($"order-{i}", "user-1"))
                .ToList();
            var mockCollection = new MockDocumentCollection<Order>(orders);
            _mockOrderRepository.Setup(r => r.GetAll()).Returns(mockCollection);
            var service = CreateOrderService();

            var paginationFilter = new PaginationFilter(1, 5);
            var queryFilter = new QueryFilter();

            // Act
            var result = service.GetPaginationOrders(paginationFilter, queryFilter, out int count);

            // Assert
            Assert.Equal(10, count);
            Assert.Equal(5, result.Count);
        }

        [Fact]
        public async Task UpdateOrderAsync_Success_ReturnsSuccess()
        {
            // Arrange
            var order = CreateTestOrder();
            _mockOrderRepository.Setup(r => r.UpdateAsync(order)).ReturnsAsync(true);
            var service = CreateOrderService();

            // Act
            var result = await service.UpdateOrderAsync(order);

            // Assert
            Assert.Equal(OrderResult.Success, result);
        }

        [Fact]
        public async Task UpdateOrderAsync_Failure_ReturnsFail()
        {
            // Arrange
            var order = CreateTestOrder();
            _mockOrderRepository.Setup(r => r.UpdateAsync(order)).ReturnsAsync(false);
            var service = CreateOrderService();

            // Act
            var result = await service.UpdateOrderAsync(order);

            // Assert
            Assert.Equal(OrderResult.Fail, result);
        }

        [Fact]
        public async Task UpdateOrderPropertyAsync_ValidProperty_ReturnsSuccess()
        {
            // Arrange
            var order = CreateTestOrder();
            _mockOrderRepository.Setup(r => r.UpdateAsync(It.IsAny<Order>())).ReturnsAsync(true);
            var service = CreateOrderService();

            // Act
            var result = await service.UpdateOrderPropertyAsync(order, "UserId", "new-user-id");

            // Assert
            Assert.Equal(OrderResult.Success, result);
            Assert.Equal("new-user-id", order.UserId);
        }

        [Fact]
        public async Task UpdateOrderPropertyAsync_InvalidProperty_ReturnsInvalidInput()
        {
            // Arrange
            var order = CreateTestOrder();
            var service = CreateOrderService();

            // Act
            var result = await service.UpdateOrderPropertyAsync(order, "NonExistentProp", "value");

            // Assert
            Assert.Equal(OrderResult.InvalidInput, result);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_SameStatus_ThrowsInvalidOperationException()
        {
            // Arrange
            var order = CreateTestOrder();
            order.Status = OrderStatus.Pending;
            _mockUnitOfWork.Setup(u => u.StartTransaction());
            _mockUnitOfWork.Setup(u => u.AbortTransaction());
            var service = CreateOrderService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateOrderStatusAsync(order, OrderStatus.Pending)
            );
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_DifferentStatus_ReturnsSuccess()
        {
            // Arrange
            var order = CreateTestOrder();
            order.Status = OrderStatus.Pending;
            _mockUnitOfWork.Setup(u => u.StartTransaction());
            _mockUnitOfWork.Setup(u => u.CommitTransaction());
            _mockOrderRepository.Setup(r => r.UpdateAsync(It.IsAny<Order>())).ReturnsAsync(true);
            var service = CreateOrderService();

            // Act
            var result = await service.UpdateOrderStatusAsync(order, OrderStatus.Successed);

            // Assert
            Assert.Equal(OrderResult.Success, result);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_CancelWithProductRefill_ReturnsSuccess()
        {
            // Arrange
            var order = CreateTestOrder();
            order.Status = OrderStatus.Pending;
            order.ProductList = new Dictionary<int, int> { { 1, 2 } };

            var product = CreateTestProduct(1, 5);
            _mockProductRepository.Setup(r => r.GetById(1)).Returns(product);
            _mockProductRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>())).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.StartTransaction());
            _mockUnitOfWork.Setup(u => u.CommitTransaction());
            _mockOrderRepository.Setup(r => r.UpdateAsync(It.IsAny<Order>())).ReturnsAsync(true);
            var service = CreateOrderService();

            // Act
            var result = await service.UpdateOrderStatusAsync(order, OrderStatus.Canceled);

            // Assert
            Assert.Equal(OrderResult.Success, result);
        }

        [Fact]
        public async Task DeleteOrderAsync_NonExistingOrder_ReturnsProductNotFound()
        {
            // Arrange
            _mockOrderRepository.Setup(r => r.GetById("nonexistent")).Returns((Order?)null);
            _mockUnitOfWork.Setup(u => u.StartTransaction());
            var service = CreateOrderService();

            // Act
            var result = await service.DeleteOrderAsync("nonexistent");

            // Assert
            Assert.Equal(OrderResult.ProductNotFound, result);
        }

        [Fact]
        public async Task DeleteOrderAsync_CanceledOrder_DeletesSuccessfully()
        {
            // Arrange
            var order = CreateTestOrder();
            order.Status = OrderStatus.Canceled;
            _mockOrderRepository.Setup(r => r.GetById("order-1")).Returns(order);
            _mockOrderRepository.Setup(r => r.DeleteAsync("order-1")).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.StartTransaction());
            _mockUnitOfWork.Setup(u => u.CommitTransaction());
            var service = CreateOrderService();

            // Act
            var result = await service.DeleteOrderAsync("order-1");

            // Assert
            Assert.Equal(OrderResult.Success, result);
        }

        [Fact]
        public async Task DeleteOrderAsync_PendingOrder_RefillsAndDeletes()
        {
            // Arrange
            var order = CreateTestOrder();
            order.Status = OrderStatus.Pending;
            order.ProductList = new Dictionary<int, int> { { 1, 2 } };

            var product = CreateTestProduct(1, 5);
            _mockOrderRepository.Setup(r => r.GetById("order-1")).Returns(order);
            _mockOrderRepository.Setup(r => r.DeleteAsync("order-1")).ReturnsAsync(true);
            _mockProductRepository.Setup(r => r.GetById(1)).Returns(product);
            _mockProductRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>())).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.StartTransaction());
            _mockUnitOfWork.Setup(u => u.CommitTransaction());
            var service = CreateOrderService();

            // Act
            var result = await service.DeleteOrderAsync("order-1");

            // Assert
            Assert.Equal(OrderResult.Success, result);
        }

        [Fact]
        public async Task InsertOrderAsync_ProductNotFound_ReturnsProductNotFound()
        {
            // Arrange
            _mockProductRepository.Setup(r => r.GetById(It.IsAny<int>())).Returns((Product?)null);
            _mockUnitOfWork.Setup(u => u.StartTransaction());
            _mockUnitOfWork.Setup(u => u.AbortTransaction());
            var service = CreateOrderService();

            // Act
            var result = await service.InsertOrderAsync("user-1", new Dictionary<int, int> { { 999, 1 } });

            // Assert
            Assert.Equal(OrderResult.ProductNotFound, result);
        }

        [Fact]
        public async Task InsertOrderAsync_InsufficientQuantity_ReturnsProductNotFound()
        {
            // Arrange
            var product = CreateTestProduct(1, 1); // Only 1 in stock
            _mockProductRepository.Setup(r => r.GetById(1)).Returns(product);
            _mockUnitOfWork.Setup(u => u.StartTransaction());
            _mockUnitOfWork.Setup(u => u.AbortTransaction());
            var service = CreateOrderService();

            // Act
            var result = await service.InsertOrderAsync("user-1", new Dictionary<int, int> { { 1, 5 } }); // Requesting 5

            // Assert
            Assert.Equal(OrderResult.ProductNotFound, result);
        }

        [Fact]
        public async Task InsertOrderAsync_Success_ReturnsSuccess()
        {
            // Arrange
            var product = CreateTestProduct(1, 10);
            _mockProductRepository.Setup(r => r.GetById(1)).Returns(product);
            _mockProductRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>())).ReturnsAsync(true);
            _mockOrderRepository.Setup(r => r.InsertAsync(It.IsAny<Order>())).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.StartTransaction());
            _mockUnitOfWork.Setup(u => u.CommitTransaction());
            var service = CreateOrderService();

            // Act
            var result = await service.InsertOrderAsync("user-1", new Dictionary<int, int> { { 1, 2 } });

            // Assert
            Assert.Equal(OrderResult.Success, result);
        }

        [Fact]
        public void Dispose_DoesNotThrow()
        {
            // Arrange
            var service = CreateOrderService();

            // Act & Assert (no exception)
            service.Dispose();
        }

        private OrderService CreateOrderService()
        {
            return new TestableOrderService(_mockUnitOfWork.Object);
        }
    }

    /// <summary>
    /// Testable OrderService that accepts IUnitOfWork mock
    /// </summary>
    internal class TestableOrderService : OrderService
    {
        public TestableOrderService(IUnitOfWork unitOfWork) : base(null!, null!)
        {
            typeof(OrderService)
                .GetField("_unitOfWork", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(this, unitOfWork);

            // Create a testable product service
            var productService = new TestableProductServiceForOrderTest(unitOfWork);
            typeof(OrderService)
                .GetField("_productService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(this, productService);
        }
    }

    internal class TestableProductServiceForOrderTest : ProductService
    {
        public TestableProductServiceForOrderTest(IUnitOfWork unitOfWork) : base(null!)
        {
            typeof(ProductService)
                .GetField("_unitOfWork", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(this, unitOfWork);
        }
    }
}

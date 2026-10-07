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

namespace EcommerceWebApi.Services.Tests
{
    // Testable subclass for OrderService
    internal class OrderServiceTestable : OrderService
    {
        public OrderServiceTestable(IUnitOfWork unitOfWork, ProductService productService)
            : base(unitOfWork, productService)
        {
        }
    }

    // Testable subclass for ProductService used in OrderService tests
    internal class ProductServiceForOrderTests : ProductService
    {
        public ProductServiceForOrderTests(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }

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

        private Order CreateSampleOrder(string? id = null)
        {
            return new Order
            {
                Id = id ?? Guid.NewGuid().ToString(),
                UserId = Guid.NewGuid().ToString(),
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
                Title = $"Product {id}",
                Price = 99.99f,
                Rating = 4.0f,
                Brand = "TestBrand",
                Category = "Electronics",
                Thumbnail = new Uri("https://example.com/product.jpg"),
                Quantity = quantity
            };
        }

        private OrderServiceTestable CreateService()
        {
            var productService = new ProductServiceForOrderTests(_mockUnitOfWork.Object);
            return new OrderServiceTestable(_mockUnitOfWork.Object, productService);
        }

        [Fact]
        public void GetAllOrders_WhenOrdersExist_ReturnsAllOrders()
        {
            // Arrange
            var orders = new List<Order>
            {
                CreateSampleOrder(),
                CreateSampleOrder()
            }.AsQueryable();
            _mockOrderRepository.Setup(r => r.GetAll()).Returns(orders);

            var service = CreateService();

            // Act
            var result = service.GetAllOrders();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetAllOrders_WhenNoOrders_ReturnsEmptyList()
        {
            // Arrange
            var orders = new List<Order>().AsQueryable();
            _mockOrderRepository.Setup(r => r.GetAll()).Returns(orders);

            var service = CreateService();

            // Act
            var result = service.GetAllOrders();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetOrderById_WhenOrderExists_ReturnsOrder()
        {
            // Arrange
            var orderId = Guid.NewGuid().ToString();
            var order = CreateSampleOrder(orderId);
            _mockOrderRepository.Setup(r => r.GetById(orderId)).Returns(order);

            var service = CreateService();

            // Act
            var result = service.GetOrderById(orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderId, result.Id);
        }

        [Fact]
        public void GetOrderById_WhenOrderNotExists_ReturnsNull()
        {
            // Arrange
            _mockOrderRepository.Setup(r => r.GetById(It.IsAny<string>())).Returns((Order?)null);

            var service = CreateService();

            // Act
            var result = service.GetOrderById("nonexistent-id");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetPaginationOrders_WithValidFilter_ReturnsPaginatedOrders()
        {
            // Arrange
            var orders = new List<Order>();
            for (int i = 0; i < 15; i++)
            {
                orders.Add(CreateSampleOrder());
            }
            _mockOrderRepository.Setup(r => r.GetAll()).Returns(orders.AsQueryable());

            var service = CreateService();
            var paginationFilter = new PaginationFilter(1, 10);
            var queryFilter = new QueryFilter();

            // Act
            var result = service.GetPaginationOrders(paginationFilter, queryFilter, out int count);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Count);
            Assert.Equal(15, count);
        }

        [Fact]
        public async Task InsertOrderAsync_WhenProductExists_ReturnsSuccess()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var productList = new Dictionary<int, int> { { 1, 2 } };
            var product = CreateSampleProduct(1, 10);

            _mockProductRepository.Setup(r => r.GetById(1)).Returns(product);
            _mockProductRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>())).ReturnsAsync(true);
            _mockOrderRepository.Setup(r => r.InsertAsync(It.IsAny<Order>())).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.StartTransaction());
            _mockUnitOfWork.Setup(u => u.CommitTransaction());

            var service = CreateService();

            // Act
            var result = await service.InsertOrderAsync(userId, productList);

            // Assert
            Assert.Equal(OrderService.OrderResult.Success, result);
        }

        [Fact]
        public async Task InsertOrderAsync_WhenProductNotFound_ReturnsProductNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var productList = new Dictionary<int, int> { { 999, 2 } };

            _mockProductRepository.Setup(r => r.GetById(999)).Returns((Product?)null);
            _mockUnitOfWork.Setup(u => u.StartTransaction());
            _mockUnitOfWork.Setup(u => u.AbortTransaction());

            var service = CreateService();

            // Act
            var result = await service.InsertOrderAsync(userId, productList);

            // Assert
            Assert.Equal(OrderService.OrderResult.ProductNotFound, result);
        }

        [Fact]
        public async Task InsertOrderAsync_WhenQuantityNotEnough_ReturnsProductNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var productList = new Dictionary<int, int> { { 1, 100 } }; // requesting 100 but only 5 available
            var product = CreateSampleProduct(1, 5);

            _mockProductRepository.Setup(r => r.GetById(1)).Returns(product);
            _mockUnitOfWork.Setup(u => u.StartTransaction());
            _mockUnitOfWork.Setup(u => u.AbortTransaction());

            var service = CreateService();

            // Act
            var result = await service.InsertOrderAsync(userId, productList);

            // Assert
            Assert.Equal(OrderService.OrderResult.ProductNotFound, result);
        }

        [Fact]
        public async Task UpdateOrderAsync_WhenSuccessful_ReturnsSuccess()
        {
            // Arrange
            var order = CreateSampleOrder();
            _mockOrderRepository.Setup(r => r.UpdateAsync(order)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var service = CreateService();

            // Act
            var result = await service.UpdateOrderAsync(order);

            // Assert
            Assert.Equal(OrderService.OrderResult.Success, result);
        }

        [Fact]
        public async Task UpdateOrderAsync_WhenFails_ReturnsFail()
        {
            // Arrange
            var order = CreateSampleOrder();
            _mockOrderRepository.Setup(r => r.UpdateAsync(order)).ReturnsAsync(false);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

            var service = CreateService();

            // Act
            var result = await service.UpdateOrderAsync(order);

            // Assert
            Assert.Equal(OrderService.OrderResult.Fail, result);
        }

        [Fact]
        public async Task UpdateOrderPropertyAsync_WithValidProperty_ReturnsSuccess()
        {
            // Arrange
            var order = CreateSampleOrder();
            _mockOrderRepository.Setup(r => r.UpdateAsync(order)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var service = CreateService();

            // Act
            var result = await service.UpdateOrderPropertyAsync(order, "Status", OrderStatus.Successed);

            // Assert
            Assert.Equal(OrderService.OrderResult.Success, result);
        }

        [Fact]
        public async Task UpdateOrderPropertyAsync_WithInvalidProperty_ReturnsInvalidInput()
        {
            // Arrange
            var order = CreateSampleOrder();
            var service = CreateService();

            // Act
            var result = await service.UpdateOrderPropertyAsync(order, "NonExistentProperty", "value");

            // Assert
            Assert.Equal(OrderService.OrderResult.InvalidInput, result);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_WithSameStatus_ThrowsInvalidOperationException()
        {
            // Arrange
            var order = CreateSampleOrder();
            order.Status = OrderStatus.Pending;
            _mockUnitOfWork.Setup(u => u.StartTransaction());
            _mockUnitOfWork.Setup(u => u.AbortTransaction());

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateOrderStatusAsync(order, OrderStatus.Pending)
            );
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_ToSuccessed_ReturnsSuccess()
        {
            // Arrange
            var order = CreateSampleOrder();
            order.Status = OrderStatus.Pending;
            _mockOrderRepository.Setup(r => r.UpdateAsync(order)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.StartTransaction());
            _mockUnitOfWork.Setup(u => u.CommitTransaction());

            var service = CreateService();

            // Act
            var result = await service.UpdateOrderStatusAsync(order, OrderStatus.Successed);

            // Assert
            Assert.Equal(OrderService.OrderResult.Success, result);
            Assert.Equal(OrderStatus.Successed, order.Status);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_ToCanceled_RefillsProducts()
        {
            // Arrange
            var order = CreateSampleOrder();
            order.Status = OrderStatus.Pending;
            order.ProductList = new Dictionary<int, int> { { 1, 2 } };
            var product = CreateSampleProduct(1, 5);

            _mockProductRepository.Setup(r => r.GetById(1)).Returns(product);
            _mockProductRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>())).ReturnsAsync(true);
            _mockOrderRepository.Setup(r => r.UpdateAsync(order)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.StartTransaction());
            _mockUnitOfWork.Setup(u => u.CommitTransaction());

            var service = CreateService();

            // Act
            var result = await service.UpdateOrderStatusAsync(order, OrderStatus.Canceled);

            // Assert
            Assert.Equal(OrderService.OrderResult.Success, result);
            Assert.Equal(OrderStatus.Canceled, order.Status);
        }

        [Fact]
        public async Task DeleteOrderAsync_WhenOrderNotFound_ReturnsProductNotFound()
        {
            // Arrange
            _mockOrderRepository.Setup(r => r.GetById(It.IsAny<string>())).Returns((Order?)null);
            _mockUnitOfWork.Setup(u => u.StartTransaction());
            _mockUnitOfWork.Setup(u => u.AbortTransaction());

            var service = CreateService();

            // Act
            var result = await service.DeleteOrderAsync("nonexistent-id");

            // Assert
            Assert.Equal(OrderService.OrderResult.ProductNotFound, result);
        }

        [Fact]
        public async Task DeleteOrderAsync_WhenOrderIsCanceled_DeletesWithoutRefill()
        {
            // Arrange
            var orderId = Guid.NewGuid().ToString();
            var order = CreateSampleOrder(orderId);
            order.Status = OrderStatus.Canceled;

            _mockOrderRepository.Setup(r => r.GetById(orderId)).Returns(order);
            _mockOrderRepository.Setup(r => r.DeleteAsync(orderId)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.StartTransaction());
            _mockUnitOfWork.Setup(u => u.CommitTransaction());

            var service = CreateService();

            // Act
            var result = await service.DeleteOrderAsync(orderId);

            // Assert
            Assert.Equal(OrderService.OrderResult.Success, result);
        }

        [Fact]
        public async Task DeleteOrderAsync_WhenOrderIsPending_RefillsAndDeletes()
        {
            // Arrange
            var orderId = Guid.NewGuid().ToString();
            var order = CreateSampleOrder(orderId);
            order.Status = OrderStatus.Pending;
            order.ProductList = new Dictionary<int, int> { { 1, 2 } };
            var product = CreateSampleProduct(1, 5);

            _mockOrderRepository.Setup(r => r.GetById(orderId)).Returns(order);
            _mockOrderRepository.Setup(r => r.DeleteAsync(orderId)).ReturnsAsync(true);
            _mockProductRepository.Setup(r => r.GetById(1)).Returns(product);
            _mockProductRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>())).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.StartTransaction());
            _mockUnitOfWork.Setup(u => u.CommitTransaction());

            var service = CreateService();

            // Act
            var result = await service.DeleteOrderAsync(orderId);

            // Assert
            Assert.Equal(OrderService.OrderResult.Success, result);
        }

        [Fact]
        public void Dispose_DoesNotThrow()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.Dispose());
            var service = CreateService();

            // Act & Assert
            var exception = Record.Exception(() => service.Dispose());
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(OrderService.OrderResult.Success)]
        [InlineData(OrderService.OrderResult.Fail)]
        [InlineData(OrderService.OrderResult.QuantityNotEnough)]
        [InlineData(OrderService.OrderResult.ProductNotFound)]
        [InlineData(OrderService.OrderResult.InvalidInput)]
        public void OrderResult_AllValues_AreValid(OrderService.OrderResult result)
        {
            // Assert
            Assert.True(Enum.IsDefined(typeof(OrderService.OrderResult), result));
        }
    }
}

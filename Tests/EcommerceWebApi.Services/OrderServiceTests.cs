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

namespace EcommerceWebApi.Services.Tests
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

        private Order CreateTestOrder(string id = "order-001", string userId = "user-001")
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
                Title = "Test Product",
                Price = 99.99f,
                Brand = "Brand",
                Quantity = quantity
            };
        }

        [Fact]
        public void GetOrderById_WithValidId_ReturnsOrder()
        {
            // Arrange
            var order = CreateTestOrder();
            _mockOrderRepository.Setup(r => r.GetById("order-001")).Returns(order);
            var service = new OrderServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetOrderById("order-001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("order-001", result.Id);
        }

        [Fact]
        public void GetOrderById_WithInvalidId_ReturnsNull()
        {
            // Arrange
            _mockOrderRepository.Setup(r => r.GetById("nonexistent")).Returns((Order?)null);
            var service = new OrderServiceTestable(_mockUnitOfWork.Object);

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
                CreateTestOrder("1", "user1"),
                CreateTestOrder("2", "user2"),
                CreateTestOrder("3", "user3")
            };
            var mockCollection = new MockDocumentCollection<Order>(orders);
            _mockOrderRepository.Setup(r => r.GetAll()).Returns(mockCollection);
            var service = new OrderServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetAllOrders();

            // Assert
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task UpdateOrderAsync_WithValidOrder_ReturnsSuccess()
        {
            // Arrange
            var order = CreateTestOrder();
            _mockOrderRepository.Setup(r => r.UpdateAsync(order)).ReturnsAsync(true);
            var service = new OrderServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.UpdateOrderAsync(order);

            // Assert
            Assert.Equal(OrderResult.Success, result);
        }

        [Fact]
        public async Task UpdateOrderAsync_WhenRepositoryFails_ReturnsFail()
        {
            // Arrange
            var order = CreateTestOrder();
            _mockOrderRepository.Setup(r => r.UpdateAsync(order)).ReturnsAsync(false);
            var service = new OrderServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.UpdateOrderAsync(order);

            // Assert
            Assert.Equal(OrderResult.Fail, result);
        }

        [Fact]
        public async Task UpdateOrderPropertyAsync_WithValidProperty_ReturnsSuccess()
        {
            // Arrange
            var order = CreateTestOrder();
            _mockOrderRepository.Setup(r => r.UpdateAsync(It.IsAny<Order>())).ReturnsAsync(true);
            var service = new OrderServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.UpdateOrderPropertyAsync(order, "UserId", "new-user");

            // Assert
            Assert.Equal(OrderResult.Success, result);
            Assert.Equal("new-user", order.UserId);
        }

        [Fact]
        public async Task UpdateOrderPropertyAsync_WithInvalidProperty_ReturnsInvalidInput()
        {
            // Arrange
            var order = CreateTestOrder();
            var service = new OrderServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.UpdateOrderPropertyAsync(order, "NonExistentProp", "value");

            // Assert
            Assert.Equal(OrderResult.InvalidInput, result);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_WithSameStatus_ThrowsInvalidOperationException()
        {
            // Arrange
            var order = CreateTestOrder();
            order.Status = OrderStatus.Pending;
            _mockUnitOfWork.Setup(u => u.StartTransaction());
            _mockUnitOfWork.Setup(u => u.AbortTransaction());
            var service = new OrderServiceTestable(_mockUnitOfWork.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateOrderStatusAsync(order, OrderStatus.Pending)
            );
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_ToSuccessed_ReturnsSuccess()
        {
            // Arrange
            var order = CreateTestOrder();
            order.Status = OrderStatus.Pending;
            _mockUnitOfWork.Setup(u => u.StartTransaction());
            _mockUnitOfWork.Setup(u => u.CommitTransaction());
            _mockUnitOfWork.Setup(u => u.AbortTransaction());
            _mockOrderRepository.Setup(r => r.UpdateAsync(It.IsAny<Order>())).ReturnsAsync(true);
            var service = new OrderServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.UpdateOrderStatusAsync(order, OrderStatus.Successed);

            // Assert
            Assert.Equal(OrderResult.Success, result);
            Assert.Equal(OrderStatus.Successed, order.Status);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_ToCanceled_RefillsProducts()
        {
            // Arrange
            var order = CreateTestOrder();
            order.Status = OrderStatus.Pending;
            order.ProductList = new Dictionary<int, int> { { 1, 2 } };

            var product = CreateTestProduct(1, 5);
            _mockProductRepository.Setup(r => r.GetById(1)).Returns(product);
            _mockProductRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>())).ReturnsAsync(true);
            _mockOrderRepository.Setup(r => r.UpdateAsync(It.IsAny<Order>())).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.StartTransaction());
            _mockUnitOfWork.Setup(u => u.CommitTransaction());
            _mockUnitOfWork.Setup(u => u.AbortTransaction());
            var service = new OrderServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.UpdateOrderStatusAsync(order, OrderStatus.Canceled);

            // Assert
            Assert.Equal(OrderResult.Success, result);
            Assert.Equal(7, product.Quantity); // 5 + 2 refilled
        }

        [Fact]
        public void GetPaginationOrders_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var orders = Enumerable.Range(1, 15)
                .Select(i => CreateTestOrder($"order-{i}", "user-001"))
                .ToList();
            var mockCollection = new MockDocumentCollection<Order>(orders);
            _mockOrderRepository.Setup(r => r.GetAll()).Returns(mockCollection);
            var service = new OrderServiceTestable(_mockUnitOfWork.Object);

            var paginationFilter = new PaginationFilter(1, 5);
            var queryFilter = new QueryFilter();

            // Act
            var result = service.GetPaginationOrders(paginationFilter, queryFilter, out int count);

            // Assert
            Assert.Equal(15, count);
            Assert.Equal(5, result.Count);
        }

        [Fact]
        public async Task DeleteOrderAsync_WithNonExistentOrder_ReturnsProductNotFound()
        {
            // Arrange
            _mockOrderRepository.Setup(r => r.GetById("nonexistent")).Returns((Order?)null);
            _mockUnitOfWork.Setup(u => u.StartTransaction());
            _mockUnitOfWork.Setup(u => u.AbortTransaction());
            var service = new OrderServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.DeleteOrderAsync("nonexistent");

            // Assert
            Assert.Equal(OrderResult.ProductNotFound, result);
        }

        [Fact]
        public async Task DeleteOrderAsync_WithCanceledOrder_DeletesWithoutRefill()
        {
            // Arrange
            var order = CreateTestOrder();
            order.Status = OrderStatus.Canceled;
            _mockOrderRepository.Setup(r => r.GetById("order-001")).Returns(order);
            _mockOrderRepository.Setup(r => r.DeleteAsync("order-001")).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.StartTransaction());
            _mockUnitOfWork.Setup(u => u.CommitTransaction());
            _mockUnitOfWork.Setup(u => u.AbortTransaction());
            var service = new OrderServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.DeleteOrderAsync("order-001");

            // Assert
            Assert.Equal(OrderResult.Success, result);
        }
    }

    // Testable wrapper for OrderService
    public class OrderServiceTestable : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderServiceTestable(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<Order> GetAllOrders()
        {
            return _unitOfWork.Orders.GetAll().AsQueryable().ToList();
        }

        public List<Order> GetPaginationOrders(PaginationFilter paginationFilter, QueryFilter queryFilter, out int queryOrderCount)
        {
            var orders = GetAllOrders();
            orders = QueryHelper.SearchObjects(orders, queryFilter.SearchBy, queryFilter.Search, StringComparison.OrdinalIgnoreCase).ToList();
            queryOrderCount = orders.Count;
            return orders
                .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                .Take(paginationFilter.PageSize)
                .ToList();
        }

        public Order? GetOrderById(string id) => _unitOfWork.Orders.GetById(id);

        public async Task<OrderResult> InsertOrderAsync(string userId, Dictionary<int, int> productList)
        {
            try
            {
                _unitOfWork.StartTransaction();
                var order = await ConstructOrderAsync(userId, productList);
                if (order == null)
                {
                    _unitOfWork.AbortTransaction();
                    return OrderResult.ProductNotFound;
                }
                var result = await _unitOfWork.Orders.InsertAsync(order);
                if (result)
                {
                    _unitOfWork.CommitTransaction();
                    return OrderResult.Success;
                }
                else
                {
                    _unitOfWork.AbortTransaction();
                    return OrderResult.Fail;
                }
            }
            catch
            {
                _unitOfWork.AbortTransaction();
                throw;
            }
        }

        private async Task<Order?> ConstructOrderAsync(string userId, Dictionary<int, int> productList)
        {
            var products = new Dictionary<int, Product>();
            foreach (var pair in productList)
            {
                var product = _unitOfWork.Products.GetById(pair.Key);
                if (product == null || product.Quantity < pair.Value)
                    return null;
                products.Add(pair.Key, product);
            }

            var order = new Order
            {
                UserId = userId,
                ProductList = productList,
                Created = DateTime.UtcNow,
                Status = OrderStatus.Pending
            };

            foreach (var pair in productList)
            {
                var product = products[pair.Key];
                product.Quantity -= pair.Value;
                await _unitOfWork.Products.UpdateAsync(product);
            }

            return order;
        }

        public async Task<OrderResult> UpdateOrderAsync(Order order)
        {
            var result = await _unitOfWork.Orders.UpdateAsync(order);
            return result ? OrderResult.Success : OrderResult.Fail;
        }

        public async Task<OrderResult> UpdateOrderPropertyAsync<T>(Order order, string property, T value)
        {
            var propertyInfo = typeof(Order).GetProperty(
                property,
                System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
            );
            if (propertyInfo == null) return OrderResult.InvalidInput;
            propertyInfo.SetValue(order, value, null);
            return await UpdateOrderAsync(order);
        }

        public async Task<OrderResult> UpdateOrderStatusAsync(Order order, OrderStatus status)
        {
            try
            {
                _unitOfWork.StartTransaction();
                if (order.Status == status)
                {
                    _unitOfWork.AbortTransaction();
                    throw new InvalidOperationException($"Current status already is {status}");
                }
                order.Status = status;
                order.Updated = DateTime.UtcNow;
                if (status == OrderStatus.Canceled)
                {
                    var fillResult = await RefillProductAsync(order);
                    if (fillResult != OrderResult.Success)
                    {
                        _unitOfWork.AbortTransaction();
                        return fillResult;
                    }
                }
                var result = await UpdateOrderAsync(order);
                if (result == OrderResult.Success)
                {
                    _unitOfWork.CommitTransaction();
                    return OrderResult.Success;
                }
                else
                {
                    _unitOfWork.AbortTransaction();
                    return OrderResult.Fail;
                }
            }
            catch
            {
                _unitOfWork.AbortTransaction();
                throw;
            }
        }

        private async Task<OrderResult> RefillProductAsync(Order order)
        {
            foreach (var pair in order.ProductList)
            {
                var product = _unitOfWork.Products.GetById(pair.Key);
                if (product == null) return OrderResult.ProductNotFound;
                product.Quantity += pair.Value;
                await _unitOfWork.Products.UpdateAsync(product);
            }
            return OrderResult.Success;
        }

        public async Task<OrderResult> DeleteOrderAsync(string id)
        {
            try
            {
                _unitOfWork.StartTransaction();
                var order = GetOrderById(id);
                if (order == null) return OrderResult.ProductNotFound;

                if (order.Status != OrderStatus.Canceled)
                {
                    var fillResult = await RefillProductAsync(order);
                    if (fillResult != OrderResult.Success)
                    {
                        _unitOfWork.AbortTransaction();
                        return fillResult;
                    }
                }

                var deleteResult = await _unitOfWork.Orders.DeleteAsync(id);
                if (deleteResult)
                {
                    _unitOfWork.CommitTransaction();
                    return OrderResult.Success;
                }
                else
                {
                    _unitOfWork.AbortTransaction();
                    return OrderResult.Fail;
                }
            }
            catch
            {
                _unitOfWork.AbortTransaction();
                throw;
            }
        }

        public void Dispose() => _unitOfWork.Dispose();
    }
}

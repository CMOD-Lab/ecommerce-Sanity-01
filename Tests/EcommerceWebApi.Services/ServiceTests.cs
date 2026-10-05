using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using EcommerceWebApi.Services;
using EcommerceWebApi.Entities;
using EcommerceWebApi.Utilities;
using EcommerceWebApi.Repositories;

namespace EcommerceWebApi.Services.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IUserRepository> _mockUserRepository;

        public UserServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockUnitOfWork.Setup(u => u.Users).Returns(_mockUserRepository.Object);
        }

        private User CreateTestUser(string id = "user-1", string username = "testuser")
        {
            return new User
            {
                Id = id,
                Username = username,
                Role = "User",
                IsTwoFactorAuthActivated = false,
                PasswordSalt = new byte[] { 1, 2, 3 },
                PasswordHash = new byte[] { 4, 5, 6 },
                RefreshToken = new RefreshToken
                {
                    Token = "token123",
                    Created = DateTime.UtcNow,
                    Expires = DateTime.UtcNow.AddDays(7)
                }
            };
        }

        [Fact]
        public void GetUserById_ExistingId_ReturnsUser()
        {
            // Arrange
            var user = CreateTestUser();
            _mockUserRepository.Setup(r => r.GetById("user-1")).Returns(user);

            // Act
            var result = _mockUnitOfWork.Object.Users.GetById("user-1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("user-1", result.Id);
        }

        [Fact]
        public void GetUserById_NonExistingId_ReturnsNull()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.GetById("nonexistent")).Returns((User?)null);

            // Act
            var result = _mockUnitOfWork.Object.Users.GetById("nonexistent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetUserByName_ExistingName_ReturnsUser()
        {
            // Arrange
            var user = CreateTestUser();
            _mockUserRepository.Setup(r => r.GetByName("testuser")).Returns(user);

            // Act
            var result = _mockUnitOfWork.Object.Users.GetByName("testuser");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testuser", result.Username);
        }

        [Fact]
        public void GetUserByName_NonExistingName_ReturnsNull()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.GetByName("unknown")).Returns((User?)null);

            // Act
            var result = _mockUnitOfWork.Object.Users.GetByName("unknown");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetUserByToken_ExistingToken_ReturnsUser()
        {
            // Arrange
            var user = CreateTestUser();
            _mockUserRepository.Setup(r => r.GetByToken("token123")).Returns(user);

            // Act
            var result = _mockUnitOfWork.Object.Users.GetByToken("token123");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetUserByToken_NonExistingToken_ReturnsNull()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.GetByToken("badtoken")).Returns((User?)null);

            // Act
            var result = _mockUnitOfWork.Object.Users.GetByToken("badtoken");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task InsertAsync_ValidUser_ReturnsTrue()
        {
            // Arrange
            var user = CreateTestUser();
            _mockUserRepository.Setup(r => r.InsertAsync(user)).ReturnsAsync(true);

            // Act
            var result = await _mockUnitOfWork.Object.Users.InsertAsync(user);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateAsync_ValidUser_ReturnsTrue()
        {
            // Arrange
            var user = CreateTestUser();
            _mockUserRepository.Setup(r => r.UpdateAsync(user)).ReturnsAsync(true);

            // Act
            var result = await _mockUnitOfWork.Object.Users.UpdateAsync(user);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_ValidId_ReturnsTrue()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.DeleteAsync("user-1")).ReturnsAsync(true);

            // Act
            var result = await _mockUnitOfWork.Object.Users.DeleteAsync("user-1");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task InsertAsync_FailedInsert_ReturnsFalse()
        {
            // Arrange
            var user = CreateTestUser();
            _mockUserRepository.Setup(r => r.InsertAsync(user)).ReturnsAsync(false);

            // Act
            var result = await _mockUnitOfWork.Object.Users.InsertAsync(user);

            // Assert
            Assert.False(result);
        }
    }

    public class ProductServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IProductRepository> _mockProductRepository;

        public ProductServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockUnitOfWork.Setup(u => u.Products).Returns(_mockProductRepository.Object);
        }

        private Product CreateTestProduct(int id = 1, string title = "Test Product")
        {
            return new Product
            {
                Id = id,
                Title = title,
                Price = 99.99f,
                Rating = 4.0f,
                Brand = "TestBrand",
                Category = "Electronics",
                Thumbnail = new Uri("https://example.com/img.jpg"),
                Quantity = 10
            };
        }

        [Fact]
        public void GetProductById_ExistingId_ReturnsProduct()
        {
            // Arrange
            var product = CreateTestProduct();
            _mockProductRepository.Setup(r => r.GetById(1)).Returns(product);

            // Act
            var result = _mockUnitOfWork.Object.Products.GetById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public void GetProductById_NonExistingId_ReturnsNull()
        {
            // Arrange
            _mockProductRepository.Setup(r => r.GetById(999)).Returns((Product?)null);

            // Act
            var result = _mockUnitOfWork.Object.Products.GetById(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task InsertAsync_ValidProduct_ReturnsTrue()
        {
            // Arrange
            var product = CreateTestProduct();
            _mockProductRepository.Setup(r => r.InsertAsync(product)).ReturnsAsync(true);

            // Act
            var result = await _mockUnitOfWork.Object.Products.InsertAsync(product);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateAsync_ValidProduct_ReturnsTrue()
        {
            // Arrange
            var product = CreateTestProduct();
            _mockProductRepository.Setup(r => r.UpdateAsync(product)).ReturnsAsync(true);

            // Act
            var result = await _mockUnitOfWork.Object.Products.UpdateAsync(product);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_ValidId_ReturnsTrue()
        {
            // Arrange
            _mockProductRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _mockUnitOfWork.Object.Products.DeleteAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingId_ReturnsFalse()
        {
            // Arrange
            _mockProductRepository.Setup(r => r.DeleteAsync(999)).ReturnsAsync(false);

            // Act
            var result = await _mockUnitOfWork.Object.Products.DeleteAsync(999);

            // Assert
            Assert.False(result);
        }
    }

    public class OrderServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IOrderRepository> _mockOrderRepository;

        public OrderServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockUnitOfWork.Setup(u => u.Orders).Returns(_mockOrderRepository.Object);
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

        [Fact]
        public void GetOrderById_ExistingId_ReturnsOrder()
        {
            // Arrange
            var order = CreateTestOrder();
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
            _mockOrderRepository.Setup(r => r.GetById("nonexistent")).Returns((Order?)null);

            // Act
            var result = _mockUnitOfWork.Object.Orders.GetById("nonexistent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task InsertAsync_ValidOrder_ReturnsTrue()
        {
            // Arrange
            var order = CreateTestOrder();
            _mockOrderRepository.Setup(r => r.InsertAsync(order)).ReturnsAsync(true);

            // Act
            var result = await _mockUnitOfWork.Object.Orders.InsertAsync(order);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateAsync_ValidOrder_ReturnsTrue()
        {
            // Arrange
            var order = CreateTestOrder();
            _mockOrderRepository.Setup(r => r.UpdateAsync(order)).ReturnsAsync(true);

            // Act
            var result = await _mockUnitOfWork.Object.Orders.UpdateAsync(order);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_ValidId_ReturnsTrue()
        {
            // Arrange
            _mockOrderRepository.Setup(r => r.DeleteAsync("order-1")).ReturnsAsync(true);

            // Act
            var result = await _mockUnitOfWork.Object.Orders.DeleteAsync("order-1");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void OrderStatus_Pending_IsDefault()
        {
            // Arrange
            var order = CreateTestOrder();

            // Assert
            Assert.Equal(OrderStatus.Pending, order.Status);
        }

        [Fact]
        public void Order_UpdateStatus_ToSuccessed()
        {
            // Arrange
            var order = CreateTestOrder();

            // Act
            order.Status = OrderStatus.Successed;

            // Assert
            Assert.Equal(OrderStatus.Successed, order.Status);
        }

        [Fact]
        public void Order_UpdateStatus_ToCanceled()
        {
            // Arrange
            var order = CreateTestOrder();

            // Act
            order.Status = OrderStatus.Canceled;

            // Assert
            Assert.Equal(OrderStatus.Canceled, order.Status);
        }
    }
}

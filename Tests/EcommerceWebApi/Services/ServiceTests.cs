using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using EcommerceWebApi.Entities;
using EcommerceWebApi.Services;
using EcommerceWebApi.Utilities;
using EcommerceWebApi.Repositories;
using JsonFlatFileDataStore;

namespace EcommerceWebApi.Tests.Services
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

        private User CreateSampleUser(string id = "user-1", string username = "testuser")
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
        public void GetUserById_WithValidId_ReturnsUser()
        {
            // Arrange
            var user = CreateSampleUser();
            _mockUserRepository.Setup(r => r.GetById("user-1")).Returns(user);

            // Act
            var result = _mockUnitOfWork.Object.Users.GetById("user-1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("user-1", result.Id);
        }

        [Fact]
        public void GetUserById_WithInvalidId_ReturnsNull()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.GetById("invalid")).Returns((User?)null);

            // Act
            var result = _mockUnitOfWork.Object.Users.GetById("invalid");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetUserByName_WithValidName_ReturnsUser()
        {
            // Arrange
            var user = CreateSampleUser();
            _mockUserRepository.Setup(r => r.GetByName("testuser")).Returns(user);

            // Act
            var result = _mockUnitOfWork.Object.Users.GetByName("testuser");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testuser", result.Username);
        }

        [Fact]
        public void GetUserByName_WithInvalidName_ReturnsNull()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.GetByName("nonexistent")).Returns((User?)null);

            // Act
            var result = _mockUnitOfWork.Object.Users.GetByName("nonexistent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetUserByToken_WithValidToken_ReturnsUser()
        {
            // Arrange
            var user = CreateSampleUser();
            _mockUserRepository.Setup(r => r.GetByToken("token123")).Returns(user);

            // Act
            var result = _mockUnitOfWork.Object.Users.GetByToken("token123");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetUserByToken_WithInvalidToken_ReturnsNull()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.GetByToken("badtoken")).Returns((User?)null);

            // Act
            var result = _mockUnitOfWork.Object.Users.GetByToken("badtoken");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task InsertAsync_WithValidUser_ReturnsTrue()
        {
            // Arrange
            var user = CreateSampleUser();
            _mockUserRepository.Setup(r => r.InsertAsync(user)).ReturnsAsync(true);

            // Act
            var result = await _mockUnitOfWork.Object.Users.InsertAsync(user);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task InsertAsync_WhenFails_ReturnsFalse()
        {
            // Arrange
            var user = CreateSampleUser();
            _mockUserRepository.Setup(r => r.InsertAsync(user)).ReturnsAsync(false);

            // Act
            var result = await _mockUnitOfWork.Object.Users.InsertAsync(user);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateAsync_WithValidUser_ReturnsTrue()
        {
            // Arrange
            var user = CreateSampleUser();
            _mockUserRepository.Setup(r => r.UpdateAsync(user)).ReturnsAsync(true);

            // Act
            var result = await _mockUnitOfWork.Object.Users.UpdateAsync(user);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_WithValidId_ReturnsTrue()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.DeleteAsync("user-1")).ReturnsAsync(true);

            // Act
            var result = await _mockUnitOfWork.Object.Users.DeleteAsync("user-1");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_WithInvalidId_ReturnsFalse()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.DeleteAsync("invalid")).ReturnsAsync(false);

            // Act
            var result = await _mockUnitOfWork.Object.Users.DeleteAsync("invalid");

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

        private Product CreateSampleProduct(int id = 1, string title = "Test Product")
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
                Quantity = 50
            };
        }

        [Fact]
        public void GetProductById_WithValidId_ReturnsProduct()
        {
            // Arrange
            var product = CreateSampleProduct();
            _mockProductRepository.Setup(r => r.GetById(1)).Returns(product);

            // Act
            var result = _mockUnitOfWork.Object.Products.GetById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public void GetProductById_WithInvalidId_ReturnsNull()
        {
            // Arrange
            _mockProductRepository.Setup(r => r.GetById(999)).Returns((Product?)null);

            // Act
            var result = _mockUnitOfWork.Object.Products.GetById(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task InsertAsync_WithValidProduct_ReturnsTrue()
        {
            // Arrange
            var product = CreateSampleProduct();
            _mockProductRepository.Setup(r => r.InsertAsync(product)).ReturnsAsync(true);

            // Act
            var result = await _mockUnitOfWork.Object.Products.InsertAsync(product);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateAsync_WithValidProduct_ReturnsTrue()
        {
            // Arrange
            var product = CreateSampleProduct();
            _mockProductRepository.Setup(r => r.UpdateAsync(product)).ReturnsAsync(true);

            // Act
            var result = await _mockUnitOfWork.Object.Products.UpdateAsync(product);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_WithValidId_ReturnsTrue()
        {
            // Arrange
            _mockProductRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _mockUnitOfWork.Object.Products.DeleteAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_WithInvalidId_ReturnsFalse()
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

        [Fact]
        public void GetOrderById_WithValidId_ReturnsOrder()
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
        public void GetOrderById_WithInvalidId_ReturnsNull()
        {
            // Arrange
            _mockOrderRepository.Setup(r => r.GetById("invalid")).Returns((Order?)null);

            // Act
            var result = _mockUnitOfWork.Object.Orders.GetById("invalid");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task InsertAsync_WithValidOrder_ReturnsTrue()
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
        public async Task UpdateAsync_WithValidOrder_ReturnsTrue()
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
        public async Task DeleteAsync_WithValidId_ReturnsTrue()
        {
            // Arrange
            _mockOrderRepository.Setup(r => r.DeleteAsync("order-1")).ReturnsAsync(true);

            // Act
            var result = await _mockUnitOfWork.Object.Orders.DeleteAsync("order-1");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_WithInvalidId_ReturnsFalse()
        {
            // Arrange
            _mockOrderRepository.Setup(r => r.DeleteAsync("invalid")).ReturnsAsync(false);

            // Act
            var result = await _mockUnitOfWork.Object.Orders.DeleteAsync("invalid");

            // Assert
            Assert.False(result);
        }
    }
}

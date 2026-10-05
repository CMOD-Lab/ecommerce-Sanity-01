using EcommerceWebApi.Entities;
using EcommerceWebApi.Repositories;
using EcommerceWebApi.Services;
using EcommerceWebApi.Tests.Helpers;
using EcommerceWebApi.Utilities;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace EcommerceWebApi.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IUserRepository> _mockUserRepository;

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
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
                    Token = "token-123",
                    Created = DateTime.UtcNow,
                    Expires = DateTime.UtcNow.AddDays(7)
                }
            };
        }

        [Fact]
        public void GetUserById_ExistingId_ReturnsUser()
        {
            // Arrange
            var user = CreateTestUser("user-1");
            _mockUserRepository.Setup(r => r.GetById("user-1")).Returns(user);
            var service = CreateUserService();

            // Act
            var result = service.GetUserById("user-1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("user-1", result!.Id);
        }

        [Fact]
        public void GetUserById_NonExistingId_ReturnsNull()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.GetById("nonexistent")).Returns((User?)null);
            var service = CreateUserService();

            // Act
            var result = service.GetUserById("nonexistent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetUserByName_ExistingName_ReturnsUser()
        {
            // Arrange
            var user = CreateTestUser(username: "john");
            _mockUserRepository.Setup(r => r.GetByName("john")).Returns(user);
            var service = CreateUserService();

            // Act
            var result = service.GetUserByName("john");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("john", result!.Username);
        }

        [Fact]
        public void GetUserByName_NonExistingName_ReturnsNull()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.GetByName("unknown")).Returns((User?)null);
            var service = CreateUserService();

            // Act
            var result = service.GetUserByName("unknown");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetUserByToken_ExistingToken_ReturnsUser()
        {
            // Arrange
            var user = CreateTestUser();
            _mockUserRepository.Setup(r => r.GetByToken("token-123")).Returns(user);
            var service = CreateUserService();

            // Act
            var result = service.GetUserByToken("token-123");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetUserByToken_NonExistingToken_ReturnsNull()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.GetByToken("bad-token")).Returns((User?)null);
            var service = CreateUserService();

            // Act
            var result = service.GetUserByToken("bad-token");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task InsertUserAsync_Success_ReturnsTrue()
        {
            // Arrange
            var user = CreateTestUser();
            _mockUserRepository.Setup(r => r.InsertAsync(user)).ReturnsAsync(true);
            var service = CreateUserService();

            // Act
            var result = await service.InsertUserAsync(user);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task InsertUserAsync_Failure_ReturnsFalse()
        {
            // Arrange
            var user = CreateTestUser();
            _mockUserRepository.Setup(r => r.InsertAsync(user)).ReturnsAsync(false);
            var service = CreateUserService();

            // Act
            var result = await service.InsertUserAsync(user);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateUserAsync_Success_ReturnsTrue()
        {
            // Arrange
            var user = CreateTestUser();
            _mockUserRepository.Setup(r => r.UpdateAsync(user)).ReturnsAsync(true);
            var service = CreateUserService();

            // Act
            var result = await service.UpdateUserAsync(user);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateUserAsync_Failure_ReturnsFalse()
        {
            // Arrange
            var user = CreateTestUser();
            _mockUserRepository.Setup(r => r.UpdateAsync(user)).ReturnsAsync(false);
            var service = CreateUserService();

            // Act
            var result = await service.UpdateUserAsync(user);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteUserAsync_Success_ReturnsTrue()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.DeleteAsync("user-1")).ReturnsAsync(true);
            var service = CreateUserService();

            // Act
            var result = await service.DeleteUserAsync("user-1");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteUserAsync_Failure_ReturnsFalse()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.DeleteAsync("user-1")).ReturnsAsync(false);
            var service = CreateUserService();

            // Act
            var result = await service.DeleteUserAsync("user-1");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateUserTokenAsync_WithRefreshToken_UpdatesToken()
        {
            // Arrange
            var user = CreateTestUser();
            var newToken = new RefreshToken
            {
                Token = "new-token",
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(true);
            var service = CreateUserService();

            // Act
            var result = await service.UpdateUserTokenAsync(user, newToken);

            // Assert
            Assert.True(result);
            Assert.Equal("new-token", user.RefreshToken.Token);
        }

        [Fact]
        public async Task UpdateUserTokenAsync_NullRefreshToken_ClearsToken()
        {
            // Arrange
            var user = CreateTestUser();
            _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(true);
            var service = CreateUserService();

            // Act
            var result = await service.UpdateUserTokenAsync(user, null);

            // Assert
            Assert.True(result);
            Assert.Null(user.RefreshToken.Token);
        }

        [Fact]
        public async Task UpdateUserPropertyAsync_ValidProperty_ReturnsTrue()
        {
            // Arrange
            var user = CreateTestUser();
            _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(true);
            var service = CreateUserService();

            // Act
            var result = await service.UpdateUserPropertyAsync(user, "Username", "newname");

            // Assert
            Assert.True(result);
            Assert.Equal("newname", user.Username);
        }

        [Fact]
        public async Task UpdateUserPropertyAsync_InvalidProperty_ReturnsFalse()
        {
            // Arrange
            var user = CreateTestUser();
            var service = CreateUserService();

            // Act
            var result = await service.UpdateUserPropertyAsync(user, "NonExistentProperty", "value");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetPaginationUsers_FirstPage_ReturnsCorrectCount()
        {
            // Arrange
            var users = new List<User>
            {
                CreateTestUser("1", "user1"),
                CreateTestUser("2", "user2"),
                CreateTestUser("3", "user3"),
                CreateTestUser("4", "user4"),
                CreateTestUser("5", "user5")
            };

            var mockCollection = new MockDocumentCollection<User>(users);
            _mockUserRepository.Setup(r => r.GetAll()).Returns(mockCollection);
            var service = CreateUserService();
            var filter = new PaginationFilter(1, 2);

            // Act
            var result = service.GetPaginationUsers(filter);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetPaginationUsers_SecondPage_ReturnsCorrectItems()
        {
            // Arrange
            var users = new List<User>
            {
                CreateTestUser("1", "user1"),
                CreateTestUser("2", "user2"),
                CreateTestUser("3", "user3"),
                CreateTestUser("4", "user4"),
                CreateTestUser("5", "user5")
            };

            var mockCollection = new MockDocumentCollection<User>(users);
            _mockUserRepository.Setup(r => r.GetAll()).Returns(mockCollection);
            var service = CreateUserService();
            var filter = new PaginationFilter(2, 2);

            // Act
            var result = service.GetPaginationUsers(filter);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("user3", result[0].Username);
        }

        [Fact]
        public void GetAllUsers_ReturnsAllUsers()
        {
            // Arrange
            var users = new List<User>
            {
                CreateTestUser("1", "user1"),
                CreateTestUser("2", "user2")
            };
            var mockCollection = new MockDocumentCollection<User>(users);
            _mockUserRepository.Setup(r => r.GetAll()).Returns(mockCollection);
            var service = CreateUserService();

            // Act
            var result = service.GetAllUsers();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void Dispose_DoesNotThrow()
        {
            // Arrange
            var service = CreateUserService();

            // Act & Assert (no exception)
            service.Dispose();
        }

        private UserService CreateUserService()
        {
            return new TestableUserService(_mockUnitOfWork.Object);
        }
    }

    /// <summary>
    /// Testable UserService that accepts IUnitOfWork mock
    /// </summary>
    internal class TestableUserService : UserService
    {
        public TestableUserService(IUnitOfWork unitOfWork) : base(null!)
        {
            typeof(UserService)
                .GetField("_unitOfWork", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(this, unitOfWork);
        }
    }
}

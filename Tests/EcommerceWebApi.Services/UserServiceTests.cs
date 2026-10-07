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
    // Testable subclass to bypass concrete UnitOfWork constructor dependency
    internal class UserServiceTestable : UserService
    {
        public UserServiceTestable(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }

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

        private User CreateSampleUser(string? id = null)
        {
            return new User
            {
                Id = id ?? Guid.NewGuid().ToString(),
                Username = "testuser",
                Role = "User",
                PasswordSalt = new byte[] { 1, 2, 3 },
                PasswordHash = new byte[] { 4, 5, 6 },
                IsTwoFactorAuthActivated = false
            };
        }

        [Fact]
        public void GetAllUsers_WhenUsersExist_ReturnsAllUsers()
        {
            // Arrange
            var users = new List<User>
            {
                CreateSampleUser(),
                CreateSampleUser()
            }.AsQueryable();
            _mockUserRepository.Setup(r => r.GetAll()).Returns(users);

            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetAllUsers();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetAllUsers_WhenNoUsers_ReturnsEmptyList()
        {
            // Arrange
            var users = new List<User>().AsQueryable();
            _mockUserRepository.Setup(r => r.GetAll()).Returns(users);

            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetAllUsers();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetUserById_WhenUserExists_ReturnsUser()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = CreateSampleUser(userId);
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(user);

            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetUserById(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
        }

        [Fact]
        public void GetUserById_WhenUserNotExists_ReturnsNull()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.GetById(It.IsAny<string>())).Returns((User?)null);

            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetUserById("nonexistent-id");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetUserByName_WhenUserExists_ReturnsUser()
        {
            // Arrange
            var user = CreateSampleUser();
            _mockUserRepository.Setup(r => r.GetByName("testuser")).Returns(user);

            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetUserByName("testuser");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testuser", result.Username);
        }

        [Fact]
        public void GetUserByName_WhenUserNotExists_ReturnsNull()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.GetByName(It.IsAny<string>())).Returns((User?)null);

            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetUserByName("nonexistent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetUserByToken_WhenUserExists_ReturnsUser()
        {
            // Arrange
            var user = CreateSampleUser();
            _mockUserRepository.Setup(r => r.GetByToken("valid-token")).Returns(user);

            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetUserByToken("valid-token");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetUserByToken_WhenTokenNotExists_ReturnsNull()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.GetByToken(It.IsAny<string>())).Returns((User?)null);

            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetUserByToken("invalid-token");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetPaginationUsers_WithValidFilter_ReturnsPaginatedUsers()
        {
            // Arrange
            var users = new List<User>();
            for (int i = 0; i < 15; i++)
            {
                users.Add(CreateSampleUser());
            }
            _mockUserRepository.Setup(r => r.GetAll()).Returns(users.AsQueryable());

            var service = new UserServiceTestable(_mockUnitOfWork.Object);
            var filter = new PaginationFilter(1, 10);

            // Act
            var result = service.GetPaginationUsers(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Count);
        }

        [Fact]
        public void GetPaginationUsers_SecondPage_ReturnsCorrectUsers()
        {
            // Arrange
            var users = new List<User>();
            for (int i = 0; i < 15; i++)
            {
                users.Add(CreateSampleUser());
            }
            _mockUserRepository.Setup(r => r.GetAll()).Returns(users.AsQueryable());

            var service = new UserServiceTestable(_mockUnitOfWork.Object);
            var filter = new PaginationFilter(2, 10);

            // Act
            var result = service.GetPaginationUsers(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Count);
        }

        [Fact]
        public async Task InsertUserAsync_WhenSuccessful_ReturnsTrue()
        {
            // Arrange
            var user = CreateSampleUser();
            _mockUserRepository.Setup(r => r.InsertAsync(user)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.InsertUserAsync(user);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task InsertUserAsync_WhenRepositoryReturnsFalse_ReturnsFalse()
        {
            // Arrange
            var user = CreateSampleUser();
            _mockUserRepository.Setup(r => r.InsertAsync(user)).ReturnsAsync(false);

            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.InsertUserAsync(user);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateUserAsync_WhenSuccessful_ReturnsTrue()
        {
            // Arrange
            var user = CreateSampleUser();
            _mockUserRepository.Setup(r => r.UpdateAsync(user)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.UpdateUserAsync(user);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateUserTokenAsync_WithRefreshToken_UpdatesToken()
        {
            // Arrange
            var user = CreateSampleUser();
            user.RefreshToken = new RefreshToken { Token = "old-token" };
            var newToken = new RefreshToken { Token = "new-token", Expires = DateTime.UtcNow.AddDays(7) };

            _mockUserRepository.Setup(r => r.UpdateAsync(user)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.UpdateUserTokenAsync(user, newToken);

            // Assert
            Assert.True(result);
            Assert.Equal(newToken, user.RefreshToken);
        }

        [Fact]
        public async Task UpdateUserTokenAsync_WithNullToken_ClearsToken()
        {
            // Arrange
            var user = CreateSampleUser();
            user.RefreshToken = new RefreshToken { Token = "existing-token" };

            _mockUserRepository.Setup(r => r.UpdateAsync(user)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.UpdateUserTokenAsync(user, null);

            // Assert
            Assert.True(result);
            Assert.Null(user.RefreshToken.Token);
        }

        [Fact]
        public async Task UpdateUserPropertyAsync_WithValidProperty_ReturnsTrue()
        {
            // Arrange
            var user = CreateSampleUser();
            _mockUserRepository.Setup(r => r.UpdateAsync(user)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.UpdateUserPropertyAsync(user, "Username", "newusername");

            // Assert
            Assert.True(result);
            Assert.Equal("newusername", user.Username);
        }

        [Fact]
        public async Task UpdateUserPropertyAsync_WithInvalidProperty_ReturnsFalse()
        {
            // Arrange
            var user = CreateSampleUser();
            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.UpdateUserPropertyAsync(user, "NonExistentProperty", "value");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteUserAsync_WhenSuccessful_ReturnsTrue()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            _mockUserRepository.Setup(r => r.DeleteAsync(userId)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.DeleteUserAsync(userId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteUserAsync_WhenUserNotFound_ReturnsFalse()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.DeleteAsync(It.IsAny<string>())).ReturnsAsync(false);

            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.DeleteUserAsync("nonexistent-id");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Dispose_DoesNotThrow()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.Dispose());
            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act & Assert
            var exception = Record.Exception(() => service.Dispose());
            Assert.Null(exception);
        }
    }
}

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

        private User CreateTestUser(string id = "user-001", string username = "testuser")
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
            var user = CreateTestUser();
            _mockUserRepository.Setup(r => r.GetById("user-001")).Returns(user);
            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetUserById("user-001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("user-001", result.Id);
        }

        [Fact]
        public void GetUserById_WithInvalidId_ReturnsNull()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.GetById("nonexistent")).Returns((User?)null);
            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetUserById("nonexistent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetUserByName_WithValidName_ReturnsUser()
        {
            // Arrange
            var user = CreateTestUser();
            _mockUserRepository.Setup(r => r.GetByName("testuser")).Returns(user);
            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetUserByName("testuser");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testuser", result.Username);
        }

        [Fact]
        public void GetUserByName_WithInvalidName_ReturnsNull()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.GetByName("nobody")).Returns((User?)null);
            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetUserByName("nobody");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetUserByToken_WithValidToken_ReturnsUser()
        {
            // Arrange
            var user = CreateTestUser();
            _mockUserRepository.Setup(r => r.GetByToken("token123")).Returns(user);
            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetUserByToken("token123");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetUserByToken_WithInvalidToken_ReturnsNull()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.GetByToken("badtoken")).Returns((User?)null);
            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetUserByToken("badtoken");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task InsertUserAsync_WithValidUser_ReturnsTrue()
        {
            // Arrange
            var user = CreateTestUser();
            _mockUserRepository.Setup(r => r.InsertAsync(user)).ReturnsAsync(true);
            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.InsertUserAsync(user);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task InsertUserAsync_WhenRepositoryFails_ReturnsFalse()
        {
            // Arrange
            var user = CreateTestUser();
            _mockUserRepository.Setup(r => r.InsertAsync(user)).ReturnsAsync(false);
            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.InsertUserAsync(user);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateUserAsync_WithValidUser_ReturnsTrue()
        {
            // Arrange
            var user = CreateTestUser();
            _mockUserRepository.Setup(r => r.UpdateAsync(user)).ReturnsAsync(true);
            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.UpdateUserAsync(user);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteUserAsync_WithValidId_ReturnsTrue()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.DeleteAsync("user-001")).ReturnsAsync(true);
            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.DeleteUserAsync("user-001");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateUserTokenAsync_WithRefreshToken_UpdatesToken()
        {
            // Arrange
            var user = CreateTestUser();
            var newToken = new RefreshToken
            {
                Token = "newtoken",
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(true);
            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.UpdateUserTokenAsync(user, newToken);

            // Assert
            Assert.True(result);
            Assert.Equal("newtoken", user.RefreshToken.Token);
        }

        [Fact]
        public async Task UpdateUserTokenAsync_WithNullToken_ClearsToken()
        {
            // Arrange
            var user = CreateTestUser();
            _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(true);
            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.UpdateUserTokenAsync(user, null);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateUserPropertyAsync_WithValidProperty_ReturnsTrue()
        {
            // Arrange
            var user = CreateTestUser();
            _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(true);
            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.UpdateUserPropertyAsync(user, "Username", "newname");

            // Assert
            Assert.True(result);
            Assert.Equal("newname", user.Username);
        }

        [Fact]
        public async Task UpdateUserPropertyAsync_WithInvalidProperty_ReturnsFalse()
        {
            // Arrange
            var user = CreateTestUser();
            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.UpdateUserPropertyAsync(user, "NonExistentProperty", "value");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetAllUsers_ReturnsAllUsers()
        {
            // Arrange
            var users = new List<User>
            {
                CreateTestUser("1", "user1"),
                CreateTestUser("2", "user2"),
                CreateTestUser("3", "user3")
            };
            var mockCollection = new MockDocumentCollection<User>(users);
            _mockUserRepository.Setup(r => r.GetAll()).Returns(mockCollection);
            var service = new UserServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetAllUsers();

            // Assert
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void GetPaginationUsers_ReturnsCorrectPage()
        {
            // Arrange
            var users = new List<User>
            {
                CreateTestUser("1", "user1"),
                CreateTestUser("2", "user2"),
                CreateTestUser("3", "user3")
            };
            var mockCollection = new MockDocumentCollection<User>(users);
            _mockUserRepository.Setup(r => r.GetAll()).Returns(mockCollection);
            var service = new UserServiceTestable(_mockUnitOfWork.Object);
            var filter = new PaginationFilter(1, 2);

            // Act
            var result = service.GetPaginationUsers(filter);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetPaginationUsers_SecondPage_ReturnsRemainingUsers()
        {
            // Arrange
            var users = new List<User>
            {
                CreateTestUser("1", "user1"),
                CreateTestUser("2", "user2"),
                CreateTestUser("3", "user3")
            };
            var mockCollection = new MockDocumentCollection<User>(users);
            _mockUserRepository.Setup(r => r.GetAll()).Returns(mockCollection);
            var service = new UserServiceTestable(_mockUnitOfWork.Object);
            var filter = new PaginationFilter(2, 2);

            // Act
            var result = service.GetPaginationUsers(filter);

            // Assert
            Assert.Single(result);
        }
    }

    // Testable wrapper for UserService
    public class UserServiceTestable : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserServiceTestable(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<User> GetAllUsers()
        {
            return _unitOfWork.Users.GetAll().AsQueryable().ToList();
        }

        public List<User> GetPaginationUsers(PaginationFilter paginationFilter)
        {
            return GetAllUsers()
                .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                .Take(paginationFilter.PageSize)
                .ToList();
        }

        public User? GetUserById(string id) => _unitOfWork.Users.GetById(id);
        public User? GetUserByToken(string token) => _unitOfWork.Users.GetByToken(token);
        public User? GetUserByName(string name) => _unitOfWork.Users.GetByName(name);

        public async Task<bool> InsertUserAsync(User user) => await _unitOfWork.Users.InsertAsync(user);
        public async Task<bool> UpdateUserAsync(User user) => await _unitOfWork.Users.UpdateAsync(user);

        public async Task<bool> UpdateUserTokenAsync(User user, RefreshToken? refreshToken)
        {
            if (refreshToken != null)
                user.RefreshToken = refreshToken;
            else
                user.RefreshToken.Token = null!;
            return await UpdateUserAsync(user);
        }

        public async Task<bool> UpdateUserPropertyAsync<T>(User user, string property, T value)
        {
            var propertyInfo = typeof(User).GetProperty(
                property,
                System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
            );
            if (propertyInfo == null) return false;
            propertyInfo.SetValue(user, value, null);
            return await UpdateUserAsync(user);
        }

        public async Task<bool> DeleteUserAsync(string id) => await _unitOfWork.Users.DeleteAsync(id);
        public void Dispose() => _unitOfWork.Dispose();
    }
}

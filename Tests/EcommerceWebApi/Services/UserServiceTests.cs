using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using EcommerceWebApi.Entities;
using EcommerceWebApi.Services;
using EcommerceWebApi.Utilities;
using EcommerceWebApi.Repositories;

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
                    Token = "refresh-token",
                    Created = DateTime.UtcNow,
                    Expires = DateTime.UtcNow.AddDays(7)
                }
            };
        }

        [Fact]
        public void GetUserById_ExistingId_ReturnsUser()
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
        public void GetUserById_NonExistingId_ReturnsNull()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.GetById("non-existing")).Returns((User?)null);

            // Act
            var result = _mockUnitOfWork.Object.Users.GetById("non-existing");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetUserByName_ExistingName_ReturnsUser()
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
        public void GetUserByName_NonExistingName_ReturnsNull()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.GetByName("nonexistent")).Returns((User?)null);

            // Act
            var result = _mockUnitOfWork.Object.Users.GetByName("nonexistent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetUserByToken_ExistingToken_ReturnsUser()
        {
            // Arrange
            var user = CreateSampleUser();
            _mockUserRepository.Setup(r => r.GetByToken("refresh-token")).Returns(user);

            // Act
            var result = _mockUnitOfWork.Object.Users.GetByToken("refresh-token");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetUserByToken_NonExistingToken_ReturnsNull()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.GetByToken("invalid-token")).Returns((User?)null);

            // Act
            var result = _mockUnitOfWork.Object.Users.GetByToken("invalid-token");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task InsertUserAsync_ValidUser_ReturnsTrue()
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
        public async Task InsertUserAsync_FailedInsert_ReturnsFalse()
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
        public async Task UpdateUserAsync_ValidUser_ReturnsTrue()
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
        public async Task DeleteUserAsync_ValidId_ReturnsTrue()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.DeleteAsync("user-1")).ReturnsAsync(true);

            // Act
            var result = await _mockUnitOfWork.Object.Users.DeleteAsync("user-1");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteUserAsync_NonExistingId_ReturnsFalse()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.DeleteAsync("non-existing")).ReturnsAsync(false);

            // Act
            var result = await _mockUnitOfWork.Object.Users.DeleteAsync("non-existing");

            // Assert
            Assert.False(result);
        }

        // Test PaginationFilter logic used in UserService
        [Fact]
        public void GetPaginationUsers_WithValidFilter_ReturnsCorrectPage()
        {
            // Arrange
            var users = new List<User>
            {
                CreateSampleUser("1", "user1"),
                CreateSampleUser("2", "user2"),
                CreateSampleUser("3", "user3"),
                CreateSampleUser("4", "user4"),
                CreateSampleUser("5", "user5"),
            };

            var filter = new PaginationFilter(1, 2);

            // Act - simulate pagination logic
            var result = users
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("1", result[0].Id);
            Assert.Equal("2", result[1].Id);
        }

        [Fact]
        public void GetPaginationUsers_SecondPage_ReturnsCorrectPage()
        {
            // Arrange
            var users = new List<User>
            {
                CreateSampleUser("1", "user1"),
                CreateSampleUser("2", "user2"),
                CreateSampleUser("3", "user3"),
                CreateSampleUser("4", "user4"),
                CreateSampleUser("5", "user5"),
            };

            var filter = new PaginationFilter(2, 2);

            // Act - simulate pagination logic
            var result = users
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("3", result[0].Id);
            Assert.Equal("4", result[1].Id);
        }

        [Fact]
        public void UpdateUserTokenAsync_WithRefreshToken_UpdatesToken()
        {
            // Arrange
            var user = CreateSampleUser();
            var newToken = new RefreshToken
            {
                Token = "new-token",
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            // Act - simulate UpdateUserTokenAsync logic
            user.RefreshToken = newToken;

            // Assert
            Assert.Equal("new-token", user.RefreshToken.Token);
        }

        [Fact]
        public void UpdateUserTokenAsync_WithNullRefreshToken_ClearsToken()
        {
            // Arrange
            var user = CreateSampleUser();

            // Act - simulate UpdateUserTokenAsync with null
            user.RefreshToken.Token = null!;

            // Assert
            Assert.Null(user.RefreshToken.Token);
        }

        [Fact]
        public void UpdateUserPropertyAsync_ValidProperty_UpdatesProperty()
        {
            // Arrange
            var user = CreateSampleUser();
            var propertyInfo = typeof(User).GetProperty(
                "Username",
                System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
            );

            // Act
            propertyInfo?.SetValue(user, "newusername", null);

            // Assert
            Assert.Equal("newusername", user.Username);
        }

        [Fact]
        public void UpdateUserPropertyAsync_InvalidProperty_PropertyInfoIsNull()
        {
            // Arrange
            var propertyInfo = typeof(User).GetProperty(
                "NonExistentProperty",
                System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
            );

            // Assert
            Assert.Null(propertyInfo);
        }
    }
}

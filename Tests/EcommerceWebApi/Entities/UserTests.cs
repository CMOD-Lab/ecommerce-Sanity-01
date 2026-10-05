using EcommerceWebApi.Entities;
using System;
using Xunit;

namespace EcommerceWebApi.Tests.Entities
{
    public class UserTests
    {
        [Fact]
        public void User_DefaultConstructor_IdIsNotEmpty()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            Assert.NotNull(user.Id);
            Assert.NotEmpty(user.Id);
        }

        [Fact]
        public void User_DefaultConstructor_IdIsGuid()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            Assert.True(Guid.TryParse(user.Id, out _));
        }

        [Fact]
        public void User_TwoInstances_HaveDifferentIds()
        {
            // Arrange & Act
            var user1 = new User();
            var user2 = new User();

            // Assert
            Assert.NotEqual(user1.Id, user2.Id);
        }

        [Fact]
        public void User_SetUsername_ReturnsCorrectValue()
        {
            // Arrange
            var user = new User();

            // Act
            user.Username = "testuser";

            // Assert
            Assert.Equal("testuser", user.Username);
        }

        [Fact]
        public void User_SetRole_ReturnsCorrectValue()
        {
            // Arrange
            var user = new User();

            // Act
            user.Role = "Admin";

            // Assert
            Assert.Equal("Admin", user.Role);
        }

        [Fact]
        public void User_SetIsTwoFactorAuthActivated_True()
        {
            // Arrange
            var user = new User();

            // Act
            user.IsTwoFactorAuthActivated = true;

            // Assert
            Assert.True(user.IsTwoFactorAuthActivated);
        }

        [Fact]
        public void User_SetIsTwoFactorAuthActivated_False()
        {
            // Arrange
            var user = new User();

            // Act
            user.IsTwoFactorAuthActivated = false;

            // Assert
            Assert.False(user.IsTwoFactorAuthActivated);
        }

        [Fact]
        public void User_SetPasswordSalt_ReturnsCorrectValue()
        {
            // Arrange
            var user = new User();
            var salt = new byte[] { 1, 2, 3, 4, 5 };

            // Act
            user.PasswordSalt = salt;

            // Assert
            Assert.Equal(salt, user.PasswordSalt);
        }

        [Fact]
        public void User_SetPasswordHash_ReturnsCorrectValue()
        {
            // Arrange
            var user = new User();
            var hash = new byte[] { 10, 20, 30, 40, 50 };

            // Act
            user.PasswordHash = hash;

            // Assert
            Assert.Equal(hash, user.PasswordHash);
        }

        [Fact]
        public void User_SetSecretCode_ReturnsCorrectValue()
        {
            // Arrange
            var user = new User();

            // Act
            user.SecretCode = "MYSECRETCODE";

            // Assert
            Assert.Equal("MYSECRETCODE", user.SecretCode);
        }

        [Fact]
        public void User_SecretCode_DefaultIsNull()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            Assert.Null(user.SecretCode);
        }

        [Fact]
        public void User_SetRefreshToken_ReturnsCorrectValue()
        {
            // Arrange
            var user = new User();
            var refreshToken = new RefreshToken
            {
                Token = "test-token",
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            // Act
            user.RefreshToken = refreshToken;

            // Assert
            Assert.Equal("test-token", user.RefreshToken.Token);
        }

        [Fact]
        public void User_SetId_OverridesDefault()
        {
            // Arrange
            var user = new User();
            var customId = "custom-id-123";

            // Act
            user.Id = customId;

            // Assert
            Assert.Equal(customId, user.Id);
        }
    }

    public class RefreshTokenTests
    {
        [Fact]
        public void RefreshToken_DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var token = new RefreshToken();

            // Assert
            Assert.NotNull(token);
        }

        [Fact]
        public void RefreshToken_SetToken_ReturnsCorrectValue()
        {
            // Arrange
            var refreshToken = new RefreshToken();

            // Act
            refreshToken.Token = "my-refresh-token";

            // Assert
            Assert.Equal("my-refresh-token", refreshToken.Token);
        }

        [Fact]
        public void RefreshToken_SetCreated_ReturnsCorrectValue()
        {
            // Arrange
            var refreshToken = new RefreshToken();
            var now = DateTime.UtcNow;

            // Act
            refreshToken.Created = now;

            // Assert
            Assert.Equal(now, refreshToken.Created);
        }

        [Fact]
        public void RefreshToken_SetExpires_ReturnsCorrectValue()
        {
            // Arrange
            var refreshToken = new RefreshToken();
            var expires = DateTime.UtcNow.AddDays(7);

            // Act
            refreshToken.Expires = expires;

            // Assert
            Assert.Equal(expires, refreshToken.Expires);
        }

        [Fact]
        public void RefreshToken_ExpiresAfterCreated_IsValid()
        {
            // Arrange
            var refreshToken = new RefreshToken
            {
                Token = "token",
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            // Assert
            Assert.True(refreshToken.Expires > refreshToken.Created);
        }

        [Fact]
        public void RefreshToken_IsExpired_WhenExpiresInPast()
        {
            // Arrange
            var refreshToken = new RefreshToken
            {
                Token = "token",
                Created = DateTime.UtcNow.AddDays(-8),
                Expires = DateTime.UtcNow.AddDays(-1)
            };

            // Assert
            Assert.True(refreshToken.Expires < DateTime.UtcNow);
        }
    }
}

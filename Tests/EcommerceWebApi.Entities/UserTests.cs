using System;
using System.ComponentModel.DataAnnotations;
using Xunit;
using EcommerceWebApi.Entities;

namespace EcommerceWebApi.Entities.Tests
{
    public class UserTests
    {
        [Fact]
        public void User_DefaultConstructor_GeneratesGuidId()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            Assert.NotNull(user.Id);
            Assert.NotEmpty(user.Id);
            Assert.True(Guid.TryParse(user.Id, out _));
        }

        [Fact]
        public void User_SetUsername_ReturnsCorrectUsername()
        {
            // Arrange
            var user = new User();

            // Act
            user.Username = "testuser";

            // Assert
            Assert.Equal("testuser", user.Username);
        }

        [Fact]
        public void User_SetRole_ReturnsCorrectRole()
        {
            // Arrange
            var user = new User();

            // Act
            user.Role = "Admin";

            // Assert
            Assert.Equal("Admin", user.Role);
        }

        [Fact]
        public void User_IsTwoFactorAuthActivated_DefaultFalse()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            Assert.False(user.IsTwoFactorAuthActivated);
        }

        [Fact]
        public void User_SetIsTwoFactorAuthActivated_ReturnsCorrectValue()
        {
            // Arrange
            var user = new User();

            // Act
            user.IsTwoFactorAuthActivated = true;

            // Assert
            Assert.True(user.IsTwoFactorAuthActivated);
        }

        [Fact]
        public void User_SetPasswordSalt_ReturnsCorrectSalt()
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
        public void User_SetPasswordHash_ReturnsCorrectHash()
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
        public void User_SetSecretCode_ReturnsCorrectSecretCode()
        {
            // Arrange
            var user = new User();

            // Act
            user.SecretCode = "ABCDEFGH";

            // Assert
            Assert.Equal("ABCDEFGH", user.SecretCode);
        }

        [Fact]
        public void User_SecretCode_DefaultNull()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            Assert.Null(user.SecretCode);
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
        public void RefreshToken_DefaultConstructor_SetsDefaultValues()
        {
            // Arrange & Act
            var token = new RefreshToken();

            // Assert
            Assert.Equal(0, token.Id);
            Assert.Null(token.Token);
        }

        [Fact]
        public void RefreshToken_SetToken_ReturnsCorrectToken()
        {
            // Arrange
            var token = new RefreshToken();

            // Act
            token.Token = "test-refresh-token";

            // Assert
            Assert.Equal("test-refresh-token", token.Token);
        }

        [Fact]
        public void RefreshToken_SetUserId_ReturnsCorrectUserId()
        {
            // Arrange
            var token = new RefreshToken();
            var userId = Guid.NewGuid().ToString();

            // Act
            token.UserId = userId;

            // Assert
            Assert.Equal(userId, token.UserId);
        }

        [Fact]
        public void RefreshToken_SetCreated_ReturnsCorrectDate()
        {
            // Arrange
            var token = new RefreshToken();
            var now = DateTime.UtcNow;

            // Act
            token.Created = now;

            // Assert
            Assert.Equal(now, token.Created);
        }

        [Fact]
        public void RefreshToken_SetExpires_ReturnsCorrectDate()
        {
            // Arrange
            var token = new RefreshToken();
            var expires = DateTime.UtcNow.AddDays(7);

            // Act
            token.Expires = expires;

            // Assert
            Assert.Equal(expires, token.Expires);
        }

        [Fact]
        public void RefreshToken_IsExpired_WhenExpiresInPast()
        {
            // Arrange
            var token = new RefreshToken
            {
                Expires = DateTime.UtcNow.AddDays(-1)
            };

            // Act & Assert
            Assert.True(token.Expires < DateTime.UtcNow);
        }

        [Fact]
        public void RefreshToken_IsNotExpired_WhenExpiresInFuture()
        {
            // Arrange
            var token = new RefreshToken
            {
                Expires = DateTime.UtcNow.AddDays(7)
            };

            // Act & Assert
            Assert.True(token.Expires > DateTime.UtcNow);
        }
    }
}

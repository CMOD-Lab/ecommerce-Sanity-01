using System;
using Xunit;
using EcommerceWebApi.Entities;

namespace EcommerceWebApi.Tests.Entities
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
        public void User_DefaultIsTwoFactorAuthActivated_IsFalse()
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
        public void User_SetPasswordSalt_ReturnsCorrectValue()
        {
            // Arrange
            var user = new User();
            var salt = new byte[] { 1, 2, 3, 4 };

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
            var hash = new byte[] { 5, 6, 7, 8 };

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
            user.SecretCode = "MYSECRET";

            // Assert
            Assert.Equal("MYSECRET", user.SecretCode);
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
        public void RefreshToken_DefaultConstructor_SetsDefaultValues()
        {
            // Arrange & Act
            var token = new RefreshToken();

            // Assert
            Assert.Equal(default(DateTime), token.Created);
            Assert.Equal(default(DateTime), token.Expires);
        }

        [Fact]
        public void RefreshToken_SetToken_ReturnsCorrectValue()
        {
            // Arrange
            var token = new RefreshToken();

            // Act
            token.Token = "my-refresh-token";

            // Assert
            Assert.Equal("my-refresh-token", token.Token);
        }

        [Fact]
        public void RefreshToken_SetCreated_ReturnsCorrectValue()
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
        public void RefreshToken_SetExpires_ReturnsCorrectValue()
        {
            // Arrange
            var token = new RefreshToken();
            var expires = DateTime.UtcNow.AddDays(7);

            // Act
            token.Expires = expires;

            // Assert
            Assert.Equal(expires, token.Expires);
        }
    }
}

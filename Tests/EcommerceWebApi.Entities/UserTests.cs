using System;
using System.Collections.Generic;
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
        public void User_SecretCode_DefaultNull()
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
                Token = "token123",
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            // Act
            user.RefreshToken = refreshToken;

            // Assert
            Assert.Equal("token123", user.RefreshToken.Token);
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
            refreshToken.Token = "mytoken";

            // Assert
            Assert.Equal("mytoken", refreshToken.Token);
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
        public void RefreshToken_IsExpired_WhenExpiresInPast()
        {
            // Arrange
            var refreshToken = new RefreshToken
            {
                Token = "token",
                Created = DateTime.UtcNow.AddDays(-8),
                Expires = DateTime.UtcNow.AddDays(-1)
            };

            // Act & Assert
            Assert.True(refreshToken.Expires < DateTime.UtcNow);
        }

        [Fact]
        public void RefreshToken_IsValid_WhenExpiresInFuture()
        {
            // Arrange
            var refreshToken = new RefreshToken
            {
                Token = "token",
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            // Act & Assert
            Assert.True(refreshToken.Expires > DateTime.UtcNow);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using EcommerceWebApi.Authentication;
using EcommerceWebApi.Entities;
using EcommerceWebApi.Services;
using EcommerceWebApi.Repositories;

namespace EcommerceWebApi.Authentication.Tests
{
    // Testable subclass for AuthService
    internal class AuthServiceTestable : AuthService
    {
        public AuthServiceTestable(IUserService userService, ITotpService totpService, IJwtService jwtService)
            : base(userService, totpService, jwtService)
        {
        }
    }

    public class AuthServiceTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ITotpService> _mockTotpService;
        private readonly Mock<IJwtService> _mockJwtService;

        public AuthServiceTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockTotpService = new Mock<ITotpService>();
            _mockJwtService = new Mock<IJwtService>();
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

        private AuthServiceTestable CreateService()
        {
            return new AuthServiceTestable(
                _mockUserService.Object,
                _mockTotpService.Object,
                _mockJwtService.Object
            );
        }

        [Fact]
        public async Task Login_WhenUserNotFound_ReturnsUserNotExist()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetUserByName("nonexistent")).Returns((User?)null);
            var service = CreateService();

            // Act
            var result = await service.Login("nonexistent", "password", null!);

            // Assert
            Assert.Equal(AuthService.AuthResult.UserNotExist, result);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ReturnsInvalidCredentials()
        {
            // Arrange
            // Create a user with a known password hash
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = "testuser",
                Role = "User",
                PasswordSalt = hmac.Key,
                PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes("correctpassword")),
                IsTwoFactorAuthActivated = false
            };

            _mockUserService.Setup(s => s.GetUserByName("testuser")).Returns(user);
            var service = CreateService();

            // Act
            var result = await service.Login("testuser", "wrongpassword", null!);

            // Assert
            Assert.Equal(AuthService.AuthResult.InvalidCredentials, result);
        }

        [Fact]
        public async Task Login_WithCorrectPasswordAndNo2FA_ReturnsSuccess()
        {
            // Arrange
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = "testuser",
                Role = "User",
                PasswordSalt = hmac.Key,
                PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes("password123")),
                IsTwoFactorAuthActivated = false
            };

            _mockUserService.Setup(s => s.GetUserByName("testuser")).Returns(user);
            _mockJwtService.Setup(j => j.GenerateJWT(user, true, It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()))
                           .ReturnsAsync(true);

            var service = CreateService();

            // Act
            var result = await service.Login("testuser", "password123", null!);

            // Assert
            Assert.Equal(AuthService.AuthResult.Success, result);
        }

        [Fact]
        public async Task Login_WithCorrectPasswordAnd2FA_ReturnsNeedSecondFactorAuth()
        {
            // Arrange
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = "testuser",
                Role = "User",
                PasswordSalt = hmac.Key,
                PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes("password123")),
                IsTwoFactorAuthActivated = true
            };

            _mockUserService.Setup(s => s.GetUserByName("testuser")).Returns(user);
            _mockJwtService.Setup(j => j.GenerateJWT(user, false, It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()))
                           .ReturnsAsync(true);

            var service = CreateService();

            // Act
            var result = await service.Login("testuser", "password123", null!);

            // Assert
            Assert.Equal(AuthService.AuthResult.NeedSecondFactorAuth, result);
        }

        [Fact]
        public async Task Register_WhenUserAlreadyExists_ReturnsUserExisted()
        {
            // Arrange
            var existingUser = CreateSampleUser();
            _mockUserService.Setup(s => s.GetUserByName("existinguser")).Returns(existingUser);

            var service = CreateService();

            // Act
            var result = await service.Register("existinguser", "password");

            // Assert
            Assert.Equal(AuthService.AuthResult.UserExisted, result);
        }

        [Fact]
        public async Task Register_WhenUserNotExists_ReturnsSuccess()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetUserByName("newuser")).Returns((User?)null);
            _mockUserService.Setup(s => s.InsertUserAsync(It.IsAny<User>())).ReturnsAsync(true);

            var service = CreateService();

            // Act
            var result = await service.Register("newuser", "password123");

            // Assert
            Assert.Equal(AuthService.AuthResult.Success, result);
        }

        [Fact]
        public async Task Register_WhenInsertFails_ReturnsFail()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetUserByName("newuser")).Returns((User?)null);
            _mockUserService.Setup(s => s.InsertUserAsync(It.IsAny<User>())).ReturnsAsync(false);

            var service = CreateService();

            // Act
            var result = await service.Register("newuser", "password123");

            // Assert
            Assert.Equal(AuthService.AuthResult.Fail, result);
        }

        [Fact]
        public async Task RevokeToken_WhenSuccessful_ReturnsSuccess()
        {
            // Arrange
            var user = CreateSampleUser();
            _mockJwtService.Setup(j => j.RevokeToken(user, It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()))
                           .ReturnsAsync(true);

            var service = CreateService();
            service.CurrentUser = user;

            // Act
            var result = await service.RevokeToken(null!);

            // Assert
            Assert.Equal(AuthService.AuthResult.Success, result);
        }

        [Fact]
        public async Task RevokeToken_WhenFails_ReturnsFail()
        {
            // Arrange
            var user = CreateSampleUser();
            _mockJwtService.Setup(j => j.RevokeToken(user, It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()))
                           .ReturnsAsync(false);

            var service = CreateService();
            service.CurrentUser = user;

            // Act
            var result = await service.RevokeToken(null!);

            // Assert
            Assert.Equal(AuthService.AuthResult.Fail, result);
        }

        [Fact]
        public void GetQrCode_ReturnsSuccess()
        {
            // Arrange
            var user = CreateSampleUser();
            user.Username = "testuser";
            _mockTotpService.Setup(t => t.GenerateBase32Secret()).Returns("ABCDEFGHIJKLMNOP");
            _mockTotpService.Setup(t => t.GenerateQrCode(It.IsAny<string>(), It.IsAny<string>()))
                            .Returns("otpauth://totp/EcommerceWebApi:testuser?secret=ABCDEFGHIJKLMNOP&issuer=EcommerceWebApi");

            var service = CreateService();
            service.CurrentUser = user;

            // Act
            var result = service.GetQrCode(out string? uriString);

            // Assert
            Assert.Equal(AuthService.AuthResult.Success, result);
            Assert.NotNull(uriString);
        }

        [Fact]
        public async Task ValidateQrCode_WithInvalidQrCode_ReturnsInvalidCredentials()
        {
            // Arrange
            _mockTotpService.Setup(t => t.GetSecretFromQrCode(It.IsAny<string>())).Returns((string?)null);

            var service = CreateService();
            service.CurrentUser = CreateSampleUser();

            // Act
            var result = await service.ValidateQrCode("invalid-qr", "123456");

            // Assert
            Assert.Equal(AuthService.AuthResult.InvalidCredentials, result);
        }

        [Fact]
        public async Task ValidateQrCode_WithInvalidTotp_ReturnsInvalidCredentials()
        {
            // Arrange
            _mockTotpService.Setup(t => t.GetSecretFromQrCode(It.IsAny<string>())).Returns("VALIDBASE32SECRET");
            _mockTotpService.Setup(t => t.ValidateTotp(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            var service = CreateService();
            service.CurrentUser = CreateSampleUser();

            // Act
            var result = await service.ValidateQrCode("valid-qr", "wrong-totp");

            // Assert
            Assert.Equal(AuthService.AuthResult.InvalidCredentials, result);
        }

        [Fact]
        public async Task ValidateQrCode_WithValidQrCodeAndTotp_ReturnsSuccess()
        {
            // Arrange
            var user = CreateSampleUser();
            _mockTotpService.Setup(t => t.GetSecretFromQrCode(It.IsAny<string>())).Returns("VALIDBASE32SECRET");
            _mockTotpService.Setup(t => t.ValidateTotp(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _mockUserService.Setup(s => s.UpdateUserAsync(user)).ReturnsAsync(true);

            var service = CreateService();
            service.CurrentUser = user;

            // Act
            var result = await service.ValidateQrCode("valid-qr", "123456");

            // Assert
            Assert.Equal(AuthService.AuthResult.Success, result);
        }

        [Fact]
        public async Task LoginSecondFactor_WithInvalidTotp_ReturnsInvalidCredentials()
        {
            // Arrange
            var user = CreateSampleUser();
            user.SecretCode = "VALIDBASE32SECRET";
            _mockTotpService.Setup(t => t.ValidateTotp(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            var service = CreateService();
            service.CurrentUser = user;

            // Act
            var result = await service.LoginSecondFactor("wrong-totp", null!);

            // Assert
            Assert.Equal(AuthService.AuthResult.InvalidCredentials, result);
        }

        [Fact]
        public async Task LoginSecondFactor_WithValidTotp_ReturnsSuccess()
        {
            // Arrange
            var user = CreateSampleUser();
            user.SecretCode = "VALIDBASE32SECRET";
            _mockTotpService.Setup(t => t.ValidateTotp(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _mockJwtService.Setup(j => j.RevokeToken(user, It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()))
                           .ReturnsAsync(true);
            _mockJwtService.Setup(j => j.GenerateJWT(user, true, It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()))
                           .ReturnsAsync(true);

            var service = CreateService();
            service.CurrentUser = user;

            // Act
            var result = await service.LoginSecondFactor("123456", null!);

            // Assert
            Assert.Equal(AuthService.AuthResult.Success, result);
        }

        [Theory]
        [InlineData(AuthService.AuthResult.UserNotExist)]
        [InlineData(AuthService.AuthResult.UserExisted)]
        [InlineData(AuthService.AuthResult.InvalidCredentials)]
        [InlineData(AuthService.AuthResult.NeedSecondFactorAuth)]
        [InlineData(AuthService.AuthResult.Success)]
        [InlineData(AuthService.AuthResult.Fail)]
        public void AuthResult_AllValues_AreValid(AuthService.AuthResult result)
        {
            // Assert
            Assert.True(Enum.IsDefined(typeof(AuthService.AuthResult), result));
        }
    }
}

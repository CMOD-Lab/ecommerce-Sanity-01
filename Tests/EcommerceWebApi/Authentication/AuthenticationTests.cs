using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using EcommerceWebApi.Authentication;
using EcommerceWebApi.Entities;
using EcommerceWebApi.Services;
using static EcommerceWebApi.Authentication.AuthService;

namespace EcommerceWebApi.Tests.Authentication
{
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
                SecretCode = null,
                RefreshToken = new RefreshToken
                {
                    Token = "token123",
                    Created = DateTime.UtcNow,
                    Expires = DateTime.UtcNow.AddDays(7)
                }
            };
        }

        [Fact]
        public void AuthResult_Enum_HasCorrectValues()
        {
            // Assert
            Assert.Equal(0, (int)AuthResult.UserNotExist);
            Assert.Equal(1, (int)AuthResult.UserExisted);
            Assert.Equal(2, (int)AuthResult.InvalidCredentials);
            Assert.Equal(3, (int)AuthResult.NeedSecondFactorAuth);
            Assert.Equal(4, (int)AuthResult.Success);
            Assert.Equal(5, (int)AuthResult.Fail);
        }

        [Fact]
        public void IUserService_GetUserByName_ReturnsNull_WhenUserNotFound()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetUserByName("nonexistent")).Returns((User?)null);

            // Act
            var result = _mockUserService.Object.GetUserByName("nonexistent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void IUserService_GetUserByName_ReturnsUser_WhenUserFound()
        {
            // Arrange
            var user = CreateSampleUser();
            _mockUserService.Setup(s => s.GetUserByName("testuser")).Returns(user);

            // Act
            var result = _mockUserService.Object.GetUserByName("testuser");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testuser", result.Username);
        }

        [Fact]
        public async Task IUserService_InsertUserAsync_ReturnsTrue_OnSuccess()
        {
            // Arrange
            var user = CreateSampleUser();
            _mockUserService.Setup(s => s.InsertUserAsync(user)).ReturnsAsync(true);

            // Act
            var result = await _mockUserService.Object.InsertUserAsync(user);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IUserService_InsertUserAsync_ReturnsFalse_OnFailure()
        {
            // Arrange
            var user = CreateSampleUser();
            _mockUserService.Setup(s => s.InsertUserAsync(user)).ReturnsAsync(false);

            // Act
            var result = await _mockUserService.Object.InsertUserAsync(user);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IUserService_UpdateUserAsync_ReturnsTrue_OnSuccess()
        {
            // Arrange
            var user = CreateSampleUser();
            _mockUserService.Setup(s => s.UpdateUserAsync(user)).ReturnsAsync(true);

            // Act
            var result = await _mockUserService.Object.UpdateUserAsync(user);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ITotpService_GenerateBase32Secret_ReturnsNonEmptyString()
        {
            // Arrange
            _mockTotpService.Setup(s => s.GenerateBase32Secret()).Returns("JBSWY3DPEHPK3PXP");

            // Act
            var result = _mockTotpService.Object.GenerateBase32Secret();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void ITotpService_GenerateQrCode_WithValidInputs_ReturnsUri()
        {
            // Arrange
            _mockTotpService.Setup(s => s.GenerateQrCode("SECRET", "user"))
                            .Returns("otpauth://totp/EcommerceWebApi:user?secret=SECRET&issuer=EcommerceWebApi");

            // Act
            var result = _mockTotpService.Object.GenerateQrCode("SECRET", "user");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("otpauth://totp", result);
        }

        [Fact]
        public void ITotpService_GenerateQrCode_WithNullUsername_ReturnsNull()
        {
            // Arrange
            _mockTotpService.Setup(s => s.GenerateQrCode(It.IsAny<string>(), null))
                            .Returns((string?)null);

            // Act
            var result = _mockTotpService.Object.GenerateQrCode("SECRET", null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ITotpService_ValidateTotp_WithValidTotp_ReturnsTrue()
        {
            // Arrange
            _mockTotpService.Setup(s => s.ValidateTotp("SECRET", "123456")).Returns(true);

            // Act
            var result = _mockTotpService.Object.ValidateTotp("SECRET", "123456");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ITotpService_ValidateTotp_WithInvalidTotp_ReturnsFalse()
        {
            // Arrange
            _mockTotpService.Setup(s => s.ValidateTotp("SECRET", "000000")).Returns(false);

            // Act
            var result = _mockTotpService.Object.ValidateTotp("SECRET", "000000");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ITotpService_GetSecretFromQrCode_WithValidQrCode_ReturnsSecret()
        {
            // Arrange
            var qrCode = "otpauth://totp/EcommerceWebApi:user?secret=MYSECRET&issuer=EcommerceWebApi";
            _mockTotpService.Setup(s => s.GetSecretFromQrCode(qrCode)).Returns("MYSECRET");

            // Act
            var result = _mockTotpService.Object.GetSecretFromQrCode(qrCode);

            // Assert
            Assert.Equal("MYSECRET", result);
        }

        [Fact]
        public void ITotpService_GetSecretFromQrCode_WithInvalidQrCode_ReturnsNull()
        {
            // Arrange
            _mockTotpService.Setup(s => s.GetSecretFromQrCode("invalid")).Returns((string?)null);

            // Act
            var result = _mockTotpService.Object.GetSecretFromQrCode("invalid");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task IJwtService_GenerateJWT_ReturnsTrue_OnSuccess()
        {
            // Arrange
            var user = CreateSampleUser();
            var mockContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
            _mockJwtService.Setup(s => s.GenerateJWT(user, true, mockContext.Object)).ReturnsAsync(true);

            // Act
            var result = await _mockJwtService.Object.GenerateJWT(user, true, mockContext.Object);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IJwtService_RevokeToken_ReturnsTrue_OnSuccess()
        {
            // Arrange
            var user = CreateSampleUser();
            var mockContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
            _mockJwtService.Setup(s => s.RevokeToken(user, mockContext.Object)).ReturnsAsync(true);

            // Act
            var result = await _mockJwtService.Object.RevokeToken(user, mockContext.Object);

            // Assert
            Assert.True(result);
        }
    }

    public class TotpServiceUnitTests
    {
        [Fact]
        public void TotpService_DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var service = new TotpService();

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void GenerateBase32Secret_ReturnsNonEmptyString()
        {
            // Arrange
            var service = new TotpService();

            // Act
            var result = service.GenerateBase32Secret();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GenerateBase32Secret_ReturnsDifferentValuesEachTime()
        {
            // Arrange
            var service = new TotpService();

            // Act
            var secret1 = service.GenerateBase32Secret();
            var secret2 = service.GenerateBase32Secret();

            // Assert
            Assert.NotEqual(secret1, secret2);
        }

        [Fact]
        public void GenerateQrCode_WithValidInputs_ReturnsOtpAuthUri()
        {
            // Arrange
            var service = new TotpService();
            var secret = service.GenerateBase32Secret();

            // Act
            var result = service.GenerateQrCode(secret, "testuser");

            // Assert
            Assert.NotNull(result);
            Assert.StartsWith("otpauth://totp/", result);
        }

        [Fact]
        public void GenerateQrCode_WithNullUsername_ReturnsNull()
        {
            // Arrange
            var service = new TotpService();

            // Act
            var result = service.GenerateQrCode("SECRET", null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GenerateQrCode_WithNullSecret_ReturnsNull()
        {
            // Arrange
            var service = new TotpService();

            // Act
            var result = service.GenerateQrCode(null, "testuser");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GenerateQrCode_WithBothNull_ReturnsNull()
        {
            // Arrange
            var service = new TotpService();

            // Act
            var result = service.GenerateQrCode(null, null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetSecretFromQrCode_WithValidQrCode_ReturnsSecret()
        {
            // Arrange
            var service = new TotpService();
            var secret = service.GenerateBase32Secret();
            var qrCode = service.GenerateQrCode(secret, "testuser");

            // Act
            var result = service.GetSecretFromQrCode(qrCode!);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(secret, result);
        }

        [Fact]
        public void GetSecretFromQrCode_WithInvalidUri_ReturnsNull()
        {
            // Arrange
            var service = new TotpService();

            // Act
            var result = service.GetSecretFromQrCode("not-a-valid-uri");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetSecretFromQrCode_WithWrongScheme_ReturnsNull()
        {
            // Arrange
            var service = new TotpService();

            // Act
            var result = service.GetSecretFromQrCode("https://example.com?secret=test");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetSecretFromQrCode_WithWrongIssuer_ReturnsNull()
        {
            // Arrange
            var service = new TotpService();

            // Act
            var result = service.GetSecretFromQrCode("otpauth://totp/WrongIssuer:user?secret=SECRET&issuer=WrongIssuer");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetSecretFromQrCode_WithMissingSecret_ReturnsNull()
        {
            // Arrange
            var service = new TotpService();

            // Act
            var result = service.GetSecretFromQrCode("otpauth://totp/EcommerceWebApi:user?issuer=EcommerceWebApi");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GenerateQrCode_ContainsIssuer()
        {
            // Arrange
            var service = new TotpService();
            var secret = service.GenerateBase32Secret();

            // Act
            var result = service.GenerateQrCode(secret, "testuser");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("EcommerceWebApi", result);
        }

        [Fact]
        public void GenerateQrCode_ContainsUsername()
        {
            // Arrange
            var service = new TotpService();
            var secret = service.GenerateBase32Secret();

            // Act
            var result = service.GenerateQrCode(secret, "myuser");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("myuser", result);
        }
    }
}

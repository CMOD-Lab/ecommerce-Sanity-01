using EcommerceWebApi.Authentication;
using EcommerceWebApi.Entities;
using EcommerceWebApi.Services;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

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

        private User CreateTestUser(string username = "testuser", bool twoFactor = false)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            return new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = username,
                Role = "User",
                IsTwoFactorAuthActivated = twoFactor,
                PasswordSalt = hmac.Key,
                PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes("password123")),
                RefreshToken = new RefreshToken
                {
                    Token = "refresh-token",
                    Created = DateTime.UtcNow,
                    Expires = DateTime.UtcNow.AddDays(7)
                }
            };
        }

        [Fact]
        public async Task Register_NewUser_ReturnsSuccess()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetUserByName("newuser")).Returns((User?)null);
            _mockUserService.Setup(s => s.InsertUserAsync(It.IsAny<User>())).ReturnsAsync(true);

            var authService = CreateAuthService();

            // Act
            var result = await authService.Register("newuser", "password123");

            // Assert
            Assert.Equal(AuthService.AuthResult.Success, result);
        }

        [Fact]
        public async Task Register_ExistingUser_ReturnsUserExisted()
        {
            // Arrange
            var existingUser = CreateTestUser("existinguser");
            _mockUserService.Setup(s => s.GetUserByName("existinguser")).Returns(existingUser);

            var authService = CreateAuthService();

            // Act
            var result = await authService.Register("existinguser", "password123");

            // Assert
            Assert.Equal(AuthService.AuthResult.UserExisted, result);
        }

        [Fact]
        public async Task Register_InsertFails_ReturnsFail()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetUserByName("newuser")).Returns((User?)null);
            _mockUserService.Setup(s => s.InsertUserAsync(It.IsAny<User>())).ReturnsAsync(false);

            var authService = CreateAuthService();

            // Act
            var result = await authService.Register("newuser", "password123");

            // Assert
            Assert.Equal(AuthService.AuthResult.Fail, result);
        }

        [Fact]
        public async Task Login_UserNotFound_ReturnsUserNotExist()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetUserByName("unknown")).Returns((User?)null);

            var authService = CreateAuthService();
            var mockContext = CreateMockHttpContext();

            // Act
            var result = await authService.Login("unknown", "password", mockContext);

            // Assert
            Assert.Equal(AuthService.AuthResult.UserNotExist, result);
        }

        [Fact]
        public async Task Login_WrongPassword_ReturnsInvalidCredentials()
        {
            // Arrange
            var user = CreateTestUser("testuser");
            _mockUserService.Setup(s => s.GetUserByName("testuser")).Returns(user);

            var authService = CreateAuthService();
            var mockContext = CreateMockHttpContext();

            // Act
            var result = await authService.Login("testuser", "wrongpassword", mockContext);

            // Assert
            Assert.Equal(AuthService.AuthResult.InvalidCredentials, result);
        }

        [Fact]
        public async Task Login_CorrectPassword_NoTwoFactor_ReturnsSuccess()
        {
            // Arrange
            var user = CreateTestUser("testuser", twoFactor: false);
            _mockUserService.Setup(s => s.GetUserByName("testuser")).Returns(user);
            _mockJwtService.Setup(j => j.GenerateJWT(user, true, It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()))
                           .ReturnsAsync(true);

            var authService = CreateAuthService();
            var mockContext = CreateMockHttpContext();

            // Act
            var result = await authService.Login("testuser", "password123", mockContext);

            // Assert
            Assert.Equal(AuthService.AuthResult.Success, result);
        }

        [Fact]
        public async Task Login_CorrectPassword_WithTwoFactor_ReturnsNeedSecondFactor()
        {
            // Arrange
            var user = CreateTestUser("testuser", twoFactor: true);
            _mockUserService.Setup(s => s.GetUserByName("testuser")).Returns(user);
            _mockJwtService.Setup(j => j.GenerateJWT(user, false, It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()))
                           .ReturnsAsync(true);

            var authService = CreateAuthService();
            var mockContext = CreateMockHttpContext();

            // Act
            var result = await authService.Login("testuser", "password123", mockContext);

            // Assert
            Assert.Equal(AuthService.AuthResult.NeedSecondFactorAuth, result);
        }

        [Fact]
        public async Task Login_JwtGenerationFails_ReturnsFail()
        {
            // Arrange
            var user = CreateTestUser("testuser", twoFactor: false);
            _mockUserService.Setup(s => s.GetUserByName("testuser")).Returns(user);
            _mockJwtService.Setup(j => j.GenerateJWT(user, true, It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()))
                           .ReturnsAsync(false);

            var authService = CreateAuthService();
            var mockContext = CreateMockHttpContext();

            // Act
            var result = await authService.Login("testuser", "password123", mockContext);

            // Assert
            Assert.Equal(AuthService.AuthResult.Fail, result);
        }

        [Fact]
        public async Task RevokeToken_Success_ReturnsSuccess()
        {
            // Arrange
            var user = CreateTestUser();
            _mockJwtService.Setup(j => j.RevokeToken(user, It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()))
                           .ReturnsAsync(true);

            var authService = CreateAuthService();
            ((TestableAuthService)authService).CurrentUser = user;
            var mockContext = CreateMockHttpContext();

            // Act
            var result = await authService.RevokeToken(mockContext);

            // Assert
            Assert.Equal(AuthService.AuthResult.Success, result);
        }

        [Fact]
        public async Task RevokeToken_Fails_ReturnsFail()
        {
            // Arrange
            var user = CreateTestUser();
            _mockJwtService.Setup(j => j.RevokeToken(user, It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()))
                           .ReturnsAsync(false);

            var authService = CreateAuthService();
            ((TestableAuthService)authService).CurrentUser = user;
            var mockContext = CreateMockHttpContext();

            // Act
            var result = await authService.RevokeToken(mockContext);

            // Assert
            Assert.Equal(AuthService.AuthResult.Fail, result);
        }

        [Fact]
        public void GetQrCode_ReturnsSuccess()
        {
            // Arrange
            var user = CreateTestUser();
            _mockTotpService.Setup(t => t.GenerateBase32Secret()).Returns("SECRETBASE32");
            _mockTotpService.Setup(t => t.GenerateQrCode(It.IsAny<string>(), It.IsAny<string>()))
                            .Returns("otpauth://totp/test");

            var authService = CreateAuthService();
            ((TestableAuthService)authService).CurrentUser = user;

            // Act
            var result = authService.GetQrCode(out string? uriString);

            // Assert
            Assert.Equal(AuthService.AuthResult.Success, result);
            Assert.NotNull(uriString);
        }

        [Fact]
        public async Task ValidateQrCode_InvalidQrCode_ReturnsInvalidCredentials()
        {
            // Arrange
            _mockTotpService.Setup(t => t.GetSecretFromQrCode(It.IsAny<string>())).Returns((string?)null);

            var authService = CreateAuthService();
            ((TestableAuthService)authService).CurrentUser = CreateTestUser();

            // Act
            var result = await authService.ValidateQrCode("invalid-qr", "123456");

            // Assert
            Assert.Equal(AuthService.AuthResult.InvalidCredentials, result);
        }

        [Fact]
        public async Task ValidateQrCode_InvalidTotp_ReturnsInvalidCredentials()
        {
            // Arrange
            _mockTotpService.Setup(t => t.GetSecretFromQrCode(It.IsAny<string>())).Returns("SECRETCODE");
            _mockTotpService.Setup(t => t.ValidateTotp(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            var authService = CreateAuthService();
            ((TestableAuthService)authService).CurrentUser = CreateTestUser();

            // Act
            var result = await authService.ValidateQrCode("valid-qr", "000000");

            // Assert
            Assert.Equal(AuthService.AuthResult.InvalidCredentials, result);
        }

        [Fact]
        public async Task ValidateQrCode_ValidTotpUpdateSuccess_ReturnsSuccess()
        {
            // Arrange
            var user = CreateTestUser();
            _mockTotpService.Setup(t => t.GetSecretFromQrCode(It.IsAny<string>())).Returns("SECRETCODE");
            _mockTotpService.Setup(t => t.ValidateTotp(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _mockUserService.Setup(s => s.UpdateUserAsync(It.IsAny<User>())).ReturnsAsync(true);

            var authService = CreateAuthService();
            ((TestableAuthService)authService).CurrentUser = user;

            // Act
            var result = await authService.ValidateQrCode("valid-qr", "123456");

            // Assert
            Assert.Equal(AuthService.AuthResult.Success, result);
        }

        [Fact]
        public async Task ValidateQrCode_ValidTotpUpdateFails_ReturnsFail()
        {
            // Arrange
            var user = CreateTestUser();
            _mockTotpService.Setup(t => t.GetSecretFromQrCode(It.IsAny<string>())).Returns("SECRETCODE");
            _mockTotpService.Setup(t => t.ValidateTotp(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _mockUserService.Setup(s => s.UpdateUserAsync(It.IsAny<User>())).ReturnsAsync(false);

            var authService = CreateAuthService();
            ((TestableAuthService)authService).CurrentUser = user;

            // Act
            var result = await authService.ValidateQrCode("valid-qr", "123456");

            // Assert
            Assert.Equal(AuthService.AuthResult.Fail, result);
        }

        [Fact]
        public async Task LoginSecondFactor_InvalidTotp_ReturnsInvalidCredentials()
        {
            // Arrange
            var user = CreateTestUser();
            user.SecretCode = "SECRETCODE";
            _mockTotpService.Setup(t => t.ValidateTotp(It.IsAny<string?>(), It.IsAny<string>())).Returns(false);

            var authService = CreateAuthService();
            ((TestableAuthService)authService).CurrentUser = user;
            var mockContext = CreateMockHttpContext();

            // Act
            var result = await authService.LoginSecondFactor("000000", mockContext);

            // Assert
            Assert.Equal(AuthService.AuthResult.InvalidCredentials, result);
        }

        [Fact]
        public async Task LoginSecondFactor_ValidTotp_RevokeTokenFails_ReturnsFail()
        {
            // Arrange
            var user = CreateTestUser();
            user.SecretCode = "SECRETCODE";
            _mockTotpService.Setup(t => t.ValidateTotp(It.IsAny<string?>(), It.IsAny<string>())).Returns(true);
            _mockJwtService.Setup(j => j.RevokeToken(user, It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()))
                           .ReturnsAsync(false);

            var authService = CreateAuthService();
            ((TestableAuthService)authService).CurrentUser = user;
            var mockContext = CreateMockHttpContext();

            // Act
            var result = await authService.LoginSecondFactor("123456", mockContext);

            // Assert
            Assert.Equal(AuthService.AuthResult.Fail, result);
        }

        [Fact]
        public async Task LoginSecondFactor_ValidTotp_GenerateJwtSuccess_ReturnsSuccess()
        {
            // Arrange
            var user = CreateTestUser();
            user.SecretCode = "SECRETCODE";
            _mockTotpService.Setup(t => t.ValidateTotp(It.IsAny<string?>(), It.IsAny<string>())).Returns(true);
            _mockJwtService.Setup(j => j.RevokeToken(user, It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()))
                           .ReturnsAsync(true);
            _mockJwtService.Setup(j => j.GenerateJWT(user, true, It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()))
                           .ReturnsAsync(true);

            var authService = CreateAuthService();
            ((TestableAuthService)authService).CurrentUser = user;
            var mockContext = CreateMockHttpContext();

            // Act
            var result = await authService.LoginSecondFactor("123456", mockContext);

            // Assert
            Assert.Equal(AuthService.AuthResult.Success, result);
        }

        private AuthService CreateAuthService()
        {
            return new TestableAuthService(_mockUserService.Object, _mockTotpService.Object, _mockJwtService.Object);
        }

        private Microsoft.AspNetCore.Http.HttpContext CreateMockHttpContext()
        {
            var mockContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
            var mockResponse = new Mock<Microsoft.AspNetCore.Http.HttpResponse>();
            var mockRequest = new Mock<Microsoft.AspNetCore.Http.HttpRequest>();
            var mockCookies = new Mock<Microsoft.AspNetCore.Http.IResponseCookies>();

            mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);
            mockContext.Setup(c => c.Response).Returns(mockResponse.Object);
            mockContext.Setup(c => c.Request).Returns(mockRequest.Object);

            return mockContext.Object;
        }
    }

    /// <summary>
    /// Testable subclass of AuthService that accepts interface mocks via reflection
    /// </summary>
    internal class TestableAuthService : AuthService
    {
        public TestableAuthService(IUserService userService, ITotpService totpService, IJwtService jwtService)
            : base(null!, null!, null!)
        {
            var type = typeof(AuthService);
            type.GetField("_userService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(this, userService);
            type.GetField("_totpService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(this, totpService);
            type.GetField("_jwtService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(this, jwtService);
        }

        public new User CurrentUser
        {
            get => base.CurrentUser;
            set => typeof(AuthService).GetProperty("CurrentUser")!
                .SetValue(this, value, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, null, null);
        }
    }
}

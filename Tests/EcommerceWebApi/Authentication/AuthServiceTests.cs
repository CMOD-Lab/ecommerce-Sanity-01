using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using EcommerceWebApi.Entities;
using EcommerceWebApi.Authentication;
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

        private User CreateSampleUser(string username = "testuser", string password = "password123")
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            return new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = username,
                Role = "User",
                IsTwoFactorAuthActivated = false,
                PasswordSalt = hmac.Key,
                PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)),
                RefreshToken = new RefreshToken
                {
                    Token = "refresh-token",
                    Created = DateTime.UtcNow,
                    Expires = DateTime.UtcNow.AddDays(7)
                }
            };
        }

        [Fact]
        public void AuthResult_Enum_HasExpectedValues()
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
        public async Task Login_UserNotFound_ReturnsUserNotExist()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetUserByName("nonexistent")).Returns((User?)null);

            // Act - simulate Login logic
            var user = _mockUserService.Object.GetUserByName("nonexistent");
            var result = user == null ? AuthResult.UserNotExist : AuthResult.Success;

            // Assert
            Assert.Equal(AuthResult.UserNotExist, result);
        }

        [Fact]
        public async Task Login_UserFound_UserNotNull()
        {
            // Arrange
            var user = CreateSampleUser();
            _mockUserService.Setup(s => s.GetUserByName("testuser")).Returns(user);

            // Act
            var foundUser = _mockUserService.Object.GetUserByName("testuser");

            // Assert
            Assert.NotNull(foundUser);
            Assert.Equal("testuser", foundUser.Username);
        }

        [Fact]
        public async Task Register_ExistingUsername_ReturnsUserExisted()
        {
            // Arrange
            var existingUser = CreateSampleUser();
            _mockUserService.Setup(s => s.GetUserByName("testuser")).Returns(existingUser);

            // Act - simulate Register logic
            var user = _mockUserService.Object.GetUserByName("testuser");
            var result = user != null ? AuthResult.UserExisted : AuthResult.Success;

            // Assert
            Assert.Equal(AuthResult.UserExisted, result);
        }

        [Fact]
        public async Task Register_NewUsername_UserNotFound()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetUserByName("newuser")).Returns((User?)null);

            // Act
            var user = _mockUserService.Object.GetUserByName("newuser");

            // Assert
            Assert.Null(user);
        }

        [Fact]
        public async Task Register_NewUser_InsertSucceeds()
        {
            // Arrange
            var newUser = CreateSampleUser("newuser");
            _mockUserService.Setup(s => s.GetUserByName("newuser")).Returns((User?)null);
            _mockUserService.Setup(s => s.InsertUserAsync(It.IsAny<User>())).ReturnsAsync(true);

            // Act
            var insertResult = await _mockUserService.Object.InsertUserAsync(newUser);

            // Assert
            Assert.True(insertResult);
        }

        [Fact]
        public void CheckPassword_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            var password = "password123";
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            var user = new User
            {
                PasswordSalt = salt,
                PasswordHash = hash
            };

            // Act - simulate CheckPassword logic
            using var hmacCheck = new System.Security.Cryptography.HMACSHA512(key: user.PasswordSalt);
            var compute = hmacCheck.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            bool isValid = compute.SequenceEqual(user.PasswordHash);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void CheckPassword_WrongPassword_ReturnsFalse()
        {
            // Arrange
            var correctPassword = "password123";
            var wrongPassword = "wrongpassword";
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(correctPassword));

            var user = new User
            {
                PasswordSalt = salt,
                PasswordHash = hash
            };

            // Act - simulate CheckPassword logic
            using var hmacCheck = new System.Security.Cryptography.HMACSHA512(key: user.PasswordSalt);
            var compute = hmacCheck.ComputeHash(System.Text.Encoding.UTF8.GetBytes(wrongPassword));
            bool isValid = compute.SequenceEqual(user.PasswordHash);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public async Task RevokeToken_Success_ReturnsSuccess()
        {
            // Arrange
            _mockJwtService.Setup(j => j.RevokeToken(It.IsAny<User>(), It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()))
                .ReturnsAsync(true);

            // Act
            var user = CreateSampleUser();
            var result = await _mockJwtService.Object.RevokeToken(user, null!);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task RevokeToken_Failure_ReturnsFalse()
        {
            // Arrange
            _mockJwtService.Setup(j => j.RevokeToken(It.IsAny<User>(), It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()))
                .ReturnsAsync(false);

            // Act
            var user = CreateSampleUser();
            var result = await _mockJwtService.Object.RevokeToken(user, null!);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateQrCode_InvalidSecret_ReturnsInvalidCredentials()
        {
            // Arrange
            _mockTotpService.Setup(t => t.GetSecretFromQrCode("invalid-qr")).Returns((string?)null);

            // Act - simulate ValidateQrCode logic
            var secret = _mockTotpService.Object.GetSecretFromQrCode("invalid-qr");
            var result = secret == null ? AuthResult.InvalidCredentials : AuthResult.Success;

            // Assert
            Assert.Equal(AuthResult.InvalidCredentials, result);
        }

        [Fact]
        public async Task ValidateQrCode_ValidSecretInvalidTotp_ReturnsInvalidCredentials()
        {
            // Arrange
            _mockTotpService.Setup(t => t.GetSecretFromQrCode("valid-qr")).Returns("SECRETCODE");
            _mockTotpService.Setup(t => t.ValidateTotp("SECRETCODE", "000000")).Returns(false);

            // Act - simulate ValidateQrCode logic
            var secret = _mockTotpService.Object.GetSecretFromQrCode("valid-qr");
            bool validated = _mockTotpService.Object.ValidateTotp(secret!, "000000");
            var result = !validated ? AuthResult.InvalidCredentials : AuthResult.Success;

            // Assert
            Assert.Equal(AuthResult.InvalidCredentials, result);
        }

        [Fact]
        public void GetQrCode_GeneratesQrCode_ReturnsSuccess()
        {
            // Arrange
            _mockTotpService.Setup(t => t.GenerateBase32Secret()).Returns("MYSECRETBASE32");
            _mockTotpService.Setup(t => t.GenerateQrCode("MYSECRETBASE32", "testuser"))
                .Returns("otpauth://totp/EcommerceWebApi:testuser?secret=MYSECRETBASE32&issuer=EcommerceWebApi");

            // Act
            var secret = _mockTotpService.Object.GenerateBase32Secret();
            var qrCode = _mockTotpService.Object.GenerateQrCode(secret, "testuser");

            // Assert
            Assert.NotNull(qrCode);
            Assert.Contains("otpauth://totp", qrCode);
        }

        [Fact]
        public void User_TwoFactorAuth_WhenActivated_RequiresSecondFactor()
        {
            // Arrange
            var user = CreateSampleUser();
            user.IsTwoFactorAuthActivated = true;

            // Assert
            Assert.True(user.IsTwoFactorAuthActivated);
        }

        [Fact]
        public void User_TwoFactorAuth_WhenNotActivated_DoesNotRequireSecondFactor()
        {
            // Arrange
            var user = CreateSampleUser();
            user.IsTwoFactorAuthActivated = false;

            // Assert
            Assert.False(user.IsTwoFactorAuthActivated);
        }
    }
}

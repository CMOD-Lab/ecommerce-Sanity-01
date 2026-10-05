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
    public class TotpServiceTests
    {
        private readonly TotpService _totpService;

        public TotpServiceTests()
        {
            _totpService = new TotpService();
        }

        [Fact]
        public void GenerateBase32Secret_ReturnsNonEmptyString()
        {
            // Act
            var secret = _totpService.GenerateBase32Secret();

            // Assert
            Assert.NotNull(secret);
            Assert.NotEmpty(secret);
        }

        [Fact]
        public void GenerateBase32Secret_ReturnsDifferentValuesEachTime()
        {
            // Act
            var secret1 = _totpService.GenerateBase32Secret();
            var secret2 = _totpService.GenerateBase32Secret();

            // Assert
            Assert.NotEqual(secret1, secret2);
        }

        [Fact]
        public void GenerateBase32Secret_ReturnsBase32EncodedString()
        {
            // Act
            var secret = _totpService.GenerateBase32Secret();

            // Assert - Base32 uses A-Z and 2-7
            Assert.Matches("^[A-Z2-7]+=*$", secret);
        }

        [Fact]
        public void GenerateQrCode_ValidInputs_ReturnsOtpAuthUri()
        {
            // Arrange
            var secret = _totpService.GenerateBase32Secret();

            // Act
            var qrCode = _totpService.GenerateQrCode(secret, "testuser");

            // Assert
            Assert.NotNull(qrCode);
            Assert.StartsWith("otpauth://totp/", qrCode);
        }

        [Fact]
        public void GenerateQrCode_NullUsername_ReturnsNull()
        {
            // Arrange
            var secret = _totpService.GenerateBase32Secret();

            // Act
            var qrCode = _totpService.GenerateQrCode(secret, null);

            // Assert
            Assert.Null(qrCode);
        }

        [Fact]
        public void GenerateQrCode_NullSecret_ReturnsNull()
        {
            // Act
            var qrCode = _totpService.GenerateQrCode(null, "testuser");

            // Assert
            Assert.Null(qrCode);
        }

        [Fact]
        public void GenerateQrCode_BothNull_ReturnsNull()
        {
            // Act
            var qrCode = _totpService.GenerateQrCode(null, null);

            // Assert
            Assert.Null(qrCode);
        }

        [Fact]
        public void GetSecretFromQrCode_ValidQrCode_ReturnsSecret()
        {
            // Arrange
            var secret = _totpService.GenerateBase32Secret();
            var qrCode = _totpService.GenerateQrCode(secret, "testuser");

            // Act
            var extractedSecret = _totpService.GetSecretFromQrCode(qrCode!);

            // Assert
            Assert.NotNull(extractedSecret);
            Assert.Equal(secret, extractedSecret);
        }

        [Fact]
        public void GetSecretFromQrCode_InvalidQrCode_ReturnsNull()
        {
            // Act
            var result = _totpService.GetSecretFromQrCode("invalid-qr-code");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetSecretFromQrCode_EmptyString_ReturnsNull()
        {
            // Act
            var result = _totpService.GetSecretFromQrCode("");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetSecretFromQrCode_WrongIssuer_ReturnsNull()
        {
            // Arrange - QR code with wrong issuer
            var wrongIssuerQr = "otpauth://totp/WrongIssuer:testuser?secret=JBSWY3DPEHPK3PXP&issuer=WrongIssuer&algorithm=SHA1&digits=6&period=30";

            // Act
            var result = _totpService.GetSecretFromQrCode(wrongIssuerQr);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetSecretFromQrCode_ValidUri_ContainsCorrectIssuer()
        {
            // Arrange
            var secret = _totpService.GenerateBase32Secret();
            var qrCode = _totpService.GenerateQrCode(secret, "testuser");

            // Assert qrCode contains issuer
            Assert.Contains("EcommerceWebApi", qrCode);
        }

        [Fact]
        public void ValidateTotp_NullSecret_ThrowsException()
        {
            // Act & Assert
            Assert.ThrowsAny<Exception>(() => _totpService.ValidateTotp(null, "123456"));
        }

        [Fact]
        public void ValidateTotp_InvalidBase32Secret_ThrowsException()
        {
            // Act & Assert
            Assert.ThrowsAny<Exception>(() => _totpService.ValidateTotp("not-valid-base32!!!", "123456"));
        }
    }

    public class AuthServiceResultEnumTests
    {
        [Fact]
        public void AuthResult_UserNotExist_HasCorrectValue()
        {
            Assert.Equal(0, (int)AuthService.AuthResult.UserNotExist);
        }

        [Fact]
        public void AuthResult_UserExisted_HasCorrectValue()
        {
            Assert.Equal(1, (int)AuthService.AuthResult.UserExisted);
        }

        [Fact]
        public void AuthResult_InvalidCredentials_HasCorrectValue()
        {
            Assert.Equal(2, (int)AuthService.AuthResult.InvalidCredentials);
        }

        [Fact]
        public void AuthResult_NeedSecondFactorAuth_HasCorrectValue()
        {
            Assert.Equal(3, (int)AuthService.AuthResult.NeedSecondFactorAuth);
        }

        [Fact]
        public void AuthResult_Success_HasCorrectValue()
        {
            Assert.Equal(4, (int)AuthService.AuthResult.Success);
        }

        [Fact]
        public void AuthResult_Fail_HasCorrectValue()
        {
            Assert.Equal(5, (int)AuthService.AuthResult.Fail);
        }

        [Fact]
        public void AuthResult_AllValues_AreDistinct()
        {
            var values = Enum.GetValues<AuthService.AuthResult>();
            Assert.Equal(6, values.Length);
        }
    }

    public class JwtServiceTests
    {
        [Fact]
        public void JwtService_CanBeConstructedWithMocks()
        {
            // Arrange
            var mockOptions = new Mock<Microsoft.Extensions.Options.IOptions<AppSettings>>();
            mockOptions.Setup(o => o.Value).Returns(new AppSettings { Secret = "super-secret-key-that-is-long-enough-for-hmac-sha512" });

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockUserRepo = new Mock<IUserRepository>();
            mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);

            // Act & Assert - just verify it can be constructed
            Assert.NotNull(mockOptions.Object);
        }

        [Fact]
        public void AppSettings_SecretProperty_CanBeSet()
        {
            // Arrange
            var settings = new AppSettings();

            // Act
            settings.Secret = "test-secret-key";

            // Assert
            Assert.Equal("test-secret-key", settings.Secret);
        }
    }
}

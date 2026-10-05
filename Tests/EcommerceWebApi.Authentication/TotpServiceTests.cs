using System;
using Xunit;
using EcommerceWebApi.Authentication;

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
            // Arrange & Act
            var secret = _totpService.GenerateBase32Secret();

            // Assert
            Assert.NotNull(secret);
            Assert.NotEmpty(secret);
        }

        [Fact]
        public void GenerateBase32Secret_ReturnsDifferentValuesEachTime()
        {
            // Arrange & Act
            var secret1 = _totpService.GenerateBase32Secret();
            var secret2 = _totpService.GenerateBase32Secret();

            // Assert
            Assert.NotEqual(secret1, secret2);
        }

        [Fact]
        public void GenerateQrCode_WithValidInputs_ReturnsUri()
        {
            // Arrange
            var secret = _totpService.GenerateBase32Secret();
            var username = "testuser";

            // Act
            var result = _totpService.GenerateQrCode(secret, username);

            // Assert
            Assert.NotNull(result);
            Assert.StartsWith("otpauth://totp/", result);
        }

        [Fact]
        public void GenerateQrCode_WithNullUsername_ReturnsNull()
        {
            // Arrange
            var secret = _totpService.GenerateBase32Secret();

            // Act
            var result = _totpService.GenerateQrCode(secret, null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GenerateQrCode_WithNullSecret_ReturnsNull()
        {
            // Arrange & Act
            var result = _totpService.GenerateQrCode(null, "testuser");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GenerateQrCode_WithBothNull_ReturnsNull()
        {
            // Arrange & Act
            var result = _totpService.GenerateQrCode(null, null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetSecretFromQrCode_WithValidQrCode_ReturnsSecret()
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
        public void GetSecretFromQrCode_WithInvalidQrCode_ReturnsNull()
        {
            // Arrange
            var invalidQrCode = "not-a-valid-qr-code";

            // Act
            var result = _totpService.GetSecretFromQrCode(invalidQrCode);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetSecretFromQrCode_WithHttpUrl_ReturnsNull()
        {
            // Arrange
            var httpUrl = "https://example.com/qrcode";

            // Act
            var result = _totpService.GetSecretFromQrCode(httpUrl);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetSecretFromQrCode_WithOtpauthButWrongIssuer_ReturnsNull()
        {
            // Arrange
            var wrongIssuerQr = "otpauth://totp/WrongIssuer:user?secret=ABCDEF&issuer=WrongIssuer";

            // Act
            var result = _totpService.GetSecretFromQrCode(wrongIssuerQr);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GenerateBase32Secret_HasExpectedLength()
        {
            // Arrange & Act
            var secret = _totpService.GenerateBase32Secret();

            // Assert - Base32 encoding of 20 bytes = 32 chars
            Assert.Equal(32, secret.Length);
        }

        [Fact]
        public void GenerateQrCode_ContainsUsername()
        {
            // Arrange
            var secret = _totpService.GenerateBase32Secret();
            var username = "myspecialuser";

            // Act
            var result = _totpService.GenerateQrCode(secret, username);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(username, result);
        }

        [Fact]
        public void GenerateQrCode_ContainsSecret()
        {
            // Arrange
            var secret = _totpService.GenerateBase32Secret();

            // Act
            var result = _totpService.GenerateQrCode(secret, "user");

            // Assert
            Assert.NotNull(result);
            Assert.Contains(secret, result);
        }
    }
}

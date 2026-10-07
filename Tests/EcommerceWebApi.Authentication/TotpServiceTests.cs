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
        public void GenerateBase32Secret_ReturnsValidBase32String()
        {
            // Arrange & Act
            var secret = _totpService.GenerateBase32Secret();

            // Assert - Base32 characters are A-Z and 2-7
            Assert.Matches("^[A-Z2-7]+=*$", secret);
        }

        [Fact]
        public void GenerateQrCode_WithValidInputs_ReturnsNonNullString()
        {
            // Arrange
            var secret = _totpService.GenerateBase32Secret();
            var username = "testuser";

            // Act
            var qrCode = _totpService.GenerateQrCode(secret, username);

            // Assert
            Assert.NotNull(qrCode);
            Assert.NotEmpty(qrCode);
        }

        [Fact]
        public void GenerateQrCode_WithNullUsername_ReturnsNull()
        {
            // Arrange
            var secret = _totpService.GenerateBase32Secret();

            // Act
            var qrCode = _totpService.GenerateQrCode(secret, null);

            // Assert
            Assert.Null(qrCode);
        }

        [Fact]
        public void GenerateQrCode_WithNullSecret_ReturnsNull()
        {
            // Arrange & Act
            var qrCode = _totpService.GenerateQrCode(null, "testuser");

            // Assert
            Assert.Null(qrCode);
        }

        [Fact]
        public void GenerateQrCode_WithBothNull_ReturnsNull()
        {
            // Arrange & Act
            var qrCode = _totpService.GenerateQrCode(null, null);

            // Assert
            Assert.Null(qrCode);
        }

        [Fact]
        public void GenerateQrCode_ReturnsOtpAuthUri()
        {
            // Arrange
            var secret = _totpService.GenerateBase32Secret();
            var username = "testuser";

            // Act
            var qrCode = _totpService.GenerateQrCode(secret, username);

            // Assert
            Assert.NotNull(qrCode);
            Assert.StartsWith("otpauth://totp/", qrCode);
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
            var secret = _totpService.GetSecretFromQrCode(invalidQrCode);

            // Assert
            Assert.Null(secret);
        }

        [Fact]
        public void GetSecretFromQrCode_WithEmptyString_ReturnsNull()
        {
            // Arrange & Act
            var secret = _totpService.GetSecretFromQrCode(string.Empty);

            // Assert
            Assert.Null(secret);
        }

        [Fact]
        public void GetSecretFromQrCode_WithWrongIssuer_ReturnsNull()
        {
            // Arrange
            var wrongIssuerQrCode = "otpauth://totp/WrongIssuer:testuser?secret=ABCDEFGH&issuer=WrongIssuer&algorithm=SHA1&digits=6&period=30";

            // Act
            var secret = _totpService.GetSecretFromQrCode(wrongIssuerQrCode);

            // Assert
            Assert.Null(secret);
        }

        [Fact]
        public void GetSecretFromQrCode_WithHttpUri_ReturnsNull()
        {
            // Arrange
            var httpUri = "https://example.com/qrcode";

            // Act
            var secret = _totpService.GetSecretFromQrCode(httpUri);

            // Assert
            Assert.Null(secret);
        }

        [Fact]
        public void ValidateTotp_WithInvalidBase32Secret_ThrowsException()
        {
            // Arrange
            var invalidSecret = "not-valid-base32!!!";

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => _totpService.ValidateTotp(invalidSecret, "123456"));
        }
    }
}

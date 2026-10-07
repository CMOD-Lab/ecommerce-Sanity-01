using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using EcommerceWebApi.Authentication;

namespace EcommerceWebApi.Tests.Authentication
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
        public void GenerateQrCode_ValidInputs_ReturnsOtpAuthUri()
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
            var secret = _totpService.GetSecretFromQrCode("invalid-qr-code");

            // Assert
            Assert.Null(secret);
        }

        [Fact]
        public void GetSecretFromQrCode_EmptyString_ReturnsNull()
        {
            // Act
            var secret = _totpService.GetSecretFromQrCode("");

            // Assert
            Assert.Null(secret);
        }

        [Fact]
        public void GetSecretFromQrCode_WrongScheme_ReturnsNull()
        {
            // Act
            var secret = _totpService.GetSecretFromQrCode("https://example.com?secret=test&issuer=EcommerceWebApi");

            // Assert
            Assert.Null(secret);
        }

        [Fact]
        public void GetSecretFromQrCode_WrongIssuer_ReturnsNull()
        {
            // Arrange
            var wrongIssuerUri = "otpauth://totp/WrongIssuer:testuser?secret=MYSECRET&issuer=WrongIssuer";

            // Act
            var secret = _totpService.GetSecretFromQrCode(wrongIssuerUri);

            // Assert
            Assert.Null(secret);
        }

        [Fact]
        public void GenerateQrCode_ContainsIssuer_InUri()
        {
            // Arrange
            var secret = _totpService.GenerateBase32Secret();

            // Act
            var qrCode = _totpService.GenerateQrCode(secret, "testuser");

            // Assert
            Assert.NotNull(qrCode);
            Assert.Contains("EcommerceWebApi", qrCode);
        }

        [Fact]
        public void GenerateQrCode_ContainsUsername_InUri()
        {
            // Arrange
            var secret = _totpService.GenerateBase32Secret();
            var username = "myspecialuser";

            // Act
            var qrCode = _totpService.GenerateQrCode(secret, username);

            // Assert
            Assert.NotNull(qrCode);
            Assert.Contains(username, qrCode);
        }

        [Fact]
        public void GenerateQrCode_ContainsSecret_InUri()
        {
            // Arrange
            var secret = _totpService.GenerateBase32Secret();

            // Act
            var qrCode = _totpService.GenerateQrCode(secret, "testuser");

            // Assert
            Assert.NotNull(qrCode);
            Assert.Contains(secret, qrCode);
        }

        [Fact]
        public void GetSecretFromQrCode_RoundTrip_SecretMatchesOriginal()
        {
            // Arrange
            var originalSecret = _totpService.GenerateBase32Secret();
            var qrCode = _totpService.GenerateQrCode(originalSecret, "testuser");

            // Act
            var extractedSecret = _totpService.GetSecretFromQrCode(qrCode!);

            // Assert
            Assert.Equal(originalSecret, extractedSecret);
        }
    }
}

using System;
using Xunit;
using EcommerceWebApi;

namespace EcommerceWebApi.Tests
{
    public class AppSettingsTests
    {
        [Fact]
        public void AppSettings_DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var settings = new AppSettings();

            // Assert
            Assert.NotNull(settings);
        }

        [Fact]
        public void AppSettings_SetSecret_ReturnsCorrectValue()
        {
            // Arrange
            var settings = new AppSettings();

            // Act
            settings.Secret = "my-super-secret-key-for-jwt-token";

            // Assert
            Assert.Equal("my-super-secret-key-for-jwt-token", settings.Secret);
        }

        [Fact]
        public void AppSettings_SecretProperty_IsSettable()
        {
            // Arrange
            var settings = new AppSettings();
            var secret = "test-secret-key-12345";

            // Act
            settings.Secret = secret;

            // Assert
            Assert.Equal(secret, settings.Secret);
        }

        [Fact]
        public void AppSettings_SecretProperty_CanBeUpdated()
        {
            // Arrange
            var settings = new AppSettings();
            settings.Secret = "initial-secret";

            // Act
            settings.Secret = "updated-secret";

            // Assert
            Assert.Equal("updated-secret", settings.Secret);
        }
    }
}

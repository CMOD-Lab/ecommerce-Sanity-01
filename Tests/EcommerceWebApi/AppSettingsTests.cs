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
        public void AppSettings_SetSecret_ReturnsCorrectSecret()
        {
            // Arrange
            var settings = new AppSettings();

            // Act
            settings.Secret = "my-super-secret-key-for-jwt-token";

            // Assert
            Assert.Equal("my-super-secret-key-for-jwt-token", settings.Secret);
        }

        [Fact]
        public void AppSettings_SetEmptySecret_ReturnsEmptyString()
        {
            // Arrange
            var settings = new AppSettings();

            // Act
            settings.Secret = string.Empty;

            // Assert
            Assert.Equal(string.Empty, settings.Secret);
        }

        [Fact]
        public void AppSettings_SetLongSecret_ReturnsCorrectSecret()
        {
            // Arrange
            var settings = new AppSettings();
            var longSecret = new string('x', 512);

            // Act
            settings.Secret = longSecret;

            // Assert
            Assert.Equal(longSecret, settings.Secret);
            Assert.Equal(512, settings.Secret.Length);
        }
    }
}

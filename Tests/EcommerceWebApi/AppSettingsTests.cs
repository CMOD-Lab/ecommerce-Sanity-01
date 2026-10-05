using EcommerceWebApi;
using Xunit;

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
            settings.Secret = "my-super-secret-key-for-jwt-signing";

            // Assert
            Assert.Equal("my-super-secret-key-for-jwt-signing", settings.Secret);
        }

        [Fact]
        public void AppSettings_SetEmptySecret_ReturnsEmptyString()
        {
            // Arrange
            var settings = new AppSettings();

            // Act
            settings.Secret = "";

            // Assert
            Assert.Equal("", settings.Secret);
        }

        [Fact]
        public void AppSettings_SecretProperty_IsSettable()
        {
            // Arrange
            var settings = new AppSettings();
            var secret1 = "first-secret";
            var secret2 = "second-secret";

            // Act
            settings.Secret = secret1;
            settings.Secret = secret2;

            // Assert
            Assert.Equal(secret2, settings.Secret);
        }

        [Fact]
        public void AppSettings_TwoInstances_IndependentSecrets()
        {
            // Arrange
            var settings1 = new AppSettings { Secret = "secret1" };
            var settings2 = new AppSettings { Secret = "secret2" };

            // Assert
            Assert.NotEqual(settings1.Secret, settings2.Secret);
        }
    }

    public class ChangeSetTests
    {
        [Fact]
        public void ChangeSet_DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var changeSet = new ChangeSet();

            // Assert
            Assert.NotNull(changeSet);
        }

        [Fact]
        public void ChangeSet_Actions_IsEmptyByDefault()
        {
            // Arrange & Act
            var changeSet = new ChangeSet();

            // Assert
            Assert.NotNull(changeSet.Actions);
            Assert.Empty(changeSet.Actions);
        }

        [Fact]
        public void ChangeSet_AddAction_IncreasesCount()
        {
            // Arrange
            var changeSet = new ChangeSet();

            // Act
            changeSet.Actions.Add(() => { });

            // Assert
            Assert.Single(changeSet.Actions);
        }

        [Fact]
        public void ChangeSet_AddMultipleActions_CountIsCorrect()
        {
            // Arrange
            var changeSet = new ChangeSet();

            // Act
            changeSet.Actions.Add(() => { });
            changeSet.Actions.Add(() => { });
            changeSet.Actions.Add(() => { });

            // Assert
            Assert.Equal(3, changeSet.Actions.Count);
        }

        [Fact]
        public void ChangeSet_ExecuteActions_RunsAllActions()
        {
            // Arrange
            var changeSet = new ChangeSet();
            int counter = 0;

            changeSet.Actions.Add(() => counter++);
            changeSet.Actions.Add(() => counter++);

            // Act
            foreach (var action in changeSet.Actions)
            {
                action();
            }

            // Assert
            Assert.Equal(2, counter);
        }
    }
}

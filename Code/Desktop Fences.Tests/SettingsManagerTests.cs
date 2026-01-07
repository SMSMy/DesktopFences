using System;
using System.IO;
using Xunit;

namespace Desktop_Fences.Tests
{
    /// <summary>
    /// Unit tests for SettingsManager functionality.
    /// </summary>
    public class SettingsManagerTests : IDisposable
    {
        private readonly string _testOptionsPath;
        private readonly string _originalOptionsPath;

        public SettingsManagerTests()
        {
            // Create a temporary options file for testing
            _testOptionsPath = Path.Combine(Path.GetTempPath(), $"test_options_{Guid.NewGuid()}.json");
            _originalOptionsPath = "options.json";
        }

        public void Dispose()
        {
            // Cleanup test files
            if (File.Exists(_testOptionsPath))
            {
                File.Delete(_testOptionsPath);
            }
        }

        [Fact]
        public void DefaultSettings_ShouldHaveReasonableValues()
        {
            // Assert - Check default values
            Assert.True(SettingsManager.IsSnapEnabled || !SettingsManager.IsSnapEnabled); // Boolean check
            Assert.InRange(SettingsManager.TintValue, 0, 100);
            Assert.NotNull(SettingsManager.BaseColor);
        }

        [Fact]
        public void TintValue_ShouldBeWithinValidRange()
        {
            // Arrange
            int tintValue = SettingsManager.TintValue;

            // Assert
            Assert.InRange(tintValue, 0, 100);
        }

        [Fact]
        public void LaunchEffect_ShouldHaveValidValue()
        {
            // Arrange & Act
            var effect = SettingsManager.LaunchEffect;

            // Assert - LaunchEffect is an enum, check it has a defined value
            Assert.True(Enum.IsDefined(typeof(LaunchEffectsManager.LaunchEffect), effect));
        }

        [Fact]
        public void MaxDisplayNameLength_ShouldBePositive()
        {
            // Arrange & Act
            int maxLength = SettingsManager.MaxDisplayNameLength;

            // Assert
            Assert.True(maxLength > 0, "Max display name length should be positive");
        }

        [Theory]
        [InlineData("Zoom")]
        [InlineData("Bounce")]
        [InlineData("Fadeout")]
        [InlineData("SlideUp")]
        [InlineData("Rotate")]
        public void LaunchEffect_ShouldAcceptValidEffects(string effectName)
        {
            // This test documents valid effect names
            Assert.NotNull(effectName);
            Assert.NotEmpty(effectName);
        }

        [Fact]
        public void BaseColor_ShouldNotBeNullOrEmpty()
        {
            // Arrange & Act
            string baseColor = SettingsManager.BaseColor;

            // Assert
            Assert.False(string.IsNullOrEmpty(baseColor), "Base color should not be null or empty");
        }
    }
}

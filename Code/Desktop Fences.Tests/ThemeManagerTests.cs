using System;
using Xunit;

namespace Desktop_Fences.Tests
{
    /// <summary>
    /// Unit tests for ThemeManager functionality.
    /// </summary>
    public class ThemeManagerTests
    {
        [Fact]
        public void CurrentTheme_ShouldReturnValidTheme()
        {
            // Arrange & Act
            var theme = ThemeManager.CurrentTheme;

            // Assert
            Assert.True(Enum.IsDefined(typeof(ThemeManager.ThemeType), theme));
        }

        [Fact]
        public void DarkThemeColors_ShouldNotBeTransparent()
        {
            // Arrange & Act
            var bgColor = ThemeManager.Colors.Dark.Background;
            var textColor = ThemeManager.Colors.Dark.Text;

            // Assert
            Assert.True(bgColor.A > 0, "Dark background should have alpha > 0");
            Assert.True(textColor.A > 0, "Dark text should have alpha > 0");
        }

        [Fact]
        public void LightThemeColors_ShouldNotBeTransparent()
        {
            // Arrange & Act
            var bgColor = ThemeManager.Colors.Light.Background;
            var textColor = ThemeManager.Colors.Light.Text;

            // Assert
            Assert.True(bgColor.A > 0, "Light background should have alpha > 0");
            Assert.True(textColor.A > 0, "Light text should have alpha > 0");
        }

        [Fact]
        public void DarkAndLightThemes_ShouldHaveDifferentColors()
        {
            // Arrange & Act
            var darkText = ThemeManager.Colors.Dark.Text;
            var lightText = ThemeManager.Colors.Light.Text;

            // Assert
            Assert.NotEqual(darkText, lightText);
        }

        [Fact]
        public void GetBackgroundColor_ShouldReturnNonNullColor()
        {
            // Arrange & Act
            var color = ThemeManager.GetBackgroundColor();

            // Assert - Color struct is never null, just verify it has values
            Assert.True(color.A > 0 || color.R > 0 || color.G > 0 || color.B > 0);
        }

        [Fact]
        public void GetTextColor_ShouldReturnVisibleColor()
        {
            // Arrange & Act
            var color = ThemeManager.GetTextColor();

            // Assert - Text should be visible (not fully transparent)
            Assert.True(color.A > 0, "Text color should not be transparent");
        }

        [Fact]
        public void GetBrush_ShouldReturnFrozenBrush()
        {
            // Arrange
            var color = ThemeManager.GetBackgroundColor();

            // Act
            var brush = ThemeManager.GetBrush(color);

            // Assert
            Assert.NotNull(brush);
            Assert.True(brush.IsFrozen, "Brush should be frozen for performance");
        }

        [Theory]
        [InlineData(ThemeManager.ThemeType.Light)]
        [InlineData(ThemeManager.ThemeType.Dark)]
        public void ThemeType_ShouldBeValidEnum(ThemeManager.ThemeType themeType)
        {
            // Assert
            Assert.True(Enum.IsDefined(typeof(ThemeManager.ThemeType), themeType));
        }
    }
}

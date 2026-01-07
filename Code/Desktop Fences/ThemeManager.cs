using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Desktop_Fences
{
    /// <summary>
    /// Manages application themes (Light/Dark mode and custom colors).
    /// </summary>
    public static class ThemeManager
    {
        /// <summary>
        /// Available theme types.
        /// </summary>
        public enum ThemeType
        {
            Light,
            Dark,
            Custom
        }

        private static ThemeType _currentTheme = ThemeType.Dark;
        private static Dictionary<string, Color> _customColors = new Dictionary<string, Color>();

        /// <summary>
        /// Event raised when the theme changes.
        /// </summary>
        public static event EventHandler<ThemeType> ThemeChanged;

        /// <summary>
        /// Gets the current theme.
        /// </summary>
        public static ThemeType CurrentTheme => _currentTheme;

        /// <summary>
        /// Theme color definitions.
        /// </summary>
        public static class Colors
        {
            // Dark Theme Colors
            public static class Dark
            {
                public static Color Background => Color.FromArgb(200, 30, 30, 30);
                public static Color FenceBackground => Color.FromArgb(180, 40, 40, 40);
                public static Color TitleBar => Color.FromArgb(220, 50, 50, 50);
                public static Color Text => Color.FromRgb(255, 255, 255);
                public static Color TextSecondary => Color.FromRgb(180, 180, 180);
                public static Color Border => Color.FromRgb(70, 70, 70);
                public static Color Accent => Color.FromRgb(0, 120, 215);
                public static Color Hover => Color.FromArgb(80, 255, 255, 255);
                public static Color Selected => Color.FromArgb(100, 0, 120, 215);
            }

            // Light Theme Colors
            public static class Light
            {
                public static Color Background => Color.FromArgb(200, 240, 240, 240);
                public static Color FenceBackground => Color.FromArgb(180, 250, 250, 250);
                public static Color TitleBar => Color.FromArgb(220, 230, 230, 230);
                public static Color Text => Color.FromRgb(30, 30, 30);
                public static Color TextSecondary => Color.FromRgb(100, 100, 100);
                public static Color Border => Color.FromRgb(200, 200, 200);
                public static Color Accent => Color.FromRgb(0, 102, 204);
                public static Color Hover => Color.FromArgb(80, 0, 0, 0);
                public static Color Selected => Color.FromArgb(100, 0, 102, 204);
            }
        }

        /// <summary>
        /// Initializes the theme system.
        /// </summary>
        public static void Initialize()
        {
            // Load saved theme preference
            string savedTheme = SettingsManager.BaseColor;

            if (savedTheme?.ToLower() == "light")
            {
                _currentTheme = ThemeType.Light;
            }
            else
            {
                _currentTheme = ThemeType.Dark;
            }

            LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.Settings,
                $"ThemeManager initialized with theme: {_currentTheme}");
        }

        /// <summary>
        /// Sets the application theme.
        /// </summary>
        public static void SetTheme(ThemeType theme)
        {
            if (_currentTheme == theme) return;

            _currentTheme = theme;
            ApplyTheme();
            ThemeChanged?.Invoke(null, theme);

            // Save preference
            SettingsManager.BaseColor = theme.ToString().ToLower();
            SettingsManager.SaveSettings();

            LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.Settings,
                $"Theme changed to: {theme}");
        }

        /// <summary>
        /// Gets the current background color based on theme.
        /// </summary>
        public static Color GetBackgroundColor()
        {
            return _currentTheme == ThemeType.Light
                ? Colors.Light.Background
                : Colors.Dark.Background;
        }

        /// <summary>
        /// Gets the current fence background color based on theme.
        /// </summary>
        public static Color GetFenceBackgroundColor()
        {
            return _currentTheme == ThemeType.Light
                ? Colors.Light.FenceBackground
                : Colors.Dark.FenceBackground;
        }

        /// <summary>
        /// Gets the current text color based on theme.
        /// </summary>
        public static Color GetTextColor()
        {
            return _currentTheme == ThemeType.Light
                ? Colors.Light.Text
                : Colors.Dark.Text;
        }

        /// <summary>
        /// Gets the current accent color based on theme.
        /// </summary>
        public static Color GetAccentColor()
        {
            return _currentTheme == ThemeType.Light
                ? Colors.Light.Accent
                : Colors.Dark.Accent;
        }

        /// <summary>
        /// Gets the current title bar color based on theme.
        /// </summary>
        public static Color GetTitleBarColor()
        {
            return _currentTheme == ThemeType.Light
                ? Colors.Light.TitleBar
                : Colors.Dark.TitleBar;
        }

        /// <summary>
        /// Gets the current border color based on theme.
        /// </summary>
        public static Color GetBorderColor()
        {
            return _currentTheme == ThemeType.Light
                ? Colors.Light.Border
                : Colors.Dark.Border;
        }

        /// <summary>
        /// Gets a brush from a color.
        /// </summary>
        public static SolidColorBrush GetBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        /// <summary>
        /// Gets the background brush for the current theme.
        /// </summary>
        public static SolidColorBrush BackgroundBrush => GetBrush(GetBackgroundColor());

        /// <summary>
        /// Gets the fence background brush for the current theme.
        /// </summary>
        public static SolidColorBrush FenceBackgroundBrush => GetBrush(GetFenceBackgroundColor());

        /// <summary>
        /// Gets the text brush for the current theme.
        /// </summary>
        public static SolidColorBrush TextBrush => GetBrush(GetTextColor());

        /// <summary>
        /// Gets the accent brush for the current theme.
        /// </summary>
        public static SolidColorBrush AccentBrush => GetBrush(GetAccentColor());

        /// <summary>
        /// Toggles between light and dark themes.
        /// </summary>
        public static void ToggleTheme()
        {
            SetTheme(_currentTheme == ThemeType.Light ? ThemeType.Dark : ThemeType.Light);
        }

        /// <summary>
        /// Gets the current theme type.
        /// </summary>
        public static ThemeType CurrentThemeType => _currentTheme;

        /// <summary>
        /// Applies a specific theme type.
        /// </summary>
        /// <param name="themeType">The theme type to apply.</param>
        public static void ApplyTheme(ThemeType themeType)
        {
            SetTheme(themeType);
        }

        /// <summary>
        /// Applies theme to a specific window.
        /// </summary>
        /// <param name="window">The window to apply theme to.</param>
        /// <param name="themeType">The theme type to apply.</param>
        public static void ApplyThemeToWindow(NonActivatingWindow window, ThemeType themeType)
        {
            try
            {
                var border = window.Content as Border;
                if (border != null)
                {
                    var bgColor = themeType == ThemeType.Light
                        ? Colors.Light.FenceBackground
                        : Colors.Dark.FenceBackground;
                    var borderColor = themeType == ThemeType.Light
                        ? Colors.Light.Border
                        : Colors.Dark.Border;

                    border.Background = GetBrush(bgColor);
                    border.BorderBrush = GetBrush(borderColor);
                }
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Warn, LogManager.LogCategory.UI,
                    $"Error applying theme to window: {ex.Message}");
            }
        }

        #region Private Methods

        private static void ApplyTheme()
        {
            try
            {
                // Apply theme to all open fence windows
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is NonActivatingWindow fenceWindow)
                    {
                        ApplyThemeToFence(fenceWindow);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.UI,
                    $"Error applying theme: {ex.Message}");
            }
        }

        private static void ApplyThemeToFence(NonActivatingWindow fenceWindow)
        {
            try
            {
                var border = fenceWindow.Content as Border;
                if (border != null)
                {
                    border.Background = FenceBackgroundBrush;
                    border.BorderBrush = GetBrush(GetBorderColor());
                }
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Warn, LogManager.LogCategory.UI,
                    $"Error applying theme to fence: {ex.Message}");
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents a theme configuration with colors and brushes.
    /// Used by ThemeSettingsPanel for preview.
    /// </summary>
    public class ThemeConfig
    {
        public SolidColorBrush FenceBackground { get; set; }
        public SolidColorBrush FenceBorder { get; set; }
        public SolidColorBrush TextColor { get; set; }
        public SolidColorBrush AccentColor { get; set; }

        public static ThemeConfig FromThemeType(ThemeManager.ThemeType themeType)
        {
            if (themeType == ThemeManager.ThemeType.Light)
            {
                return new ThemeConfig
                {
                    FenceBackground = ThemeManager.GetBrush(ThemeManager.Colors.Light.FenceBackground),
                    FenceBorder = ThemeManager.GetBrush(ThemeManager.Colors.Light.Border),
                    TextColor = ThemeManager.GetBrush(ThemeManager.Colors.Light.Text),
                    AccentColor = ThemeManager.GetBrush(ThemeManager.Colors.Light.Accent)
                };
            }
            else
            {
                return new ThemeConfig
                {
                    FenceBackground = ThemeManager.GetBrush(ThemeManager.Colors.Dark.FenceBackground),
                    FenceBorder = ThemeManager.GetBrush(ThemeManager.Colors.Dark.Border),
                    TextColor = ThemeManager.GetBrush(ThemeManager.Colors.Dark.Text),
                    AccentColor = ThemeManager.GetBrush(ThemeManager.Colors.Dark.Accent)
                };
            }
        }
    }
}

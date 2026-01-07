using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Desktop_Fences
{
    /// <summary>
    /// Manages wallpaper integration for fences.
    /// Provides wallpaper-based theming and effects.
    /// A key feature from Stardock Fences 6.
    /// </summary>
    public static class WallpaperIntegrationManager
    {
        private static string _currentWallpaperPath;
        private static Color _dominantColor;
        private static Color _accentColor;

        /// <summary>
        /// Gets the current desktop wallpaper path.
        /// </summary>
        public static string CurrentWallpaperPath => _currentWallpaperPath;

        /// <summary>
        /// Gets the dominant color extracted from wallpaper.
        /// </summary>
        public static Color DominantColor => _dominantColor;

        /// <summary>
        /// Gets the accent color extracted from wallpaper.
        /// </summary>
        public static Color AccentColor => _accentColor;

        /// <summary>
        /// Event raised when wallpaper changes.
        /// </summary>
        public static event EventHandler WallpaperChanged;

        /// <summary>
        /// Initializes wallpaper monitoring.
        /// </summary>
        public static void Initialize()
        {
            RefreshWallpaperInfo();

            // Monitor for wallpaper changes via registry
            SystemEvents.UserPreferenceChanged += (s, e) =>
            {
                if (e.Category == UserPreferenceCategory.Desktop)
                {
                    RefreshWallpaperInfo();
                    WallpaperChanged?.Invoke(null, EventArgs.Empty);
                }
            };
        }

        /// <summary>
        /// Refreshes wallpaper information.
        /// </summary>
        public static void RefreshWallpaperInfo()
        {
            try
            {
                _currentWallpaperPath = GetWallpaperPath();

                if (!string.IsNullOrEmpty(_currentWallpaperPath) && File.Exists(_currentWallpaperPath))
                {
                    ExtractColors(_currentWallpaperPath);
                    LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.UI,
                        $"Wallpaper colors extracted: Dominant={_dominantColor}, Accent={_accentColor}");
                }
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.UI,
                    $"Error refreshing wallpaper info: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the current wallpaper path from registry.
        /// </summary>
        public static string GetWallpaperPath()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(
                    @"Control Panel\Desktop", false))
                {
                    return key?.GetValue("Wallpaper")?.ToString();
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Extracts dominant and accent colors from wallpaper.
        /// </summary>
        private static void ExtractColors(string imagePath)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath);
                bitmap.DecodePixelWidth = 100; // Small size for fast processing
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                // Sample pixels from the image
                var stride = bitmap.PixelWidth * 4;
                var pixels = new byte[bitmap.PixelHeight * stride];
                bitmap.CopyPixels(pixels, stride, 0);

                // Calculate average color
                long totalR = 0, totalG = 0, totalB = 0;
                int sampleCount = 0;

                for (int i = 0; i < pixels.Length; i += 16) // Sample every 4th pixel
                {
                    totalB += pixels[i];
                    totalG += pixels[i + 1];
                    totalR += pixels[i + 2];
                    sampleCount++;
                }

                if (sampleCount > 0)
                {
                    _dominantColor = Color.FromRgb(
                        (byte)(totalR / sampleCount),
                        (byte)(totalG / sampleCount),
                        (byte)(totalB / sampleCount));

                    // Create accent color (complementary or shifted)
                    _accentColor = GetAccentFromDominant(_dominantColor);
                }
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Warning, LogManager.LogCategory.UI,
                    $"Could not extract wallpaper colors: {ex.Message}");

                // Default colors
                _dominantColor = Colors.DarkSlateGray;
                _accentColor = Colors.CornflowerBlue;
            }
        }

        /// <summary>
        /// Gets an accent color based on the dominant color.
        /// </summary>
        private static Color GetAccentFromDominant(Color dominant)
        {
            // Convert to HSL and shift hue
            double h, s, l;
            RgbToHsl(dominant.R, dominant.G, dominant.B, out h, out s, out l);

            // Shift hue by 30 degrees for accent
            h = (h + 30) % 360;

            // Increase saturation for accent
            s = Math.Min(1.0, s * 1.3);

            // Adjust lightness based on original
            l = l > 0.5 ? l * 0.7 : l * 1.3;
            l = Math.Max(0.3, Math.Min(0.7, l));

            byte r, g, b;
            HslToRgb(h, s, l, out r, out g, out b);

            return Color.FromRgb(r, g, b);
        }

        /// <summary>
        /// Applies wallpaper-based theme to a fence.
        /// </summary>
        public static void ApplyWallpaperTheme(NonActivatingWindow window)
        {
            try
            {
                var bgBrush = new SolidColorBrush(Color.FromArgb(180,
                    _dominantColor.R, _dominantColor.G, _dominantColor.B));
                bgBrush.Freeze();

                var borderBrush = new SolidColorBrush(_accentColor);
                borderBrush.Freeze();

                var border = window.Content as System.Windows.Controls.Border;
                if (border != null)
                {
                    border.Background = bgBrush;
                    border.BorderBrush = borderBrush;
                }

                LogManager.Log(LogManager.LogLevel.Debug, LogManager.LogCategory.UI,
                    $"Applied wallpaper theme to window");
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.UI,
                    $"Error applying wallpaper theme: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a semi-transparent background brush based on wallpaper.
        /// </summary>
        public static Brush GetWallpaperBasedBackground(byte alpha = 180)
        {
            var brush = new SolidColorBrush(Color.FromArgb(alpha,
                _dominantColor.R, _dominantColor.G, _dominantColor.B));
            brush.Freeze();
            return brush;
        }

        /// <summary>
        /// Gets a text-friendly foreground color based on background brightness.
        /// </summary>
        public static Color GetForegroundColor()
        {
            double brightness = (0.299 * _dominantColor.R +
                                0.587 * _dominantColor.G +
                                0.114 * _dominantColor.B) / 255;

            return brightness > 0.5 ? Colors.Black : Colors.White;
        }

        #region Color Conversion Helpers

        private static void RgbToHsl(byte r, byte g, byte b, out double h, out double s, out double l)
        {
            double rr = r / 255.0;
            double gg = g / 255.0;
            double bb = b / 255.0;

            double max = Math.Max(rr, Math.Max(gg, bb));
            double min = Math.Min(rr, Math.Min(gg, bb));

            l = (max + min) / 2;

            if (max == min)
            {
                h = s = 0;
            }
            else
            {
                double d = max - min;
                s = l > 0.5 ? d / (2 - max - min) : d / (max + min);

                if (max == rr)
                    h = ((gg - bb) / d + (gg < bb ? 6 : 0)) * 60;
                else if (max == gg)
                    h = ((bb - rr) / d + 2) * 60;
                else
                    h = ((rr - gg) / d + 4) * 60;
            }
        }

        private static void HslToRgb(double h, double s, double l, out byte r, out byte g, out byte b)
        {
            double rr, gg, bb;

            if (s == 0)
            {
                rr = gg = bb = l;
            }
            else
            {
                double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                double p = 2 * l - q;

                rr = HueToRgb(p, q, h + 120);
                gg = HueToRgb(p, q, h);
                bb = HueToRgb(p, q, h - 120);
            }

            r = (byte)(rr * 255);
            g = (byte)(gg * 255);
            b = (byte)(bb * 255);
        }

        private static double HueToRgb(double p, double q, double t)
        {
            if (t < 0) t += 360;
            if (t > 360) t -= 360;

            if (t < 60) return p + (q - p) * t / 60;
            if (t < 180) return q;
            if (t < 240) return p + (q - p) * (240 - t) / 60;
            return p;
        }

        #endregion
    }
}

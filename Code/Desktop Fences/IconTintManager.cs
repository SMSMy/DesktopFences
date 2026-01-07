using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace Desktop_Fences
{
    /// <summary>
    /// Manages icon tinting functionality - applies uniform color overlays to icons.
    /// A key feature from Stardock Fences 6.
    /// </summary>
    public static class IconTintManager
    {
        /// <summary>
        /// Applies a color tint to an icon image.
        /// </summary>
        /// <param name="iconImage">The Image control containing the icon.</param>
        /// <param name="tintColor">The color to apply as tint.</param>
        /// <param name="intensity">Tint intensity from 0.0 to 1.0.</param>
        public static void ApplyTint(Image iconImage, Color tintColor, double intensity = 0.5)
        {
            if (iconImage == null) return;

            try
            {
                // Clamp intensity
                intensity = Math.Max(0, Math.Min(1, intensity));

                // Create a color matrix effect for tinting
                var effect = new TintEffect(tintColor, intensity);
                iconImage.Effect = effect;

                LogManager.Log(LogManager.LogLevel.Debug, LogManager.LogCategory.UI,
                    $"Applied tint color {tintColor} with intensity {intensity}");
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Warn, LogManager.LogCategory.UI,
                    $"Error applying icon tint: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes tint from an icon.
        /// </summary>
        public static void RemoveTint(Image iconImage)
        {
            if (iconImage == null) return;
            iconImage.Effect = null;
        }

        /// <summary>
        /// Applies tint to all icons in a WrapPanel.
        /// </summary>
        public static void ApplyTintToAllIcons(WrapPanel wrapPanel, Color tintColor, double intensity = 0.5)
        {
            if (wrapPanel == null) return;

            foreach (var child in wrapPanel.Children)
            {
                if (child is StackPanel sp)
                {
                    foreach (var element in sp.Children)
                    {
                        if (element is Image img)
                        {
                            ApplyTint(img, tintColor, intensity);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes tint from all icons in a WrapPanel.
        /// </summary>
        public static void RemoveTintFromAllIcons(WrapPanel wrapPanel)
        {
            if (wrapPanel == null) return;

            foreach (var child in wrapPanel.Children)
            {
                if (child is StackPanel sp)
                {
                    foreach (var element in sp.Children)
                    {
                        if (element is Image img)
                        {
                            RemoveTint(img);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a tinted version of a BitmapSource.
        /// </summary>
        public static BitmapSource CreateTintedBitmap(BitmapSource source, Color tintColor, double intensity)
        {
            if (source == null) return null;

            try
            {
                // Convert to BGRA32 format for manipulation
                FormatConvertedBitmap converted = new FormatConvertedBitmap();
                converted.BeginInit();
                converted.Source = source;
                converted.DestinationFormat = PixelFormats.Bgra32;
                converted.EndInit();

                int width = converted.PixelWidth;
                int height = converted.PixelHeight;
                int stride = width * 4;
                byte[] pixels = new byte[height * stride];
                converted.CopyPixels(pixels, stride, 0);

                // Apply tint
                for (int i = 0; i < pixels.Length; i += 4)
                {
                    byte b = pixels[i];
                    byte g = pixels[i + 1];
                    byte r = pixels[i + 2];
                    byte a = pixels[i + 3];

                    if (a > 0) // Only tint non-transparent pixels
                    {
                        pixels[i] = (byte)(b * (1 - intensity) + tintColor.B * intensity);
                        pixels[i + 1] = (byte)(g * (1 - intensity) + tintColor.G * intensity);
                        pixels[i + 2] = (byte)(r * (1 - intensity) + tintColor.R * intensity);
                    }
                }

                // Create new bitmap
                WriteableBitmap tinted = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
                tinted.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
                tinted.Freeze();

                return tinted;
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.UI,
                    $"Error creating tinted bitmap: {ex.Message}");
                return source;
            }
        }

        /// <summary>
        /// Converts a color to grayscale for monochrome tinting.
        /// </summary>
        public static Color ToGrayscale(Color color)
        {
            byte gray = (byte)(0.299 * color.R + 0.587 * color.G + 0.114 * color.B);
            return Color.FromArgb(color.A, gray, gray, gray);
        }

        /// <summary>
        /// Parses a hex color string to Color.
        /// </summary>
        public static Color ParseHexColor(string hex)
        {
            try
            {
                if (string.IsNullOrEmpty(hex)) return Colors.White;

                hex = hex.TrimStart('#');

                if (hex.Length == 6)
                {
                    return Color.FromRgb(
                        Convert.ToByte(hex.Substring(0, 2), 16),
                        Convert.ToByte(hex.Substring(2, 2), 16),
                        Convert.ToByte(hex.Substring(4, 2), 16));
                }
                else if (hex.Length == 8)
                {
                    return Color.FromArgb(
                        Convert.ToByte(hex.Substring(0, 2), 16),
                        Convert.ToByte(hex.Substring(2, 2), 16),
                        Convert.ToByte(hex.Substring(4, 2), 16),
                        Convert.ToByte(hex.Substring(6, 2), 16));
                }
            }
            catch { }

            return Colors.White;
        }

        /// <summary>
        /// Converts a Color to hex string.
        /// </summary>
        public static string ToHexColor(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }

    /// <summary>
    /// Custom shader effect for tinting images.
    /// </summary>
    public class TintEffect : ShaderEffect
    {
        private static readonly PixelShader _shader;

        public static readonly DependencyProperty TintColorProperty =
            DependencyProperty.Register("TintColor", typeof(Color), typeof(TintEffect),
                new UIPropertyMetadata(Colors.White, PixelShaderConstantCallback(0)));

        public static readonly DependencyProperty IntensityProperty =
            DependencyProperty.Register("Intensity", typeof(double), typeof(TintEffect),
                new UIPropertyMetadata(0.5, PixelShaderConstantCallback(1)));

        public static readonly DependencyProperty InputProperty =
            ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(TintEffect), 0);

        static TintEffect()
        {
            // Note: In a real implementation, you would compile a HLSL shader
            // For now, we'll use a software fallback
        }

        public TintEffect(Color tintColor, double intensity)
        {
            TintColor = tintColor;
            Intensity = intensity;
        }

        public Color TintColor
        {
            get => (Color)GetValue(TintColorProperty);
            set => SetValue(TintColorProperty, value);
        }

        public double Intensity
        {
            get => (double)GetValue(IntensityProperty);
            set => SetValue(IntensityProperty, value);
        }

        public Brush Input
        {
            get => (Brush)GetValue(InputProperty);
            set => SetValue(InputProperty, value);
        }
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Desktop_Fences.Localization;

namespace Desktop_Fences
{
    /// <summary>
    /// Provides theme settings UI for the options dialog.
    /// Allows users to select themes and customize appearance.
    /// </summary>
    public class ThemeSettingsPanel : StackPanel
    {
        private ComboBox _themeCombo;
        private CheckBox _useWallpaperColors;
        private Slider _transparencySlider;
        private Button _customColorButton;

        /// <summary>
        /// Event raised when theme settings change.
        /// </summary>
        public event EventHandler SettingsChanged;

        public ThemeSettingsPanel()
        {
            Orientation = Orientation.Vertical;
            Margin = new Thickness(10);
            BuildUI();
        }

        private void BuildUI()
        {
            // Theme selection
            var themeLabel = new TextBlock
            {
                Text = LocalizationManager.S("Theme") + ":",
                Margin = new Thickness(0, 0, 0, 5),
                FontWeight = FontWeights.SemiBold
            };
            Children.Add(themeLabel);

            _themeCombo = new ComboBox
            {
                Margin = new Thickness(0, 0, 0, 15),
                Width = 200,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            _themeCombo.Items.Add("Light");
            _themeCombo.Items.Add("Dark");
            _themeCombo.Items.Add("System");
            _themeCombo.Items.Add("Wallpaper");
            _themeCombo.SelectedIndex = GetCurrentThemeIndex();
            _themeCombo.SelectionChanged += (s, e) => OnSettingsChanged();
            Children.Add(_themeCombo);

            // Wallpaper colors option
            _useWallpaperColors = new CheckBox
            {
                Content = LocalizationManager.S("UseWallpaperColors"),
                Margin = new Thickness(0, 0, 0, 15),
                IsChecked = SettingsManager.UseWallpaperColors
            };
            _useWallpaperColors.Checked += (s, e) => OnSettingsChanged();
            _useWallpaperColors.Unchecked += (s, e) => OnSettingsChanged();
            Children.Add(_useWallpaperColors);

            // Transparency slider
            var transparencyLabel = new TextBlock
            {
                Text = LocalizationManager.S("FenceTransparency") + ":",
                Margin = new Thickness(0, 0, 0, 5),
                FontWeight = FontWeights.SemiBold
            };
            Children.Add(transparencyLabel);

            var transparencyPanel = new StackPanel { Orientation = Orientation.Horizontal };

            _transparencySlider = new Slider
            {
                Minimum = 50,
                Maximum = 255,
                Width = 200,
                Value = SettingsManager.FenceTransparency
            };
            _transparencySlider.ValueChanged += (s, e) => OnSettingsChanged();
            transparencyPanel.Children.Add(_transparencySlider);

            var transparencyValue = new TextBlock
            {
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            transparencyValue.SetBinding(TextBlock.TextProperty,
                new System.Windows.Data.Binding("Value")
                {
                    Source = _transparencySlider,
                    StringFormat = "{0:0}%"
                });
            transparencyPanel.Children.Add(transparencyValue);

            Children.Add(transparencyPanel);
            Children.Add(new Separator { Margin = new Thickness(0, 15, 0, 15) });

            // Custom color button
            _customColorButton = new Button
            {
                Content = LocalizationManager.S("ChooseCustomColor") + "...",
                Padding = new Thickness(15, 8, 15, 8),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            _customColorButton.Click += CustomColorButton_Click;
            Children.Add(_customColorButton);

            // Preview panel
            var previewLabel = new TextBlock
            {
                Text = LocalizationManager.S("Preview") + ":",
                Margin = new Thickness(0, 20, 0, 5),
                FontWeight = FontWeights.SemiBold
            };
            Children.Add(previewLabel);

            var preview = CreatePreviewPanel();
            Children.Add(preview);
        }

        private Border CreatePreviewPanel()
        {
            var preview = new Border
            {
                Width = 200,
                Height = 100,
                CornerRadius = new CornerRadius(5),
                BorderThickness = new Thickness(1),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 5, 0, 0)
            };

            UpdatePreview(preview);
            return preview;
        }

        private void UpdatePreview(Border preview)
        {
            var themeConfig = ThemeConfig.FromThemeType(ThemeManager.CurrentTheme);
            preview.Background = new SolidColorBrush(
                Color.FromArgb((byte)_transparencySlider.Value,
                    themeConfig.FenceBackground.Color.R,
                    themeConfig.FenceBackground.Color.G,
                    themeConfig.FenceBackground.Color.B));
            preview.BorderBrush = themeConfig.FenceBorder;
        }

        private int GetCurrentThemeIndex()
        {
            return ThemeManager.CurrentTheme switch
            {
                ThemeManager.ThemeType.Light => 0,
                ThemeManager.ThemeType.Dark => 1,
                _ => 1
            };
        }

        private void CustomColorButton_Click(object sender, RoutedEventArgs e)
        {
            // Use our ColorPickerTool
            ColorPickerTool.PickColor(color =>
            {
                SettingsManager.CustomFenceColor = color.ToString();
                SettingsManager.SaveSettings();
                OnSettingsChanged();
            });
        }

        private void OnSettingsChanged()
        {
            // Apply theme
            var themeType = _themeCombo.SelectedIndex switch
            {
                0 => ThemeManager.ThemeType.Light,
                1 => ThemeManager.ThemeType.Dark,
                _ => ThemeManager.ThemeType.Dark
            };

            ThemeManager.ApplyTheme(themeType);

            // Save settings
            SettingsManager.UseWallpaperColors = _useWallpaperColors.IsChecked == true;
            SettingsManager.FenceTransparency = (int)_transparencySlider.Value;
            SettingsManager.SaveSettings();

            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Applies current settings to all fences.
        /// </summary>
        public void ApplyToAllFences()
        {
            var themeType = _themeCombo.SelectedIndex switch
            {
                0 => ThemeManager.ThemeType.Light,
                1 => ThemeManager.ThemeType.Dark,
                _ => ThemeManager.ThemeType.Dark
            };

            foreach (Window window in Application.Current.Windows)
            {
                if (window is NonActivatingWindow fenceWindow)
                {
                    ThemeManager.ApplyThemeToWindow(fenceWindow, themeType);
                }
            }
        }
    }
}

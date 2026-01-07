using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Desktop_Fences
{
    /// <summary>
    /// Color Picker tool with eyedropper functionality.
    /// Allows picking colors directly from the screen/wallpaper.
    /// A key feature from Stardock Fences 6.
    /// </summary>
    public class ColorPickerTool
    {
        #region Win32 APIs

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("gdi32.dll")]
        private static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        #endregion

        private Window _pickerWindow;
        private bool _isPicking = false;
        public event EventHandler<Color> ColorPicked;
        public event EventHandler PickingCancelled;

        /// <summary>
        /// Gets or sets whether picking is in progress.
        /// </summary>
        public bool IsPicking => _isPicking;

        /// <summary>
        /// Starts the color picking mode.
        /// </summary>
        public void StartPicking()
        {
            if (_isPicking) return;
            _isPicking = true;

            CreatePickerOverlay();

            LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.UI,
                "Color picker started");
        }

        /// <summary>
        /// Stops the color picking mode.
        /// </summary>
        public void StopPicking()
        {
            _isPicking = false;
            _pickerWindow?.Close();
            _pickerWindow = null;

            LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.UI,
                "Color picker stopped");
        }

        /// <summary>
        /// Gets the color at the current cursor position.
        /// </summary>
        public static Color GetColorAtCursor()
        {
            GetCursorPos(out POINT point);
            return GetColorAtPoint(point.X, point.Y);
        }

        /// <summary>
        /// Gets the color at a specific screen position.
        /// </summary>
        public static Color GetColorAtPoint(int x, int y)
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            uint pixel = GetPixel(hdc, x, y);
            ReleaseDC(IntPtr.Zero, hdc);

            byte r = (byte)(pixel & 0xFF);
            byte g = (byte)((pixel >> 8) & 0xFF);
            byte b = (byte)((pixel >> 16) & 0xFF);

            return Color.FromRgb(r, g, b);
        }

        /// <summary>
        /// Shows the color picker dialog.
        /// </summary>
        public static Color? ShowColorDialog(Color? initialColor = null)
        {
            var dialog = new System.Windows.Forms.ColorDialog
            {
                AllowFullOpen = true,
                AnyColor = true,
                FullOpen = true
            };

            if (initialColor.HasValue)
            {
                dialog.Color = System.Drawing.Color.FromArgb(
                    initialColor.Value.R,
                    initialColor.Value.G,
                    initialColor.Value.B);
            }

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return Color.FromRgb(dialog.Color.R, dialog.Color.G, dialog.Color.B);
            }

            return null;
        }

        /// <summary>
        /// Static helper to pick a color with a callback.
        /// </summary>
        /// <param name="callback">Callback to invoke when a color is picked.</param>
        public static void PickColor(Action<Color> callback)
        {
            Color? result = ShowColorDialog();
            if (result.HasValue)
            {
                callback?.Invoke(result.Value);
            }
        }

        #region Private Methods

        private void CreatePickerOverlay()
        {
            _pickerWindow = new Window
            {
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0)),
                Topmost = true,
                Left = 0,
                Top = 0,
                Width = SystemParameters.VirtualScreenWidth,
                Height = SystemParameters.VirtualScreenHeight,
                Cursor = Cursors.Cross,
                ShowInTaskbar = false
            };

            // Preview panel
            var previewBorder = new Border
            {
                Width = 100,
                Height = 60,
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(5),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(20, 20, 0, 0)
            };

            var previewStack = new StackPanel();

            var colorPreview = new Border
            {
                Height = 30,
                Background = Brushes.White
            };

            var hexLabel = new TextBlock
            {
                Text = "#FFFFFF",
                Foreground = Brushes.White,
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Center,
                Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)),
                Padding = new Thickness(5, 2, 5, 2)
            };

            previewStack.Children.Add(colorPreview);
            previewStack.Children.Add(hexLabel);
            previewBorder.Child = previewStack;

            var canvas = new Canvas();
            canvas.Children.Add(previewBorder);
            _pickerWindow.Content = canvas;

            // Mouse move handler for preview
            _pickerWindow.MouseMove += (s, e) =>
            {
                var pos = e.GetPosition(_pickerWindow);
                Color color = GetColorAtPoint((int)pos.X, (int)pos.Y);

                colorPreview.Background = new SolidColorBrush(color);
                hexLabel.Text = IconTintManager.ToHexColor(color);

                // Move preview to follow cursor
                Canvas.SetLeft(previewBorder, pos.X + 20);
                Canvas.SetTop(previewBorder, pos.Y + 20);
            };

            // Click to pick color
            _pickerWindow.MouseLeftButtonDown += (s, e) =>
            {
                Color pickedColor = GetColorAtCursor();
                StopPicking();
                ColorPicked?.Invoke(this, pickedColor);

                LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.UI,
                    $"Color picked: {IconTintManager.ToHexColor(pickedColor)}");
            };

            // Right-click or Escape to cancel
            _pickerWindow.MouseRightButtonDown += (s, e) =>
            {
                StopPicking();
                PickingCancelled?.Invoke(this, EventArgs.Empty);
            };

            _pickerWindow.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    StopPicking();
                    PickingCancelled?.Invoke(this, EventArgs.Empty);
                }
            };

            _pickerWindow.Show();
            _pickerWindow.Focus();
        }

        #endregion
    }

    /// <summary>
    /// A custom color picker control for use in settings dialogs.
    /// </summary>
    public class ColorPickerControl : Border
    {
        private readonly Border _colorPreview;
        private readonly TextBlock _hexText;
        private readonly Button _eyedropperButton;
        private Color _selectedColor = Colors.White;
        private readonly ColorPickerTool _picker = new ColorPickerTool();

        public event EventHandler<Color> ColorChanged;

        public Color SelectedColor
        {
            get => _selectedColor;
            set
            {
                _selectedColor = value;
                UpdatePreview();
                ColorChanged?.Invoke(this, value);
            }
        }

        public ColorPickerControl()
        {
            BorderBrush = new SolidColorBrush(Color.FromRgb(70, 70, 70));
            BorderThickness = new Thickness(1);
            CornerRadius = new CornerRadius(4);
            Padding = new Thickness(5);
            Background = new SolidColorBrush(Color.FromRgb(45, 45, 45));

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });

            // Color preview
            _colorPreview = new Border
            {
                Width = 24,
                Height = 24,
                CornerRadius = new CornerRadius(3),
                Background = new SolidColorBrush(_selectedColor),
                Cursor = Cursors.Hand
            };
            _colorPreview.MouseLeftButtonDown += (s, e) => OpenColorDialog();
            Grid.SetColumn(_colorPreview, 0);

            // Hex text
            _hexText = new TextBlock
            {
                Text = IconTintManager.ToHexColor(_selectedColor),
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0)
            };
            Grid.SetColumn(_hexText, 1);

            // Eyedropper button
            _eyedropperButton = new Button
            {
                Content = "💧",
                Width = 24,
                Height = 24,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                ToolTip = "Pick color from screen"
            };
            _eyedropperButton.Click += (s, e) => StartEyedropper();
            Grid.SetColumn(_eyedropperButton, 2);

            grid.Children.Add(_colorPreview);
            grid.Children.Add(_hexText);
            grid.Children.Add(_eyedropperButton);

            Child = grid;

            // Setup picker events
            _picker.ColorPicked += (s, color) => SelectedColor = color;
        }

        private void UpdatePreview()
        {
            _colorPreview.Background = new SolidColorBrush(_selectedColor);
            _hexText.Text = IconTintManager.ToHexColor(_selectedColor);
        }

        private void OpenColorDialog()
        {
            Color? result = ColorPickerTool.ShowColorDialog(_selectedColor);
            if (result.HasValue)
            {
                SelectedColor = result.Value;
            }
        }

        private void StartEyedropper()
        {
            _picker.StartPicking();
        }
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Desktop_Fences
{
    /// <summary>
    /// Manages "click to open" behavior for rolled-up fences.
    /// Prevents accidental expansion when moving mouse over rolled fences.
    /// A key feature from Stardock Fences 6.
    /// </summary>
    public static class RollClickManager
    {
        private static bool _requireClickToOpen = false;
        private static string _currentlyHoveredFenceId = null;
        private static readonly object _hoverLock = new object();

        /// <summary>
        /// Gets or sets whether click is required to open rolled fences.
        /// </summary>
        public static bool RequireClickToOpen
        {
            get => _requireClickToOpen;
            set
            {
                _requireClickToOpen = value;
                LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.Settings,
                    $"Require click to open rolled fences: {value}");
            }
        }

        /// <summary>
        /// Handles mouse enter on a rolled fence title bar.
        /// </summary>
        public static bool OnFenceMouseEnter(string fenceId, dynamic fence)
        {
            lock (_hoverLock)
            {
                bool isRolled = fence?.IsRolled?.ToString().ToLower() == "true";

                if (!isRolled)
                {
                    return false; // Not rolled, normal behavior
                }

                if (_requireClickToOpen)
                {
                    _currentlyHoveredFenceId = fenceId;
                    return false; // Don't auto-expand
                }

                return true; // Auto-expand (legacy behavior)
            }
        }

        /// <summary>
        /// Handles mouse leave on a rolled fence.
        /// </summary>
        public static void OnFenceMouseLeave(string fenceId)
        {
            lock (_hoverLock)
            {
                if (_currentlyHoveredFenceId == fenceId)
                {
                    _currentlyHoveredFenceId = null;
                }
            }
        }

        /// <summary>
        /// Handles click on a rolled fence to expand it.
        /// </summary>
        public static void OnFenceClick(string fenceId, dynamic fence, NonActivatingWindow window)
        {
            if (!_requireClickToOpen) return;

            bool isRolled = fence?.IsRolled?.ToString().ToLower() == "true";

            if (isRolled)
            {
                ExpandFence(fence, window);
            }
        }

        /// <summary>
        /// Expands a rolled-up fence with animation.
        /// </summary>
        public static void ExpandFence(dynamic fence, NonActivatingWindow window,
            double animationDuration = 250)
        {
            try
            {
                double originalHeight = Convert.ToDouble(fence.Height ?? 200);
                double titleHeight = 30; // Title bar height

                var animation = new DoubleAnimation
                {
                    From = titleHeight,
                    To = originalHeight,
                    Duration = TimeSpan.FromMilliseconds(animationDuration),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                animation.Completed += (s, e) =>
                {
                    fence.IsRolled = false;
                    FenceDataManager.SaveFenceData();
                };

                window.BeginAnimation(FrameworkElement.HeightProperty, animation);

                LogManager.Log(LogManager.LogLevel.Debug, LogManager.LogCategory.UI,
                    $"Expanding fence '{fence.Title}'");
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.UI,
                    $"Error expanding fence: {ex.Message}");
            }
        }

        /// <summary>
        /// Rolls up a fence with animation.
        /// </summary>
        public static void RollUpFence(dynamic fence, NonActivatingWindow window,
            double animationDuration = 250)
        {
            try
            {
                double currentHeight = window.ActualHeight;
                double titleHeight = 30; // Title bar height

                // Store original height before rolling
                fence.Height = (int)currentHeight;

                var animation = new DoubleAnimation
                {
                    From = currentHeight,
                    To = titleHeight,
                    Duration = TimeSpan.FromMilliseconds(animationDuration),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                };

                animation.Completed += (s, e) =>
                {
                    fence.IsRolled = true;
                    FenceDataManager.SaveFenceData();
                };

                window.BeginAnimation(FrameworkElement.HeightProperty, animation);

                LogManager.Log(LogManager.LogLevel.Debug, LogManager.LogCategory.UI,
                    $"Rolling up fence '{fence.Title}'");
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.UI,
                    $"Error rolling up fence: {ex.Message}");
            }
        }

        /// <summary>
        /// Toggles the roll state of a fence.
        /// </summary>
        public static void ToggleRollState(dynamic fence, NonActivatingWindow window)
        {
            bool isRolled = fence?.IsRolled?.ToString().ToLower() == "true";

            if (isRolled)
            {
                ExpandFence(fence, window);
            }
            else
            {
                RollUpFence(fence, window);
            }
        }

        /// <summary>
        /// Sets up click-to-open behavior for a fence window.
        /// </summary>
        public static void SetupClickToOpen(NonActivatingWindow window, dynamic fence)
        {
            string fenceId = fence.Id?.ToString();

            // Title bar click handler
            var titleBar = FindTitleBar(window);
            if (titleBar != null)
            {
                titleBar.MouseLeftButtonDown += (s, e) =>
                {
                    if (e.ClickCount == 2 && !_requireClickToOpen)
                    {
                        // Double-click to toggle roll (legacy behavior)
                        ToggleRollState(fence, window);
                        e.Handled = true;
                    }
                    else if (_requireClickToOpen)
                    {
                        // Single click to expand when RequireClickToOpen is enabled
                        bool isRolled = fence?.IsRolled?.ToString().ToLower() == "true";
                        if (isRolled)
                        {
                            ExpandFence(fence, window);
                            e.Handled = true;
                        }
                    }
                };
            }
        }

        #region Private Helpers

        private static FrameworkElement FindTitleBar(NonActivatingWindow window)
        {
            try
            {
                var border = window.Content as Border;
                var dockPanel = border?.Child as DockPanel;

                if (dockPanel != null)
                {
                    foreach (var child in dockPanel.Children)
                    {
                        if (child is Grid grid)
                        {
                            // Title bar is typically a Grid with the fence title
                            foreach (var gridChild in grid.Children)
                            {
                                if (gridChild is TextBlock tb &&
                                    !string.IsNullOrEmpty(tb.Text) &&
                                    tb.Name != "FenceLockIcon")
                                {
                                    return grid;
                                }
                            }
                        }
                    }
                }
            }
            catch { }

            return null;
        }

        #endregion
    }
}

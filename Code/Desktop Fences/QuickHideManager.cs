using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Desktop_Fences
{
    /// <summary>
    /// Manages quick-hide options - allows hiding fences while keeping desktop icons visible.
    /// A key feature from Stardock Fences 6.
    /// </summary>
    public static class QuickHideManager
    {
        /// <summary>
        /// Hide mode options.
        /// </summary>
        public enum HideMode
        {
            /// <summary>Hide all fences and desktop icons (classic behavior).</summary>
            All,
            /// <summary>Hide only fences, keep desktop icons visible.</summary>
            FencesOnly,
            /// <summary>Hide only specific fences.</summary>
            Selected
        }

        private static HideMode _currentMode = HideMode.All;
        private static bool _isHidden = false;
        private static readonly List<string> _hiddenFenceIds = new List<string>();

        /// <summary>
        /// Gets or sets the current hide mode.
        /// </summary>
        public static HideMode CurrentMode
        {
            get => _currentMode;
            set
            {
                _currentMode = value;
                SettingsManager.SaveSettings();
                LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.Settings,
                    $"Quick-hide mode set to: {value}");
            }
        }

        /// <summary>
        /// Gets whether fences are currently hidden.
        /// </summary>
        public static bool IsHidden => _isHidden;

        /// <summary>
        /// Toggles fence visibility based on current mode.
        /// </summary>
        public static void ToggleVisibility()
        {
            _isHidden = !_isHidden;

            if (_isHidden)
            {
                HideFences();
            }
            else
            {
                ShowFences();
            }

            LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.UI,
                $"Fences {(_isHidden ? "hidden" : "shown")}");
        }

        /// <summary>
        /// Hides fences based on current mode.
        /// </summary>
        public static void HideFences()
        {
            _isHidden = true;

            var windows = Application.Current.Windows.OfType<NonActivatingWindow>().ToList();

            switch (_currentMode)
            {
                case HideMode.All:
                    foreach (var window in windows)
                    {
                        window.Hide();
                    }
                    break;

                case HideMode.FencesOnly:
                    foreach (var window in windows)
                    {
                        window.Hide();
                    }
                    // Desktop icons remain visible
                    break;

                case HideMode.Selected:
                    foreach (var window in windows)
                    {
                        string fenceId = window.Tag?.ToString();
                        if (_hiddenFenceIds.Contains(fenceId))
                        {
                            window.Hide();
                        }
                    }
                    break;
            }

            OnVisibilityChanged?.Invoke(null, _isHidden);
        }

        /// <summary>
        /// Shows all hidden fences.
        /// </summary>
        public static void ShowFences()
        {
            _isHidden = false;

            var windows = Application.Current.Windows.OfType<NonActivatingWindow>().ToList();

            foreach (var window in windows)
            {
                string fenceId = window.Tag?.ToString();
                var fence = FenceDataManager.FenceData.FirstOrDefault(f =>
                    f.Id?.ToString() == fenceId);

                // Don't show fences that are meant to be hidden
                bool isPermanentlyHidden = fence?.IsHidden?.ToString().ToLower() == "true";

                if (!isPermanentlyHidden)
                {
                    window.Show();
                }
            }

            OnVisibilityChanged?.Invoke(null, _isHidden);
        }

        /// <summary>
        /// Adds a fence to the "hide on quick-hide" list.
        /// </summary>
        public static void AddToQuickHide(string fenceId)
        {
            if (!_hiddenFenceIds.Contains(fenceId))
            {
                _hiddenFenceIds.Add(fenceId);
            }
        }

        /// <summary>
        /// Removes a fence from the "hide on quick-hide" list.
        /// </summary>
        public static void RemoveFromQuickHide(string fenceId)
        {
            _hiddenFenceIds.Remove(fenceId);
        }

        /// <summary>
        /// Gets whether a fence is in the quick-hide list.
        /// </summary>
        public static bool IsInQuickHideList(string fenceId)
        {
            return _hiddenFenceIds.Contains(fenceId);
        }

        /// <summary>
        /// Toggles whether a fence is in the quick-hide list.
        /// </summary>
        public static void ToggleQuickHideList(string fenceId)
        {
            if (_hiddenFenceIds.Contains(fenceId))
            {
                _hiddenFenceIds.Remove(fenceId);
            }
            else
            {
                _hiddenFenceIds.Add(fenceId);
            }
        }

        /// <summary>
        /// Gets the number of currently hidden fences.
        /// </summary>
        public static int GetHiddenFenceCount()
        {
            if (!_isHidden) return 0;

            return Application.Current.Windows.OfType<NonActivatingWindow>()
                .Count(w => !w.IsVisible);
        }

        /// <summary>
        /// Event raised when visibility changes.
        /// </summary>
        public static event EventHandler<bool> OnVisibilityChanged;

        /// <summary>
        /// Hides fences with animation.
        /// </summary>
        public static void HideWithAnimation(double durationMs = 300)
        {
            var windows = Application.Current.Windows.OfType<NonActivatingWindow>().ToList();

            foreach (var window in windows)
            {
                var animation = new System.Windows.Media.Animation.DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(durationMs)
                };

                animation.Completed += (s, e) => window.Hide();
                window.BeginAnimation(UIElement.OpacityProperty, animation);
            }

            _isHidden = true;
        }

        /// <summary>
        /// Shows fences with animation.
        /// </summary>
        public static void ShowWithAnimation(double durationMs = 300)
        {
            var windows = Application.Current.Windows.OfType<NonActivatingWindow>().ToList();

            foreach (var window in windows)
            {
                string fenceId = window.Tag?.ToString();
                var fence = FenceDataManager.FenceData.FirstOrDefault(f =>
                    f.Id?.ToString() == fenceId);

                bool isPermanentlyHidden = fence?.IsHidden?.ToString().ToLower() == "true";
                if (isPermanentlyHidden) continue;

                window.Opacity = 0;
                window.Show();

                var animation = new System.Windows.Media.Animation.DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(durationMs)
                };

                window.BeginAnimation(UIElement.OpacityProperty, animation);
            }

            _isHidden = false;
        }
    }
}

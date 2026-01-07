using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;

namespace Desktop_Fences
{
    /// <summary>
    /// Handles icon operations including creation, caching, and manipulation.
    /// Extracted from FenceManager for better code organization.
    /// </summary>
    public static class FenceIconHandler
    {
        // Cache for icon states to prevent GDI leaks
        private static readonly Dictionary<string, (DateTime LastWrite, bool IsBroken)> _iconStates
            = new Dictionary<string, (DateTime, bool)>();

        // Cache for extracted icons
        private static readonly Dictionary<string, BitmapSource> _iconCache
            = new Dictionary<string, BitmapSource>();

        /// <summary>
        /// Gets or creates a cached icon for the specified path.
        /// </summary>
        /// <param name="path">The file path to get the icon for.</param>
        /// <returns>The cached or newly extracted icon.</returns>
        public static BitmapSource GetCachedIcon(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path)) return null;

                string key = path.ToLowerInvariant();

                // Check cache first
                if (_iconCache.TryGetValue(key, out var cachedIcon))
                {
                    // Verify the file hasn't changed
                    if (System.IO.File.Exists(path))
                    {
                        var currentWrite = System.IO.File.GetLastWriteTime(path);
                        if (_iconStates.TryGetValue(key, out var state) && state.LastWrite == currentWrite)
                        {
                            return cachedIcon;
                        }
                    }
                }

                // Extract new icon
                var icon = IconManager.ExtractIcon(path);
                if (icon != null)
                {
                    _iconCache[key] = icon;
                    if (System.IO.File.Exists(path))
                    {
                        _iconStates[key] = (System.IO.File.GetLastWriteTime(path), false);
                    }
                }

                return icon;
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Warn, LogManager.LogCategory.IconHandling,
                    $"Error getting cached icon for {path}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets an icon for the specified path with optional size parameter.
        /// This is an alias for GetCachedIcon used by LazyIconLoader.
        /// </summary>
        /// <param name="path">The file path to get the icon for.</param>
        /// <param name="iconSize">The desired icon size (currently ignored, uses default).</param>
        /// <returns>The cached or newly extracted icon.</returns>
        public static BitmapSource GetIcon(string path, int iconSize = 48)
        {
            return GetCachedIcon(path);
        }

        /// <summary>
        /// Clears the icon cache.
        /// </summary>
        public static void ClearCache()
        {
            _iconCache.Clear();
            _iconStates.Clear();
            LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.IconHandling, "Icon cache cleared");
        }

        /// <summary>
        /// Exports all icons from a Data fence to the desktop.
        /// </summary>
        /// <param name="fence">The fence to export icons from.</param>
        /// <param name="showConfirmation">If true, shows a message box on completion.</param>
        public static void ExportAllIconsToDesktop(dynamic fence, bool showConfirmation = true)
        {
            try
            {
                if (fence.ItemsType?.ToString() != "Data")
                {
                    LogManager.Log(LogManager.LogLevel.Warn, LogManager.LogCategory.UI,
                        $"Export all icons attempted on non-Data fence: {fence.ItemsType}");
                    return;
                }

                int exportedCount = 0;
                int totalCount = 0;

                bool hasTabsEnabled = fence.TabsEnabled?.ToString().ToLower() == "true";

                if (hasTabsEnabled && fence.Tabs != null)
                {
                    // Handle tabbed fence
                    var tabs = fence.Tabs as JArray ?? new JArray();

                    foreach (var tab in tabs)
                    {
                        var tabObj = tab as JObject;
                        var tabItems = tabObj?["Items"] as JArray ?? new JArray();

                        foreach (var item in tabItems)
                        {
                            totalCount++;
                            try
                            {
                                CopyPasteManager.SendToDesktop(item);
                                exportedCount++;
                            }
                            catch (Exception itemEx)
                            {
                                LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.UI,
                                    $"Error exporting item to desktop: {itemEx.Message}");
                            }
                        }
                    }
                }
                else
                {
                    // Handle regular fence
                    var items = fence.Items as JArray ?? new JArray();

                    foreach (var item in items)
                    {
                        totalCount++;
                        try
                        {
                            CopyPasteManager.SendToDesktop(item);
                            exportedCount++;
                        }
                        catch (Exception itemEx)
                        {
                            LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.UI,
                                $"Error exporting item to desktop: {itemEx.Message}");
                        }
                    }
                }

                // Show result message
                if (showConfirmation)
                {
                    string resultMessage = $"Exported {exportedCount} of {totalCount} icons to desktop.";
                    if (exportedCount != totalCount)
                    {
                        resultMessage += $" {totalCount - exportedCount} items failed to export.";
                    }

                    // Delay message to allow desktop refresh
                    var delayTimer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(1700)
                    };
                    delayTimer.Tick += (s, e) =>
                    {
                        delayTimer.Stop();
                        MessageBoxesManager.ShowOKOnlyMessageBoxForm(resultMessage, "Export Complete");
                    };
                    delayTimer.Start();
                }

                LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.UI,
                    $"Export completed for fence '{fence.Title}': {exportedCount}/{totalCount}");
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.UI,
                    $"Error in ExportAllIconsToDesktop: {ex.Message}");
                MessageBoxesManager.ShowOKOnlyMessageBoxForm($"Error exporting icons: {ex.Message}", "Export Error");
            }
        }

        /// <summary>
        /// Checks if a fence has any dead (broken) shortcuts.
        /// </summary>
        /// <param name="fence">The fence to check.</param>
        /// <returns>True if dead shortcuts are found.</returns>
        public static bool HasDeadShortcuts(dynamic fence)
        {
            try
            {
                if (fence.ItemsType?.ToString() != "Data") return false;

                bool tabsEnabled = fence.TabsEnabled?.ToString().ToLower() == "true";

                if (tabsEnabled && fence.Tabs != null)
                {
                    var tabs = fence.Tabs as JArray ?? new JArray();
                    foreach (var tab in tabs)
                    {
                        var tabObj = tab as JObject;
                        var items = tabObj?["Items"] as JArray ?? new JArray();
                        if (CheckItemsForDead(items)) return true;
                    }
                }
                else
                {
                    var items = fence.Items as JArray ?? new JArray();
                    return CheckItemsForDead(items);
                }

                return false;
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Warn, LogManager.LogCategory.IconHandling,
                    $"Error checking for dead shortcuts: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Updates the icon state in the cache.
        /// </summary>
        public static void UpdateIconState(string path, bool isBroken)
        {
            if (string.IsNullOrEmpty(path)) return;

            string key = path.ToLowerInvariant();
            DateTime lastWrite = DateTime.MinValue;

            try
            {
                if (System.IO.File.Exists(path))
                {
                    lastWrite = System.IO.File.GetLastWriteTime(path);
                }
            }
            catch { }

            _iconStates[key] = (lastWrite, isBroken);
        }

        /// <summary>
        /// Gets the icon state from cache.
        /// </summary>
        public static (DateTime LastWrite, bool IsBroken)? GetIconState(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            string key = path.ToLowerInvariant();
            if (_iconStates.TryGetValue(key, out var state))
            {
                return state;
            }
            return null;
        }

        #region Private Helpers

        private static bool CheckItemsForDead(JArray items)
        {
            foreach (var item in items)
            {
                string filename = item["Filename"]?.ToString();
                if (string.IsNullOrEmpty(filename)) continue;

                // Check if file exists
                if (!System.IO.File.Exists(filename) && !System.IO.Directory.Exists(filename))
                {
                    // Check if it's a valid URL
                    if (!IsValidUrl(filename))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool IsValidUrl(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;

            return Uri.TryCreate(path, UriKind.Absolute, out Uri uri) &&
                   (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        #endregion
    }
}

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Desktop_Fences
{
    /// <summary>
    /// Handles Portal Fence navigation operations.
    /// Extracted from FenceManager for better code organization.
    /// </summary>
    public static class FenceNavigationManager
    {
        // Tracks temporary navigation paths for Portal Fences
        private static readonly System.Collections.Generic.Dictionary<string, string> _portalNavigationStates
            = new System.Collections.Generic.Dictionary<string, string>();

        /// <summary>
        /// Gets the current navigation path for a portal fence.
        /// </summary>
        public static string GetCurrentPath(string fenceId, string basePath)
        {
            if (string.IsNullOrEmpty(fenceId)) return basePath;

            return _portalNavigationStates.TryGetValue(fenceId, out string path) ? path : basePath;
        }

        /// <summary>
        /// Sets the navigation path for a portal fence.
        /// </summary>
        public static void SetCurrentPath(string fenceId, string path, string basePath)
        {
            if (string.IsNullOrEmpty(fenceId)) return;

            if (string.Equals(path, basePath, StringComparison.OrdinalIgnoreCase))
            {
                // Return to home - remove from state
                _portalNavigationStates.Remove(fenceId);
            }
            else
            {
                _portalNavigationStates[fenceId] = path;
            }
        }

        /// <summary>
        /// Checks if a portal fence is navigated away from home.
        /// </summary>
        public static bool IsNavigating(string fenceId, string basePath)
        {
            if (string.IsNullOrEmpty(fenceId)) return false;

            string currentPath = GetCurrentPath(fenceId, basePath);
            return !string.Equals(currentPath, basePath, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the display path for the navigation bar.
        /// </summary>
        public static string GetDisplayPath(string currentPath, string basePath)
        {
            try
            {
                string displayPath = new System.IO.DirectoryInfo(currentPath).Name;

                if (currentPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                {
                    string rootName = new System.IO.DirectoryInfo(basePath).Name;
                    string relativePart = currentPath.Substring(basePath.Length)
                        .TrimStart(System.IO.Path.DirectorySeparatorChar);

                    displayPath = string.IsNullOrEmpty(relativePart)
                        ? rootName
                        : System.IO.Path.Combine(rootName, relativePart);
                }

                return displayPath;
            }
            catch
            {
                return new System.IO.DirectoryInfo(currentPath).Name;
            }
        }

        /// <summary>
        /// Navigates to a parent directory.
        /// </summary>
        public static string GetParentPath(string currentPath)
        {
            try
            {
                return System.IO.Directory.GetParent(currentPath)?.FullName ?? currentPath;
            }
            catch
            {
                return currentPath;
            }
        }

        /// <summary>
        /// Clears navigation state for a fence.
        /// </summary>
        public static void ClearNavigationState(string fenceId)
        {
            if (!string.IsNullOrEmpty(fenceId))
            {
                _portalNavigationStates.Remove(fenceId);
            }
        }

        /// <summary>
        /// Clears all navigation states.
        /// </summary>
        public static void ClearAllNavigationStates()
        {
            _portalNavigationStates.Clear();
            LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.UI,
                "Cleared all portal navigation states");
        }

        /// <summary>
        /// Checks if a path is valid for navigation.
        /// </summary>
        public static bool IsValidPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;

            try
            {
                return System.IO.Directory.Exists(path);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if a path is a UNC root (e.g., \\Server).
        /// </summary>
        public static bool IsUncRoot(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;

            if (path.StartsWith(@"\\") && !path.Contains(@":\"))
            {
                string clean = path.Substring(2);
                int slash = clean.IndexOf('\\');
                return (slash < 0 || slash == clean.Length - 1);
            }
            return false;
        }
    }
}

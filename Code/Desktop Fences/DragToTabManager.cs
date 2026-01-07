using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json.Linq;

namespace Desktop_Fences
{
    /// <summary>
    /// Manages drag-to-tab-header functionality.
    /// Allows dropping files directly onto tab headers to add them to that fence.
    /// A key feature from Stardock Fences 6.
    /// </summary>
    public static class DragToTabManager
    {
        private static TabItem _dragOverTab = null;
        private static DateTime _dragEnterTime = DateTime.MinValue;
        private static readonly TimeSpan SwitchDelay = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Sets up drag-to-tab handlers for a TabControl.
        /// </summary>
        public static void SetupTabControl(TabControl tabControl)
        {
            if (tabControl == null) return;

            tabControl.AllowDrop = true;
            tabControl.PreviewDragOver += TabControl_PreviewDragOver;
            tabControl.PreviewDrop += TabControl_PreviewDrop;
        }

        /// <summary>
        /// Handles drag over event on tab control.
        /// </summary>
        private static void TabControl_PreviewDragOver(object sender, DragEventArgs e)
        {
            var tabControl = sender as TabControl;
            if (tabControl == null) return;

            // Find which tab is being hovered
            var hitTab = GetTabItemUnderMouse(tabControl, e.GetPosition(tabControl));

            if (hitTab != null)
            {
                if (_dragOverTab != hitTab)
                {
                    _dragOverTab = hitTab;
                    _dragEnterTime = DateTime.Now;
                }
                else if (DateTime.Now - _dragEnterTime > SwitchDelay)
                {
                    // Auto-switch to this tab after delay
                    tabControl.SelectedItem = hitTab;
                }

                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles drop event on tab header.
        /// </summary>
        private static void TabControl_PreviewDrop(object sender, DragEventArgs e)
        {
            var tabControl = sender as TabControl;
            if (tabControl == null) return;

            var hitTab = GetTabItemUnderMouse(tabControl, e.GetPosition(tabControl));
            if (hitTab == null) return;

            // Get fence ID from tab
            string fenceId = hitTab.Tag?.ToString();
            if (string.IsNullOrEmpty(fenceId)) return;

            // Process dropped files
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null && files.Length > 0)
                {
                    AddFilesToFence(fenceId, files);
                    e.Handled = true;
                }
            }

            _dragOverTab = null;
        }

        /// <summary>
        /// Adds files to a specific fence.
        /// </summary>
        public static void AddFilesToFence(string fenceId, string[] files)
        {
            try
            {
                var fence = FenceDataManager.FenceData.FirstOrDefault(f =>
                    f.Id?.ToString() == fenceId);

                if (fence == null)
                {
                    LogManager.Log(LogManager.LogLevel.Warning, LogManager.LogCategory.FenceUpdate,
                        $"Fence not found: {fenceId}");
                    return;
                }

                var items = fence.Items as JArray ?? new JArray();
                int addedCount = 0;

                foreach (var file in files)
                {
                    if (!File.Exists(file) && !Directory.Exists(file)) continue;

                    // Check if already exists
                    bool exists = items.Any(i =>
                        i["Filename"]?.ToString().Equals(file, StringComparison.OrdinalIgnoreCase) == true);

                    if (!exists)
                    {
                        var newItem = new JObject
                        {
                            ["Filename"] = file,
                            ["AddedDate"] = DateTime.Now.ToString("o")
                        };
                        items.Add(newItem);
                        addedCount++;
                    }
                }

                if (addedCount > 0)
                {
                    fence.Items = items;
                    FenceDataManager.SaveFenceData();

                    LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.FenceUpdate,
                        $"Added {addedCount} item(s) to fence '{fence.Title}' via tab drop");

                    // Refresh the fence display
                    FenceManager.RefreshFence(fenceId);
                }
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.FenceUpdate,
                    $"Error adding files to fence: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the TabItem under the mouse cursor.
        /// </summary>
        private static TabItem GetTabItemUnderMouse(TabControl tabControl, Point mousePosition)
        {
            foreach (TabItem tab in tabControl.Items)
            {
                if (tab.IsMouseOver)
                {
                    return tab;
                }
            }

            // Fallback: hit test
            var result = VisualTreeHelper.HitTest(tabControl, mousePosition);
            if (result != null)
            {
                var element = result.VisualHit;
                while (element != null)
                {
                    if (element is TabItem tabItem)
                    {
                        return tabItem;
                    }
                    element = VisualTreeHelper.GetParent(element);
                }
            }

            return null;
        }

        /// <summary>
        /// Visual helper for VisualTreeHelper.
        /// </summary>
        private static class VisualTreeHelper
        {
            public static HitTestResult HitTest(Visual visual, Point point)
            {
                return System.Windows.Media.VisualTreeHelper.HitTest(visual, point);
            }

            public static DependencyObject GetParent(DependencyObject child)
            {
                return System.Windows.Media.VisualTreeHelper.GetParent(child);
            }
        }
    }
}

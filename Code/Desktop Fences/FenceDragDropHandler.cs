using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json.Linq;

namespace Desktop_Fences
{
    /// <summary>
    /// Handles drag and drop operations for fences.
    /// Extracted from FenceManager for better code organization.
    /// </summary>
    public static class FenceDragDropHandler
    {
        private static bool _isDragging = false;
        private static Point _dragStartPoint;
        private static StackPanel _draggedIcon = null;

        /// <summary>
        /// Gets whether a drag operation is in progress.
        /// </summary>
        public static bool IsDragging => _isDragging;

        /// <summary>
        /// Initializes drag and drop for a fence.
        /// </summary>
        public static void InitializeDragDrop(NonActivatingWindow fenceWindow, dynamic fence)
        {
            var border = fenceWindow.Content as Border;
            var dockPanel = border?.Child as DockPanel;
            if (dockPanel == null) return;

            var scrollViewer = dockPanel.Children.OfType<ScrollViewer>().FirstOrDefault();
            var wrapPanel = scrollViewer?.Content as WrapPanel;
            if (wrapPanel == null) return;

            // Setup drop handler
            wrapPanel.AllowDrop = true;
            wrapPanel.Drop += (s, e) => HandleDrop(e, fence, wrapPanel);
            wrapPanel.DragOver += (s, e) => HandleDragOver(e);
        }

        /// <summary>
        /// Starts a drag operation for an icon.
        /// </summary>
        public static void StartDrag(StackPanel iconPanel, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _dragStartPoint = e.GetPosition(null);
            _draggedIcon = iconPanel;
        }

        /// <summary>
        /// Ends the current drag operation.
        /// </summary>
        public static void EndDrag()
        {
            _isDragging = false;
            _draggedIcon = null;
        }

        /// <summary>
        /// Handles the drop event on a fence.
        /// </summary>
        public static void HandleDrop(DragEventArgs e, dynamic fence, WrapPanel wrapPanel)
        {
            try
            {
                // Handle file drops
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    foreach (var file in files)
                    {
                        AddFileToFence(file, fence, wrapPanel);
                    }
                    e.Handled = true;
                    return;
                }

                // Handle URL drops
                string url = ExtractUrlFromDropData(e.Data);
                if (!string.IsNullOrEmpty(url))
                {
                    AddUrlToFence(url, fence, wrapPanel);
                    e.Handled = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.FenceCreation,
                    $"Error handling drop: {ex.Message}");
            }
            finally
            {
                EndDrag();
            }
        }

        /// <summary>
        /// Handles reordering icons within a fence using CTRL+Drag.
        /// </summary>
        public static void HandleReorder(WrapPanel wrapPanel, StackPanel draggedIcon, Point dropPosition)
        {
            try
            {
                int sourceIndex = wrapPanel.Children.IndexOf(draggedIcon);
                if (sourceIndex < 0) return;

                // Find target index based on drop position
                int targetIndex = GetTargetIndex(wrapPanel, dropPosition);
                if (targetIndex < 0) targetIndex = wrapPanel.Children.Count - 1;

                if (sourceIndex == targetIndex) return;

                // Move in UI
                wrapPanel.Children.RemoveAt(sourceIndex);
                wrapPanel.Children.Insert(targetIndex, draggedIcon);

                // Update display order in data
                UpdateDisplayOrder(wrapPanel);

                LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.UI,
                    $"Reordered icon from {sourceIndex} to {targetIndex}");
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.UI,
                    $"Error reordering icons: {ex.Message}");
            }
        }

        #region Private Helpers

        private static void HandleDragOver(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) ||
                e.Data.GetDataPresent("UniformResourceLocator") ||
                e.Data.GetDataPresent(DataFormats.Text))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private static void AddFileToFence(string filePath, dynamic fence, WrapPanel wrapPanel)
        {
            try
            {
                // Check if file is already in fence
                var items = fence.Items as JArray ?? new JArray();
                bool exists = items.Any(i =>
                    string.Equals(i["Filename"]?.ToString(), filePath, StringComparison.OrdinalIgnoreCase));

                if (exists)
                {
                    LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.FenceCreation,
                        $"File already in fence: {filePath}");
                    return;
                }

                bool isFolder = System.IO.Directory.Exists(filePath);
                bool isLink = System.IO.Path.GetExtension(filePath).ToLower() == ".lnk";
                bool isNetwork = IsNetworkPath(filePath);

                // Create item data
                dynamic newItem = new System.Dynamic.ExpandoObject();
                IDictionary<string, object> itemDict = newItem;
                itemDict["Filename"] = filePath;
                itemDict["IsFolder"] = isFolder;
                itemDict["IsLink"] = isLink;
                itemDict["IsNetwork"] = isNetwork;
                itemDict["DisplayOrder"] = items.Count;
                itemDict["AlwaysRunAsAdmin"] = false;

                // Add to fence data
                items.Add(JObject.FromObject(newItem));
                FenceDataManager.SaveFenceData();

                // Add to UI
                FenceManager.AddIcon(newItem, wrapPanel);

                // Delete original if setting enabled
                if (SettingsManager.DeleteOriginalShortcutsOnDrop)
                {
                    try
                    {
                        if (System.IO.File.Exists(filePath) && !isFolder)
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                    catch { }
                }

                LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.FenceCreation,
                    $"Added file to fence: {filePath}");
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.FenceCreation,
                    $"Error adding file to fence: {ex.Message}");
            }
        }

        private static void AddUrlToFence(string url, dynamic fence, WrapPanel wrapPanel)
        {
            try
            {
                // Create shortcuts directory
                if (!System.IO.Directory.Exists("Shortcuts"))
                {
                    System.IO.Directory.CreateDirectory("Shortcuts");
                }

                // Generate filename from URL
                Uri uri = new Uri(url);
                string displayName = uri.Host.Replace("www.", "");
                string shortcutPath = System.IO.Path.Combine("Shortcuts", $"{displayName}.url");

                // Ensure unique filename
                int counter = 1;
                while (System.IO.File.Exists(shortcutPath))
                {
                    shortcutPath = System.IO.Path.Combine("Shortcuts", $"{displayName} ({counter++}).url");
                }

                // Create URL file
                string urlContent = $"[InternetShortcut]\r\nURL={url}\r\nIconIndex=0\r\n";
                System.IO.File.WriteAllText(shortcutPath, urlContent, System.Text.Encoding.ASCII);

                // Add to fence
                AddFileToFence(shortcutPath, fence, wrapPanel);

                LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.FenceCreation,
                    $"Added URL to fence: {url}");
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.FenceCreation,
                    $"Error adding URL to fence: {ex.Message}");
            }
        }

        private static string ExtractUrlFromDropData(IDataObject dataObject)
        {
            try
            {
                // Try UniformResourceLocator format first
                if (dataObject.GetDataPresent("UniformResourceLocator"))
                {
                    object urlData = dataObject.GetData("UniformResourceLocator");
                    if (urlData is System.IO.MemoryStream stream)
                    {
                        byte[] bytes = new byte[stream.Length];
                        stream.Read(bytes, 0, (int)stream.Length);
                        return System.Text.Encoding.ASCII.GetString(bytes).Trim('\0');
                    }
                }

                // Try text format
                if (dataObject.GetDataPresent(DataFormats.Text))
                {
                    string text = dataObject.GetData(DataFormats.Text) as string;
                    if (IsValidUrl(text?.Trim()))
                    {
                        return text.Trim();
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.FenceCreation,
                    $"Error extracting URL: {ex.Message}");
                return null;
            }
        }

        private static bool IsValidUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return false;

            return Uri.TryCreate(url, UriKind.Absolute, out Uri uri) &&
                   (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        private static bool IsNetworkPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            return path.StartsWith(@"\\");
        }

        private static int GetTargetIndex(WrapPanel wrapPanel, Point dropPosition)
        {
            for (int i = 0; i < wrapPanel.Children.Count; i++)
            {
                var child = wrapPanel.Children[i] as FrameworkElement;
                if (child == null) continue;

                var position = child.TransformToAncestor(wrapPanel).Transform(new Point(0, 0));
                var bounds = new Rect(position, new Size(child.ActualWidth, child.ActualHeight));

                if (bounds.Contains(dropPosition))
                {
                    return i;
                }
            }
            return -1;
        }

        private static void UpdateDisplayOrder(WrapPanel wrapPanel)
        {
            try
            {
                var parentWindow = VisualTreeHelperExtensions.FindVisualParent<NonActivatingWindow>(wrapPanel);
                if (parentWindow == null) return;

                string fenceId = parentWindow.Tag?.ToString();
                var fence = FenceDataManager.FenceData.FirstOrDefault(f => f.Id?.ToString() == fenceId);
                if (fence == null) return;

                var items = fence.Items as JArray ?? new JArray();

                // Update display order based on UI order
                for (int i = 0; i < wrapPanel.Children.Count; i++)
                {
                    var sp = wrapPanel.Children[i] as StackPanel;
                    if (sp?.Tag == null) continue;

                    string filePath = sp.Tag.ToString();
                    var item = items.FirstOrDefault(it =>
                        string.Equals(it["Filename"]?.ToString(), filePath, StringComparison.OrdinalIgnoreCase));

                    if (item != null)
                    {
                        item["DisplayOrder"] = i;
                    }
                }

                FenceDataManager.SaveFenceData();
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.UI,
                    $"Error updating display order: {ex.Message}");
            }
        }

        #endregion
    }
}

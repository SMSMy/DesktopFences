using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Desktop_Fences
{
    /// <summary>
    /// Provides virtualization support for large fences.
    /// Only renders visible items to improve performance.
    /// </summary>
    public static class VirtualizationHelper
    {
        private const int DefaultItemHeight = 80;
        private const int DefaultItemWidth = 80;
        private const int BufferItems = 5; // Extra items to render outside viewport

        /// <summary>
        /// Configures a ScrollViewer for virtualized scrolling.
        /// </summary>
        public static void ConfigureVirtualizedScrollViewer(ScrollViewer scrollViewer)
        {
            if (scrollViewer == null) return;

            scrollViewer.CanContentScroll = true;
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            // Enable hardware acceleration
            RenderOptions.SetBitmapScalingMode(scrollViewer, BitmapScalingMode.LowQuality);
        }

        /// <summary>
        /// Creates a virtualized WrapPanel for icon display.
        /// </summary>
        public static VirtualizingWrapPanel CreateVirtualizedPanel()
        {
            var panel = new VirtualizingWrapPanel
            {
                ItemWidth = DefaultItemWidth,
                ItemHeight = DefaultItemHeight
            };

            VirtualizingPanel.SetIsVirtualizing(panel, true);
            VirtualizingPanel.SetVirtualizationMode(panel, VirtualizationMode.Recycling);
            VirtualizingPanel.SetScrollUnit(panel, ScrollUnit.Pixel);
            VirtualizingPanel.SetCacheLengthUnit(panel, VirtualizationCacheLengthUnit.Item);
            VirtualizingPanel.SetCacheLength(panel, new VirtualizationCacheLength(BufferItems));

            return panel;
        }

        /// <summary>
        /// Gets the visible item range for a scrollable container.
        /// </summary>
        public static (int Start, int End) GetVisibleItemRange(
            ScrollViewer scrollViewer,
            int totalItems,
            int itemHeight = DefaultItemHeight,
            int columns = 1)
        {
            if (scrollViewer == null || totalItems == 0)
            {
                return (0, 0);
            }

            double viewportTop = scrollViewer.VerticalOffset;
            double viewportBottom = viewportTop + scrollViewer.ViewportHeight;

            int startRow = Math.Max(0, (int)(viewportTop / itemHeight) - BufferItems);
            int endRow = Math.Min(
                (int)Math.Ceiling((double)totalItems / columns),
                (int)(viewportBottom / itemHeight) + BufferItems);

            int startIndex = startRow * columns;
            int endIndex = Math.Min(totalItems - 1, (endRow + 1) * columns - 1);

            return (startIndex, endIndex);
        }

        /// <summary>
        /// Checks if an item is visible in the viewport.
        /// </summary>
        public static bool IsItemVisible(FrameworkElement item, ScrollViewer scrollViewer)
        {
            if (item == null || scrollViewer == null) return false;

            try
            {
                var transform = item.TransformToAncestor(scrollViewer);
                var itemBounds = transform.TransformBounds(
                    new Rect(0, 0, item.ActualWidth, item.ActualHeight));

                var viewport = new Rect(0, 0, scrollViewer.ViewportWidth, scrollViewer.ViewportHeight);

                return viewport.IntersectsWith(itemBounds);
            }
            catch
            {
                return true; // Assume visible if we can't determine
            }
        }

        /// <summary>
        /// Optimizes rendering for a fence panel.
        /// </summary>
        public static void OptimizeFencePanel(Panel panel)
        {
            if (panel == null) return;

            // Enable layout rounding for crisp rendering
            panel.UseLayoutRounding = true;
            panel.SnapsToDevicePixels = true;

            // Set bitmap caching for static content
            var cache = new BitmapCache
            {
                EnableClearType = false,
                RenderAtScale = 1.0,
                SnapsToDevicePixels = true
            };
            panel.CacheMode = cache;
        }

        /// <summary>
        /// Defers loading of items outside viewport.
        /// </summary>
        public static void DeferOffscreenItems(
            IList<FrameworkElement> items,
            ScrollViewer scrollViewer,
            Action<FrameworkElement> loadAction,
            Action<FrameworkElement> unloadAction)
        {
            if (items == null || scrollViewer == null) return;

            var (start, end) = GetVisibleItemRange(scrollViewer, items.Count);

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                if (i >= start && i <= end)
                {
                    loadAction?.Invoke(item);
                }
                else
                {
                    unloadAction?.Invoke(item);
                }
            }
        }
    }

    /// <summary>
    /// A virtualizing wrap panel for efficient icon display.
    /// </summary>
    public class VirtualizingWrapPanel : VirtualizingPanel
    {
        public static readonly DependencyProperty ItemWidthProperty =
            DependencyProperty.Register(nameof(ItemWidth), typeof(double),
                typeof(VirtualizingWrapPanel), new PropertyMetadata(80.0));

        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register(nameof(ItemHeight), typeof(double),
                typeof(VirtualizingWrapPanel), new PropertyMetadata(80.0));

        public double ItemWidth
        {
            get => (double)GetValue(ItemWidthProperty);
            set => SetValue(ItemWidthProperty, value);
        }

        public double ItemHeight
        {
            get => (double)GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            UpdateChildren();

            double totalHeight = 0;
            double rowHeight = 0;
            double currentRowWidth = 0;

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(new Size(ItemWidth, ItemHeight));

                if (currentRowWidth + ItemWidth > availableSize.Width && currentRowWidth > 0)
                {
                    totalHeight += rowHeight;
                    currentRowWidth = 0;
                    rowHeight = 0;
                }

                currentRowWidth += ItemWidth;
                rowHeight = Math.Max(rowHeight, ItemHeight);
            }

            totalHeight += rowHeight;

            return new Size(
                double.IsInfinity(availableSize.Width) ? currentRowWidth : availableSize.Width,
                totalHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double x = 0;
            double y = 0;
            double rowHeight = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (x + ItemWidth > finalSize.Width && x > 0)
                {
                    x = 0;
                    y += rowHeight;
                    rowHeight = 0;
                }

                child.Arrange(new Rect(x, y, ItemWidth, ItemHeight));

                x += ItemWidth;
                rowHeight = Math.Max(rowHeight, ItemHeight);
            }

            return finalSize;
        }

        private void UpdateChildren()
        {
            var generator = ItemContainerGenerator;
            if (generator == null) return;

            // Generate visible items
            var startPos = generator.GeneratorPositionFromIndex(0);

            using (generator.StartAt(startPos, GeneratorDirection.Forward, true))
            {
                int childIndex = 0;
                while (true)
                {
                    bool isNew;
                    var child = generator.GenerateNext(out isNew) as UIElement;

                    if (child == null) break;

                    if (isNew)
                    {
                        if (childIndex < InternalChildren.Count)
                        {
                            InsertInternalChild(childIndex, child);
                        }
                        else
                        {
                            AddInternalChild(child);
                        }

                        generator.PrepareItemContainer(child);
                    }

                    childIndex++;
                }
            }
        }
    }
}

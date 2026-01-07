using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace Desktop_Fences
{
    /// <summary>
    /// Manages intelligent fence placement with consistent spacing.
    /// A key feature from Stardock Fences 6.
    /// </summary>
    public static class IntelligentPlacementManager
    {
        // Default spacing between fences
        private const int DefaultSpacing = 20;
        private const int SnapThreshold = 30;

        /// <summary>
        /// Calculates the best position for a new fence.
        /// </summary>
        public static Point FindBestPosition(List<dynamic> existingFences, Size newFenceSize)
        {
            // Get available screen area
            var workArea = SystemParameters.WorkArea;

            // If no existing fences, place in top-left with margin
            if (existingFences == null || !existingFences.Any())
            {
                return new Point(DefaultSpacing, DefaultSpacing);
            }

            // Try to find a gap in the existing layout
            var occupiedAreas = GetOccupiedAreas(existingFences);

            // Try positions along the top
            for (double x = DefaultSpacing; x < workArea.Width - newFenceSize.Width; x += 50)
            {
                var testRect = new Rect(x, DefaultSpacing, newFenceSize.Width, newFenceSize.Height);
                if (!IntersectsAny(testRect, occupiedAreas))
                {
                    return new Point(x, DefaultSpacing);
                }
            }

            // Try positions along the left
            for (double y = DefaultSpacing; y < workArea.Height - newFenceSize.Height; y += 50)
            {
                var testRect = new Rect(DefaultSpacing, y, newFenceSize.Width, newFenceSize.Height);
                if (!IntersectsAny(testRect, occupiedAreas))
                {
                    return new Point(DefaultSpacing, y);
                }
            }

            // Find first available spot using grid search
            for (double y = DefaultSpacing; y < workArea.Height - newFenceSize.Height; y += 100)
            {
                for (double x = DefaultSpacing; x < workArea.Width - newFenceSize.Width; x += 100)
                {
                    var testRect = new Rect(x, y, newFenceSize.Width, newFenceSize.Height);
                    if (!IntersectsAny(testRect, occupiedAreas))
                    {
                        return new Point(x, y);
                    }
                }
            }

            // Fallback: cascade from top-left
            int cascade = existingFences.Count * 30;
            return new Point(
                DefaultSpacing + (cascade % (int)(workArea.Width - newFenceSize.Width - DefaultSpacing)),
                DefaultSpacing + (cascade % (int)(workArea.Height - newFenceSize.Height - DefaultSpacing))
            );
        }

        /// <summary>
        /// Aligns fences to a grid with consistent spacing.
        /// </summary>
        public static void AlignAllToGrid(List<dynamic> fences, int gridSize = 50)
        {
            foreach (var fence in fences)
            {
                try
                {
                    double x = Convert.ToDouble(fence.X);
                    double y = Convert.ToDouble(fence.Y);

                    // Snap to grid
                    int newX = (int)Math.Round(x / gridSize) * gridSize;
                    int newY = (int)Math.Round(y / gridSize) * gridSize;

                    fence.X = newX;
                    fence.Y = newY;
                }
                catch { }
            }

            FenceDataManager.SaveFenceData();
            LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.FenceUpdate,
                $"Aligned {fences.Count} fences to grid");
        }

        /// <summary>
        /// Distributes fences evenly horizontally.
        /// </summary>
        public static void DistributeHorizontally(List<dynamic> fences)
        {
            if (fences == null || fences.Count < 2) return;

            var workArea = SystemParameters.WorkArea;
            var sorted = fences.OrderBy(f => Convert.ToDouble(f.X)).ToList();

            double totalWidth = sorted.Sum(f => Convert.ToDouble(f.Width ?? 200));
            double availableSpace = workArea.Width - totalWidth - (DefaultSpacing * 2);
            double spacing = availableSpace / (sorted.Count - 1);

            double currentX = DefaultSpacing;
            foreach (var fence in sorted)
            {
                fence.X = (int)currentX;
                currentX += Convert.ToDouble(fence.Width ?? 200) + spacing;
            }

            FenceDataManager.SaveFenceData();
            LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.FenceUpdate,
                $"Distributed {fences.Count} fences horizontally");
        }

        /// <summary>
        /// Distributes fences evenly vertically.
        /// </summary>
        public static void DistributeVertically(List<dynamic> fences)
        {
            if (fences == null || fences.Count < 2) return;

            var workArea = SystemParameters.WorkArea;
            var sorted = fences.OrderBy(f => Convert.ToDouble(f.Y)).ToList();

            double totalHeight = sorted.Sum(f => Convert.ToDouble(f.Height ?? 200));
            double availableSpace = workArea.Height - totalHeight - (DefaultSpacing * 2);
            double spacing = availableSpace / (sorted.Count - 1);

            double currentY = DefaultSpacing;
            foreach (var fence in sorted)
            {
                fence.Y = (int)currentY;
                currentY += Convert.ToDouble(fence.Height ?? 200) + spacing;
            }

            FenceDataManager.SaveFenceData();
            LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.FenceUpdate,
                $"Distributed {fences.Count} fences vertically");
        }

        /// <summary>
        /// Snaps a fence to nearby fences for alignment.
        /// </summary>
        public static Point SnapToNearbyFences(dynamic movingFence, List<dynamic> otherFences,
            Point proposedPosition)
        {
            double x = proposedPosition.X;
            double y = proposedPosition.Y;
            double width = Convert.ToDouble(movingFence.Width ?? 200);
            double height = Convert.ToDouble(movingFence.Height ?? 200);

            foreach (var other in otherFences)
            {
                if (other.Id?.ToString() == movingFence.Id?.ToString()) continue;

                double ox = Convert.ToDouble(other.X);
                double oy = Convert.ToDouble(other.Y);
                double ow = Convert.ToDouble(other.Width ?? 200);
                double oh = Convert.ToDouble(other.Height ?? 200);

                // Snap left edge to right edge of other
                if (Math.Abs(x - (ox + ow + DefaultSpacing)) < SnapThreshold)
                {
                    x = ox + ow + DefaultSpacing;
                }

                // Snap right edge to left edge of other
                if (Math.Abs((x + width + DefaultSpacing) - ox) < SnapThreshold)
                {
                    x = ox - width - DefaultSpacing;
                }

                // Snap top edge to bottom edge of other
                if (Math.Abs(y - (oy + oh + DefaultSpacing)) < SnapThreshold)
                {
                    y = oy + oh + DefaultSpacing;
                }

                // Snap bottom edge to top edge of other
                if (Math.Abs((y + height + DefaultSpacing) - oy) < SnapThreshold)
                {
                    y = oy - height - DefaultSpacing;
                }

                // Snap to same horizontal position
                if (Math.Abs(x - ox) < SnapThreshold)
                {
                    x = ox;
                }

                // Snap to same vertical position
                if (Math.Abs(y - oy) < SnapThreshold)
                {
                    y = oy;
                }
            }

            return new Point(x, y);
        }

        /// <summary>
        /// Organizes fences in a tiled layout.
        /// </summary>
        public static void TileLayout(List<dynamic> fences, int columns = 0)
        {
            if (fences == null || !fences.Any()) return;

            var workArea = SystemParameters.WorkArea;

            // Auto-calculate columns if not specified
            if (columns <= 0)
            {
                columns = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(fences.Count)));
            }

            int rows = (int)Math.Ceiling((double)fences.Count / columns);

            double fenceWidth = (workArea.Width - (columns + 1) * DefaultSpacing) / columns;
            double fenceHeight = (workArea.Height - (rows + 1) * DefaultSpacing) / rows;

            // Apply minimum size
            fenceWidth = Math.Max(200, fenceWidth);
            fenceHeight = Math.Max(150, fenceHeight);

            int index = 0;
            for (int row = 0; row < rows && index < fences.Count; row++)
            {
                for (int col = 0; col < columns && index < fences.Count; col++)
                {
                    var fence = fences[index];
                    fence.X = (int)(DefaultSpacing + col * (fenceWidth + DefaultSpacing));
                    fence.Y = (int)(DefaultSpacing + row * (fenceHeight + DefaultSpacing));
                    fence.Width = (int)fenceWidth;
                    fence.Height = (int)fenceHeight;
                    index++;
                }
            }

            FenceDataManager.SaveFenceData();
            LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.FenceUpdate,
                $"Applied tile layout to {fences.Count} fences ({columns} columns)");
        }

        /// <summary>
        /// Stacks fences vertically on the left side.
        /// </summary>
        public static void StackVertically(List<dynamic> fences)
        {
            if (fences == null || !fences.Any()) return;

            double currentY = DefaultSpacing;
            foreach (var fence in fences)
            {
                fence.X = DefaultSpacing;
                fence.Y = (int)currentY;
                currentY += Convert.ToDouble(fence.Height ?? 200) + DefaultSpacing;
            }

            FenceDataManager.SaveFenceData();
        }

        #region Private Helpers

        private static List<Rect> GetOccupiedAreas(List<dynamic> fences)
        {
            var areas = new List<Rect>();

            foreach (var fence in fences)
            {
                try
                {
                    if (fence.IsHidden?.ToString().ToLower() == "true") continue;

                    double x = Convert.ToDouble(fence.X);
                    double y = Convert.ToDouble(fence.Y);
                    double w = Convert.ToDouble(fence.Width ?? 200);
                    double h = Convert.ToDouble(fence.Height ?? 200);

                    areas.Add(new Rect(x - DefaultSpacing, y - DefaultSpacing,
                        w + 2 * DefaultSpacing, h + 2 * DefaultSpacing));
                }
                catch { }
            }

            return areas;
        }

        private static bool IntersectsAny(Rect rect, List<Rect> areas)
        {
            return areas.Any(a => a.IntersectsWith(rect));
        }

        #endregion
    }
}

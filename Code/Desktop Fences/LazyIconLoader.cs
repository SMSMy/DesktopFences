using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Desktop_Fences
{
    /// <summary>
    /// Provides lazy loading functionality for icons.
    /// Icons are loaded on-demand to improve startup performance.
    /// </summary>
    public static class LazyIconLoader
    {
        private static readonly ConcurrentDictionary<string, BitmapSource> _iconCache =
            new ConcurrentDictionary<string, BitmapSource>();

        private static readonly ConcurrentQueue<IconLoadRequest> _loadQueue =
            new ConcurrentQueue<IconLoadRequest>();

        private static CancellationTokenSource _cts;
        private static Task _loaderTask;
        private static bool _isRunning = false;

        private const int BatchSize = 10;
        private const int DelayBetweenBatches = 50; // ms

        /// <summary>
        /// Represents a request to load an icon.
        /// </summary>
        private class IconLoadRequest
        {
            public string FilePath { get; set; }
            public Image TargetImage { get; set; }
            public int IconSize { get; set; }
            public Action<BitmapSource> Callback { get; set; }
        }

        /// <summary>
        /// Event raised when an icon is loaded.
        /// </summary>
        public static event EventHandler<string> IconLoaded;

        /// <summary>
        /// Starts the background icon loader.
        /// </summary>
        public static void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            _cts = new CancellationTokenSource();
            _loaderTask = Task.Run(() => ProcessLoadQueue(_cts.Token));

            LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.Performance,
                "Lazy icon loader started");
        }

        /// <summary>
        /// Stops the background icon loader.
        /// </summary>
        public static void Stop()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _cts?.Cancel();

            try
            {
                _loaderTask?.Wait(1000);
            }
            catch { }

            LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.Performance,
                "Lazy icon loader stopped");
        }

        /// <summary>
        /// Requests an icon to be loaded lazily.
        /// </summary>
        public static void RequestIcon(string filePath, Image targetImage, int iconSize = 48)
        {
            if (string.IsNullOrEmpty(filePath) || targetImage == null) return;

            // Check cache first
            if (_iconCache.TryGetValue(filePath, out var cachedIcon))
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    targetImage.Source = cachedIcon;
                }));
                return;
            }

            // Set placeholder
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                targetImage.Source = GetPlaceholderIcon();
            }));

            // Queue for background loading
            _loadQueue.Enqueue(new IconLoadRequest
            {
                FilePath = filePath,
                TargetImage = targetImage,
                IconSize = iconSize
            });
        }

        /// <summary>
        /// Requests an icon with callback.
        /// </summary>
        public static void RequestIcon(string filePath, int iconSize, Action<BitmapSource> callback)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            // Check cache first
            if (_iconCache.TryGetValue(filePath, out var cachedIcon))
            {
                callback?.Invoke(cachedIcon);
                return;
            }

            _loadQueue.Enqueue(new IconLoadRequest
            {
                FilePath = filePath,
                IconSize = iconSize,
                Callback = callback
            });
        }

        /// <summary>
        /// Gets an icon synchronously (for critical items).
        /// </summary>
        public static BitmapSource GetIconSync(string filePath, int iconSize = 48)
        {
            if (string.IsNullOrEmpty(filePath)) return null;

            if (_iconCache.TryGetValue(filePath, out var cachedIcon))
            {
                return cachedIcon;
            }

            try
            {
                var icon = FenceIconHandler.GetIcon(filePath, iconSize);
                if (icon != null)
                {
                    _iconCache.TryAdd(filePath, icon);
                }
                return icon;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Preloads icons for a list of files.
        /// </summary>
        public static async Task PreloadIconsAsync(IEnumerable<string> filePaths, int iconSize = 48)
        {
            var tasks = new List<Task>();

            foreach (var path in filePaths)
            {
                if (_iconCache.ContainsKey(path)) continue;

                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var icon = FenceIconHandler.GetIcon(path, iconSize);
                        if (icon != null)
                        {
                            _iconCache.TryAdd(path, icon);
                        }
                    }
                    catch { }
                }));
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Clears the icon cache.
        /// </summary>
        public static void ClearCache()
        {
            _iconCache.Clear();
            LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.Performance,
                "Icon cache cleared");
        }

        /// <summary>
        /// Gets cache statistics.
        /// </summary>
        public static (int CachedCount, int QueuedCount) GetStats()
        {
            return (_iconCache.Count, _loadQueue.Count);
        }

        #region Private Methods

        private static async Task ProcessLoadQueue(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    int processed = 0;

                    while (processed < BatchSize && _loadQueue.TryDequeue(out var request))
                    {
                        if (token.IsCancellationRequested) break;

                        await ProcessRequest(request);
                        processed++;
                    }

                    if (processed > 0)
                    {
                        await Task.Delay(DelayBetweenBatches, token);
                    }
                    else
                    {
                        await Task.Delay(100, token); // Wait for new items
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    LogManager.Log(LogManager.LogLevel.Warning, LogManager.LogCategory.Performance,
                        $"Icon loader error: {ex.Message}");
                }
            }
        }

        private static async Task ProcessRequest(IconLoadRequest request)
        {
            try
            {
                BitmapSource icon = null;

                // Check cache again
                if (!_iconCache.TryGetValue(request.FilePath, out icon))
                {
                    // Load icon on background thread
                    icon = await Task.Run(() =>
                        FenceIconHandler.GetIcon(request.FilePath, request.IconSize));

                    if (icon != null)
                    {
                        _iconCache.TryAdd(request.FilePath, icon);
                    }
                }

                if (icon != null)
                {
                    // Update UI
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        if (request.TargetImage != null)
                        {
                            request.TargetImage.Source = icon;
                        }

                        request.Callback?.Invoke(icon);
                    });

                    IconLoaded?.Invoke(null, request.FilePath);
                }
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Debug, LogManager.LogCategory.Performance,
                    $"Failed to load icon for {request.FilePath}: {ex.Message}");
            }
        }

        private static BitmapSource _placeholderIcon;
        private static BitmapSource GetPlaceholderIcon()
        {
            if (_placeholderIcon == null)
            {
                // Create a simple placeholder
                var visual = new DrawingVisual();
                using (var context = visual.RenderOpen())
                {
                    context.DrawRectangle(
                        new SolidColorBrush(Color.FromArgb(50, 128, 128, 128)),
                        null,
                        new Rect(0, 0, 48, 48));
                }

                var bitmap = new RenderTargetBitmap(48, 48, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(visual);
                bitmap.Freeze();
                _placeholderIcon = bitmap;
            }

            return _placeholderIcon;
        }

        #endregion
    }
}

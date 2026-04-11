using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;

namespace Microsoft.Maui.Media
{
    partial class ScreenshotImplementation : IScreenshot
    {
        public bool IsCaptureSupported =>
           true;

        public Task<IScreenshotResult> CaptureAsync()
        {
            var view = Application.Current?.ApplicationLifetime as ISingleViewApplicationLifetime;
            IScreenshotResult result = new ScreenshotResult(view.MainView);
            return Task.FromResult(result);
        }

        public static Task<Stream> CaptureToStreamAsync(Visual visual, ScreenshotFormat format, int quality)
        {
            var pixelSize = new PixelSize((int)visual.Bounds.Width, (int)visual.Bounds.Height);
            var dpi = new Vector(96, 96);
            var bitmap = new RenderTargetBitmap(pixelSize, dpi);

            bitmap.Render(visual);

            Stream stream = new MemoryStream();
            switch (format)
            {
                case ScreenshotFormat.Png:
                    bitmap.Save(stream, quality);
                    break;
                default:
                    throw new NotSupportedException("Unsupported format.");
            }

            stream.Position = 0;
            return Task.FromResult(stream);
        }

    }

    partial class ScreenshotResult : IScreenshotResult
    {
        readonly Visual visual;

        internal ScreenshotResult(Visual visual)
        {
            this.visual = visual;
        }

        Task<Stream> PlatformOpenReadAsync(ScreenshotFormat format, int quality) =>
            ScreenshotImplementation.CaptureToStreamAsync(visual, format, quality);

        public async Task PlatformCopyToAsync(Stream destination, ScreenshotFormat format, int quality)
        {
            using var sourceStream = await PlatformOpenReadAsync(format, quality);
            await sourceStream.CopyToAsync(destination);
        }
    }
}

using Microsoft.Maui.Storage;
using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.Media
{
    partial class MediaPickerImplementation : IMediaPicker
    {
        static byte[]? data;

        [JSImport("mediaCapture.captureVideoInPopup", "essentials")]
        public static partial Task<string> CaptureVideo();

        [JSImport("mediaCapture.sendBlobToDotNet", "essentials")]
        public static partial Task<string> SendBlob(string blob);

        [JSImport("mediaCapture.capturePhotoInPopup", "essentials")]
        public static partial Task<string> CapturePhoto();

        [JSExport]
        internal static void ReceiveBlobData(byte[] bytes)
        {
            data = bytes;
        }

        public bool IsCaptureSupported => true;        

        public Task<FileResult> PickPhotoAsync(MediaPickerOptions? options = null)
        {
            return FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = options?.Title,
                FileTypes = FilePickerFileType.Images
            });
        }

        public async Task<FileResult> CapturePhotoAsync(MediaPickerOptions? options = null)
        {
            var result = await CapturePhoto();
            if (!string.IsNullOrWhiteSpace(result))
            {
                await SendBlob(result);
                return new FileResult(result)
                {
                    Data = data,
                };
            }
            return null!;
        }

        public Task<FileResult> PickVideoAsync(MediaPickerOptions? options = null)
        {
            return FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = options?.Title,
                FileTypes = FilePickerFileType.Videos
            });
        }

        public async Task<FileResult> CaptureVideoAsync(MediaPickerOptions? options = null)
        {
            var result = await CaptureVideo();
            if (!string.IsNullOrWhiteSpace(result))
            {
                await SendBlob(result);
                return new FileResult(result)
                {
                    Data = data,
                };
            }
            return null!;
        }
    }
}

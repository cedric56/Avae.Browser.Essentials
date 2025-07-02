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

        public async Task<FileResult> PickPhotoAsync(MediaPickerOptions? options = null)
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images
            });
            return result;
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
            return null;
        }

        public async Task<FileResult> PickVideoAsync(MediaPickerOptions? options = null)
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Videos
            });
            return result;
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
            return null;
        }
    }
}

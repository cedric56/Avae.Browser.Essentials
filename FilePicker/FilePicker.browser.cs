using Avalonia.Platform.Storage;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Essentials;

namespace Microsoft.Maui.Storage
{
  partial class FilePickerImplementation : IFilePicker
  {
        async Task<IEnumerable<FileResult>> PlatformPickAsync(PickOptions options, bool allowMultiple = false)
        {
            var provider = AvaloniaInterop.GetStorage();
            if (provider == null) return null;

            var pOptions = new FilePickerOpenOptions
            {
                Title = options?.PickerTitle,
                AllowMultiple = allowMultiple,
                //FileTypeFilter = new List<Avalonia.Platform.Storage.FilePickerFileType>()
                //{
                //    new Avalonia.Platform.Storage.FilePickerFileType(string.Empty)
                //    {
                //         MimeTypes = options?.FileTypes?.Value.Select(v=> MimeHelper.GetMimeType(v)).ToArray(),
                //         Patterns = options?.FileTypes?.Value.ToArray()
                //    }
                //}
                FileTypeFilter = options?.FileTypes?.Value.Select(v => new Avalonia.Platform.Storage.FilePickerFileType(string.Empty)
                {
                    AppleUniformTypeIdentifiers = [v],
                    MimeTypes = [MimeHelper.GetMimeType(v)],
                    Patterns = [v]

                }).ToList()
            };

            var results = await provider.OpenFilePickerAsync(pOptions);
            var resultList = new List<FileResult>();
            foreach (var file in results)
            {
                //using var stream = await file.OpenReadAsync();
                //var data = await ReadFully(stream);
                
                var moq = new FileResult(file.Path.OriginalString, MimeHelper.GetMimeType(Path.GetExtension(file.Name)))
                {
                    //Data = data
                    OpenStreamAsync = () => file.OpenReadAsync()
                };
                resultList.Add(moq);
            }

            return resultList;

        }
    }

    public partial class FilePickerFileType
    {
        static FilePickerFileType PlatformImageFileType()
          => new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>()
          {
              {  DevicePlatform.Wasm, new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff" } }
          });

        static FilePickerFileType PlatformPngFileType()
          => new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>()
          {
              { DevicePlatform.Wasm, new[] { ".png" } }
          });

        static FilePickerFileType PlatformJpegFileType()
      => new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>()
      {
          { DevicePlatform.Wasm, new[] { ".jpg", ".jpeg" } }
      });

        static FilePickerFileType PlatformVideoFileType()
      => new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>()
      {
          {      DevicePlatform.Wasm, new[] { ".mp4", ".mov", ".avi", ".mkv", ".wmv" } }
      });

        static FilePickerFileType PlatformPdfFileType()
      => new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>()
      {
          { DevicePlatform.Wasm, new[] { ".pdf" } }
      });

    }
}


public class MimeHelper
{
    public static string GetMimeType(string fileNameOrExt)
    {
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(fileNameOrExt, out string? contentType))
        {
            contentType = "application/octet-stream"; // default fallback
        }
        return contentType;
    }
}

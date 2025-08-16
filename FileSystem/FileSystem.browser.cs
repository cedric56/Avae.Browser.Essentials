using Microsoft.Maui.ApplicationModel;
using System.Collections;

namespace Microsoft.Maui.Storage
{
    partial class FileSystemImplementation : IFileSystem
    {
        string PlatformCacheDirectory => "/_cache";

        string PlatformAppDataDirectory => "/_appdata";

        Task<Stream> PlatformOpenAppPackageFileAsync(string filename)
        {
            var path = Path.Combine(PlatformAppDataDirectory, filename);
            if (File.Exists(path))
            {
                var stream = new MemoryStream(File.ReadAllBytes(path)) as Stream;
                return Task.FromResult(stream);
            }
            return Task.FromException<Stream>(
                new FileNotFoundException($"File '{filename}' not found in app package."));
        }

        Task<bool> PlatformAppPackageFileExistsAsync(string filename)
        {
            return Task.FromResult(File.Exists(Path.Combine(PlatformAppDataDirectory, filename)));
        }

    }

    public partial class FileBase
    {

        static string PlatformGetContentType(string extension) =>
            MimeHelper.GetMimeType(extension);

        internal void Init(FileBase file) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        internal virtual Task<Stream> PlatformOpenReadAsync()
        {
            if (Data is not null)
            {
                using var stream = new MemoryStream(Data);
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                return Task.FromResult<Stream>(stream);
            }

            throw ExceptionUtils.NotSupportedOrImplementedException;
        }

        void PlatformInit(FileBase file)
        { }

    }
}

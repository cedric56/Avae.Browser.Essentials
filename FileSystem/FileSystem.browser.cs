using Microsoft.Maui.ApplicationModel;
using System.Collections;

namespace Microsoft.Maui.Storage
{
    partial class FileSystemImplementation : IFileSystem
    {
        static string PlatformCacheDirectory => "/_cache";

        static string PlatformAppDataDirectory => "/_appdata";

        static Task<Stream> PlatformOpenAppPackageFileAsync(string filename)
        {
            var path = Path.Combine(PlatformAppDataDirectory, filename);
            if (File.Exists(path))
            {
                return Task.FromResult<Stream>(File.OpenRead(path));
            }
            return Task.FromException<Stream>(
                new FileNotFoundException($"File '{filename}' not found in app package."));
        }

        static Task<bool> PlatformAppPackageFileExistsAsync(string filename)
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

        internal Task<Stream> PlatformOpenReadAsync()
        {
            if (Data is not null)
            {
                return Task.FromResult<Stream>(new MemoryStream(Data));
            }
            return Task.FromResult<Stream>(File.OpenRead(FullPath));
        }

        void PlatformInit(FileBase file)
        { }
    }
}

using Microsoft.Maui.Essentials;

namespace Microsoft.Maui.ApplicationModel
{
    partial class LauncherImplementation
    {
        Task<bool> PlatformCanOpenAsync(Uri uri) =>
            Task.FromResult(true);

        Task<bool> PlatformOpenAsync(Uri uri) =>
            OpenAsync(uri.OriginalString);

        Task<bool> PlatformTryOpenAsync(Uri uri) =>
           Task.FromResult(true);

        Task<bool> PlatformOpenAsync(OpenFileRequest request) =>
            OpenAsync(request.File.FullPath);

        private Task<bool> OpenAsync(string url)
        {
            bool result = false;
            try
            {

                JSInterop.Open(url, "_blank");
                result = true;
            }
            catch
            {
                JSInterop.Eval(url);
            }
            return Task.FromResult(result);
        }
    }
}

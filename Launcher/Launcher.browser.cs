using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.ApplicationModel
{
    partial class LauncherImplementation
    {
        [JSImport("globalThis.open")]
        public static partial Task Open(string url, string param = "_blank");

        [JSImport("globalThis.eval")]
        public static partial Task Invoke(string @params);

        Task<bool> PlatformCanOpenAsync(Uri uri) =>
            Task.FromResult(true);

        Task<bool> PlatformOpenAsync(Uri uri) =>
            OpenAsync(uri.OriginalString);

        Task<bool> PlatformTryOpenAsync(Uri uri) =>
           Task.FromResult(true);

        Task<bool> PlatformOpenAsync(OpenFileRequest request) =>
            OpenAsync(request.File.FullPath);

        private async Task<bool> OpenAsync(string url)
        {
            bool result = false;
            try
            {

                await Open(url, "_blank");
                result = true;
            }
            catch
            {
                await Invoke(url);
            }
            return result;
        }
    }
}

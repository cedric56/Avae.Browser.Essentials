using Microsoft.Maui.Essentials;

namespace Microsoft.Maui.ApplicationModel
{
    partial class BrowserImplementation : IBrowser
    {
        public Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options)
        {
            JSInterop.Open(uri.OriginalString, "_blank");
            return Task.FromResult(true);
        }
    }
}

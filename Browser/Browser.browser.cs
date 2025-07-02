using System;
using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.ApplicationModel
{
    partial class BrowserImplementation : IBrowser
    {
        [JSImport("globalThis.open")]
        public static partial Task Open(string url, string param = "_blank");

        public Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options)
        {
            Open(uri.OriginalString, "_blank");
            return Task.FromResult(true);
        }
    }
}

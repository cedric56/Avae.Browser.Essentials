using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.Authentication
{
    partial class WebAuthenticatorImplementation : IWebAuthenticator, IPlatformWebAuthenticatorCallback
    {
        [JSImport("authInterop.authenticate", "essentials")]
        public static partial Task<string> AuthenticateAsync(string authUrl, string redirecturl);

        public async Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions)
        {
            var token = await AuthenticateAsync(webAuthenticatorOptions.Url.ToString(), webAuthenticatorOptions.CallbackUrl.ToString());
            if (webAuthenticatorOptions.ResponseDecoder is not null)
                return new WebAuthenticatorResult(new Uri(token), webAuthenticatorOptions.ResponseDecoder);

            return new WebAuthenticatorResult(new Uri(token));
        }
    }
}


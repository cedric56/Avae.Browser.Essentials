using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.ApplicationModel.Communication
{
    partial class EmailImplementation : IEmail
    {        
        [JSImport("globalThis.open")]
        public static partial Task Open(string url, string param = "_blank");

        public bool IsComposeSupported =>
            true;

        async Task PlatformComposeAsync(EmailMessage message)
        {
            if (message == null)
            {
                await Open("mailto:");
            }
            else
            {
                var recipients = string.Join(",", message.To?.Select(r => Uri.EscapeDataString(r)) ?? []);
                var uri = $"mailto:{recipients}";
                if (!string.IsNullOrEmpty(message.Subject))
                    uri += "?&subject=" + Uri.EscapeDataString(message.Subject);
                if (!string.IsNullOrEmpty(message.Body))
                    uri += "?&body=" + Uri.EscapeDataString(message.Body);
                await Open($"{uri}");
            }
        }
    }
}

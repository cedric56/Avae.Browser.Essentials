using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.ApplicationModel.Communication
{
    partial class SmsImplementation : ISms
    {
        [JSImport("globalThis.eval")]
        public static partial Task Invoke(string @params);

        public bool IsComposeSupported => true;

        async Task PlatformComposeAsync(SmsMessage message)
        {
            var recipients = string.Join(",", message.Recipients.Select(r => Uri.EscapeDataString(r)));
            var uri = $"sms:{recipients}";
            if (!string.IsNullOrEmpty(message?.Body))
                uri += "?&body=" + Uri.EscapeDataString(message.Body);
            await Invoke($"window.location.href='{uri}'");
        }
    }
}


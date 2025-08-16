using Microsoft.Maui.Essentials;

namespace Microsoft.Maui.ApplicationModel.Communication
{
    partial class SmsImplementation : ISms
    {
        public bool IsComposeSupported => true;

        Task PlatformComposeAsync(SmsMessage message)
        {
            var recipients = string.Join(",", message.Recipients.Select(r => Uri.EscapeDataString(r)));
            var uri = $"sms:{recipients}";
            if (!string.IsNullOrEmpty(message?.Body))
                uri += "?&body=" + Uri.EscapeDataString(message.Body);
            JSInterop.Eval($"window.location.href='{uri}'");
            return Task.CompletedTask;
        }
    }
}


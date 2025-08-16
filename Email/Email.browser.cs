using Microsoft.Maui.Essentials;
using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.ApplicationModel.Communication
{
    partial class EmailImplementation : IEmail
    {   
        public bool IsComposeSupported =>
            true;

        Task PlatformComposeAsync(EmailMessage message)
        {
            if (message == null)
            {
                JSInterop.Eval("window.location.href='mailto:'");
            }
            else
            {
                var recipients = string.Join(",", message.To?.Select(r => Uri.EscapeDataString(r)) ?? []);
                var uri = $"mailto:{recipients}";
                if (!string.IsNullOrEmpty(message.Subject))
                    uri += "?&subject=" + Uri.EscapeDataString(message.Subject);
                if (!string.IsNullOrEmpty(message.Body))
                    uri += string.IsNullOrEmpty(message.Subject) ? "?&body=" : "&body" + Uri.EscapeDataString(message.Body);
                JSInterop.Eval($"window.location.href='{uri}'");
            }

            return Task.CompletedTask;
        }
    }
}

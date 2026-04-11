using Microsoft.Maui.Essentials;

namespace Microsoft.Maui.ApplicationModel.DataTransfer
{
    partial class ClipboardImplementation : IClipboard
    {        
        public Task SetTextAsync(string? text)
        {
            var clipboard = AvaloniaInterop.GetClipboard();
            if (clipboard is null)
            {
                return Task.CompletedTask;
            }
            return clipboard.SetTextAsync(text);
        }

        public bool HasText
            => true;

        public Task<string?> GetTextAsync()
        {
            var clipboard = AvaloniaInterop.GetClipboard();
            if (clipboard is null)
            {
                return Task.FromResult<string?>(null);
            }
            return clipboard.GetTextAsync()!;
        }

        void StartClipboardListeners()
            => throw ExceptionUtils.NotSupportedOrImplementedException;

        void StopClipboardListeners()
            => throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}

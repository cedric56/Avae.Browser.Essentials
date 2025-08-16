using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.Devices
{
    partial class FlashlightImplementation : IFlashlight
    {
        [JSImport("flashInterop.on", "essentials")]
        public static partial void On();

        [JSImport("flashInterop.off", "essentials")]
        public static partial void Off();

        public Task<bool> IsSupportedAsync()
        {
            return Task.FromResult(true);
        }

        public Task TurnOffAsync()
        {
            Off();
            return Task.CompletedTask;
        }

        public Task TurnOnAsync()
        {
            On();
            return Task.CompletedTask;
        }
    }
}

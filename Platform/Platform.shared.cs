using Microsoft.Maui.Devices;
using Microsoft.Maui.Networking;
using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.ApplicationModel
{
    /// <summary>
    /// A static class that contains platform-specific helper methods.
    /// </summary>
    public static partial class Platform
    {
        public static async Task Initialize()
        {
            await JSHost.ImportAsync("essentials", "/essentials.js");

            if (DeviceInfo.Current is DeviceInfoImplementation implementation)
            {
                implementation.Initialize();
            }

            if (Connectivity.Current is ConnectivityImplementation connectivity)
            {
                connectivity.Initialize();
            }

            if (Battery.Default is BatteryImplementation battery)
            {
                battery.Initialize();
            }
        }

        [JSImport("fsInterop.initFsSync", "app")]
        public static partial Task InitiateFsSyncAsync();

        [JSImport("fsInterop.persistFs", "app")]
        public static partial Task<bool> PersistFsAsync();

        [JSImport("fsInterop.startPeriodicFlush", "app")]
        public static partial void StartPeriodicFlush();

        [JSImport("fsInterop.stopPeriodicFlush", "app")]
        public static partial void StopPeriodicFlush();
    }
}

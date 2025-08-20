using Microsoft.Maui.Devices;
using Microsoft.Maui.Networking;
using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.ApplicationModel
{
    /// <summary>
    /// A static class that contains platform-specific helper methods.
    /// </summary>
    public static class Platform
    {
        public static async Task Initialize()
        {
            await JSHost.ImportAsync("essentials", "/essentials.js");


            var implementation = DeviceInfo.Current as DeviceInfoImplementation;
            if (implementation != null)
            {
                implementation.Initialize();
            }

            var connectivity = Connectivity.Current as ConnectivityImplementation;
            if (connectivity != null)
            {
                connectivity.Initialize();
            }

            var battery = Battery.Default as BatteryImplementation;
            if (battery != null)
            {
                battery.Initialize();
            }
        }
    }
}

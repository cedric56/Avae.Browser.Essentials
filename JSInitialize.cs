using Microsoft.Maui.Devices;
using Microsoft.Maui.Networking;
using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.Essentials
{
    public static partial class JSInitialize
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

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
        /// <summary>
        /// Initializes the platform-specific services and resources required by the application.
        /// </summary>
        /// <returns></returns>
        public static async Task Initialize()
        {
            await JSHost.ImportAsync("essentials", "/essentials.js");

            await DeviceInfoImplementation.InitializeAsync();

            if (Connectivity.Current is ConnectivityImplementation connectivity)
            {
                connectivity.Initialize();
            }

            if (Battery.Default is BatteryImplementation battery)
            {
                await battery.InitializeAsync();
            }
        }

        /// <summary>
        /// Loads files from indexed db to be available like a normal files for I/O. 
        /// This should be called before any manipulation with files.
        /// </summary>
        /// <returns></returns>
        [JSImport("fsInterop.initFsSync", "essentials")]
        public static partial Task InitiateFsSyncAsync();

        /// <summary>
        /// When called, will persist any operations to an IndexedDB instance.
        /// </summary>
        /// <returns></returns>
        [JSImport("fsInterop.persistFs", "essentials")]
        public static partial Task<bool> PersistFsAsync();

        /// <summary>
        /// Starts a periodic timer every 5 seconds to call PersistFsAsync internally.
        /// </summary>
        [JSImport("fsInterop.startPeriodicFlush", "essentials")]
        public static partial void StartPeriodicFlush();

        /// <summary>
        /// Stops the periodic timer started by StartPeriodicFlush.
        /// </summary>
        [JSImport("fsInterop.stopPeriodicFlush", "essentials")]
        public static partial void StopPeriodicFlush();
    }
}

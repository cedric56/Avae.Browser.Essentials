using Avalonia.Controls;
using Microsoft.Maui.Devices.Sensors;
using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.ApplicationModel
{
    partial class MapImplementation : IMap
    {
        [JSImport("mapInterop.initMap", "essentials")]
        public static partial void Open(double latitude, double longitude, double zoom);

        public Task OpenAsync(double latitude, double longitude, MapLaunchOptions options)
        {
            Open(latitude, longitude, 10);
            return Task.CompletedTask;
        }

        public Task OpenAsync(Placemark placemark, MapLaunchOptions options)
        {
            if (placemark == null)
                throw new ArgumentNullException(nameof(placemark));
            return OpenAsync(placemark.Location.Latitude, placemark.Location.Longitude, options);
        }

        public Task<bool> TryOpenAsync(double latitude, double longitude, MapLaunchOptions options)
        {
            return OpenAsync(latitude, longitude, options).ContinueWith(t => true);
        }

        public Task<bool> TryOpenAsync(Placemark placemark, MapLaunchOptions options)
        {
            return Task.FromResult(true);
        }
    }
}


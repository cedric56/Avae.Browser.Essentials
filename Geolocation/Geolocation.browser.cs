using Microsoft.Maui.ApplicationModel;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;

namespace Microsoft.Maui.Devices.Sensors
{
    partial class GeolocationImplementation : IGeolocation
    {
        [JSImport("geolocationInterop.getCurrentLocation", "essentials")]
        public static partial Task<string> GetCurrentLocation();

        /// <summary>
        /// Indicates if currently listening to location updates while the app is in foreground.
        /// </summary>
        public bool IsListeningForeground { get; set; }

        public async Task<Location?> GetLastKnownLocationAsync()
        {
            var result = await GetCurrentLocation();

            var obj = JsonSerializer.Deserialize<GeolocationResult>(result);
            if (true == obj?.success)
            {
                var moq = new Location()
                {
                    Accuracy = obj.accuracy,
                    Latitude = obj.latitude ?? 0,
                    Longitude = obj.longitude ?? 0,

                };
                return moq;
            }
            return null;
        }

        public async Task<Location?> GetLocationAsync(GeolocationRequest request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var result = await GetCurrentLocation();
            
            var obj = JsonSerializer.Deserialize<GeolocationResult>(result);
            if (true == obj?.success)
            {
                var moq = new Location()
                {
                    Accuracy = obj.accuracy,
                    Latitude = obj.latitude ?? 0,
                    Longitude = obj.longitude ?? 0,                     

                };
                return moq;
            }
            return null;
        }

        /// <summary>
        /// Starts listening to location updates using the <see cref="Geolocation.LocationChanged"/>
        /// event or the <see cref="Geolocation.ListeningFailed"/> event. Events may only sent when
        /// the app is in the foreground. Requests <see cref="Permissions.LocationWhenInUse"/>
        /// from the user.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
        /// <exception cref="FeatureNotSupportedException">Thrown if listening is not supported on this platform.</exception>
        /// <exception cref="InvalidOperationException">Thrown if already listening and <see cref="IsListeningForeground"/> returns <see langword="true"/>.</exception>
        /// <param name="request">The listening request parameters to use.</param>
        /// <returns><see langword="true"/> when listening was started, or <see langword="false"/> when listening couldn't be started.</returns>
        public Task<bool> StartListeningForegroundAsync(GeolocationListeningRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (IsListeningForeground)
                throw new InvalidOperationException("Already listening to location changes.");

            IsListeningForeground = true;

            return Task.FromResult(true);
        }

        /// <summary>
        /// Stop listening for location updates when the app is in the foreground.
        /// Has no effect when not listening and <see cref="Geolocation.IsListeningForeground"/>
        /// is currently <see langword="false"/>.
        /// </summary>
        public void StopListeningForeground()
        {
            IsListeningForeground = false;
        }

        public class GeolocationResult
        {
            public bool success { get; set; }
            public string? message { get; set; }
            public double? latitude { get; set; }
            public double? longitude { get; set; }
            public double? accuracy { get; set; }
            public int? errorCode { get; set; }
        }
    }
}

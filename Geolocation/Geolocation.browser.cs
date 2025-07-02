using Microsoft.Maui.ApplicationModel;
using Newtonsoft.Json;
using System.Runtime.InteropServices.JavaScript;

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

            var obj = JsonConvert.DeserializeObject<GeolocationResult>(result);
            if (true == obj?.Success)
            {
                var moq = new Location()
                {
                    Accuracy = obj.Accuracy,
                    Latitude = obj.Latitude ?? 0,
                    Longitude = obj.Longitude ?? 0,

                };
                return moq;
            }
            return null;
        }

        public async Task<Location?> GetLocationAsync(GeolocationRequest request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var result = await GetCurrentLocation();
            
            var obj = JsonConvert.DeserializeObject<GeolocationResult>(result);
            if (true == obj?.Success)
            {
                var moq = new Location()
                {
                    Accuracy = obj.Accuracy,
                    Latitude = obj.Latitude ?? 0,
                    Longitude = obj.Longitude ?? 0,                     

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
            public bool Success { get; set; }
            public string? Message { get; set; }
            public double? Latitude { get; set; }
            public double? Longitude { get; set; }
            public double? Accuracy { get; set; }
            public int? ErrorCode { get; set; }
        }
    }
}

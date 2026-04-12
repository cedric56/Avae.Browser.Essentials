using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Essentials;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;

namespace Microsoft.Maui.Devices.Sensors
{
    partial class GeolocationImplementation : IGeolocation
    {
        [JSImport("geolocationInterop.getCurrentLocation", "essentials")]
        public static partial Task<string> GetCurrentLocation();

        [JSImport("geolocationInterop.startLocationReading", "essentials")]
        internal static partial int StartLocationReadingInterop(
            [JSMarshalAs<JSType.Function<JSType.String>>] Action<string> onSuccess
            , [JSMarshalAs<JSType.Function<JSType.Number, JSType.String>>] Action<short, string> onError
            , bool highAccuracy = false);

        [JSImport("geolocationInterop.stopLocationReading", "essentials")]
        internal static partial void StopLocationReadingInterop([JSMarshalAs<JSType.Number>] int id);

        /// <summary>
        /// Indicates if currently listening to location updates while the app is in foreground.
        /// </summary>
        public bool IsListeningForeground => watchingId != -1;

        int watchingId = -1;

        public async Task<Location?> GetLastKnownLocationAsync()
        {
            var result = await GetCurrentLocation();

            var obj = JsonSerializer.Deserialize(result, AvaeJsonSerializerContext.Default.GeolocationResultInterop);
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
            
            var obj = JsonSerializer.Deserialize(result, AvaeJsonSerializerContext.Default.GeolocationResultInterop);
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

            watchingId = StartLocationReadingInterop(OnSuccessInterop, OnErrorInterop, request.DesiredAccuracy is GeolocationAccuracy.High or GeolocationAccuracy.Best);

            return Task.FromResult(true);
        }

        /// <summary>
        /// Stop listening for location updates when the app is in the foreground.
        /// Has no effect when not listening and <see cref="Geolocation.IsListeningForeground"/>
        /// is currently <see langword="false"/>.
        /// </summary>
        public void StopListeningForeground()
        {
            StopLocationReadingInterop(watchingId);
            watchingId = -1;
        }

        void OnSuccessInterop(string jsonData)
        {
            var result = JsonSerializer.Deserialize(jsonData, AvaeJsonSerializerContext.Default.GeolocationReadingResultInterop);

            if (result is null)
            {
                return;
            }

            OnLocationChanged(new Location(result.Latitude, result.Longitude)
            {
                Altitude = result.Altitude,
                Accuracy = result.Accuracy,
                Speed = result.Speed,
                Course = result.Course
            });
        }

        void OnErrorInterop(short code, string message)
        {
            if (code == 1)
            {
                OnLocationError(GeolocationError.Unauthorized);
            }
            else
            {
                OnLocationError(GeolocationError.PositionUnavailable);
            }
            StopListeningForeground();
        }
    }
}

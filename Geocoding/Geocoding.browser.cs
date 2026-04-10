using Microsoft.Maui.Essentials;
using System.Text.Json;

namespace Microsoft.Maui.Devices.Sensors
{
    partial class GeocodingImplementation : IGeocoding
    {
        public async Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude)
        {
            using var client = new HttpClient();
            // Nominatim reverse geocoding URL
            string url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latitude}&lon={longitude}&addressdetails=1";

            // Make the request to the Nominatim API
            var response = await client.GetStringAsync(url);

            // Parse the JSON response
            var data = JsonSerializer.Deserialize(response, AvaeJsonSerializerContext.Default.PlacemarkResponseInterop);

            if (data is null)
            {
                return [new()];
            }

            // Return the full address (can also return specific components)
            return
            [
                new()
                {
                    CountryName = data.Address?.Country ?? string.Empty,
                    CountryCode = data.Address?.CcountryCode ?? string.Empty,
                    Location = new Location() { Latitude = data.LatValue, Longitude = data.LonValue },
                    FeatureName = data.Address?.Road ?? string.Empty,
                    Locality = data.Address?.Village ?? string.Empty,
                    PostalCode = data.Address?.Postcode ?? string.Empty
                }
            ];
        }

        public async Task<IEnumerable<Location>> GetLocationsAsync(string address)
        {
            using var client = new HttpClient();
            // Nominatim request URL
            string url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(address)}";

            // Make the request to the Nominatim API
            var response = await client.GetStringAsync(url);

            // Parse the JSON response
            var data = JsonSerializer.Deserialize(response, AvaeJsonSerializerContext.Default.IEnumerableNominatimResponse);
            if (data is null)
            {
                return [];
            }
            return [.. data.Select(d =>
            {
                return new Location()
                {
                    Latitude =  d.LatValue,
                    Longitude = d.LonValue
                };

            })];
        }
    }
}

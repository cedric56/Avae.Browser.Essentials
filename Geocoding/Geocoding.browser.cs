using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Maui.Devices.Sensors
{
    class GeocodingImplementation : IGeocoding
    {
        public async Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude)
        {
            using var client = new HttpClient();
            // Nominatim reverse geocoding URL
            string url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latitude}&lon={longitude}&addressdetails=1";

            // Make the request to the Nominatim API
            var response = await client.GetStringAsync(url);

            // Parse the JSON response
            var data = JsonSerializer.Deserialize<Root>(response);

            // Return the full address (can also return specific components)
            return new List<Placemark>()
                {
                    new Placemark()
                {
                    CountryName = data.address.country,
                    CountryCode = data.address.country_code,
                    Location = new Location() { Latitude = data.LatValue, Longitude = data.LonValue },
                    FeatureName = data.address.road,
                    Locality = data.address.village,
                    PostalCode = data.address.postcode,

                }
            };
        }

        public async Task<IEnumerable<Location>> GetLocationsAsync(string address)
        {
            using var client = new HttpClient();
            // Nominatim request URL
            string url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(address)}";

            // Make the request to the Nominatim API
            var response = await client.GetStringAsync(url);

            // Parse the JSON response
            var data = JsonSerializer.Deserialize<IEnumerable<NominatimResponse>>(response);
            return data.Select(d =>
            {
                return new Location()
                {
                    Latitude =  d.LatValue,
                    Longitude = d.LonValue
                };

            }).ToList();
        }

        public class Root
        {
            public int place_id { get; set; }
            public string licence { get; set; }
            public string osm_type { get; set; }
            public int osm_id { get; set; }
            public string lat { get; set; }
            public string lon { get; set; }
            public string @class { get; set; }
            public string type { get; set; }
            public int place_rank { get; set; }
            public string importance { get; set; }
            public string addresstype { get; set; }
            public string name { get; set; }
            public string display_name { get; set; }
            public Address address { get; set; }
            public List<string> boundingbox { get; set; }

            public double LatValue => double.Parse(lat, CultureInfo.InvariantCulture);
            public double LonValue => double.Parse(lon, CultureInfo.InvariantCulture);
            public double Importance => double.Parse(importance, CultureInfo.InvariantCulture);

        }

        public class Address
        {
            public string road { get; set; }
            public string hamlet { get; set; }
            public string village { get; set; }
            public string municipality { get; set; }
            public string county { get; set; }

            [JsonPropertyName("ISO3166-2-lvl6")]
            public string ISO31662lvl6 { get; set; }
            public string state { get; set; }

            [JsonPropertyName("ISO3166-2-lvl4")]
            public string ISO31662lvl4 { get; set; }
            public string region { get; set; }
            public string postcode { get; set; }
            public string country { get; set; }
            public string country_code { get; set; }
        }

        public class NominatimResponse
        {
            [JsonPropertyName("lat")]
            public string Lat { get; set; }

            [JsonPropertyName("lon")]
            public string Lon { get; set; }


            public double LatValue => double.Parse(Lat, CultureInfo.InvariantCulture);
            public double LonValue => double.Parse(Lon, CultureInfo.InvariantCulture);
        }
    }
}

using System.Globalization;
using System.Text.Json.Serialization;

namespace Microsoft.Maui.Devices.Sensors
{
    internal class NominatimResponse
    {
        [JsonPropertyName("lat")]
        public string Lat { get; set; } = null!;

        [JsonPropertyName("lon")]
        public string Lon { get; set; } = null!;


        public double LatValue => double.Parse(Lat, CultureInfo.InvariantCulture);
        public double LonValue => double.Parse(Lon, CultureInfo.InvariantCulture);
    }
}

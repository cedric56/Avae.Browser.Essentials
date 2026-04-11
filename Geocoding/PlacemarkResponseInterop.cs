using System.Globalization;
using System.Text.Json.Serialization;

namespace Microsoft.Maui.Devices.Sensors
{
    internal class PlacemarkResponseInterop
    {
        [JsonPropertyName("place_id")]
        public int PlaceId { get; set; }

        [JsonPropertyName("licence")]
        public string? License { get; set; }

        [JsonPropertyName("osm_type")]
        public string? OsmType { get; set; }

        [JsonPropertyName("osm_id")]
        public int OsmId { get; set; }

        [JsonPropertyName("lat")]
        public string? Lat { get; set; }

        [JsonPropertyName("lon")]
        public string? Lon { get; set; }

        [JsonPropertyName("class")]
        public string? Class { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("place_rank")]
        public int PlaceRank { get; set; }

        [JsonPropertyName("importance")]
        public string? ImportanceStr { get; set; }

        [JsonPropertyName("addresstype")]
        public string? AddressType { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("address")]
        public Address? Address { get; set; }

        [JsonPropertyName("boundingbox")]
        public List<string>? boundingbox { get; set; }

        public double LatValue => double.Parse(Lat!, CultureInfo.InvariantCulture);
        public double LonValue => double.Parse(Lon!, CultureInfo.InvariantCulture);
        public double Importance => double.Parse(ImportanceStr!, CultureInfo.InvariantCulture);

    }
}

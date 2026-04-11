using System.Text.Json.Serialization;

namespace Microsoft.Maui.ApplicationModel.Communication
{
    internal class ContactsResponseInterop
    {
        [JsonPropertyName("name")]
        public List<string>? Name { get; set; }

        [JsonPropertyName("email")]
        public List<string>? Email { get; set; }

        [JsonPropertyName("tel")]
        public List<string>? Tel { get; set; }

        [JsonPropertyName("address")]
        public List<string>? Address { get; set; }
    }
}

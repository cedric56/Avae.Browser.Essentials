using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Media;
using System.Text.Json.Serialization;

namespace Microsoft.Maui.Essentials;

[JsonSerializable(typeof(BatteryResult))]
[JsonSerializable(typeof(DeviceDisplayInfo))]
[JsonSerializable(typeof(ContactsResponseInterop))]
[JsonSerializable(typeof(List<ContactsResponseInterop>))]
[JsonSerializable(typeof(BrowserInfo))]
[JsonSerializable(typeof(PlacemarkResponseInterop))]
[JsonSerializable(typeof(IEnumerable<NominatimResponse>))]
[JsonSerializable(typeof(GeolocationResultInterop))]
[JsonSerializable(typeof(TextToSpeechResponseInterop))]
[JsonSerializable(typeof(IEnumerable<TextToSpeechResponseInterop>))]
[JsonSerializable(typeof(GeolocationReadingResultInterop))]
internal partial class AvaeJsonSerializerContext : JsonSerializerContext
{
}

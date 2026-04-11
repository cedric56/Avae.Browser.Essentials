using Microsoft.Maui.Essentials;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;

namespace Microsoft.Maui.Devices
{
    internal partial class DeviceInfoImplementation : IDeviceInfo
    {
        [JSImport("browserDetect.get", "essentials")]
        public static partial Task<string> GetInfos();

        [JSExport]
        public static void OnOSUpdate(string value)
        {
            osInfo = value;
        }

        [JSExport]
        public static void OnOSArchitecture(string value)
        {
            browserInfo.CPUArchitect = value;
        }

        private static string? osInfo;
        private static BrowserInfo browserInfo = new();

        public string Model => browserInfo.DeviceModel ?? string.Empty;

        public string Manufacturer
        {
            get
            {
                if (!string.IsNullOrEmpty(browserInfo.DeviceVendor))
                    return browserInfo.DeviceVendor;

                if (RealPlatform == DevicePlatform.iOS || 
                    RealPlatform == DevicePlatform.MacCatalyst ||
                    RealPlatform == DevicePlatform.macOS)
                    return "Apple Inc.";

                return browserInfo.GPUVendor ?? string.Empty;
            }
        }

        public string Name
        {
            get
            {
                return browserInfo.BrowserName ?? string.Empty;
            }
        }

        public string VersionString => browserInfo.OSName + " " + osInfo;

        public Version Version
        {
            get
            {
                int.TryParse(browserInfo.BrowserMajor, out int major);
                int.TryParse(browserInfo.BrowserVersion, out int minor);
                return new Version(major, minor);
            }
        }

        public DevicePlatform RealPlatform
        {
            get
            {
                if (true == browserInfo.IsAndroid)
                    return DevicePlatform.Android;

                if (true == browserInfo.IsIPad ||
                    true == browserInfo.IsIPadPro ||
                    true == browserInfo.IsIPhone)
                    return DevicePlatform.iOS;


                if (true == browserInfo.OSName?.Contains("Windows", StringComparison.OrdinalIgnoreCase))
                    return DevicePlatform.WinUI;

                if (true == browserInfo.OSName?.Contains("LINUX", StringComparison.OrdinalIgnoreCase))
                    return DevicePlatform.Linux;

                if (true == browserInfo.OSName?.Contains("MAC", StringComparison.OrdinalIgnoreCase))
                    return DevicePlatform.macOS;

                return DevicePlatform.Unknown;
            }
        }

        public DevicePlatform Platform
        {
            get
            {  
                return DevicePlatform.Wasm;
            }
        }

        public DeviceIdiom Idiom
        {
            get
            {
                if (browserInfo.IsMobile ?? false)
                    return DeviceIdiom.Phone;
                else if (browserInfo.IsTablet ?? false)
                    return DeviceIdiom.Tablet;
                else if (browserInfo.IsDesktop ?? false)
                    return DeviceIdiom.Desktop;

                return DeviceIdiom.Unknown;
            }
        }

        public DeviceType DeviceType
        {
            get
            {
                if (Enum.TryParse<DeviceType>(browserInfo.DeviceType, out var result))
                {
                    return result;
                }
                return DeviceType.Unknown;
            }
        }

        public static async Task InitializeAsync()
        {
            var infos = await GetInfos();
            browserInfo = JsonSerializer.Deserialize(infos, AvaeJsonSerializerContext.Default.BrowserInfo) ?? new BrowserInfo();
        }
    }
}

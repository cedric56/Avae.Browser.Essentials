using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;

namespace Microsoft.Maui.Devices
{
    public partial class DeviceInfoImplementation : IDeviceInfo
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

        private static string osInfo;
        private static BrowserInfo browserInfo = new BrowserInfo();

        public string Model => browserInfo.DeviceModel;

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

                return browserInfo.GPUVendor;
            }
        }

        public string Name
        {
            get
            {
                return browserInfo.BrowserName;
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


                if (true == browserInfo.OSName?.Contains("Windows"))
                    return DevicePlatform.WinUI;

                if (true == browserInfo.OSName?.ToUpper().Contains("LINUX"))
                    return DevicePlatform.Linux;

                if (true == browserInfo.OSName?.ToUpper().Contains("MAC"))
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
                Enum.TryParse<DeviceType>(browserInfo.DeviceType, out var result);
                return result;
            }
        }

        public async void Initialize()
        {
            var infos = await GetInfos();
            browserInfo = JsonSerializer.Deserialize<BrowserInfo>(infos);
        }

        /// <summary>
        /// Class BrowserInfo.
        /// </summary>
        public class BrowserInfo
        {
            /// <summary>
            /// Gets or sets the browser major.
            /// </summary>
            /// <value>The browser major.</value>
            public string? BrowserMajor { get; set; }
            /// <summary>
            /// Gets or sets the name of the browser.
            /// </summary>
            /// <value>The name of the browser.</value>
            public string? BrowserName { get; set; }
            /// <summary>
            /// Gets or sets the browser version.
            /// </summary>
            /// <value>The browser version.</value>
            public string? BrowserVersion { get; set; }
            /// <summary>
            /// Gets or sets the cpu architect.
            /// </summary>
            /// <value>The cpu architect.</value>
            public string? CPUArchitect { get; set; }
            /// <summary>
            /// Gets or sets the device model.
            /// </summary>
            /// <value>The device model.</value>
            public string? DeviceModel { get; set; }
            /// <summary>
            /// Gets or sets the type of the device.
            /// </summary>
            /// <value>The type of the device.</value>
            public string? DeviceType { get; set; }
            /// <summary>
            /// Gets or sets the device vendor.
            /// </summary>
            /// <value>The device vendor.</value>
            public string? DeviceVendor { get; set; }
            /// <summary>
            /// Gets or sets the name of the engine.
            /// </summary>
            /// <value>The name of the engine.</value>
            public string? EngineName { get; set; }
            /// <summary>
            /// Gets or sets the engine version.
            /// </summary>
            /// <value>The engine version.</value>
            public string? EngineVersion { get; set; }
            /// <summary>
            /// Gets or sets the gpu renderer.
            /// </summary>
            /// <value>The gpu renderer.</value>
            public string? GPURenderer { get; set; }
            /// <summary>
            /// Gets or sets the gpu vendor.
            /// </summary>
            /// <value>The gpu vendor.</value>
            public string? GPUVendor { get; set; }
            /// <summary>
            /// Gets or sets the ip.
            /// </summary>
            /// <value>The ip.</value>
            public string? IP { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether this instance is android.
            /// </summary>
            /// <value><c>null</c> if [is android] contains no value, <c>true</c> if [is android]; otherwise, <c>false</c>.</value>
            public bool? IsAndroid { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether this instance is desktop.
            /// </summary>
            /// <value><c>null</c> if [is desktop] contains no value, <c>true</c> if [is desktop]; otherwise, <c>false</c>.</value>
            public bool? IsDesktop { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether this instance is i pad.
            /// </summary>
            /// <value><c>null</c> if [is i pad] contains no value, <c>true</c> if [is i pad]; otherwise, <c>false</c>.</value>
            public bool? IsIPad { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether this instance is i pad pro.
            /// </summary>
            /// <value><c>null</c> if [is i pad pro] contains no value, <c>true</c> if [is i pad pro]; otherwise, <c>false</c>.</value>
            public bool? IsIPadPro { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether this instance is i phone.
            /// </summary>
            /// <value><c>null</c> if [is i phone] contains no value, <c>true</c> if [is i phone]; otherwise, <c>false</c>.</value>
            public bool? IsIPhone { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether this instance is mobile.
            /// </summary>
            /// <value><c>null</c> if [is mobile] contains no value, <c>true</c> if [is mobile]; otherwise, <c>false</c>.</value>
            public bool? IsMobile { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether this instance is tablet.
            /// </summary>
            /// <value><c>null</c> if [is tablet] contains no value, <c>true</c> if [is tablet]; otherwise, <c>false</c>.</value>
            public bool? IsTablet { get; set; }
            /// <summary>
            /// Gets or sets the name of the os.
            /// </summary>
            /// <value>The name of the os.</value>
            public string? OSName { get; set; }
            /// <summary>
            /// Gets or sets the os version.
            /// </summary>
            /// <value>The os version.</value>
            public string? OSVersion { get; set; }
            /// <summary>
            /// Gets or sets the screen resolution.
            /// </summary>
            /// <value>The screen resolution.</value>
            public string? ScreenResolution { get; set; }
            /// <summary>
            /// Gets or sets the time zone.
            /// </summary>
            /// <value>The time zone.</value>
            public string? TimeZone { get; set; }
            /// <summary>
            /// Gets or sets the user agent.
            /// </summary>
            /// <value>The user agent.</value>
            public string? UserAgent { get; set; }
        }
    }
}

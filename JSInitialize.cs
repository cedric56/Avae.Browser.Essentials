using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Accessibility;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Media;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.Essentials
{
    public static partial class JSInitialize
    {

        public static async Task Initialize()
        {
            await JSHost.ImportAsync("essentials", "/essentials.js");


            var implementation = DeviceInfo.Current as DeviceInfoImplementation;
            if (implementation != null)
            {
                implementation.Initialize();
            }

            var connectivity = Connectivity.Current as ConnectivityImplementation;
            if (connectivity != null)
            {
                connectivity.Initialize();
            }

            var battery = Battery.Default as BatteryImplementation;
            if (battery != null)
            {
                battery.Initialize();
            }
        }

        public static void ConfigureEssentials(this IServiceCollection services)
        {
            services.AddSingleton(_ => Compass.Default);
            services.AddSingleton(_ => MediaPicker.Default);
            services.AddSingleton(_ => Accelerometer.Default);
            services.AddSingleton(_ => Magnetometer.Default);
            services.AddSingleton(_ => Barometer.Default);
            services.AddSingleton(_ => OrientationSensor.Default);
            services.AddSingleton(_ => Vibration.Default);
            services.AddSingleton(_ => WebAuthenticator.Default);
            services.AddSingleton(_ => Geolocation.Default);
            services.AddSingleton(_ => Geocoding.Default);
            services.AddSingleton(_ => Battery.Default);
            services.AddSingleton(_ => Sms.Default);
            services.AddSingleton(_ => Preferences.Default);
            services.AddSingleton(_ => TextToSpeech.Default);
            services.AddSingleton(_ => Email.Default);
            services.AddSingleton(_ => Microsoft.Maui.ApplicationModel.Communication.Contacts.Default);
            services.AddSingleton(_ => Connectivity.Current);
            services.AddSingleton(_ => AppInfo.Current);
            services.AddSingleton(_ => Browser.Default);
            services.AddSingleton(_ => Map.Default);
            services.AddSingleton(_ => DeviceInfo.Current);
            services.AddSingleton(_ => Share.Default);
            services.AddSingleton(_ => Flashlight.Default);
            services.AddSingleton(_ => PhoneDialer.Default);
            services.AddSingleton(_ => Launcher.Default);
            services.AddSingleton(_ => HapticFeedback.Default);
            services.AddSingleton(_ => Gyroscope.Default);
            services.AddSingleton(_ => VersionTracking.Default);
            services.AddSingleton(_ => SemanticScreenReader.Default);
            services.AddSingleton(_ => SecureStorage.Default);
            services.AddSingleton(_ => FileSystem.Current);
            services.AddSingleton(_ => DeviceDisplay.Current);
            services.AddSingleton(_ => Screenshot.Default);
            services.AddSingleton(_ => FilePicker.Default);
            services.AddSingleton(_ => AppActions.Current);

        }
    }

}

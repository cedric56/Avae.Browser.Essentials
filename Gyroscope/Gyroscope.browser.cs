using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.Devices.Sensors
{
    partial class GyroscopeImplementation : IGyroscope
    {
        [JSImport("gyroscopeInterop.startListening", "essentials")]
        public static partial void StartListening(int frequancy);

        [JSImport("gyroscopeInterop.stopListening", "essentials")]
        public static partial void StopListening();

        [JSExport]
        public static void OnReadingChanged(double x, double y, double z)
        {
            if (Gyroscope.Default is not GyroscopeImplementation implementation)
                return;

            if (!implementation.IsMonitoring)
                return;

            implementation.RaiseReadingChanged(new GyroscopeData(x, y, z));
        }

        static bool PlatformIsSupported => true;

        void PlatformStart(SensorSpeed sensorSpeed)
        {
            StartListening(GetFrequency(sensorSpeed));
            IsMonitoring = true;
        }

        void PlatformStop() {
            StopListening();
            IsMonitoring = false;
        }

        private static int GetFrequency(SensorSpeed sensorSpeed)
        {
            return sensorSpeed switch
            {
                SensorSpeed.Default => 30,
                SensorSpeed.UI => 15,
                SensorSpeed.Game => 60,
                SensorSpeed.Fastest => 100,
                _ => 30,
            };
        }
    }
}

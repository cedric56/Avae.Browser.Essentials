using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.Devices.Sensors
{
    partial class MagnetometerImplementation
    {
        [JSImport("magnetometerInterop.startListening", "essentials")]
        public static partial void StartListening(int frequency);

        [JSImport("magnetometerInterop.stopListening", "essentials")]
        public static partial void StopListening();

        [JSExport]
        public static void OnReadingChanged(double x, double y, double z)
        {
            var implementation = Magnetometer.Default as MagnetometerImplementation;
            if(implementation == null)
                return;

            if (!implementation.IsMonitoring)
                return;

            implementation.RaiseReadingChanged(new MagnetometerData(x, y, z));
        }

        bool PlatformIsSupported =>
            true;

        void PlatformStart(SensorSpeed sensorSpeed)
        {            
            StartListening(GetFrequency(sensorSpeed));
            IsMonitoring = true;
        }

        void PlatformStop()
        {
            StopListening();
            IsMonitoring = false;
        }

        private int GetFrequency(SensorSpeed sensorSpeed)
        {
            switch (sensorSpeed)
            {
                case SensorSpeed.Default:
                    return 30;
                case SensorSpeed.UI:
                    return 15;
                case SensorSpeed.Game:
                    return 60;
                case SensorSpeed.Fastest:
                    return 100;
            }
            return 30;
        }
    }
}

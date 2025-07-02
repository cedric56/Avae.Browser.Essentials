using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Devices.Sensors
{
    partial class OrientationSensorImplementation : IOrientationSensor
    {
        [JSImport("orientationInterop.startListening", "essentials")]
        public static partial void StartListening(int frequency);

        [JSImport("orientationInterop.stopListening", "essentials")]
        public static partial void StopListening();

        [JSExport]
        public static void OnReadingChanged(double x, double y, double z, double w)
        {
            var implementation = OrientationSensor.Default as OrientationSensorImplementation;

            if (implementation == null)
                return;

            if (!implementation.IsMonitoring)
                return;

            implementation.RaiseReadingChanged(new OrientationSensorData(x, y, z, w));
        }

        bool PlatformIsSupported =>
            true;

        void PlatformStart(SensorSpeed sensorSpeed) {            
            StartListening(GetFrequency(sensorSpeed));
            IsMonitoring = true;
        }
            

        void PlatformStop() {
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

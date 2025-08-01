﻿using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.Devices.Sensors
{
    internal partial class AccelerometerImplementation : AccelerometerImplementationBase
    {
        [JSImport("accelerometerInterop.startListening", "essentials")]
        public static partial void StartListening(int frequency);

        [JSImport("accelerometerInterop.stopListening", "essentials")]
        public static partial void StopListening();

        [JSExport]
        public static void OnReadingChanged(double x, double y, double z)
        {
            var implementation = Accelerometer.Default as AccelerometerImplementation;
            if (implementation == null)
                return; 
            if (!implementation.IsMonitoring)
                return;

            implementation.OnChanged(new AccelerometerChangedEventArgs(new AccelerometerData(x, y, z)));
        }

        public override bool IsSupported => true;

        protected override void PlatformStart(SensorSpeed sensorSpeed)
        {
            StartListening(GetFrequency(sensorSpeed));
        }

        protected override void PlatformStop()
        {
            StopListening();
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

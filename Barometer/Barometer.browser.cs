using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Devices.Sensors
{
    partial class BarometerImplementation : IBarometer
    {
        void PlatformStart(SensorSpeed sensorSpeed)
            => throw ExceptionUtils.NotSupportedOrImplementedException;

        void PlatformStop()
            => throw ExceptionUtils.NotSupportedOrImplementedException;

        public bool IsSupported
            => false;
    }
}

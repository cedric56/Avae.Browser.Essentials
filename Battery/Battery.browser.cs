using Newtonsoft.Json;
using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.Devices
{
    public partial class BatteryImplementation : IBattery
    {
        [JSImport("batteryInterop.getBatteryStatus", "essentials")]
        public static partial Task<string> GetBatteryStatus();

        [JSExport]
        public static void OnBatteryChanged(double level, bool charging, double chargingTime, double dischargingTime)
        {
            var implementation = Battery.Default as BatteryImplementation;
            if (implementation == null)
                return;

            implementation.chargeLevel = level;
            implementation.batteryState = charging ? BatteryState.Charging : BatteryState.Discharging;
            implementation.batteryPowerSource = charging ? BatteryPowerSource.AC : BatteryPowerSource.Battery;
            implementation.OnBatteryInfoChanged();
        }

        private double chargeLevel;
        private BatteryState batteryState;
        private BatteryPowerSource batteryPowerSource;
        private EnergySaverStatus energySaverStatus;

        public double ChargeLevel => chargeLevel;

        public BatteryState State => batteryState;

        public BatteryPowerSource PowerSource => batteryPowerSource;

        public EnergySaverStatus EnergySaverStatus => EnergySaverStatus.Off;

        public async void Initialize()
        {
            var result = await GetBatteryStatus();
            var obj = JsonConvert.DeserializeObject<BatteryResult>(result);
            if (true == obj?.success)
            {
                chargeLevel = obj.Level;
                batteryState = obj.Charging ? BatteryState.Charging : BatteryState.Discharging;
                batteryPowerSource = obj.Charging ? BatteryPowerSource.AC : BatteryPowerSource.Battery;
                
                //energySaverStatus = obj.DischargingTime
            }
        }

        void StartBatteryListeners()
        {
            
        }

        void StopBatteryListeners()
        {
        }        

        void StartEnergySaverListeners()
        {
        }

        void StopEnergySaverListeners()
        {
        }

        public class BatteryResult
        {
            public bool success { get; set; }
            public string? Message { get; set; }
            public bool Charging { get; set; }
            public double? ChargingTime { get; set; }
            public double? DischargingTime { get; set; }
            public double Level { get; set; }
            public int? ErrorCode { get; set; }
        }
    }
}

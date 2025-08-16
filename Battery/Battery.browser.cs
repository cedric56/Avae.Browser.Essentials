using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;

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
            //var obj = JsonConvert.DeserializeObject<BatteryResult>(result);
            var obj = JsonSerializer.Deserialize<BatteryResult>(result);
            if (true == obj?.success)
            {
                chargeLevel = obj.level;
                batteryState = obj.charging ? BatteryState.Charging : BatteryState.Discharging;
                batteryPowerSource = obj.charging ? BatteryPowerSource.AC : BatteryPowerSource.Battery;
                
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
            public string? message { get; set; }
            public bool charging { get; set; }
            public double? chargingTime { get; set; }
            public double? dischargingTime { get; set; }
            public double level { get; set; }
            public int? errorCode { get; set; }
        }
    }
}

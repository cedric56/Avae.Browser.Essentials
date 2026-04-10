using Microsoft.Maui.Essentials;
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
            if (Battery.Default is not BatteryImplementation implementation)
                return;

            implementation.chargeLevel = level;
            implementation.batteryState = charging ? BatteryState.Charging : BatteryState.Discharging;
            implementation.batteryPowerSource = charging ? BatteryPowerSource.AC : BatteryPowerSource.Battery;
            implementation.OnBatteryInfoChanged();
        }

        private double chargeLevel;
        private BatteryState batteryState;
        private BatteryPowerSource batteryPowerSource;

        public double ChargeLevel => chargeLevel;

        public BatteryState State => batteryState;

        public BatteryPowerSource PowerSource => batteryPowerSource;

        public EnergySaverStatus EnergySaverStatus => EnergySaverStatus.Off;

        public async Task InitializeAsync()
        {
            var result = await GetBatteryStatus();

            var obj = JsonSerializer.Deserialize(result, AvaeJsonSerializerContext.Default.BatteryResult);
            if (obj?.Success ?? false)
            {
                chargeLevel = obj.Level;
                batteryState = obj.Charging ? BatteryState.Charging : BatteryState.Discharging;
                batteryPowerSource = obj.Charging ? BatteryPowerSource.AC : BatteryPowerSource.Battery;
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
    }
}

using Shiny.BluetoothLE;

namespace SukaliCareApp.Services
{
    public class AppBluetoothService : ReactiveObject
    {
        private readonly IBleManager bleManager;
        
        public AppBluetoothService(IBleManager bleManager)
        {
            this.bleManager = bleManager;
        }
        public IPeripheral ConnectedDevice(string uuid) => bleManager.GetKnownPeripheral(uuid);
        public IEnumerable<IPeripheral> GetConnetedDevices() => bleManager.GetConnectedPeripherals();

        public async Task ScanDevices()
        {
            var scanner = bleManager.CreateManagedScanner();

            await scanner.Start();
            //scanner.Peripherals;

            scanner.Stop();
        }

        public void OpenScan()
        {
            var scanner = bleManager.Scan()
                .Subscribe(result =>
                {

                });
            scanner.Dispose();
        }

    }
}

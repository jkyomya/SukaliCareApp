using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Interfaces;
using Mopups.Services;
using Plugin.BLE.Abstractions.Contracts;
using Shiny.BluetoothLE;
using System.Diagnostics;

namespace SukaliCareApp.ViewModels
{
    public partial class DeviceMgtViewModel : BaseViewModel
    {
        private readonly CgmDataService dataService;
        private readonly IPopupNavigation popupNavigation;
        private readonly IBleManager bleManager;
        private readonly IAdapter adapter;
        private readonly IBluetoothService bluetoothService;
        private readonly AddDeviceViewModel addDeviceViewModel;

        [ObservableProperty]
        bool isRefreshing = true;
        public ObservableCollection<CGMDevice> ScannedDevices { get; } = [];

        public DeviceMgtViewModel(
                CgmDataService dataService,
                IPopupNavigation popupNavigation,
                IBleManager bleManager,
                IAdapter adapter,
                IBluetoothService bluetoothService,
                AddDeviceViewModel addDeviceViewModel)
        {
            this.dataService = dataService;
            this.popupNavigation = popupNavigation;
            this.bleManager = bleManager;
            this.adapter = adapter;
            this.bluetoothService = bluetoothService;
            this.addDeviceViewModel = addDeviceViewModel;

            WeakReferenceMessenger.Default.Register<CGMDevice>(this, (r, m) =>
            {
                OnDeviceConnected(m);
            });
        }
        
        public ObservableCollection<CGMDevice> PairedDevices { get; set; } = [];

        [RelayCommand]
        public void LoadData()
        {
            if(Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>().Result != PermissionStatus.Granted)
            {
                Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
            IsRefreshing = true;

            PairedDevices.Clear();
            var devices = bluetoothService.GetConnectedDevices();
            foreach (var item in devices)
            {
                PairedDevices.Add(new CGMDevice
                {
                    DeviceId = item.Id.ToString() ?? Guid.NewGuid().ToString(),
                    DeviceName = item.Name,
                    Uuid = item.Rssi.ToString() ?? string.Empty
                });
            }


            IsRefreshing = false;
        }

        [RelayCommand]
        public async Task ScanDevicesAsync()
        {
            await popupNavigation.PushAsync(new AddDeviceView(addDeviceViewModel));
        }

        private void OnDeviceConnected(CGMDevice device) => PairedDevices.Add(device);

        [RelayCommand]
        public async Task ScanAllDevices()
        {
            IsRefreshing = true;
            try
            {
                var scanSession = bleManager.Scan()
                    .Subscribe(result =>
                    {
                        Debug.WriteLine(result.Rssi);
                        if (!string.IsNullOrEmpty(result.Peripheral.Name))
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                ScannedDevices.Add(new CGMDevice
                                {
                                    DeviceId = result.Peripheral.Uuid.ToString() ?? Guid.NewGuid().ToString(),
                                    DeviceName = result.Peripheral.Name,
                                    Uuid = result.Peripheral.Uuid.ToString() ?? string.Empty
                                });
                            });

                        }
                    });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(
                      "Scan Error",
                      ex.Message,
                      "Ok");
            }
            finally
            {
                IsRefreshing = false;
            }

        }
    }
}

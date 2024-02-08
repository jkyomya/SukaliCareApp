using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Services;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using Shiny;
using Shiny.BluetoothLE;
using Shiny.BluetoothLE.Managed;
using System.Diagnostics;
using System.Reactive.Linq;
using Permissions = Microsoft.Maui.ApplicationModel.Permissions;

namespace SukaliCareApp.ViewModels;

public partial class AddDeviceViewModel : BaseViewModel
{
    private readonly IBluetoothService bluetoothService;
    private readonly IAdapter adapter;

    [ObservableProperty]
    ObservableCollection<IDevice> scannedDevices = [];

    [ObservableProperty]
    private bool isScanning = false;

    public AddDeviceViewModel(IBluetoothService bluetoothService, IAdapter adapter)
    {
        this.bluetoothService = bluetoothService;
        this.adapter = adapter;

        adapter.DeviceDiscovered += (s, a) =>
        {
            Debug.WriteLine($"Found scanned device: {a.Device.Name}");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ScannedDevices.Add(a.Device);
            });
        };
    }

    

    [RelayCommand]
    public async Task CloseDialog()
    {
        await MopupService.Instance.PopAllAsync();
    }

    [RelayCommand]
    public async Task ScanDevices()
    {
        if ((await Permissions.CheckStatusAsync<Permissions.Bluetooth>()) != PermissionStatus.Granted)
        {
            await Permissions.RequestAsync<Permissions.Bluetooth>();
        }
        IsScanning = true;
        ScannedDevices.Clear();
        await adapter.StartScanningForDevicesAsync();
    }

    [RelayCommand]
    public async Task StopScan()
    {
        await adapter.StopScanningForDevicesAsync();
    }

    [RelayCommand]
    public async Task ConnectDevice(IDevice peripheral)
    {
        bool result = await Shell.Current.DisplayAlert(
            "Connect Device",
            $"Do you want to connect to {peripheral.Name}?", "Yes", "No");
        if (result)
        {
            try
            {
                await bluetoothService.Connect(peripheral.Name);
            }
            catch(DeviceConnectionException)
            {
                await Shell.Current.DisplayAlert(
                      "Connection Error",
                      $"Could not connect to {peripheral.Name}. Please try again.", "Ok");
            }
        }

    }

    
}

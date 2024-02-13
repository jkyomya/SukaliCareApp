using Mopups.Services;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using Permissions = Microsoft.Maui.ApplicationModel.Permissions;

namespace SukaliCareApp.ViewModels;

public partial class AddDeviceViewModel : BaseViewModel, IDisposable
{
    private readonly IBluetoothService bluetoothService;
    private readonly IAdapter adapter;

    [ObservableProperty]
    ObservableCollection<IDevice> scannedDevices = [];

    [ObservableProperty]
    private bool isScanning = false;

    private CancellationTokenSource token;

    public AddDeviceViewModel(IBluetoothService bluetoothService, IAdapter adapter)
    {
        this.bluetoothService = bluetoothService;
        this.adapter = adapter;
        adapter.ScanTimeout = 50000;
        adapter.ScanMode = ScanMode.Balanced;
    }


    [RelayCommand]
    public async Task CloseDialog()
    {
        await adapter.StopScanningForDevicesAsync();
        await MopupService.Instance.PopAllAsync();
    }

    [RelayCommand]
    public async Task ScanDevices()
    {
        
        adapter.DeviceAdvertised += OnDiscoveredDevice;
        adapter.DeviceDiscovered += OnDiscoveredDevice;

        adapter.ScanTimeoutElapsed += async(s, e) =>
        {
            await Shell.Current.DisplayAlert("Stop Scan", "Scan has been stopped", "Ok");
            Dispose();
            IsScanning = false;
        };

        token = new CancellationTokenSource();


        await Shell.Current.DisplayAlert("Scan Devices", "Scanning for devices", "Ok");
        if ((await Permissions.CheckStatusAsync<Permissions.Bluetooth>()) != PermissionStatus.Granted)
        {
            await Permissions.RequestAsync<Permissions.Bluetooth>();
        }
        IsScanning = true;
        ScannedDevices.Clear();
        await adapter.StartScanningForDevicesAsync(cancellationToken: token.Token);
    }

    private void OnDiscoveredDevice(object sender, DeviceEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Shell.Current.DisplayAlert("Device Discovered", $"Discovered {e.Device.Name}", "Ok");
            ScannedDevices.Add(e.Device);
        });
    }

    [RelayCommand]
    public async Task StopScan()
    {
        await Shell.Current.DisplayAlert("Stop Scan", "Scan has been stopped", "Ok");
        await adapter.StopScanningForDevicesAsync();
        IsScanning = false;
        Dispose();
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

    public void Dispose()
    {
        if (token is not null)
        {
            token.Cancel();
            token.Dispose();
        }

        adapter.DeviceAdvertised -= OnDiscoveredDevice;
        adapter.DeviceDiscovered -= OnDiscoveredDevice;
    }
}

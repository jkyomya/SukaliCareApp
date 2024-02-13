using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Interfaces;
using Mopups.Services;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Shiny.BluetoothLE;
using System.Diagnostics;

namespace SukaliCareApp.ViewModels;

public partial class DeviceMgtViewModel : BaseViewModel
{
    private readonly CgmDataService dataService;
    private readonly IPopupNavigation popupNavigation;
    private readonly IBleManager bleManager;
    private readonly IAdapter adapter;
    private readonly IBluetoothService bluetoothService;
    private readonly AddDeviceViewModel addDeviceViewModel;
    private CancellationTokenSource token;

    [ObservableProperty]
    bool isRefreshing = true;
    public ObservableCollection<IDevice> ScannedDevices { get; } = [];

    public ObservableCollection<CGMDevice> PairedDevices { get; set; } = [];

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


    [RelayCommand]
    public async Task LoadData()
    {
        try
        {
#if ANDROID
            var enable = new Android.Content.Intent(
            Android.Bluetooth.BluetoothAdapter.ActionRequestEnable);

            enable.SetFlags(Android.Content.ActivityFlags.NewTask);

            var disable = new Android.Content.Intent(
            Android.Bluetooth.BluetoothAdapter.ActionRequestDiscoverable);

            disable.SetFlags(Android.Content.ActivityFlags.NewTask);

            var bluetoothManager = (Android.Bluetooth.BluetoothManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.BluetoothService);
            var bluetoothAdapter = bluetoothManager.Adapter;

            if (!bluetoothAdapter.IsEnabled)
            {
                await Shell.Current.DisplayAlert("Bluetooth", "Please enable bluetooth", "Ok");
                Android.App.Application.Context.StartActivity(enable);
            }
#endif

            adapter.DeviceConnected += (s, a) =>
            {
                Debug.WriteLine($"Found device: {a.Device.Name}");
                PairedDevices.Add(new CGMDevice
                {
                    Uuid = a.Device.Id.ToString() ?? Guid.NewGuid().ToString(),
                    DeviceName = a.Device.Name
                });
            };

            if (Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>().Result != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
            IsRefreshing = true;
            PairedDevices.Clear();

            var devices = bluetoothService.GetConnectedDevices();
            foreach (var device in devices)
            {
                Debug.WriteLine($"Device: {device.Name}");
                PairedDevices.Add(new CGMDevice
                {
                    Uuid = device.Id.ToString() ?? Guid.NewGuid().ToString(),
                    DeviceName = device.Name
                });
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "Ok");
        }
        finally
        {
            IsRefreshing = false;
        }


    }

    [RelayCommand]
    public async Task ScanDevicesAsync()
    {
        //await popupNavigation.PushAsync(new AddDeviceView(addDeviceViewModel));

        //adapter.DeviceAdvertised += OnDiscoveredDevice;
        adapter.DeviceDiscovered += OnDiscoveredDevice;

        adapter.ScanTimeoutElapsed += async (s, e) =>
        {
            await Shell.Current.DisplayAlert("Stop Scan", "Scan has been stopped", "Ok");
            Dispose();
        };

        token = new CancellationTokenSource();


        await Shell.Current.DisplayAlert("Scan Devices", "Scanning for devices", "Ok");
        if ((await Permissions.CheckStatusAsync<Permissions.Bluetooth>()) != PermissionStatus.Granted)
        {
            await Permissions.RequestAsync<Permissions.Bluetooth>();
        }

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

    public async Task StopScan()
    {
        await Shell.Current.DisplayAlert("Stop Scan", "Scan has been stopped", "Ok");
        await adapter.StopScanningForDevicesAsync();
        Dispose();
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
    private void OnDeviceConnected(CGMDevice device) => PairedDevices.Add(device);

}

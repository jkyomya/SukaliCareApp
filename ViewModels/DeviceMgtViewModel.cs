using Mopups.Interfaces;
using Plugin.BLE.Abstractions.EventArgs;
using System.Text.Json;

namespace SukaliCareApp.ViewModels;

public partial class DeviceMgtViewModel : BaseViewModel
{
    private readonly IPopupNavigation popupNavigation;
    private readonly AddDeviceViewModel addDeviceViewModel;
    private readonly HttpProxyService _httpProxyService;
    private readonly BluetoothConnectionService bluetoothConnectionService;

    [ObservableProperty]
    bool isRefreshing = true;

    [ObservableProperty]
    ObservableCollection<CGMDevice> pairedDevices = [];

    public DeviceMgtViewModel(
            IPopupNavigation popupNavigation,
            AddDeviceViewModel addDeviceViewModel,
            HttpProxyService httpProxyService,
            BluetoothConnectionService bluetoothConnectionService)
    {
        this.popupNavigation = popupNavigation;
        this.addDeviceViewModel = addDeviceViewModel;
        _httpProxyService = httpProxyService;
        this.bluetoothConnectionService = bluetoothConnectionService;

        WeakReferenceMessenger.Default.Register<CGMDevice>(this, (r, m) =>
        {
            OnDeviceConnected(m);
        });
        
    }


    [RelayCommand]
    public async Task LoadData()
    {
        IsRefreshing = true;

        PairedDevices = new ObservableCollection<CGMDevice>(
            await bluetoothConnectionService.GetBondedDevices());

        IsRefreshing = false;
    }

    [RelayCommand]
    public async Task ScanDevicesAsync()
    {
        await popupNavigation.PushAsync(new AddDeviceView(addDeviceViewModel));

    }

    [RelayCommand]
    public async Task ConnectDevice(CGMDevice peripheral)
    {
        bool result = await Shell.Current.DisplayAlert(
            "Connect Device",
            $"Do you want to connect to {peripheral.DeviceName}?", "Yes", "No");
        if (result)
        {
            if (await bluetoothConnectionService.ConnectDevice(peripheral))
            {
                await _httpProxyService.SaveDevice(peripheral);
                await Shell.Current.DisplayAlert("Success", $"Connected to {peripheral.DeviceName}", "Ok");
                Preferences.Set(Stores.connectedDevice, JsonSerializer.Serialize(peripheral));
            }
        }

    }
    
    private void OnDeviceConnected(CGMDevice device) => PairedDevices.Add(device);

}

using DynamicData;
using Microsoft.Maui.Platform;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SukaliCareApp.Services;

public class BluetoothConnectionService
{
    private readonly IBluetoothService bluetoothService;
    private readonly AppNotificationService notificationService;
    private readonly IAdapter adapter;
    private readonly CgmReadingDatabase _database;

    private CancellationTokenSource token;

    public BluetoothConnectionService(
        IBluetoothService bluetoothService,
        AppNotificationService notificationService,
        IAdapter adapter,
        CgmReadingDatabase database)
    {
        this.bluetoothService = bluetoothService;
        this.notificationService = notificationService;
        this.adapter = adapter;
        _database = database;
    }

    async Task InitAsync()
    {
        try
        {
            // Awaited rather than .Result, which can freeze the UI thread.
            if (await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>() != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
            // Android 12+ needs the "Nearby devices" permission to scan and connect.
            if (await Permissions.CheckStatusAsync<Permissions.Bluetooth>() != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.Bluetooth>();
            }

#if ANDROID
            var enable = new Android.Content.Intent(
            Android.Bluetooth.BluetoothAdapter.ActionRequestEnable);

            enable.SetFlags(Android.Content.ActivityFlags.NewTask);
            enable.AddFlags(Android.Content.ActivityFlags.NewTask);

            var disable = new Android.Content.Intent(
            Android.Bluetooth.BluetoothAdapter.ActionRequestDiscoverable);

            disable.SetFlags(Android.Content.ActivityFlags.NewTask);

            var bluetoothManager = (Android.Bluetooth.BluetoothManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.BluetoothService);
            var bluetoothAdapter = bluetoothManager.Adapter;

            if (!bluetoothAdapter.IsEnabled)
            {
                await Shell.Current.DisplayAlert("Bluetooth", "Please enable blueTooth and reconnect", "Ok");
                var currentActivity = await Platform.WaitForActivityAsync();
                currentActivity.StartActivity(enable);
                
                //Android.App.Application.Context.StartActivity(enable);
            }
#endif
        }
        catch(Exception ex)
        {
            Debug.WriteLine(ex);
            
        }
    }

    public async Task<IEnumerable<CGMDevice>> GetBondedDevices()
    {
        await InitAsync();
        return bluetoothService.GetConnectedDevices()
            .Select(item => new CGMDevice
            {
                DeviceName = item.Name,
                Uuid = item.Id.ToString(),
                Id = item.Id,
                IsConnected = false

            });
    }
    public async Task<CGMDevice> ConnectDevice()
    {
        await InitAsync();
        try
        {
            string userDeviceJson = Preferences.Get(Stores.cgmDeviceStore, null);
            if (string.IsNullOrEmpty(userDeviceJson))
            {
                return null;
            }
            CGMDevice userDevice = JsonSerializer.Deserialize<CGMDevice>(userDeviceJson);
            if (userDevice is null)
            {
                return null;
            }

            IDevice device = bluetoothService.GetConnectedDevices()
                .FirstOrDefault(x => x.Name == userDevice.DeviceName && x.Id == userDevice.Id);
            if (device == null)
            {
                return null;
            }
            if (device.State != Plugin.BLE.Abstractions.DeviceState.Connected)
            {
                await bluetoothService.Disconnect();
                await bluetoothService.Connect(device.Name);
            }

            userDevice.IsConnected = true;
            return userDevice;

        }
        catch (DeviceConnectionException ex)
        {
            Debug.Write(ex);
            await Shell.Current.DisplayAlert(
                  "Connection Error",
                  $"Could not connect to device. Please try again.", "Ok");

            return null;
        }
        catch (Exception ex)
        {
            Debug.Write(ex);
            await Shell.Current.DisplayAlert(
                  "Connection Error",
                  $"Something went wrong! Could not connect to device. Please try again.", "Ok");

            return null;
        }
    }
    public async Task<bool> ConnectDevice(CGMDevice peripheral)
    {
        await InitAsync();
        try
        {
            IDevice device =  bluetoothService.GetConnectedDevices()
                .FirstOrDefault(x => x.Name == peripheral.DeviceName && x.Id ==  peripheral.Id);
            if(device == null)
            {
                return false;
            }
            if(device.State != Plugin.BLE.Abstractions.DeviceState.Connected)
            {
                await bluetoothService.Disconnect();
                await bluetoothService.Connect(device.Name);
            }
            Preferences.Set(Stores.cgmDeviceStore, JsonSerializer.Serialize(peripheral));

            return true;
            
        }
        catch (DeviceConnectionException ex)
        {
            Debug.Write(ex);
            await Shell.Current.DisplayAlert(
                  "Connection Error",
                  $"Could not connect to {peripheral.DeviceName}. Please try again.", "Ok");

            return false;
        }
        catch (Exception ex)
        {
            Debug.Write(ex);
            await Shell.Current.DisplayAlert(
                  "Connection Error",
                  $"Something went wrong! Could not connect to {peripheral.DeviceName}. Please try again.", "Ok");

            return false;
        }
    }

    public async Task<IEnumerable<CGMDevice>> ScanDevicesAsync()
    {
        //await popupNavigation.PushAsync(new AddDeviceView(addDeviceViewModel));

        await InitAsync();

        adapter.DeviceDiscovered += (sender, e) =>
        {

        };
        adapter.ScanTimeoutElapsed += async (s, e) =>
        {
            await Shell.Current.DisplayAlert("Stop Scan", "Scan has been stopped", "Ok");
            Dispose();
        };

        token = new CancellationTokenSource();

        await Shell.Current.DisplayAlert("Scan Devices", "Scanning for devices", "Ok");
      
        await adapter.StartScanningForDevicesAsync(cancellationToken: token.Token);

        return [];
    }

    public async Task<CgmReading> ReadDataAsync()
    {
        await InitAsync();
        try
        {
            string deviceJson = Preferences.Get(Stores.connectedDevice, null);
            if (string.IsNullOrEmpty(deviceJson))
            {
                return null;
            }
            CGMDevice device = JsonSerializer.Deserialize<CGMDevice>(deviceJson);
            if (device is null)
            {
                return null;
            }
            await bluetoothService.Send(device.DeviceName, Encoding.UTF8.GetBytes("ping"));
            var response = await bluetoothService.Receive(device.DeviceName);
            string readingRaw = Encoding.UTF8.GetString(response);

            if (GlucoseMessageParser.TryParse(readingRaw, out float readingParsed))
            {
                Debug.WriteLine($"RECEIVED READING: {readingParsed} mmol/l");

                CgmReading reading = new()
                {
                    GlucoseValue = new GlucoseValue(readingParsed, "mmol/l"),
                    Id = Guid.NewGuid(),
                    ReadingDate = DateTime.Now
                };

                await _database.SaveReadingAsync(reading);

                WeakReferenceMessenger.Default.Send(new DataReceivedMessage(reading));

                return reading;
            }

            return null;

        }
        catch(Exception ex)
        {
            Debug.Write(ex);
            return null;
        }
    }
    void Dispose()
    {
        if (token is not null)
        {
            token.Cancel();
            token.Dispose();
        }

        
    }
}

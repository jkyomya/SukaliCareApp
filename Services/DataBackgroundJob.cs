using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using Shiny.Jobs;
using SukaliCareApp.Messages;
using System.Diagnostics;

namespace SukaliCareApp.Services;

public class DataBackgroundJob : IJob
{
    private readonly IBluetoothService _bluetoothService;
    private readonly AppNotificationService _notificationService;

    public DataBackgroundJob(
        IBluetoothService bluetoothService,
        AppNotificationService notificationService)
    {
        _bluetoothService = bluetoothService;
        _notificationService = notificationService;
    }

    public async Task Run(JobInfo jobInfo, CancellationToken cancelToken)
    {
        try
        {
            await Shell.Current.DisplayAlert("Data Background Job", "Fetching data from device", "Ok");
            var device = JsonConvert.DeserializeObject<CGMDevice>(
                Preferences.Get(Stores.connectedDevice, null)) ?? throw new ArgumentNullException("No device connected");

            await _bluetoothService.Connect(device.DeviceName);
            while (true)
            {
                var response = await _bluetoothService.Receive(device.DeviceName);
                WeakReferenceMessenger.Default.Send(
                    new DataReceivedMessage(Encoding.UTF8.GetString(response)));

                var prefs = JsonConvert.DeserializeObject<UserPreferences>(Preferences.Get(Stores.preferenceStore, null));
                await Task.Delay(TimeSpan.FromMinutes(prefs.SyncFrequency), cancelToken);
            }
            
        }
        catch(DeviceConnectionException ex)
        {
            Debug.Write(ex.Message);
#if __MOBILE__
            _notificationService.PublishNotification("Connection failed", "CGM device",
                               $"Could not find {ex.DeviceName} device. Tap here to connect",
                                DateTime.Now, NotificationRepeat.No, TimeSpan.FromHours(1));
#endif
        }
        catch (ArgumentNullException)
        {
            Debug.WriteLine("No device connected");
#if __MOBILE__
            _notificationService.PublishNotification("No connected device", "CGM device",
                               "There is not connected device to your app. Tap here to connect",
                                DateTime.Now, NotificationRepeat.No, TimeSpan.FromHours(1));
#endif
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            await Shell.Current.DisplayAlert("No data",
                "Could not fetch data from device. Please confirm that the device is connected",
                "Ok");

        }

       
    }

}

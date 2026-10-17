using System.Text.Json;

namespace SukaliCareApp.Services;

public class DataBackgroundJob : IJob
{
    private readonly AppNotificationService _notificationService;
    private readonly CgmReadingDatabase _database;
    private readonly BluetoothConnectionService bluetoothConnectionService;
    // Use the interface: only IBluetoothService is registered in MauiProgram,
    // so asking for the concrete BluetoothService made the job fail to start.
    private readonly IBluetoothService _bluetoothService;

    public DataBackgroundJob(
        AppNotificationService notificationService,
        CgmReadingDatabase database, BluetoothConnectionService bluetoothConnectionService,
        IBluetoothService bluetoothService)
    {
        _notificationService = notificationService;
        _database = database;
        this.bluetoothConnectionService = bluetoothConnectionService;
        _bluetoothService = bluetoothService;
    }

    public async Task Run(JobInfo jobInfo, CancellationToken cancelToken)
    {
        try
        {
            Debug.WriteLine("BACKGROUND JOB IS RUNNING");

            string deviceJson = Preferences.Get(Stores.connectedDevice, null);
            if (string.IsNullOrEmpty(deviceJson))
            {
                throw new ArgumentNullException(nameof(deviceJson), "No device connected");
            }
            CGMDevice device = JsonSerializer.Deserialize<CGMDevice>(deviceJson)
                ?? throw new ArgumentNullException(nameof(deviceJson), "No device connected");

            if (!await bluetoothConnectionService.ConnectDevice(device))
            {
                throw new ApplicationException("Device not paired");
            }

            while (!cancelToken.IsCancellationRequested)
            {
                await _bluetoothService.Send(device.DeviceName, Encoding.UTF8.GetBytes("ping"));
                var response = await _bluetoothService.Receive(device.DeviceName);
                string readingRaw = Encoding.UTF8.GetString(response);

                // No pop-up alerts here: this job runs in the background, where there is no
                // screen to show them on, and Shell.Current can be null.
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
                }
                else if (!string.IsNullOrWhiteSpace(readingRaw.Replace("\0", string.Empty)))
                {
                    Debug.WriteLine($"UNRECOGNISED MESSAGE: {readingRaw.Replace("\0", string.Empty)}");
                }

                // Fall back to a 10-minute sync if preferences have not been saved yet.
                string prefsJson = Preferences.Get(Stores.preferenceStore, null);
                UserPreferences prefs = string.IsNullOrEmpty(prefsJson)
                    ? null
                    : JsonSerializer.Deserialize<UserPreferences>(prefsJson);
                int syncMinutes = prefs?.SyncFrequency > 0 ? prefs.SyncFrequency : 10;

                await Task.Delay(TimeSpan.FromMinutes(syncMinutes), cancelToken);
            }
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("Background job cancelled");
        }
        catch (DeviceConnectionException ex)
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
                               "There is no device connected to your app. Tap here to connect",
                                DateTime.Now, NotificationRepeat.No, TimeSpan.FromHours(1));
#endif
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
#if __MOBILE__
            _notificationService.PublishNotification("No data", "CGM device",
                               "Could not fetch data from the device. Please confirm that it is connected",
                                DateTime.Now, NotificationRepeat.No, TimeSpan.FromHours(1));
#endif
        }
    }
}

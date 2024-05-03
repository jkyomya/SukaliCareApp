using System.Text.Json;
using System.Text.RegularExpressions;

namespace SukaliCareApp.Services;

public class DataBackgroundJob : IJob
{
    private readonly AppNotificationService _notificationService;
    private readonly CgmReadingDatabase _database;
    private readonly BluetoothConnectionService bluetoothConnectionService;
    private readonly BluetoothService _bluetoothService;

    public DataBackgroundJob(
        AppNotificationService notificationService,
        CgmReadingDatabase database, BluetoothConnectionService bluetoothConnectionService,
        BluetoothService bluetoothService)
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
            Debug.WriteLine("BACKGROUND JOB IS RUNNNING");
            CGMDevice device = JsonSerializer.Deserialize<CGMDevice>(
                Preferences.Get(Stores.connectedDevice, null)) ?? throw new ArgumentNullException("No device connected");

            if(!await bluetoothConnectionService.ConnectDevice(device))
            {
                throw new ApplicationException("Device not paired");
            }
            
            while (true)
            {
                await _bluetoothService.Send(device.DeviceName, Encoding.UTF8.GetBytes("ping"));
                var response = await _bluetoothService.Receive(device.DeviceName);
                string readingRaw = Encoding.UTF8.GetString(response);

                if (!string.IsNullOrEmpty(readingRaw) &&
                    readingRaw.Contains("Glucose", StringComparison.CurrentCultureIgnoreCase))
                {
                    Debug.Write($"RECEIVED MSG: {readingRaw}");
                    await Shell.Current.DisplayAlert("Received reading", readingRaw, "Ok");

                    string pattern = @"^Glucose_reading_\d+\.\d+_mmol/L_end$";
                    Match match = Regex.Match(readingRaw, pattern);
                    if (match.Success)
                    {

                        if (float.TryParse(match.Groups[1].Value, out float readingParsed))
                        {
                            CgmReading reading = new()
                            {
                                GlucoseValue = new GlucoseValue(readingParsed, "mmol/l"),
                                Id = Guid.NewGuid(),
                                ReadingDate = DateTime.Now
                            };

                            await _database.SaveReadingAsync(reading);

                            WeakReferenceMessenger.Default.Send(new DataReceivedMessage(reading));
                        }
                    }
                }

                var prefs = JsonSerializer.Deserialize<UserPreferences>(Preferences.Get(Stores.preferenceStore, null));
                if (prefs.SyncFrequency > 0)
                {
                    await Task.Delay(TimeSpan.FromMinutes(prefs.SyncFrequency), cancelToken);
                }
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancelToken);
                }
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

namespace SukaliCareApp.Services;

public class DataBackgroundJob : IJob
{
    private readonly IBluetoothService _bluetoothService;
    private readonly AppNotificationService _notificationService;
    private readonly CgmReadingDatabase _database;
    private readonly IAdapter _adapter;

    public DataBackgroundJob(
        IBluetoothService bluetoothService,
        AppNotificationService notificationService,
        IAdapter adapter,
        CgmReadingDatabase database)
    {
        _bluetoothService = bluetoothService;
        _adapter = adapter;
        _notificationService = notificationService;
        _database = database;
    }

    public async Task Run(JobInfo jobInfo, CancellationToken cancelToken)
    {
        try
        {
            CGMDevice device = JsonConvert.DeserializeObject<CGMDevice>(
                Preferences.Get(Stores.connectedDevice, null)) ?? throw new ArgumentNullException("No device connected");
            
            await _bluetoothService.Connect(device.DeviceName);
            while (true)
            {
                await _bluetoothService.Send(device.DeviceName, Encoding.UTF8.GetBytes("ping"));
                var response = await _bluetoothService.Receive(device.DeviceName);
                string readingRaw = Encoding.UTF8.GetString(response);

                Debug.Write($"RECEIVED MSG: {readingRaw}");

                if(!string.IsNullOrEmpty(readingRaw) &&
                    readingRaw.Contains("Glucose", StringComparison.CurrentCultureIgnoreCase))
                {
                    readingRaw = readingRaw.Replace("mmol/l", string.Empty);
                    readingRaw = readingRaw.Replace("Glucose ", string.Empty);
                }

                if(float.TryParse(readingRaw, out float readingParsed))
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
                
                var prefs = JsonConvert.DeserializeObject<UserPreferences>(Preferences.Get(Stores.preferenceStore, null));
                if (prefs.SyncFrequency > 0)
                {
                    await Task.Delay(TimeSpan.FromMinutes(prefs.SyncFrequency), cancelToken);
                }
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), cancelToken);
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

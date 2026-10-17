using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SukaliCareApp.Converters;
using System.Reactive.Linq;
using LiveChartsCore.Defaults;
using CommunityToolkit.Maui.Alerts;
using System.Text.Json;

namespace SukaliCareApp.ViewModels;

public partial class DashboardViewModel : BaseViewModel, IRecipient<DeviceConnectedMessage>
{
    [ObservableProperty]
    CgmReading lastReading;

    [ObservableProperty]
    CgmReading highestReading;

    [ObservableProperty]
    CGMDevice device;

    [ObservableProperty]
    string connectionStatus = "Disconnected";

    [ObservableProperty]
    ObservableCollection<CgmReading> chartReadings;

    [ObservableProperty]
    bool isCompleted = false;

    [ObservableProperty]
    public ISeries[] chartSeries = [];
    [ObservableProperty]
    Axis[] xAxes =
    {
        new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("ddd"))
        {
            ShowSeparatorLines = true,
            SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
            {
                StrokeThickness = 1,
                PathEffect = new DashEffect([3f, 3f], 1)
            },
            TextSize = 10,
            LabelsPaint = new SolidColorPaint(
                Application.Current.RequestedTheme == AppTheme.Light ?
                    SKColors.LightSlateGray :
                    SKColors.White)
        },
    };
    [ObservableProperty]
    Axis[] yAxes =
    {
        new Axis
        {
            ShowSeparatorLines = true,
            SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
            {
                StrokeThickness = 1,
                PathEffect = new DashEffect([2f, 2f])
            },
            TextSize = 10,
            Name = "Glucose levels",
            NamePaint = new SolidColorPaint(SKColors.LightGray),
            NameTextSize = 12,
            MinStep = 2,
            SubseparatorsCount = 4,
            SubseparatorsPaint = new SolidColorPaint(new SKColor(235,235,235,76)),
            LabelsPaint = new SolidColorPaint(
                Application.Current.RequestedTheme == AppTheme.Light ?
                    SKColors.LightSlateGray :
                    SKColors.White),
            MinLimit  = 0
        }
    };
    private readonly IJobManager _jobManager;
    private readonly CgmReadingDatabase database;
    private readonly BluetoothConnectionService _blueToothConnectionService;
    private readonly IAdapter _adapter;

    public DashboardViewModel(
        IJobManager jobManager,
        CgmReadingDatabase database,
        BluetoothConnectionService blueToothConnectionService,
        IAdapter adapter)
    {
        _jobManager = jobManager;
        this.database = database;
        _blueToothConnectionService = blueToothConnectionService;
        _adapter = adapter;

        if (!Preferences.ContainsKey(Stores.preferenceStore))
        {
            UserPreferences prefs = new()
            {
                SyncFrequency = 10,
                AllowAutoConnect = true,
                AllowNotifications = true,
                Units = "mmol/l"
            };
            Preferences.Set(Stores.preferenceStore, JsonSerializer.Serialize(prefs));
        }

        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    ObservableCollection<DateTimePoint> ChartValues { get; } = new ObservableCollection<DateTimePoint>();

    [RelayCommand]
    public async Task IsRefreshing()
    {
        try
        {
            UnitsConveter converter = new();

            var data = await database.GetDailyReadings(7);
            ColumnSeries<DateTimePoint> columnSeries = new()
            {
                Values = ChartValues,
                Fill = new SolidColorPaint(SKColors.DodgerBlue),
                MaxBarWidth = double.MaxValue,
                Stroke = null,
            };


            ChartSeries = [columnSeries];
            ChartValues.Clear();
            foreach (var item in data)
            {
                ChartValues.Add(new DateTimePoint(item.ReadingDate, (double)converter.ConvertValue(item.GlucoseValue)));
            }

            if (data.Any())
            {
                var ordered = data.ToList().OrderByDescending(p => p.GlucoseValue.Value);
                HighestReading = ordered.First();
                LastReading = ordered.Last();
            }

            IsBusy = false;
            IsCompleted = true;

#if __MOBILE__
            var access = await _jobManager.RequestAccess();
            if (access != Shiny.AccessState.Available)
            {
                await Shell.Current.DisplayAlert("Error", "Job permissions failed: " + access, "OK");
                return;
            }
            else
            {

                if (!_jobManager.IsRunning)
                {
                    Debug.WriteLine("STARTING BACKGROUNF JOB");
                    Observable.FromAsync(async () => await _blueToothConnectionService.ConnectDevice())
                        .Subscribe(result =>
                        {
                            if(result is not null)
                            {
                                ConnectionStatus = "Connected";
                                Device = result;
                            }
                        });
                    await _jobManager.RunAll();
                }
            }
#endif
        
        }
        catch(Exception ex)
        {
            Debug.Write(ex);
            await Shell.Current.DisplayAlert("Dash error", ex.Message, "Ok");
        }
        finally
        {
            IsBusy = false;
        }
    }
    public void OnAppearing()
    {
        IsBusy = true;

    }

    public void Receive(DeviceConnectedMessage message)
    {
        Toast.Make("Device connected");
        ConnectionStatus = "Connected";
        Device = new CGMDevice
        {
            DeviceName = message.Value.Name,
            IsConnected = true,
            Id = message.Value.Id,
            LastConnected = DateTime.Now,
            Uuid = message.Value.Id.ToString()
        };
    }
}

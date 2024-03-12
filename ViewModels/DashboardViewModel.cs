using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SukaliCareApp.Converters;
using System.Reactive.Linq;
using LiveChartsCore.Defaults;
using Shiny.Jobs;
using Shiny;
using CommunityToolkit.Mvvm.Messaging;
using SukaliCareApp.Messaging;
using CommunityToolkit.Maui.Alerts;
using System.Diagnostics;

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
                    SKColors.White)
        }
    };
    private readonly IJobManager _jobManager;
    private readonly CgmReadingDatabase database;

    public DashboardViewModel(
        IJobManager jobManager,
        CgmReadingDatabase database)
    {
        _jobManager = jobManager;
        this.database = database;

        if (!Preferences.ContainsKey(Stores.preferenceStore))
        {
            UserPreferences prefs = new()
            {
                SyncFrequency = 10,
                AllowAutoConnect = true,
                AllowNotifications = true,
                Units = "mmol/l"
            };
            Preferences.Set(Stores.preferenceStore, JsonConvert.SerializeObject(prefs));
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
            var access = await _jobManager.RequestAccess();
            if (access != AccessState.Available)
            {
                await Shell.Current.DisplayAlert("Error", "Job permissions failed: " + access, "OK");
                return;
            }
            else
            {
#if __MOBILE__

                if (_jobManager.IsRunning)
                {
                    await Shell.Current.DisplayAlert("ERROR", "Job Manager is already running", "OK");
                }
                else
                {

                    await _jobManager.RunAll();
                }

#endif
            }
        }
        catch(Exception ex)
        {
            Debug.Write(ex);
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

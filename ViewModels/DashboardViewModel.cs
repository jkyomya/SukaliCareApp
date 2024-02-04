
using Firebase.Auth;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SukaliCareApp.Converters;
using System.Reactive.Linq;
using LiveChartsCore.Defaults;

namespace SukaliCareApp.ViewModels;

public partial class DashboardViewModel(
    CgmDataService dataService
#if ANDROID || IOS
    ,
    AppNotificationService notificationService
#endif
    ) 
    : BaseViewModel
{
    [ObservableProperty]
    AppUser userDetails;

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
        }
    };
    ObservableCollection<DateTimePoint> ChartValues { get; } = new ObservableCollection<DateTimePoint>();
    
    [RelayCommand]
    public async Task IsRefreshing()
    {
        UnitsConveter converter = new();
        string savedUser = Preferences.Get(Stores.userDetailsStore, null);
        if (!string.IsNullOrEmpty(savedUser))
        {
            UserDetails = JsonConvert.DeserializeObject<AppUser>(savedUser);
        }
        var data = await dataService.GetItemsByDay();
        data.OrderBy(p => p.ReadingDate);
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
        var ordered = data.ToList().OrderByDescending(p => p.GlucoseValue.Value);
        HighestReading = ordered.First();
        LastReading = ordered.Last();

#if ANDROID || IOS
        notificationService.PublishNotification("No connected device", "CGM device", "There is not connected device to your app. Tap here to connect",
            DateTime.Now, NotificationRepeat.No, TimeSpan.FromHours(1));
#endif

        IsBusy = false;
        IsCompleted = true;
    }
    public void OnAppearing()
    {
        IsBusy = true;       
    }
}

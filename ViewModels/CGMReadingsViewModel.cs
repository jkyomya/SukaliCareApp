using CommunityToolkit.Mvvm.Messaging;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using SukaliCareApp.Converters;
using System.Reactive.Linq;
using System.Text.Json;

namespace SukaliCareApp.ViewModels;

public partial class CGMReadingsViewModel : BaseViewModel, IRecipient<DataReceivedMessage>
{
    [ObservableProperty]
    string currentMonth = DateTime.Today.ToString("MMMM");

    [ObservableProperty]
    ISeries[] series = [];
    [ObservableProperty]
    Axis[] xAxes =
    {
        new DateTimeAxis(TimeSpan.FromMinutes(1), date => date.ToString("dd ddd h:mm tt"))
        {
            ShowSeparatorLines = true,
            SeparatorsPaint = new SolidColorPaint(
                Application.Current.RequestedTheme == AppTheme.Light ?
                    SKColors.LightSlateGray :
                    SKColors.White)
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
            SeparatorsPaint = new SolidColorPaint(
                Application.Current.RequestedTheme == AppTheme.Light ?
                    SKColors.LightSlateGray : 
                    SKColors.White)
            {
                StrokeThickness = 1,
                PathEffect = new DashEffect([2f, 2f])
            },
            TextSize = 12,
            MinStep = 2,
            SubseparatorsCount = 4,
            SubseparatorsPaint = new SolidColorPaint(new SKColor(235,235,235,76)),
            LabelsPaint = new SolidColorPaint(
                Application.Current.RequestedTheme == AppTheme.Light ?
                    SKColors.LightSlateGray :
                    SKColors.White),
            Position = AxisPosition.End,
            Labeler = value => value.ToString("F1"),
            MinLimit = 0
        }
    };

    public Margin DrawMargin { get; } = new Margin(8, 10, 40, 40);
    private static bool IsStandard() => JsonSerializer.Deserialize<UserPreferences>(Preferences.Get(Stores.preferenceStore, null)).Units == "mmol/l";
    [ObservableProperty]
    RectangularSection[] sections;

    [ObservableProperty]
    string currentDay = DateTime.Today.ToString("dddd");

    [ObservableProperty]
    ObservableCollection<CgmReading> items = [];
    ObservableCollection<DateTimePoint> ChartValues { get; } = new ObservableCollection<DateTimePoint>();

    [ObservableProperty]
    CgmReading latestReading;

    private readonly AppNotificationService notificationService;
    private readonly CgmReadingDatabase database;
    private UnitsConveter converter = new();

    public CGMReadingsViewModel(
        AppNotificationService notificationService, 
        CgmReadingDatabase database)
    {
        this.notificationService = notificationService;
        this.database = database;
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    [RelayCommand]
    public void OnRefreshing()
    {
        LineSeries<DateTimePoint> lineSeries = new()
        {
            Values = ChartValues,
            Stroke = new SolidColorPaint(SKColors.DodgerBlue, 2),
            GeometryFill = null,
            GeometryStroke = null,
            Fill = new SolidColorPaint(new SKColor(220,236,225,76)),
            DataPadding = new LiveChartsCore.Drawing.LvcPoint(0, 0)            
        };

        Series = [lineSeries];
        Sections =
        [
            new RectangularSection()
            {
                Fill = new SolidColorPaint(
                    Application.Current.RequestedTheme == AppTheme.Light ?
                     new SKColor(255, 225, 230) :
                     SKColors.HotPink),
                Yi = IsStandard() ? 14 : 252,
                Yj = IsStandard() ? 42 : 756,

            }
        ];

        Observable.FromAsync(() => database.GetReadingsAsync())
            .Retry(3)
            .Subscribe((result) =>
            {

                Items.Clear();
                ChartValues.Clear();
#if ANDROID || IOS
                var highest = result.MaxBy(m => m.GlucoseValue.Value);
                if (converter.ConvertValue(highest.GlucoseValue) < converter.ConvertValue(new GlucoseValue(12, "mmol/l")))
                {
                    notificationService.PublishNotification($"Normal blood sugar level ({highest.GlucoseValue.Value} {highest.GlucoseValue.Unit})", "Current readings", "Your glucose readings are ready and the latest one seems okay. Open app to see more",
                    DateTime.Now, NotificationRepeat.No, TimeSpan.FromHours(1));
                }
                else if (converter.ConvertValue(highest.GlucoseValue) > converter.ConvertValue(new GlucoseValue(12, "mmol/l")))
                {
                    notificationService.PublishNotification($"High blood sugar ({highest.GlucoseValue.Value} {highest.GlucoseValue.Unit})", "Take caution", "Your glucose blood sugar seems to be out of control. Please take adequate measures to correct this",
                    DateTime.Now, NotificationRepeat.No, TimeSpan.FromHours(1));
                }

#endif
                foreach (var item in result)
                {
                    Items.Add(item);
                    ChartValues.Add(new DateTimePoint(item.ReadingDate, (double)converter.ConvertValue(item.GlucoseValue)));
                }
                YAxes[0].MinLimit = null;
                YAxes[0].MaxLimit = null;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    IsBusy = false;
                });
            },
            onError: async (error) =>
            {
                await Shell.Current.DisplayAlert("Internet", "Please make sure you are connected to the internet", "Ok");
            });
    }

    [RelayCommand]
    public async Task LoadMore()
    {
        var items = await database.GetReadingsAsync();

        foreach (var item in items)
        {
            Items.Add(item);
        }
    }

    [RelayCommand]
    private Task GoToDetails(CgmReading item)
    {
        return Task.CompletedTask;
    }

    public void Receive(DataReceivedMessage message)
    {
            Items.Insert(0,message.Value);

            ChartValues.Add(
                new DateTimePoint(
                    message.Value.ReadingDate,
                    (double)converter.ConvertValue(message.Value.GlucoseValue)
                  ));
    }
}

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
    private static bool IsStandard() => UnitsConveter.IsMmolL(UnitsConveter.PreferredUnit());
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
                // Shade the high zone: above 10.0 mmol/L (180 mg/dL), the top of the target range.
                Yi = IsStandard() ? 10.0 : 180,
                Yj = IsStandard() ? 33.3 : 600,

            }
        ];

        Observable.FromAsync(() => database.GetReadingsAsync())
            .Retry(3)
            .Subscribe((result) =>
            {

                Items.Clear();
                ChartValues.Clear();
#if ANDROID || IOS
                // Alert on the most recent reading. Skip if there are no readings yet,
                // which previously crashed the screen on first use.
                var latest = result?.Where(r => r?.GlucoseValue is not null)
                    .MaxBy(r => r.ReadingDate);
                if (latest is not null)
                {
                    string shown = converter.Convert(latest.GlucoseValue, typeof(string), null,
                        System.Globalization.CultureInfo.CurrentCulture) as string;
                    float mmol = UnitsConveter.IsMmolL(latest.GlucoseValue.Unit)
                        ? latest.GlucoseValue.Value
                        : latest.GlucoseValue.Value / 18f;

                    if (mmol < ValueToColorConvert.LowMmol)
                    {
                        notificationService.PublishNotification($"Low blood sugar ({shown})", "Take action now",
                            "Your latest glucose reading is low. Take fast-acting sugar and recheck in 15 minutes. Seek care if you feel unwell.",
                            DateTime.Now, NotificationRepeat.No, TimeSpan.FromHours(1));
                    }
                    else if (mmol > ValueToColorConvert.HighMmol)
                    {
                        notificationService.PublishNotification($"High blood sugar ({shown})", "Take caution",
                            "Your latest glucose reading is high. Follow your care plan and contact your health worker if it stays high.",
                            DateTime.Now, NotificationRepeat.No, TimeSpan.FromHours(1));
                    }
                    else
                    {
                        notificationService.PublishNotification($"Blood sugar in range ({shown})", "Current readings",
                            "Your latest glucose reading is within the target range. Open the app to see more.",
                            DateTime.Now, NotificationRepeat.No, TimeSpan.FromHours(1));
                    }
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

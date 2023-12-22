using System.Globalization;

namespace SukaliCareApp.Converters
{
    public class DateToDurationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dateDiff = (DateTime.Now).Subtract((DateTime)value);
            if(dateDiff < TimeSpan.FromSeconds(30))
            {
                return $"now";
            }
            else if (dateDiff < TimeSpan.FromSeconds(60))
            {
                return $"{Math.Round(dateDiff.TotalSeconds,0)} secs";
            }
            else if (dateDiff < TimeSpan.FromHours(1))
            {
                return $"{Math.Round(dateDiff.TotalMinutes, 0)} mins";
            }
            else if(dateDiff < TimeSpan.FromDays(1))
            {
                return $"{Math.Round(dateDiff.TotalHours, 0)} hrs";
            }
            else if (dateDiff == TimeSpan.FromDays(1))
            {
                return $"{Math.Round(dateDiff.TotalDays, 0)} day";
            }
            else if (dateDiff < TimeSpan.FromDays(31))
            {
                return $"{Math.Round(dateDiff.TotalDays, 0)} days";
            }
            else if (dateDiff < TimeSpan.FromDays(60))
            {
                return "Over a month";
            }
            else
            {
                return "months";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

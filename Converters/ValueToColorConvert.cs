using System.Globalization;

namespace SukaliCareApp.Converters
{
    public class ValueToColorConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var reading = (GlucoseValue)value;
            if(reading.Unit == "mmol/l")
            {
                if(reading.Value < 5)
                {
                    return Color.Parse("#9bab57");
                }
                else if(reading.Value < 7)
                {
                    return Color.Parse("#69a769");
                }
                else if(reading.Value  <= 12)
                {
                    return Color.Parse("Green");
                }
                else if (reading.Value <= 15)
                {
                    return Color.Parse("#a56767");
                }
                else
                {
                    return Color.Parse("#bb3b3b");
                }
                
            }
            else
            {
                if (reading.Value < 90)
                {
                    return Color.Parse("#9bab57");
                }
                else if (reading.Value < 126)
                {
                    return Color.Parse("#69a769");
                }
                else if (reading.Value <= 216)
                {
                    return Color.Parse("Green");
                }
                else if (reading.Value <= 279)
                {
                    return Color.Parse("#a56767");
                }
                else
                {
                    return Color.Parse("#bb3b3b");
                }
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

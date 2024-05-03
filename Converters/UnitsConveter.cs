using System.Globalization;
using System.Text.Json;

namespace SukaliCareApp.Converters
{
    public class UnitsConveter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var reading = (GlucoseValue)value;
            var prefs = Preferences.Get(Stores.preferenceStore, null);
            if (string.IsNullOrEmpty(prefs))
            {
                return null;
            }
            UserPreferences userPreferences = JsonSerializer.Deserialize<UserPreferences>(prefs);
            if (userPreferences.Units == reading.Unit )
            {
                return $"{MathF.Round(reading.Value, 2)} {reading.Unit}";
            }
            else if(userPreferences.Units.Equals("mmol/l", StringComparison.OrdinalIgnoreCase)
                && reading.Unit.Equals("mg/dl",StringComparison.OrdinalIgnoreCase))
            {
                return $"{MathF.Round(reading.Value/18, 2)} mmol/l";
            }
            else
            {
                return $"{MathF.Round(reading.Value * 18, 2)} mg/dl";
            }

        }

        public float ConvertValue(GlucoseValue value)
        {
            var reading = value;
            var prefs = Preferences.Get(Stores.preferenceStore, null);
            if (string.IsNullOrEmpty(prefs))
            {
                return 0;
            }
            UserPreferences userPreferences = JsonSerializer.Deserialize<UserPreferences>(prefs);
            if (userPreferences.Units == reading.Unit)
            {
                return MathF.Round(reading.Value, 2);
            }
            else if (userPreferences.Units.Equals("mmol/l", StringComparison.OrdinalIgnoreCase)
                && reading.Unit.Equals("mg/dl", StringComparison.OrdinalIgnoreCase))
            {
                return MathF.Round(reading.Value / 18, 2);
            }
            else
            {
                return MathF.Round(reading.Value * 18, 2);
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            float.TryParse((string)value, out float reading);
            var prefs = Preferences.Get(Stores.preferenceStore, null);
            if (string.IsNullOrEmpty(prefs))
            {
                return null;
            }
            UserPreferences userPreferences = JsonSerializer.Deserialize <UserPreferences>(prefs);
            if (userPreferences.Units == "mmol/l")
            {
                return (float)value;
            }
            else
            {
                return MathF.Round((float)value / 18, 2);
            }

        }
    }
}

using System.Globalization;
using System.Text.Json;

namespace SukaliCareApp.Converters
{
    public class UnitsConveter : IValueConverter
    {
        // 1 mmol/L of glucose = 18.0 mg/dL (rounded from 18.016).
        private const float MgDlPerMmolL = 18f;
        private const string MmolL = "mmol/l";
        private const string MgDl = "mg/dl";

        /// <summary>
        /// The user's preferred unit. Defaults to mmol/L, the unit used in Uganda,
        /// if preferences have not been saved yet.
        /// </summary>
        public static string PreferredUnit()
        {
            string prefs = Preferences.Get(Stores.preferenceStore, null);
            if (string.IsNullOrEmpty(prefs))
            {
                return MmolL;
            }
            UserPreferences userPreferences = JsonSerializer.Deserialize<UserPreferences>(prefs);
            return string.IsNullOrEmpty(userPreferences?.Units) ? MmolL : userPreferences.Units.ToLowerInvariant();
        }

        public static bool IsMmolL(string unit) =>
            string.Equals(unit, MmolL, StringComparison.OrdinalIgnoreCase);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not GlucoseValue reading)
            {
                return null;
            }
            float converted = ConvertValue(reading);
            string unit = IsMmolL(PreferredUnit()) ? MmolL : MgDl;
            return $"{converted.ToString(IsMmolL(unit) ? "0.0" : "0", culture)} {unit}";
        }

        public float ConvertValue(GlucoseValue value)
        {
            if (value is null)
            {
                return 0;
            }

            bool wantMmol = IsMmolL(PreferredUnit());
            bool haveMmol = IsMmolL(value.Unit);

            if (wantMmol == haveMmol)
            {
                return MathF.Round(value.Value, 2);
            }
            return wantMmol
                ? MathF.Round(value.Value / MgDlPerMmolL, 2)
                : MathF.Round(value.Value * MgDlPerMmolL, 2);
        }

        // Converts text typed in the user's unit back to mmol/L.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value?.ToString()?.Split(' ')[0];
            if (!float.TryParse(text, NumberStyles.Float, culture, out float reading)
                && !float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out reading))
            {
                return null;
            }
            return IsMmolL(PreferredUnit())
                ? reading
                : MathF.Round(reading / MgDlPerMmolL, 2);
        }
    }
}

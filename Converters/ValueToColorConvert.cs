using System.Globalization;

namespace SukaliCareApp.Converters
{
    /// <summary>
    /// Colours a glucose reading by clinical band.
    /// Bands follow the International Consensus on Time in Range (Battelino et al., Diabetes Care 2019):
    ///   Very low  &lt; 3.0 mmol/L  (&lt; 54 mg/dL)      Level 2 hypoglycaemia
    ///   Low       3.0 to 3.8      (54 to 69)        Level 1 hypoglycaemia
    ///   In range  3.9 to 10.0     (70 to 180)       Target range
    ///   High      10.1 to 13.9    (181 to 250)      Level 1 hyperglycaemia
    ///   Very high &gt; 13.9          (&gt; 250)           Level 2 hyperglycaemia
    /// Review these against local guidance before clinical use.
    /// </summary>
    public class ValueToColorConvert : IValueConverter
    {
        public const float VeryLowMmol = 3.0f;
        public const float LowMmol = 3.9f;
        public const float HighMmol = 10.0f;
        public const float VeryHighMmol = 13.9f;

        public static readonly Color VeryLowColour = Color.FromArgb("#8B0000");  // dark red
        public static readonly Color LowColour = Color.FromArgb("#D9534F");      // red
        public static readonly Color InRangeColour = Color.FromArgb("#3C9A5F");  // green
        public static readonly Color HighColour = Color.FromArgb("#E0A800");     // amber
        public static readonly Color VeryHighColour = Color.FromArgb("#C0392B"); // red

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not GlucoseValue reading)
            {
                return InRangeColour;
            }

            // Work in mmol/L whatever unit the reading was stored in.
            float mmol = UnitsConveter.IsMmolL(reading.Unit) ? reading.Value : reading.Value / 18f;

            if (mmol < VeryLowMmol) return VeryLowColour;
            if (mmol < LowMmol) return LowColour;
            if (mmol <= HighMmol) return InRangeColour;
            if (mmol <= VeryHighMmol) return HighColour;
            return VeryHighColour;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

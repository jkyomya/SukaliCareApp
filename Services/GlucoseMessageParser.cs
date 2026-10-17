using System.Globalization;
using System.Text.RegularExpressions;

namespace SukaliCareApp.Services;

/// <summary>
/// Parses glucose messages sent by the CGM device over Bluetooth.
/// Expected format: "Glucose_reading_6.5_mmol/L_end".
/// </summary>
public static class GlucoseMessageParser
{
    // The value is captured in a named group so it can actually be read.
    // Integers ("7") and decimals ("6.5") are both accepted, units are case-insensitive.
    private static readonly Regex Pattern = new(
        @"Glucose_reading_(?<value>\d+(?:\.\d+)?)_mmol/L_end",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static bool TryParse(string raw, out float valueMmolL)
    {
        valueMmolL = 0;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        // Socket buffers are padded with null bytes; strip them and any whitespace.
        string cleaned = raw.Replace("\0", string.Empty).Trim();

        Match match = Pattern.Match(cleaned);
        if (!match.Success)
        {
            return false;
        }

        // Always parse with "." as the decimal separator, whatever the phone's language.
        return float.TryParse(
            match.Groups["value"].Value,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out valueMmolL);
    }
}

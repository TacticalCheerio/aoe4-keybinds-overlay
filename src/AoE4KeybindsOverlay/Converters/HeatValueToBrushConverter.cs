using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AoE4KeybindsOverlay.Converters;

/// <summary>
/// Converts a double value (0.0 to 1.0) representing heat intensity to a
/// gradient brush ranging from transparent blue through yellow to red.
/// </summary>
public sealed class HeatValueToBrushConverter : IValueConverter
{
    // Color stops for the heatmap gradient:
    // 0.0 -> Transparent blue (#00337AB7)
    // 0.5 -> Yellow (#FFFFC107)
    // 1.0 -> Red (#FFF44336)

    private static readonly Color ColorCool = Color.FromArgb(0x00, 0x33, 0x7A, 0xB7);
    private static readonly Color ColorMid = Color.FromArgb(0xFF, 0xFF, 0xC1, 0x07);
    private static readonly Color ColorHot = Color.FromArgb(0xFF, 0xF4, 0x43, 0x36);

    /// <summary>
    /// Converts a heat value (0.0-1.0) to a SolidColorBrush along the cool-to-hot gradient.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var intensity = value is double d ? Math.Clamp(d, 0.0, 1.0) : 0.0;

        Color resultColor;
        if (intensity <= 0.5)
        {
            // Interpolate from cool (transparent blue) to mid (yellow)
            var t = intensity * 2.0; // normalize 0..0.5 to 0..1
            resultColor = LerpColor(ColorCool, ColorMid, t);
        }
        else
        {
            // Interpolate from mid (yellow) to hot (red)
            var t = (intensity - 0.5) * 2.0; // normalize 0.5..1 to 0..1
            resultColor = LerpColor(ColorMid, ColorHot, t);
        }

        return new SolidColorBrush(resultColor);
    }

    /// <summary>
    /// Not supported. This is a one-way converter.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("HeatValueToBrushConverter is a one-way converter.");
    }

    /// <summary>
    /// Linearly interpolates between two colors.
    /// </summary>
    private static Color LerpColor(Color from, Color to, double t)
    {
        var clampedT = Math.Clamp(t, 0.0, 1.0);
        return Color.FromArgb(
            (byte)(from.A + (to.A - from.A) * clampedT),
            (byte)(from.R + (to.R - from.R) * clampedT),
            (byte)(from.G + (to.G - from.G) * clampedT),
            (byte)(from.B + (to.B - from.B) * clampedT)
        );
    }
}

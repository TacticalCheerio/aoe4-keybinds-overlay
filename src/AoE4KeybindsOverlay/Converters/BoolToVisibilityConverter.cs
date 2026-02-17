using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AoE4KeybindsOverlay.Converters;

/// <summary>
/// Converts a boolean value to a <see cref="Visibility"/> value.
/// True maps to Visible, False maps to Collapsed.
/// Pass "Invert" as the converter parameter to reverse the mapping.
/// </summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean to a Visibility value.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var boolValue = value is bool b && b;

        if (parameter is string paramStr &&
            paramStr.Equals("Invert", StringComparison.OrdinalIgnoreCase))
        {
            boolValue = !boolValue;
        }

        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// Converts a Visibility value back to a boolean.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isVisible = value is Visibility visibility && visibility == Visibility.Visible;

        if (parameter is string paramStr &&
            paramStr.Equals("Invert", StringComparison.OrdinalIgnoreCase))
        {
            isVisible = !isVisible;
        }

        return isVisible;
    }
}

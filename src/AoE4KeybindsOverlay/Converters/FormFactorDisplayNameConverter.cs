using System.Globalization;
using System.Windows.Data;
using AoE4KeybindsOverlay.Models;

namespace AoE4KeybindsOverlay.Converters;

/// <summary>
/// Converts a <see cref="KeyboardFormFactor"/> enum value to its user-friendly display name.
/// </summary>
public sealed class FormFactorDisplayNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is KeyboardFormFactor ff ? ff.DisplayName() : value?.ToString() ?? "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is string s ? KeyboardFormFactorExtensions.ParseFormFactor(s) : KeyboardFormFactor.Full;
    }
}

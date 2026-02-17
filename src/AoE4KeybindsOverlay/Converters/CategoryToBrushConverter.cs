using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using AoE4KeybindsOverlay.Models;

namespace AoE4KeybindsOverlay.Converters;

/// <summary>
/// Converts a <see cref="BindingCategory"/> value to a <see cref="SolidColorBrush"/>
/// using the category's <see cref="BindingCategoryExtensions.DefaultColor"/> value.
/// </summary>
public sealed class CategoryToBrushConverter : IValueConverter
{
    /// <summary>
    /// Converts a <see cref="BindingCategory"/> to a <see cref="SolidColorBrush"/>.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is BindingCategory category)
        {
            var colorHex = category.DefaultColor();
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(colorHex);
                return new SolidColorBrush(color);
            }
            catch
            {
                return Brushes.White;
            }
        }

        return Brushes.White;
    }

    /// <summary>
    /// Not supported. This is a one-way converter.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("CategoryToBrushConverter is a one-way converter.");
    }
}

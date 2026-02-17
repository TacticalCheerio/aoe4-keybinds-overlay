using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace AoE4KeybindsOverlay.Views.Controls;

/// <summary>
/// A small legend control showing category colors as colored dots with category names.
/// Each item should expose: Category (BindingCategory enum), DisplayName (string).
/// </summary>
public partial class CategoryLegend : UserControl
{
    #region Dependency Properties

    /// <summary>
    /// The collection of legend items to display.
    /// Each item should expose: Category, DisplayName.
    /// </summary>
    public static readonly DependencyProperty LegendItemsProperty =
        DependencyProperty.Register(
            nameof(LegendItems), typeof(IEnumerable), typeof(CategoryLegend),
            new PropertyMetadata(null));

    #endregion

    #region Properties

    public IEnumerable? LegendItems
    {
        get => (IEnumerable?)GetValue(LegendItemsProperty);
        set => SetValue(LegendItemsProperty, value);
    }

    #endregion

    public CategoryLegend()
    {
        InitializeComponent();
    }
}

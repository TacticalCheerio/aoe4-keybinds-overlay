using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace AoE4KeybindsOverlay.Views.Controls;

/// <summary>
/// A control that displays active keybindings as a scrollable list.
/// Each item shows the key combination (colored by category), the command name,
/// and a category badge. Items highlight when triggered.
/// </summary>
public partial class BindingListControl : UserControl
{
    #region Dependency Properties

    /// <summary>
    /// The collection of binding items to display.
    /// Each item should expose: KeyComboText, CommandName, Category, CategoryName, IsTriggered.
    /// </summary>
    public static readonly DependencyProperty BindingItemsProperty =
        DependencyProperty.Register(
            nameof(BindingItems), typeof(IEnumerable), typeof(BindingListControl),
            new PropertyMetadata(null));

    #endregion

    #region Properties

    public IEnumerable? BindingItems
    {
        get => (IEnumerable?)GetValue(BindingItemsProperty);
        set => SetValue(BindingItemsProperty, value);
    }

    #endregion

    public BindingListControl()
    {
        InitializeComponent();
    }
}

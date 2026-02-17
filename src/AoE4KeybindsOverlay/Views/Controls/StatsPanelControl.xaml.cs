using System.Windows;
using System.Windows.Controls;
using AoE4KeybindsOverlay.ViewModels;

namespace AoE4KeybindsOverlay.Views.Controls;

/// <summary>
/// A control that displays keybinding usage statistics including session summary,
/// match history, and most/least/never used commands.
/// </summary>
public partial class StatsPanelControl : UserControl
{
    /// <summary>
    /// The StatisticsViewModel to display.
    /// </summary>
    public static readonly DependencyProperty StatisticsProperty =
        DependencyProperty.Register(
            nameof(Statistics), typeof(StatisticsViewModel), typeof(StatsPanelControl),
            new PropertyMetadata(null));

    public StatisticsViewModel? Statistics
    {
        get => (StatisticsViewModel?)GetValue(StatisticsProperty);
        set => SetValue(StatisticsProperty, value);
    }

    public StatsPanelControl()
    {
        InitializeComponent();
    }
}

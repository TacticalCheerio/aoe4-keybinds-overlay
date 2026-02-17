using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AoE4KeybindsOverlay.Models;

namespace AoE4KeybindsOverlay.ViewModels;

/// <summary>
/// ViewModel for the statistics and heatmap display.
/// </summary>
public partial class StatisticsViewModel : ObservableObject
{
    /// <summary>Most used keybindings.</summary>
    public ObservableCollection<KeyUsageStatistic> MostUsed { get; } = [];

    /// <summary>Least used keybindings.</summary>
    public ObservableCollection<KeyUsageStatistic> LeastUsed { get; } = [];

    /// <summary>Keybindings that have never been used.</summary>
    public ObservableCollection<string> NeverUsed { get; } = [];

    /// <summary>Completed match history.</summary>
    public ObservableCollection<MatchStatistics> MatchHistory { get; } = [];

    /// <summary>Current session stats.</summary>
    [ObservableProperty]
    private SessionStatistics? _currentSession;

    /// <summary>Current in-progress match stats, or null if not in a match.</summary>
    [ObservableProperty]
    private MatchStatistics? _currentMatch;

    /// <summary>Whether heatmap mode is active.</summary>
    [ObservableProperty]
    private bool _isHeatmapActive;

    /// <summary>Total key presses across all sessions.</summary>
    [ObservableProperty]
    private int _totalAllTimePresses;

    /// <summary>Total matched binding presses across all sessions.</summary>
    [ObservableProperty]
    private int _totalAllTimeMatched;

    /// <summary>
    /// Refreshes the statistics display from the statistics service.
    /// </summary>
    public void Refresh(
        IReadOnlyList<KeyUsageStatistic> mostUsed,
        IReadOnlyList<KeyUsageStatistic> leastUsed,
        IReadOnlyList<string> neverUsed,
        SessionStatistics? currentSession,
        IReadOnlyList<MatchStatistics> matchHistory,
        MatchStatistics? currentMatch)
    {
        MostUsed.Clear();
        foreach (var stat in mostUsed)
            MostUsed.Add(stat);

        LeastUsed.Clear();
        foreach (var stat in leastUsed)
            LeastUsed.Add(stat);

        NeverUsed.Clear();
        foreach (var name in neverUsed)
            NeverUsed.Add(name);

        MatchHistory.Clear();
        foreach (var match in matchHistory.OrderByDescending(m => m.MatchStart))
            MatchHistory.Add(match);

        CurrentSession = currentSession;
        CurrentMatch = currentMatch;
    }

    [RelayCommand]
    private void ToggleHeatmap()
    {
        IsHeatmapActive = !IsHeatmapActive;
    }
}

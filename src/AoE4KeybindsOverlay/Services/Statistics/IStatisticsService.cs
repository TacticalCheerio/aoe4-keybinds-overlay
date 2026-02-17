using AoE4KeybindsOverlay.Models;

namespace AoE4KeybindsOverlay.Services.Statistics;

/// <summary>
/// Tracks and queries keybinding usage statistics.
/// </summary>
public interface IStatisticsService
{
    /// <summary>Records a key press event, optionally matched to a keybinding.</summary>
    void RecordKeyPress(string relicKeyName, ModifierKeys modifiers, Models.Keybinding? matchedBinding);

    /// <summary>Starts a new tracking session.</summary>
    void StartSession();

    /// <summary>Ends the current tracking session.</summary>
    void EndSession();

    /// <summary>Starts tracking a new in-game match.</summary>
    void StartMatch(int matchTypeId, string sessionId);

    /// <summary>Ends the current in-game match and saves its stats.</summary>
    void EndMatch();

    /// <summary>Whether a match is currently being tracked.</summary>
    bool IsInMatch { get; }

    /// <summary>Gets the current match statistics, or null if not in a match.</summary>
    MatchStatistics? CurrentMatch { get; }

    /// <summary>Gets all completed match statistics.</summary>
    IReadOnlyList<MatchStatistics> GetMatchHistory();

    /// <summary>Gets the most frequently used keybindings.</summary>
    IReadOnlyList<KeyUsageStatistic> GetMostUsed(int count = 10);

    /// <summary>Gets the least frequently used keybindings (that have been used at least once).</summary>
    IReadOnlyList<KeyUsageStatistic> GetLeastUsed(int count = 10);

    /// <summary>Gets keybindings that have never been used.</summary>
    IReadOnlyList<Models.Keybinding> GetNeverUsed();

    /// <summary>Gets heatmap data mapping Relic key names to 0.0-1.0 intensity.</summary>
    IReadOnlyDictionary<string, double> GetHeatmapData();

    /// <summary>Gets the current session statistics.</summary>
    SessionStatistics? GetCurrentSessionStats();

    /// <summary>Saves statistics to persistent storage.</summary>
    Task SaveAsync();

    /// <summary>Loads statistics from persistent storage.</summary>
    Task LoadAsync();

    /// <summary>Sets the keybinding list for tracking never-used bindings.</summary>
    void SetBindings(IReadOnlyList<Models.Keybinding> bindings);
}

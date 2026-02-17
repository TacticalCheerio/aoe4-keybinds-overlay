namespace AoE4KeybindsOverlay.Models;

/// <summary>
/// Represents statistics for a single usage session.
/// </summary>
public sealed class SessionStatistics
{
    /// <summary>
    /// Gets the timestamp when the session started.
    /// </summary>
    public DateTime SessionStart { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when the session ended.
    /// </summary>
    public DateTime? SessionEnd { get; set; }

    /// <summary>
    /// Gets the total duration of the session.
    /// </summary>
    public TimeSpan Duration => (SessionEnd ?? DateTime.UtcNow) - SessionStart;

    /// <summary>
    /// Gets or sets the total number of key presses during the session.
    /// </summary>
    public int TotalKeyPresses { get; set; }

    /// <summary>
    /// Gets or sets the number of key presses that matched a keybinding.
    /// </summary>
    public int MatchedPresses { get; set; }

    /// <summary>
    /// Gets or sets the number of key presses that did not match any keybinding.
    /// </summary>
    public int UnmatchedPresses { get; set; }

    /// <summary>
    /// Gets the dictionary of command IDs to their usage counts.
    /// </summary>
    public Dictionary<string, int> CommandUsageCounts { get; init; } = new();

    /// <summary>
    /// Gets the dictionary of raw key names to their press counts.
    /// </summary>
    public Dictionary<string, int> RawKeyCounts { get; init; } = new();
}

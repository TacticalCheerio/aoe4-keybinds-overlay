using System.Text.Json.Serialization;

namespace AoE4KeybindsOverlay.Models;

/// <summary>
/// Represents keybinding usage statistics for a single AoE4 match.
/// </summary>
public sealed class MatchStatistics
{
    /// <summary>Unique identifier for this match.</summary>
    public string MatchId { get; init; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>Session ID from the game's warnings.log.</summary>
    public string? GameSessionId { get; init; }

    /// <summary>Match type ID from the game (e.g., 22 = rogue mode).</summary>
    public int? MatchTypeId { get; init; }

    /// <summary>When the match started (UTC).</summary>
    public DateTime MatchStart { get; init; } = DateTime.UtcNow;

    /// <summary>When the match ended (UTC).</summary>
    public DateTime? MatchEnd { get; set; }

    /// <summary>Duration of the match.</summary>
    [JsonIgnore]
    public TimeSpan Duration => (MatchEnd ?? DateTime.UtcNow) - MatchStart;

    /// <summary>Total key presses during this match.</summary>
    public int TotalKeyPresses { get; set; }

    /// <summary>Key presses that matched a keybinding.</summary>
    public int MatchedPresses { get; set; }

    /// <summary>Key presses that did not match any keybinding.</summary>
    public int UnmatchedPresses { get; set; }

    /// <summary>Command usage counts keyed by "groupName:commandId".</summary>
    public Dictionary<string, int> CommandUsageCounts { get; init; } = new();

    /// <summary>Raw key press counts keyed by Relic key name.</summary>
    public Dictionary<string, int> RawKeyCounts { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Persistent collection of all match statistics.
/// </summary>
public sealed class MatchStatisticsData
{
    /// <summary>All completed match statistics, most recent first.</summary>
    public List<MatchStatistics> Matches { get; init; } = [];
}

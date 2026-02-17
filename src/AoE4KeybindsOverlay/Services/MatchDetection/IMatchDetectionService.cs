namespace AoE4KeybindsOverlay.Services.MatchDetection;

/// <summary>
/// Event args for match lifecycle events detected from the AoE4 warnings.log.
/// </summary>
public sealed class MatchStartedEventArgs : EventArgs
{
    /// <summary>The match type ID from the log (e.g., 22 = rogue mode).</summary>
    public int MatchTypeId { get; init; }

    /// <summary>The session ID from the log.</summary>
    public string SessionId { get; init; } = string.Empty;

    /// <summary>Timestamp from the log line.</summary>
    public DateTime Timestamp { get; init; }
}

/// <summary>
/// Event args for match end events.
/// </summary>
public sealed class MatchEndedEventArgs : EventArgs
{
    /// <summary>Timestamp from the log line.</summary>
    public DateTime Timestamp { get; init; }
}

/// <summary>
/// Monitors AoE4's warnings.log for match start and end events.
/// </summary>
public interface IMatchDetectionService : IDisposable
{
    /// <summary>Fired when a match starts.</summary>
    event EventHandler<MatchStartedEventArgs>? MatchStarted;

    /// <summary>Fired when a match ends.</summary>
    event EventHandler<MatchEndedEventArgs>? MatchEnded;

    /// <summary>Whether a match is currently in progress.</summary>
    bool IsInMatch { get; }

    /// <summary>Starts monitoring the warnings.log file.</summary>
    void Start();

    /// <summary>Stops monitoring.</summary>
    void Stop();
}

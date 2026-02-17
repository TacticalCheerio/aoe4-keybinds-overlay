using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Threading;

namespace AoE4KeybindsOverlay.Services.MatchDetection;

/// <summary>
/// Monitors AoE4's warnings.log file by tailing it for match start/end patterns.
///
/// Match start pattern:
///   (I) [HH:MM:SS.mmm] [...]: [Match Flow] MatchSetup::StartMatch() - m_matchStatus = ST_STARTING [match type: N, session id N]
///
/// Match end pattern:
///   (I) [HH:MM:SS.mmm] [...]: MatchSetupManager::RequestLeaveGameMatch - RequestGameEnd()
/// </summary>
public sealed class WarningsLogMatchDetectionService : IMatchDetectionService
{
    private static readonly Regex MatchStartPattern = new(
        @"\[(\d{2}:\d{2}:\d{2}\.\d{3})\].*\[Match Flow\] MatchSetup::StartMatch\(\).*match type: (\d+), session id (\d+)",
        RegexOptions.Compiled);

    private static readonly Regex MatchEndPattern = new(
        @"\[(\d{2}:\d{2}:\d{2}\.\d{3})\].*MatchSetupManager::RequestLeaveGameMatch.*RequestGameEnd\(\)",
        RegexOptions.Compiled);

    private readonly string _warningsLogPath;
    private readonly Dispatcher _dispatcher;
    private FileSystemWatcher? _watcher;
    private long _lastReadPosition;
    private Timer? _pollTimer;
    private bool _disposed;

    public event EventHandler<MatchStartedEventArgs>? MatchStarted;
    public event EventHandler<MatchEndedEventArgs>? MatchEnded;

    public bool IsInMatch { get; private set; }

    /// <summary>
    /// Creates a new instance monitoring the specified warnings.log.
    /// </summary>
    /// <param name="aoe4DocumentsPath">Path to "Documents/my games/Age of Empires IV".</param>
    /// <param name="dispatcher">UI dispatcher for marshalling events.</param>
    public WarningsLogMatchDetectionService(string aoe4DocumentsPath, Dispatcher dispatcher)
    {
        _warningsLogPath = Path.Combine(aoe4DocumentsPath, "warnings.log");
        _dispatcher = dispatcher;
    }

    public void Start()
    {
        if (!File.Exists(_warningsLogPath))
            return;

        // Seek to end of file so we only process new lines
        using (var fs = OpenFileShared(_warningsLogPath))
        {
            if (fs is not null)
                _lastReadPosition = fs.Length;
        }

        // Watch for file changes
        var dir = Path.GetDirectoryName(_warningsLogPath)!;
        var file = Path.GetFileName(_warningsLogPath);

        _watcher = new FileSystemWatcher(dir, file)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true
        };
        _watcher.Changed += OnLogFileChanged;

        // Also poll every 2 seconds as a fallback (FileSystemWatcher can miss events)
        _pollTimer = new Timer(_ => ReadNewLines(), null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
    }

    public void Stop()
    {
        _pollTimer?.Dispose();
        _pollTimer = null;

        if (_watcher is not null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Changed -= OnLogFileChanged;
            _watcher.Dispose();
            _watcher = null;
        }
    }

    private void OnLogFileChanged(object sender, FileSystemEventArgs e)
    {
        ReadNewLines();
    }

    private void ReadNewLines()
    {
        if (_disposed) return;

        try
        {
            using var fs = OpenFileShared(_warningsLogPath);
            if (fs is null || fs.Length <= _lastReadPosition)
                return;

            fs.Seek(_lastReadPosition, SeekOrigin.Begin);

            using var reader = new StreamReader(fs);
            string? line;
            while ((line = reader.ReadLine()) is not null)
            {
                ProcessLine(line);
            }

            _lastReadPosition = fs.Position;
        }
        catch (IOException)
        {
            // File may be locked by the game, retry next poll
        }
    }

    private void ProcessLine(string line)
    {
        // Check for match start
        var startMatch = MatchStartPattern.Match(line);
        if (startMatch.Success)
        {
            IsInMatch = true;
            var timestamp = ParseLogTimestamp(startMatch.Groups[1].Value);
            var matchType = int.TryParse(startMatch.Groups[2].Value, out var mt) ? mt : 0;
            var sessionId = startMatch.Groups[3].Value;

            _dispatcher.BeginInvoke(() =>
            {
                MatchStarted?.Invoke(this, new MatchStartedEventArgs
                {
                    MatchTypeId = matchType,
                    SessionId = sessionId,
                    Timestamp = timestamp
                });
            });
            return;
        }

        // Check for match end
        var endMatch = MatchEndPattern.Match(line);
        if (endMatch.Success)
        {
            IsInMatch = false;
            var timestamp = ParseLogTimestamp(endMatch.Groups[1].Value);

            _dispatcher.BeginInvoke(() =>
            {
                MatchEnded?.Invoke(this, new MatchEndedEventArgs
                {
                    Timestamp = timestamp
                });
            });
        }
    }

    /// <summary>
    /// Parses a time-only timestamp from the log (HH:mm:ss.fff) and combines with today's date.
    /// </summary>
    private static DateTime ParseLogTimestamp(string timeStr)
    {
        if (TimeSpan.TryParseExact(timeStr, @"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture, out var time))
        {
            return DateTime.UtcNow.Date + time;
        }
        return DateTime.UtcNow;
    }

    /// <summary>
    /// Opens a file with shared read access so the game can keep writing.
    /// </summary>
    private static FileStream? OpenFileShared(string path)
    {
        try
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
        catch (IOException)
        {
            return null;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
    }
}

using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using AoE4KeybindsOverlay.Models;

namespace AoE4KeybindsOverlay.Services.Statistics;

/// <summary>
/// Tracks keybinding usage statistics with in-memory accumulation and JSON persistence.
/// </summary>
public sealed class StatisticsService : IStatisticsService
{
    private readonly string _dataPath;
    private SessionStatistics? _currentSession;
    private MatchStatistics? _currentMatch;
    private IReadOnlyList<Models.Keybinding> _allBindings = [];

    private readonly Dictionary<string, KeyUsageStatistic> _allTimeStats = new();
    private readonly Dictionary<string, int> _allTimeRawKeyCounts = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<MatchStatistics> _matchHistory = [];

    public bool IsInMatch => _currentMatch is not null;
    public MatchStatistics? CurrentMatch => _currentMatch;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public StatisticsService()
    {
        _dataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AoE4KeybindsOverlay");
        Directory.CreateDirectory(_dataPath);
    }

    public void SetBindings(IReadOnlyList<Models.Keybinding> bindings)
    {
        _allBindings = bindings;
    }

    public void StartSession()
    {
        _currentSession = new SessionStatistics();
    }

    public void EndSession()
    {
        if (_currentSession is not null)
        {
            _currentSession.SessionEnd = DateTime.UtcNow;
        }
    }

    public void StartMatch(int matchTypeId, string sessionId)
    {
        // End any existing match first
        if (_currentMatch is not null)
            EndMatch();

        _currentMatch = new MatchStatistics
        {
            MatchTypeId = matchTypeId,
            GameSessionId = sessionId,
            MatchStart = DateTime.UtcNow
        };
    }

    public void EndMatch()
    {
        if (_currentMatch is null) return;

        _currentMatch.MatchEnd = DateTime.UtcNow;
        _matchHistory.Add(_currentMatch);
        _currentMatch = null;

        // Auto-save match data
        _ = SaveMatchHistoryAsync();
    }

    public IReadOnlyList<MatchStatistics> GetMatchHistory() => _matchHistory;

    public void RecordKeyPress(string relicKeyName, ModifierKeys modifiers, Models.Keybinding? matchedBinding)
    {
        if (_currentSession is null) return;

        _currentSession.TotalKeyPresses++;

        // Raw key count (for heatmap)
        _currentSession.RawKeyCounts.TryGetValue(relicKeyName, out var count);
        _currentSession.RawKeyCounts[relicKeyName] = count + 1;

        _allTimeRawKeyCounts.TryGetValue(relicKeyName, out var allTimeCount);
        _allTimeRawKeyCounts[relicKeyName] = allTimeCount + 1;

        // Per-match tracking
        if (_currentMatch is not null)
        {
            _currentMatch.TotalKeyPresses++;
            _currentMatch.RawKeyCounts.TryGetValue(relicKeyName, out var matchKeyCount);
            _currentMatch.RawKeyCounts[relicKeyName] = matchKeyCount + 1;
        }

        if (matchedBinding is not null)
        {
            _currentSession.MatchedPresses++;
            var key = $"{matchedBinding.GroupName}:{matchedBinding.CommandId}";
            _currentSession.CommandUsageCounts.TryGetValue(key, out var bCount);
            _currentSession.CommandUsageCounts[key] = bCount + 1;

            if (!_allTimeStats.TryGetValue(key, out var stat))
            {
                stat = new KeyUsageStatistic
                {
                    CommandId = matchedBinding.CommandId,
                    GroupName = matchedBinding.GroupName,
                    Category = matchedBinding.Category,
                    FirstUsed = DateTime.UtcNow
                };
                _allTimeStats[key] = stat;
            }
            stat.TotalPresses++;
            stat.LastUsed = DateTime.UtcNow;

            // Per-match command tracking
            if (_currentMatch is not null)
            {
                _currentMatch.MatchedPresses++;
                _currentMatch.CommandUsageCounts.TryGetValue(key, out var matchCmdCount);
                _currentMatch.CommandUsageCounts[key] = matchCmdCount + 1;
            }
        }
        else
        {
            _currentSession.UnmatchedPresses++;
            if (_currentMatch is not null)
                _currentMatch.UnmatchedPresses++;
        }
    }

    public IReadOnlyList<KeyUsageStatistic> GetMostUsed(int count = 10)
    {
        return _allTimeStats.Values
            .OrderByDescending(s => s.TotalPresses)
            .Take(count)
            .ToList();
    }

    public IReadOnlyList<KeyUsageStatistic> GetLeastUsed(int count = 10)
    {
        return _allTimeStats.Values
            .Where(s => s.TotalPresses > 0)
            .OrderBy(s => s.TotalPresses)
            .Take(count)
            .ToList();
    }

    public IReadOnlyList<Models.Keybinding> GetNeverUsed()
    {
        return _allBindings
            .Where(b => !b.Primary.IsEmpty)
            .Where(b =>
            {
                var key = $"{b.GroupName}:{b.CommandId}";
                return !_allTimeStats.ContainsKey(key);
            })
            .ToList();
    }

    public IReadOnlyDictionary<string, double> GetHeatmapData()
    {
        if (_allTimeRawKeyCounts.Count == 0)
            return new Dictionary<string, double>();

        var maxCount = _allTimeRawKeyCounts.Values.Max();
        if (maxCount == 0)
            return new Dictionary<string, double>();

        return _allTimeRawKeyCounts.ToDictionary(
            kvp => kvp.Key,
            kvp => (double)kvp.Value / maxCount,
            StringComparer.OrdinalIgnoreCase);
    }

    public SessionStatistics? GetCurrentSessionStats() => _currentSession;

    public async Task SaveAsync()
    {
        var data = new StatisticsData
        {
            AllTimeStats = _allTimeStats.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            AllTimeRawKeyCounts = _allTimeRawKeyCounts.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };

        var path = Path.Combine(_dataPath, "statistics.json");
        var json = JsonSerializer.Serialize(data, JsonOptions);
        await File.WriteAllTextAsync(path, json);

        await SaveMatchHistoryAsync();
    }

    public async Task LoadAsync()
    {
        var path = Path.Combine(_dataPath, "statistics.json");
        if (File.Exists(path))
        {
            try
            {
                var json = await File.ReadAllTextAsync(path);
                var data = JsonSerializer.Deserialize<StatisticsData>(json, JsonOptions);
                if (data is not null)
                {
                    _allTimeStats.Clear();
                    foreach (var kvp in data.AllTimeStats)
                        _allTimeStats[kvp.Key] = kvp.Value;

                    _allTimeRawKeyCounts.Clear();
                    foreach (var kvp in data.AllTimeRawKeyCounts)
                        _allTimeRawKeyCounts[kvp.Key] = kvp.Value;
                }
            }
            catch (JsonException)
            {
                // Corrupted stats file; start fresh
            }
        }

        await LoadMatchHistoryAsync();
    }

    private async Task SaveMatchHistoryAsync()
    {
        try
        {
            var data = new MatchStatisticsData { Matches = [.. _matchHistory] };
            var path = Path.Combine(_dataPath, "match_history.json");
            var json = JsonSerializer.Serialize(data, JsonOptions);
            await File.WriteAllTextAsync(path, json);
        }
        catch (IOException)
        {
            // Best effort save
        }
    }

    private async Task LoadMatchHistoryAsync()
    {
        var path = Path.Combine(_dataPath, "match_history.json");
        if (!File.Exists(path)) return;

        try
        {
            var json = await File.ReadAllTextAsync(path);
            var data = JsonSerializer.Deserialize<MatchStatisticsData>(json, JsonOptions);
            if (data?.Matches is not null)
            {
                _matchHistory.Clear();
                _matchHistory.AddRange(data.Matches);
            }
        }
        catch (JsonException)
        {
            // Corrupted match history; start fresh
        }
    }

    /// <summary>
    /// Internal persistence data structure.
    /// </summary>
    private sealed class StatisticsData
    {
        public Dictionary<string, KeyUsageStatistic> AllTimeStats { get; init; } = new();
        public Dictionary<string, int> AllTimeRawKeyCounts { get; init; } = new();
    }
}

using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using AoE4KeybindsOverlay.Models;

namespace AoE4KeybindsOverlay.ViewModels;

/// <summary>
/// ViewModel managing the visual state of the entire keyboard display.
/// </summary>
public partial class KeyboardViewModel : ObservableObject
{
    private const double ScaleFactor = 50.0;

    private readonly Dictionary<string, KeyViewModel> _keyLookup = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Tracks which keys were highlighted in the last update, for diff-based clearing.</summary>
    private readonly HashSet<string> _previouslyHighlightedKeys = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>One KeyViewModel per physical key on the keyboard.</summary>
    public ObservableCollection<KeyViewModel> Keys { get; } = [];

    /// <summary>Canvas width in pixels, adjusted for the active form factor.</summary>
    [ObservableProperty]
    private double _canvasWidth = 22.5 * ScaleFactor;

    /// <summary>Canvas height in pixels, adjusted for the active form factor.</summary>
    [ObservableProperty]
    private double _canvasHeight = 6.5 * ScaleFactor;

    /// <summary>
    /// Loads the keyboard layout from the embedded JSON data file,
    /// filtering keys based on the selected form factor preset.
    /// </summary>
    public void LoadLayout(string jsonPath, KeyboardFormFactor formFactor = KeyboardFormFactor.Full)
    {
        var json = File.ReadAllText(jsonPath);
        using var doc = JsonDocument.Parse(json);

        var keyboard = doc.RootElement.GetProperty("keyboard");
        var keys = keyboard.GetProperty("keys");

        var excludedKeys = formFactor.GetExcludedKeyIds();
        var shiftY = formFactor.ExcludesFRow();

        Keys.Clear();
        _keyLookup.Clear();
        _previouslyHighlightedKeys.Clear();

        foreach (var keyElement in keys.EnumerateArray())
        {
            var keyId = keyElement.GetProperty("keyId").GetString() ?? "";

            // Skip keys excluded by the form factor
            if (excludedKeys.Contains(keyId))
                continue;

            var y = keyElement.GetProperty("y").GetDouble();

            // Shift Y up by 1.25 to close the F-row gap when F-row is excluded
            if (shiftY && y > 0)
            {
                y -= 1.25;
            }

            var vm = new KeyViewModel
            {
                KeyId = keyId,
                Label = keyElement.GetProperty("label").GetString() ?? "",
                X = keyElement.GetProperty("x").GetDouble(),
                Y = y,
                Width = keyElement.TryGetProperty("width", out var w) ? w.GetDouble() : 1.0,
                Height = keyElement.TryGetProperty("height", out var h) ? h.GetDouble() : 1.0
            };

            Keys.Add(vm);
            _keyLookup[vm.KeyId] = vm;
        }

        CanvasWidth = formFactor.GetCanvasWidth() * ScaleFactor;
        CanvasHeight = formFactor.GetCanvasHeight() * ScaleFactor;
    }

    /// <summary>
    /// Handles a key-down event: marks the key as pressed.
    /// </summary>
    public void OnKeyDown(string relicKeyName)
    {
        if (_keyLookup.TryGetValue(relicKeyName, out var keyVm))
        {
            keyVm.IsPressed = true;
        }
    }

    /// <summary>
    /// Handles a key-up event: marks the key as released.
    /// </summary>
    public void OnKeyUp(string relicKeyName)
    {
        if (_keyLookup.TryGetValue(relicKeyName, out var keyVm))
        {
            keyVm.IsPressed = false;
        }
    }

    /// <summary>
    /// Highlights keys that would complete a binding given the current modifiers.
    /// Non-viable keys are dimmed. Uses diff-based updates to minimize property changes.
    /// </summary>
    public void HighlightPossibleKeys(IReadOnlyList<Keybinding> possibleBindings)
    {
        // Build the new highlight set
        var newHighlightedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var binding in possibleBindings)
        {
            ApplyHighlightKey(binding.Primary, binding, newHighlightedKeys);
            ApplyHighlightKey(binding.Alternate, binding, newHighlightedKeys);
        }

        // Clear only keys that were highlighted before but aren't now
        foreach (var keyId in _previouslyHighlightedKeys)
        {
            if (!newHighlightedKeys.Contains(keyId) && _keyLookup.TryGetValue(keyId, out var keyVm))
            {
                keyVm.ClearHighlight();
            }
        }

        // Update dimming — only change keys whose dim state actually differs
        foreach (var key in Keys)
        {
            var shouldDim = !newHighlightedKeys.Contains(key.KeyId);
            if (key.IsDimmed != shouldDim)
                key.IsDimmed = shouldDim;
        }

        // Swap the tracking set
        _previouslyHighlightedKeys.Clear();
        foreach (var id in newHighlightedKeys)
            _previouslyHighlightedKeys.Add(id);
    }

    /// <summary>
    /// Clears all binding highlights and dimming from keys.
    /// Only touches keys that actually have state to clear.
    /// </summary>
    public void ClearHighlights()
    {
        foreach (var keyId in _previouslyHighlightedKeys)
        {
            if (_keyLookup.TryGetValue(keyId, out var keyVm))
            {
                keyVm.ClearHighlight();
            }
        }
        _previouslyHighlightedKeys.Clear();

        foreach (var key in Keys)
        {
            if (key.IsDimmed)
                key.IsDimmed = false;
        }
    }

    /// <summary>
    /// Applies heatmap data to the keyboard visualization.
    /// </summary>
    public void ApplyHeatmapData(IReadOnlyDictionary<string, double> heatData)
    {
        foreach (var key in Keys)
        {
            var newIntensity = heatData.TryGetValue(key.KeyId, out var intensity) ? intensity : 0.0;
            if (Math.Abs(key.HeatmapIntensity - newIntensity) > 0.001)
                key.HeatmapIntensity = newIntensity;
        }
    }

    /// <summary>
    /// Clears all heatmap data from keys.
    /// </summary>
    public void ClearHeatmap()
    {
        foreach (var key in Keys)
        {
            if (key.HeatmapIntensity > 0.0)
                key.HeatmapIntensity = 0.0;
        }
    }

    private void ApplyHighlightKey(KeyCombination combo, Keybinding binding, HashSet<string> highlightedKeys)
    {
        if (combo.IsEmpty) return;

        if (_keyLookup.TryGetValue(combo.PrimaryKey, out var keyVm))
        {
            keyVm.ApplyHighlight(binding.Category.DefaultColor(), binding.DisplayName, binding.Category);
            highlightedKeys.Add(combo.PrimaryKey);
        }
    }
}

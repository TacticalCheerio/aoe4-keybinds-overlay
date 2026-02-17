using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using AoE4KeybindsOverlay.Models;

namespace AoE4KeybindsOverlay.ViewModels;

/// <summary>
/// ViewModel managing the visual state of the mouse button display.
/// </summary>
public partial class MouseViewModel : ObservableObject
{
    private readonly Dictionary<string, KeyViewModel> _buttonLookup = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>One KeyViewModel per mouse button.</summary>
    public ObservableCollection<KeyViewModel> Buttons { get; } = [];

    /// <summary>
    /// Loads the mouse layout from the embedded JSON data file.
    /// </summary>
    public void LoadLayout(string jsonPath)
    {
        var json = File.ReadAllText(jsonPath);
        using var doc = JsonDocument.Parse(json);

        var mouse = doc.RootElement.GetProperty("mouse");
        var buttons = mouse.GetProperty("buttons");

        Buttons.Clear();
        _buttonLookup.Clear();

        foreach (var btn in buttons.EnumerateArray())
        {
            var vm = new KeyViewModel
            {
                KeyId = btn.GetProperty("keyId").GetString() ?? "",
                Label = btn.GetProperty("label").GetString() ?? "",
                X = btn.GetProperty("x").GetDouble(),
                Y = btn.GetProperty("y").GetDouble(),
                Width = btn.TryGetProperty("width", out var w) ? w.GetDouble() : 1.0,
                Height = btn.TryGetProperty("height", out var h) ? h.GetDouble() : 1.0
            };

            Buttons.Add(vm);
            _buttonLookup[vm.KeyId] = vm;
        }
    }

    /// <summary>
    /// Handles a mouse button press.
    /// </summary>
    public void OnButtonDown(string relicKeyName)
    {
        if (_buttonLookup.TryGetValue(relicKeyName, out var vm))
            vm.IsPressed = true;
    }

    /// <summary>
    /// Handles a mouse button release.
    /// </summary>
    public void OnButtonUp(string relicKeyName)
    {
        if (_buttonLookup.TryGetValue(relicKeyName, out var vm))
            vm.IsPressed = false;
    }

    /// <summary>
    /// Highlights mouse buttons that would complete a binding.
    /// </summary>
    public void HighlightPossibleButtons(IReadOnlyList<Keybinding> possibleBindings)
    {
        ClearHighlights();

        foreach (var binding in possibleBindings)
        {
            TryHighlight(binding.Primary, binding);
            TryHighlight(binding.Alternate, binding);
        }
    }

    /// <summary>
    /// Clears all highlights from mouse buttons.
    /// </summary>
    public void ClearHighlights()
    {
        foreach (var btn in Buttons)
        {
            btn.HighlightColor = null;
            btn.BindingLabel = null;
            btn.HasBinding = false;
            btn.ActiveCategory = null;
        }
    }

    private void TryHighlight(KeyCombination combo, Keybinding binding)
    {
        if (combo.IsEmpty) return;

        if (_buttonLookup.TryGetValue(combo.PrimaryKey, out var vm))
        {
            vm.HighlightColor = binding.Category.DefaultColor();
            vm.BindingLabel = binding.DisplayName;
            vm.HasBinding = true;
            vm.ActiveCategory = binding.Category;
        }
    }
}

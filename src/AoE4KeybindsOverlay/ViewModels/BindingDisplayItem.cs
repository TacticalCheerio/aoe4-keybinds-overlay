using CommunityToolkit.Mvvm.ComponentModel;
using AoE4KeybindsOverlay.Models;

namespace AoE4KeybindsOverlay.ViewModels;

/// <summary>
/// Display wrapper for a keybinding shown in the active bindings list.
/// </summary>
public partial class BindingDisplayItem : ObservableObject
{
    /// <summary>The underlying keybinding.</summary>
    public required Keybinding Binding { get; init; }

    /// <summary>Whether this binding was just triggered (flash highlight).</summary>
    [ObservableProperty]
    private bool _isTriggered;

    /// <summary>Whether this is a partial match (modifier held, waiting for primary key).</summary>
    [ObservableProperty]
    private bool _isPartialMatch;

    /// <summary>Display string for the key combo.</summary>
    public string KeyComboText => Binding.Primary.DisplayString;

    /// <summary>Display name of the command.</summary>
    public string CommandName => Binding.DisplayName;

    /// <summary>The category for color-coding.</summary>
    public BindingCategory Category => Binding.Category;

    /// <summary>Display name of the category.</summary>
    public string CategoryName => Binding.Category.DisplayName();

    /// <summary>Hex color string for the category.</summary>
    public string CategoryColor => Binding.Category.DefaultColor();
}

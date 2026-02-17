using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using AoE4KeybindsOverlay.Models;

namespace AoE4KeybindsOverlay.ViewModels;

/// <summary>
/// ViewModel showing currently relevant bindings based on pressed modifiers.
/// </summary>
public partial class ActiveBindingsViewModel : ObservableObject
{
    /// <summary>Filtered list of visible bindings that updates dynamically.</summary>
    [ObservableProperty]
    private ObservableCollection<BindingDisplayItem> _visibleBindings = [];

    /// <summary>The currently active modifier keys.</summary>
    [ObservableProperty]
    private ModifierKeys _activeModifiers;

    /// <summary>Optional category filter.</summary>
    [ObservableProperty]
    private BindingCategory? _categoryFilter;

    /// <summary>
    /// Updates the visible bindings list based on the current modifier state.
    /// Replaces the entire collection reference for a single PropertyChanged notification.
    /// </summary>
    public void UpdateForModifiers(ModifierKeys modifiers, IReadOnlyList<Keybinding> possibleBindings)
    {
        ActiveModifiers = modifiers;

        var newList = new ObservableCollection<BindingDisplayItem>();
        foreach (var binding in possibleBindings)
        {
            if (CategoryFilter.HasValue && binding.Category != CategoryFilter.Value)
                continue;

            newList.Add(new BindingDisplayItem
            {
                Binding = binding,
                IsPartialMatch = true
            });
        }
        VisibleBindings = newList;
    }

    /// <summary>
    /// Highlights a triggered binding temporarily.
    /// </summary>
    public void HighlightTriggered(Keybinding binding)
    {
        foreach (var item in VisibleBindings)
        {
            if (item.Binding.CommandId == binding.CommandId &&
                item.Binding.GroupName == binding.GroupName)
            {
                item.IsTriggered = true;
                item.IsPartialMatch = false;
            }
        }
    }

    /// <summary>
    /// Shows the single-key (no-modifier) bindings for the idle state.
    /// Replaces the entire collection reference for a single PropertyChanged notification.
    /// </summary>
    public void ShowIdleBindings(IReadOnlyList<Keybinding> noModifierBindings)
    {
        ActiveModifiers = ModifierKeys.None;

        var newList = new ObservableCollection<BindingDisplayItem>();
        foreach (var binding in noModifierBindings)
        {
            if (CategoryFilter.HasValue && binding.Category != CategoryFilter.Value)
                continue;

            newList.Add(new BindingDisplayItem
            {
                Binding = binding,
                IsPartialMatch = true
            });
        }
        VisibleBindings = newList;
    }

    /// <summary>
    /// Clears all highlights and resets the bindings list.
    /// </summary>
    public void Clear()
    {
        VisibleBindings = [];
        ActiveModifiers = ModifierKeys.None;
    }
}

namespace AoE4KeybindsOverlay.Models;

/// <summary>
/// Represents a group of related keybindings (e.g., all camera bindings, all archery range bindings).
/// </summary>
public sealed class BindingGroup
{
    /// <summary>
    /// Gets the group name from the .rkp file (e.g., "camera", "archery_range").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the category this binding group belongs to.
    /// </summary>
    public required BindingCategory Category { get; init; }

    /// <summary>
    /// Gets the list of keybindings in this group.
    /// </summary>
    public required List<Keybinding> Bindings { get; init; }
}

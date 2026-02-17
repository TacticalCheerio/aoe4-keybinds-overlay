namespace AoE4KeybindsOverlay.Models;

/// <summary>
/// Represents a complete keybinding profile loaded from an .rkp file.
/// </summary>
public sealed class BindingProfile
{
    /// <summary>
    /// Gets the name of this binding profile.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the file path to the .rkp file.
    /// </summary>
    public required string FilePath { get; init; }

    /// <summary>
    /// Gets whether to warn about conflicting keybindings.
    /// </summary>
    public bool WarnConflicts { get; init; } = true;

    /// <summary>
    /// Gets whether to warn about unmapped commands.
    /// </summary>
    public bool WarnUnremapped { get; init; } = false;

    /// <summary>
    /// Gets the list of binding groups in this profile.
    /// </summary>
    public required List<BindingGroup> BindingGroups { get; init; }

    /// <summary>
    /// Gets all keybindings across all groups.
    /// </summary>
    public IEnumerable<Keybinding> AllBindings => BindingGroups.SelectMany(g => g.Bindings);
}

namespace AoE4KeybindsOverlay.Models;

/// <summary>
/// Represents usage statistics for a specific keybinding command.
/// </summary>
public sealed class KeyUsageStatistic
{
    /// <summary>
    /// Gets the command identifier.
    /// </summary>
    public required string CommandId { get; init; }

    /// <summary>
    /// Gets the binding group name.
    /// </summary>
    public required string GroupName { get; init; }

    /// <summary>
    /// Gets the binding category.
    /// </summary>
    public required BindingCategory Category { get; init; }

    /// <summary>
    /// Gets or sets the total number of times this command was executed.
    /// </summary>
    public int TotalPresses { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this command was first used.
    /// </summary>
    public DateTime FirstUsed { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this command was last used.
    /// </summary>
    public DateTime LastUsed { get; set; }
}

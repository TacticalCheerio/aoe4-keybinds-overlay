namespace AoE4KeybindsOverlay.Models;

/// <summary>
/// Represents a keyboard layout with physical key positions.
/// </summary>
public sealed class KeyboardLayout
{
    /// <summary>
    /// Gets the name of this keyboard layout (e.g., "QWERTY", "AZERTY").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the total width of the keyboard layout in logical units.
    /// </summary>
    public required double TotalWidth { get; init; }

    /// <summary>
    /// Gets the total height of the keyboard layout in logical units.
    /// </summary>
    public required double TotalHeight { get; init; }

    /// <summary>
    /// Gets the list of physical keys in this layout.
    /// </summary>
    public required List<PhysicalKey> Keys { get; init; }
}

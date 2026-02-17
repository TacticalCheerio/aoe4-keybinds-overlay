namespace AoE4KeybindsOverlay.Models;

/// <summary>
/// Represents a physical key on a keyboard with its position and display properties.
/// </summary>
public sealed class PhysicalKey
{
    /// <summary>
    /// Gets the Relic key identifier (e.g., "A", "LBracket", "F1").
    /// </summary>
    public required string KeyId { get; init; }

    /// <summary>
    /// Gets the display label for this key (e.g., "[", "F1", "A").
    /// </summary>
    public required string Label { get; init; }

    /// <summary>
    /// Gets the X coordinate of the key's top-left corner in logical units.
    /// </summary>
    public required double X { get; init; }

    /// <summary>
    /// Gets the Y coordinate of the key's top-left corner in logical units.
    /// </summary>
    public required double Y { get; init; }

    /// <summary>
    /// Gets the width of the key in logical units (default is 1.0).
    /// </summary>
    public double Width { get; init; } = 1.0;

    /// <summary>
    /// Gets the height of the key in logical units (default is 1.0).
    /// </summary>
    public double Height { get; init; } = 1.0;
}

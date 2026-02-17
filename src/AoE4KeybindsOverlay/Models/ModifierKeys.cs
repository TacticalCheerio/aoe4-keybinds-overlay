namespace AoE4KeybindsOverlay.Models;

/// <summary>
/// Represents keyboard modifier keys that can be combined with primary keys.
/// </summary>
[Flags]
public enum ModifierKeys
{
    /// <summary>
    /// No modifier keys pressed.
    /// </summary>
    None = 0,

    /// <summary>
    /// Control key modifier.
    /// </summary>
    Ctrl = 1,

    /// <summary>
    /// Shift key modifier.
    /// </summary>
    Shift = 2,

    /// <summary>
    /// Alt key modifier.
    /// </summary>
    Alt = 4
}

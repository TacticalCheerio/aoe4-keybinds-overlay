using AoE4KeybindsOverlay.Models;

namespace AoE4KeybindsOverlay.Services.InputHook;

/// <summary>
/// Event arguments for keyboard hook events, carrying the Relic key name,
/// Win32 virtual key code, and the currently active modifier keys.
/// </summary>
public sealed class KeyboardHookEventArgs : EventArgs
{
    /// <summary>
    /// Gets the key name mapped to Relic naming convention (e.g., "A", "F1", "LBracket").
    /// </summary>
    public required string RelicKeyName { get; init; }

    /// <summary>
    /// Gets the Win32 virtual key code for the pressed key.
    /// </summary>
    public required int VirtualKeyCode { get; init; }

    /// <summary>
    /// Gets the modifier keys that are active at the time of this event.
    /// </summary>
    public required ModifierKeys ActiveModifiers { get; init; }
}

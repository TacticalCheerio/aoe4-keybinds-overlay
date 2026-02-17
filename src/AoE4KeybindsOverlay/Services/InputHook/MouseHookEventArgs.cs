using AoE4KeybindsOverlay.Models;

namespace AoE4KeybindsOverlay.Services.InputHook;

/// <summary>
/// Event arguments for mouse hook events, carrying the Relic mouse button name
/// and the currently active modifier keys.
/// </summary>
public sealed class MouseHookEventArgs : EventArgs
{
    /// <summary>
    /// Gets the Relic-convention mouse button name (e.g., "MouseLeft", "MouseRight",
    /// "MouseMiddle", "MouseX1", "MouseX2", "MouseWheelUp", "MouseWheelDown").
    /// </summary>
    public required string RelicKeyName { get; init; }

    /// <summary>
    /// Gets the modifier keys that are active at the time of this event.
    /// </summary>
    public required ModifierKeys ActiveModifiers { get; init; }
}

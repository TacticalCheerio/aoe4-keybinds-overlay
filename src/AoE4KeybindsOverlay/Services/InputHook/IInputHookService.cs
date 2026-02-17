using AoE4KeybindsOverlay.Models;

namespace AoE4KeybindsOverlay.Services.InputHook;

/// <summary>
/// Provides global input hooking for keyboard and mouse events.
/// Implementations capture input system-wide (not just when the application is focused)
/// so the overlay can react while Age of Empires IV is in the foreground.
/// </summary>
public interface IInputHookService : IDisposable
{
    /// <summary>
    /// Raised when a key is pressed down.
    /// </summary>
    event EventHandler<KeyboardHookEventArgs>? KeyDown;

    /// <summary>
    /// Raised when a key is released.
    /// </summary>
    event EventHandler<KeyboardHookEventArgs>? KeyUp;

    /// <summary>
    /// Raised when a mouse button is pressed down.
    /// </summary>
    event EventHandler<MouseHookEventArgs>? MouseDown;

    /// <summary>
    /// Raised when a mouse button is released.
    /// </summary>
    event EventHandler<MouseHookEventArgs>? MouseUp;

    /// <summary>
    /// Gets the currently held modifier keys (Ctrl, Shift, Alt).
    /// </summary>
    ModifierKeys CurrentModifiers { get; }

    /// <summary>
    /// Gets whether the input hooks are currently installed and active.
    /// </summary>
    bool IsHookActive { get; }

    /// <summary>
    /// Installs the global keyboard and mouse hooks.
    /// Must be called from a thread with a message pump (e.g., WPF UI thread).
    /// </summary>
    void Start();

    /// <summary>
    /// Removes the global keyboard and mouse hooks and releases resources.
    /// </summary>
    void Stop();
}

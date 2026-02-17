namespace AoE4KeybindsOverlay.Services.Overlay;

/// <summary>
/// Service for managing overlay window behavior (topmost enforcement, click-through toggling).
/// </summary>
public interface IOverlayWindowService
{
    /// <summary>Sets whether the overlay passes mouse clicks through to windows beneath it.</summary>
    void SetClickThrough(bool enable);

    /// <summary>Starts periodic topmost enforcement to keep overlay above other windows.</summary>
    void StartTopmostEnforcement();

    /// <summary>Stops topmost enforcement.</summary>
    void StopTopmostEnforcement();

    /// <summary>Sets the window handle for the overlay.</summary>
    void SetWindowHandle(IntPtr hwnd);
}

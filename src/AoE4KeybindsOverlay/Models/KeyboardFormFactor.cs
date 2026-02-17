namespace AoE4KeybindsOverlay.Models;

/// <summary>
/// Keyboard physical form factor presets.
/// </summary>
public enum KeyboardFormFactor
{
    Full,
    TKL,
    Percent75,
    Percent65,
    Percent60
}

/// <summary>
/// Extension methods for <see cref="KeyboardFormFactor"/>.
/// </summary>
public static class KeyboardFormFactorExtensions
{
    private static readonly HashSet<string> NumpadKeys =
    [
        "NumLock", "NumpadDivide", "NumpadMultiply", "NumpadMinus",
        "Numpad7", "Numpad8", "Numpad9", "NumpadPlus",
        "Numpad4", "Numpad5", "Numpad6",
        "Numpad1", "Numpad2", "Numpad3", "NumpadEnter",
        "Numpad0", "NumpadDecimal"
    ];

    private static readonly HashSet<string> NavClusterKeys =
    [
        "Insert", "Home", "PageUp",
        "Delete", "End", "PageDown"
    ];

    private static readonly HashSet<string> FRowKeys =
    [
        "Escape", "F1", "F2", "F3", "F4", "F5", "F6",
        "F7", "F8", "F9", "F10", "F11", "F12",
        "PrintScreen", "ScrollLock", "Pause"
    ];

    private static readonly HashSet<string> ArrowKeys =
    [
        "Up", "Down", "Left", "Right"
    ];

    /// <summary>
    /// Gets the set of keyIds to exclude for a given form factor.
    /// </summary>
    public static HashSet<string> GetExcludedKeyIds(this KeyboardFormFactor formFactor)
    {
        var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (formFactor >= KeyboardFormFactor.TKL)
        {
            excluded.UnionWith(NumpadKeys);
        }

        if (formFactor >= KeyboardFormFactor.Percent75)
        {
            excluded.UnionWith(NavClusterKeys);
        }

        if (formFactor >= KeyboardFormFactor.Percent65)
        {
            excluded.UnionWith(FRowKeys);
        }

        if (formFactor >= KeyboardFormFactor.Percent60)
        {
            excluded.UnionWith(ArrowKeys);
        }

        return excluded;
    }

    /// <summary>
    /// Gets the canvas width in logical key units for a form factor.
    /// </summary>
    public static double GetCanvasWidth(this KeyboardFormFactor formFactor) => formFactor switch
    {
        KeyboardFormFactor.Full => 22.5,
        KeyboardFormFactor.TKL => 18.25,
        KeyboardFormFactor.Percent75 => 18.25,
        KeyboardFormFactor.Percent65 => 18.25,
        KeyboardFormFactor.Percent60 => 15.0,
        _ => 22.5
    };

    /// <summary>
    /// Gets the canvas height in logical key units for a form factor.
    /// </summary>
    public static double GetCanvasHeight(this KeyboardFormFactor formFactor) => formFactor switch
    {
        KeyboardFormFactor.Full => 6.5,
        KeyboardFormFactor.TKL => 6.5,
        KeyboardFormFactor.Percent75 => 6.5,
        KeyboardFormFactor.Percent65 => 5.25,
        KeyboardFormFactor.Percent60 => 5.25,
        _ => 6.5
    };

    /// <summary>
    /// Whether the F-row is excluded (requiring Y coordinate shifting).
    /// </summary>
    public static bool ExcludesFRow(this KeyboardFormFactor formFactor) =>
        formFactor >= KeyboardFormFactor.Percent65;

    /// <summary>
    /// Gets the user-friendly display name.
    /// </summary>
    public static string DisplayName(this KeyboardFormFactor formFactor) => formFactor switch
    {
        KeyboardFormFactor.Full => "Full",
        KeyboardFormFactor.TKL => "TKL",
        KeyboardFormFactor.Percent75 => "75%",
        KeyboardFormFactor.Percent65 => "65%",
        KeyboardFormFactor.Percent60 => "60%",
        _ => formFactor.ToString()
    };

    /// <summary>
    /// Parses a display name or enum name back to a <see cref="KeyboardFormFactor"/>.
    /// </summary>
    public static KeyboardFormFactor ParseFormFactor(string name) => name switch
    {
        "Full" => KeyboardFormFactor.Full,
        "TKL" => KeyboardFormFactor.TKL,
        "75%" or "Percent75" => KeyboardFormFactor.Percent75,
        "65%" or "Percent65" => KeyboardFormFactor.Percent65,
        "60%" or "Percent60" => KeyboardFormFactor.Percent60,
        _ => KeyboardFormFactor.Full
    };
}

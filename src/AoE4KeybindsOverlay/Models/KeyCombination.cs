namespace AoE4KeybindsOverlay.Models;

/// <summary>
/// Represents a key combination consisting of a primary key and optional modifier keys.
/// </summary>
public sealed record KeyCombination
{
    /// <summary>
    /// Gets the primary key name (e.g., "A", "F1", "MouseWheelUp", "LBracket").
    /// </summary>
    public required string PrimaryKey { get; init; }

    /// <summary>
    /// Gets the modifier keys (Ctrl, Shift, Alt) for this combination.
    /// </summary>
    public required ModifierKeys Modifiers { get; init; }

    /// <summary>
    /// Gets whether this key combination is empty (unbound).
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(PrimaryKey);

    /// <summary>
    /// Gets the formatted display string for this key combination (e.g., "Ctrl + A").
    /// </summary>
    public string DisplayString
    {
        get
        {
            if (IsEmpty)
                return string.Empty;

            var parts = new List<string>();

            if (Modifiers.HasFlag(ModifierKeys.Ctrl))
                parts.Add("Ctrl");
            if (Modifiers.HasFlag(ModifierKeys.Shift))
                parts.Add("Shift");
            if (Modifiers.HasFlag(ModifierKeys.Alt))
                parts.Add("Alt");

            parts.Add(FormatKeyName(PrimaryKey));

            return string.Join(" + ", parts);
        }
    }

    /// <summary>
    /// Parses a Relic key combination string (e.g., "Control+Shift+A") into a KeyCombination.
    /// </summary>
    /// <param name="relicComboString">The Relic format key combination string.</param>
    /// <returns>A parsed KeyCombination instance.</returns>
    public static KeyCombination Parse(string relicComboString)
    {
        if (string.IsNullOrWhiteSpace(relicComboString))
        {
            return new KeyCombination
            {
                PrimaryKey = string.Empty,
                Modifiers = ModifierKeys.None
            };
        }

        var parts = relicComboString.Split('+', StringSplitOptions.RemoveEmptyEntries);
        var modifiers = ModifierKeys.None;
        var primaryKey = string.Empty;

        foreach (var part in parts)
        {
            var trimmed = part.Trim();

            if (trimmed.Equals("Control", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= ModifierKeys.Ctrl;
            }
            else if (trimmed.Equals("Shift", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= ModifierKeys.Shift;
            }
            else if (trimmed.Equals("Alt", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= ModifierKeys.Alt;
            }
            else
            {
                // This is the primary key
                primaryKey = trimmed;
            }
        }

        return new KeyCombination
        {
            PrimaryKey = primaryKey,
            Modifiers = modifiers
        };
    }

    /// <summary>
    /// Checks if this key combination exactly matches the given key and active modifiers.
    /// </summary>
    /// <param name="key">The primary key to match.</param>
    /// <param name="activeModifiers">The currently active modifier keys.</param>
    /// <returns>True if the combination matches exactly.</returns>
    public bool Matches(string key, ModifierKeys activeModifiers)
    {
        if (IsEmpty)
            return false;

        return PrimaryKey.Equals(key, StringComparison.OrdinalIgnoreCase) &&
               Modifiers == activeModifiers;
    }

    /// <summary>
    /// Checks if the active modifiers could potentially complete this key combination.
    /// </summary>
    /// <param name="activeModifiers">The currently active modifier keys.</param>
    /// <returns>True if the active modifiers match this combination's required modifiers.</returns>
    public bool IsPartialMatch(ModifierKeys activeModifiers)
    {
        if (IsEmpty)
            return false;

        return Modifiers == activeModifiers;
    }

    /// <summary>
    /// Returns the display string representation of this key combination.
    /// </summary>
    public override string ToString() => DisplayString;

    /// <summary>
    /// Formats a Relic key name into a more user-friendly display name.
    /// </summary>
    private static string FormatKeyName(string keyName)
    {
        return keyName switch
        {
            "LBracket" => "[",
            "RBracket" => "]",
            "Semicolon" => ";",
            "Apostrophe" => "'",
            "Comma" => ",",
            "Period" => ".",
            "Slash" => "/",
            "Backslash" => "\\",
            "Minus" => "-",
            "Equals" => "=",
            "Grave" => "`",
            "MouseWheelUp" => "Mouse Wheel Up",
            "MouseWheelDown" => "Mouse Wheel Down",
            "MouseX1" => "Mouse X1",
            "MouseX2" => "Mouse X2",
            "LeftMouseButton" => "LMB",
            "RightMouseButton" => "RMB",
            "MiddleMouseButton" => "MMB",
            _ => keyName
        };
    }
}

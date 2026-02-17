namespace AoE4KeybindsOverlay.Services.InputHook;

/// <summary>
/// Provides bidirectional mapping between Win32 virtual key codes and Relic key names.
/// </summary>
/// <remarks>
/// <para>
/// Relic key names follow the conventions found in .rkp files (e.g., "A", "F1", "LBracket",
/// "MouseWheelUp"). This class maps Win32 VK codes to those names and vice versa.
/// </para>
/// <para>
/// All lookups are O(1) via dictionaries that are initialized once at static construction time.
/// </para>
/// </remarks>
public static class VirtualKeyMap
{
    /// <summary>
    /// Maps Win32 virtual key codes to Relic key names.
    /// </summary>
    private static readonly Dictionary<int, string> VkToRelic = new();

    /// <summary>
    /// Maps Relic key names (case-insensitive) to Win32 virtual key codes.
    /// </summary>
    private static readonly Dictionary<string, int> RelicToVk =
        new(StringComparer.OrdinalIgnoreCase);

    static VirtualKeyMap()
    {
        // Letters: A-Z (0x41 - 0x5A)
        for (int vk = 0x41; vk <= 0x5A; vk++)
        {
            var name = ((char)vk).ToString();
            Register(vk, name);
        }

        // Digits: 0-9 (0x30 - 0x39)
        for (int vk = 0x30; vk <= 0x39; vk++)
        {
            var name = (vk - 0x30).ToString();
            Register(vk, name);
        }

        // Function keys: F1-F12 (0x70 - 0x7B)
        for (int vk = 0x70; vk <= 0x7B; vk++)
        {
            var name = $"F{vk - 0x70 + 1}";
            Register(vk, name);
        }

        // Numpad digits: Numpad0-Numpad9 (0x60 - 0x69)
        for (int vk = 0x60; vk <= 0x69; vk++)
        {
            var name = $"Numpad{vk - 0x60}";
            Register(vk, name);
        }

        // Special keys
        Register(0x1B, "Escape");
        Register(0x09, "Tab");
        Register(0x20, "Space");
        Register(0x0D, "Enter");
        Register(0x08, "Backspace");

        // Bracket and punctuation keys
        Register(0xDB, "LBracket");
        Register(0xDD, "RBracket");
        Register(0xBE, "Period");
        Register(0xBC, "Comma");
        Register(0xBF, "Slash");
        Register(0xBA, "Semicolon");
        Register(0xDE, "Apostrophe");
        Register(0xBD, "Minus");
        Register(0xBB, "Equals");
        Register(0xC0, "Backquote");
        Register(0xDC, "Backslash");

        // Arrow keys
        Register(0x25, "Left");
        Register(0x26, "Up");
        Register(0x27, "Right");
        Register(0x28, "Down");

        // Navigation keys
        Register(0x2D, "Insert");
        Register(0x2E, "Delete");
        Register(0x24, "Home");
        Register(0x23, "End");
        Register(0x21, "PageUp");
        Register(0x22, "PageDown");

        // Toggle and special keys
        Register(0x14, "CapsLock");
        Register(0x13, "Pause");

        // Modifier keys (left/right specific VK codes from low-level hooks)
        Register(0xA0, "LShift");
        Register(0xA1, "RShift");
        Register(0xA2, "LControl");
        Register(0xA3, "RControl");
        Register(0xA4, "LMenu");   // Left Alt
        Register(0xA5, "RMenu");   // Right Alt

        // Numpad operators
        Register(0x6A, "NumpadMultiply");
        Register(0x6B, "NumpadPlus");
        Register(0x6D, "NumpadMinus");
        Register(0x6E, "NumpadDecimal");
        Register(0x6F, "NumpadDivide");
    }

    /// <summary>
    /// Registers a bidirectional mapping between a virtual key code and a Relic key name.
    /// </summary>
    private static void Register(int vk, string relicName)
    {
        VkToRelic[vk] = relicName;
        RelicToVk[relicName] = vk;
    }

    /// <summary>
    /// Converts a Win32 virtual key code to a Relic key name.
    /// </summary>
    /// <param name="virtualKeyCode">The Win32 virtual key code.</param>
    /// <returns>The Relic key name, or null if the key code is not mapped.</returns>
    public static string? ToRelicName(int virtualKeyCode)
    {
        return VkToRelic.TryGetValue(virtualKeyCode, out var name) ? name : null;
    }

    /// <summary>
    /// Converts a Relic key name to a Win32 virtual key code.
    /// </summary>
    /// <param name="relicKeyName">The Relic key name (case-insensitive).</param>
    /// <returns>The Win32 virtual key code, or null if the key name is not mapped.</returns>
    public static int? ToVirtualKeyCode(string relicKeyName)
    {
        return RelicToVk.TryGetValue(relicKeyName, out var vk) ? vk : null;
    }

    /// <summary>
    /// Checks whether a Win32 virtual key code has a known Relic mapping.
    /// </summary>
    /// <param name="virtualKeyCode">The Win32 virtual key code.</param>
    /// <returns>True if the key code is mapped.</returns>
    public static bool IsKnownVirtualKey(int virtualKeyCode) => VkToRelic.ContainsKey(virtualKeyCode);

    /// <summary>
    /// Checks whether a Relic key name has a known virtual key code mapping.
    /// </summary>
    /// <param name="relicKeyName">The Relic key name (case-insensitive).</param>
    /// <returns>True if the key name is mapped.</returns>
    public static bool IsKnownRelicName(string relicKeyName) => RelicToVk.ContainsKey(relicKeyName);
}

namespace AoE4KeybindsOverlay.Models;

/// <summary>
/// Persisted user preferences, saved to %APPDATA% as JSON between app restarts.
/// </summary>
public sealed class UserSettings
{
    // Visual settings
    public double BackgroundOpacity { get; set; } = 0.6;
    public double ContentOpacity { get; set; } = 1.0;
    public double KeyFontSize { get; set; } = 11.0;
    public double BindingListFontSize { get; set; } = 11.0;
    public double LegendFontSize { get; set; } = 10.0;

    // Panel visibility
    public bool ShowBindingList { get; set; } = true;
    public bool ShowMouse { get; set; } = true;

    // Profile
    public string? SelectedProfileName { get; set; }

    // Keyboard layout
    public string KeyboardFormFactor { get; set; } = "Full";

    // Window position and size
    public double? WindowLeft { get; set; }
    public double? WindowTop { get; set; }
    public double? WindowWidth { get; set; }
    public double? WindowHeight { get; set; }
    public bool IsLocked { get; set; }
}

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AoE4KeybindsOverlay.Models;

namespace AoE4KeybindsOverlay.ViewModels;

/// <summary>
/// ViewModel for application settings.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    /// <summary>Available profile names.</summary>
    public ObservableCollection<string> AvailableProfiles { get; } = [];

    /// <summary>Currently selected profile name.</summary>
    [ObservableProperty]
    private string? _selectedProfile;

    /// <summary>Background opacity (0.0 = transparent, 1.0 = solid).</summary>
    [ObservableProperty]
    private double _backgroundOpacity = 0.6;

    /// <summary>Content opacity for keyboard, legend, and binding list (0.1 to 1.0).</summary>
    [ObservableProperty]
    private double _contentOpacity = 1.0;

    /// <summary>Whether the overlay is currently visible.</summary>
    [ObservableProperty]
    private bool _isOverlayVisible = true;

    /// <summary>Whether the overlay is click-through.</summary>
    [ObservableProperty]
    private bool _isClickThrough = true;

    /// <summary>Whether to show the binding list panel.</summary>
    [ObservableProperty]
    private bool _showBindingList = true;

    /// <summary>Whether to show the mouse control.</summary>
    [ObservableProperty]
    private bool _showMouse = true;

    /// <summary>The directory path for AoE4 keybinding profiles.</summary>
    [ObservableProperty]
    private string _profilesDirectory = string.Empty;

    /// <summary>Scale factor for the overlay (0.5 to 2.0).</summary>
    [ObservableProperty]
    private double _overlayScale = 1.0;

    /// <summary>Font size for keyboard key labels (6-24).</summary>
    [ObservableProperty]
    private double _keyFontSize = 11.0;

    /// <summary>Font size for binding list items (8-28).</summary>
    [ObservableProperty]
    private double _bindingListFontSize = 11.0;

    /// <summary>Font size for the category legend (6-20).</summary>
    [ObservableProperty]
    private double _legendFontSize = 10.0;

    /// <summary>Background color hex string for the overlay key backgrounds.</summary>
    [ObservableProperty]
    private string _keyBackgroundColor = "#AA1A1A2E";

    /// <summary>Pressed key highlight color hex string.</summary>
    [ObservableProperty]
    private string _pressedKeyColor = "#BBDAA520";

    /// <summary>Selected keyboard form factor preset.</summary>
    [ObservableProperty]
    private KeyboardFormFactor _keyboardFormFactor = KeyboardFormFactor.Full;

    /// <summary>Available form factor options for the UI dropdown.</summary>
    public IReadOnlyList<KeyboardFormFactor> AvailableFormFactors { get; } =
        Enum.GetValues<KeyboardFormFactor>().ToList();

    partial void OnKeyboardFormFactorChanged(KeyboardFormFactor value)
    {
        KeyboardFormFactorChanged?.Invoke(this, value);
    }

    /// <summary>Fired when the user selects a different keyboard form factor.</summary>
    public event EventHandler<KeyboardFormFactor>? KeyboardFormFactorChanged;

    partial void OnSelectedProfileChanged(string? value)
    {
        if (value is not null)
        {
            ProfileChanged?.Invoke(this, value);
        }
    }

    /// <summary>Fired when the user selects a different profile.</summary>
    public event EventHandler<string>? ProfileChanged;

    /// <summary>
    /// Creates a <see cref="UserSettings"/> snapshot of current preferences for persistence.
    /// </summary>
    public UserSettings ToUserSettings()
    {
        return new UserSettings
        {
            BackgroundOpacity = BackgroundOpacity,
            ContentOpacity = ContentOpacity,
            KeyFontSize = KeyFontSize,
            BindingListFontSize = BindingListFontSize,
            LegendFontSize = LegendFontSize,
            ShowBindingList = ShowBindingList,
            ShowMouse = ShowMouse,
            SelectedProfileName = SelectedProfile,
            KeyboardFormFactor = KeyboardFormFactor.DisplayName(),
        };
    }

    /// <summary>
    /// Applies persisted settings from a <see cref="UserSettings"/> object.
    /// Does not fire <see cref="ProfileChanged"/> — the caller should handle profile loading.
    /// </summary>
    public void ApplyUserSettings(UserSettings settings)
    {
        BackgroundOpacity = settings.BackgroundOpacity;
        ContentOpacity = settings.ContentOpacity;
        KeyFontSize = settings.KeyFontSize;
        BindingListFontSize = settings.BindingListFontSize;
        LegendFontSize = settings.LegendFontSize;
        ShowBindingList = settings.ShowBindingList;
        ShowMouse = settings.ShowMouse;
        KeyboardFormFactor = KeyboardFormFactorExtensions.ParseFormFactor(settings.KeyboardFormFactor);
        // SelectedProfile is set separately after profiles are loaded,
        // so we don't set it here to avoid triggering ProfileChanged before profiles are available.
    }
}

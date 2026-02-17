using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AoE4KeybindsOverlay.Models;
using AoE4KeybindsOverlay.Services.InputHook;
using AoE4KeybindsOverlay.Services.Keybinding;
using AoE4KeybindsOverlay.Services.Statistics;

namespace AoE4KeybindsOverlay.ViewModels;

/// <summary>
/// Simple data item for the category legend display.
/// </summary>
public record CategoryLegendItem(BindingCategory Category, string DisplayName);

/// <summary>
/// Display state for the overlay's binding display.
/// </summary>
public enum OverlayDisplayState
{
    /// <summary>No modifiers held. Show default state.</summary>
    Idle,
    /// <summary>Modifier keys held. Show possible completions.</summary>
    ModifierHeld,
    /// <summary>A binding was just triggered. Flash highlight.</summary>
    Triggered
}

/// <summary>
/// Operating mode for the overlay window.
/// </summary>
public enum OverlayMode
{
    /// <summary>Normal overlay showing keybindings.</summary>
    Live,
    /// <summary>Heatmap/statistics view.</summary>
    Statistics,
    /// <summary>Key remapping mode (click-through disabled).</summary>
    Remapping
}

/// <summary>
/// Root ViewModel that orchestrates child ViewModels and coordinates between
/// input events and UI state via a display state machine.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IKeybindingService _keybindingService;
    private readonly IInputHookService _inputHookService;
    private readonly IStatisticsService _statisticsService;
    private readonly DispatcherTimer _triggerFlashTimer;
    private string? _layoutJsonPath;

    /// <summary>Keyboard visual state.</summary>
    public KeyboardViewModel Keyboard { get; }

    /// <summary>Mouse visual state.</summary>
    public MouseViewModel Mouse { get; }

    /// <summary>Active bindings list display.</summary>
    public ActiveBindingsViewModel ActiveBindings { get; }

    /// <summary>Statistics and heatmap display.</summary>
    public StatisticsViewModel Statistics { get; }

    /// <summary>Application settings.</summary>
    public SettingsViewModel Settings { get; }

    /// <summary>Legend items for the category color legend.</summary>
    public IReadOnlyList<CategoryLegendItem> CategoryLegendItems { get; } = Enum.GetValues<BindingCategory>()
        .Where(c => c != BindingCategory.Unknown)
        .Select(c => new CategoryLegendItem(c, c.DisplayName()))
        .ToList();

    /// <summary>Current display state of the overlay.</summary>
    [ObservableProperty]
    private OverlayDisplayState _displayState = OverlayDisplayState.Idle;

    /// <summary>Current operating mode.</summary>
    [ObservableProperty]
    private OverlayMode _currentMode = OverlayMode.Live;

    /// <summary>Whether the overlay is in live keybinding mode.</summary>
    public bool IsLiveMode => CurrentMode == OverlayMode.Live;

    /// <summary>Whether the overlay is in statistics mode.</summary>
    public bool IsStatisticsMode => CurrentMode == OverlayMode.Statistics;

    partial void OnCurrentModeChanged(OverlayMode value)
    {
        OnPropertyChanged(nameof(IsLiveMode));
        OnPropertyChanged(nameof(IsStatisticsMode));
    }

    /// <summary>Whether the overlay is visible.</summary>
    [ObservableProperty]
    private bool _isOverlayVisible = true;

    /// <summary>Name of the currently loaded profile.</summary>
    [ObservableProperty]
    private string _activeProfileName = "None";

    public MainViewModel(
        IKeybindingService keybindingService,
        IInputHookService inputHookService,
        IStatisticsService statisticsService,
        KeyboardViewModel keyboard,
        MouseViewModel mouse,
        ActiveBindingsViewModel activeBindings,
        StatisticsViewModel statistics,
        SettingsViewModel settings)
    {
        _keybindingService = keybindingService;
        _inputHookService = inputHookService;
        _statisticsService = statisticsService;

        Keyboard = keyboard;
        Mouse = mouse;
        ActiveBindings = activeBindings;
        Statistics = statistics;
        Settings = settings;

        _triggerFlashTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(600)
        };
        _triggerFlashTimer.Tick += OnTriggerFlashTimerTick;

        // Wire up input events
        _inputHookService.KeyDown += OnKeyDown;
        _inputHookService.KeyUp += OnKeyUp;
        _inputHookService.MouseDown += OnMouseDown;
        _inputHookService.MouseUp += OnMouseUp;

        // Wire up settings changes
        Settings.ProfileChanged += OnProfileChanged;
        Settings.KeyboardFormFactorChanged += OnKeyboardFormFactorChanged;
    }

    /// <summary>
    /// Initializes the ViewModel after DI construction.
    /// </summary>
    public void Initialize(string layoutJsonPath, KeyboardFormFactor formFactor = KeyboardFormFactor.Full)
    {
        _layoutJsonPath = layoutJsonPath;
        Keyboard.LoadLayout(layoutJsonPath, formFactor);
        Mouse.LoadLayout(layoutJsonPath);

        if (_keybindingService.ActiveProfile is not null)
        {
            ActiveProfileName = _keybindingService.ActiveProfile.Name;
        }

        // Show single-key bindings in the idle state
        ShowIdleBindings();
    }

    /// <summary>
    /// Populates the binding list with all single-key (no-modifier) bindings.
    /// Called when entering Idle state so users can always see what each key does.
    /// </summary>
    private void ShowIdleBindings()
    {
        var noModifierBindings = _keybindingService.GetPossibleBindings(ModifierKeys.None);
        ActiveBindings.ShowIdleBindings(noModifierBindings);
        Keyboard.HighlightPossibleKeys(noModifierBindings);
        Mouse.HighlightPossibleButtons(noModifierBindings);
    }

    private void OnKeyDown(object? sender, KeyboardHookEventArgs e)
    {
        if (CurrentMode != OverlayMode.Live) return;

        var relicKey = e.RelicKeyName;
        var modifiers = _inputHookService.CurrentModifiers;

        // Update keyboard visual
        Keyboard.OnKeyDown(relicKey);

        // Check for exact binding match
        var matchedBinding = _keybindingService.FindExactMatch(relicKey, modifiers);

        if (matchedBinding is not null)
        {
            // STATE -> Triggered
            DisplayState = OverlayDisplayState.Triggered;
            ActiveBindings.HighlightTriggered(matchedBinding);
            _statisticsService.RecordKeyPress(relicKey, modifiers, matchedBinding);

            _triggerFlashTimer.Stop();
            _triggerFlashTimer.Start();
        }
        else if (IsModifierKey(relicKey))
        {
            // STATE -> ModifierHeld
            DisplayState = OverlayDisplayState.ModifierHeld;
            var possibleBindings = _keybindingService.GetPossibleBindings(modifiers);
            ActiveBindings.UpdateForModifiers(modifiers, possibleBindings);
            Keyboard.HighlightPossibleKeys(possibleBindings);
            Mouse.HighlightPossibleButtons(possibleBindings);
        }
        else
        {
            // Non-modifier, non-binding press
            _statisticsService.RecordKeyPress(relicKey, modifiers, null);
        }
    }

    private void OnKeyUp(object? sender, KeyboardHookEventArgs e)
    {
        if (CurrentMode != OverlayMode.Live) return;

        Keyboard.OnKeyUp(e.RelicKeyName);

        var modifiers = _inputHookService.CurrentModifiers;

        if (modifiers == ModifierKeys.None && DisplayState != OverlayDisplayState.Triggered)
        {
            DisplayState = OverlayDisplayState.Idle;
            ShowIdleBindings();
        }
        else if (DisplayState == OverlayDisplayState.ModifierHeld)
        {
            // Modifier set changed; recalculate
            var possibleBindings = _keybindingService.GetPossibleBindings(modifiers);
            ActiveBindings.UpdateForModifiers(modifiers, possibleBindings);
            Keyboard.HighlightPossibleKeys(possibleBindings);
            Mouse.HighlightPossibleButtons(possibleBindings);
        }
    }

    private void OnMouseDown(object? sender, MouseHookEventArgs e)
    {
        if (CurrentMode != OverlayMode.Live) return;

        Mouse.OnButtonDown(e.RelicKeyName);

        var modifiers = _inputHookService.CurrentModifiers;
        var matchedBinding = _keybindingService.FindExactMatch(e.RelicKeyName, modifiers);

        if (matchedBinding is not null)
        {
            DisplayState = OverlayDisplayState.Triggered;
            ActiveBindings.HighlightTriggered(matchedBinding);
            _statisticsService.RecordKeyPress(e.RelicKeyName, modifiers, matchedBinding);

            _triggerFlashTimer.Stop();
            _triggerFlashTimer.Start();
        }
    }

    private void OnMouseUp(object? sender, MouseHookEventArgs e)
    {
        if (CurrentMode != OverlayMode.Live) return;
        Mouse.OnButtonUp(e.RelicKeyName);
    }

    private void OnTriggerFlashTimerTick(object? sender, EventArgs e)
    {
        _triggerFlashTimer.Stop();

        var modifiers = _inputHookService.CurrentModifiers;
        if (modifiers == ModifierKeys.None)
        {
            DisplayState = OverlayDisplayState.Idle;
            ShowIdleBindings();
        }
        else
        {
            DisplayState = OverlayDisplayState.ModifierHeld;
            var possibleBindings = _keybindingService.GetPossibleBindings(modifiers);
            ActiveBindings.UpdateForModifiers(modifiers, possibleBindings);
            Keyboard.HighlightPossibleKeys(possibleBindings);
            Mouse.HighlightPossibleButtons(possibleBindings);
        }
    }

    private void OnKeyboardFormFactorChanged(object? sender, KeyboardFormFactor formFactor)
    {
        if (_layoutJsonPath is null) return;

        Keyboard.LoadLayout(_layoutJsonPath, formFactor);
        ShowIdleBindings();
    }

    private void OnProfileChanged(object? sender, string profileName)
    {
        // Load the selected profile from disk
        var profilePath = System.IO.Path.Combine(Settings.ProfilesDirectory, profileName + ".rkp");
        if (System.IO.File.Exists(profilePath))
        {
            _keybindingService.LoadProfile(profilePath);
            _statisticsService.SetBindings(_keybindingService.GetAllBindings());
        }

        ActiveProfileName = profileName;
        // Refresh idle bindings for the new profile
        ShowIdleBindings();
    }

    [RelayCommand]
    private void ToggleOverlay()
    {
        IsOverlayVisible = !IsOverlayVisible;
    }

    [RelayCommand]
    private void SwitchMode(OverlayMode mode)
    {
        CurrentMode = mode;

        if (mode == OverlayMode.Statistics)
        {
            RefreshStatistics();
        }
        else if (mode == OverlayMode.Live)
        {
            Keyboard.ClearHeatmap();
            Keyboard.ClearHighlights();
            ShowIdleBindings();
        }
    }

    private void RefreshStatistics()
    {
        var mostUsed = _statisticsService.GetMostUsed(10);
        var leastUsed = _statisticsService.GetLeastUsed(10);
        var neverUsed = _statisticsService.GetNeverUsed()
            .Select(b => b.DisplayName)
            .ToList();
        var session = _statisticsService.GetCurrentSessionStats();
        var matchHistory = _statisticsService.GetMatchHistory();
        var currentMatch = _statisticsService.CurrentMatch;

        Statistics.Refresh(mostUsed, leastUsed, neverUsed, session, matchHistory, currentMatch);

        // Auto-activate heatmap when entering stats mode
        if (!Statistics.IsHeatmapActive)
            Statistics.IsHeatmapActive = true;

        var heatData = _statisticsService.GetHeatmapData();
        Keyboard.ApplyHeatmapData(heatData);
    }

    private static bool IsModifierKey(string relicKeyName)
    {
        return relicKeyName
            is "LControl" or "RControl"
            or "LShift" or "RShift"
            or "LMenu" or "RMenu";
    }
}

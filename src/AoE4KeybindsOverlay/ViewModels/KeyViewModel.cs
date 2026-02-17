using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using AoE4KeybindsOverlay.Models;

namespace AoE4KeybindsOverlay.ViewModels;

/// <summary>
/// ViewModel representing a single physical key's visual state on the keyboard.
/// </summary>
public partial class KeyViewModel : ObservableObject
{
    private const double ScaleFactor = 50.0;

    /// <summary>The Relic key identifier (e.g., "A", "LBracket", "F1").</summary>
    public string KeyId { get; init; } = string.Empty;

    /// <summary>Display label shown on the key (e.g., "A", "[", "F1").</summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>X position in key-unit coordinates.</summary>
    public double X { get; init; }

    /// <summary>Y position in key-unit coordinates.</summary>
    public double Y { get; init; }

    /// <summary>Width in key-unit coordinates (1.0 = standard key).</summary>
    public double Width { get; init; } = 1.0;

    /// <summary>Height in key-unit coordinates.</summary>
    public double Height { get; init; } = 1.0;

    // --- Computed pixel properties for XAML Canvas layout ---

    /// <summary>Canvas X position in pixels (X * ScaleFactor).</summary>
    public double CanvasX => X * ScaleFactor;

    /// <summary>Canvas Y position in pixels (Y * ScaleFactor).</summary>
    public double CanvasY => Y * ScaleFactor;

    /// <summary>Width in pixels (Width * ScaleFactor).</summary>
    public double PixelWidth => Width * ScaleFactor;

    /// <summary>Height in pixels (Height * ScaleFactor).</summary>
    public double PixelHeight => Height * ScaleFactor;

    /// <summary>Whether this key is currently physically pressed.</summary>
    [ObservableProperty]
    private bool _isPressed;

    /// <summary>Category color brush hex when this key is part of an active binding.</summary>
    [ObservableProperty]
    private string? _highlightColor;

    /// <summary>Heatmap intensity from 0.0 (cold) to 1.0 (hot).</summary>
    [ObservableProperty]
    private double _heatmapIntensity;

    /// <summary>Short label for the binding shown on the key in overlay mode.</summary>
    [ObservableProperty]
    private string? _bindingLabel;

    /// <summary>Whether this key participates in any active/possible binding.</summary>
    [ObservableProperty]
    private bool _hasBinding;

    /// <summary>The binding category for color-coding, if applicable.</summary>
    [ObservableProperty]
    private BindingCategory? _activeCategory;

    /// <summary>Whether heatmap mode is currently active.</summary>
    [ObservableProperty]
    private bool _isHeatmapMode;

    /// <summary>Whether this key should be visually dimmed (not part of any possible binding).</summary>
    [ObservableProperty]
    private bool _isDimmed;

    // --- Cached brush to avoid re-creating on every XAML access ---
    private Brush? _cachedHighlightBrush;

    // --- Computed properties for XAML bindings ---

    /// <summary>Brush for the highlight overlay, derived from HighlightColor hex string.</summary>
    public Brush? HighlightBrush => _cachedHighlightBrush;

    /// <summary>Whether this key currently has a highlight applied.</summary>
    public bool HasHighlight => HighlightColor is not null;

    /// <summary>Binding text shown below key label, maps from BindingLabel.</summary>
    public string? BindingText => BindingLabel;

    partial void OnHighlightColorChanged(string? value)
    {
        _cachedHighlightBrush = value is not null
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(value))
            : null;
        _cachedHighlightBrush?.Freeze(); // Frozen brushes are cheaper to render
        OnPropertyChanged(nameof(HighlightBrush));
        OnPropertyChanged(nameof(HasHighlight));
    }

    partial void OnBindingLabelChanged(string? value)
    {
        OnPropertyChanged(nameof(BindingText));
    }

    /// <summary>
    /// Applies highlight state. The generated property setters include equality checks,
    /// so only properties that actually changed will fire notifications.
    /// </summary>
    public void ApplyHighlight(string color, string label, BindingCategory category)
    {
        HighlightColor = color;
        BindingLabel = label;
        HasBinding = true;
        ActiveCategory = category;
    }

    /// <summary>
    /// Clears highlight state. The generated property setters include equality checks,
    /// so only properties that actually changed will fire notifications.
    /// </summary>
    public void ClearHighlight()
    {
        HighlightColor = null;
        BindingLabel = null;
        HasBinding = false;
        ActiveCategory = null;
    }

    /// <summary>
    /// Creates a KeyViewModel from a PhysicalKey layout definition.
    /// </summary>
    public static KeyViewModel FromPhysicalKey(PhysicalKey physicalKey)
    {
        return new KeyViewModel
        {
            KeyId = physicalKey.KeyId,
            Label = physicalKey.Label,
            X = physicalKey.X,
            Y = physicalKey.Y,
            Width = physicalKey.Width,
            Height = physicalKey.Height
        };
    }
}

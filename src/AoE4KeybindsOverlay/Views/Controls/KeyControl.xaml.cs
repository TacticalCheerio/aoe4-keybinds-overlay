using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AoE4KeybindsOverlay.Views.Controls;

/// <summary>
/// A visual control representing a single keyboard or mouse key.
/// Displays the key label, optional binding text, and visual states
/// for pressed, highlighted, and heatmap modes.
/// </summary>
public partial class KeyControl : UserControl
{
    #region Dependency Properties

    /// <summary>
    /// The display label of the key (e.g., "A", "Ctrl", "F1").
    /// </summary>
    public static readonly DependencyProperty KeyLabelProperty =
        DependencyProperty.Register(
            nameof(KeyLabel), typeof(string), typeof(KeyControl),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// The width of the key in pixels (scaled from layout units).
    /// </summary>
    public static readonly DependencyProperty KeyWidthProperty =
        DependencyProperty.Register(
            nameof(KeyWidth), typeof(double), typeof(KeyControl),
            new PropertyMetadata(50.0));

    /// <summary>
    /// The height of the key in pixels (scaled from layout units).
    /// </summary>
    public static readonly DependencyProperty KeyHeightProperty =
        DependencyProperty.Register(
            nameof(KeyHeight), typeof(double), typeof(KeyControl),
            new PropertyMetadata(50.0));

    /// <summary>
    /// Whether the key is currently pressed.
    /// </summary>
    public static readonly DependencyProperty IsPressedProperty =
        DependencyProperty.Register(
            nameof(IsPressed), typeof(bool), typeof(KeyControl),
            new PropertyMetadata(false));

    /// <summary>
    /// The brush used to highlight the key when a binding matches current modifiers.
    /// </summary>
    public static readonly DependencyProperty HighlightBrushProperty =
        DependencyProperty.Register(
            nameof(HighlightBrush), typeof(Brush), typeof(KeyControl),
            new PropertyMetadata(null));

    /// <summary>
    /// Whether the key currently has a highlight applied.
    /// </summary>
    public static readonly DependencyProperty HasHighlightProperty =
        DependencyProperty.Register(
            nameof(HasHighlight), typeof(bool), typeof(KeyControl),
            new PropertyMetadata(false));

    /// <summary>
    /// The heatmap intensity value (0.0 = cold/unused, 1.0 = hot/heavily used).
    /// </summary>
    public static readonly DependencyProperty HeatmapIntensityProperty =
        DependencyProperty.Register(
            nameof(HeatmapIntensity), typeof(double), typeof(KeyControl),
            new PropertyMetadata(0.0));

    /// <summary>
    /// Whether heatmap mode is active.
    /// </summary>
    public static readonly DependencyProperty IsHeatmapModeProperty =
        DependencyProperty.Register(
            nameof(IsHeatmapMode), typeof(bool), typeof(KeyControl),
            new PropertyMetadata(false));

    /// <summary>
    /// The binding text to show below the key label.
    /// </summary>
    public static readonly DependencyProperty BindingTextProperty =
        DependencyProperty.Register(
            nameof(BindingText), typeof(string), typeof(KeyControl),
            new PropertyMetadata(string.Empty, OnBindingTextChanged));

    /// <summary>
    /// Whether the key has any binding text to display.
    /// </summary>
    public static readonly DependencyProperty HasBindingTextProperty =
        DependencyProperty.Register(
            nameof(HasBindingText), typeof(bool), typeof(KeyControl),
            new PropertyMetadata(false));

    /// <summary>
    /// The Relic key identifier for this key (e.g., "A", "LBracket").
    /// </summary>
    public static readonly DependencyProperty KeyIdProperty =
        DependencyProperty.Register(
            nameof(KeyId), typeof(string), typeof(KeyControl),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Whether the key is visually dimmed (not part of any possible binding).
    /// </summary>
    public static readonly DependencyProperty IsDimmedProperty =
        DependencyProperty.Register(
            nameof(IsDimmed), typeof(bool), typeof(KeyControl),
            new PropertyMetadata(false));

    /// <summary>
    /// Font size for the main key label text.
    /// </summary>
    public static readonly DependencyProperty KeyFontSizeProperty =
        DependencyProperty.Register(
            nameof(KeyFontSize), typeof(double), typeof(KeyControl),
            new PropertyMetadata(11.0));

    #endregion

    #region Properties

    public string KeyLabel
    {
        get => (string)GetValue(KeyLabelProperty);
        set => SetValue(KeyLabelProperty, value);
    }

    public double KeyWidth
    {
        get => (double)GetValue(KeyWidthProperty);
        set => SetValue(KeyWidthProperty, value);
    }

    public double KeyHeight
    {
        get => (double)GetValue(KeyHeightProperty);
        set => SetValue(KeyHeightProperty, value);
    }

    public bool IsPressed
    {
        get => (bool)GetValue(IsPressedProperty);
        set => SetValue(IsPressedProperty, value);
    }

    public Brush? HighlightBrush
    {
        get => (Brush?)GetValue(HighlightBrushProperty);
        set => SetValue(HighlightBrushProperty, value);
    }

    public bool HasHighlight
    {
        get => (bool)GetValue(HasHighlightProperty);
        set => SetValue(HasHighlightProperty, value);
    }

    public double HeatmapIntensity
    {
        get => (double)GetValue(HeatmapIntensityProperty);
        set => SetValue(HeatmapIntensityProperty, value);
    }

    public bool IsHeatmapMode
    {
        get => (bool)GetValue(IsHeatmapModeProperty);
        set => SetValue(IsHeatmapModeProperty, value);
    }

    public string BindingText
    {
        get => (string)GetValue(BindingTextProperty);
        set => SetValue(BindingTextProperty, value);
    }

    public bool HasBindingText
    {
        get => (bool)GetValue(HasBindingTextProperty);
        set => SetValue(HasBindingTextProperty, value);
    }

    public string KeyId
    {
        get => (string)GetValue(KeyIdProperty);
        set => SetValue(KeyIdProperty, value);
    }

    public bool IsDimmed
    {
        get => (bool)GetValue(IsDimmedProperty);
        set => SetValue(IsDimmedProperty, value);
    }

    public double KeyFontSize
    {
        get => (double)GetValue(KeyFontSizeProperty);
        set => SetValue(KeyFontSizeProperty, value);
    }

    #endregion

    public KeyControl()
    {
        InitializeComponent();
    }

    private static void OnBindingTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is KeyControl control)
        {
            control.HasBindingText = !string.IsNullOrEmpty(e.NewValue as string);
        }
    }
}

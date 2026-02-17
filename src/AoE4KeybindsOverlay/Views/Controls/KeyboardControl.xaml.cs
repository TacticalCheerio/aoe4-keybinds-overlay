using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace AoE4KeybindsOverlay.Views.Controls;

/// <summary>
/// A control that renders a full keyboard layout by positioning individual
/// <see cref="KeyControl"/> instances on a canvas according to their layout coordinates.
/// </summary>
public partial class KeyboardControl : UserControl
{
    /// <summary>
    /// The number of pixels per logical key unit used for layout calculations.
    /// </summary>
    public const double ScaleFactor = 50.0;

    #region Dependency Properties

    /// <summary>
    /// The collection of key view models to render. Each item should expose
    /// KeyId, Label, CanvasX, CanvasY, PixelWidth, PixelHeight, IsPressed,
    /// HighlightBrush, HasHighlight, HeatmapIntensity, IsHeatmapMode, BindingText.
    /// </summary>
    public static readonly DependencyProperty KeyViewModelsProperty =
        DependencyProperty.Register(
            nameof(KeyViewModels), typeof(IEnumerable), typeof(KeyboardControl),
            new PropertyMetadata(null));

    /// <summary>
    /// The total canvas width in pixels.
    /// </summary>
    public static readonly DependencyProperty CanvasWidthProperty =
        DependencyProperty.Register(
            nameof(CanvasWidth), typeof(double), typeof(KeyboardControl),
            new PropertyMetadata(22.5 * ScaleFactor));

    /// <summary>
    /// The total canvas height in pixels.
    /// </summary>
    public static readonly DependencyProperty CanvasHeightProperty =
        DependencyProperty.Register(
            nameof(CanvasHeight), typeof(double), typeof(KeyboardControl),
            new PropertyMetadata(6.5 * ScaleFactor));

    /// <summary>
    /// Font size for key labels.
    /// </summary>
    public static readonly DependencyProperty KeyFontSizeProperty =
        DependencyProperty.Register(
            nameof(KeyFontSize), typeof(double), typeof(KeyboardControl),
            new PropertyMetadata(11.0));

    #endregion

    #region Properties

    public double KeyFontSize
    {
        get => (double)GetValue(KeyFontSizeProperty);
        set => SetValue(KeyFontSizeProperty, value);
    }

    public IEnumerable? KeyViewModels
    {
        get => (IEnumerable?)GetValue(KeyViewModelsProperty);
        set => SetValue(KeyViewModelsProperty, value);
    }

    public double CanvasWidth
    {
        get => (double)GetValue(CanvasWidthProperty);
        set => SetValue(CanvasWidthProperty, value);
    }

    public double CanvasHeight
    {
        get => (double)GetValue(CanvasHeightProperty);
        set => SetValue(CanvasHeightProperty, value);
    }

    #endregion

    public KeyboardControl()
    {
        InitializeComponent();
    }
}

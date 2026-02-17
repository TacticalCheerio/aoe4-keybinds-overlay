using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace AoE4KeybindsOverlay.Views.Controls;

/// <summary>
/// A control that renders a stylized mouse with clickable button regions.
/// Each button is a <see cref="KeyControl"/> positioned on a canvas
/// matching the mouse layout coordinates.
/// </summary>
public partial class MouseControl : UserControl
{
    /// <summary>
    /// The number of pixels per logical mouse unit used for layout calculations.
    /// </summary>
    public const double ScaleFactor = 50.0;

    #region Dependency Properties

    /// <summary>
    /// The collection of mouse button view models to render.
    /// </summary>
    public static readonly DependencyProperty ButtonViewModelsProperty =
        DependencyProperty.Register(
            nameof(ButtonViewModels), typeof(IEnumerable), typeof(MouseControl),
            new PropertyMetadata(null));

    /// <summary>
    /// The total canvas width in pixels (mouse totalWidth * ScaleFactor).
    /// </summary>
    public static readonly DependencyProperty CanvasWidthProperty =
        DependencyProperty.Register(
            nameof(CanvasWidth), typeof(double), typeof(MouseControl),
            new PropertyMetadata(4.0 * ScaleFactor));

    /// <summary>
    /// The total canvas height in pixels (mouse totalHeight * ScaleFactor).
    /// </summary>
    public static readonly DependencyProperty CanvasHeightProperty =
        DependencyProperty.Register(
            nameof(CanvasHeight), typeof(double), typeof(MouseControl),
            new PropertyMetadata(6.0 * ScaleFactor));

    /// <summary>
    /// The X position of the center divider line between LMB and RMB.
    /// </summary>
    public static readonly DependencyProperty DividerXProperty =
        DependencyProperty.Register(
            nameof(DividerX), typeof(double), typeof(MouseControl),
            new PropertyMetadata(2.0 * ScaleFactor));

    /// <summary>
    /// Font size for button labels.
    /// </summary>
    public static readonly DependencyProperty KeyFontSizeProperty =
        DependencyProperty.Register(
            nameof(KeyFontSize), typeof(double), typeof(MouseControl),
            new PropertyMetadata(11.0));

    #endregion

    #region Properties

    public double KeyFontSize
    {
        get => (double)GetValue(KeyFontSizeProperty);
        set => SetValue(KeyFontSizeProperty, value);
    }

    public IEnumerable? ButtonViewModels
    {
        get => (IEnumerable?)GetValue(ButtonViewModelsProperty);
        set => SetValue(ButtonViewModelsProperty, value);
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

    public double DividerX
    {
        get => (double)GetValue(DividerXProperty);
        set => SetValue(DividerXProperty, value);
    }

    #endregion

    public MouseControl()
    {
        InitializeComponent();
    }
}

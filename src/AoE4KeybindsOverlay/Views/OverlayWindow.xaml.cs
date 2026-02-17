using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using AoE4KeybindsOverlay.ViewModels;

namespace AoE4KeybindsOverlay.Views;

/// <summary>
/// The main overlay window that displays keyboard/mouse visuals and binding information.
/// Supports lock/unlock for repositioning, scroll-to-resize, and system tray integration.
/// </summary>
public partial class OverlayWindow : Window
{
    #region Win32 Constants

    private const int GWL_EXSTYLE = -20;
    private const long WS_EX_TRANSPARENT = 0x20L;
    private const long WS_EX_LAYERED = 0x80000L;
    private const long WS_EX_TOOLWINDOW = 0x80L;

    #endregion

    #region Win32 P/Invoke

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    #endregion

    private bool _isLocked;

    public OverlayWindow()
    {
        InitializeComponent();
        _isLocked = false; // Start unlocked so controls are visible
        PositionBottomCenter();
    }

    /// <summary>
    /// Whether the overlay is currently locked (click-through, not movable).
    /// </summary>
    public bool IsLocked
    {
        get => _isLocked;
        set
        {
            _isLocked = value;
            ApplyLockState();
        }
    }

    private void PositionBottomCenter()
    {
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;

        Left = (screenWidth - Width) / 2;
        Top = screenHeight - Height - 20;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        SetExToolWindow();
        ApplyLockState();
    }

    private void ApplyLockState()
    {
        SetClickThrough(_isLocked);

        var unlocked = _isLocked ? Visibility.Collapsed : Visibility.Visible;
        MoveGrip.Visibility = unlocked;
        ResizeGrip.Visibility = unlocked;

        if (LockButton != null)
            LockButton.Content = _isLocked ? "Unlock" : "Lock";
    }

    /// <summary>
    /// Enables or disables click-through behavior on the overlay window.
    /// </summary>
    public void SetClickThrough(bool enable)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero)
            return;

        var extendedStyle = (long)GetWindowLongPtr(hwnd, GWL_EXSTYLE);

        if (enable)
        {
            extendedStyle |= WS_EX_TRANSPARENT | WS_EX_LAYERED;
        }
        else
        {
            extendedStyle &= ~WS_EX_TRANSPARENT;
        }

        SetWindowLongPtr(hwnd, GWL_EXSTYLE, (IntPtr)extendedStyle);
    }

    private void SetExToolWindow()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero)
            return;

        var extendedStyle = (long)GetWindowLongPtr(hwnd, GWL_EXSTYLE);
        extendedStyle |= WS_EX_TOOLWINDOW;
        SetWindowLongPtr(hwnd, GWL_EXSTYLE, (IntPtr)extendedStyle);
    }

    private void MoveGrip_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!_isLocked)
        {
            DragMove();
        }
    }

    private void LockButton_Click(object sender, RoutedEventArgs e)
    {
        IsLocked = !IsLocked;
    }

    private void HideButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void StatsButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            var newMode = vm.CurrentMode == OverlayMode.Statistics
                ? OverlayMode.Live
                : OverlayMode.Statistics;
            vm.SwitchModeCommand.Execute(newMode);
            StatsButton.Content = vm.IsStatisticsMode ? "Live" : "Stats";
        }
    }

    private void ToggleListButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.Settings.ShowBindingList = !vm.Settings.ShowBindingList;
            ToggleListButton.Content = vm.Settings.ShowBindingList ? "List" : "List Off";
        }
    }

    private void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void ResizeGrip_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (_isLocked) return;

        var newWidth = Width + e.HorizontalChange;
        var newHeight = Height + e.VerticalChange;

        if (newWidth >= MinWidth)
            Width = newWidth;
        if (newHeight >= MinHeight)
            Height = newHeight;
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        // Scroll-to-resize when unlocked
        if (_isLocked) return;

        double scaleFactor = e.Delta > 0 ? 1.05 : 0.95;

        var newWidth = Width * scaleFactor;
        var newHeight = Height * scaleFactor;

        if (newWidth >= MinWidth && newHeight >= MinHeight)
        {
            // Resize around center
            var centerX = Left + Width / 2;
            var centerY = Top + Height / 2;

            Width = newWidth;
            Height = newHeight;

            Left = centerX - Width / 2;
            Top = centerY - Height / 2;
        }

        e.Handled = true;
    }
}

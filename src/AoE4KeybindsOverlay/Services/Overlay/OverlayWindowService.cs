using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace AoE4KeybindsOverlay.Services.Overlay;

/// <summary>
/// Manages overlay window behavior using Win32 interop.
/// </summary>
public sealed class OverlayWindowService : IOverlayWindowService
{
    private IntPtr _hwnd;
    private readonly DispatcherTimer _topmostTimer;

    private const int GWL_EXSTYLE = -20;
    private const long WS_EX_TRANSPARENT = 0x00000020L;
    private const long WS_EX_LAYERED = 0x00080000L;

    private static readonly IntPtr HWND_TOPMOST = new(-1);
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOACTIVATE = 0x0010;

    public OverlayWindowService()
    {
        _topmostTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _topmostTimer.Tick += (_, _) => EnforceTopmost();
    }

    public void SetWindowHandle(IntPtr hwnd)
    {
        _hwnd = hwnd;
    }

    public void SetClickThrough(bool enable)
    {
        if (_hwnd == IntPtr.Zero) return;

        var style = GetWindowLongPtr(_hwnd, GWL_EXSTYLE).ToInt64();

        if (enable)
        {
            style |= WS_EX_TRANSPARENT;
            style |= WS_EX_LAYERED;
        }
        else
        {
            style &= ~WS_EX_TRANSPARENT;
        }

        SetWindowLongPtr(_hwnd, GWL_EXSTYLE, (IntPtr)style);
    }

    public void StartTopmostEnforcement()
    {
        _topmostTimer.Start();
    }

    public void StopTopmostEnforcement()
    {
        _topmostTimer.Stop();
    }

    private void EnforceTopmost()
    {
        if (_hwnd == IntPtr.Zero) return;
        SetWindowPos(_hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int x, int y, int cx, int cy, uint uFlags);
}

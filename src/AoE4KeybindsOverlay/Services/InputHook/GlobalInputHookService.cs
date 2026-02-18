using System.Runtime.InteropServices;
using System.Windows.Threading;
using AoE4KeybindsOverlay.Models;

namespace AoE4KeybindsOverlay.Services.InputHook;

/// <summary>
/// Implements global input hooking using Win32 low-level keyboard and mouse hooks.
/// </summary>
/// <remarks>
/// <para>
/// Uses <c>WH_KEYBOARD_LL</c> (13) and <c>WH_MOUSE_LL</c> (14) via <c>SetWindowsHookEx</c>.
/// Hook delegates are stored as instance fields to prevent garbage collection.
/// </para>
/// <para>
/// Must be started from a thread with a message pump (the WPF UI thread). Hook callbacks
/// are fast: they capture the minimal data and use <see cref="Dispatcher.BeginInvoke(Action)"/>
/// to marshal event raising to the UI thread.
/// </para>
/// </remarks>
public sealed class GlobalInputHookService : IInputHookService
{
    #region Win32 P/Invoke

    private const int WH_KEYBOARD_LL = 13;
    private const int WH_MOUSE_LL = 14;

    // Keyboard messages
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;

    // Mouse messages
    private const int WM_MOUSEMOVE = 0x0200;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_LBUTTONUP = 0x0202;
    private const int WM_RBUTTONDOWN = 0x0204;
    private const int WM_RBUTTONUP = 0x0205;
    private const int WM_MBUTTONDOWN = 0x0207;
    private const int WM_MBUTTONUP = 0x0208;
    private const int WM_MOUSEWHEEL = 0x020A;
    private const int WM_XBUTTONDOWN = 0x020B;
    private const int WM_XBUTTONUP = 0x020C;
    private const int WM_NCMOUSEMOVE = 0x00A0;

    // Virtual key codes for modifier keys
    private const int VK_LCONTROL = 0xA2;
    private const int VK_RCONTROL = 0xA3;
    private const int VK_LSHIFT = 0xA0;
    private const int VK_RSHIFT = 0xA1;
    private const int VK_LMENU = 0xA4;  // Left Alt
    private const int VK_RMENU = 0xA5;  // Right Alt

    // XBUTTON identifiers (stored in high word of mouseData)
    private const int XBUTTON1 = 0x0001;
    private const int XBUTTON2 = 0x0002;

    private delegate IntPtr LowLevelHookProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelHookProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    /// <summary>
    /// Low-level keyboard hook structure (KBDLLHOOKSTRUCT).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public int vkCode;
        public int scanCode;
        public int flags;
        public int time;
        public IntPtr dwExtraInfo;
    }

    /// <summary>
    /// Low-level mouse hook structure (MSLLHOOKSTRUCT).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public int ptX;
        public int ptY;
        public int mouseData;
        public int flags;
        public int time;
        public IntPtr dwExtraInfo;
    }

    #endregion

    /// <summary>
    /// The WPF dispatcher to marshal events to the UI thread.
    /// </summary>
    private readonly Dispatcher _dispatcher;

    /// <summary>
    /// Keyboard hook delegate - stored as a field to prevent GC from collecting it.
    /// </summary>
    private readonly LowLevelHookProc _keyboardHookProc;

    /// <summary>
    /// Mouse hook delegate - stored as a field to prevent GC from collecting it.
    /// </summary>
    private readonly LowLevelHookProc _mouseHookProc;

    /// <summary>
    /// Handle to the installed keyboard hook.
    /// </summary>
    private IntPtr _keyboardHookId = IntPtr.Zero;

    /// <summary>
    /// Handle to the installed mouse hook.
    /// </summary>
    private IntPtr _mouseHookId = IntPtr.Zero;

    /// <summary>
    /// Tracks currently pressed keys by virtual key code to suppress repeat KeyDown events.
    /// </summary>
    private readonly HashSet<int> _pressedKeys = new();

    /// <summary>
    /// Tracks currently pressed mouse buttons by Relic name to suppress repeat events.
    /// </summary>
    private readonly HashSet<string> _pressedMouseButtons = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Whether the service has been disposed.
    /// </summary>
    private bool _disposed;

    /// <inheritdoc />
    public event EventHandler<KeyboardHookEventArgs>? KeyDown;

    /// <inheritdoc />
    public event EventHandler<KeyboardHookEventArgs>? KeyUp;

    /// <inheritdoc />
    public event EventHandler<MouseHookEventArgs>? MouseDown;

    /// <inheritdoc />
    public event EventHandler<MouseHookEventArgs>? MouseUp;

    /// <inheritdoc />
    public ModifierKeys CurrentModifiers { get; private set; } = ModifierKeys.None;

    /// <inheritdoc />
    public bool IsHookActive => _keyboardHookId != IntPtr.Zero || _mouseHookId != IntPtr.Zero;

    /// <summary>
    /// Initializes a new instance of <see cref="GlobalInputHookService"/>.
    /// </summary>
    /// <param name="dispatcher">
    /// The WPF dispatcher for the UI thread. Events will be marshalled to this dispatcher.
    /// </param>
    public GlobalInputHookService(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

        // Store delegates as instance fields to prevent garbage collection.
        _keyboardHookProc = KeyboardHookCallback;
        _mouseHookProc = MouseHookCallback;
    }

    /// <inheritdoc />
    public void Start()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(GlobalInputHookService));

        if (IsHookActive)
            return;

        var moduleHandle = GetModuleHandle(null);

        _keyboardHookId = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardHookProc, moduleHandle, 0);
        if (_keyboardHookId == IntPtr.Zero)
        {
            var error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException(
                $"Failed to install keyboard hook. Win32 error code: {error}");
        }

        _mouseHookId = SetWindowsHookEx(WH_MOUSE_LL, _mouseHookProc, moduleHandle, 0);
        if (_mouseHookId == IntPtr.Zero)
        {
            // Clean up the keyboard hook if mouse hook fails
            UnhookWindowsHookEx(_keyboardHookId);
            _keyboardHookId = IntPtr.Zero;

            var error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException(
                $"Failed to install mouse hook. Win32 error code: {error}");
        }

        _pressedKeys.Clear();
        _pressedMouseButtons.Clear();
    }

    /// <inheritdoc />
    public void Stop()
    {
        if (_keyboardHookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_keyboardHookId);
            _keyboardHookId = IntPtr.Zero;
        }

        if (_mouseHookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_mouseHookId);
            _mouseHookId = IntPtr.Zero;
        }

        _pressedKeys.Clear();
        _pressedMouseButtons.Clear();
        CurrentModifiers = ModifierKeys.None;
    }

    /// <summary>
    /// Low-level keyboard hook callback. Must return quickly.
    /// </summary>
    private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
            var vkCode = hookStruct.vkCode;
            var message = wParam.ToInt32();

            bool isKeyDown = message == WM_KEYDOWN || message == WM_SYSKEYDOWN;
            bool isKeyUp = message == WM_KEYUP || message == WM_SYSKEYUP;

            if (isKeyDown || isKeyUp)
            {
                // Update modifier state
                UpdateModifierState(vkCode, isKeyDown);

                if (isKeyDown)
                {
                    // Suppress repeat events: only fire if key is not already pressed
                    if (_pressedKeys.Add(vkCode))
                    {
                        var relicName = VirtualKeyMap.ToRelicName(vkCode);
                        if (relicName is not null)
                        {
                            var modifiers = CurrentModifiers;
                            // Use Send priority for modifier keys so combo highlights
                            // appear instantly without waiting behind queued render work.
                            var priority = IsModifierVk(vkCode)
                                ? DispatcherPriority.Send
                                : DispatcherPriority.Input;
                            _dispatcher.BeginInvoke(priority, () =>
                            {
                                KeyDown?.Invoke(this, new KeyboardHookEventArgs
                                {
                                    RelicKeyName = relicName,
                                    VirtualKeyCode = vkCode,
                                    ActiveModifiers = modifiers
                                });
                            });
                        }
                    }
                }
                else // isKeyUp
                {
                    _pressedKeys.Remove(vkCode);

                    var relicName = VirtualKeyMap.ToRelicName(vkCode);
                    if (relicName is not null)
                    {
                        var modifiers = CurrentModifiers;
                        var priority = IsModifierVk(vkCode)
                            ? DispatcherPriority.Send
                            : DispatcherPriority.Input;
                        _dispatcher.BeginInvoke(priority, () =>
                        {
                            KeyUp?.Invoke(this, new KeyboardHookEventArgs
                            {
                                RelicKeyName = relicName,
                                VirtualKeyCode = vkCode,
                                ActiveModifiers = modifiers
                            });
                        });
                    }
                }
            }
        }

        return CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
    }

    /// <summary>
    /// Low-level mouse hook callback. Must return quickly.
    /// Bails out immediately for mouse move events to avoid overhead during gameplay.
    /// </summary>
    private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var message = wParam.ToInt32();

            // Fast reject: skip mouse moves immediately — no marshalling, no processing.
            // In a game, mouse moves fire hundreds of times per second; processing them
            // causes unnecessary context switches and measurable input lag.
            if (message == WM_MOUSEMOVE || message == WM_NCMOUSEMOVE)
                return CallNextHookEx(_mouseHookId, nCode, wParam, lParam);

            var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);

            var (relicName, isDown) = ResolveMouseEvent(message, hookStruct.mouseData);

            if (relicName is not null)
            {
                var modifiers = CurrentModifiers;

                if (isDown)
                {
                    // For wheel events, always fire (no press tracking); for buttons, track state
                    bool isWheel = relicName.StartsWith("MouseWheel", StringComparison.Ordinal);
                    if (isWheel || _pressedMouseButtons.Add(relicName))
                    {
                        _dispatcher.BeginInvoke(() =>
                        {
                            MouseDown?.Invoke(this, new MouseHookEventArgs
                            {
                                RelicKeyName = relicName,
                                ActiveModifiers = modifiers
                            });
                        });

                        // Wheel events have no corresponding "up" message, so auto-release after a short delay
                        if (isWheel)
                        {
                            var wheelName = relicName;
                            var wheelMods = modifiers;
                            _ = Task.Delay(TimeSpan.FromMilliseconds(100)).ContinueWith(_ =>
                            {
                                _dispatcher.BeginInvoke(() =>
                                {
                                    MouseUp?.Invoke(this, new MouseHookEventArgs
                                    {
                                        RelicKeyName = wheelName,
                                        ActiveModifiers = wheelMods
                                    });
                                });
                            });
                        }
                    }
                }
                else
                {
                    _pressedMouseButtons.Remove(relicName);
                    _dispatcher.BeginInvoke(() =>
                    {
                        MouseUp?.Invoke(this, new MouseHookEventArgs
                        {
                            RelicKeyName = relicName,
                            ActiveModifiers = modifiers
                        });
                    });
                }
            }
        }

        return CallNextHookEx(_mouseHookId, nCode, wParam, lParam);
    }

    /// <summary>
    /// Resolves a mouse window message and mouse data into a Relic key name and down/up state.
    /// </summary>
    /// <param name="message">The window message (WM_LBUTTONDOWN, etc.).</param>
    /// <param name="mouseData">The mouseData field from MSLLHOOKSTRUCT (used for wheel/xbutton).</param>
    /// <returns>A tuple of (relicName, isDown). relicName is null if the event is not mapped.</returns>
    private static (string? RelicName, bool IsDown) ResolveMouseEvent(int message, int mouseData)
    {
        return message switch
        {
            WM_LBUTTONDOWN => ("LeftMouseButton", true),
            WM_LBUTTONUP => ("LeftMouseButton", false),
            WM_RBUTTONDOWN => ("RightMouseButton", true),
            WM_RBUTTONUP => ("RightMouseButton", false),
            WM_MBUTTONDOWN => ("MiddleMouseButton", true),
            WM_MBUTTONUP => ("MiddleMouseButton", false),
            WM_MOUSEWHEEL => ResolveWheelEvent(mouseData),
            WM_XBUTTONDOWN => ResolveXButtonEvent(mouseData, isDown: true),
            WM_XBUTTONUP => ResolveXButtonEvent(mouseData, isDown: false),
            _ => (null, false)
        };
    }

    /// <summary>
    /// Resolves a mouse wheel event into a Relic key name.
    /// </summary>
    private static (string? RelicName, bool IsDown) ResolveWheelEvent(int mouseData)
    {
        // The high word of mouseData contains the wheel delta (signed short).
        // Positive = scroll up, Negative = scroll down.
        var delta = (short)((mouseData >> 16) & 0xFFFF);
        if (delta > 0)
            return ("MouseWheelUp", true);
        if (delta < 0)
            return ("MouseWheelDown", true);
        return (null, false);
    }

    /// <summary>
    /// Resolves an X button event into a Relic key name.
    /// </summary>
    private static (string? RelicName, bool IsDown) ResolveXButtonEvent(int mouseData, bool isDown)
    {
        var xButton = (mouseData >> 16) & 0xFFFF;
        return xButton switch
        {
            XBUTTON1 => ("MouseX1", isDown),
            XBUTTON2 => ("MouseX2", isDown),
            _ => (null, false)
        };
    }

    /// <summary>
    /// Returns true if the virtual key code is a modifier (Ctrl, Shift, Alt).
    /// </summary>
    private static bool IsModifierVk(int vkCode) =>
        vkCode is VK_LCONTROL or VK_RCONTROL
              or VK_LSHIFT or VK_RSHIFT
              or VK_LMENU or VK_RMENU;

    /// <summary>
    /// Updates the tracked modifier state based on key events for Ctrl, Shift, and Alt.
    /// </summary>
    /// <param name="vkCode">The virtual key code.</param>
    /// <param name="isDown">True if the key is being pressed, false if released.</param>
    private void UpdateModifierState(int vkCode, bool isDown)
    {
        var modifier = vkCode switch
        {
            VK_LCONTROL or VK_RCONTROL => ModifierKeys.Ctrl,
            VK_LSHIFT or VK_RSHIFT => ModifierKeys.Shift,
            VK_LMENU or VK_RMENU => ModifierKeys.Alt,
            _ => ModifierKeys.None
        };

        if (modifier == ModifierKeys.None)
            return;

        if (isDown)
            CurrentModifiers |= modifier;
        else
            CurrentModifiers &= ~modifier;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        Stop();
        _disposed = true;
    }
}

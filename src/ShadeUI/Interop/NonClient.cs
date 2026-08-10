using System.Runtime.InteropServices;

namespace ShadeUI.Interop;

/// <summary>
/// Non-client window messages and the bits of user32 needed to bring back the
/// behaviours a custom title bar loses: Snap Layouts on the maximize button and
/// the system menu on right-click.
/// </summary>
internal static class NonClient
{
    public const int WmNcHitTest = 0x0084;
    public const int WmNcMouseMove = 0x00A0;
    public const int WmNcLButtonDown = 0x00A1;
    public const int WmNcLButtonUp = 0x00A2;
    public const int WmNcLButtonDblClk = 0x00A3;
    public const int WmNcRButtonUp = 0x00A5;
    public const int WmNcMouseLeave = 0x02A2;
    public const int WmSysCommand = 0x0112;

    public const int HtCaption = 2;
    public const int HtSysMenu = 3;
    public const int HtMaxButton = 9;

    public const uint ScSize = 0xF000;
    public const uint ScMove = 0xF010;
    public const uint ScMinimize = 0xF020;
    public const uint ScMaximize = 0xF030;
    public const uint ScClose = 0xF060;
    public const uint ScRestore = 0xF120;

    private const uint MfByCommand = 0x0000;
    private const uint MfEnabled = 0x0000;
    private const uint MfGrayed = 0x0001;

    private const uint TpmReturnCmd = 0x0100;
    private const uint TpmRightButton = 0x0002;

    [DllImport("user32.dll")]
    private static extern IntPtr GetSystemMenu(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool revert);

    [DllImport("user32.dll")]
    private static extern int TrackPopupMenuEx(IntPtr menu, uint flags, int x, int y, IntPtr hwnd, IntPtr param);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnableMenuItem(IntPtr menu, uint item, uint enable);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

    /// <summary>Extracts the signed screen X coordinate packed into an lParam.</summary>
    public static int GetX(IntPtr lParam) => (short)(lParam.ToInt64() & 0xFFFF);

    /// <summary>Extracts the signed screen Y coordinate packed into an lParam.</summary>
    public static int GetY(IntPtr lParam) => (short)((lParam.ToInt64() >> 16) & 0xFFFF);

    /// <summary>
    /// Shows the standard Move / Size / Minimize / Maximize / Close menu at a screen point
    /// and posts the chosen command back to the window.
    /// </summary>
    public static void ShowSystemMenu(IntPtr hwnd, int screenX, int screenY, SystemMenuState state)
    {
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        IntPtr menu = GetSystemMenu(hwnd, false);
        if (menu == IntPtr.Zero)
        {
            return;
        }

        Enable(menu, ScRestore, state.CanRestore);
        Enable(menu, ScMove, state.CanMove);
        Enable(menu, ScSize, state.CanSize);
        Enable(menu, ScMinimize, state.CanMinimize);
        Enable(menu, ScMaximize, state.CanMaximize);
        Enable(menu, ScClose, state.CanClose);

        int command = TrackPopupMenuEx(menu, TpmReturnCmd | TpmRightButton, screenX, screenY, hwnd, IntPtr.Zero);

        if (command != 0)
        {
            _ = PostMessage(hwnd, WmSysCommand, new IntPtr(command), IntPtr.Zero);
        }
    }

    private static void Enable(IntPtr menu, uint command, bool enabled)
    {
        _ = EnableMenuItem(menu, command, MfByCommand | (enabled ? MfEnabled : MfGrayed));
    }

    /// <summary>Which system menu commands are currently valid for a window.</summary>
    internal readonly record struct SystemMenuState(
        bool CanRestore,
        bool CanMove,
        bool CanSize,
        bool CanMinimize,
        bool CanMaximize,
        bool CanClose);
}

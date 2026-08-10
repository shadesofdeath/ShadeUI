using System.Runtime.InteropServices;

namespace ShadeUI.Interop;

/// <summary>
/// Turns a normal (non-transparent) window into a layered one just long enough to
/// fade its whole frame out. Used for the close animation, where animating the
/// window's <c>Opacity</c> would only fade the content and leave the window
/// background behind.
/// </summary>
internal static class LayeredWindow
{
    private const int GwlExStyle = -20;
    private const int WsExLayered = 0x00080000;
    private const int LwaAlpha = 0x02;

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hwnd, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hwnd, int index, IntPtr value);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)]
    private static extern int GetWindowLong32(IntPtr hwnd, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
    private static extern int SetWindowLong32(IntPtr hwnd, int index, int value);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint key, byte alpha, int flags);

    /// <summary>Adds <c>WS_EX_LAYERED</c> so <see cref="SetAlpha"/> can take effect.</summary>
    public static bool Enable(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero)
        {
            return false;
        }

        try
        {
            if (IntPtr.Size == 8)
            {
                long style = GetWindowLongPtr64(hwnd, GwlExStyle).ToInt64();
                _ = SetWindowLongPtr64(hwnd, GwlExStyle, new IntPtr(style | WsExLayered));
            }
            else
            {
                int style = GetWindowLong32(hwnd, GwlExStyle);
                _ = SetWindowLong32(hwnd, GwlExStyle, style | WsExLayered);
            }

            return SetAlpha(hwnd, 255);
        }
        catch (EntryPointNotFoundException)
        {
            return false;
        }
    }

    /// <summary>Sets the whole-window alpha (0 = invisible, 255 = opaque).</summary>
    public static bool SetAlpha(IntPtr hwnd, byte alpha)
    {
        if (hwnd == IntPtr.Zero)
        {
            return false;
        }

        try
        {
            return SetLayeredWindowAttributes(hwnd, 0, alpha, LwaAlpha);
        }
        catch (EntryPointNotFoundException)
        {
            return false;
        }
    }
}

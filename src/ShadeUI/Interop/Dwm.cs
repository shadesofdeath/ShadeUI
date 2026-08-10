using System.Runtime.InteropServices;
using System.Windows.Media;

namespace ShadeUI.Interop;

/// <summary>
/// Thin wrapper over the Desktop Window Manager attributes ShadeUI needs.
/// All calls fail silently on Windows versions that do not support them.
/// </summary>
internal static class Dwm
{
    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwaUseImmersiveDarkModeOld = 19; // Windows 10 builds before 20H1
    private const int DwmwaWindowCornerPreference = 33;
    private const int DwmwaBorderColor = 34;

    private const int DwmwcpRound = 2;

    [DllImport("dwmapi.dll", ExactSpelling = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    public static void SetImmersiveDarkMode(IntPtr hwnd, bool enabled)
    {
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        int value = enabled ? 1 : 0;

        try
        {
            if (DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkMode, ref value, sizeof(int)) != 0)
            {
                _ = DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkModeOld, ref value, sizeof(int));
            }
        }
        catch (DllNotFoundException)
        {
        }
    }

    public static void SetRoundedCorners(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        int value = DwmwcpRound;

        try
        {
            _ = DwmSetWindowAttribute(hwnd, DwmwaWindowCornerPreference, ref value, sizeof(int));
        }
        catch (DllNotFoundException)
        {
        }
    }

    /// <summary>
    /// Paints the 1 px non-client border around the window. Windows 11 22000+ only;
    /// older builds silently keep the system border.
    /// </summary>
    public static void SetBorderColor(IntPtr hwnd, Color color)
    {
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        // COLORREF is 0x00BBGGRR
        int value = color.R | (color.G << 8) | (color.B << 16);

        try
        {
            _ = DwmSetWindowAttribute(hwnd, DwmwaBorderColor, ref value, sizeof(int));
        }
        catch (DllNotFoundException)
        {
        }
    }
}

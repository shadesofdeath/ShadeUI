using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace ShadeUI.Appearance;

/// <summary>
/// Listens for Windows theme and accent changes on a hidden message-only window
/// and triggers a <see cref="ThemeManager.Refresh"/> when something actually changed.
/// </summary>
internal static class SystemThemeWatcher
{
    private const int WmSettingChange = 0x001A;
    private const int WmDwmColorizationColorChanged = 0x0320;

    private static readonly IntPtr HwndMessage = new(-3);

    private static HwndSource? _source;
    private static (bool IsDark, Color Accent) _lastState;

    public static void Start()
    {
        if (_source is not null)
        {
            return;
        }

        var parameters = new HwndSourceParameters("ShadeUI.SystemThemeWatcher")
        {
            ParentWindow = HwndMessage,
            WindowStyle = 0,
            Width = 1,
            Height = 1,
        };

        _source = new HwndSource(parameters);
        _source.AddHook(WndProc);
        _lastState = ReadState();
    }

    public static void Stop()
    {
        if (_source is null)
        {
            return;
        }

        _source.RemoveHook(WndProc);
        _source.Dispose();
        _source = null;
    }

    private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmDwmColorizationColorChanged)
        {
            CheckForChanges();
        }
        else if (msg == WmSettingChange && IsThemeSettingChange(lParam))
        {
            CheckForChanges();
        }

        return IntPtr.Zero;
    }

    private static bool IsThemeSettingChange(IntPtr lParam)
    {
        if (lParam == IntPtr.Zero)
        {
            return true;
        }

        try
        {
            string? area = Marshal.PtrToStringUni(lParam);
            return string.IsNullOrEmpty(area)
                   || area.Equals("ImmersiveColorSet", StringComparison.OrdinalIgnoreCase)
                   || area.Equals("WindowsThemeElement", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static void CheckForChanges()
    {
        (bool IsDark, Color Accent) current = ReadState();
        if (current == _lastState)
        {
            return;
        }

        _lastState = current;

        // Get out of the WndProc before touching application resources.
        Application.Current?.Dispatcher.BeginInvoke(new Action(ThemeManager.Refresh));
    }

    private static (bool, Color) ReadState()
    {
        return (SystemThemeProbe.IsSystemDark(), SystemThemeProbe.GetAccentColor());
    }
}

using System.Windows;
using System.Windows.Interop;
using ShadeUI.Appearance;
using ShadeUI.Interop;

namespace ShadeUI.Controls;

/// <summary>
/// A <see cref="Window"/> with ShadeUI chrome: custom title bar area, themed surfaces,
/// dark-mode aware non-client parts and rounded corners on Windows 11.
/// </summary>
public class ShadeWindow : Window
{
    static ShadeWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ShadeWindow),
            new FrameworkPropertyMetadata(typeof(ShadeWindow)));
    }

    public ShadeWindow()
    {
        ThemeManager.ThemeChanged += OnThemeChanged;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        ApplyChromeAttributes();
    }

    protected override void OnClosed(EventArgs e)
    {
        ThemeManager.ThemeChanged -= OnThemeChanged;
        base.OnClosed(e);
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        ApplyChromeAttributes();
    }

    private void ApplyChromeAttributes()
    {
        IntPtr hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        Dwm.SetImmersiveDarkMode(hwnd, ThemeManager.ActualTheme == ApplicationTheme.Dark);
        Dwm.SetRoundedCorners(hwnd);
    }
}

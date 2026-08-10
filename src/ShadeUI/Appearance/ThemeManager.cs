using System.Windows;
using System.Windows.Threading;

namespace ShadeUI.Appearance;

/// <summary>
/// Central entry point for applying and querying the ShadeUI theme.
/// </summary>
public static class ThemeManager
{
    private static readonly Uri DarkPaletteSource =
        new("pack://application:,,,/ShadeUI;component/Resources/Palette/Dark.xaml", UriKind.Absolute);

    private static readonly Uri LightPaletteSource =
        new("pack://application:,,,/ShadeUI;component/Resources/Palette/Light.xaml", UriKind.Absolute);

    private const string PaletteSourceMarker = ";component/Resources/Palette/";

    /// <summary>The theme mode the application asked for (may be <see cref="ApplicationTheme.System"/>).</summary>
    public static ApplicationTheme RequestedTheme { get; private set; } = ApplicationTheme.System;

    /// <summary>The palette currently in effect. Always <see cref="ApplicationTheme.Light"/> or <see cref="ApplicationTheme.Dark"/>.</summary>
    public static ApplicationTheme ActualTheme { get; private set; } =
        SystemThemeProbe.IsSystemDark() ? ApplicationTheme.Dark : ApplicationTheme.Light;

    /// <summary>Raised after the palette or accent has been (re)applied.</summary>
    public static event EventHandler? ThemeChanged;

    /// <summary>
    /// Applies a theme mode. <see cref="ApplicationTheme.System"/> starts watching Windows
    /// for theme and accent changes; explicit modes stop the watcher.
    /// </summary>
    public static void Apply(ApplicationTheme theme)
    {
        RequestedTheme = theme;

        if (theme == ApplicationTheme.System)
        {
            SystemThemeWatcher.Start();
        }
        else
        {
            SystemThemeWatcher.Stop();
        }

        Refresh();
    }

    /// <summary>
    /// Called by <see cref="Markup.ThemesDictionary"/> while the application resources are being parsed.
    /// Records the requested mode synchronously and schedules the full apply for when the app is up.
    /// </summary>
    internal static void InitializeFromDictionary(ApplicationTheme theme)
    {
        RequestedTheme = theme;
        ActualTheme = ResolveActual(theme);

        Dispatcher dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
        dispatcher.BeginInvoke(new Action(() => Apply(theme)), DispatcherPriority.Loaded);
    }

    /// <summary>Re-reads system state (if following the system) and re-applies palette and accent.</summary>
    internal static void Refresh()
    {
        ApplicationTheme actual = ResolveActual(RequestedTheme);

        SwapPalette(actual == ApplicationTheme.Dark);
        AccentManager.Apply(actual == ApplicationTheme.Dark);

        ActualTheme = actual;
        ThemeChanged?.Invoke(null, EventArgs.Empty);
    }

    private static ApplicationTheme ResolveActual(ApplicationTheme requested)
    {
        return requested switch
        {
            ApplicationTheme.Light => ApplicationTheme.Light,
            ApplicationTheme.Dark => ApplicationTheme.Dark,
            _ => SystemThemeProbe.IsSystemDark() ? ApplicationTheme.Dark : ApplicationTheme.Light,
        };
    }

    private static void SwapPalette(bool dark)
    {
        ResourceDictionary? appResources = Application.Current?.Resources;
        if (appResources is null)
        {
            return;
        }

        Uri target = dark ? DarkPaletteSource : LightPaletteSource;
        ResourceDictionary? palette = FindPaletteDictionary(appResources);

        if (palette is not null && palette.Source != target)
        {
            palette.Source = target;
        }
    }

    private static ResourceDictionary? FindPaletteDictionary(ResourceDictionary root)
    {
        if (root.Source is not null &&
            root.Source.OriginalString.Contains(PaletteSourceMarker, StringComparison.OrdinalIgnoreCase))
        {
            return root;
        }

        foreach (ResourceDictionary child in root.MergedDictionaries)
        {
            ResourceDictionary? match = FindPaletteDictionary(child);
            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }
}

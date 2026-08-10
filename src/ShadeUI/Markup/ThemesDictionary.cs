using System.Windows;
using ShadeUI.Appearance;

namespace ShadeUI.Markup;

/// <summary>
/// Merges the ShadeUI color palette into application resources.
/// <code language="xml">
/// &lt;ui:ThemesDictionary Theme="System" /&gt;
/// </code>
/// </summary>
public class ThemesDictionary : ResourceDictionary
{
    private const string DarkSource = "pack://application:,,,/ShadeUI;component/Resources/Palette/Dark.xaml";
    private const string LightSource = "pack://application:,,,/ShadeUI;component/Resources/Palette/Light.xaml";

    private ApplicationTheme _theme = ApplicationTheme.System;

    public ThemesDictionary()
    {
        SetTheme(ApplicationTheme.System);
    }

    /// <summary>The theme mode to load. Defaults to <see cref="ApplicationTheme.System"/>.</summary>
    public ApplicationTheme Theme
    {
        get => _theme;
        set => SetTheme(value);
    }

    private void SetTheme(ApplicationTheme theme)
    {
        _theme = theme;

        bool dark = theme switch
        {
            ApplicationTheme.Dark => true,
            ApplicationTheme.Light => false,
            _ => SystemThemeProbe.IsSystemDark(),
        };

        Source = new Uri(dark ? DarkSource : LightSource, UriKind.Absolute);

        ThemeManager.InitializeFromDictionary(theme);
    }
}

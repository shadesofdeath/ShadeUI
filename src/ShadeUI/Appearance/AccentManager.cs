using System.Windows;
using System.Windows.Media;

namespace ShadeUI.Appearance;

/// <summary>
/// Derives the accent palette from a base accent color and publishes it
/// into <see cref="Application.Resources"/> so DynamicResource references update live.
/// </summary>
internal static class AccentManager
{
    public static void Apply(bool isDarkTheme)
    {
        Apply(SystemThemeProbe.GetAccentColor(), isDarkTheme);
    }

    public static void Apply(Color accent, bool isDarkTheme)
    {
        ResourceDictionary? resources = Application.Current?.Resources;
        if (resources is null)
        {
            return;
        }

        Color light1 = Blend(accent, Colors.White, 0.2);
        Color light2 = Blend(accent, Colors.White, 0.4);
        Color light3 = Blend(accent, Colors.White, 0.6);
        Color dark1 = Blend(accent, Colors.Black, 0.2);
        Color dark2 = Blend(accent, Colors.Black, 0.4);
        Color dark3 = Blend(accent, Colors.Black, 0.6);

        // Same mapping WinUI uses: light accent variants on dark surfaces, dark variants on light surfaces.
        Color fillDefault = isDarkTheme ? light2 : dark1;
        Color textAccent = isDarkTheme ? light3 : dark2;

        resources["SystemAccentColor"] = accent;
        resources["SystemAccentColorLight1"] = light1;
        resources["SystemAccentColorLight2"] = light2;
        resources["SystemAccentColorLight3"] = light3;
        resources["SystemAccentColorDark1"] = dark1;
        resources["SystemAccentColorDark2"] = dark2;
        resources["SystemAccentColorDark3"] = dark3;

        resources["AccentFillColorDefaultBrush"] = CreateBrush(fillDefault);
        resources["AccentFillColorSecondaryBrush"] = CreateBrush(WithOpacity(fillDefault, 0.9));
        resources["AccentFillColorTertiaryBrush"] = CreateBrush(WithOpacity(fillDefault, 0.8));
        resources["AccentTextFillColorPrimaryBrush"] = CreateBrush(textAccent);
    }

    private static SolidColorBrush CreateBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static Color WithOpacity(Color color, double opacity)
    {
        return Color.FromArgb((byte)(255 * opacity), color.R, color.G, color.B);
    }

    private static Color Blend(Color source, Color target, double amount)
    {
        return Color.FromRgb(
            (byte)(source.R + (target.R - source.R) * amount),
            (byte)(source.G + (target.G - source.G) * amount),
            (byte)(source.B + (target.B - source.B) * amount));
    }
}

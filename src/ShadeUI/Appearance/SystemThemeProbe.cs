using System.Windows.Media;
using Microsoft.Win32;

namespace ShadeUI.Appearance;

/// <summary>
/// Reads the current Windows personalization settings (app theme and accent color).
/// </summary>
internal static class SystemThemeProbe
{
    private const string PersonalizeKey =
        @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    private const string DwmKey =
        @"HKEY_CURRENT_USER\Software\Microsoft\Windows\DWM";

    private const string AccentKey =
        @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Accent";

    /// <summary>Default Windows accent blue, used when the registry cannot be read.</summary>
    internal static readonly Color FallbackAccent = Color.FromRgb(0x00, 0x78, 0xD4);

    public static bool IsSystemDark()
    {
        try
        {
            return Registry.GetValue(PersonalizeKey, "AppsUseLightTheme", 1) is int value && value == 0;
        }
        catch
        {
            return false;
        }
    }

    public static Color GetAccentColor()
    {
        try
        {
            // Both values are stored as 0xAABBGGRR.
            object? raw = Registry.GetValue(DwmKey, "AccentColor", null)
                          ?? Registry.GetValue(AccentKey, "AccentColorMenu", null);

            if (raw is int abgr)
            {
                return Color.FromRgb(
                    (byte)(abgr & 0xFF),
                    (byte)((abgr >> 8) & 0xFF),
                    (byte)((abgr >> 16) & 0xFF));
            }
        }
        catch
        {
            // Fall through to the default accent.
        }

        return FallbackAccent;
    }
}

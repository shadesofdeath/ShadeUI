namespace ShadeUI.Appearance;

/// <summary>
/// The material the desktop compositor paints behind a window.
/// Everything except <see cref="Solid"/> needs Windows 11 build 22621 or newer;
/// on older systems <see cref="Controls.ShadeWindow"/> falls back to <see cref="Solid"/>.
/// </summary>
public enum WindowBackdropType
{
    /// <summary>No system material — the window paints its own opaque background.</summary>
    Solid,

    /// <summary>Mica: a slow, desktop-wallpaper-tinted material for long-lived windows.</summary>
    Mica,

    /// <summary>Mica Alt ("tabbed"): a stronger Mica variant meant for tabbed shells.</summary>
    Tabbed,

    /// <summary>Acrylic: a live blur of what is behind the window, for transient surfaces.</summary>
    Acrylic,
}

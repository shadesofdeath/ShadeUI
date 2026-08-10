using System.Windows;
using System.Windows.Controls;

namespace ShadeUI.Controls;

/// <summary>Identifies what a <see cref="TitleBarButton"/> does.</summary>
public enum TitleBarButtonType
{
    Minimize,
    Maximize,
    Restore,
    Close,
}

/// <summary>A caption button (minimize / maximize / restore / close) used by <see cref="TitleBar"/>.</summary>
public class TitleBarButton : Button
{
    static TitleBarButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TitleBarButton),
            new FrameworkPropertyMetadata(typeof(TitleBarButton)));
    }

    public static readonly DependencyProperty ButtonTypeProperty = DependencyProperty.Register(
        nameof(ButtonType),
        typeof(TitleBarButtonType),
        typeof(TitleBarButton),
        new PropertyMetadata(TitleBarButtonType.Minimize));

    public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register(
        nameof(Glyph),
        typeof(string),
        typeof(TitleBarButton),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IsSimulatedHoverProperty = DependencyProperty.Register(
        nameof(IsSimulatedHover),
        typeof(bool),
        typeof(TitleBarButton),
        new PropertyMetadata(false));

    public static readonly DependencyProperty IsSimulatedPressProperty = DependencyProperty.Register(
        nameof(IsSimulatedPress),
        typeof(bool),
        typeof(TitleBarButton),
        new PropertyMetadata(false));

    public TitleBarButtonType ButtonType
    {
        get => (TitleBarButtonType)GetValue(ButtonTypeProperty);
        set => SetValue(ButtonTypeProperty, value);
    }

    /// <summary>Segoe Fluent Icons glyph shown by the button. Normally set by the style from <see cref="ButtonType"/>.</summary>
    public string Glyph
    {
        get => (string)GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    /// <summary>
    /// Draws the hover state even though WPF sees no mouse. Needed while Windows owns the
    /// pointer for Snap Layouts, where the button is reported as a non-client area.
    /// </summary>
    public bool IsSimulatedHover
    {
        get => (bool)GetValue(IsSimulatedHoverProperty);
        set => SetValue(IsSimulatedHoverProperty, value);
    }

    /// <summary>Draws the pressed state during a non-client (Snap Layouts) click.</summary>
    public bool IsSimulatedPress
    {
        get => (bool)GetValue(IsSimulatedPressProperty);
        set => SetValue(IsSimulatedPressProperty, value);
    }
}

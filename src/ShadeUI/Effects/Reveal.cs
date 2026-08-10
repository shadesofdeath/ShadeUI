using System.Windows;
using System.Windows.Input;

namespace ShadeUI.Effects;

/// <summary>
/// Fluent "reveal" highlight: tracks the pointer over an element and publishes its
/// position as a normalized <see cref="Point"/> that control templates bind to as the
/// center of a radial gradient.
/// <code language="xml">
/// &lt;Setter Property="effects:Reveal.IsEnabled" Value="True" /&gt;
/// </code>
/// </summary>
public static class Reveal
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(Reveal),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static readonly DependencyProperty OriginProperty =
        DependencyProperty.RegisterAttached(
            "Origin",
            typeof(Point),
            typeof(Reveal),
            new FrameworkPropertyMetadata(new Point(0.5, 0.5)));

    public static void SetIsEnabled(DependencyObject element, bool value) =>
        element.SetValue(IsEnabledProperty, value);

    public static bool GetIsEnabled(DependencyObject element) =>
        (bool)element.GetValue(IsEnabledProperty);

    public static void SetOrigin(DependencyObject element, Point value) =>
        element.SetValue(OriginProperty, value);

    public static Point GetOrigin(DependencyObject element) =>
        (Point)element.GetValue(OriginProperty);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
        {
            return;
        }

        element.MouseMove -= OnMouseMove;
        element.MouseLeave -= OnMouseLeave;

        if (e.NewValue is true)
        {
            element.MouseMove += OnMouseMove;
            element.MouseLeave += OnMouseLeave;
        }
    }

    private static void OnMouseMove(object sender, MouseEventArgs e)
    {
        var element = (FrameworkElement)sender;

        if (element.ActualWidth <= 0 || element.ActualHeight <= 0)
        {
            return;
        }

        var position = e.GetPosition(element);

        SetOrigin(element, new Point(
            position.X / element.ActualWidth,
            position.Y / element.ActualHeight));
    }

    private static void OnMouseLeave(object sender, MouseEventArgs e) =>
        SetOrigin((FrameworkElement)sender, new Point(0.5, 0.5));
}

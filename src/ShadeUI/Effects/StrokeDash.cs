using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ShadeUI.Effects;

/// <summary>
/// Makes a shape's dash pattern animatable.
/// <para>
/// WPF exposes <see cref="Shape.StrokeDashArray"/> as a <see cref="DoubleCollection"/>, which a
/// <c>DoubleAnimation</c> cannot target. This attached property stands in for the first (visible)
/// dash segment, so the CSS <c>stroke-dasharray</c> trick — sliding a dash window along a path to
/// morph one drawing into another — can be reproduced with an ordinary animation.
/// </para>
/// <para>
/// Values are in multiples of <see cref="Shape.StrokeThickness"/>, exactly like
/// <see cref="Shape.StrokeDashArray"/>; divide CSS pixel values by the stroke width.
/// </para>
/// </summary>
public static class StrokeDash
{
    /// <summary>Length of the gap that follows the visible dash. Large enough to never repeat.</summary>
    private const double GapLength = 1_000_000d;

    public static readonly DependencyProperty LengthProperty = DependencyProperty.RegisterAttached(
        "Length",
        typeof(double),
        typeof(StrokeDash),
        new PropertyMetadata(double.NaN, OnLengthChanged));

    public static void SetLength(DependencyObject element, double value) =>
        element.SetValue(LengthProperty, value);

    public static double GetLength(DependencyObject element) =>
        (double)element.GetValue(LengthProperty);

    private static void OnLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Shape shape || e.NewValue is not double length || double.IsNaN(length))
        {
            return;
        }

        // Mutate the existing collection while animating so a 60 fps animation does not
        // allocate a new one on every frame.
        if (shape.StrokeDashArray is { IsFrozen: false, Count: 2 } dashes)
        {
            dashes[0] = length;
            return;
        }

        shape.StrokeDashArray = new DoubleCollection { length, GapLength };
    }
}

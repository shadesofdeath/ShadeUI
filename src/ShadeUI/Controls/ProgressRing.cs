using System.Windows;
using System.Windows.Controls.Primitives;

namespace ShadeUI.Controls;

/// <summary>
/// A circular progress indicator. WPF has no built-in equivalent, so the arc is drawn by
/// dashing a full circle: the visible dash length is the fraction of the circumference that
/// corresponds to <see cref="RangeBase.Value"/>.
/// <code language="xml">
/// &lt;ui:ProgressRing Value="65" /&gt;
/// &lt;ui:ProgressRing IsIndeterminate="True" /&gt;
/// </code>
/// </summary>
public class ProgressRing : RangeBase
{
    /// <summary>
    /// Circumference of the template's circle expressed in stroke-thickness units,
    /// which is what WPF's <c>StrokeDashArray</c> works in: 2·π·32 / 6.
    /// </summary>
    private const double Circumference = 33.5103;

    /// <summary>Fraction of the ring drawn while indeterminate.</summary>
    private const double IndeterminateFraction = 0.25;

    static ProgressRing()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ProgressRing),
            new FrameworkPropertyMetadata(typeof(ProgressRing)));

        MaximumProperty.OverrideMetadata(
            typeof(ProgressRing),
            new FrameworkPropertyMetadata(100d));
    }

    public ProgressRing()
    {
        UpdateDashLength();
    }

    public static readonly DependencyProperty IsIndeterminateProperty = DependencyProperty.Register(
        nameof(IsIndeterminate), typeof(bool), typeof(ProgressRing),
        new PropertyMetadata(false, OnIsIndeterminateChanged));

    public static readonly DependencyProperty RingSizeProperty = DependencyProperty.Register(
        nameof(RingSize), typeof(double), typeof(ProgressRing), new PropertyMetadata(20d));

    private static readonly DependencyPropertyKey DashLengthPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(DashLength), typeof(double), typeof(ProgressRing), new PropertyMetadata(0d));

    /// <summary>Identifies the read-only <see cref="DashLength"/> property.</summary>
    public static readonly DependencyProperty DashLengthProperty = DashLengthPropertyKey.DependencyProperty;

    /// <summary>When true the ring spins with a fixed arc instead of showing a value.</summary>
    public bool IsIndeterminate
    {
        get => (bool)GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    /// <summary>Outer diameter in device-independent pixels. Defaults to 20.</summary>
    public double RingSize
    {
        get => (double)GetValue(RingSizeProperty);
        set => SetValue(RingSizeProperty, value);
    }

    /// <summary>Visible dash length in stroke-thickness units. Bound by the template.</summary>
    public double DashLength => (double)GetValue(DashLengthProperty);

    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);
        UpdateDashLength();
    }

    protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
    {
        base.OnMinimumChanged(oldMinimum, newMinimum);
        UpdateDashLength();
    }

    protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
    {
        base.OnMaximumChanged(oldMaximum, newMaximum);
        UpdateDashLength();
    }

    private static void OnIsIndeterminateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((ProgressRing)d).UpdateDashLength();
    }

    private void UpdateDashLength()
    {
        if (IsIndeterminate)
        {
            SetValue(DashLengthPropertyKey, Circumference * IndeterminateFraction);
            return;
        }

        double range = Maximum - Minimum;
        double fraction = range <= 0 ? 0d : Math.Clamp((Value - Minimum) / range, 0d, 1d);

        SetValue(DashLengthPropertyKey, Circumference * fraction);
    }
}

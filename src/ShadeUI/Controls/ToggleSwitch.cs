using System.Windows;
using System.Windows.Controls.Primitives;

namespace ShadeUI.Controls;

/// <summary>
/// A sliding on/off switch. Behaves like a <see cref="ToggleButton"/>, so
/// <c>IsChecked</c>, <c>Checked</c>, <c>Unchecked</c> and <c>Command</c> all work as usual.
/// <code language="xml">
/// &lt;ui:ToggleSwitch OnContent="Açık" OffContent="Kapalı" /&gt;
/// </code>
/// </summary>
public class ToggleSwitch : ToggleButton
{
    static ToggleSwitch()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ToggleSwitch),
            new FrameworkPropertyMetadata(typeof(ToggleSwitch)));
    }

    public static readonly DependencyProperty OnContentProperty = DependencyProperty.Register(
        nameof(OnContent), typeof(object), typeof(ToggleSwitch),
        new PropertyMetadata(null, OnStateContentChanged));

    public static readonly DependencyProperty OffContentProperty = DependencyProperty.Register(
        nameof(OffContent), typeof(object), typeof(ToggleSwitch),
        new PropertyMetadata(null, OnStateContentChanged));

    private static readonly DependencyPropertyKey ActualContentPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ActualContent), typeof(object), typeof(ToggleSwitch), new PropertyMetadata(null));

    /// <summary>Identifies the read-only <see cref="ActualContent"/> property.</summary>
    public static readonly DependencyProperty ActualContentProperty =
        ActualContentPropertyKey.DependencyProperty;

    /// <summary>Label shown next to the switch while it is on. Falls back to <c>Content</c>.</summary>
    public object? OnContent
    {
        get => GetValue(OnContentProperty);
        set => SetValue(OnContentProperty, value);
    }

    /// <summary>Label shown next to the switch while it is off. Falls back to <c>Content</c>.</summary>
    public object? OffContent
    {
        get => GetValue(OffContentProperty);
        set => SetValue(OffContentProperty, value);
    }

    /// <summary>The label the template is currently rendering. Resolved from the state and the three content properties.</summary>
    public object? ActualContent => GetValue(ActualContentProperty);

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateActualContent();
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        UpdateActualContent();
    }

    protected override void OnChecked(RoutedEventArgs e)
    {
        base.OnChecked(e);
        UpdateActualContent();
    }

    protected override void OnUnchecked(RoutedEventArgs e)
    {
        base.OnUnchecked(e);
        UpdateActualContent();
    }

    private static void OnStateContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((ToggleSwitch)d).UpdateActualContent();
    }

    private void UpdateActualContent()
    {
        object? stateContent = IsChecked == true ? OnContent : OffContent;
        SetValue(ActualContentPropertyKey, stateContent ?? Content);
    }
}

using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ShadeUI.Automation;

namespace ShadeUI.Controls;

/// <summary>
/// An entry in a <see cref="NavigationView"/> pane: a glyph, a label and a selection indicator.
/// </summary>
public class NavigationViewItem : ButtonBase
{
    static NavigationViewItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NavigationViewItem),
            new FrameworkPropertyMetadata(typeof(NavigationViewItem)));
    }

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(string), typeof(NavigationViewItem), new PropertyMetadata(null));

    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
        nameof(IsSelected), typeof(bool), typeof(NavigationViewItem), new PropertyMetadata(false));

    public static readonly DependencyProperty InfoBadgeProperty = DependencyProperty.Register(
        nameof(InfoBadge), typeof(string), typeof(NavigationViewItem), new PropertyMetadata(null));

    /// <summary>Segoe Fluent Icons glyph shown before the label.</summary>
    public string? Icon
    {
        get => (string?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Whether this is the current page. Managed by the owning <see cref="NavigationView"/>.</summary>
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>Optional short text drawn in a pill at the trailing edge (unread counts and the like).</summary>
    public string? InfoBadge
    {
        get => (string?)GetValue(InfoBadgeProperty);
        set => SetValue(InfoBadgeProperty, value);
    }

    /// <summary>Selects this item as if it had been clicked. Used by UI Automation.</summary>
    public void PerformClick() => OnClick();

    protected override AutomationPeer OnCreateAutomationPeer() => new NavigationViewItemAutomationPeer(this);

    protected override void OnClick()
    {
        base.OnClick();
        FindOwner()?.SelectItem(this);
    }

    private NavigationView? FindOwner()
    {
        DependencyObject? current = VisualTreeHelper.GetParent(this);

        while (current is not null)
        {
            if (current is NavigationView view)
            {
                return view;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }
}

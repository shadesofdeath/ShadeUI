using System.Windows;
using System.Windows.Controls;

namespace ShadeUI.Controls;

/// <summary>
/// A non-interactive category label between groups of <see cref="NavigationViewItem"/>s.
/// Collapses to a divider line when the pane is in its icon rail state.
/// </summary>
public class NavigationViewItemHeader : ContentControl
{
    static NavigationViewItemHeader()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NavigationViewItemHeader),
            new FrameworkPropertyMetadata(typeof(NavigationViewItemHeader)));

        FocusableProperty.OverrideMetadata(
            typeof(NavigationViewItemHeader),
            new FrameworkPropertyMetadata(false));
    }
}

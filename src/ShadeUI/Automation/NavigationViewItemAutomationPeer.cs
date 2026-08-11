using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ShadeUI.Controls;

namespace ShadeUI.Automation;

/// <summary>
/// Exposes a <see cref="NavigationViewItem"/> to UI Automation as a selectable list item.
/// Without this, a <c>ButtonBase</c>-derived custom control falls back to a generic peer and
/// screen readers see neither its name nor its selected state.
/// </summary>
public class NavigationViewItemAutomationPeer : FrameworkElementAutomationPeer, IInvokeProvider, ISelectionItemProvider
{
    public NavigationViewItemAutomationPeer(NavigationViewItem owner)
        : base(owner)
    {
    }

    private NavigationViewItem Item => (NavigationViewItem)Owner;

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.ListItem;

    protected override string GetClassNameCore() => nameof(NavigationViewItem);

    protected override string GetNameCore()
    {
        string name = base.GetNameCore();

        if (!string.IsNullOrEmpty(name))
        {
            return name;
        }

        return Item.Content?.ToString() ?? string.Empty;
    }

    public override object? GetPattern(PatternInterface patternInterface)
    {
        if (patternInterface is PatternInterface.Invoke or PatternInterface.SelectionItem)
        {
            return this;
        }

        return base.GetPattern(patternInterface);
    }

    public void Invoke() => Item.PerformClick();

    public bool IsSelected => Item.IsSelected;

    public IRawElementProviderSimple? SelectionContainer => null;

    public void AddToSelection() => Item.PerformClick();

    public void RemoveFromSelection()
    {
        // Navigation always keeps exactly one item selected.
    }

    public void Select() => Item.PerformClick();
}

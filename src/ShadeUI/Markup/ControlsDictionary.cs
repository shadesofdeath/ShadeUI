using System.Windows;

namespace ShadeUI.Markup;

/// <summary>
/// Merges all ShadeUI control styles into application resources.
/// <code language="xml">
/// &lt;ui:ControlsDictionary /&gt;
/// </code>
/// </summary>
public class ControlsDictionary : ResourceDictionary
{
    private const string ControlsSource = "pack://application:,,,/ShadeUI;component/Resources/ShadeUI.xaml";

    public ControlsDictionary()
    {
        Source = new Uri(ControlsSource, UriKind.Absolute);
    }
}

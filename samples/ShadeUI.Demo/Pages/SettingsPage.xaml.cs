using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using ShadeUI.Controls;

namespace ShadeUI.Demo.Pages;

public partial class SettingsPage : UserControl
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private TitleBar? AppTitleBar => (Window.GetWindow(this) as MainWindow)?.AppTitleBar;

    private void OnTitleAlignmentChecked(object sender, RoutedEventArgs e)
    {
        if (AppTitleBar is not { } titleBar || sender is not RadioButton { Tag: string tag })
        {
            return;
        }

        if (Enum.TryParse(tag, out TitleAlignment alignment))
        {
            titleBar.TitleAlignment = alignment;
        }
    }

    private void OnShowIconChanged(object sender, RoutedEventArgs e)
    {
        if (AppTitleBar is { } titleBar && sender is CheckBox box)
        {
            titleBar.ShowIcon = box.IsChecked == true;
        }
    }

    private void OnCanMaximizeChanged(object sender, RoutedEventArgs e)
    {
        if (AppTitleBar is { } titleBar && sender is CheckBox box)
        {
            titleBar.CanMaximize = box.IsChecked == true;
        }
    }

    private void OnBoldTitleChanged(object sender, RoutedEventArgs e)
    {
        if (AppTitleBar is { } titleBar && sender is CheckBox box)
        {
            titleBar.TitleFontWeight = box.IsChecked == true
                ? FontWeights.SemiBold
                : FontWeights.Normal;
        }
    }

    private void OnNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }
}

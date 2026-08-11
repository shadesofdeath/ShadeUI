using System.Windows;
using System.Windows.Controls;
using ShadeUI.Appearance;
using ShadeUI.Controls;

namespace ShadeUI.Demo.Pages;

public partial class ShellPage : UserControl
{
    public ShellPage()
    {
        InitializeComponent();

        Ex0.Xaml = """
            <ui:ShadeWindow Backdrop="Mica" />

            <!-- Solid | Mica | Tabbed | Acrylic -->
            <!-- ActualBackdrop, isteğin kabul edilip edilmediğini bildirir. -->
            """;

        Ex1.Xaml = """
            <ui:TitleBar Title="Uygulamam"
                         TitleAlignment="Center"
                         IconGlyph="&#xE790;" />
            """;

        Ex2.Xaml = """
            <ui:TitleBar ShowIcon="True"
                         CanMaximize="False"
                         CloseButtonAction="Hide"
                         IsSnapLayoutEnabled="True"
                         IsSystemMenuEnabled="True" />
            """;

        Ex3.Xaml = """
            <ui:NavigationView PaneTitle="ShadeUI"
                               IsSearchVisible="True"
                               OpenPaneLength="212"
                               CompactPaneLength="44">
                <ui:NavigationView.MenuItems>
                    <ui:NavigationViewItemHeader Content="Temel" />
                    <ui:NavigationViewItem Content="Button"
                                           Icon="&#xE8A9;"
                                           Tag="button" />
                </ui:NavigationView.MenuItems>
                <ui:NavigationView.FooterItems>
                    <ui:NavigationViewItem Content="Ayarlar" Icon="&#xE713;" />
                </ui:NavigationView.FooterItems>
            </ui:NavigationView>
            """;
    }

    private MainWindow? Host => Window.GetWindow(this) as MainWindow;

    private void OnBackdropChecked(object sender, RoutedEventArgs e)
    {
        if (Host is not { } host || sender is not RadioButton { Tag: string tag })
        {
            return;
        }

        if (!Enum.TryParse(tag, out WindowBackdropType requested))
        {
            return;
        }

        host.Backdrop = requested;

        BackdropStatus.Text = host.ActualBackdrop == requested
            ? $"Uygulandı: {host.ActualBackdrop}"
            : $"'{requested}' bu Windows sürümünde desteklenmiyor, {host.ActualBackdrop} kullanılıyor.";
    }

    private void OnTitleAlignmentChecked(object sender, RoutedEventArgs e)
    {
        if (Host is not { } host || sender is not RadioButton { Tag: string tag })
        {
            return;
        }

        if (Enum.TryParse(tag, out TitleAlignment alignment))
        {
            host.AppTitleBar.TitleAlignment = alignment;
        }
    }

    private void OnShowIconChanged(object sender, RoutedEventArgs e)
    {
        if (Host is { } host && sender is ToggleSwitch toggle)
        {
            host.AppTitleBar.ShowIcon = toggle.IsChecked == true;
        }
    }

    private void OnCanMaximizeChanged(object sender, RoutedEventArgs e)
    {
        if (Host is { } host && sender is ToggleSwitch toggle)
        {
            host.AppTitleBar.CanMaximize = toggle.IsChecked == true;
        }
    }

    private void OnTogglePaneClick(object sender, RoutedEventArgs e)
    {
        Host?.Nav.TogglePane();
    }
}

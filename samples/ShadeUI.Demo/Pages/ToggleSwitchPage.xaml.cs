using System.Windows.Controls;

namespace ShadeUI.Demo.Pages;

public partial class ToggleSwitchPage : UserControl
{
    public ToggleSwitchPage()
    {
        InitializeComponent();

        Ex1.Xaml = """
            <ui:ToggleSwitch Content="Bildirimler" />
            <ui:ToggleSwitch Content="Otomatik güncelleme"
                             IsChecked="True" />
            """;

        Ex2.Xaml = """
            <ui:ToggleSwitch OnContent="Açık"
                             OffContent="Kapalı"
                             IsChecked="True" />
            """;

        Ex3.Xaml = """
            <ui:ToggleSwitch Content="Devre dışı" IsEnabled="False" />
            <ui:ToggleSwitch Content="Devre dışı, açık"
                             IsChecked="True"
                             IsEnabled="False" />
            """;

        Ex4.Xaml = """
            <ToggleButton Content="Damla anahtarı"
                          Style="{StaticResource DropletToggleSwitchStyle}" />
            """;
    }
}

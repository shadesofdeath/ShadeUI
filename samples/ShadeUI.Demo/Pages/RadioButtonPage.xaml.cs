using System.Windows.Controls;

namespace ShadeUI.Demo.Pages;

public partial class RadioButtonPage : UserControl
{
    public RadioButtonPage()
    {
        InitializeComponent();

        Ex1.Xaml = """
            <RadioButton Content="Sistemi takip et"
                         GroupName="Tema"
                         IsChecked="True" />
            <RadioButton Content="Her zaman açık" GroupName="Tema" />
            <RadioButton Content="Her zaman koyu" GroupName="Tema" />
            """;

        Ex2.Xaml = """
            <RadioButton Content="Devre dışı"
                         GroupName="Kapalı"
                         IsEnabled="False" />
            <RadioButton Content="Devre dışı, seçili"
                         GroupName="Kapalı"
                         IsChecked="True"
                         IsEnabled="False" />
            """;
    }
}

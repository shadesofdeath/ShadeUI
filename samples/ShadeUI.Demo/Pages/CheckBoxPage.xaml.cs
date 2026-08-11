using System.Windows.Controls;

namespace ShadeUI.Demo.Pages;

public partial class CheckBoxPage : UserControl
{
    public CheckBoxPage()
    {
        InitializeComponent();

        Ex1.Xaml = """
            <CheckBox Content="İşaretsiz" />
            <CheckBox Content="İşaretli" IsChecked="True" />
            """;

        Ex2.Xaml = """
            <CheckBox Content="Belirsiz"
                      IsThreeState="True"
                      IsChecked="{x:Null}" />
            """;

        Ex3.Xaml = """
            <CheckBox Content="Devre dışı" IsEnabled="False" />
            <CheckBox Content="Devre dışı, işaretli"
                      IsChecked="True"
                      IsEnabled="False" />
            """;

        Ex4.Xaml = """
            <CheckBox Content="Klasik, işaretli"
                      IsChecked="True"
                      Style="{StaticResource ClassicCheckBoxStyle}" />
            """;
    }
}

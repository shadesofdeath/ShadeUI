using System.Windows.Controls;

namespace ShadeUI.Demo.Pages;

public partial class TextBoxPage : UserControl
{
    public TextBoxPage()
    {
        InitializeComponent();

        Ex1.Xaml = """
            <ui:TextBox Width="240"
                        PlaceholderText="Adınızı yazın" />
            """;

        Ex2.Xaml = """
            <ui:TextBox Width="200"
                        Icon="&#xE721;"
                        PlaceholderText="Ara" />

            <ui:TextBox Width="220"
                        Icon="&#xE715;"
                        IconPlacement="Right"
                        Text="ornek@shadeui.dev" />
            """;

        Ex3.Xaml = """
            <ui:TextBox Width="240"
                        Text="Odaklanınca × belirir"
                        ClearButtonEnabled="True" />
            """;

        Ex4.Xaml = """
            <ui:TextBox IsReadOnly="True" Text="Salt okunur" />
            <ui:TextBox IsEnabled="False" Text="Devre dışı" />

            <ui:TextBox Height="72"
                        AcceptsReturn="True"
                        ClearButtonEnabled="False"
                        PlaceholderText="Notlarınızı buraya yazabilirsiniz..."
                        TextWrapping="Wrap"
                        VerticalScrollBarVisibility="Auto" />
            """;
    }
}

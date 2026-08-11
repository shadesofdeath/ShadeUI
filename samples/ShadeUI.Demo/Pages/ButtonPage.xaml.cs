using System.Windows.Controls;

namespace ShadeUI.Demo.Pages;

public partial class ButtonPage : UserControl
{
    public ButtonPage()
    {
        InitializeComponent();

        // Raw string literals keep the snippets formatted exactly as they read here,
        // with no XML escaping — XAML property elements cannot preserve line breaks.
        Ex1.Xaml = """
            <Button Content="Varsayılan" />

            <Button Content="Vurgulu"
                    Style="{StaticResource AccentButtonStyle}" />
            """;

        Ex2.Xaml = """
            <Button Content="Varsayılan" IsEnabled="False" />

            <Button Content="Vurgulu"
                    IsEnabled="False"
                    Style="{StaticResource AccentButtonStyle}" />
            """;

        Ex3.Xaml = """
            <Button>
                <StackPanel Orientation="Horizontal">
                    <TextBlock FontFamily="{DynamicResource ShadeIconFontFamily}"
                               FontSize="12"
                               Text="&#xE710;"
                               VerticalAlignment="Center" />
                    <TextBlock Margin="6,0,0,0"
                               Text="Yeni"
                               VerticalAlignment="Center" />
                </StackPanel>
            </Button>
            """;
    }
}

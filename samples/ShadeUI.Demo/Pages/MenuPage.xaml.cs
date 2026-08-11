using System.Windows.Controls;

namespace ShadeUI.Demo.Pages;

public partial class MenuPage : UserControl
{
    public MenuPage()
    {
        InitializeComponent();

        Ex1.Xaml = """
            <Button Content="Sağ tıkla">
                <Button.ContextMenu>
                    <ContextMenu>
                        <MenuItem Header="Kes" InputGestureText="Ctrl+X" />
                        <MenuItem Header="Kopyala" InputGestureText="Ctrl+C" />
                        <Separator />
                        <MenuItem Header="Sabitlenmiş" IsCheckable="True" IsChecked="True" />
                        <MenuItem Header="Daha fazla">
                            <MenuItem Header="Dışa aktar" />
                        </MenuItem>
                    </ContextMenu>
                </Button.ContextMenu>
            </Button>
            """;

        Ex2.Xaml = """
            <Menu>
                <MenuItem Header="_Dosya">
                    <MenuItem Header="Yeni" InputGestureText="Ctrl+N" />
                    <Separator />
                    <MenuItem Header="Çıkış" />
                </MenuItem>
            </Menu>
            """;

        Ex3.Xaml = """
            <Button Content="Üzerine gel"
                    ToolTip="ToolTip de aynı flyout yüzeyini kullanıyor." />
            """;
    }
}

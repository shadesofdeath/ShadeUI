using System.Windows.Controls;

namespace ShadeUI.Demo.Pages;

public partial class ComboBoxPage : UserControl
{
    public ComboBoxPage()
    {
        LongList = Enumerable.Range(1, 30).Select(i => $"Öğe {i}").ToList();
        InitializeComponent();

        Ex1.Xaml = """
            <ComboBox Width="240" SelectedIndex="0">
                <ComboBoxItem Content="Sistemi takip et" />
                <ComboBoxItem Content="Açık tema" />
                <ComboBoxItem Content="Koyu tema" />
            </ComboBox>
            """;

        Ex2.Xaml = """
            <ComboBox Width="240"
                      IsEditable="True"
                      Text="Segoe UI Variable">
                <ComboBoxItem Content="Segoe UI Variable" />
                <ComboBoxItem Content="Cascadia Code" />
            </ComboBox>
            """;

        Ex3.Xaml = """
            <ComboBox IsEnabled="False" SelectedIndex="0">
                <ComboBoxItem Content="Değiştirilemez" />
            </ComboBox>

            <ComboBox ItemsSource="{Binding LongList}"
                      MaxDropDownHeight="260"
                      SelectedIndex="0" />
            """;
    }

    /// <summary>Sample data for the scrolling drop-down example.</summary>
    public IReadOnlyList<string> LongList { get; }
}

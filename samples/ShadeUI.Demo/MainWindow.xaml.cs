using System.Windows.Controls;
using ShadeUI.Controls;
using ShadeUI.Demo.Pages;

namespace ShadeUI.Demo;

public partial class MainWindow : ShadeWindow
{
    private readonly Dictionary<int, UserControl> _pages = new();

    public MainWindow()
    {
        InitializeComponent();
        NavigateTo(0);
    }

    private void OnNavSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        NavigateTo(Nav.SelectedIndex);
    }

    private void NavigateTo(int index)
    {
        if (PageHost is null || index < 0)
        {
            return;
        }

        if (!_pages.TryGetValue(index, out UserControl? page))
        {
            page = index switch
            {
                1 => new ControlsPage(),
                2 => new ThemePage(),
                3 => new SettingsPage(),
                _ => new HomePage(),
            };

            _pages[index] = page;
        }

        PageHost.Content = page;
    }
}

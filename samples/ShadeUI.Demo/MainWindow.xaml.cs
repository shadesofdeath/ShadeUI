using System.Windows.Controls;
using ShadeUI.Controls;
using ShadeUI.Demo.Pages;
using ShadeWindow = ShadeUI.Controls.ShadeWindow;

namespace ShadeUI.Demo;

public partial class MainWindow : ShadeWindow
{
    private readonly Dictionary<int, UserControl> _pages = new();

    public MainWindow()
    {
        InitializeComponent();
        Nav.SelectItem(Nav.MenuItems[0]);
    }

    private void OnNavSelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (e.SelectedItem is not NavigationViewItem { Tag: string tag } || !int.TryParse(tag, out int index))
        {
            return;
        }

        NavigateTo(index);
    }

    private void NavigateTo(int index)
    {
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

        Nav.Content = page;
    }
}

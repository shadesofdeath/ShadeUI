using System.Windows.Controls;
using ShadeUI.Controls;
using ShadeUI.Demo.Pages;
using ShadeWindow = ShadeUI.Controls.ShadeWindow;

namespace ShadeUI.Demo;

public partial class MainWindow : ShadeWindow
{
    private readonly Dictionary<string, UserControl> _pages = new();

    public MainWindow()
    {
        InitializeComponent();
        Nav.SelectItem(Nav.MenuItems[0]);
    }

    private void OnNavSelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (e.SelectedItem is NavigationViewItem { Tag: string tag })
        {
            NavigateTo(tag);
        }
    }

    private void NavigateTo(string tag)
    {
        if (!_pages.TryGetValue(tag, out UserControl? page))
        {
            page = CreatePage(tag);
            _pages[tag] = page;
        }

        Nav.Content = page;
    }

    private static UserControl CreatePage(string tag) => tag switch
    {
        "button" => new ButtonPage(),
        "typingtext" => new TypingTextPage(),
        "textbox" => new TextBoxPage(),
        "combobox" => new ComboBoxPage(),
        "checkbox" => new CheckBoxPage(),
        "radiobutton" => new RadioButtonPage(),
        "toggleswitch" => new ToggleSwitchPage(),
        "progressbar" => new ProgressBarPage(),
        "progressring" => new ProgressRingPage(),
        "menu" => new MenuPage(),
        "shell" => new ShellPage(),
        "theme" => new ThemePage(),
        "settings" => new SettingsPage(),
        _ => new HomePage(),
    };
}

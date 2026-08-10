using System.Windows;
using System.Windows.Controls;
using ShadeUI.Appearance;

namespace ShadeUI.Demo.Pages;

public partial class ThemePage : UserControl
{
    public ThemePage()
    {
        InitializeComponent();

        ThemeManager.ThemeChanged += OnThemeChanged;
        Unloaded += (_, _) => ThemeManager.ThemeChanged -= OnThemeChanged;

        UpdateThemeText();
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        UpdateThemeText();
    }

    private void UpdateThemeText()
    {
        string requested = ThemeManager.RequestedTheme switch
        {
            ApplicationTheme.Light => "Açık",
            ApplicationTheme.Dark => "Koyu",
            _ => "Sistemi takip et",
        };

        string actual = ThemeManager.ActualTheme == ApplicationTheme.Dark ? "koyu" : "açık";

        CurrentThemeText.Text = $"Seçili mod: {requested} — şu an {actual} palet etkin.";
    }

    private void OnLightClick(object sender, RoutedEventArgs e)
    {
        ThemeManager.Apply(ApplicationTheme.Light);
    }

    private void OnDarkClick(object sender, RoutedEventArgs e)
    {
        ThemeManager.Apply(ApplicationTheme.Dark);
    }

    private void OnSystemClick(object sender, RoutedEventArgs e)
    {
        ThemeManager.Apply(ApplicationTheme.System);
    }
}

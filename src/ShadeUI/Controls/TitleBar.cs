using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShadeUI.Controls;

/// <summary>
/// The ShadeUI title bar: application icon, title, an optional trailing content slot
/// and the caption buttons. Place it at the top of a <see cref="ShadeWindow"/>.
/// </summary>
[TemplatePart(Name = MinimizeButtonPart, Type = typeof(TitleBarButton))]
[TemplatePart(Name = MaximizeButtonPart, Type = typeof(TitleBarButton))]
[TemplatePart(Name = CloseButtonPart, Type = typeof(TitleBarButton))]
public class TitleBar : Control
{
    private const string MinimizeButtonPart = "PART_MinimizeButton";
    private const string MaximizeButtonPart = "PART_MaximizeButton";
    private const string CloseButtonPart = "PART_CloseButton";

    private TitleBarButton? _minimizeButton;
    private TitleBarButton? _maximizeButton;
    private TitleBarButton? _closeButton;

    static TitleBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TitleBar),
            new FrameworkPropertyMetadata(typeof(TitleBar)));
    }

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(TitleBar), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(ImageSource), typeof(TitleBar), new PropertyMetadata(null));

    public static readonly DependencyProperty TrailingContentProperty = DependencyProperty.Register(
        nameof(TrailingContent), typeof(object), typeof(TitleBar), new PropertyMetadata(null));

    public static readonly DependencyProperty ShowMinimizeProperty = DependencyProperty.Register(
        nameof(ShowMinimize), typeof(bool), typeof(TitleBar), new PropertyMetadata(true));

    public static readonly DependencyProperty ShowMaximizeProperty = DependencyProperty.Register(
        nameof(ShowMaximize), typeof(bool), typeof(TitleBar), new PropertyMetadata(true));

    public static readonly DependencyProperty ShowCloseProperty = DependencyProperty.Register(
        nameof(ShowClose), typeof(bool), typeof(TitleBar), new PropertyMetadata(true));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public ImageSource? Icon
    {
        get => (ImageSource?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Extra content rendered just before the caption buttons (e.g. a theme toggle).</summary>
    public object? TrailingContent
    {
        get => GetValue(TrailingContentProperty);
        set => SetValue(TrailingContentProperty, value);
    }

    public bool ShowMinimize
    {
        get => (bool)GetValue(ShowMinimizeProperty);
        set => SetValue(ShowMinimizeProperty, value);
    }

    public bool ShowMaximize
    {
        get => (bool)GetValue(ShowMaximizeProperty);
        set => SetValue(ShowMaximizeProperty, value);
    }

    public bool ShowClose
    {
        get => (bool)GetValue(ShowCloseProperty);
        set => SetValue(ShowCloseProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_minimizeButton is not null)
        {
            _minimizeButton.Click -= OnMinimizeClick;
        }

        if (_maximizeButton is not null)
        {
            _maximizeButton.Click -= OnMaximizeClick;
        }

        if (_closeButton is not null)
        {
            _closeButton.Click -= OnCloseClick;
        }

        _minimizeButton = GetTemplateChild(MinimizeButtonPart) as TitleBarButton;
        _maximizeButton = GetTemplateChild(MaximizeButtonPart) as TitleBarButton;
        _closeButton = GetTemplateChild(CloseButtonPart) as TitleBarButton;

        if (_minimizeButton is not null)
        {
            _minimizeButton.Click += OnMinimizeClick;
        }

        if (_maximizeButton is not null)
        {
            _maximizeButton.Click += OnMaximizeClick;
        }

        if (_closeButton is not null)
        {
            _closeButton.Click += OnCloseClick;
        }
    }

    private void OnMinimizeClick(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is { } window)
        {
            window.WindowState = WindowState.Minimized;
        }
    }

    private void OnMaximizeClick(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is { } window)
        {
            window.WindowState = window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Window.GetWindow(this)?.Close();
    }
}

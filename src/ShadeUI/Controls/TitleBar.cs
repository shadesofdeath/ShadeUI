using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using ShadeUI.Interop;

namespace ShadeUI.Controls;

/// <summary>Where <see cref="TitleBar.Title"/> sits inside the bar.</summary>
public enum TitleAlignment
{
    /// <summary>After the icon and leading content. The default.</summary>
    Left,

    /// <summary>Centred on the window, independent of the icon and caption buttons.</summary>
    Center,

    /// <summary>Pushed against the trailing content.</summary>
    Right,
}

/// <summary>What the close button does.</summary>
public enum TitleBarCloseAction
{
    /// <summary>Closes the window. The default.</summary>
    Close,

    /// <summary>Hides the window instead of closing it — for apps that live in the tray.</summary>
    Hide,
}

/// <summary>How the title bar is currently drawing its icon.</summary>
public enum TitleBarIconMode
{
    None,
    Image,
    Glyph,
}

/// <summary>
/// The ShadeUI title bar: application icon, title, leading and trailing content slots
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

    private Window? _window;
    private HwndSource? _source;

    static TitleBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TitleBar),
            new FrameworkPropertyMetadata(typeof(TitleBar)));
    }

    public TitleBar()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(TitleBar), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty TitleAlignmentProperty = DependencyProperty.Register(
        nameof(TitleAlignment), typeof(TitleAlignment), typeof(TitleBar),
        new PropertyMetadata(TitleAlignment.Left));

    public static readonly DependencyProperty TitleFontSizeProperty = DependencyProperty.Register(
        nameof(TitleFontSize), typeof(double), typeof(TitleBar), new PropertyMetadata(12d));

    public static readonly DependencyProperty TitleFontWeightProperty = DependencyProperty.Register(
        nameof(TitleFontWeight), typeof(FontWeight), typeof(TitleBar),
        new PropertyMetadata(FontWeights.Normal));

    public static readonly DependencyProperty TitleForegroundProperty = DependencyProperty.Register(
        nameof(TitleForeground), typeof(Brush), typeof(TitleBar), new PropertyMetadata(null));

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(ImageSource), typeof(TitleBar),
        new PropertyMetadata(null, OnIconSourceChanged));

    public static readonly DependencyProperty IconGlyphProperty = DependencyProperty.Register(
        nameof(IconGlyph), typeof(string), typeof(TitleBar),
        new PropertyMetadata(null, OnIconSourceChanged));

    public static readonly DependencyProperty ShowIconProperty = DependencyProperty.Register(
        nameof(ShowIcon), typeof(bool), typeof(TitleBar),
        new PropertyMetadata(true, OnIconSourceChanged));

    private static readonly DependencyPropertyKey IconModePropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(IconMode), typeof(TitleBarIconMode), typeof(TitleBar),
        new PropertyMetadata(TitleBarIconMode.None));

    /// <summary>Identifies the read-only <see cref="IconMode"/> property.</summary>
    public static readonly DependencyProperty IconModeProperty = IconModePropertyKey.DependencyProperty;

    public static readonly DependencyProperty LeadingContentProperty = DependencyProperty.Register(
        nameof(LeadingContent), typeof(object), typeof(TitleBar), new PropertyMetadata(null));

    public static readonly DependencyProperty TrailingContentProperty = DependencyProperty.Register(
        nameof(TrailingContent), typeof(object), typeof(TitleBar), new PropertyMetadata(null));

    public static readonly DependencyProperty ShowMinimizeProperty = DependencyProperty.Register(
        nameof(ShowMinimize), typeof(bool), typeof(TitleBar), new PropertyMetadata(true));

    public static readonly DependencyProperty ShowMaximizeProperty = DependencyProperty.Register(
        nameof(ShowMaximize), typeof(bool), typeof(TitleBar), new PropertyMetadata(true));

    public static readonly DependencyProperty ShowCloseProperty = DependencyProperty.Register(
        nameof(ShowClose), typeof(bool), typeof(TitleBar), new PropertyMetadata(true));

    public static readonly DependencyProperty CanMaximizeProperty = DependencyProperty.Register(
        nameof(CanMaximize), typeof(bool), typeof(TitleBar), new PropertyMetadata(true));

    public static readonly DependencyProperty CloseButtonActionProperty = DependencyProperty.Register(
        nameof(CloseButtonAction), typeof(TitleBarCloseAction), typeof(TitleBar),
        new PropertyMetadata(TitleBarCloseAction.Close));

    public static readonly DependencyProperty IsSnapLayoutEnabledProperty = DependencyProperty.Register(
        nameof(IsSnapLayoutEnabled), typeof(bool), typeof(TitleBar), new PropertyMetadata(true));

    public static readonly DependencyProperty IsSystemMenuEnabledProperty = DependencyProperty.Register(
        nameof(IsSystemMenuEnabled), typeof(bool), typeof(TitleBar), new PropertyMetadata(true));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Where the title sits. <see cref="Controls.TitleAlignment.Center"/> centres it on the window.</summary>
    public TitleAlignment TitleAlignment
    {
        get => (TitleAlignment)GetValue(TitleAlignmentProperty);
        set => SetValue(TitleAlignmentProperty, value);
    }

    public double TitleFontSize
    {
        get => (double)GetValue(TitleFontSizeProperty);
        set => SetValue(TitleFontSizeProperty, value);
    }

    public FontWeight TitleFontWeight
    {
        get => (FontWeight)GetValue(TitleFontWeightProperty);
        set => SetValue(TitleFontWeightProperty, value);
    }

    /// <summary>Overrides the title colour. Falls back to <c>TextFillColorPrimaryBrush</c> when unset.</summary>
    public Brush? TitleForeground
    {
        get => (Brush?)GetValue(TitleForegroundProperty);
        set => SetValue(TitleForegroundProperty, value);
    }

    /// <summary>Bitmap icon. Takes precedence over <see cref="IconGlyph"/>.</summary>
    public ImageSource? Icon
    {
        get => (ImageSource?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Segoe Fluent Icons glyph to use when no <see cref="Icon"/> image is set.</summary>
    public string? IconGlyph
    {
        get => (string?)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    /// <summary>Whether any icon is rendered at all. Defaults to <see langword="true"/>.</summary>
    public bool ShowIcon
    {
        get => (bool)GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    /// <summary>Which icon the bar is currently showing. Resolved from <see cref="Icon"/>, <see cref="IconGlyph"/> and <see cref="ShowIcon"/>.</summary>
    public TitleBarIconMode IconMode => (TitleBarIconMode)GetValue(IconModeProperty);

    /// <summary>Content between the icon and the title (menu button, breadcrumb, tabs...).</summary>
    public object? LeadingContent
    {
        get => GetValue(LeadingContentProperty);
        set => SetValue(LeadingContentProperty, value);
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

    /// <summary>
    /// When <see langword="false"/> the maximize button is disabled, Snap Layouts is suppressed
    /// and double-clicking the bar no longer maximizes the window.
    /// </summary>
    public bool CanMaximize
    {
        get => (bool)GetValue(CanMaximizeProperty);
        set => SetValue(CanMaximizeProperty, value);
    }

    /// <summary>Whether the close button closes or merely hides the window.</summary>
    public TitleBarCloseAction CloseButtonAction
    {
        get => (TitleBarCloseAction)GetValue(CloseButtonActionProperty);
        set => SetValue(CloseButtonActionProperty, value);
    }

    /// <summary>Whether hovering the maximize button opens the Windows 11 Snap Layouts flyout.</summary>
    public bool IsSnapLayoutEnabled
    {
        get => (bool)GetValue(IsSnapLayoutEnabledProperty);
        set => SetValue(IsSnapLayoutEnabledProperty, value);
    }

    /// <summary>Whether right-clicking the bar opens the window's system menu.</summary>
    public bool IsSystemMenuEnabled
    {
        get => (bool)GetValue(IsSystemMenuEnabledProperty);
        set => SetValue(IsSystemMenuEnabledProperty, value);
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

        UpdateIconMode();
    }

    private static void OnIconSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((TitleBar)d).UpdateIconMode();
    }

    private void UpdateIconMode()
    {
        TitleBarIconMode mode = TitleBarIconMode.None;

        if (ShowIcon)
        {
            if (Icon is not null)
            {
                mode = TitleBarIconMode.Image;
            }
            else if (!string.IsNullOrEmpty(IconGlyph))
            {
                mode = TitleBarIconMode.Glyph;
            }
        }

        SetValue(IconModePropertyKey, mode);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _window = Window.GetWindow(this);
        if (_window is null)
        {
            return;
        }

        _source = PresentationSource.FromVisual(_window) as HwndSource;
        _source?.AddHook(WndProc);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _source?.RemoveHook(WndProc);
        _source = null;
        _window = null;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (msg)
        {
            case NonClient.WmNcHitTest:
                if (IsOverMaximizeButton(lParam))
                {
                    SetMaximizeVisualState(hover: true, press: false);
                    handled = true;
                    return new IntPtr(NonClient.HtMaxButton);
                }

                SetMaximizeVisualState(hover: false, press: false);
                break;

            case NonClient.WmNcMouseLeave:
                SetMaximizeVisualState(hover: false, press: false);
                break;

            case NonClient.WmNcLButtonDown:
                if (wParam.ToInt32() == NonClient.HtMaxButton)
                {
                    SetMaximizeVisualState(hover: true, press: true);
                    handled = true;
                    return IntPtr.Zero;
                }

                break;

            case NonClient.WmNcLButtonUp:
                if (wParam.ToInt32() == NonClient.HtMaxButton)
                {
                    SetMaximizeVisualState(hover: true, press: false);
                    ToggleMaximize();
                    handled = true;
                    return IntPtr.Zero;
                }

                break;

            case NonClient.WmNcLButtonDblClk:
                if (!CanMaximize && wParam.ToInt32() == NonClient.HtCaption)
                {
                    handled = true;
                    return IntPtr.Zero;
                }

                break;

            case NonClient.WmNcRButtonUp:
                if (IsSystemMenuEnabled &&
                    wParam.ToInt32() is NonClient.HtCaption or NonClient.HtSysMenu)
                {
                    ShowSystemMenu(hwnd, NonClient.GetX(lParam), NonClient.GetY(lParam));
                    handled = true;
                    return IntPtr.Zero;
                }

                break;
        }

        return IntPtr.Zero;
    }

    private bool IsOverMaximizeButton(IntPtr lParam)
    {
        if (_maximizeButton is null || !IsSnapLayoutEnabled || !ShowMaximize || !CanMaximize)
        {
            return false;
        }

        if (_maximizeButton.ActualWidth <= 0 || _maximizeButton.ActualHeight <= 0)
        {
            return false;
        }

        try
        {
            Point p = _maximizeButton.PointFromScreen(
                new Point(NonClient.GetX(lParam), NonClient.GetY(lParam)));

            return p.X >= 0 && p.X <= _maximizeButton.ActualWidth
                && p.Y >= 0 && p.Y <= _maximizeButton.ActualHeight;
        }
        catch (InvalidOperationException)
        {
            // No presentation source yet.
            return false;
        }
    }

    private void SetMaximizeVisualState(bool hover, bool press)
    {
        if (_maximizeButton is null)
        {
            return;
        }

        _maximizeButton.IsSimulatedHover = hover;
        _maximizeButton.IsSimulatedPress = press;
    }

    private void ShowSystemMenu(IntPtr hwnd, int screenX, int screenY)
    {
        Window? window = _window ?? Window.GetWindow(this);
        if (window is null)
        {
            return;
        }

        bool maximized = window.WindowState == WindowState.Maximized;
        bool resizable = window.ResizeMode is ResizeMode.CanResize or ResizeMode.CanResizeWithGrip;

        var state = new NonClient.SystemMenuState(
            CanRestore: maximized,
            CanMove: !maximized,
            CanSize: !maximized && resizable,
            CanMinimize: ShowMinimize && window.ResizeMode != ResizeMode.NoResize,
            CanMaximize: !maximized && CanMaximize && resizable,
            CanClose: ShowClose);

        NonClient.ShowSystemMenu(hwnd, screenX, screenY, state);
    }

    private void ToggleMaximize()
    {
        if (!CanMaximize || Window.GetWindow(this) is not { } window)
        {
            return;
        }

        window.WindowState = window.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
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
        ToggleMaximize();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is not { } window)
        {
            return;
        }

        if (CloseButtonAction == TitleBarCloseAction.Hide)
        {
            window.Hide();
            return;
        }

        window.Close();
    }
}

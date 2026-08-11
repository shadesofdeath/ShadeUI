using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using ShadeUI.Appearance;
using ShadeUI.Interop;

namespace ShadeUI.Controls;

/// <summary>
/// A <see cref="Window"/> with ShadeUI chrome: custom title bar area, themed surfaces,
/// dark-mode aware non-client parts, a themed 1 px border and rounded corners on Windows 11.
/// </summary>
public class ShadeWindow : Window
{
    private const string RootPartName = "PART_Root";
    private const int CloseFadeSteps = 8;
    private static readonly TimeSpan CloseFadeInterval = TimeSpan.FromMilliseconds(14);

    private FrameworkElement? _root;
    private TranslateTransform? _rootSlide;
    private bool _isFadingOut;

    static ShadeWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ShadeWindow),
            new FrameworkPropertyMetadata(typeof(ShadeWindow)));
    }

    public ShadeWindow()
    {
        ThemeManager.ThemeChanged += OnThemeChanged;
        Loaded += OnWindowLoaded;
    }

    public static readonly DependencyProperty EnableTransitionsProperty = DependencyProperty.Register(
        nameof(EnableTransitions),
        typeof(bool),
        typeof(ShadeWindow),
        new PropertyMetadata(true));

    /// <summary>
    /// Whether the window plays its open and close animations. Defaults to <see langword="true"/>.
    /// </summary>
    public bool EnableTransitions
    {
        get => (bool)GetValue(EnableTransitionsProperty);
        set => SetValue(EnableTransitionsProperty, value);
    }

    public static readonly DependencyProperty BackdropProperty = DependencyProperty.Register(
        nameof(Backdrop),
        typeof(WindowBackdropType),
        typeof(ShadeWindow),
        new PropertyMetadata(WindowBackdropType.Solid, OnBackdropChanged));

    private static readonly DependencyPropertyKey ActualBackdropPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(ActualBackdrop),
        typeof(WindowBackdropType),
        typeof(ShadeWindow),
        new PropertyMetadata(WindowBackdropType.Solid));

    /// <summary>Identifies the read-only <see cref="ActualBackdrop"/> property.</summary>
    public static readonly DependencyProperty ActualBackdropProperty = ActualBackdropPropertyKey.DependencyProperty;

    /// <summary>
    /// The system material requested behind the window. Defaults to
    /// <see cref="WindowBackdropType.Solid"/>.
    /// </summary>
    public WindowBackdropType Backdrop
    {
        get => (WindowBackdropType)GetValue(BackdropProperty);
        set => SetValue(BackdropProperty, value);
    }

    /// <summary>
    /// The material actually in effect. Equals <see cref="Backdrop"/> when the compositor
    /// accepted it, otherwise <see cref="WindowBackdropType.Solid"/>. The window style keys
    /// its background off this, so an unsupported request still renders correctly.
    /// </summary>
    public WindowBackdropType ActualBackdrop => (WindowBackdropType)GetValue(ActualBackdropProperty);

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _root = GetTemplateChild(RootPartName) as FrameworkElement;

        if (_root is not null)
        {
            // Freezables declared inside a ControlTemplate come back frozen, so the
            // transform we animate has to be created here.
            _rootSlide = new TranslateTransform();
            _root.RenderTransform = _rootSlide;
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // A system backdrop is only visible through the WPF render surface if the
        // composition target itself is transparent. The template root paints an opaque
        // background in Solid mode, so this is safe to set unconditionally.
        if (PresentationSource.FromVisual(this) is HwndSource { CompositionTarget: { } target })
        {
            target.BackgroundColor = Colors.Transparent;
        }

        ApplyChromeAttributes();
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        if (!EnableTransitions || _root is null)
        {
            return;
        }

        _root.BeginAnimation(
            OpacityProperty,
            new DoubleAnimation(0d, 1d, new Duration(TimeSpan.FromMilliseconds(400))));

        _rootSlide?.BeginAnimation(
            TranslateTransform.YProperty,
            new DoubleAnimation(12d, 0d, new Duration(TimeSpan.FromMilliseconds(900)))
            {
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut },
            });
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        if (e.Cancel || _isFadingOut || !EnableTransitions)
        {
            return;
        }

        IntPtr hwnd = new WindowInteropHelper(this).Handle;
        if (!LayeredWindow.Enable(hwnd))
        {
            return;
        }

        e.Cancel = true;
        _isFadingOut = true;
        FadeOutThenClose(hwnd);
    }

    protected override void OnClosed(EventArgs e)
    {
        ThemeManager.ThemeChanged -= OnThemeChanged;
        base.OnClosed(e);
    }

    private void FadeOutThenClose(IntPtr hwnd)
    {
        int step = 0;

        var timer = new DispatcherTimer(DispatcherPriority.Render) { Interval = CloseFadeInterval };
        timer.Tick += (_, _) =>
        {
            step++;

            if (step >= CloseFadeSteps)
            {
                timer.Stop();
                Close();
                return;
            }

            LayeredWindow.SetAlpha(hwnd, (byte)(255 - 255 * step / CloseFadeSteps));
        };

        timer.Start();
    }

    private static void OnBackdropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((ShadeWindow)d).ApplyBackdrop();
    }

    private void ApplyBackdrop()
    {
        IntPtr hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        WindowBackdropType requested = Backdrop;
        bool applied = Dwm.SetSystemBackdrop(hwnd, requested);

        SetValue(
            ActualBackdropPropertyKey,
            applied && requested != WindowBackdropType.Solid ? requested : WindowBackdropType.Solid);
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        ApplyChromeAttributes();
    }

    private void ApplyChromeAttributes()
    {
        IntPtr hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        Dwm.SetImmersiveDarkMode(hwnd, ThemeManager.ActualTheme == ApplicationTheme.Dark);
        Dwm.SetRoundedCorners(hwnd);

        if (TryFindResource("WindowBorderColor") is Color borderColor)
        {
            Dwm.SetBorderColor(hwnd, borderColor);
        }

        // The dark-mode flag changes how the compositor tints the material, so the
        // backdrop has to be re-applied after a theme switch.
        ApplyBackdrop();
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ShadeUI.Controls;

/// <summary>
/// A <see cref="ContentControl"/> that fades and slides its content in whenever
/// <see cref="ContentControl.Content"/> changes — the same motion the window uses when it opens.
/// <code language="xml">
/// &lt;ui:TransitionPresenter Content="{Binding CurrentPage}" /&gt;
/// </code>
/// </summary>
[TemplatePart(Name = PresenterPartName, Type = typeof(FrameworkElement))]
public class TransitionPresenter : ContentControl
{
    private const string PresenterPartName = "PART_Presenter";

    private FrameworkElement? _presenter;
    private TranslateTransform? _slide;

    static TransitionPresenter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TransitionPresenter),
            new FrameworkPropertyMetadata(typeof(TransitionPresenter)));
    }

    public static readonly DependencyProperty SlideOffsetProperty = DependencyProperty.Register(
        nameof(SlideOffset),
        typeof(double),
        typeof(TransitionPresenter),
        new PropertyMetadata(10d));

    public static readonly DependencyProperty IsTransitionEnabledProperty = DependencyProperty.Register(
        nameof(IsTransitionEnabled),
        typeof(bool),
        typeof(TransitionPresenter),
        new PropertyMetadata(true));

    /// <summary>How far, in pixels, new content slides up into place. Defaults to 10.</summary>
    public double SlideOffset
    {
        get => (double)GetValue(SlideOffsetProperty);
        set => SetValue(SlideOffsetProperty, value);
    }

    /// <summary>Whether content changes are animated. Defaults to <see langword="true"/>.</summary>
    public bool IsTransitionEnabled
    {
        get => (bool)GetValue(IsTransitionEnabledProperty);
        set => SetValue(IsTransitionEnabledProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _presenter = GetTemplateChild(PresenterPartName) as FrameworkElement;

        if (_presenter is not null)
        {
            // Freezables declared inside a ControlTemplate come back frozen, so the
            // transform we animate has to be created here.
            _slide = new TranslateTransform();
            _presenter.RenderTransform = _slide;
        }

        PlayTransition();
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        PlayTransition();
    }

    private void PlayTransition()
    {
        if (_presenter is null || !IsTransitionEnabled || Content is null)
        {
            return;
        }

        _presenter.BeginAnimation(
            OpacityProperty,
            new DoubleAnimation(0d, 1d, new Duration(TimeSpan.FromMilliseconds(180))));

        _slide?.BeginAnimation(
            TranslateTransform.YProperty,
            new DoubleAnimation(SlideOffset, 0d, new Duration(TimeSpan.FromMilliseconds(280)))
            {
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut },
            });
    }
}

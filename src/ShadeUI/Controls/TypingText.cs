using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ShadeUI.Controls;

/// <summary>Shape of the caret drawn after the typed text.</summary>
public enum TypingCursorStyle
{
    /// <summary>A thin vertical bar, like the default VS Code caret.</summary>
    Line,

    /// <summary>A filled block covering a character cell.</summary>
    Block,

    /// <summary>A low horizontal bar sitting on the baseline.</summary>
    Underscore,
}

/// <summary>
/// Which entry of the ShadeUI type ramp <see cref="TypingText"/> renders with.
/// This is the WPF counterpart of the web component's <c>as</c> prop: WPF cannot swap
/// its element type, so the semantic level is expressed as typography instead.
/// </summary>
public enum TypingTextStyle
{
    Caption,
    Body,
    BodyStrong,
    Subtitle,
    Title,
    TitleLarge,
    Display,
}

/// <summary>
/// A text element that types itself out one character at a time, optionally cycling
/// through a list of words.
/// <code language="xml">
/// &lt;ui:TypingText Loop="True" PauseDelay="1200"&gt;
///     &lt;ui:TypingText.Words&gt;
///         &lt;sys:String&gt;tema kütüphanesi&lt;/sys:String&gt;
///         &lt;sys:String&gt;kontrol seti&lt;/sys:String&gt;
///     &lt;/ui:TypingText.Words&gt;
/// &lt;/ui:TypingText&gt;
/// </code>
/// </summary>
public class TypingText : Control
{
    private enum Phase
    {
        Idle,
        Delaying,
        Typing,
        Pausing,
        Deleting,
        Done,
    }

    private readonly DispatcherTimer _timer;

    private Phase _phase = Phase.Idle;
    private int _wordIndex;
    private int _charCount;
    private bool _hasStarted;
    private ScrollViewer? _scrollHost;

    static TypingText()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TypingText),
            new FrameworkPropertyMetadata(typeof(TypingText)));
    }

    public TypingText()
    {
        SetValue(WordsPropertyKey, new ObservableCollection<string>());

        _timer = new DispatcherTimer(DispatcherPriority.Render);
        _timer.Tick += OnTick;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        IsVisibleChanged += OnIsVisibleChanged;
        SizeChanged += OnSizeChanged;

        UpdateCursorMetrics();
    }

    /// <summary>Raised once the animation finishes. Never raised while <see cref="Loop"/> is on.</summary>
    public event EventHandler? Completed;

    // ---------------------------------------------------------------- content

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(TypingText),
        new PropertyMetadata(string.Empty, OnSourceTextChanged));

    private static readonly DependencyPropertyKey WordsPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Words), typeof(ObservableCollection<string>), typeof(TypingText),
        new PropertyMetadata(null));

    /// <summary>Identifies the read-only <see cref="Words"/> property.</summary>
    public static readonly DependencyProperty WordsProperty = WordsPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey DisplayTextPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(DisplayText), typeof(string), typeof(TypingText),
        new PropertyMetadata(string.Empty));

    /// <summary>Identifies the read-only <see cref="DisplayText"/> property.</summary>
    public static readonly DependencyProperty DisplayTextProperty = DisplayTextPropertyKey.DependencyProperty;

    /// <summary>The single string to type. Ignored when <see cref="Words"/> has entries.</summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Strings to type and delete in sequence. Takes precedence over <see cref="Text"/>.</summary>
    public ObservableCollection<string> Words => (ObservableCollection<string>)GetValue(WordsProperty);

    /// <summary>The portion of the current string that has been typed so far.</summary>
    public string DisplayText => (string)GetValue(DisplayTextProperty);

    // ---------------------------------------------------------------- timing

    public static readonly DependencyProperty DurationProperty = DependencyProperty.Register(
        nameof(Duration), typeof(double), typeof(TypingText), new PropertyMetadata(100d));

    public static readonly DependencyProperty TypeSpeedProperty = DependencyProperty.Register(
        nameof(TypeSpeed), typeof(double), typeof(TypingText), new PropertyMetadata(100d));

    public static readonly DependencyProperty DeleteSpeedProperty = DependencyProperty.Register(
        nameof(DeleteSpeed), typeof(double), typeof(TypingText), new PropertyMetadata(50d));

    public static readonly DependencyProperty DelayProperty = DependencyProperty.Register(
        nameof(Delay), typeof(double), typeof(TypingText), new PropertyMetadata(0d));

    public static readonly DependencyProperty PauseDelayProperty = DependencyProperty.Register(
        nameof(PauseDelay), typeof(double), typeof(TypingText), new PropertyMetadata(1000d));

    public static readonly DependencyProperty LoopProperty = DependencyProperty.Register(
        nameof(Loop), typeof(bool), typeof(TypingText), new PropertyMetadata(false));

    /// <summary>Milliseconds per character while typing a single <see cref="Text"/>. Defaults to 100.</summary>
    public double Duration
    {
        get => (double)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    /// <summary>Milliseconds per character while typing an entry from <see cref="Words"/>. Defaults to 100.</summary>
    public double TypeSpeed
    {
        get => (double)GetValue(TypeSpeedProperty);
        set => SetValue(TypeSpeedProperty, value);
    }

    /// <summary>Milliseconds per character while deleting. Defaults to 50.</summary>
    public double DeleteSpeed
    {
        get => (double)GetValue(DeleteSpeedProperty);
        set => SetValue(DeleteSpeedProperty, value);
    }

    /// <summary>Milliseconds to wait before the first character appears. Defaults to 0.</summary>
    public double Delay
    {
        get => (double)GetValue(DelayProperty);
        set => SetValue(DelayProperty, value);
    }

    /// <summary>Milliseconds to hold a finished word before deleting it. Defaults to 1000.</summary>
    public double PauseDelay
    {
        get => (double)GetValue(PauseDelayProperty);
        set => SetValue(PauseDelayProperty, value);
    }

    /// <summary>Whether the sequence restarts after the last entry. Defaults to <see langword="false"/>.</summary>
    public bool Loop
    {
        get => (bool)GetValue(LoopProperty);
        set => SetValue(LoopProperty, value);
    }

    // ---------------------------------------------------------------- appearance

    public static readonly DependencyProperty TextStyleProperty = DependencyProperty.Register(
        nameof(TextStyle), typeof(TypingTextStyle), typeof(TypingText),
        new PropertyMetadata(TypingTextStyle.Body));

    public static readonly DependencyProperty StartOnViewProperty = DependencyProperty.Register(
        nameof(StartOnView), typeof(bool), typeof(TypingText), new PropertyMetadata(true));

    public static readonly DependencyProperty ShowCursorProperty = DependencyProperty.Register(
        nameof(ShowCursor), typeof(bool), typeof(TypingText), new PropertyMetadata(true));

    public static readonly DependencyProperty BlinkCursorProperty = DependencyProperty.Register(
        nameof(BlinkCursor), typeof(bool), typeof(TypingText), new PropertyMetadata(true));

    public static readonly DependencyProperty CursorStyleProperty = DependencyProperty.Register(
        nameof(CursorStyle), typeof(TypingCursorStyle), typeof(TypingText),
        new PropertyMetadata(TypingCursorStyle.Line));

    public static readonly DependencyProperty CursorBrushProperty = DependencyProperty.Register(
        nameof(CursorBrush), typeof(Brush), typeof(TypingText), new PropertyMetadata(null));

    public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(
        nameof(TextWrapping), typeof(TextWrapping), typeof(TypingText),
        new PropertyMetadata(TextWrapping.NoWrap));

    /// <summary>Which entry of the type ramp to render with. The WPF counterpart of the web <c>as</c> prop.</summary>
    public TypingTextStyle TextStyle
    {
        get => (TypingTextStyle)GetValue(TextStyleProperty);
        set => SetValue(TextStyleProperty, value);
    }

    /// <summary>Whether typing waits until the control is scrolled into view. Defaults to <see langword="true"/>.</summary>
    public bool StartOnView
    {
        get => (bool)GetValue(StartOnViewProperty);
        set => SetValue(StartOnViewProperty, value);
    }

    public bool ShowCursor
    {
        get => (bool)GetValue(ShowCursorProperty);
        set => SetValue(ShowCursorProperty, value);
    }

    public bool BlinkCursor
    {
        get => (bool)GetValue(BlinkCursorProperty);
        set => SetValue(BlinkCursorProperty, value);
    }

    public TypingCursorStyle CursorStyle
    {
        get => (TypingCursorStyle)GetValue(CursorStyleProperty);
        set => SetValue(CursorStyleProperty, value);
    }

    /// <summary>Caret colour. Falls back to <see cref="Control.Foreground"/> when unset.</summary>
    public Brush? CursorBrush
    {
        get => (Brush?)GetValue(CursorBrushProperty);
        set => SetValue(CursorBrushProperty, value);
    }

    public TextWrapping TextWrapping
    {
        get => (TextWrapping)GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    // ------------------------------------------------- cursor metrics (template)

    private static readonly DependencyPropertyKey CursorWidthPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(CursorWidth), typeof(double), typeof(TypingText), new PropertyMetadata(6d));

    private static readonly DependencyPropertyKey CursorHeightPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(CursorHeight), typeof(double), typeof(TypingText), new PropertyMetadata(15d));

    private static readonly DependencyPropertyKey CursorThicknessPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(CursorThickness), typeof(double), typeof(TypingText), new PropertyMetadata(1d));

    public static readonly DependencyProperty CursorWidthProperty = CursorWidthPropertyKey.DependencyProperty;
    public static readonly DependencyProperty CursorHeightProperty = CursorHeightPropertyKey.DependencyProperty;
    public static readonly DependencyProperty CursorThicknessProperty = CursorThicknessPropertyKey.DependencyProperty;

    /// <summary>Caret cell width, derived from the font size. Used by the template.</summary>
    public double CursorWidth => (double)GetValue(CursorWidthProperty);

    /// <summary>Caret height, derived from the font size. Used by the template.</summary>
    public double CursorHeight => (double)GetValue(CursorHeightProperty);

    /// <summary>Caret bar thickness, derived from the font size. Used by the template.</summary>
    public double CursorThickness => (double)GetValue(CursorThicknessProperty);

    // ---------------------------------------------------------------- api

    /// <summary>Whether the animation is currently running.</summary>
    public bool IsRunning => _timer.IsEnabled;

    /// <summary>Restarts the animation from the first character.</summary>
    public void Start()
    {
        _hasStarted = true;
        _wordIndex = 0;
        _charCount = 0;
        SetValue(DisplayTextPropertyKey, string.Empty);

        if (string.IsNullOrEmpty(CurrentTarget))
        {
            _phase = Phase.Done;
            _timer.Stop();
            return;
        }

        if (Delay > 0)
        {
            SetPhase(Phase.Delaying, Delay);
        }
        else
        {
            SetPhase(Phase.Typing, TypeInterval);
        }
    }

    /// <summary>Stops the animation, leaving the text as it is.</summary>
    public void Stop()
    {
        _timer.Stop();
        _phase = Phase.Idle;
    }

    // ---------------------------------------------------------------- internals

    private bool UsesWords => Words.Count > 0;

    private string CurrentTarget => UsesWords
        ? (_wordIndex >= 0 && _wordIndex < Words.Count ? Words[_wordIndex] ?? string.Empty : string.Empty)
        : Text ?? string.Empty;

    private double TypeInterval => Math.Max(1d, UsesWords ? TypeSpeed : Duration);

    private double DeleteInterval => Math.Max(1d, DeleteSpeed);

    private void SetPhase(Phase phase, double intervalMs)
    {
        _phase = phase;
        _timer.Interval = TimeSpan.FromMilliseconds(Math.Max(1d, intervalMs));
        _timer.Start();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        switch (_phase)
        {
            case Phase.Delaying:
                SetPhase(Phase.Typing, TypeInterval);
                break;

            case Phase.Typing:
                TypeStep();
                break;

            case Phase.Pausing:
                SetPhase(Phase.Deleting, DeleteInterval);
                break;

            case Phase.Deleting:
                DeleteStep();
                break;

            default:
                _timer.Stop();
                break;
        }
    }

    private void TypeStep()
    {
        string target = CurrentTarget;

        if (_charCount < target.Length)
        {
            _charCount++;
            SetValue(DisplayTextPropertyKey, target[.._charCount]);
        }

        if (_charCount < target.Length)
        {
            _timer.Interval = TimeSpan.FromMilliseconds(TypeInterval);
            return;
        }

        // Word finished. Whether we delete it depends on what comes next.
        bool isLastWord = !UsesWords || _wordIndex == Words.Count - 1;

        if (isLastWord && !Loop)
        {
            Finish();
            return;
        }

        SetPhase(Phase.Pausing, PauseDelay);
    }

    private void DeleteStep()
    {
        if (_charCount > 0)
        {
            _charCount--;
            SetValue(DisplayTextPropertyKey, CurrentTarget[.._charCount]);
        }

        if (_charCount > 0)
        {
            _timer.Interval = TimeSpan.FromMilliseconds(DeleteInterval);
            return;
        }

        if (UsesWords)
        {
            _wordIndex = (_wordIndex + 1) % Words.Count;
        }

        SetPhase(Phase.Typing, TypeInterval);
    }

    private void Finish()
    {
        _timer.Stop();
        _phase = Phase.Done;
        Completed?.Invoke(this, EventArgs.Empty);
    }

    private static void OnSourceTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var typing = (TypingText)d;

        if (typing._hasStarted)
        {
            typing.Start();
        }
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == FontSizeProperty)
        {
            UpdateCursorMetrics();
        }
    }

    private void UpdateCursorMetrics()
    {
        double size = FontSize;
        SetValue(CursorWidthPropertyKey, Math.Round(size * 0.55));
        SetValue(CursorHeightPropertyKey, Math.Round(size * 1.25));
        SetValue(CursorThicknessPropertyKey, Math.Max(1d, Math.Round(size / 12d)));
    }

    // ---------------------------------------------------------------- lifecycle

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateCursorMetrics();

        if (!StartOnView)
        {
            Start();
            return;
        }

        _scrollHost = FindScrollHost(this);

        if (_scrollHost is not null)
        {
            _scrollHost.ScrollChanged += OnHostScrollChanged;
        }

        // Wait for the first arrange pass: until then the control has no position,
        // and with no text and no caret it has no size either.
        Dispatcher.BeginInvoke(new Action(TryStartWhenVisible), DispatcherPriority.Loaded);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();

        if (_scrollHost is not null)
        {
            _scrollHost.ScrollChanged -= OnHostScrollChanged;
            _scrollHost = null;
        }
    }

    private void OnHostScrollChanged(object sender, ScrollChangedEventArgs e) => TryStartWhenVisible();

    private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) => TryStartWhenVisible();

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => TryStartWhenVisible();

    private void TryStartWhenVisible()
    {
        if (_hasStarted || !IsLoaded || !IsVisible || !StartOnView)
        {
            return;
        }

        if (_scrollHost is null)
        {
            Start();
            return;
        }

        try
        {
            GeneralTransform transform = TransformToAncestor(_scrollHost);

            // An empty TypingText with no caret measures 0x0; probe at least one pixel
            // so Rect.IntersectsWith has something to hit.
            var probe = new Rect(0, 0, Math.Max(ActualWidth, 1d), Math.Max(ActualHeight, 1d));
            Rect bounds = transform.TransformBounds(probe);
            var viewport = new Rect(0, 0, _scrollHost.ActualWidth, _scrollHost.ActualHeight);

            if (bounds.IntersectsWith(viewport))
            {
                Start();
            }
        }
        catch (InvalidOperationException)
        {
            // Not in the same visual tree (yet).
        }
    }

    private static ScrollViewer? FindScrollHost(DependencyObject start)
    {
        DependencyObject? current = VisualTreeHelper.GetParent(start);

        while (current is not null)
        {
            if (current is ScrollViewer viewer)
            {
                return viewer;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }
}

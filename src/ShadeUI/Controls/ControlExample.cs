using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace ShadeUI.Controls;

/// <summary>
/// A gallery block: a live preview of a control plus the XAML that produced it,
/// with a copy button. Used to document a control without keeping the sample and
/// the snippet in sync by hand.
/// <code language="xml">
/// &lt;ui:ControlExample Header="Intents" Xaml="..."&gt;
///     &lt;Button Content="Default" /&gt;
/// &lt;/ui:ControlExample&gt;
/// </code>
/// </summary>
[TemplatePart(Name = CopyButtonPartName, Type = typeof(ButtonBase))]
[TemplatePart(Name = ToggleCodePartName, Type = typeof(ButtonBase))]
public class ControlExample : ContentControl
{
    private const string CopyButtonPartName = "PART_CopyButton";
    private const string ToggleCodePartName = "PART_ToggleCodeButton";

    private static readonly TimeSpan CopiedFeedbackDuration = TimeSpan.FromSeconds(1.6);

    private ButtonBase? _copyButton;
    private ButtonBase? _toggleButton;
    private DispatcherTimer? _copiedTimer;

    static ControlExample()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ControlExample),
            new FrameworkPropertyMetadata(typeof(ControlExample)));
    }

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header), typeof(string), typeof(ControlExample), new PropertyMetadata(null));

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(string), typeof(ControlExample), new PropertyMetadata(null));

    public static readonly DependencyProperty XamlProperty = DependencyProperty.Register(
        nameof(Xaml), typeof(string), typeof(ControlExample),
        new PropertyMetadata(null, OnXamlChanged));

    private static readonly DependencyPropertyKey DisplayXamlPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(DisplayXaml), typeof(string), typeof(ControlExample), new PropertyMetadata(null));

    /// <summary>Identifies the read-only <see cref="DisplayXaml"/> property.</summary>
    public static readonly DependencyProperty DisplayXamlProperty = DisplayXamlPropertyKey.DependencyProperty;

    public static readonly DependencyProperty IsCodeVisibleProperty = DependencyProperty.Register(
        nameof(IsCodeVisible), typeof(bool), typeof(ControlExample), new PropertyMetadata(false));

    private static readonly DependencyPropertyKey IsCopiedPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(IsCopied), typeof(bool), typeof(ControlExample), new PropertyMetadata(false));

    /// <summary>Identifies the read-only <see cref="IsCopied"/> property.</summary>
    public static readonly DependencyProperty IsCopiedProperty = IsCopiedPropertyKey.DependencyProperty;

    /// <summary>Short title for the block, e.g. "Intents".</summary>
    public string? Header
    {
        get => (string?)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>Optional sentence explaining what the sample shows.</summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// The markup for this sample. Declare it with <c>xml:space="preserve"</c> so the line
    /// breaks survive XAML parsing; the surrounding indentation is stripped automatically.
    /// </summary>
    public string? Xaml
    {
        get => (string?)GetValue(XamlProperty);
        set => SetValue(XamlProperty, value);
    }

    /// <summary>
    /// <see cref="Xaml"/> with blank edge lines removed and the common indentation stripped.
    /// This is what the code pane shows and what the copy button puts on the clipboard.
    /// </summary>
    public string? DisplayXaml => (string?)GetValue(DisplayXamlProperty);

    /// <summary>Whether the code pane is expanded. Defaults to <see langword="false"/>.</summary>
    public bool IsCodeVisible
    {
        get => (bool)GetValue(IsCodeVisibleProperty);
        set => SetValue(IsCodeVisibleProperty, value);
    }

    /// <summary>True for a moment after a successful copy, so the template can confirm it.</summary>
    public bool IsCopied => (bool)GetValue(IsCopiedProperty);

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_copyButton is not null)
        {
            _copyButton.Click -= OnCopyClick;
        }

        if (_toggleButton is not null)
        {
            _toggleButton.Click -= OnToggleCodeClick;
        }

        _copyButton = GetTemplateChild(CopyButtonPartName) as ButtonBase;
        _toggleButton = GetTemplateChild(ToggleCodePartName) as ButtonBase;

        if (_copyButton is not null)
        {
            _copyButton.Click += OnCopyClick;
        }

        if (_toggleButton is not null)
        {
            _toggleButton.Click += OnToggleCodeClick;
        }
    }

    private void OnToggleCodeClick(object sender, RoutedEventArgs e)
    {
        SetCurrentValue(IsCodeVisibleProperty, !IsCodeVisible);
    }

    private static void OnXamlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((ControlExample)d).SetValue(DisplayXamlPropertyKey, Dedent(e.NewValue as string));
    }

    /// <summary>
    /// Drops blank leading and trailing lines and removes the indentation the snippet
    /// inherited from the XAML file it was written in.
    /// </summary>
    private static string? Dedent(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        string[] lines = value.Replace("\r\n", "\n").Split('\n');

        int first = 0;
        while (first < lines.Length && lines[first].Trim().Length == 0)
        {
            first++;
        }

        int last = lines.Length - 1;
        while (last >= first && lines[last].Trim().Length == 0)
        {
            last--;
        }

        if (first > last)
        {
            return string.Empty;
        }

        int indent = int.MaxValue;
        for (int i = first; i <= last; i++)
        {
            string line = lines[i];
            if (line.Trim().Length == 0)
            {
                continue;
            }

            indent = Math.Min(indent, line.Length - line.TrimStart().Length);
        }

        if (indent is int.MaxValue or 0)
        {
            return string.Join(Environment.NewLine, lines[first..(last + 1)]);
        }

        var trimmed = new string[last - first + 1];
        for (int i = first; i <= last; i++)
        {
            string line = lines[i];
            trimmed[i - first] = line.Length >= indent ? line[indent..] : line.TrimStart();
        }

        return string.Join(Environment.NewLine, trimmed);
    }

    private void OnCopyClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(DisplayXaml))
        {
            return;
        }

        try
        {
            Clipboard.SetText(DisplayXaml);
        }
        catch (System.Runtime.InteropServices.ExternalException)
        {
            // Another process holds the clipboard; nothing useful to do.
            return;
        }

        SetValue(IsCopiedPropertyKey, true);

        _copiedTimer ??= new DispatcherTimer { Interval = CopiedFeedbackDuration };
        _copiedTimer.Stop();
        _copiedTimer.Tick -= OnCopiedTimerTick;
        _copiedTimer.Tick += OnCopiedTimerTick;
        _copiedTimer.Start();
    }

    private void OnCopiedTimerTick(object? sender, EventArgs e)
    {
        _copiedTimer?.Stop();
        SetValue(IsCopiedPropertyKey, false);
    }
}

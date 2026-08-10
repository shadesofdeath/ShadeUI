using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ShadeUI.Controls;

/// <summary>
/// A themed single- or multi-line text input with placeholder text, an optional
/// glyph and a clear button.
/// <code language="xml">
/// &lt;ui:TextBox PlaceholderText="Ara..." Icon="&amp;#xE721;" /&gt;
/// </code>
/// </summary>
[TemplatePart(Name = ClearButtonPartName, Type = typeof(ButtonBase))]
public class TextBox : System.Windows.Controls.TextBox
{
    private const string ClearButtonPartName = "PART_ClearButton";

    private ButtonBase? _clearButton;

    static TextBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TextBox),
            new FrameworkPropertyMetadata(typeof(TextBox)));
    }

    public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
        nameof(PlaceholderText),
        typeof(string),
        typeof(TextBox),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty PlaceholderEnabledProperty = DependencyProperty.Register(
        nameof(PlaceholderEnabled),
        typeof(bool),
        typeof(TextBox),
        new PropertyMetadata(true));

    public static readonly DependencyProperty ClearButtonEnabledProperty = DependencyProperty.Register(
        nameof(ClearButtonEnabled),
        typeof(bool),
        typeof(TextBox),
        new PropertyMetadata(true));

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(string),
        typeof(TextBox),
        new PropertyMetadata(null));

    public static readonly DependencyProperty IconPlacementProperty = DependencyProperty.Register(
        nameof(IconPlacement),
        typeof(IconPlacement),
        typeof(TextBox),
        new PropertyMetadata(IconPlacement.Left));

    private static readonly DependencyPropertyKey HasTextPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(HasText),
        typeof(bool),
        typeof(TextBox),
        new PropertyMetadata(false));

    /// <summary>Identifies the read-only <see cref="HasText"/> property.</summary>
    public static readonly DependencyProperty HasTextProperty = HasTextPropertyKey.DependencyProperty;

    /// <summary>Hint shown while the box is empty.</summary>
    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    /// <summary>Whether <see cref="PlaceholderText"/> is rendered at all. Defaults to <see langword="true"/>.</summary>
    public bool PlaceholderEnabled
    {
        get => (bool)GetValue(PlaceholderEnabledProperty);
        set => SetValue(PlaceholderEnabledProperty, value);
    }

    /// <summary>Whether the inline clear button appears while the box is focused and not empty.</summary>
    public bool ClearButtonEnabled
    {
        get => (bool)GetValue(ClearButtonEnabledProperty);
        set => SetValue(ClearButtonEnabledProperty, value);
    }

    /// <summary>Optional Segoe Fluent Icons glyph rendered inside the box.</summary>
    public string? Icon
    {
        get => (string?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Which side <see cref="Icon"/> is drawn on. Defaults to <see cref="Controls.IconPlacement.Left"/>.</summary>
    public IconPlacement IconPlacement
    {
        get => (IconPlacement)GetValue(IconPlacementProperty);
        set => SetValue(IconPlacementProperty, value);
    }

    /// <summary>True while <see cref="System.Windows.Controls.TextBox.Text"/> is not empty. Used by the template.</summary>
    public bool HasText => (bool)GetValue(HasTextProperty);

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_clearButton is not null)
        {
            _clearButton.Click -= OnClearButtonClick;
        }

        _clearButton = GetTemplateChild(ClearButtonPartName) as ButtonBase;

        if (_clearButton is not null)
        {
            _clearButton.Click += OnClearButtonClick;
        }

        UpdateHasText();
    }

    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        base.OnTextChanged(e);
        UpdateHasText();
    }

    private void UpdateHasText()
    {
        SetValue(HasTextPropertyKey, !string.IsNullOrEmpty(Text));
    }

    private void OnClearButtonClick(object sender, RoutedEventArgs e)
    {
        SetCurrentValue(TextProperty, string.Empty);
        Focus();
    }
}

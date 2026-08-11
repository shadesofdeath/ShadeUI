using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace ShadeUI.Controls;

/// <summary>Carries the item that just became current.</summary>
public class NavigationViewSelectionChangedEventArgs : EventArgs
{
    public NavigationViewSelectionChangedEventArgs(object? selectedItem, bool isFooterItem)
    {
        SelectedItem = selectedItem;
        IsFooterItem = isFooterItem;
    }

    /// <summary>The newly selected item, or <see langword="null"/> when the selection was cleared.</summary>
    public object? SelectedItem { get; }

    /// <summary>Whether the item came from <see cref="NavigationView.FooterItems"/>.</summary>
    public bool IsFooterItem { get; }
}

/// <summary>
/// A left navigation pane with a content area, in the style of the Windows 11 Settings app.
/// The pane collapses to an icon rail and expands back with an animation.
/// <code language="xml">
/// &lt;ui:NavigationView PaneTitle="ShadeUI"&gt;
///     &lt;ui:NavigationView.MenuItems&gt;
///         &lt;ui:NavigationViewItem Icon="&amp;#xE80F;" Content="Home" /&gt;
///     &lt;/ui:NavigationView.MenuItems&gt;
/// &lt;/ui:NavigationView&gt;
/// </code>
/// </summary>
[TemplatePart(Name = PanePartName, Type = typeof(FrameworkElement))]
public class NavigationView : ContentControl
{
    private const string PanePartName = "PART_Pane";
    private const string MenuHostPartName = "PART_MenuHost";
    private const string FooterHostPartName = "PART_FooterHost";

    private FrameworkElement? _pane;
    private Panel? _menuHost;
    private Panel? _footerHost;

    static NavigationView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NavigationView),
            new FrameworkPropertyMetadata(typeof(NavigationView)));
    }

    public NavigationView()
    {
        var menu = new ObservableCollection<object>();
        menu.CollectionChanged += OnItemsCollectionChanged;
        SetValue(MenuItemsPropertyKey, menu);

        var footer = new ObservableCollection<object>();
        footer.CollectionChanged += OnItemsCollectionChanged;
        SetValue(FooterItemsPropertyKey, footer);
    }

    /// <summary>Raised after <see cref="SelectedItem"/> changes.</summary>
    public event EventHandler<NavigationViewSelectionChangedEventArgs>? SelectionChanged;

    /// <summary>Raised when the back button is pressed.</summary>
    public event EventHandler? BackRequested;

    // ---------------------------------------------------------------- items

    private static readonly DependencyPropertyKey MenuItemsPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(MenuItems), typeof(ObservableCollection<object>), typeof(NavigationView),
        new PropertyMetadata(null));

    private static readonly DependencyPropertyKey FooterItemsPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(FooterItems), typeof(ObservableCollection<object>), typeof(NavigationView),
        new PropertyMetadata(null));

    public static readonly DependencyProperty MenuItemsProperty = MenuItemsPropertyKey.DependencyProperty;
    public static readonly DependencyProperty FooterItemsProperty = FooterItemsPropertyKey.DependencyProperty;

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem), typeof(object), typeof(NavigationView),
        new FrameworkPropertyMetadata(null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

    /// <summary>Main navigation entries, drawn at the top of the pane.</summary>
    public ObservableCollection<object> MenuItems => (ObservableCollection<object>)GetValue(MenuItemsProperty);

    /// <summary>Entries pinned to the bottom of the pane, such as Settings.</summary>
    public ObservableCollection<object> FooterItems => (ObservableCollection<object>)GetValue(FooterItemsProperty);

    /// <summary>The current page's item.</summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    // ---------------------------------------------------------------- pane

    public static readonly DependencyProperty IsPaneOpenProperty = DependencyProperty.Register(
        nameof(IsPaneOpen), typeof(bool), typeof(NavigationView),
        new PropertyMetadata(true, OnIsPaneOpenChanged));

    public static readonly DependencyProperty OpenPaneLengthProperty = DependencyProperty.Register(
        nameof(OpenPaneLength), typeof(double), typeof(NavigationView), new PropertyMetadata(190d));

    public static readonly DependencyProperty CompactPaneLengthProperty = DependencyProperty.Register(
        nameof(CompactPaneLength), typeof(double), typeof(NavigationView), new PropertyMetadata(44d));

    public static readonly DependencyProperty PaneTitleProperty = DependencyProperty.Register(
        nameof(PaneTitle), typeof(string), typeof(NavigationView), new PropertyMetadata(null));

    public static readonly DependencyProperty IsPaneToggleButtonVisibleProperty = DependencyProperty.Register(
        nameof(IsPaneToggleButtonVisible), typeof(bool), typeof(NavigationView), new PropertyMetadata(true));

    public static readonly DependencyProperty IsBackButtonVisibleProperty = DependencyProperty.Register(
        nameof(IsBackButtonVisible), typeof(bool), typeof(NavigationView), new PropertyMetadata(false));

    public static readonly DependencyProperty IsBackEnabledProperty = DependencyProperty.Register(
        nameof(IsBackEnabled), typeof(bool), typeof(NavigationView), new PropertyMetadata(true));

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header), typeof(object), typeof(NavigationView), new PropertyMetadata(null));

    public static readonly DependencyProperty IsSearchVisibleProperty = DependencyProperty.Register(
        nameof(IsSearchVisible), typeof(bool), typeof(NavigationView), new PropertyMetadata(false));

    public static readonly DependencyProperty SearchPlaceholderProperty = DependencyProperty.Register(
        nameof(SearchPlaceholder), typeof(string), typeof(NavigationView), new PropertyMetadata("Ara"));

    public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register(
        nameof(SearchText), typeof(string), typeof(NavigationView),
        new FrameworkPropertyMetadata(string.Empty,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSearchTextChanged));

    /// <summary>Whether a filter box is shown at the top of the pane.</summary>
    public bool IsSearchVisible
    {
        get => (bool)GetValue(IsSearchVisibleProperty);
        set => SetValue(IsSearchVisibleProperty, value);
    }

    /// <summary>Hint shown in the filter box.</summary>
    public string SearchPlaceholder
    {
        get => (string)GetValue(SearchPlaceholderProperty);
        set => SetValue(SearchPlaceholderProperty, value);
    }

    /// <summary>Current filter text. Items whose content does not contain it are hidden.</summary>
    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    /// <summary>Whether the pane shows labels or collapses to an icon rail. Defaults to <see langword="true"/>.</summary>
    public bool IsPaneOpen
    {
        get => (bool)GetValue(IsPaneOpenProperty);
        set => SetValue(IsPaneOpenProperty, value);
    }

    /// <summary>Pane width when open. Defaults to 190.</summary>
    public double OpenPaneLength
    {
        get => (double)GetValue(OpenPaneLengthProperty);
        set => SetValue(OpenPaneLengthProperty, value);
    }

    /// <summary>Pane width when collapsed. Defaults to 44.</summary>
    public double CompactPaneLength
    {
        get => (double)GetValue(CompactPaneLengthProperty);
        set => SetValue(CompactPaneLengthProperty, value);
    }

    /// <summary>Optional caption shown next to the toggle button while the pane is open.</summary>
    public string? PaneTitle
    {
        get => (string?)GetValue(PaneTitleProperty);
        set => SetValue(PaneTitleProperty, value);
    }

    public bool IsPaneToggleButtonVisible
    {
        get => (bool)GetValue(IsPaneToggleButtonVisibleProperty);
        set => SetValue(IsPaneToggleButtonVisibleProperty, value);
    }

    public bool IsBackButtonVisible
    {
        get => (bool)GetValue(IsBackButtonVisibleProperty);
        set => SetValue(IsBackButtonVisibleProperty, value);
    }

    public bool IsBackEnabled
    {
        get => (bool)GetValue(IsBackEnabledProperty);
        set => SetValue(IsBackEnabledProperty, value);
    }

    /// <summary>Content shown above the page, inside the content pane.</summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    // ---------------------------------------------------------------- api

    /// <summary>Selects an item as if the user had clicked it.</summary>
    public void SelectItem(object? item)
    {
        SetCurrentValue(SelectedItemProperty, item);
    }

    /// <summary>Flips <see cref="IsPaneOpen"/>.</summary>
    public void TogglePane()
    {
        SetCurrentValue(IsPaneOpenProperty, !IsPaneOpen);
    }

    /// <summary>Raises <see cref="BackRequested"/>. Called by the template's back button.</summary>
    public void RequestBack()
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _pane = GetTemplateChild(PanePartName) as FrameworkElement;
        _menuHost = GetTemplateChild(MenuHostPartName) as Panel;
        _footerHost = GetTemplateChild(FooterHostPartName) as Panel;

        if (_pane is not null)
        {
            _pane.Width = IsPaneOpen ? OpenPaneLength : CompactPaneLength;
        }

        RebuildHosts();

        if (GetTemplateChild("PART_ToggleButton") is ButtonBase toggle)
        {
            toggle.Click -= OnToggleClick;
            toggle.Click += OnToggleClick;
        }

        if (GetTemplateChild("PART_BackButton") is ButtonBase back)
        {
            back.Click -= OnBackClick;
            back.Click += OnBackClick;
        }

        UpdateItemSelection();
    }

    private void OnToggleClick(object sender, RoutedEventArgs e) => TogglePane();

    private void OnBackClick(object sender, RoutedEventArgs e) => RequestBack();

    private static void OnIsPaneOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((NavigationView)d).AnimatePane((bool)e.NewValue);
    }

    private void AnimatePane(bool open)
    {
        if (_pane is null)
        {
            return;
        }

        double target = open ? OpenPaneLength : CompactPaneLength;

        var animation = new DoubleAnimation(target, new Duration(TimeSpan.FromMilliseconds(220)))
        {
            EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut },
        };

        _pane.BeginAnimation(WidthProperty, animation);
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var view = (NavigationView)d;
        view.UpdateItemSelection();
        view.SelectionChanged?.Invoke(
            view,
            new NavigationViewSelectionChangedEventArgs(e.NewValue, view.FooterItems.Contains(e.NewValue)));
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildHosts();
        UpdateItemSelection();
        ApplyFilter();
    }

    private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((NavigationView)d).ApplyFilter();
    }

    /// <summary>
    /// Hides items that do not match <see cref="SearchText"/>, and hides a category header
    /// once every item under it has been filtered out.
    /// </summary>
    private void ApplyFilter()
    {
        string filter = SearchText?.Trim() ?? string.Empty;
        bool filtering = filter.Length > 0;

        NavigationViewItemHeader? pendingHeader = null;
        bool headerHasMatch = false;

        foreach (object item in MenuItems)
        {
            switch (item)
            {
                case NavigationViewItemHeader header:
                    if (pendingHeader is not null)
                    {
                        pendingHeader.Visibility = headerHasMatch ? Visibility.Visible : Visibility.Collapsed;
                    }

                    pendingHeader = header;
                    headerHasMatch = false;
                    break;

                case NavigationViewItem navItem:
                    bool matches = !filtering ||
                        (navItem.Content?.ToString() ?? string.Empty)
                            .Contains(filter, StringComparison.CurrentCultureIgnoreCase);

                    navItem.Visibility = matches ? Visibility.Visible : Visibility.Collapsed;
                    headerHasMatch |= matches;
                    break;
            }
        }

        if (pendingHeader is not null)
        {
            pendingHeader.Visibility = headerHasMatch ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void RebuildHosts()
    {
        FillHost(_menuHost, MenuItems);
        FillHost(_footerHost, FooterItems);
    }

    private static void FillHost(Panel? panel, IEnumerable? items)
    {
        if (panel is null)
        {
            return;
        }

        panel.Children.Clear();

        if (items is null)
        {
            return;
        }

        foreach (object? item in items)
        {
            UIElement child = item switch
            {
                UIElement element => element,
                _ => new ContentPresenter { Content = item },
            };

            // A re-templated pane would otherwise try to add an element that still
            // has a parent from the previous template.
            if (child is FrameworkElement { Parent: Panel previous } && !ReferenceEquals(previous, panel))
            {
                previous.Children.Remove(child);
            }

            panel.Children.Add(child);
        }
    }

    private void UpdateItemSelection()
    {
        MarkSelection(MenuItems);
        MarkSelection(FooterItems);
    }

    private void MarkSelection(IEnumerable? items)
    {
        if (items is null)
        {
            return;
        }

        foreach (object? item in items)
        {
            if (item is NavigationViewItem navItem)
            {
                navItem.IsSelected = ReferenceEquals(navItem, SelectedItem);
            }
        }
    }
}

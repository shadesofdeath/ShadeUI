# ShadeUI

A modern, Fluent-inspired UI and theme library for WPF — custom window chrome, dark/light themes that follow Windows live, and cleanly styled controls.

![License](https://img.shields.io/badge/license-MIT-blue)
![.NET](https://img.shields.io/badge/.NET-10.0--windows-512BD4)
![Platform](https://img.shields.io/badge/platform-Windows-0078D4)

![ShadeUI Demo](docs/screenshot.png)

## Features

- 🪟 **ShadeWindow** — custom window chrome with a Win11-style title bar, dark-mode aware non-client area, a themed 1 px DWM border, rounded corners and open/close transitions
- 🎨 **Live system theming** — follows the Windows dark/light setting *and* accent color in real time (`Theme="System"`)
- 🌗 **Dark & Light palettes** — WinUI-style design tokens (`ApplicationBackgroundBrush`, `TextFillColorPrimaryBrush`, `AccentFillColorDefaultBrush`, …) on one even surface ramp: shell → content pane → card → control
- 🧩 **Styled controls** — buttons (default + accent), `TextBox` with placeholder / icon / clear button, navigation list, cards, modern scrollbars
- ✨ **Reveal highlight** — `effects:Reveal.IsEnabled` makes a control light up from the pointer position, plus animated press/spring-back and an accent glow; the reveal colors are palette tokens, so it works on both dark and light surfaces
- 📐 **Compact by design** — a single set of sizing tokens (`ControlHeight`, `ControlPadding`, `ControlCornerRadius`, …) drives every control; 24 px control height, 12 px body text
- 📦 **Drop-in setup** — two resource dictionaries in `App.xaml` and you are done

## Getting started

> NuGet package coming soon. For now, clone and reference `src/ShadeUI/ShadeUI.csproj`.

**1. Merge the theme in `App.xaml`:**

```xml
<Application
    xmlns:ui="http://schemas.shadeui.dev/2026/xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ui:ThemesDictionary Theme="System" />
                <ui:ControlsDictionary />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

`Theme` can be `System`, `Dark` or `Light`.

**2. Use `ShadeWindow` with a `TitleBar`:**

```xml
<ui:ShadeWindow
    x:Class="MyApp.MainWindow"
    xmlns:ui="http://schemas.shadeui.dev/2026/xaml"
    Title="My App">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>

        <ui:TitleBar Grid.Row="0" Title="My App" />

        <!-- your content -->
    </Grid>
</ui:ShadeWindow>
```

**3. Switch themes at runtime:**

```csharp
using ShadeUI.Appearance;

ThemeManager.Apply(ApplicationTheme.Dark);    // force dark
ThemeManager.Apply(ApplicationTheme.System);  // follow Windows again
```

## Demo

The repository ships a live gallery app:

```
dotnet run --project samples/ShadeUI.Demo
```

## Roadmap

- [x] ShadeWindow + TitleBar (custom chrome)
- [x] Dark/Light palettes with live system + accent tracking
- [x] Button, navigation, card, scrollbar styles
- [x] Compact sizing token system
- [x] Reveal / press animations on buttons
- [x] TextBox (placeholder, icon, clear button, multiline)
- [x] CheckBox (three-state) + RadioButton
- [x] Deep dark surface ramp, window border, open/close + page transitions
- [ ] ToggleSwitch, ComboBox
- [ ] NavigationView control
- [ ] Snap-layouts flyout on the maximize button
- [ ] NuGet package

## License

[MIT](LICENSE) © shadesofdeath

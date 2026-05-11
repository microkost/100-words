# 100 Words

A Windows language-learning app that helps you build a practical starting vocabulary through repeated exposure to the most common words.

<p align="center">
  <img src="documentation/100words-logo-250.png" alt="100 Words Logo" width="250"/>
</p>

<p align="center">
  <a href="https://www.microsoft.com/store/productId/9MWRDJGPXZ6Q">
    <img src="https://img.shields.io/badge/Download-Microsoft%20Store-blue" alt="Download from Microsoft Store"/>
  </a>
</p>

## What It Is

100 Words is for learners who want a small, useful vocabulary set they can revisit often. The app rotates words automatically, lets you control which languages are shown, and works fully offline.

The original idea came from learning Finnish as a non-native speaker: a compact set of high-frequency words is often enough to make starting easier.

## Current App

### For Users

- Windows desktop app available through the [Microsoft Store](https://www.microsoft.com/store/productId/9MWRDJGPXZ6Q)
- Offline-first experience with no account, sync, or backend required
- Up to 4 visible language slots with configurable ordering
- Adjustable refresh timer and manual shuffle
- Light, dark, and system theme support
- Optional advanced words in addition to the core vocabulary set
- Live Tile-related behavior is no longer central on modern Windows, but the app still preserves the core learning flow

### For Developers

The current application is a **WinUI 3 unpackaged desktop app** built on **.NET 8**.

- .NET 8
- C# and XAML with `Microsoft.UI.Xaml`
- WinUI 3 through **Windows App SDK 1.6**
- File-backed local settings in `%LOCALAPPDATA%\100words\settings.json`
- Vocabulary loaded from `100words/Assets/dictionary.json`
- No backend services or online dependency

## Features

- Learn from a compact, practical vocabulary list
- Show translations in multiple languages at once
- Reorder language priority in settings
- Automatically rotate words on a timer
- Shuffle manually at any time
- Keep using it without an internet connection
- Browse the source on [GitHub](https://github.com/microkost/100-words)

## Screenshots

<p align="center">
  <img src="documentation/screenshot-metro-ui-thumb.png" alt="Start Menu View 2018" width="45%"/>
  <img src="documentation/screenshot-desktop-thumb.png" alt="Desktop App View 2018" width="45%"/>
</p>

## Supported Languages

- Czech
- English
- Finnish
- Polish

See the current vocabulary data in [100words/Assets/dictionary.json](100words/Assets/dictionary.json).

Part of the vocabulary was sourced from [uusikielemme.fi](http://uusikielemme.fi/).

If you want to suggest new words or phrases, use the [contribution form](https://goo.gl/forms/a72Osyz1Bpu4mqq22).

## Settings

The app keeps its settings locally in:

```text
%LOCALAPPDATA%\100words\settings.json
```

## How It Got Here

### 2018: Original UWP Version

The first version was built by hand as a **Universal Windows Platform (UWP)** app and published as a personal Windows development exercise. At that time, **Windows 10 Live Tiles** were a meaningful part of the experience because vocabulary could appear directly in the Start menu.

That version used:

- `Windows.UI.Xaml`
- UWP lifecycle APIs
- `ApplicationData` for settings
- Tile APIs such as `TileUpdateManager` and `StartScreenManager`

### 2026: Modernized WinUI 3 Version

As UWP moved into maintenance mode and Live Tiles disappeared from Windows 11, the app was modernized to the current WinUI 3 desktop model.

The migration included:

- `Windows.UI.Xaml` to `Microsoft.UI.Xaml`
- UWP packaging to an unpackaged desktop app with bootstrap initialization
- `ApplicationData` settings to file-backed JSON in local app data
- `ms-appx:///` asset handling to runtime-resolved local file paths when unpackaged
- older shell patterns to a `NavigationView`-based UI with Mica backdrop support
- package-identity-only functionality guarded so it is skipped safely when unavailable

The modernization also served as an experiment in AI-assisted legacy migration (Claude Sonnet 4.6). GitHub Copilot was part of that process, and AI was also used to help expand dictionary content.

Microsoft Store registration for individual developers is now [free](https://blogs.windows.com/windowsdeveloper/2025/09/10/free-developer-registration-for-individual-developers-on-microsoft-store/).

## Development

### Requirements

- Visual Studio 2022 or later
- Windows application development workload
- Windows App SDK support from that workload
- Nuget `Microsoft.WindowsAppSDK`
- Nuget `Microsoft.Windows.SDK.BuildTools`

### Getting Started

1. Clone the repository.
2. Open `100words.sln` in Visual Studio.
3. Restore NuGet packages.
4. Select `x64` as the target platform.
5. Build and run.

### Project Structure

```text
100words/
├── Program.cs             # Bootstrap entry point for unpackaged WinUI 3
├── App.xaml / App.xaml.cs # App startup, window creation, Mica backdrop
├── MainPage.xaml          # Main UI shell
├── MainPage.xaml.cs       # Vocabulary flow, settings, theme, tile guards
├── Dictionary.cs          # Vocabulary loader
├── Phrase.cs              # Phrase model
├── LocalSettingsHelper.cs # File-backed settings store
└── Assets/                # Dictionary, flags, and icons
```

### Publishing

- [Partner Center dashboard](https://partner.microsoft.com/en-US/dashboard/apps-and-games/overview)
- [Microsoft Store publishing documentation](https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/msix/create-app-submission)

## Contributing

Contributions are welcome, especially for:

- vocabulary improvements
- new translations
- bug fixes
- documentation improvements

Issues and pull requests are both welcome.

## License

Open source and open to remixing. Please attribute the original work when sharing or modifying it.

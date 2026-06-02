# Architecture Documentation

## Overview

100 Words is a Windows desktop language-learning app built with WinUI 3 on .NET 10. It helps users learn a small set of practical vocabulary through repeated exposure, while keeping the app offline and lightweight.

## Technical Stack

- **Platform**: Windows desktop app with WinUI 3
- **Language**: C# with XAML
- **Target Framework**: `net10.0-windows10.0.19041.0`
- **Minimum Platform Version**: `10.0.19041.0` (Windows 10, version 2004)
- **Target Platforms**: `x86`, `x64`, `ARM64`
- **Runtime Identifiers**: `win-x86`, `win-x64`, `win-arm64`
- **IDE**: Visual Studio 2026+
- **Project Style**: SDK-style .NET desktop project
- **XAML Support**: `Microsoft.UI.Xaml` from Windows App SDK
- **Build System**: Visual Studio MSBuild

### Key Dependencies

- `Microsoft.WindowsAppSDK` 2.0.1
- `Microsoft.Windows.SDK.BuildTools` 10.0.26100.4654

## Solution Structure

The solution currently contains one project:

- `100words/100words.csproj`

Solution file:

- `100-words.slnx`

The solution also includes documentation items such as:

- `README.md`
- `ARCHITECTURE.md`
- `documentation/StoreDescription.txt`

## Architecture Design

The application follows a simple single-project desktop architecture with no backend services. Vocabulary data, settings, and UI logic all live in the same app project.

### Core Components

#### 1. App Startup

**Program.cs**
- Entry point for the app
- Initializes Windows App SDK bootstrap when running unpackaged
- Initializes COM wrappers
- Starts the WinUI application loop
- Uses `PACKAGED` compilation symbol to switch between unpackaged and packaged behavior

**App.xaml / App.xaml.cs**
- Application lifecycle and window creation
- Creates the main window on launch
- Applies Mica backdrop when supported
- Sets the window title and icon
- Hosts the root frame and navigates to `MainPage`

#### 2. Main UI Layer

**MainPage.xaml / MainPage.xaml.cs**
- Main application page and user interface
- Displays vocabulary words in up to four language slots
- Supports theme selection
- Supports manual shuffle and automatic refresh
- Supports enabling or disabling advanced words
- Supports language ordering customization
- Persists settings locally

Key behavior:
- Loads vocabulary from `dictionary.json`
- Restores saved language order, theme, refresh interval, and advanced-word setting
- Refreshes words using a `DispatcherTimer`
- Resolves `ms-appx:///` asset URIs to local file paths when running unpackaged

#### 3. Data Layer

**Dictionary.cs**
- Loads vocabulary from `100words/Assets/dictionary.json`
- Parses available languages and phrases from JSON
- Caches loaded data in memory
- Returns either the basic vocabulary set or the full set depending on the advanced-word setting

**Phrase.cs**
- Represents a vocabulary phrase
- Stores per-language translations
- Stores a `Level` field such as `basic` or `advanced`

#### 4. Settings Layer

**LocalSettingsHelper.cs**
- File-based local settings implementation
- Stores settings in `%LOCALAPPDATA%\\100words\\settings.json`
- Mimics dictionary-style access similar to `ApplicationDataContainer.Values`
- Supports reset by deleting the settings file

Used settings include:
- `100wordsLanguageOrder`
- `100wordsRefreshTime`
- `100wordsTheme`
- `100wordsIncludeAdvanced`

## Runtime Model

The app can run in two modes:

### Packaged mode
- Runs as a Store/MSIX package
- Uses package identity when available
- Skips runtime bootstrap
- Uses packaged asset paths directly

### Unpackaged mode
- Used for local development
- Initializes Windows App SDK manually in `Program.cs`
- Resolves `ms-appx:///` asset URIs to local files in the app directory

## Data Flow

1. **Startup**
   - App bootstraps Windows App SDK if unpackaged
   - `App` creates the window and navigates to `MainPage`
   - `MainPage` loads settings and vocabulary

2. **Vocabulary loading**
   - `Dictionary.LoadAsync()` reads `dictionary.json`
   - Languages and phrases are cached in memory
   - Basic or advanced phrases are selected based on user preference

3. **Display update**
   - A phrase is chosen and shuffled
   - Translations are mapped to the selected language order
   - Flags and text are rendered in four slots

4. **Timer refresh**
   - `DispatcherTimer` advances to the next word
   - When the list is exhausted, the dictionary is reloaded and reshuffled

5. **Settings persistence**
   - User changes are saved to the JSON settings file
   - Settings survive app restarts

## Design Decisions

### Why JSON-backed local settings?

- Works without package identity
- Keeps the app usable in both packaged and unpackaged modes
- Easy to inspect and reset
- No database or backend required

### Why WinUI 3?

- Modern Windows desktop UI stack
- Works well with .NET 10
- Supports current Windows App SDK APIs
- Fits the app’s Windows-only, desktop-first scope

### Why a single project?

- Keeps the app simple
- Reduces build and maintenance complexity
- Matches the app’s offline, self-contained nature

## Known Limitations

1. **Windows-only**: The app targets Windows desktop only.
2. **Fixed language count**: The UI is built around four visible language slots.
3. **Local dictionary updates require rebuilding**: Vocabulary changes ship with app updates.
4. **No backend sync**: Settings and vocabulary are entirely local.
5. **Single-device settings model**: No cloud sync between devices.

## Building and Deployment

### Local Development

1. Open `100-words.slnx` in Visual Studio 2026
2. Restore NuGet packages
3. Select a target platform (`x86`, `x64`, or `ARM64`)
4. Build and run

### Publishing

The project includes publish profiles under:

- `100words/Properties/PublishProfiles/`

Current profiles include:

- `FolderProfile.pubxml`
- `SingleFileSelfContained.pubxml`

Notes:
- `FolderProfile.pubxml` is framework-dependent and platform-specific
- `SingleFileSelfContained.pubxml` produces a self-contained, single-file publish for `win-x64`
- Store submission still uses MSIX packaging and Partner Center upload flow

### Microsoft Store Deployment

1. Associate the app with Partner Center
2. Update the package version in `Package.appxmanifest`
3. Create the Store package through Visual Studio
4. Upload the package upload file to Partner Center
5. Submit for certification

## Code Style

- Standard C# conventions
- Minimal comments unless needed for clarity
- Simple, direct control flow
- Local settings and runtime helpers kept lightweight
- No dependency injection or layered framework abstractions

## Testing Strategy

The app is primarily validated through manual testing:

- Launch the app and verify vocabulary display
- Confirm language order changes work
- Verify theme selection works
- Check timer-based refresh behavior
- Confirm settings persist across restarts
- Test packaged and unpackaged startup paths

## Security and Privacy Considerations

- No network communication
- No user accounts
- No analytics backend
- No cloud storage
- Settings remain on the local device only
- Vocabulary data ships with the app

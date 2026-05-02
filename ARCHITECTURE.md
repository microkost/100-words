# Architecture Documentation

## Overview

100 Words is a Universal Windows Platform (UWP) application designed to help users learn the 100 most essential words in a foreign language through passive exposure via Windows Live Tiles.

## Technical Stack

- **Platform**: Universal Windows Platform (UWP) on modern .NET
- **Language**: C# with XAML
- **Target Framework**: net10.0-windows10.0.26100.0
- **Minimum Platform Version**: 10.0.19041.0 (Windows 10, version 2004)
- **Maximum Tested Version**: 10.0.26100.0 (Windows 11, version 24H2)
- **IDE**: Visual Studio 2022 (17.8+) or Visual Studio 2026+
- **Project Style**: SDK-style with `UseUwp` and `UseUwpTools`
- **XAML Support**: Windows.UI.Xaml via CsWinRT projections
- **Build System**: Visual Studio MSBuild (required for UWP XAML compilation)

### Key Dependencies

- `Microsoft.Toolkit.Uwp.Notifications` 7.1.3 - For Live Tile notifications

### Migration History

This application was successfully modernized from legacy .NET Native UWP to modern .NET. See [MODERNIZATION-PLAN.md](MODERNIZATION-PLAN.md) for details.

## Architecture Design

The application follows a simple, monolithic UWP architecture with no external dependencies or backend services. All data is embedded in the application and stored locally.

### Core Components

#### 1. Data Layer

**Dictionary.cs**
- Static class containing all vocabulary data
- Hardcoded list of supported languages
- Hardcoded list of 100 essential phrases with translations
- Methods:
  - `GetListOfLanguages()` - Returns list of 4 supported language names
  - `GetListOfWords()` - Returns list of 100 Phrase objects

**Phrase.cs**
- Data model representing a single vocabulary word/phrase
- Properties:
  - `wordFI` - Finnish translation
  - `wordEN` - English translation
  - `wordCZ` - Czech translation
  - `wordPL` - Polish translation
- Constructor takes all 4 translations as parameters

#### 2. Presentation Layer

**MainPage.xaml / MainPage.xaml.cs**
- Main and only user interface screen
- Displays current vocabulary word with translations
- Manages Live Tile updates
- Handles user interactions (skip button, settings)
- Core functionality:
  - **Vocabulary Management**: Shuffles and displays words
  - **Live Tile Updates**: Pushes current word to Windows Start menu tile
  - **Timer Management**: Automatic word rotation based on user-defined interval
  - **Language Ordering**: Customizable display order for translations
  - **Settings Persistence**: Uses UWP ApplicationData for local storage

**App.xaml / App.xaml.cs**
- Application lifecycle management
- Standard UWP application entry point

#### 3. Storage Layer

Uses Windows UWP `ApplicationDataContainer` for persistent local storage:
- `100wordsLanguageOrder` - User's preferred language display order (string array)
- `100wordsRefreshTime` - Automatic refresh interval in hours (string)

## Key Features Implementation

### 1. Live Tile Integration

The app's signature feature uses Windows 10/11 Live Tiles:

```csharp
var notification = new TileNotification(GetNotificationScheme(...).GetXml());
TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
```

- Updates whenever a new word is displayed
- Shows current vocabulary word directly in Start menu
- Provides passive learning through repeated exposure

### 2. Vocabulary Shuffling

Words are randomly shuffled using Fisher-Yates algorithm:
- Ensures each session presents words in different order
- Prevents predictability and maintains user engagement
- Reloads full dictionary when all words are shown

### 3. Automatic Timer

Uses `DispatcherTimer` for periodic word updates:
- Default: 2 hours between word changes
- User-configurable via settings
- Persisted across app sessions

### 4. Language Priority Customization

Users can reorder which language appears in which position:
- Allows learning any supported language from any other supported language
- Settings stored locally and restored on app restart

## Data Flow

1. **App Launch**:
   - Load dictionary (100 words × 4 languages)
   - Restore user settings from local storage
   - Initialize UI with language ordering
   - Shuffle vocabulary list

2. **Display Word**:
   - Select first word from shuffled list
   - Display in UI according to language order
   - Update Live Tile with same content
   - Remove word from current session list

3. **Timer Tick** (or Manual Skip):
   - Trigger vocabulary refresh
   - If list empty, reload and reshuffle dictionary
   - Display next word
   - Update Live Tile

4. **Settings Change**:
   - Update language order
   - Update timer interval
   - Persist to local storage
   - Refresh display with new settings

## Design Decisions

### Why Hardcoded Dictionary?

- **Offline-First**: No internet dependency
- **Simplicity**: No database or file I/O complexity
- **Performance**: Instant load times
- **Reliability**: No data corruption or version conflicts
- **Trade-off**: Adding languages requires app update

### Why UWP?

- **Live Tiles**: Core feature only available in UWP
- **Windows Integration**: Native OS features
- **Historical Context**: Developed when UWP was Microsoft's recommended platform
- **Modern .NET Support**: Successfully migrated to modern .NET while keeping UWP features

### Why 4 Languages Max?

- UI designed for fixed 4-column layout
- Simplifies language ordering logic
- Current implementation has hardcoded checks: `if (specifiedOrder.Count < 4)`
- Expanding requires UI redesign and code refactoring

## Known Limitations

1. **UWP Deprecation**: Microsoft has deprecated UWP in favor of WinUI 3 and .NET MAUI
2. **Fixed Language Count**: Adding more languages requires code changes
3. **Manual Dictionary Updates**: New vocabulary requires recompilation
4. **Windows-Only**: Cannot run on other platforms (Mac, Linux, mobile)
5. **Live Tile Dependency**: Core feature unavailable if Microsoft removes Live Tiles

## Future Considerations

### Potential Migration Paths

1. **WinUI 3**: Maintain Windows-native experience with modern framework
2. **.NET MAUI**: Enable cross-platform support (Windows, Mac, iOS, Android)
3. **Blazor Hybrid**: Web-based UI with native capabilities

### Architecture Improvements

- Extract dictionary to JSON/database for easier updates
- Add dynamic language support without recompilation
- Implement backend API for community-contributed translations
- Add spaced repetition algorithm for better learning
- Support user-custom word lists

## Building and Deployment

### Local Development

1. Open `100words.sln` in Visual Studio 2022 (17.8+) or Visual Studio 2026
2. Ensure UWP workload and Windows SDK (10.0.26100.0) installed
3. Restore NuGet packages
4. Set `100words` as the startup project
5. Select platform (x86, x64, or ARM64)
6. Build and run (F5)

> **Important**: Must use Visual Studio build, not `dotnet build`, as UWP XAML compilation requires VS MSBuild.

### Common Issues

**Missing StoreLogo.png**: If you encounter an error about a missing payload file for `StoreLogo.png`, create the base file by copying one of the scaled versions:
```powershell
Copy-Item "Assets\StoreLogo.scale-100.png" -Destination "Assets\StoreLogo.png"
```

The project includes scaled asset versions (e.g., `StoreLogo.scale-100.png`, `StoreLogo.scale-200.png`) but Visual Studio may require the base file to exist.

### Microsoft Store Deployment

1. Associate app with Microsoft Store (requires Developer Account)
2. Update version in Package.appxmanifest
3. Create app package: Project → Publish → Create App Packages
4. Upload .appxupload file to Partner Center
5. Submit for certification

Certificate management: http://go.microsoft.com/fwlink/?LinkID=241478

## Code Style

- Standard C# conventions
- Minimal comments (code is self-documenting)
- Hardcoded constants for simplicity
- Exception handling with try-catch for settings
- No dependency injection or complex patterns

## Testing Strategy

Currently manual testing only:
- Launch app and verify word display
- Check Live Tile updates in Start menu
- Test language reordering
- Verify timer functionality
- Test settings persistence across app restarts

## Performance Characteristics

- **Startup Time**: Near-instant (< 1 second)
- **Memory Usage**: Minimal (< 50 MB)
- **CPU Usage**: Negligible (timer-based only)
- **Storage**: < 1 MB installed size
- **Network**: None (fully offline)

## Security Considerations

- No user data collection
- No network communication
- Settings stored only locally on device
- No authentication or authorization required
- Safe for privacy-conscious users

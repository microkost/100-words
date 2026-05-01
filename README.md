# 100 Words

A language learning application that teaches users the 100 most essential words needed to kickstart their journey in a new language.

<p align="center">
  <img src="documentation/100words-logo-250.png" alt="100 Words Logo" width="250"/>
</p>

<p align="center">
  <a href="https://www.microsoft.com/store/productId/9MWRDJGPXZ6Q">
    <img src="https://img.shields.io/badge/Download-Microsoft%20Store-blue" alt="Download from Microsoft Store"/>
  </a>
</p>

## About

The concept behind this app is based on the observation that the 100 most frequent words in any language provide the foundation for basic communication. While not academically researched, this idea emerged from personal experience learning Finnish as a non-native speaker.

The app's standout feature is its **Windows 10 Live Tile** integration, displaying vocabulary words and translations directly in your Start menu. Each time you open the Start menu, you're exposed to new vocabulary, making language learning a natural part of your daily computer use.

Users can learn the 100 essential words from any supported language as long as they understand at least one other supported language. Language priorities can be customized in the settings.

## Features

- Multi-language translations - In-app translations to 4 languages in responsive design
- Windows Live Tile - Pinned vocabulary display in Start menu
- Customizable language priority - User-defined language order
- Adjustable timer - Control how frequently words change
- Skip functionality - Manually skip to the next word
- Offline capable - Works without internet connection
- Open source - Full source code available on GitHub

## Screenshots

<p align="center">
  <img src="documentation/screenshot-metro-ui-thumb.png" alt="Start Menu View" width="45%"/>
  <img src="documentation/screenshot-desktop-thumb.png" alt="Desktop App View" width="45%"/>
</p>

## Supported Languages

Currently supported languages:
- Czech
- English
- Finnish
- Polish

View the complete [dictionary here](https://github.com/microkost/100-words/blob/master/100words/Dictionary.cs).

### Future Language Support

Planned additions:
- German
- Swedish

Language additions depend on the availability of language experts and native speakers. [Submit suggestions or volunteer to help](https://goo.gl/forms/a72Osyz1Bpu4mqq22).

Future updates may include variable and extended dictionaries, contingent on user adoption and feedback.

## Technology Stack

This application is built as a **Universal Windows Platform (UWP)** app using:

- C# and XAML
- Windows Live Tiles API for Start menu integration
- UWP notifications framework
- Offline-first architecture with no backend dependencies

Note: UWP is deprecated by Microsoft, but the app remains functional on Windows 10 and 11. Future versions may migrate to modern platforms like WinUI 3 or .NET MAUI.

## Development

### Prerequisites

- Visual Studio 2022 (or later) with UWP development tools
- Windows 10/11 SDK (10.0.19041.0 or later recommended)
- .NET development workload

### Installation for Developers

1. Install Visual Studio 2022 or later
2. In Visual Studio Installer, ensure you have:
   - Universal Windows Platform development workload
   - Windows 11 SDK (10.0.22621.0) or Windows 10 SDK (10.0.19041.0)
   - Download older SDKs from [Windows SDK Archive](https://learn.microsoft.com/en-us/windows/apps/windows-sdk/downloads-archive) if needed
3. Clone the repository:
   ```
   git clone https://github.com/microkost/100-words.git
   ```
4. Open `100words.sln` in Visual Studio
5. Restore NuGet packages
6. Build and run

### Publishing to Microsoft Store

For publishing certificates and Microsoft Store deployment, see: http://go.microsoft.com/fwlink/?LinkID=241478

### Required Dependencies

The project uses the following NuGet packages:

- **Microsoft.NETCore.UniversalWindowsPlatform** (6.2.14) - UWP runtime
- **Microsoft.Toolkit.Uwp.Notifications** (7.1.3) - Live Tile notifications

### Project Structure

```
100words/
├── Dictionary.cs          # Hardcoded vocabulary data for all languages
├── Phrase.cs             # Data model for multi-language phrases
├── MainPage.xaml         # Main UI layout
├── MainPage.xaml.cs      # Core app logic and Live Tile updates
├── App.xaml.cs           # Application lifecycle management
└── Assets/               # Images, flags, and app icons
```

For detailed technical architecture, see [ARCHITECTURE.md](ARCHITECTURE.md).

### Repository Links

- **Source Code**: https://github.com/microkost/100-words
- **Microsoft Store**: https://apps.microsoft.com/detail/9MWRDJGPXZ6Q

## Contributing

Contributions are welcome! Whether you want to:
- Add translations for new languages
- Fix bugs or improve features
- Enhance documentation

Please feel free to submit issues and pull requests.

## License

This project is open source. Please check the repository for license details.

## Documentation

- [ARCHITECTURE.md](ARCHITECTURE.md) - Technical architecture and design documentation for developers

---

<p align="center">Made with care for language learners</p>
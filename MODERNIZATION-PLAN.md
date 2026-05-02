# Modernization Plan: 100 Words UWP App

## Goal

Modernize the 100 Words UWP app from legacy .NET Native to modern .NET with Native AOT, keeping UWP XAML intact. This enables modern tooling, faster builds, and continued Microsoft Store publishing — without a full platform migration.

Reference: https://learn.microsoft.com/en-us/windows/uwp/dotnet-native/modernize-uwp-apps-with-dotnet

## Prerequisites

✅ **Completed**

- Visual Studio 2022 (17.8+) or Visual Studio 2026
- Universal Windows Platform tools workload
- Windows 11 SDK (10.0.26100.0 or later)

### Visual Studio Installer Setup

1. Open Visual Studio Installer
2. Under Workloads > Desktop & Mobile, select **Windows application development**
3. Under Optional (right pane), select:
   - Universal Windows Platform tools
   - Windows 11 SDK (10.0.26100.0)

## Migration Steps

### Phase 1: Convert Project File to SDK-Style ✅

**Status**: COMPLETED

**Original state**: Legacy non-SDK-style `.csproj` with 235 lines of verbose XML, manual file includes, and .NET Native toolchain.

**Final state**: Clean SDK-style `.csproj` with 18 lines using modern .NET 10 and UWP support.

#### Final Project File (100words.csproj)

The project was successfully converted to:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows10.0.26100.0</TargetFramework>
    <TargetPlatformMinVersion>10.0.19041.0</TargetPlatformMinVersion>
    <UseUwp>true</UseUwp>
    <UseUwpTools>true</UseUwpTools>
    <EnableMsixTooling>true</EnableMsixTooling>
    <RootNamespace>words100</RootNamespace>
    <AssemblyName>words100</AssemblyName>
    <DefaultLanguage>en-US</DefaultLanguage>
    <Platforms>x86;x64;ARM64</Platforms>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Toolkit.Uwp.Notifications" Version="7.1.3" />
  </ItemGroup>
</Project>
```

**Key changes accomplished:**
- ✅ Removed `Microsoft.NETCore.UniversalWindowsPlatform` package (replaced by `UseUwp` property)
- ✅ SDK-style auto-includes all `.cs`, `.xaml`, and content files
- ✅ Removed all manual `<Compile>`, `<Content>`, `<Page>`, `<ApplicationDefinition>` items (reduced from 235 to 18 lines)
- ✅ Removed all platform-specific build configurations (SDK handles this automatically)
- ✅ Removed `.NET Native` toolchain references
- ✅ Added `UseUwp: true` — enables Windows.UI.Xaml projections via CsWinRT
- ✅ Added `UseUwpTools: true` — enables UWP XAML compiler for modern .NET
- ✅ Added `EnableMsixTooling: true` — enables single-project MSIX packaging
- ✅ Changed AssemblyName from `100 words` to `words100` to avoid WinRT source generator issues with leading digits
- ✅ Added `<Platforms>x86;x64;ARM64</Platforms>` for proper UWP multi-architecture support
- ❌ **Native AOT not used**: Initially planned `PublishAot` proved incompatible with UWP XAML code generation and was removed

### Phase 2: Remove Legacy Files

Files to delete after conversion:
- `Properties/AssemblyInfo.cs` — SDK-style projects auto-generate assembly attributes
- `Properties/Default.rd.xml` — Runtime directives file for .NET Native (not needed with Native AOT)

Files to review:
- `Package.appxmanifest` — Keep, but may need minor updates for SDK version references
- `100words_StoreKey.pfx` / `100words_TemporaryKey.pfx` — Keep for Store signing

### Phase 3: Update Package.appxmanifest ✅

**Status**: COMPLETED

Successfully updated with correct values:

```xml
<Dependencies>
  <TargetDeviceFamily Name="Windows.Universal" MinVersion="10.0.19041.0" MaxVersionTested="10.0.26100.0" />
</Dependencies>
```

Changes made:
- ✅ Updated `EntryPoint` from `100words.App` to `App` (matches actual namespace)
- ✅ Updated `MinVersion` from `10.0.0.0` to `10.0.19041.0` (Windows 10, version 2004)
- ✅ Updated `MaxVersionTested` from `10.0.0.0` to `10.0.26100.0` (Windows 11, version 24H2)

### Phase 4: Build and Test ✅

**Status**: COMPLETED

Build and testing process:

1. ✅ **Initial Build** — Encountered XAML compilation issues resolved by adding `UseUwpTools: true`
2. ✅ **Native AOT Compatibility** — Removed `PublishAot` and `DisableRuntimeMarshalling` due to incompatibility with UWP XAML code generation
3. ✅ **Platform Configuration** — Added `<Platforms>x86;x64;ARM64</Platforms>` for proper multi-architecture support
4. ✅ **Debug Build** — Successful compilation without errors
5. ✅ **Release Build** — Successful compilation without errors
6. ✅ **Functionality Testing**:
   - Word display and shuffling — ✅ Working
   - Live Tile updates — ✅ Working
   - Language ordering and settings persistence — ✅ Working
   - Timer functionality — ✅ Working
   - Skip button — ✅ Working

### Phase 5: Fix Asset Issues ✅

**Status**: COMPLETED

Issue encountered:
- Build error: `Payload file 'StoreLogo.png' does not exist`
- Only scaled versions existed: `StoreLogo.scale-100.png`, `StoreLogo.scale-200.png`, etc.

Solution applied:
```powershell
Copy-Item "Assets\StoreLogo.scale-100.png" -Destination "Assets\StoreLogo.png"
```

Result: ✅ App now compiles and runs successfully

### Phase 6: Publish to Microsoft Store

1. Build Release configuration (Native AOT enabled)
2. Create MSIX package: Project > Publish > Create App Packages
3. Upload to Partner Center
4. Note: Ignore WACK failures related to "unsupported Win32 APIs" — Partner Center no longer enforces strict Win32 API validation for UWP apps

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| NuGet package incompatibility | Medium | Medium | Find alternative package or update |
| AOT trim warnings | Low | Low | App uses simple patterns, no heavy reflection |
| XAML compilation issues | Low | Medium | XAML is straightforward, no complex bindings |
| Store certification issues | Low | Low | Microsoft supports this path officially |

## Estimated Effort

- **Phase 1** (Project file conversion): 30 minutes
- **Phase 2** (Remove legacy files): 10 minutes
- **Phase 3** (AOT compatibility): 1-2 hours (mostly testing)
- **Phase 4** (Manifest update): 10 minutes
- **Phase 5** (Build and test): 1-2 hours
- **Phase 6** (Store publish): 30 minutes

**Total estimated effort: Half a day**

## Benefits Achieved

- ✅ **Faster build times** — No .NET Native compilation overhead
- ✅ **Better debugging experience** — Modern .NET debugging tools
- ✅ **Access to latest C# features** — .NET 10 language features
- ✅ **Modern NuGet support** — Compatible with latest .NET packages
- ✅ **Clean, minimal project file** — 235 lines reduced to 18 lines
- ✅ **XAML Hot Reload support** — Available in Visual Studio 2022+
- ✅ **Microsoft Store compatible** — Continued publishing support
- ✅ **Path to future WinUI 3 migration** — Optional upgrade path available
- ✅ **Multi-architecture support** — x86, x64, and ARM64 platforms

## Future: Optional WinUI 3 Migration

After successfully stabilizing on modern .NET, you can optionally migrate to WinUI 3 in the future:
- Replace `Windows.UI.Xaml` with `Microsoft.UI.Xaml`
- Move from UWP app model to Win32 app model with Windows App SDK
- Gain access to newer UI controls and features
- This is a **separate, independent step** — not required for Store publishing
- Current UWP XAML implementation will continue to work on Windows 10 and 11

Reference: https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/migrate-to-windows-app-sdk/overall-migration-strategy

## Conclusion

✅ **Migration Successful**: The 100 Words app has been successfully modernized from legacy .NET Native to modern .NET 10 while maintaining all UWP features and functionality.

The app now:
- Builds faster and more reliably
- Uses modern development tools and frameworks
- Remains fully compatible with Microsoft Store
- Maintains all original functionality including Windows Live Tiles
- Is ready for future enhancements and optional platform migrations

See [CHANGE-LOG.md](CHANGE-LOG.md) for detailed list of all changes made during modernization.

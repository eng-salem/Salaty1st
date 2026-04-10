# Architecture Decision Records

This directory contains Architecture Decision Records (ADRs) for the PrayerWidget migration from WPF to Avalonia UI.

## Overview

An Architecture Decision Record (ADR) captures an important architectural decision made along with its context and consequences. Each ADR follows the format defined in the migration roadmap.

## Records

### ADR-001: UI Framework Selection
**Status**: Accepted

**Context**: The existing PrayerWidget application uses WPF, which is Windows-only. To support Ubuntu, we need a cross-platform UI framework.

**Decision**: Use Avalonia UI as the replacement for WPF.

**Rationale**:
- XAML-based (minimal learning curve for existing WPF developers)
- True cross-platform support (Windows, Linux, macOS)
- Active community and commercial backing
- Similar architecture to WPF (XAML + Code-behind/MVVM)
- Can migrate incrementally

**Consequences**:
- **Positive**: Single codebase for all platforms, familiar XAML syntax, good performance
- **Negative**: Some WPF features not available, smaller ecosystem than WPF, requires .NET 6+

**Alternatives Considered**:
- MAUI: Too mobile-focused, limited desktop customization
- Uno Platform: Steeper learning curve, smaller community
- Qt/Qml: C++ dependencies, requires learning new language
- GTK#/Eto.Forms: Different paradigm, limited styling options

---

### ADR-002: MVVM Framework Selection
**Status**: Accepted

**Context**: The current WPF application uses code-behind approach. Moving to MVVM improves testability and separation of concerns.

**Decision**: Use CommunityToolkit.Mvvm (formerly Microsoft.Toolkit.Mvvm).

**Rationale**:
- Microsoft-maintained, part of .NET Community Toolkit
- Lightweight (no external dependencies)
- Source generators reduce boilerplate
- Excellent performance
- Simple to learn and use

**Consequences**:
- **Positive**: Less boilerplate, auto-generated code, well-supported
- **Negative**: New syntax to learn, requires .NET 6+

**Alternatives Considered**:
- ReactiveUI: Powerful but steep learning curve
- Prism: Over-engineered for this application
- MVVM Light: Deprecated
- Caliburn.Micro: Heavier, more complex

---

### ADR-003: Cross-Platform Device Identification
**Status**: Accepted

**Context**: The current application uses Windows Registry and WMI for device identification, which doesn't work on Linux.

**Decision**: Implement cross-platform device fingerprinting using available .NET APIs.

**Rationale**:
- No external dependencies
- Works on Windows, Linux, and macOS
- Privacy-friendly (no sensitive hardware access)
- Sufficient for analytics purposes

**Implementation Approach**:
1. Use `Environment.MachineName` and `Environment.UserName`
2. Use `Environment.OSVersion` for platform identification
3. Use `NetworkInterface.GetAllNetworkInterfaces()` for MAC addresses
4. Hash combined information to create consistent device ID

**Consequences**:
- **Positive**: Cross-platform, no special permissions needed, privacy-respecting
- **Negative**: May change if network interfaces change, less stable than hardware ID

**Alternatives Considered**:
- Hardware-based fingerprinting: Requires native libraries, platform-specific
- UUID storage in file: Loses device if file deleted
- Machine GUID from Windows only: Not cross-platform

---

### ADR-004: Database Strategy
**Status**: Accepted

**Context**: Current application uses SQLite with Microsoft.Data.Sqlite 5.0.17.

**Decision**: Continue using SQLite, upgrade to Microsoft.Data.Sqlite 8.0.x.

**Rationale**:
- SQLite is natively cross-platform
- Microsoft.Data.Sqlite works on all .NET platforms
- No schema changes required
- Proven reliability

**Consequences**:
- **Positive**: Zero migration effort for data layer
- **Negative**: None

**Migration Notes**:
- Update NuGet package to latest version
- Connection strings remain the same
- SQL queries work identically

---

### ADR-005: Audio Playback Strategy
**Status**: Proposed

**Context**: Current application uses System.Media.SoundPlayer for athan audio, which is Windows-only.

**Decision**: Use platform-specific audio libraries with abstraction layer.

**Rationale**:
- Audio is platform-specific (Windows: WASAPI, Linux: PulseAudio/ALSA)
- No perfect cross-platform solution exists
- Abstraction layer allows platform-appropriate implementations

**Implementation Approach**:
1. Create `IAudioService` interface
2. Implement `WindowsAudioService` using NAudio or Windows APIs
3. Implement `LinuxAudioService` using NAudio.Linux or native bindings
4. Implement `MacAudioService` for macOS support
5. Use dependency injection to select appropriate implementation

**Consequences**:
- **Positive**: Works on all platforms with native quality
- **Negative**: More complex setup, platform-specific code to maintain

**Alternatives Considered**:
- LibVLC: Heavy dependency, overkill for simple audio
- SDL2: Gaming-focused, complex setup
- Cross-platform wrapper library: Limited options available

---

### ADR-006: System Tray / Notification Area
**Status**: Proposed

**Context**: Current application uses Hardcodet.NotifyIcon.Wpf for Windows system tray.

**Decision**: Implement platform-specific notification approaches.

**Rationale**:
- System tray concepts differ across platforms
- Linux has multiple desktop environments with different APIs
- Windows has traditional system tray

**Implementation Approach**:
1. Windows: Use native Win32 APIs or Avalonia's tray support
2. Linux: Support both AppIndicator (Ubuntu/GNOME) and StatusNotifierItem (KDE)
3. macOS: Use NSStatusBar

**Avalonia Built-in Support**:
```csharp
// Avalonia has TrayIcon support
var trayIcon = new TrayIcon
{
    Icon = new WindowIcon(iconStream),
    ToolTipText = "Salaty First",
    Menu = new NativeMenu
    {
        Items =
        {
            new NativeMenuItem("Show") { Command = ShowCommand },
            new NativeMenuItem("Settings") { Command = SettingsCommand },
            new NativeMenuItemSeparator(),
            new NativeMenuItem("Exit") { Command = ExitCommand }
        }
    }
};
```

**Consequences**:
- **Positive**: Native look and feel on each platform
- **Negative**: Platform-specific code, testing complexity

---

### ADR-007: Project Structure
**Status**: Accepted

**Context**: Need to organize code for maintainability and testability.

**Decision**: Use Clean Architecture with separate projects for UI, Core, and Tests.

**Structure**:
```
Salaty.Avalonia/
├── src/
│   ├── Salaty.Avalonia/        # UI Layer (Avalonia-specific)
│   │   ├── Views/              # Windows and Controls
│   │   ├── ViewModels/         # MVVM ViewModels
│   │   ├── Styles/             # Avalonia Styles
│   │   ├── Assets/             # Images, Fonts, Sounds
│   │   └── Services/           # UI-specific services (audio, tray)
│   └── Salaty.Avalonia.Core/   # Business Logic
│       ├── Models/             # Domain models
│       ├── Services/           # Business services
│       └── Data/               # Database/repositories
└── tests/
    └── Salaty.Avalonia.Tests/  # Unit and integration tests
```

**Consequences**:
- **Positive**: Clear separation, testable, maintainable
- **Negative**: Slightly more complex build, project references

---

### ADR-008: Dependency Injection
**Status**: Accepted

**Context**: Need to manage dependencies and enable testability.

**Decision**: Use Microsoft.Extensions.DependencyInjection.

**Rationale**:
- Built into .NET
- Well-documented
- Works with CommunityToolkit.Mvvm
- Supports service lifetime management

**Configuration**:
```csharp
var services = new ServiceCollection();

// Core services
services.AddSingleton<PrayerService>();
services.AddSingleton<SettingsService>();
services.AddSingleton<DeviceIdentificationService>();

// Platform-specific services
if (OperatingSystem.IsWindows())
    services.AddSingleton<IAudioService, WindowsAudioService>();
else if (OperatingSystem.IsLinux())
    services.AddSingleton<IAudioService, LinuxAudioService>();

// ViewModels
services.AddTransient<MainWindowViewModel>();

var serviceProvider = services.BuildServiceProvider();
```

**Consequences**:
- **Positive**: Testable, maintainable, clear dependency graph
- **Negative**: Slightly more setup code

---

### ADR-009: .NET Version
**Status**: Accepted

**Context**: Need to choose target framework for cross-platform support.

**Decision**: Use .NET 8 (LTS).

**Rationale**:
- Long-term support until November 2026
- Latest performance improvements
- Full Avalonia support
- Native AOT compilation available
- Required by latest dependencies

**Consequences**:
- **Positive**: Latest features, performance, security patches
- **Negative**: Requires .NET 8 runtime or self-contained deployment

**Alternatives Considered**:
- .NET 6: Ending support soon (November 2024)
- .NET 7: Not LTS
- .NET Framework 4.8: Not cross-platform

---

### ADR-010: Configuration and Settings Storage
**Status**: Accepted

**Context**: Application needs to store user settings across sessions.

**Decision**: Use JSON file storage with Microsoft.Extensions.Configuration.

**Rationale**:
- Cross-platform (uses standard file system)
- Human-readable format
- Easy to backup/migrate
- Industry standard

**Storage Location**:
- Windows: `%LOCALAPPDATA%/Salaty/settings.json`
- Linux: `~/.config/Salaty/settings.json`
- macOS: `~/Library/Application Support/Salaty/settings.json`

**Implementation**:
```csharp
var configPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "Salaty",
    "settings.json");
```

**Consequences**:
- **Positive**: Cross-platform, simple, standard
- **Negative**: Manual schema migration if settings change

---

## Status Key

- **Proposed**: Under discussion
- **Accepted**: Decision made, being implemented
- **Deprecated**: Decision no longer valid, superseded
- **Superseded by ADR-XXX**: Replaced by newer decision

## Contributing

When adding a new ADR:
1. Use the next sequential number
2. Follow the established format
3. Include alternatives considered
4. Document both positive and negative consequences
5. Update this index

# PrayerWidget Setup - Release Summary

## Build Information

| Item | Details |
|------|---------|
| **Version** | 1.0.2 |
| **Build Date** | 2026-03-08 |
| **Output** | `Salaty.setup\Release\Salaty.setup.msi` |
| **Size** | ~30.6 MB |
| **Platform** | Windows x64 |
| **Framework** | .NET 5.0 (WPF) |

---

## What's New in This Release

### 🔧 Critical Fixes

1. **SQLite Write Permission Fix**
   - Database now copies to user's AppData folder on first run
   - Fixes "Access Denied" errors when saving settings after installation
   - Path: `%LOCALAPPDATA%\PrayerWidget\salaty.sqlite`

### ✨ Setup Improvements

1. **Product Information**
   - Manufacturer: Salaty.First
   - Support URL: https://github.com/eng-salem/Salaty1st
   - Product icon included in installer

2. **Shortcuts**
   - ✅ Desktop shortcut created
   - ✅ Start Menu shortcut created

3. **Repair/Uninstall**
   - ✅ Standard Windows Installer repair support
   - ✅ Clean uninstall via Add/Remove Programs

---

## Installation

### Requirements
- Windows 10/11 (x64)
- **.NET 5.0 Runtime** (required - app is framework-dependent)
  - Download: https://dotnet.microsoft.com/download/dotnet/5.0
  - Install: **Windows x64 Desktop Runtime 5.0.x**

### Install Steps
1. Run `Salaty.setup.msi`
2. Follow the installation wizard
3. Choose installation folder (default: `C:\Program Files\Salaty.First\`)
4. Complete installation

### After Installation

**The application should be run from the installed location:**
```
C:\Program Files\Salaty.First\Salaty.First.exe
```

**DO NOT run from Debug folder:**
```
❌ PrayerWidget\bin\Debug\...\Salaty.First.exe  (Development only)
✅ C:\Program Files\Salaty.First\Salaty.First.exe  (Correct location)
```

### .NET 5.0 Runtime Required

If you get an error asking to install .NET when running from `C:\Program Files (x86)\Salaty.First\`, you have two options:

**Option 1: Install .NET 5.0 Runtime (Recommended)**
- Download: https://dotnet.microsoft.com/download/dotnet/5.0
- Install: **Windows x64 Desktop Runtime 5.0.x**

**Option 2: Copy Runtime Files (No Installation Needed)**
1. Right-click `Copy_Runtime_Files.bat` in the project folder
2. Select **"Run as administrator"**
3. This copies 284 runtime files (~137 MB) to the installation folder

The Debug folder works without installing .NET because it uses the development environment's runtime.

### First Run
On first launch, the application will:
1. Copy database to `%LOCALAPPDATA%\PrayerWidget\`
2. Load default settings
3. Display the prayer times widget

---

## Project Structure

```
PrayerWidget/
├── PrayerWidget/               # Main WPF Application
│   ├── Services/
│   │   ├── DatabaseManager.vb  # NEW: Manages AppData database
│   │   ├── SqlitePrayerService.vb
│   │   ├── PrayerApiService.vb
│   │   └── GeocodingService.vb
│   ├── SettingsManager.vb      # UPDATED: Uses AppData path
│   └── ...
├── Salaty.setup/               # Setup Project
│   └── Release/
│       └── Salaty.setup.msi    # Installer output
└── build_setup.bat             # Build script
```

---

## Technical Changes

### New Files
- `Services/DatabaseManager.vb` - Manages database location and migration

### Modified Files
- `SettingsManager.vb` - Uses `DatabaseManager.GetDatabasePath()`
- `PrayerApiService.vb` - Uses `DatabaseManager.GetDatabasePath()`
- `Salaty.setup.vdproj` - Updated for Release/x64, added icon and product info

---

## Database Location

| Before | After |
|--------|-------|
| `[InstallDir]\salaty.sqlite` | `%LOCALAPPDATA%\PrayerWidget\salaty.sqlite` |

### Benefits
- ✅ No admin permissions required for settings
- ✅ Settings persist across app updates
- ✅ Clean uninstall removes user data
- ✅ Multiple users can have separate settings

---

## Build Instructions

### Using Visual Studio
1. Open `PrayerWidget\PrayerWidget.sln`
2. Select **Release** configuration
3. Build → Build Solution

### Using Command Line
```batch
cd c:\Users\Administrator\Desktop\PrayerWidget
build_setup.bat
```

### Output Location
```
Salaty.setup\Release\Salaty.setup.msi
```

---

## Testing Checklist

- [ ] Install on clean Windows 10/11
- [ ] Verify desktop shortcut created
- [ ] Verify Start Menu shortcut created
- [ ] Open app and change settings
- [ ] Close and reopen - verify settings persist
- [ ] Test Repair via Add/Remove Programs
- [ ] Test Uninstall via Add/Remove Programs
- [ ] Verify database removed on uninstall

---

## Known Issues

1. **Auto-start**: Windows Registry auto-start not implemented (setup project limitation)
   - *Workaround*: Users can manually add to Startup folder

2. **Dependency Warnings**: Build shows warnings for system assemblies
   - *Status*: Harmless, does not affect functionality

---

## Support

| Resource | Link |
|----------|------|
| GitHub | https://github.com/eng-salem/Salaty1st |
| Facebook | https://www.facebook.com/Salaty.1st |
| Issues | https://github.com/eng-salem/Salaty1st/issues |

---

## Version History

| Version | Date | Notes |
|---------|------|-------|
| 1.0.2 | 2026-03-08 | Fixed SQLite permissions, improved setup |
| 1.0.1 | - | Previous release |
| 1.0.0 | - | Initial release |

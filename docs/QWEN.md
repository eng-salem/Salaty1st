# PrayerWidget - Project Context

## Project Overview

**PrayerWidget** (Salaty.First) is a WPF-based desktop application for displaying Islamic prayer times. It's a lightweight, customizable widget that sits on the desktop and provides:

- **Real-time countdown** to the next prayer time
- **Visual progress ring** showing time progression between prayers
- **Hijri date display** with moon sighting adjustment (±1 day)
- **Athan sound notifications** at prayer times
- **Toast notifications** before/after prayers with customizable timing
- **Islamic quotes** display with audio notification (new feature)

### Key Features

| Feature | Description |
|---------|-------------|
| **Offline Calculation** | Uses PrayTime algorithm - no internet required for prayer times |
| **23,000+ Cities** | SQLite database with worldwide city coverage |
| **IP Location Detection** | Auto-detect location from IP address (ipwhois.app) |
| **Geocoding** | Address search via Nominatim (OpenStreetMap) API |
| **6 Calculation Methods** | MWL, ISNA, Makkah, Egypt, Karachi, Tehran, Jafari |
| **Asr Method Selection** | Shafii (standard) or Hanafi (later) |
| **Multi-language** | English, Arabic, French, Turkish, Urdu |
| **Islamic Quotes** | Periodic display of hadith/quotes with audio |

### Recent Changes (v1.0.2)

- **Islamic Quotes Tab**: Dedicated tab for quote settings (enabled by default)
- **Quote Audio**: Plays `quotes.mp3` when displaying quotes
- **JSON Parsing Fix**: Handles API response with `post_content` field
- **Settings Gear Button**: Toast notifications include gear icon to open settings

---

## Technology Stack

| Component | Technology |
|-----------|-----------|
| **Framework** | .NET 5.0 (WPF) |
| **Language** | VB.NET |
| **Database** | SQLite (Microsoft.Data.Sqlite 10.0.3) |
| **UI** | XAML with custom styling |
| **JSON** | System.Text.Json 10.0.3 |
| **Geocoding** | Nominatim (OpenStreetMap) API |
| **IP Geolocation** | ipwhois.app API |
| **Quotes API** | https://quotes.islamicquotes.deno.net/quote |
| **Taskbar Icon** | Hardcodet.NotifyIcon.Wpf 2.0.1 |

---

## Project Structure

```
PrayerWidget/
├── PrayerWidget/                     # Main WPF Application
│   ├── Application.xaml/vb           # App entry point, analytics tracking
│   ├── MainWindow.xaml/vb            # Main widget window (circular progress)
│   ├── SettingsManager.vb            # Settings persistence (SQLite)
│   ├── PrayerWidget.sln              # Solution file
│   ├── PrayerWidget.vbproj           # Project file
│   ├── version.json                  # Version info, features, changelog
│   ├── salaty.sqlite                 # SQLite database (30MB)
│   │
│   ├── Models/
│   │   ├── PrayerTimes.vb            # PrayTime class + DailyPrayerTimes model
│   │   ├── WidgetSettings.vb         # Widget configuration model
│   │   └── ReminderSettings.vb       # Prayer reminder settings model
│   │
│   ├── Services/
│   │   ├── DatabaseManager.vb        # Manages DB location (AppData vs Install)
│   │   ├── PrayerApiService.vb       # Main prayer calculation service
│   │   ├── SqlitePrayerService.vb    # Database access layer
│   │   ├── GeocodingService.vb       # Address/IP geocoding
│   │   ├── QuoteService.vb           # Islamic quotes fetching
│   │   ├── LocalizationService.vb    # Multi-language support
│   │   └── GoogleAnalyticsService.vb # Anonymous usage tracking
│   │
│   ├── Forms/
│   │   ├── SettingsWindow.xaml/vb        # Tabbed settings dialog
│   │   ├── QuoteNotificationWindow.xaml/vb # Quote display window
│   │   ├── ToastNotification.xaml/vb     # Prayer reminder toast
│   │   └── ShowAllPrayersWindow.xaml/vb  # All prayers display
│   │
│   ├── Resources/
│   │   └── MP3/
│   │       ├── quotes.mp3            # Quote notification audio
│   │       ├── Reminder0.mp3         # Reminder sound
│   │       └── *_athan.mp3           # Athan sound files
│   │
│   ├── Translations/
│   │   ├── en.json                   # English translations
│   │   ├── ar.json                   # Arabic translations
│   │   ├── fr.json                   # French translations
│   │   ├── tr.json                   # Turkish translations
│   │   └── ur.json                   # Urdu translations
│   │
│   └── Icons/
│       ├── salaty.first.ico          # Application icon
│       └── gear.ico                  # Settings icon
│
├── Salaty.setup/                     # Windows Installer Setup Project
│   └── Release/
│       └── Salaty.setup.msi          # MSI installer output
│
├── build_setup.bat                   # Build script for Visual Studio
└── SETUP_SUMMARY.md                  # Release documentation
```

---

## Building and Running

### Prerequisites

- **Windows 10/11** (WPF requirement)
- **.NET 5.0 SDK** or later
- **Visual Studio 2022** (for setup project)

### Build Commands

```bash
# Navigate to project directory
cd c:\Users\Administrator\Desktop\PrayerWidget\PrayerWidget

# Build the project
dotnet build PrayerWidget.sln

# Run the application (development)
dotnet run --project PrayerWidget.vbproj

# Publish for deployment (framework-dependent)
dotnet publish -c Release -r win-x64 --self-contained false

# Build setup project (requires Visual Studio)
build_setup.bat
```

### Output Locations

| Configuration | Output Path |
|--------------|-------------|
| Debug | `bin\Debug\net5.0-windows7.0\` |
| Release | `bin\Release\net5.0-windows7.0\` |
| Setup MSI | `Salaty.setup\Release\Salaty.setup.msi` |

### Installation

1. Run `Salaty.setup.msi`
2. Installs to: `C:\Program Files\Salaty.First\`
3. Database copied to: `%LOCALAPPDATA%\PrayerWidget\salaty.sqlite`

---

## Database Architecture

### Database Location Strategy

```
┌─────────────────────────────────────────┐
│     Installation Directory (Read-Only)   │
│     C:\Program Files\Salaty.First\       │
│     ┌────────────────┐                   │
│     │ salaty.sqlite  │ ← Source DB       │
│     └────────────────┘                   │
└─────────────────────────────────────────┘
              │ Copy on first run
              ▼
┌─────────────────────────────────────────┐
│     User AppData (Read-Write)            │
│     %LOCALAPPDATA%\PrayerWidget\         │
│     ┌────────────────┐                   │
│     │ salaty.sqlite  │ ← Active DB       │
│     └────────────────┘                   │
└─────────────────────────────────────────┘
```

### Key Tables

**settings** - User preferences
```sql
Name TEXT PRIMARY KEY, Value TEXT
```

**Cities** - 23,000+ cities
```sql
City_ID, City_Name, La (Lat), Lo (Lon), Country_Code, Time_Zone, Tz
```

**Countries** - 205 countries
```sql
Country_Code, Country_Name, Method (default), AdjustHighLats, Elev
```

---

## Key Services

### DatabaseManager
Manages database file location:
- `GetDatabasePath()` - Returns AppData path, copies DB if needed
- `GetAppDataPath()` - Returns AppData path without copying
- `GetInstallPath()` - Returns installation directory path

### PrayerApiService
Main prayer calculation service:
- `GetPrayerTimesAsync()` - Calculate prayer times for a location
- `GetLocationFromIPAsync()` - Auto-detect location from IP
- `FindCityByAddressAsync()` - Geocode address to city
- `GetCalculationMethodsAsync()` - Fetch calculation methods

### QuoteService
Islamic quotes fetching:
- `GetRandomQuote()` - Fetch random quote from API
- Handles `post_content` JSON field format
- Extracts source from hadith reference markers

### LocalizationService
Multi-language support:
- `Initialize(language)` - Load translations
- `GetString(key)` - Get translated string
- `SetLanguage(code)` - Change language

---

## Settings Storage

### Key Settings (SQLite)

| Name | Type | Default | Description |
|------|------|---------|-------------|
| `EnableQuotes` | 0/1 | 1 | Enable Islamic quotes |
| `QuoteInterval` | int | 30 | Minutes between quotes |
| `EnableQuoteAudio` | 0/1 | 1 | Play quotes.mp3 |
| `WidgetOpacity` | double | 70 | Widget transparency % |
| `NotificationOpacity` | double | 95 | Toast transparency % |
| `ShowProgressRing` | 0/1 | 1 | Show circular progress |
| `HijriAdjustment` | int | 0 | ±1 day adjustment |
| `AsrMethod` | 0/1 | 0 | Shafii/Hanafi |
| `HasCompletedSetup` | 0/1 | 0 | First-run flag |
| `Language` | string | "en" | UI language |

### Prayer Reminder Settings

| Pattern | Example | Description |
|---------|---------|-------------|
| `{Prayer}Reminder_EnabledBefore` | `FajrReminder_EnabledBefore` | Enable before reminder |
| `{Prayer}Reminder_MinutesBefore` | `FajrReminder_MinutesBefore` | Minutes before prayer |
| `{Prayer}Reminder_EnabledAfter` | `FajrReminder_EnabledAfter` | Enable after reminder |
| `{Prayer}Reminder_MinutesAfter` | `FajrReminder_MinutesAfter` | Minutes after prayer |

---

## Calculation Methods

| ID | Name | Fajr Angle | Isha Angle |
|----|------|------------|------------|
| 0 | الجعفري (Jafari) | 16° | 14° |
| 1 | رابطة العالم الإسلامي (MWL) | 18° | 17° |
| 2 | الهيئة المصرية (Egyptian) | 19.5° | 17.5° |
| 3 | كراتشي (Karachi) | 18° | 18° |
| 4 | أم القرى (Umm Al-Qura) | 18.5° | 90min after Maghrib |
| 5 | ISNA (North America) | 15° | 15° |

---

## External APIs

### Quotes API
- **URL**: `https://quotes.islamicquotes.deno.net/quote`
- **Response Format**:
```json
{
  "post_content": "رسول الله صلى الله عليه وسلم قد قال...",
  "post_type": "post",
  "post_date": "2014-11-08 18:48:16"
}
```

### IP Geolocation
- **URL**: `https://ipwhois.app/json/`
- **Rate Limit**: Unlimited (free tier)
- **Returns**: latitude, longitude, city, country_code

### Nominatim Geocoding
- **URL**: `https://nominatim.openstreetmap.org/search`
- **Rate Limit**: 1 request/second
- **User-Agent**: Required header

---

## Development Conventions

### Code Style
- **Language**: VB.NET with `Option Infer On`, `Option Strict Off`
- **Naming**: PascalCase for public members, `_prefix` for private fields
- **Async**: Use `Async`/`Await` for I/O operations
- **Error Handling**: Try-Catch with `Console.WriteLine` for logging

### UI Patterns
- **XAML Code-Behind**: `.xaml` + `.xaml.vb` pairs
- **Resource Dictionaries**: Modern styles for ComboBox, TextBox, Button
- **Glass Effect**: Custom drop shadow and transparency
- **Tabbed Settings**: Location, Appearance, Notifications, Islamic Quotes

### Testing Practices
- Debug logging to `debug.log` in application directory
- Console.WriteLine for service-level logging
- Manual testing checklist in SETUP_SUMMARY.md

---

## Deployment

### MSI Installer Features

| Feature | Description |
|---------|-------------|
| **Shortcuts** | Desktop + Start Menu |
| **Repair/Uninstall** | Windows Add/Remove Programs |
| **Product Icon** | salaty.first.ico in installer |
| **Platform** | Windows x64 only |

### Runtime Requirements

Users need **.NET 5.0 Desktop Runtime** installed:
- Download: https://dotnet.microsoft.com/download/dotnet/5.0
- Alternative: Copy runtime files using `Copy_Runtime_Files.bat`

---

## Known Limitations

1. **Platform**: Windows-only (WPF dependency)
2. **High Latitudes**: Uses MidNight method for extreme latitudes
3. **Geocoding Rate Limit**: Nominatim API limited to 1 req/sec
4. **IP Location Accuracy**: City-level approximation
5. **Language Switch**: Circular widget requires restart for language update

---

## Support & Links

| Resource | URL |
|----------|-----|
| **GitHub** | https://github.com/eng-salem/Salaty1st |
| **Releases** | https://github.com/eng-salem/Salaty1st/releases/latest |
| **Facebook** | https://www.facebook.com/Salaty.1st |
| **Issues** | https://github.com/eng-salem/Salaty1st/issues |

---

## Version History

| Version | Date | Notes |
|---------|------|-------|
| 1.0.2 | 2026-03-08 | Islamic Quotes tab, audio playback, JSON fix |
| 1.0.1 | - | SQLite permissions fix, AppData migration |
| 1.0.0 | - | Initial release |

## Qwen Added Memories
- PrayerWidget (Salaty.First) build process: 1) Update versions in AssemblyInfo.vb, version.json, Setup.vdproj (ProductVersion), 2) Generate new ProductCode and PackageCode GUIDs in Setup.vdproj, 3) Keep UpgradeCode unchanged, 4) Build: dotnet build PrayerWidget.sln -c Release, 5) Publish: dotnet publish PrayerWidget.vbproj -c Release -r win-x64 --self-contained false -o publish, 6) Rebuild Setup.vdproj in Visual Studio, 7) Install with: msiexec /i Setup.msi REINSTALLMODE=voums REINSTALL=ALL /passive /norestart

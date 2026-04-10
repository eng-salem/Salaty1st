# QWEN.md - PrayerWidget Project Context

## Project Overview

**PrayerWidget** is a WPF-based desktop application for displaying Islamic prayer times. It's a lightweight, customizable widget that sits on the desktop and shows:
- Current prayer countdown timer
- Next prayer name
- Previous prayer name with elapsed time
- Visual progress ring showing time progression between prayers
- Sun/Moon icon indicating day/night prayers

### Key Features
- **Offline Prayer Time Calculation** using the PrayTime library (no internet required)
- **SQLite Database Integration** with 23,000+ cities and 205 countries
- **Geocoding Support** - Auto-detect location from address using Nominatim API
- **IP-Based Location Detection** - Auto-detect location from IP address using ipapi.co
- **Multiple Calculation Methods** - MWL, ISNA, Makkah, Egypt, Karachi, Tehran, Jafari
- **Customizable Appearance** - Small/Medium/Large sizes, rounded corners, colors
- **Athan Sound Notifications** - Configurable prayer time alerts
- **Pin to Desktop** - Always-on-top widget functionality

## Links

| Platform | URL |
|----------|-----|
| GitHub | https://github.com/eng-salem/Salaty1st/releases/latest |
| Facebook | https://www.facebook.com/Salaty.1st |

## Technology Stack

| Component | Technology |
|-----------|-----------|
| Framework | .NET 5.0 (WPF) |
| Language | VB.NET |
| Database | SQLite (Microsoft.Data.Sqlite 10.0.3) |
| UI | XAML with custom styling |
| JSON | System.Text.Json 10.0.3 |
| Geocoding | Nominatim (OpenStreetMap) API |
| IP Geolocation | ipapi.co API |

## Project Structure

```
PrayerWidget/
├── Application.xaml              # WPF Application entry point
├── MainWindow.xaml/vb            # Main widget window
├── SettingsManager.vb            # Settings persistence (SQLite)
├── salaty.sqlite                 # SQLite database (cities, countries, settings)
├── version.json                  # Version info, release notes, social links
├── Models/
│   ├── PrayerTimes.vb            # PrayTime class + DailyPrayerTimes model
│   └── WidgetSettings.vb         # Widget configuration model
├── Services/
│   ├── PrayerApiService.vb       # Main prayer calculation service
│   ├── SqlitePrayerService.vb    # Database access layer
│   └── GeocodingService.vb       # Address geocoding service
├── Forms/
│   ├── SettingsWindow.xaml/vb        # Settings dialog (tabbed UI)
│   ├── ShowAllPrayersWindow.xaml/vb  # All prayers display window
│   ├── ReminderSettingsWindow.xaml/vb # Prayer reminder configuration
│   └── ToastNotification.xaml/vb     # Toast notification UI
├── Icons/
│   ├── sun.png                   # Day prayer icon
│   └── moon.png                  # Night prayer icon
└── sqllitedb/                    # CSV exports of database tables
```

## Building and Running

### Prerequisites
- .NET 6.0 SDK or later
- Windows OS (WPF requirement)

### Build Commands
```bash
# Build the project
dotnet build

# Run the application
dotnet run

# Publish for deployment
dotnet publish -c Release -r win-x64 --self-contained
```

### Development
```bash
# Watch for changes (requires dotnet-watch)
dotnet watch run
```

## Database Schema

### Tables Used

**settings** - User preferences
```sql
Name           TEXT PRIMARY KEY
Value          TEXT
```

**Cities** - 23,000+ cities worldwide
```sql
City_Name      TEXT
La             REAL (Latitude)
Lo             REAL (Longitude)
Country_Code   TEXT
Time_Zone      TEXT
City_ID        INTEGER PRIMARY KEY
Tz             INTEGER
```

**Countries** - 205 countries
```sql
Country_Code   TEXT PRIMARY KEY
Country_Name   TEXT
Method         INTEGER (default calculation method)
AdjustHighLats INTEGER
Elev           INTEGER
Country_ID     INTEGER
```

**Methods** - 6 calculation methods
```sql
ID             INTEGER PRIMARY KEY
Name           TEXT (Arabic names)
```

## Key Services

### PrayerApiService
Main service for prayer time calculations:
- `GetPrayerTimesAsync(city, country, method)` - Calculate prayer times
- `GetLocationFromIPAsync()` - Get location from IP address
- `FindCityByAddressAsync(address)` - Geocode address to city
- `GetCountries()` - Get all countries
- `GetCitiesByCountry(code)` - Get cities by country code
- `GetCalculationMethodsAsync()` - Get calculation methods

### SqlitePrayerService
Database access layer:
- `GetSettings()` / `SaveSettings()` - Settings CRUD
- `GetCountries()` - Fetch countries
- `GetCitiesByCountry()` - Fetch cities
- `SearchCities()` - Search cities by name

### GeocodingService
Location detection:
- `GetLocationFromIPAsync()` - Get location from IP address using ipapi.co
- `FindCityByAddressAsync()` - Uses Nominatim API
- `FindNearestCity()` - Optimized SQL query with Haversine formula (500km radius)

## Settings Storage

All settings stored in SQLite `settings` table:

| Name | Type | Description |
|------|------|-------------|
| City_Name | String | Selected city name |
| Country_Code | String | ISO country code |
| Method | Integer | Calculation method ID |
| Size | String | Small/Medium/Large |
| WindowWidth | Integer | Widget width |
| WindowHeight | Integer | Widget height |
| CornerRadius | Integer | Border radius |
| GridMargin | Integer | Inner margin |
| StrokeThickness | Double | Progress ring thickness |
| AthanSound | String | Sound file name |

## Calculation Methods

| ID | Name | Fajr | Isha |
|----|------|------|------|
| 0 | الجعفري | 16° | 14° |
| 1 | رابطة العالم الإسلامي | 18° | 17° |
| 2 | الهيئة المصرية العامة للمساحة | 19.5° | 17.5° |
| 3 | جامعة العلوم الإسلامية بكراتشي | 18° | 18° |
| 4 | تقويم أم القرى | 18.5° | 90min after Maghrib |
| 5 | الإتحاد الإسلامي بأمريكا الشمالية | 15° | 15° |

## Development Conventions

### Code Style
- **Language**: VB.NET with `Option Infer On`, `Option Strict Off`
- **Naming**: PascalCase for public members, camelCase for private fields
- **Async**: Use `Async`/`Await` for I/O operations
- **Error Handling**: Try-Catch with Console.WriteLine for debugging

### File Organization
- XAML files paired with code-behind (.xaml.vb)
- Services in `Services/` folder
- Models in `Models/` folder
- Forms in `Forms/` folder
- Database file copied to output directory

### UI Styling
- Dark theme (#2A2A2A background)
- Glass effect on main widget
- Custom ComboBox and TextBox styles
- DropShadow effects on icons

## Known Limitations

1. **High Latitude Handling**: Uses MidNight method for extreme latitudes
2. **Geocoding Rate Limit**: Nominatim API has usage limits (1 request/second)
3. **City Database**: Search limited to 50 results for performance
4. **Platform**: Windows-only due to WPF dependency
5. **IP Geolocation Accuracy**: IP-based location may be approximate (city-level accuracy)
6. **Circular Widget Language**: The circular progress widget does not update dynamically when language is changed (requires restart)

## File Copy Settings

```xml
<None Update="Icons\sun.png">
  <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</None>
<None Update="Icons\moon.png">
  <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</None>
<None Update="salaty.sqlite">
  <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</None>
```

## External APIs

### Nominatim (Geocoding)
- **URL**: `https://nominatim.openstreetmap.org/search`
- **Format**: JSON
- **Rate Limit**: 1 request/second
- **User-Agent**: Required header ("PrayerWidget/1.0")

### ipwhois.app (IP Geolocation)
- **URL**: `https://ipwhois.app/json/`
- **Format**: JSON
- **Rate Limit**: Unlimited (free tier)
- **No API key required**
- **Returns**: latitude, longitude, city, country, country_code
- **Protocol**: HTTPS (secure)

### PrayTime Algorithm
- Based on astronomical calculations
- Uses Julian date for sun position
- Haversine formula for distance calculations
- Supports Shafii and Hanafi Asr methods

## Version Information

Current version info from `version.json`:
- **Version**: 1.0.2
- **Version Code**: 3
- **Release Date**: 2026-03-06
- **Download URL**: https://github.com/eng-salem/Salaty1st/releases/latest
- **Facebook Page**: https://www.facebook.com/Salaty.1st

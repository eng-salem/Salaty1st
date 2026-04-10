# PrayerWidget Database Schema

**Database:** SQLite (`salaty.sqlite`)
**Location:** `%LOCALAPPDATA%\PrayerWidget\salaty.sqlite`
**Version:** 1.0.10+

---

## Table of Contents

1. [Settings Table](#settings-table)
2. [Cities Table](#cities-table)
3. [Countries Table](#countries-table)
4. [Calculation Methods](#calculation-methods)
5. [Complete SQL Schema](#complete-sql-schema)

---

## Settings Table

Stores user preferences and widget configuration.

### Schema

```sql
CREATE TABLE IF NOT EXISTS settings (
    Name TEXT PRIMARY KEY NOT NULL,
    Value TEXT
);
```

### Fields Stored

| Name | Type | Default | Description |
|------|------|---------|-------------|
| `City_ID` | INTEGER | 5 | City database ID |
| `Country_Code` | TEXT | 'EG' | Country code (ISO 3166) |
| `Country_Name` | TEXT | 'Egypt' | Country name |
| `Method` | INTEGER | 5 | Calculation method ID |
| `City_Name` | TEXT | 'Cairo' | City name |
| `La` | REAL | 30.06263 | Latitude |
| `Lo` | REAL | 31.24967 | Longitude |
| `AsrMethod` | INTEGER | 0 | Asr method (0=Shafii, 1=Hanafi) |
| `Summer` / `IsSummerTime` | INTEGER | 0 | Daylight saving (0/1) |
| `WidgetOpacity` | REAL | 70 | Widget opacity % (0-100) |
| `NotificationOpacity` | REAL | 95 | Toast opacity % (0-100) |
| `ShowProgressRing` | INTEGER | 1 | Show circular progress (0/1) |
| `HijriAdjustment` | INTEGER | 0 | Hijri date adjustment (-1,0,1) |
| `AthanSound` | TEXT | 'default' | Athan sound file name |
| `EnableDuaaAfterAthan` | INTEGER | 0 | Play duaa after athan (0/1) |
| `DuaaSound` | TEXT | 'sha3rawy_duaa' | Duaa sound file name |
| `Size` | TEXT | 'Medium' | Widget size preset |
| `CornerRadius` | REAL | 75 | Corner radius % |
| `GridMargin` | INTEGER | 8 | Grid margin pixels |
| `StrokeThickness` | REAL | 3 | Progress arc thickness |
| `WindowWidth` | INTEGER | 180 | Window width pixels |
| `WindowHeight` | INTEGER | 180 | Window height pixels |
| `HasCompletedSetup` | INTEGER | 0 | Setup completed flag (0/1) |
| `Language` | TEXT | 'en' | UI language code |
| `AthanVolume` | INTEGER | 100 | Athan volume % (0-100) |

### Prayer Reminder Settings

| Name Pattern | Example | Description |
|--------------|---------|-------------|
| `{Prayer}Reminder_EnabledBefore` | `FajrReminder_EnabledBefore` | Enable before reminder |
| `{Prayer}Reminder_MinutesBefore` | `FajrReminder_MinutesBefore` | Minutes before prayer |
| `{Prayer}Reminder_EnabledAfter` | `FajrReminder_EnabledAfter` | Enable after reminder |
| `{Prayer}Reminder_MinutesAfter` | `FajrReminder_MinutesAfter` | Minutes after prayer |

Prayer names: `Fajr`, `Dhuhr`, `Asr`, `Maghrib`, `Isha`

### Quote Settings

| Name | Type | Default | Description |
|------|------|---------|-------------|
| `EnableQuotes` | INTEGER | 1 | Enable Islamic quotes (0/1) |
| `QuoteInterval` | INTEGER | 30 | Minutes between quotes |
| `EnableQuoteAudio` | INTEGER | 1 | Play audio with quotes (0/1) |

### Other Settings

| Name | Type | Default | Description |
|------|------|---------|-------------|
| `ShowWidget` | INTEGER | 1 | Show/hide widget (0/1) |
| `EnableAnalytics` | INTEGER | 1 | Enable analytics (0/1) |

### Example Queries

```sql
-- Insert or update a setting
INSERT OR REPLACE INTO settings (Name, Value) VALUES ('WidgetOpacity', '70');

-- Get all settings
SELECT Name, Value FROM settings;

-- Get specific setting
SELECT Value FROM settings WHERE Name = 'AthanSound';

-- Delete setting
DELETE FROM settings WHERE Name = 'City_ID';

-- Count settings
SELECT COUNT(*) FROM settings;
```

---

## Cities Table

Contains 23,000+ cities worldwide with coordinates and timezones.

### Schema

```sql
CREATE TABLE IF NOT EXISTS Cities (
    City_ID INTEGER PRIMARY KEY AUTOINCREMENT,
    City_Name TEXT NOT NULL,
    La REAL NOT NULL,
    Lo REAL NOT NULL,
    Country_Code TEXT NOT NULL,
    Time_Zone TEXT,
    Tz INTEGER,
    FOREIGN KEY (Country_Code) REFERENCES Countries(Country_Code)
);
```

### Fields

| Column | Type | Description |
|--------|------|-------------|
| `City_ID` | INTEGER | Unique city identifier |
| `City_Name` | TEXT | City name (localized) |
| `La` | REAL | Latitude (decimal degrees) |
| `Lo` | REAL | Longitude (decimal degrees) |
| `Country_Code` | TEXT | ISO 3166-1 alpha-2 country code |
| `Time_Zone` | TEXT | IANA timezone name |
| `Tz` | INTEGER | UTC offset in hours |

### Indexes

```sql
CREATE INDEX IF NOT EXISTS idx_cities_country ON Cities(Country_Code);
CREATE INDEX IF NOT EXISTS idx_cities_name ON Cities(City_Name);
CREATE INDEX IF NOT EXISTS idx_cities_coords ON Cities(La, Lo);
```

### Example Queries

```sql
-- Find city by ID
SELECT * FROM Cities WHERE City_ID = 5;

-- Search cities by name
SELECT * FROM Cities WHERE City_Name LIKE '%Cairo%';

-- Get cities by country
SELECT * FROM Cities WHERE Country_Code = 'EG' ORDER BY City_Name;

-- Find nearest cities (simplified)
SELECT *, 
       (La - 30.0) * (La - 30.0) + (Lo - 31.0) * (Lo - 31.0) AS dist_sq
FROM Cities 
ORDER BY dist_sq 
LIMIT 10;
```

---

## Countries Table

Contains 205 countries with default calculation methods.

### Schema

```sql
CREATE TABLE IF NOT EXISTS Countries (
    Country_Code TEXT PRIMARY KEY NOT NULL,
    Country_Name TEXT NOT NULL,
    Method INTEGER DEFAULT 5,
    AdjustHighLats INTEGER DEFAULT 0,
    Elev INTEGER DEFAULT 0,
    FOREIGN KEY (Method) REFERENCES CalculationMethods(Method_ID)
);
```

### Fields

| Column | Type | Description |
|--------|------|-------------|
| `Country_Code` | TEXT | ISO 3166-1 alpha-2 code |
| `Country_Name` | TEXT | Country name (localized) |
| `Method` | INTEGER | Default calculation method ID |
| `AdjustHighLats` | INTEGER | High latitude adjustment method |
| `Elev` | INTEGER | Elevation in meters |

### Example Queries

```sql
-- Get country by code
SELECT * FROM Countries WHERE Country_Code = 'EG';

-- Get all countries
SELECT * FROM Countries ORDER BY Country_Name;

-- Get country with default method
SELECT c.*, m.Method_Name 
FROM Countries c
LEFT JOIN CalculationMethods m ON c.Method = m.Method_ID;
```

---

## Calculation Methods

### Schema

```sql
CREATE TABLE IF NOT EXISTS CalculationMethods (
    Method_ID INTEGER PRIMARY KEY,
    Method_Name TEXT NOT NULL,
    FajrAngle REAL,
    IshaAngle REAL,
    IshaDelay INTEGER DEFAULT 0
);
```

### Default Data

```sql
INSERT INTO CalculationMethods (Method_ID, Method_Name, FajrAngle, IshaAngle, IshaDelay) VALUES
(0, 'الجعفري (Jafari)', 16.0, 14.0, 0),
(1, 'رابطة العالم الإسلامي (MWL)', 18.0, 17.0, 0),
(2, 'الهيئة المصرية العامة للمساحة (Egyptian)', 19.5, 17.5, 0),
(3, 'كراتي (Karachi)', 18.0, 18.0, 0),
(4, 'أم القرى (Umm Al-Qura)', 18.5, 0.0, 90),
(5, 'ISNA (North America)', 15.0, 15.0, 0);
```

### Fields

| Column | Type | Description |
|--------|------|-------------|
| `Method_ID` | INTEGER | Unique method identifier |
| `Method_Name` | TEXT | Display name (with Arabic) |
| `FajrAngle` | REAL | Fajr angle in degrees |
| `IshaAngle` | REAL | Isha angle in degrees |
| `IshaDelay` | INTEGER | Minutes after Maghrib for Isha |

---

## Complete SQL Schema

```sql
-- ============================================
-- PrayerWidget Database Schema
-- Database: salaty.sqlite
-- Version: 1.0.10
-- ============================================

-- Settings Table
CREATE TABLE IF NOT EXISTS settings (
    Name TEXT PRIMARY KEY NOT NULL,
    Value TEXT
);

-- Cities Table
CREATE TABLE IF NOT EXISTS Cities (
    City_ID INTEGER PRIMARY KEY AUTOINCREMENT,
    City_Name TEXT NOT NULL,
    La REAL NOT NULL,
    Lo REAL NOT NULL,
    Country_Code TEXT NOT NULL,
    Time_Zone TEXT,
    Tz INTEGER
);

-- Countries Table
CREATE TABLE IF NOT EXISTS Countries (
    Country_Code TEXT PRIMARY KEY NOT NULL,
    Country_Name TEXT NOT NULL,
    Method INTEGER DEFAULT 5,
    AdjustHighLats INTEGER DEFAULT 0,
    Elev INTEGER DEFAULT 0
);

-- Calculation Methods Table
CREATE TABLE IF NOT EXISTS CalculationMethods (
    Method_ID INTEGER PRIMARY KEY,
    Method_Name TEXT NOT NULL,
    FajrAngle REAL,
    IshaAngle REAL,
    IshaDelay INTEGER DEFAULT 0
);

-- Indexes for performance
CREATE INDEX IF NOT EXISTS idx_cities_country ON Cities(Country_Code);
CREATE INDEX IF NOT EXISTS idx_cities_name ON Cities(City_Name);
CREATE INDEX IF NOT EXISTS idx_cities_coords ON Cities(La, Lo);
CREATE INDEX IF NOT EXISTS idx_settings_name ON settings(Name);

-- Default calculation methods
INSERT OR REPLACE INTO CalculationMethods (Method_ID, Method_Name, FajrAngle, IshaAngle, IshaDelay) VALUES
(0, 'الجعفري (Jafari)', 16.0, 14.0, 0),
(1, 'رابطة العالم الإسلامي (MWL)', 18.0, 17.0, 0),
(2, 'الهيئة المصرية العامة للمساحة (Egyptian)', 19.5, 17.5, 0),
(3, 'كراتي (Karachi)', 18.0, 18.0, 0),
(4, 'أم القرى (Umm Al-Qura)', 18.5, 0.0, 90),
(5, 'ISNA (North America)', 15.0, 15.0, 0);

-- Default settings (optional - app creates these on first run)
INSERT OR IGNORE INTO settings (Name, Value) VALUES ('WidgetOpacity', '70');
INSERT OR IGNORE INTO settings (Name, Value) VALUES ('NotificationOpacity', '95');
INSERT OR IGNORE INTO settings (Name, Value) VALUES ('ShowProgressRing', '1');
INSERT OR IGNORE INTO settings (Name, Value) VALUES ('HijriAdjustment', '0');
INSERT OR IGNORE INTO settings (Name, Value) VALUES ('AthanSound', 'default');
INSERT OR IGNORE INTO settings (Name, Value) VALUES ('EnableDuaaAfterAthan', '0');
INSERT OR IGNORE INTO settings (Name, Value) VALUES ('DuaaSound', 'sha3rawy_duaa');
INSERT OR IGNORE INTO settings (Name, Value) VALUES ('Size', 'Medium');
INSERT OR IGNORE INTO settings (Name, Value) VALUES ('CornerRadius', '75');
INSERT OR IGNORE INTO settings (Name, Value) VALUES ('HasCompletedSetup', '0');
INSERT OR IGNORE INTO settings (Name, Value) VALUES ('Language', 'en');
INSERT OR IGNORE INTO settings (Name, Value) VALUES ('AthanVolume', '100');
INSERT OR IGNORE INTO settings (Name, Value) VALUES ('EnableQuotes', '1');
INSERT OR IGNORE INTO settings (Name, Value) VALUES ('QuoteInterval', '30');
INSERT OR IGNORE INTO settings (Name, Value) VALUES ('EnableQuoteAudio', '1');
```

---

## Database Maintenance

### Vacuum Database

```sql
VACUUM;
```

### Check Integrity

```sql
PRAGMA integrity_check;
```

### Get Table Info

```sql
PRAGMA table_info(settings);
PRAGMA table_info(Cities);
PRAGMA table_info(Countries);
```

### Get All Settings

```sql
SELECT Name, Value FROM settings ORDER BY Name;
```

---

## Usage Examples

### C# / VB.NET

```vbnet
Using connection As New SqliteConnection("Data Source=" & dbPath)
    connection.Open()
    
    ' Insert/Update setting
    Dim cmd As New SqliteCommand(
        "INSERT OR REPLACE INTO settings (Name, Value) VALUES (@name, @value);", 
        connection)
    cmd.Parameters.AddWithValue("@name", "WidgetOpacity")
    cmd.Parameters.AddWithValue("@value", "70")
    cmd.ExecuteNonQuery()
    
    ' Get setting
    Dim getCmd As New SqliteCommand(
        "SELECT Value FROM settings WHERE Name = @name;", 
        connection)
    getCmd.Parameters.AddWithValue("@name", "WidgetOpacity")
    Dim result = getCmd.ExecuteScalar()
End Using
```

### Python

```python
import sqlite3

conn = sqlite3.connect('salaty.sqlite')
cursor = conn.cursor()

# Insert/Update setting
cursor.execute(
    "INSERT OR REPLACE INTO settings (Name, Value) VALUES (?, ?)",
    ("WidgetOpacity", "70")
)
conn.commit()

# Get setting
cursor.execute(
    "SELECT Value FROM settings WHERE Name = ?",
    ("WidgetOpacity",)
)
result = cursor.fetchone()

conn.close()
```

---

**Last Updated:** March 21, 2026
**Database Version:** 1.0.10

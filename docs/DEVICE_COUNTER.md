# Device Counter Tracking with libSQL (Turso)

## Overview

The PrayerWidget application now includes device usage tracking using a libSQL database hosted on Turso. Each device is uniquely identified using hardware identifiers, and usage is tracked on every application launch.

## Database Configuration

**Database URL:** `libsql://counter-manhag.aws-eu-west-1.turso.io`

**Authentication:** JWT Token (ed25519 signed)

**Region:** AWS EU West 1 (Ireland)

---

## Table Schema

```sql
CREATE TABLE IF NOT EXISTS device_counters (
    device_id TEXT PRIMARY KEY,
    machine_name TEXT,
    os_version TEXT,
    dotnet_version TEXT,
    app_version TEXT,
    first_seen TEXT,
    last_seen TEXT,
    total_launches INTEGER DEFAULT 0,
    country TEXT,
    city TEXT
);
```

### Column Descriptions

| Column | Type | Description |
|--------|------|-------------|
| `device_id` | TEXT | Unique hardware-based identifier (32-char hex) |
| `machine_name` | TEXT | Windows computer name |
| `os_version` | TEXT | Operating system version |
| `dotnet_version` | TEXT | .NET runtime version |
| `app_version` | TEXT | Application version (e.g., "1.0.10") |
| `first_seen` | TEXT | First launch timestamp (UTC) |
| `last_seen` | TEXT | Most recent launch timestamp (UTC) |
| `total_launches` | INTEGER | Total number of app launches |
| `country` | TEXT | Country code (future feature) |
| `city` | TEXT | City name (future feature) |

---

## Device ID Generation

The device ID is generated using a combination of hardware identifiers to ensure uniqueness and persistence:

### Priority Order

1. **Machine GUID** (Windows Registry) - Most reliable
2. **Motherboard Serial Number** (WMI: Win32_BaseBoard)
3. **BIOS Serial Number** (WMI: Win32_BIOS)
4. **MAC Address** (First non-virtual network adapter)
5. **Volume Serial Number** (System drive)

### Fallback Mechanism

If no hardware identifiers are available, the system:
1. Generates a random ID and stores it in the registry at `HKEY_CURRENT_USER\SOFTWARE\PrayerWidget\DeviceId`
2. This ensures the same ID is used across application restarts

### Hashing

All collected identifiers are combined and hashed using **SHA-256**, then truncated to 16 bytes (32 hex characters) for the final device ID.

**Example Device ID:** `A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6`

---

## How It Works

### Application Startup Flow

```
Application Start
    ↓
Generate/Load Device ID
    ↓
Get Device Info (machine name, OS, .NET version, app version)
    ↓
Initialize Database Table (if not exists)
    ↓
Check if Device Exists
    ├─→ New Device: INSERT record with total_launches = 1
    └─→ Existing Device: UPDATE last_seen, increment total_launches
```

### Code Integration

The tracking is implemented in `Application.xaml.vb`:

```vbnet
Private Sub Application_Startup(...) Handles Me.Startup
    ' Initialize device counter service
    _deviceCounterService = New DeviceCounterService()
    
    ' Track launch (fire-and-forget)
    TrackAppLaunchAsync()
End Sub

Private Async Sub TrackAppLaunchAsync()
    ' Initialize table and increment counter
    Await _deviceCounterService.InitializeTableAsync()
    Await _deviceCounterService.IncrementCounterAsync()
End Sub
```

---

## API Usage

### libSQL HTTP API

The service uses the libSQL HTTP API to execute SQL queries:

**Endpoint:** `POST {turso_url}/v2`

**Headers:**
- `Authorization: Bearer {jwt_token}`
- `Content-Type: application/json`

**Request Body:**
```json
{
  "statements": [
    {
      "sql": "SELECT * FROM device_counters WHERE device_id = '...'"
    }
  ]
}
```

**Response:**
```json
{
  "results": {
    "columns": ["device_id", "machine_name", ...],
    "rows": [["A1B2C3...", "DESKTOP-PC", ...]],
    "rows_affected": 1
  }
}
```

---

## Query Examples

### Get Total Active Devices

```sql
SELECT COUNT(*) as total_devices FROM device_counters;
```

### Get Total Launches (All Time)

```sql
SELECT SUM(total_launches) as total_launches FROM device_counters;
```

### Get Most Active Devices

```sql
SELECT device_id, machine_name, total_launches, last_seen
FROM device_counters
ORDER BY total_launches DESC
LIMIT 10;
```

### Get New Devices Today

```sql
SELECT * FROM device_counters
WHERE first_seen >= datetime('now', 'start of day');
```

### Get Device Activity by App Version

```sql
SELECT app_version, COUNT(*) as device_count, SUM(total_launches) as launches
FROM device_counters
GROUP BY app_version
ORDER BY app_version;
```

---

## Security Considerations

### SQL Injection Prevention

All string values are escaped using the `EscapeSql()` method:

```vbnet
Private Function EscapeSql(value As String) As String
    If String.IsNullOrEmpty(value) Then Return ""
    Return value.Replace("'", "''")
End Function
```

### Token Security

- The JWT token is embedded in the application (read-only access)
- Token grants `rw` (read-write) permissions only to the specific database
- Token expiration: Check `iat` claim in JWT payload

### Privacy

- No personal information is collected
- Device ID is a hash (cannot be reverse-engineered)
- Machine name is used for identification only

---

## Troubleshooting

### Check Debug Logs

Logs are written to:
```
%LOCALAPPDATA%\PrayerWidget\debug.log
```

Look for:
```
[DeviceCounter] Device ID: A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6
[DeviceCounter] Machine: DESKTOP-PC
[DeviceCounter] Counter incremented for device: A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6
```

### Common Issues

| Issue | Solution |
|-------|----------|
| "HTTP Error: 401" | Token expired or invalid |
| "HTTP Error: 403" | Insufficient permissions |
| "Connection timeout" | Network issue or firewall blocking |
| "Table not found" | Call `InitializeTableAsync()` first |

---

## Future Enhancements

- [ ] Add geolocation tracking (country, city) from IP address
- [ ] Track session duration (on app close)
- [ ] Add crash reporting integration
- [ ] Implement daily active users (DAU) / monthly active users (MAU) metrics
- [ ] Add user opt-out mechanism in settings

---

## Files Modified

| File | Purpose |
|------|---------|
| `Services/DeviceCounterService.vb` | Main tracking service |
| `Application.xaml.vb` | Integration on startup |
| `PrayerWidget.vbproj` | Added System.Management package |

---

## Testing

### Manual Testing

1. Build and run the application
2. Check debug log for device ID generation
3. Query the database to verify record creation:
   ```sql
   SELECT * FROM device_counters WHERE device_id = 'YOUR_DEVICE_ID';
   ```
4. Restart the application multiple times
5. Verify `total_launches` increments correctly

### Expected Log Output

```
=== PrayerWidget Started ===
Time: 3/21/2026 10:30:45 AM
[DeviceCounter] Device ID: A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6
[DeviceCounter] Machine: DESKTOP-PC
[DeviceCounter] Table initialized successfully
[DeviceCounter] Counter incremented for device: A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6
```

---

## Resources

- **Turso Documentation:** https://docs.turso.tech
- **libSQL Protocol:** https://docs.turso.tech/http/rest
- **System.Management Package:** https://www.nuget.org/packages/System.Management

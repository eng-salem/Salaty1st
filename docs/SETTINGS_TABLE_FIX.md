# Settings Table Loading Fix

## Problem

The application stopped loading settings from the database. Widget settings like opacity, Hijri adjustment, athan sound, etc. were not being retrieved.

## Root Cause

The `SqlitePrayerService.GetSettings()` method was only reading a limited set of fields from the settings table:
- City_ID
- Country_Code
- Method
- City_Name
- La (Latitude)
- Lo (Longitude)
- AsrMethod
- Summer

But `SettingsManager.LoadSettings()` expected many more fields:
- WidgetOpacity
- NotificationOpacity
- ShowProgressRing
- HijriAdjustment
- AthanSound
- EnableDuaaAfterAthan
- DuaaSound
- Size
- CornerRadius
- GridMargin
- StrokeThickness
- WindowWidth
- WindowHeight
- HasCompletedSetup

## Solution

### 1. Updated UserSettings Class

Added all widget settings properties to `Services/SqlitePrayerService.vb`:

```vbnet
Public Class UserSettings
    ' Existing prayer settings
    Public Property CityId As Integer = 5
    Public Property CountryCode As String = "Egypt"
    Public Property Method As Integer = 5
    Public Property CityName As String = "Cairo"
    Public Property Latitude As Double = 30.06263
    Public Property Longitude As Double = 31.24967
    Public Property AsrMethod As Integer = 0
    Public Property IsSummer As Boolean = False
    
    ' NEW: Widget settings
    Public Property WidgetOpacity As Double = 70
    Public Property NotificationOpacity As Double = 95
    Public Property ShowProgressRing As Boolean = True
    Public Property HijriAdjustment As Integer = 0
    Public Property AthanSound As String = "default"
    Public Property EnableDuaaAfterAthan As Boolean = False
    Public Property DuaaSound As String = "sha3rawy_duaa"
    Public Property Size As String = "Medium"
    Public Property CornerRadius As Double = 75
    Public Property GridMargin As Integer = 8
    Public Property StrokeThickness As Double = 3
    Public Property WindowWidth As Integer = 180
    Public Property WindowHeight As Integer = 180
    Public Property HasCompletedSetup As Boolean = False
End Class
```

### 2. Updated GetSettings() Method

Added all missing field mappings in `Services/SqlitePrayerService.vb`:

```vbnet
Select Case name
    ' ... existing cases ...
    
    ' NEW: Widget settings cases
    Case "WidgetOpacity"
        settings.WidgetOpacity = CDbl(value)
    Case "NotificationOpacity"
        settings.NotificationOpacity = CDbl(value)
    Case "ShowProgressRing"
        settings.ShowProgressRing = Convert.ToInt32(value) = 1
    Case "HijriAdjustment"
        settings.HijriAdjustment = Convert.ToInt32(value)
    Case "AthanSound"
        settings.AthanSound = value.ToString()
    Case "EnableDuaaAfterAthan"
        settings.EnableDuaaAfterAthan = Convert.ToInt32(value) = 1
    Case "DuaaSound"
        settings.DuaaSound = value.ToString()
    Case "Size"
        settings.Size = value.ToString()
    Case "CornerRadius"
        settings.CornerRadius = CDbl(value)
    Case "GridMargin"
        settings.GridMargin = Convert.ToInt32(value)
    Case "StrokeThickness"
        settings.StrokeThickness = CDbl(value)
    Case "WindowWidth"
        settings.WindowWidth = Convert.ToInt32(value)
    Case "WindowHeight"
        settings.WindowHeight = Convert.ToInt32(value)
    Case "HasCompletedSetup"
        settings.HasCompletedSetup = Convert.ToInt32(value) = 1
End Select
```

### 3. Added Summer/IsSummerTime Alias

Also fixed a bug where the field name was inconsistent:

```vbnet
Case "Summer", "IsSummerTime"
    settings.IsSummer = Convert.ToInt32(value) = 1
```

## Files Modified

| File | Changes |
|------|---------|
| `Services/SqlitePrayerService.vb` | - Added widget settings to UserSettings class<br>- Added field mappings in GetSettings() method<br>- Added Summer/IsSummerTime alias |

## Testing

After this fix, the application should:

1. **Load all settings correctly:**
   - Widget opacity
   - Notification opacity
   - Show progress ring
   - Hijri adjustment
   - Athan sound
   - Duaa after athan
   - Duaa sound
   - Widget size
   - Corner radius
   - Grid margin
   - Stroke thickness
   - Window dimensions
   - Has completed setup flag

2. **Debug log should show:**
   ```
   === DATABASE SETTINGS ===
     WidgetOpacity = 70
     NotificationOpacity = 95
     ShowProgressRing = 1
     HijriAdjustment = 0
     AthanSound = default
     EnableDuaaAfterAthan = 1
     DuaaSound = sha3rawy_duaa
     Size = Medium
     ...
   ```

3. **Settings window should display saved values:**
   - Opacity sliders at saved positions
   - Athan sound combo box shows selected sound
   - Hijri adjustment shows saved value
   - All checkboxes reflect saved state

## Build Status

```
Build succeeded with 5 warning(s)
Output: bin\Release\net48\Salaty.First.exe
```

## Related Issues

This fix resolves the issue where settings were not being loaded from the database, causing:
- Widget to reset to default opacity
- Athan sound to reset to default
- Hijri adjustment to be lost
- All custom settings to be ignored

---

**Date Fixed:** March 21, 2026
**Version:** 1.0.10+
**Status:** RESOLVED

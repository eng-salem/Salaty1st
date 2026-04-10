# Athan Toast Notification Fix

## Problem
The athan toast notification was not showing when prayer time was reached.

## Root Cause Analysis

1. **MediaPlayer Conflict**: Two MediaPlayer instances were trying to open the same MP3 file simultaneously:
   - MainWindow's MediaPlayer opened the file first
   - ToastNotification's MediaPlayer tried to open it immediately after
   - This could cause the toast's MediaPlayer to fail silently

2. **Missing Error Handling**: ToastNotification had no `MediaFailed` event handler, so if the media failed to open, it failed silently with no diagnostic information

3. **No Diagnostic Logging**: Insufficient logging made it difficult to determine if the toast was being created and shown

## Solution Implemented

### 1. Added MediaFailed Handler to ToastNotification

**File:** `Forms/ToastNotification.xaml.vb`

Added comprehensive error handling:
```vb
AddHandler _mediaPlayer.MediaFailed, Sub(s, e)
    Console.WriteLine($"[ToastNotification] MediaFailed: {e.ErrorException?.Message}")
    NowPlayingText.Text = "Audio unavailable (file may be in use)"
    ' Hide media controls if sound fails
    If ProgressSlider IsNot Nothing Then ProgressSlider.Visibility = Visibility.Collapsed
    If BtnPlayPause IsNot Nothing Then BtnPlayPause.Visibility = Visibility.Collapsed
    If BtnStop IsNot Nothing Then BtnStop.Visibility = Visibility.Collapsed
    If VolumeSlider IsNot Nothing Then VolumeSlider.Visibility = Visibility.Collapsed
End Sub
```

**Benefits:**
- Toast still shows even if sound fails
- Clear error messages in debug log
- UI gracefully degrades when audio unavailable

### 2. Enhanced Logging in ToastNotification

Added logging throughout the media initialization:
```vb
Console.WriteLine($"[ToastNotification] MediaOpened: {soundPath}")
Console.WriteLine($"[ToastNotification] Opening media file: {soundPath}")
Console.WriteLine("[ToastNotification] MediaEnded - playback completed")
```

### 3. Enhanced Logging in ShowToastNotification

**File:** `MainWindow.xaml.vb`

Added diagnostic logging:
```vb
Console.WriteLine($"[ShowToastNotification] Position: X={toast.Left}, Y={toast.Top}")
Console.WriteLine($"[ShowToastNotification] Title='{title}', Message='{message}'")
Console.WriteLine("[ShowToastNotification] Toast shown successfully")
```

### 4. Reordered PlayAthan Method

Changed the order to show toast **before** playing sound from MainWindow:
```vb
' Show toast notification FIRST with athan sound
' The toast will handle its own sound playback
ShowToastNotification($"🕌 {prayerName} Time",
                     $"The time for {prayerName} prayer has started.",
                     soundPath)

' Also play sound from main window for reliability
Me.Dispatcher.Invoke(Sub()
    _mediaPlayer.Open(New Uri(soundPath))
    _mediaPlayer.Play()
End Sub)
```

### 5. Enhanced Timer_Tick Logging

Added detailed logging to track prayer time detection:
```vb
' Debug logging every 10 seconds or when near prayer time
Static lastLogTime As DateTime = DateTime.MinValue
If (DateTime.Now - lastLogTime).TotalSeconds >= 10 OrElse (timeDiff >= -10 AndAlso timeDiff <= 10) Then
    Console.WriteLine($"[Timer_Tick] Debug: nextPrayer={_nextPrayerTime:HH:mm:ss}, now={now:HH:mm:ss}, timeDiff={timeDiff:F2}s, flag={_hasPlayedAthanForCurrentPrayer}")
    lastLogTime = DateTime.Now
End If
```

### 6. Enhanced LoadPrayerTimesAsync Logging

Logs all loaded prayer times for debugging:
```vb
Console.WriteLine($"[LoadPrayerTimesAsync] Loaded prayer times:")
Console.WriteLine($"  Fajr: {prayerTimes.Fajr}")
Console.WriteLine($"  Dhuhr: {prayerTimes.Dhuhr}")
Console.WriteLine($"  Asr: {prayerTimes.Asr}")
Console.WriteLine($"  Maghrib: {prayerTimes.Maghrib}")
Console.WriteLine($"  Isha: {prayerTimes.Isha}")
Console.WriteLine($"  Next: {prayerTimes.NextPrayer} at {prayerTimes.NextPrayerTime:HH:mm:ss}")
```

## Testing

### Method 1: Test with Ctrl+T Shortcut
1. Run the application
2. Press **Ctrl+T** on your keyboard
3. Verify toast notification appears in bottom-right corner
4. Check if athan sound plays

### Method 2: Time Simulation Script
Use the PowerShell script to set system time close to prayer time:
```powershell
# Set time to 19:25:30 (close to Isha)
$newTime = Get-Date -Hour 19 -Minute 25 -Second 30
Set-Date -Date $newTime

# Launch application
$appPath = "C:\Users\Administrator\Desktop\PrayerWidget\PrayerWidget\bin\Release\net48\Salaty.First.exe"
Start-Process $appPath
```

### Method 3: Check Debug Log

The debug log is located at:
```
%LOCALAPPDATA%\PrayerWidget\debug.log
```

View with:
```powershell
Get-Content "$env:LOCALAPPDATA\PrayerWidget\debug.log" -Tail 100 -Wait
```

Look for these entries:
```
[LoadPrayerTimesAsync] Loaded prayer times:
  Fajr: 05:30
  Dhuhr: 12:30
  Asr: 15:45
  Maghrib: 18:15
  Isha: 19:30
  Next: ISHA at 19:30:00
  Current time: 19:25:30
  Time to next prayer: 4.5 minutes

[Timer_Tick] Debug: nextPrayer=19:30:00, now=19:29:55, timeDiff=-5.00s, flag=False
[Timer_Tick] Debug: nextPrayer=19:30:00, now=19:30:00, timeDiff=0.00s, flag=False
[Timer_Tick] *** PRAYER TIME REACHED! *** 19:30:00, now=19:30:00, diff=0.0s
[Timer_Tick] Calling PlayAthan()...
[PlayAthan] Prayer: ISHA, Sound: al-afasy_athan, Path: C:\...\Resources\MP3\al-afasy_athan.mp3
[PlayAthan] Opening media file: C:\...\Resources\MP3\al-afasy_athan.mp3, Volume: 100%
[ShowToastNotification] Position: X=1910, Y=1030, ScreenSize=1920x1080
[ShowToastNotification] Title='🕌 ISHA Time', Message='The time for ISHA prayer has started.'
[ToastNotification] Opening media file: C:\...\Resources\MP3\al-afasy_athan.mp3
[ToastNotification] MediaOpened: C:\...\Resources\MP3\al-afasy_athan.mp3
[ShowToastNotification] Toast shown successfully
[Timer_Tick] PlayAthan() returned
```

## Files Modified

| File | Changes |
|------|---------|
| `Forms/ToastNotification.xaml.vb` | - Added MediaFailed event handler<br>- Enhanced logging<br>- Graceful UI degradation on audio failure |
| `MainWindow.xaml.vb` | - Reordered PlayAthan method<br>- Added ShowToastNotification logging<br>- Improved error handling |

## Expected Behavior

### Normal Operation
1. Prayer time is reached
2. Toast notification appears in bottom-right corner
3. Athan sound plays (from both MainWindow and Toast for reliability)
4. User can:
   - Play/pause sound
   - Adjust volume
   - Stop playback
   - Close toast
   - Open settings from toast

### If Audio Fails
1. Toast notification still appears
2. Message shows "Audio unavailable (file may be in use)"
3. Media controls are hidden
4. Toast can still be closed or settings opened

## Troubleshooting

### Toast Not Showing
Check debug log for:
```
[ShowToastNotification] ERROR: <error message>
```

### Sound Not Playing
Check debug log for:
```
[ToastNotification] MediaFailed: <error message>
[PlayAthan] MediaFailed: <error message>
```

Common issues:
- MP3 file not found in `Resources/MP3` folder
- File corrupted or invalid format
- File locked by another process

### Toast Position Wrong
Check debug log for position values:
```
[ShowToastNotification] Position: X=1910, Y=1030, ScreenSize=1920x1080
```

If position is off-screen, check:
- Multiple monitor setup
- Taskbar position
- Screen resolution changes

## Version Information

- **Fix Applied:** March 22, 2026
- **Version:** 1.0.10+
- **Build:** Release

---

**For support:** Check debug log first, then open a GitHub issue or message on Facebook page

# Athan Sound Playback Fix

## Problem Description

The athan sound was not playing reliably at prayer times. Sometimes it worked, other times it didn't.

### Root Cause

The original timer check in `Timer_Tick` had a **very narrow trigger window**:

```vb
' OLD CODE (UNRELIABLE)
If (_nextPrayerTime - DateTime.Now).TotalSeconds <= 1 AndAlso 
   (_nextPrayerTime - DateTime.Now).TotalSeconds > 0 Then
    PlayAthan()
End If
```

**Issues:**
1. The timer runs every 1 second, but if the timer tick is slightly delayed (even by milliseconds), it can miss the exact 1-second window
2. The condition only checked for time **before** the prayer time (0 to 1 second before)
3. No flag to prevent duplicate plays, causing potential race conditions
4. Insufficient logging to diagnose failures

---

## Solution Implemented

### 1. Wider Trigger Window

Changed to check for time **after** the prayer time with a 5-second window:

```vb
' NEW CODE (RELIABLE)
Dim timeDiff As Double = (now - _nextPrayerTime).TotalSeconds

If timeDiff >= 0 AndAlso timeDiff <= 5 AndAlso Not _hasPlayedAthanForCurrentPrayer Then
    _hasPlayedAthanForCurrentPrayer = True
    PlayAthan()
End If
```

**Benefits:**
- Checks for time **after** prayer time (more reliable)
- 5-second window catches delayed timer ticks
- Uses flag to prevent duplicate plays

### 2. Play Flag

Added `_hasPlayedAthanForCurrentPrayer` flag:
- Prevents multiple triggers for the same prayer time
- Automatically resets when far from prayer time (>60 seconds)
- Ensures one-time playback per prayer

### 3. Enhanced Logging

Added comprehensive logging throughout `PlayAthan()`:

```vb
Console.WriteLine($"[PlayAthan] Prayer: {prayerName}, Sound: {athanSound}, Path: {soundPath}")
Console.WriteLine($"[PlayAthan] Opening media file: {soundPath}, Volume: {volume}%")
Console.WriteLine($"[PlayAthan] Play() called, MediaPlayer state: {_mediaPlayer.Source?.AbsoluteUri}")
Console.WriteLine($"[PlayAthan] MediaEnded - playback completed")
```

**Logs include:**
- Prayer name and selected athan sound
- Full file path verification
- Volume setting
- MediaPlayer state
- Error details with stack traces
- List of available MP3 files if file not found

---

## How to Debug Athan Playback Issues

### Step 1: Check the Debug Log

The log file is located at:
```
%LOCALAPPDATA%\PrayerWidget\debug.log
```

Open it with:
```powershell
Get-Content "$env:LOCALAPPDATA\PrayerWidget\debug.log" -Tail 100
```

### Step 2: Look for These Log Entries

**Normal Operation:**
```
[Timer_Tick] Prayer time reached! 05:30:00, now=05:30:01, diff=1.0s
[PlayAthan] Prayer: FAJR, Sound: al-afasy_athan, Path: C:\...\Resources\MP3\al-afasy_athan.mp3
[PlayAthan] Opening media file: C:\...\Resources\MP3\al-afasy_athan.mp3, Volume: 100%
[PlayAthan] Play() called, MediaPlayer state: file:///C:/.../al-afasy_athan.mp3
[PlayAthan] MediaEnded - playback completed
```

**Common Errors:**

1. **Sound file not found:**
   ```
   [PlayAthan] ERROR: Sound file not found: C:\...\Resources\MP3\default_athan.mp3
   [PlayAthan] Available MP3 files in C:\...\Resources\MP3:
     - al-afasy_athan.mp3
     - minshawi_athan.mp3
   ```
   **Fix:** Check if the MP3 file exists in the Resources/MP3 folder

2. **Athan sound set to none:**
   ```
   [PlayAthan] Athan sound is set to 'none', skipping
   ```
   **Fix:** Go to Settings → Athan Sound and select a sound (not "None")

3. **Media playback failed:**
   ```
   [PlayAthan] MediaFailed: The codec is not supported
   ```
   **Fix:** Ensure MP3 files are valid and not corrupted

4. **Volume is zero:**
   ```
   [PlayAthan] Opening media file: ..., Volume: 0%
   ```
   **Fix:** Increase volume in Settings → Athan Volume

---

## Testing the Fix

### Method 1: Wait for Next Prayer Time

1. Leave the application running
2. Check the debug log around prayer time
3. Verify the log entries show successful playback

### Method 2: Use Test Mode (Ctrl+T)

1. Open the widget
2. Press **Ctrl+T** on your keyboard
3. This triggers `PlayAthan()` immediately
4. Check if sound plays and verify log output

### Method 3: Manual Prayer Time Test

1. Open Settings
2. Temporarily change city to one where prayer time is 1-2 minutes away
3. Wait for the prayer time to trigger
4. Verify athan plays correctly

---

## Files Modified

| File | Changes |
|------|---------|
| `MainWindow.xaml.vb` | - Added `_hasPlayedAthanForCurrentPrayer` flag<br>- Fixed `Timer_Tick` prayer time check<br>- Enhanced `PlayAthan()` with detailed logging<br>- Added MediaFailed error handler |

---

## Additional Improvements

### MediaPlayer Error Handling

Added `MediaFailed` event handler:
```vb
AddHandler _mediaPlayer.MediaFailed, Sub(s, e)
    Console.WriteLine($"[PlayAthan] MediaFailed: {e.ErrorException?.Message}")
    _isAthanPlaying = False
End Sub
```

### File Existence Check

Verifies MP3 file exists before attempting playback:
```vb
If Not System.IO.File.Exists(soundPath) Then
    Console.WriteLine($"[PlayAthan] ERROR: Sound file not found: {soundPath}")
    ' Lists available MP3 files for debugging
    Return
End If
```

### Per-Prayer Sound Selection

Supports different athan sounds for each prayer:
- Fajr → `FajrAthanSound`
- Dhuhr → `DhuhrAthanSound`
- Asr → `AsrAthanSound`
- Maghrib → `MaghribAthanSound`
- Isha → `IshaAthanSound`

---

## Troubleshooting Checklist

If athan still doesn't play:

- [ ] **Check Settings → Athan Sound**: Make sure it's not set to "None"
- [ ] **Check Settings → Athan Volume**: Should be > 0%
- [ ] **Check MP3 files exist**: Verify in `C:\Program Files\Salaty.First\Resources\MP3\`
- [ ] **Check Windows volume mixer**: Ensure app is not muted
- [ ] **Check debug log**: Look for `[PlayAthan]` entries
- [ ] **Test with Ctrl+T**: Verify manual trigger works
- [ ] **Restart application**: Clear any stuck MediaPlayer state

---

## Technical Details

### Timer Configuration

```vb
_timer = New DispatcherTimer() With {.Interval = TimeSpan.FromSeconds(1)}
```

- Runs every 1 second
- WPF DispatcherTimer (runs on UI thread)
- Accuracy: ~10-50ms typically

### MediaPlayer Configuration

```vb
_mediaPlayer = New MediaPlayer()
_mediaPlayer.Volume = volume ' 0-100 scale
_mediaPlayer.Open(New Uri(soundPath))
_mediaPlayer.Play()
```

- Uses WPF MediaPlayer (async playback)
- Volume scale: 0-100 (not 0-1 like System.Media)
- Supports MP3, WMA formats
- Non-blocking (doesn't freeze UI)

### Prayer Time Detection Window

```
-60s  0s   +1s   +5s  +60s
  |   |    |    |    |
  |   [----PRAYER TIME----]
  |        ^        ^
  |        |        |
  |     Trigger   Reset flag
  |
Reset flag
```

- **Before -60s**: Flag reset (safety)
- **0 to +5s**: Trigger window (plays athan)
- **After +60s**: Flag reset (ready for next prayer)

---

## Version Information

- **Fix Applied:** March 21, 2026
- **Version:** 1.0.10+
- **Build:** Release

---

## Related Issues

- [GitHub Issue #XX](https://github.com/eng-salem/Salaty1st/issues) - Athan not playing consistently
- [Facebook Discussion](https://www.facebook.com/Salaty.1st) - User reports

---

**For support:** Open a GitHub issue or message on Facebook page

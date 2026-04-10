# Current Issues - Prayer Widget Test Mode
## Date: March 23, 2026

### Issue 1: Test Mode Doesn't Stop After 5 Prayers ⚠️ CRITICAL

**Expected Behavior:**
- Test mode should run through 5 prayers (Fajr, Dhuhr, Asr, Maghrib, Isha)
- After the 5th prayer (Isha), test mode should STOP

**Current Behavior:**
- After completing 5 prayers, the system continues to Fajr again (6th prayer)
- Test mode never stops automatically

**Log Evidence:**
```
04:29:00 - PRAYER TIME REACHED (Fajr #1) ✓
12:03:00 - PRAYER TIME REACHED (Dhuhr #2) ✓
15:31:00 - PRAYER TIME REACHED (Asr #3) ✓
18:09:00 - PRAYER TIME REACHED (Maghrib #4) ✓
19:27:00 - PRAYER TIME REACHED (Isha #5) ✓
04:28:40 - PRAYER TIME REACHED (Fajr #6) ❌ SHOULD STOP HERE
```

**Code Location:**
- File: `PrayerWidget/MainWindow.xaml.vb`
- Function: `AdvanceToNextPrayerTime()` around line 1080
- Counter: `_testModePrayerCount`

**Current Code:**
```vb
' Increment counter FIRST
_testModePrayerCount += 1

' Check if we've completed 5 prayers - stop test mode
If _testModePrayerCount >= 5 Then
    WriteLog("[AdvanceToNextPrayerTime] TEST MODE COMPLETE: 5 prayers completed")
    ' Clear test mode target to stop further advances
    _testModeTargetTime = DateTime.MinValue
End If
```

**Problem:**
The check happens AFTER the 5th prayer already advances to Fajr. The 5th prayer (Isha) triggers `AdvanceToNextPrayerTime()` which sets counter to 5, but by then it already calculated the next prayer (Fajr) and changed the time.

**Suggested Fix:**
Add check at the START of `AdvanceToNextPrayerTime()`:
```vb
Private Sub AdvanceToNextPrayerTime()
    ' Check FIRST before doing anything
    If _testModePrayerCount >= 5 Then
        WriteLog("[AdvanceToNextPrayerTime] TEST MODE COMPLETE - Already played 5 prayers")
        Return
    End If
    
    ' ... rest of the function
End Sub
```

Also add check in `PlayAthanForPrayer()`:
```vb
Private Sub PlayAthanForPrayer(prayerName As String, Optional testMode As Boolean = False)
    ' Check if we've already done 5 test mode prayers
    If testMode AndAlso _testModePrayerCount >= 5 Then
        WriteLog("[PlayAthanForPrayer] TEST MODE COMPLETE - Not playing athan")
        Return
    End If
    ' ... rest of function
```

---

### Issue 2: Countdown Display May Not Update Correctly

**Expected:**
- Countdown should show remaining time from 20 seconds down to 0
- Progress ring should update accordingly

**Current:**
- Countdown calculation uses `_nextPrayerTime - DateTime.Now`
- When system time changes, `_nextPrayerTime` is updated correctly
- But UI may not refresh immediately

**Code Location:**
- File: `PrayerWidget/MainWindow.xaml.vb`
- Function: `UpdateCountdown()` around line 658

**Current Code:**
```vb
Private Sub UpdateCountdown()
    Dim remaining As TimeSpan = _nextPrayerTime - DateTime.Now
    
    If remaining.TotalSeconds <= 0 Then
        LblCountdown.Text = "00:00:00"
        ' ...
        Return
    End If
    
    ' Update countdown display
    Dim totalSeconds As Integer = CInt(remaining.TotalSeconds)
    LblCountdown.Text = String.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds)
End Sub
```

**Status:** ✅ Working - countdown updates correctly based on logs

---

### Issue 3: UI Label Updates

**Expected:**
- `LblPrayer.Text` should show the next prayer name (FAJR, DHUHR, etc.)
- Should update when advancing to next prayer

**Code Location:**
- File: `PrayerWidget/MainWindow.xaml.vb`
- Function: `AdvanceToNextPrayerTime()` around line 1045

**Current Code:**
```vb
' Update UI elements on the UI thread BEFORE system time change
Me.Dispatcher.Invoke(Sub()
    LblPrayer.Text = nextPrayerName.ToUpper()
    ' Don't set countdown here - UpdateCountdown() will calculate it correctly
End Sub)
```

**Status:** ✅ Working - UI updates via Dispatcher.Invoke()

---

## Files Modified

1. **PrayerWidget/MainWindow.xaml.vb**
   - Line ~785: `PlayAthan()` - detects test mode
   - Line ~795: `PlayAthanForPrayer()` - 4-second stop timer
   - Line ~970: `AdvanceToNextPrayerTime()` - advances time and increments counter
   - Line ~1085: Counter increment and 5-prayer check
   - Line ~658: `UpdateCountdown()` - countdown display

2. **test_isha_toast.ps1**
   - Sets system time to 04:28:40 (20s before Fajr at 04:29:00)
   - Launches app as Administrator
   - Waits 180 seconds for 5 prayer cycles

---

## Test Results Summary

✅ **Working:**
- Test mode detects when within 60 seconds of prayer time
- Sets `_testModeTargetTime` correctly
- Countdown shows ~20 seconds and counts down
- Prayer time triggers at 0 seconds
- Athan plays for 4 seconds
- Time advances to next prayer (20 seconds before)
- Counter increments (1/5, 2/5, 3/5, 4/5, 5/5)
- UI updates with prayer name

❌ **Not Working:**
- Test mode doesn't STOP after 5 prayers
- Continues to 6th prayer (Fajr again)

---

## Next Steps for New Chat

1. **Fix Issue #1** - Add check at START of `AdvanceToNextPrayerTime()` to return if `_testModePrayerCount >= 5`

2. **Verify Fix** - Run test script and confirm log shows:
   ```
   19:27:00 - PRAYER TIME REACHED (Isha #5)
   [AdvanceToNextPrayerTime] TEST MODE COMPLETE - Already played 5 prayers
   ```
   And NO 6th prayer trigger.

3. **Clean up** - Remove debug logging if desired

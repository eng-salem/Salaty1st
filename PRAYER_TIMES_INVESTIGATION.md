# Prayer Times Calculation Investigation Rules

## 🎯 Investigation Framework for Cairo, Egypt Prayer Times Issue

### 📋 Issue Description
Prayer times showing incorrect remaining time for Cairo, Egypt - calculation appears to be inaccurate for local timezone and coordinates.

## 🔍 Systematic Investigation Rules

### Rule 1: API Call Verification
**✅ CHECK**: Verify API is called with correct parameters
- **City**: "Cairo" 
- **Country**: "Egypt"
- **Method**: [Selected calculation method ID]
- **Expected API URL**: `https://api.aladhan.com/v1/timingsByCity?city=Cairo&country=Egypt&method=[ID]&school=1&timezonestring=auto`

**🔍 Debug Output Required**:
```
[API] GetPrayerTimesByCityAsync called with:
  City: Cairo
  Country: Egypt  
  Method: [Method ID]
Fetching prayer times for Cairo, Egypt
```

### Rule 2: API Response Analysis
**✅ CHECK**: Verify API returns correct Cairo prayer times
- **Expected Timezone**: Africa/Cairo or Asia/Cairo
- **Expected Prayer Times**: Standard Cairo prayer times for current date
- **Expected Raw Format**: HH:MM (24-hour format)

**🔍 Debug Output Required**:
```
API returned timezone: [Timezone from API]
Raw prayer times from API:
  Fajr: [API Fajr time]
  Dhuhr: [API Dhuhr time]
  Asr: [API Asr time]
  Maghrib: [API Maghrib time]
  Isha: [API Isha time]
```

### Rule 3: Timezone Conversion Verification
**✅ CHECK**: Verify timezone parsing is correct
- **Cairo Timezone**: Should be "Africa/Cairo" (UTC+2 or UTC+3 during DST)
- **Current System Time**: Should match Cairo local time
- **Expected Behavior**: API times converted to Cairo local time

**🔍 Debug Output Required**:
```
Parsed prayer times with timezone:
  Fajr: [Parsed Fajr time]
  Dhuhr: [Parsed Dhuhr time]
  Asr: [Parsed Asr time]
  Maghrib: [Parsed Maghrib time]
  Isha: [Parsed Isha time]
```

### Rule 4: Coordinates Verification
**✅ CHECK**: Verify Cairo coordinates are correct
- **Expected Latitude**: 30.0444° N
- **Expected Longitude**: 31.2357° E
- **Expected Method**: Should match user selection

**🔍 Debug Output Required**:
```
[API] GetPrayerTimesAsync called with:
  Latitude: [Latitude used]
  Longitude: [Longitude used]
  Method: [Method ID]
```

### Rule 5: Time Calculation Verification
**✅ CHECK**: Verify "time until next prayer" calculation
- **Current Local Time**: Should be Cairo local time
- **Next Prayer Time**: Should be Cairo local time
- **Time Difference**: Should be accurate countdown
- **Expected Behavior**: Countdown should decrement correctly

**🔍 Debug Output Required**:
```
Current local time: [Current Cairo time]
Next prayer: [Next prayer name] at [Next prayer time]
Time until next prayer: [HH:MM:SS]
```

### Rule 6: System Time vs Cairo Time
**✅ CHECK**: Verify system time matches Cairo time
- **System Timezone**: Should be Cairo timezone or properly converted
- **Current Time**: Should match Cairo local time
- **Expected Offset**: UTC+2 (standard) or UTC+3 (DST)

**🔍 Debug Output Required**:
```
System time: [System local time]
UTC time: [UTC time]
Timezone offset: [Current timezone offset]
```

## 🧪 Test Cases to Run

### Test Case 1: Cairo with Egyptian Method
1. **Set Location**: Cairo, Egypt
2. **Set Method**: Egyptian General Authority (ID: 5)
3. **Set Asr**: Shafii (ID: 0)
4. **Save Settings**
5. **Check Debug Output**: Verify all rules above

### Test Case 2: Cairo with Makkah Method  
1. **Set Location**: Cairo, Egypt
2. **Set Method**: Umm al-Qura, Makkah (ID: 4)
3. **Set Asr**: Hanafi (ID: 1)
4. **Save Settings**
5. **Check Debug Output**: Compare with Test Case 1

### Test Case 3: Manual Coordinates
1. **Set Location**: Manual coordinates (30.0444, 31.2357)
2. **Set Method**: Egyptian (ID: 5)
3. **Save Settings**
4. **Check Debug Output**: Compare with city-based method

## 🚨 Common Issues to Check

### Issue 1: Wrong Timezone
- **Problem**: API returns wrong timezone for Cairo
- **Expected**: Africa/Cairo
- **Actual**: Might be Asia/Riyadh or other
- **Fix**: Force Cairo timezone in API call

### Issue 2: DST Handling
- **Problem**: Daylight Saving Time not handled correctly
- **Expected**: UTC+2 or UTC+3 based on current date
- **Actual**: Wrong offset applied
- **Fix**: Proper DST detection

### Issue 3: System Time Mismatch
- **Problem**: System time not matching Cairo time
- **Expected**: Cairo local time
- **Actual**: System local time (different timezone)
- **Fix**: Timezone conversion

### Issue 4: Calculation Method
- **Problem**: Wrong calculation method applied
- **Expected**: User-selected method (Egyptian ID: 5)
- **Actual**: Default method (MWL ID: 1)
- **Fix**: Ensure method parameter passed correctly

## 🔧 Debug Commands to Run

### Command 1: Enable Full Debug
```bash
dotnet run
# Then in app:
# 1. Right-click → "📍 Use IP Location"
# 2. Select Cairo, Egypt
# 3. Select Egyptian method
# 4. Click Save
# 5. Check console output
```

### Command 2: Compare with External Source
1. **Get Reference Times**: Check IslamicFinder or similar for Cairo today
2. **Compare**: Fajr, Dhuhr, Asr, Maghrib, Isha times
3. **Identify**: Which prayer times are incorrect
4. **Pattern**: All prayer times wrong or specific ones?

### Command 3: Manual API Test
```bash
curl "https://api.aladhan.com/v1/timingsByCity?city=Cairo&country=Egypt&method=5&school=1&timezonestring=auto"
# Compare with application output
```

## 📊 Investigation Checklist

- [ ] API called with correct Cairo, Egypt parameters?
- [ ] API returns Africa/Cairo timezone?
- [ ] Raw prayer times match external reference?
- [ ] Timezone conversion preserves Cairo times?
- [ ] System time matches Cairo local time?
- [ ] Time until next prayer calculated correctly?
- [ ] Countdown decrements properly?
- [ ] All prayer times update when settings change?

## 🎯 Success Criteria

### ✅ Investigation Complete When:
1. **Root Cause Identified**: Specific reason for incorrect times
2. **Pattern Understood**: Why Cairo specifically affected
3. **Fix Strategy**: Clear action to resolve issue
4. **Verification Method**: How to confirm fix works

---

**Run these investigation rules systematically to identify exactly why Cairo prayer times are incorrect!**

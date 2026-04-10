# Comprehensive API Testing for Cairo Prayer Times Issue

## 🎯 Test All Possible Cases to Get Root Cause

### 🧪 Test Matrix for Cairo, Egypt

#### Test Case 1: City-Based API Call
**Scenario**: GetPrayerTimesByCityAsync with Cairo, Egypt
```bash
# Manual API Test
curl -X GET "https://api.aladhan.com/v1/timingsByCity?city=Cairo&country=Egypt&method=5&school=1&timezonestring=auto" \
  -H "Accept: application/json"
```

**Expected Results**:
- Timezone: Africa/Cairo
- Method: Egyptian (ID: 5)
- Prayer Times: Valid Cairo times for today

**Debug Points to Check**:
- [ ] API returns correct timezone?
- [ ] Method parameter 5 passed correctly?
- [ ] Raw prayer times reasonable for Cairo?
- [ ] Response time under 2 seconds?

#### Test Case 2: Coordinates-Based API Call
**Scenario**: GetPrayerTimesAsync with Cairo coordinates
```bash
# Manual API Test with Cairo coordinates
curl -X GET "https://api.aladhan.com/v1/timings?latitude=30.0444&longitude=31.2357&method=5&school=1&timezonestring=auto" \
  -H "Accept: application/json"
```

**Expected Results**:
- Same prayer times as city-based call
- Timezone: Africa/Cairo
- Coordinates: 30.0444°N, 31.2357°E

#### Test Case 3: Different Calculation Methods
**Scenario**: Test all methods with Cairo
```bash
# Test Egyptian Method (ID: 5)
curl "https://api.aladhan.com/v1/timingsByCity?city=Cairo&country=Egypt&method=5&school=1&timezonestring=auto"

# Test Makkah Method (ID: 4)  
curl "https://api.aladhan.com/v1/timingsByCity?city=Cairo&country=Egypt&method=4&school=1&timezonestring=auto"

# Test MWL Method (ID: 1)
curl "https://api.aladhan.com/v1/timingsByCity?city=Cairo&country=Egypt&method=1&school=1&timezonestring=auto"

# Test ISNA Method (ID: 2)
curl "https://api.aladhan.com/v1/timingsByCity?city=Cairo&country=Egypt&method=2&school=1&timezonestring=auto"
```

#### Test Case 4: Asr Method Variations
**Scenario**: Test Shafii vs Hanafi with Cairo
```bash
# Shafii (school=0)
curl "https://api.aladhan.com/v1/timingsByCity?city=Cairo&country=Egypt&method=5&school=0&timezonestring=auto"

# Hanafi (school=1) 
curl "https://api.aladhan.com/v1/timingsByCity?city=Cairo&country=Egypt&method=5&school=1&timezonestring=auto"
```

#### Test Case 5: Timezone Variations
**Scenario**: Test different timezone parameters
```bash
# Auto timezone
curl "https://api.aladhan.com/v1/timingsByCity?city=Cairo&country=Egypt&method=5&school=1&timezonestring=auto"

# Explicit Cairo timezone
curl "https://api.aladhan.com/v1/timingsByCity?city=Cairo&country=Egypt&method=5&school=1&timezonestring=Africa/Cairo"

# No timezone parameter
curl "https://api.aladhan.com/v1/timingsByCity?city=Cairo&country=Egypt&method=5&school=1"
```

## 🔍 Application Debug Tests

### Test 1: Full Application Debug Run
**Steps**:
1. Start application with debug output enabled
2. Right-click widget → "📍 Use IP Location"
3. Select "Cairo, Egypt"
4. Select "Egyptian General Authority" method
5. Select "Shafii" Asr method
6. Click Save
7. Monitor console output

**Expected Console Output**:
```
[MainWindow] Location updated in settings
[MainWindow] Cache cleared, reloading prayer times
[DEBUG] Current settings - Method: 5, Asr: 0, Hijri: 0
Loading prayer times...
[API] GetPrayerTimesByCityAsync called with:
  City: Cairo
  Country: Egypt
  Method: 5
Fetching prayer times for Cairo, Egypt
API returned timezone: Africa/Cairo
Raw prayer times from API:
  Fajr: [HH:MM]
  Dhuhr: [HH:MM]
  Asr: [HH:MM]
  Maghrib: [HH:MM]
  Isha: [HH:MM]
Parsed prayer times with timezone:
  Fajr: [HH:MM:SS]
  Dhuhr: [HH:MM:SS]
  Asr: [HH:MM:SS]
  Maghrib: [HH:MM:SS]
  Isha: [HH:MM:SS]
Loaded prayer times:
  Fajr: [HH:MM:SS]
  Dhuhr: [HH:MM:SS]
  Asr: [HH:MM:SS]
  Maghrib: [HH:MM:SS]
  Isha: [HH:MM:SS]
  Next prayer: [Prayer] at [HH:MM:SS]
  Previous prayer: [Prayer] at [HH:MM:SS]
  Time until next prayer: [HH:MM:SS]
```

### Test 2: Compare with External Reference
**Reference Sources**:
- IslamicFinder: https://www.islamicfinder.com/world/egypt/cairo/5.html
- Aladhan Official: https://aladhan.com/prayer-times/cairo/egypt

**Comparison Points**:
- Fajr time match?
- Dhuhr time match?
- Asr time match?
- Maghrib time match?
- Isha time match?

### Test 3: Timezone Investigation
**System Time Check**:
```bash
# Check system timezone
timedatectl status  # Linux
Get-TimeZone -Id "Egypt Standard Time"  # PowerShell
date  # Current system time
```

**Expected**: System should detect Cairo timezone correctly

## 🚨 Potential Root Causes & Tests

### Cause 1: Timezone Mismatch
**Test**: Check if API returns wrong timezone
```bash
# Test what timezone API returns for Cairo
curl "https://api.aladhan.com/v1/timingsByCity?city=Cairo&country=Egypt&method=5&school=1&timezonestring=auto" | jq '.data.meta.timezone'
```

**Expected**: "Africa/Cairo"
**Problem Cases**:
- Returns "Asia/Riyadh" (wrong continent)
- Returns "Asia/Cairo" (wrong continent)
- Returns null/empty

### Cause 2: DST Handling Issue
**Test**: Check DST impact
```bash
# Test during DST transition periods
curl "https://api.aladhan.com/v1/timingsByCity?city=Cairo&country=Egypt&method=5&school=1&timezonestring=auto" \
  --data-urlencode "date=2024-04-01"  # DST start
curl "https://api.aladhan.com/v1/timingsByCity?city=Cairo&country=Egypt&method=5&school=1&timezonestring=auto" \
  --data-urlencode "date=2024-10-01"  # DST end
```

**Expected**: Proper DST adjustment

### Cause 3: Method Parameter Not Passed
**Test**: Verify method ID in API call
```bash
# Check if method=5 is actually sent
curl -v "https://api.aladhan.com/v1/timingsByCity?city=Cairo&country=Egypt&method=5&school=1&timezonestring=auto" 2>&1 | grep "GET"
```

**Expected**: URL contains `method=5`

### Cause 4: Cache Not Cleared
**Test**: Force cache bypass
```bash
# Add timestamp to bypass cache
curl "https://api.aladhan.com/v1/timingsByCity?city=Cairo&country=Egypt&method=5&school=1&timezonestring=auto&t=$(date +%s)"
```

**Expected**: Fresh data every request

### Cause 5: Time Parsing Error
**Test**: Check time format parsing
```json
// Sample API response structure
{
  "data": {
    "timings": {
      "Fajr": "04:15",
      "Sunrise": "05:45", 
      "Dhuhr": "12:15",
      "Asr": "15:30",
      "Maghrib": "18:15",
      "Isha": "19:30"
    },
    "meta": {
      "timezone": "Africa/Cairo"
    }
  }
}
```

**Expected**: HH:MM format parsed correctly

## 📊 Test Execution Plan

### Phase 1: Manual API Testing (5 minutes)
1. Run all curl commands above
2. Compare results with external sources
3. Document any discrepancies

### Phase 2: Application Testing (10 minutes)
1. Run application with Cairo, Egypt
2. Monitor all debug output
3. Test different methods and settings
4. Document console output

### Phase 3: Cross-Validation (5 minutes)
1. Compare manual API results with application results
2. Identify differences
3. Pinpoint root cause

## 🎯 Success Criteria

### ✅ Root Cause Identified When:
- [ ] Manual API calls show expected results
- [ ] Application shows different results
- [ ] Specific discrepancy identified (timezone, method, parsing)
- [ ] Reproducible pattern found

### ✅ Fix Strategy Ready When:
- [ ] Root cause isolated to specific component
- [ ] Fix approach clearly defined
- [ ] Test case to verify fix works
- [ ] Rollback plan if fix breaks other locations

---

**Execute these comprehensive tests systematically to identify exactly why Cairo prayer times are incorrect!**

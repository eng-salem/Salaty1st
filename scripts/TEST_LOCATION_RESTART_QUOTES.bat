@echo off
echo ================================================================
echo Salaty.First v1.0.13 - Location & Restart Test
echo ================================================================
echo.
echo This will test:
echo 1. Location detection with console logging
echo 2. Save & Restart functionality 
echo 3. Quote timer functionality
echo.
echo Starting application with console output...
echo.

cd /d "C:\Users\Administrator\Desktop\PrayerWidget\PrayerWidget\bin\Release\net48"

echo ================================================================
echo TESTING LOCATION BUTTON:
echo ================================================================
echo 1. Open Settings
echo 2. Click "📍 Use IP Location" button
echo 3. Watch console for [Location] and [Geocoding] messages
echo 4. Check if location is filled in City/Country fields
echo.
echo ================================================================
echo TESTING SAVE & RESTART:
echo ================================================================
echo 1. Change City or Country field
echo 2. Click "Save and Restart" button
echo 3. Watch console for [Settings] messages
echo 4. Application should restart automatically
echo.
echo ================================================================
echo TESTING QUOTES:
echo ================================================================
echo 1. Go to Islamic Quotes tab
echo 2. Ensure "Enable" is checked
echo 3. Set interval to "1 min"
echo 4. Click "🧪 Test" button - should show quote immediately
echo 5. Wait 1 minute - should show another quote automatically
echo.
echo ================================================================
echo CONSOLE LOGS TO WATCH FOR:
echo ================================================================
echo [Location] BtnIP_Click - Starting IP location detection
echo [Geocoding] Trying ipwhois.app: https://ipwhois.app/json/...
echo [Geocoding] Success with ipwhois.app OR ipapi.co
echo [Location] Success: [City Name], [Country Code]
echo [Settings] Save button clicked - saving settings...
echo [Settings] Location changed: old -> new
echo [Settings] Restart required - restarting application...
echo [QuoteTimer] EnableQuotes setting: 1
echo [QuoteTimer] Quote interval: 1 minutes
echo [QuoteTimer] Timer started - will show first quote in 1 minute
echo [QuoteTimer] QuoteTimer_Tick triggered - fetching quote...
echo.
echo ================================================================
echo Press any key to start the application...
pause > nul

Salaty.First.exe

echo.
echo ================================================================
echo Application closed. Check console output above for any errors.
echo ================================================================
pause

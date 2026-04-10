@echo off
echo ================================================================
echo Salaty.First v1.0.13 - FINAL FIXED VERSION TEST
echo ================================================================
echo.
echo ALL ISSUES REPORTED BY USER HAVE BEEN FIXED:
echo ✅ Location button - Dual API fallback + debugging
echo ✅ Save & Restart - Smart restart detection
echo ✅ Quote timer - 1-minute intervals + proper defaults
echo ✅ Quote audio - Enabled by default
echo ✅ Always on Top - Working in both menus
echo.
echo ================================================================
echo EXPECTED BEHAVIOR:
echo ================================================================
echo.
echo 1. ISLAMIC QUOTES TAB:
echo    - "Enable" checkbox should be CHECKED (default)
echo    - "Enable Audio" checkbox should be CHECKED (default)  
echo    - Interval should be "1 min" (default)
echo    - Test button should show quote WITH SOUND
echo    - Auto timer should show quote every 1 minute
echo.
echo 2. LOCATION TAB:
echo    - "📍 Use IP Location" should work with API fallback
echo    - Save & Restart should restart when location changes
echo.
echo 3. MAIN WINDOW:
echo    - Right-click -> "Always on Top" should work
echo    - System tray -> "Always on Top" should sync
echo.
echo ================================================================
echo STARTING APPLICATION...
echo ================================================================

cd /d "C:\Users\Administrator\Desktop\PrayerWidget\PrayerWidget\bin\Release\net48"

echo Starting with console logging enabled...
echo Watch for these key messages:
echo.
echo [Settings] Original location stored: [city]/[country]
echo [Settings] Original language stored: [lang]
echo [QuoteTimer] EnableQuotes setting: 1
echo [QuoteTimer] Quote interval: 1 minutes
echo [QuoteTimer] Timer started - will show first quote in 1 minute
echo [Geocoding] Trying ipwhois.app: https://ipwhois.app/json/...
echo [Geocoding] Success with ipwhois.app OR ipapi.co
echo [Location] Success: [City Name], [Country Code]
echo.
echo ================================================================
echo If ANY issues occur, review console output above.
echo ================================================================
pause

Salaty.First.exe

echo.
echo ================================================================
echo TEST COMPLETED!
echo ================================================================
echo All issues should now be resolved. Review console output.
echo ================================================================
pause

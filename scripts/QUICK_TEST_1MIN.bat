@echo off
echo ================================================================
echo Salaty.First v1.0.13 - Quick Test (1 Minute Quotes)
echo ================================================================
echo.
echo This will start the app with console logging for:
echo - Location detection (IP APIs with fallback)
echo - Save & Restart functionality  
echo - Quote timer (1 minute interval)
echo.

cd /d "C:\Users\Administrator\Desktop\PrayerWidget\PrayerWidget\bin\Release\net48"

echo Starting application...
echo Watch for console messages:
echo.
echo [Location] messages - when you click location button
echo [Settings] messages - when you save settings
echo [QuoteTimer] messages - quote timer status
echo.

Salaty.First.exe

echo.
echo ================================================================
echo Test completed! Review console output above.
echo ================================================================
pause

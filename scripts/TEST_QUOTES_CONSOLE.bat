@echo off
echo ================================================================
echo Salaty.First - Console Test for Quotes Timer
echo ================================================================
echo.

echo This will start Salaty.First with console output visible.
echo Watch for [QuoteTimer] messages in the console.
echo.

cd /d "C:\Users\Administrator\Desktop\PrayerWidget\PrayerWidget\bin\Release\net48"

echo Starting application with console output...
echo.
echo Look for these messages:
echo - [QuoteTimer] EnableQuotes setting: 1
echo - [QuoteTimer] Quote interval: 5 minutes  
echo - [QuoteTimer] Timer started - will show first quote in 5 minutes
echo - [QuoteTimer] QuoteTimer_Tick triggered - fetching quote...
echo.

Salaty.First.exe

echo.
echo Application closed.
pause

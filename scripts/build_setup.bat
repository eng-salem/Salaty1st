@echo off
set DEVENV="C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\devenv.com"
set SOLUTION=c:\Users\Administrator\Desktop\PrayerWidget\PrayerWidget\PrayerWidget.sln
set SETUP=c:\Users\Administrator\Desktop\PrayerWidget\Setup\Setup.vdproj

echo =========================================
echo Building PrayerWidget for .NET Framework 4.8...
echo =========================================
%DEVENV% %SOLUTION% /build "Release|Any CPU"
echo.

echo =========================================
echo Publishing PrayerWidget...
echo =========================================
cd c:\Users\Administrator\Desktop\PrayerWidget\PrayerWidget
dotnet publish PrayerWidget.vbproj -c Release -r win-x64 --self-contained false -o publish
echo.

echo =========================================
echo Building Setup Project...
echo =========================================
%DEVENV% %SETUP% /build "Release|Any CPU"
echo.

echo =========================================
echo Build complete!
echo =========================================
echo.
echo Setup output location:
echo   c:\Users\Administrator\Desktop\PrayerWidget\Setup\Release\Setup.msi
echo.
echo =========================================
echo Silent Installation Commands:
echo =========================================
echo.
echo 1. Progress bar (recommended):
echo    msiexec /i "Setup\Release\Setup.msi" /passive /norestart
echo.
echo 2. Completely silent (no UI):
echo    msiexec /i "Setup\Release\Setup.msi" /quiet /norestart
echo.
echo 3. With log file:
echo    msiexec /i "Setup\Release\Setup.msi" /passive /norestart /log "%%TEMP%%\PrayerWidget.log"
echo =========================================
echo.
pause

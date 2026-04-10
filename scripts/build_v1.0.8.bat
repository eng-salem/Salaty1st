@echo off
setlocal enabledelayedexpansion

echo =========================================
echo Building PrayerWidget v1.0.8
echo =========================================
echo.

REM Step 1: Generate new GUIDs for ProductCode and PackageCode
echo [1/5] Generating new GUIDs for Setup...
for /f "delims=" %%a in ('powershell -c "[guid]::NewGuid().ToString('B').ToUpper()"') do set PRODUCTCODE=%%a
for /f "delims=" %%b in ('powershell -c "[guid]::NewGuid().ToString('B').ToUpper()"') do set PACKAGECODE=%%b
echo ProductCode: %PRODUCTCODE%
echo PackageCode: %PACKAGECODE%
echo.

REM Step 2: Update Setup.vdproj with new GUIDs
echo [2/5] Updating Setup.vdproj...
set VDPATH=c:\Users\Administrator\Desktop\PrayerWidget\Setup\Setup.vdproj
powershell -c "(Get-Content '%VDPATH%') -replace '\"ProductCode\" = \"8:\{[^}]+\}\"', '\"ProductCode\" = \"8:%PRODUCTCODE%\"' -replace '\"PackageCode\" = \"8:\{[^}]+\}\"', '\"PackageCode\" = \"8:%PACKAGECODE%\"' | Set-Content '%VDPATH%'"
echo Updated ProductCode and PackageCode in Setup.vdproj
echo.

REM Step 3: Build application
echo [3/5] Building PrayerWidget...
cd c:\Users\Administrator\Desktop\PrayerWidget\PrayerWidget
call dotnet build PrayerWidget.sln -c Release
if errorlevel 1 (
    echo ERROR: Build failed!
    pause
    exit /b 1
)
echo.

REM Step 4: Publish application
echo [4/5] Publishing PrayerWidget...
call dotnet publish PrayerWidget.vbproj -c Release -r win-x64 --self-contained false -o publish
if errorlevel 1 (
    echo ERROR: Publish failed!
    pause
    exit /b 1
)
echo.

REM Step 5: Instructions for Setup project
echo [5/5] Setup Project Instructions
echo =========================================
echo.
echo The Setup.vdproj has been updated with:
echo   - ProductVersion: 1.0.8
echo   - ProductCode: %PRODUCTCODE%
echo   - PackageCode: %PACKAGECODE%
echo   - UpgradeCode: {699C9032-A75D-43D5-B1EF-44B4E1694FDC} (unchanged)
echo.
echo NOW OPEN VISUAL STUDIO AND:
echo   1. Open: c:\Users\Administrator\Desktop\PrayerWidget\Setup\Setup.vdproj
echo   2. Right-click Setup project -^> Rebuild
echo   3. Output: Setup\Release\Setup.msi
echo.
echo OR run this command if Visual Studio is in PATH:
echo   devenv c:\Users\Administrator\Desktop\PrayerWidget\Setup\Setup.vdproj /rebuild "Release|Any CPU"
echo.
echo =========================================
echo INSTALLATION COMMAND:
echo =========================================
echo msiexec /i "c:\Users\Administrator\Desktop\PrayerWidget\Setup\Release\Setup.msi" REINSTALLMODE=voums REINSTALL=ALL /passive /norestart
echo.
pause

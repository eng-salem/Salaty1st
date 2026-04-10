@echo off
echo ========================================
echo PrayerWidget - Clean Install
echo ========================================
echo.

REM Step 1: Uninstall old version
echo [1/3] Uninstalling old version...
msiexec /x {31E73EE1-C403-4424-908E-BCA7C3E47786} /quiet /norestart
timeout /t 3 /nobreak >nul

REM Step 2: Delete old files (if any remain)
echo [2/3] Cleaning up old files...
if exist "C:\Program Files (x86)\Salaty.First" (
    rmdir /s /q "C:\Program Files (x86)\Salaty.First" 2>nul
)

REM Step 3: Install new version
echo [3/3] Installing new version...
msiexec /i "%~dp0Salaty.setup\Release\Salaty.setup.msi" /qb /norestart

echo.
echo ========================================
echo Installation Complete!
echo ========================================
echo.
echo The app should now work from:
echo   C:\Program Files (x86)\Salaty.First\
echo.
echo Log file location:
echo   %%LOCALAPPDATA%%\PrayerWidget\debug.log
echo.
pause

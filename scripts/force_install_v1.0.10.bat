@echo off
echo =========================================
echo Salaty.First v1.0.10 - Force Install
echo =========================================
echo.

set MSI_PATH=c:\Users\Administrator\Desktop\PrayerWidget\Setup\Release\salaty-setup.msi
set LOG_PATH=%TEMP%\Salaty_First_ForceInstall.log

if not exist "%MSI_PATH%" (
    echo ERROR: MSI file not found at: %MSI_PATH%
    echo.
    echo Please build the setup project in Visual Studio first:
    echo   1. Open Visual Studio
    echo   2. Open: c:\Users\Administrator\Desktop\PrayerWidget\Setup\Setup.vdproj
    echo   3. Right-click Setup project -^> Rebuild
    echo.
    pause
    exit /b 1
)

echo [1/2] Uninstalling any existing version...
msiexec /x {A1D27525-3BB1-4A6B-A923-B39A8672EEE4} /passive /norestart 2>nul
timeout /t 3 /nobreak >nul

echo [2/2] Installing Salaty.First v1.0.10...
echo.
echo Command: msiexec /i "%MSI_PATH%" REINSTALLMODE=voums REINSTALL=ALL /passive /norestart
echo.

msiexec /i "%MSI_PATH%" REINSTALLMODE=voums REINSTALL=ALL /passive /norestart /log "%LOG_PATH%"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo =========================================
    echo SUCCESS! Installation complete
    echo =========================================
    echo.
    echo Log file: %LOG_PATH%
    echo.
    
    if exist "C:\Program Files\Salaty.First\Salaty.First.exe" (
        echo Launching Salaty.First...
        start "" "C:\Program Files\Salaty.First\Salaty.First.exe"
    )
) else (
    echo.
    echo =========================================
    echo Installation failed with error: %ERRORLEVEL%
    echo =========================================
    echo.
    echo Check log file: %LOG_PATH%
    echo.
)

pause

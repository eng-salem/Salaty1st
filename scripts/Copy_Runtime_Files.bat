@echo off
echo ============================================================================
echo PrayerWidget - Copy Runtime Files to Installation Folder
echo ============================================================================
echo.
echo This script copies the .NET 5.0 runtime files to the installation folder.
echo.
echo Source: c:\Users\Administrator\Desktop\Extra_Runtime_Files
echo Destination: C:\Program Files (x86)\Salaty.First
echo.
echo Files to copy: 284 files (~137 MB)
echo.
pause
echo.
echo Copying files...
echo.

robocopy "c:\Users\Administrator\Desktop\Extra_Runtime_Files" "C:\Program Files (x86)\Salaty.First" /E /COPY:DAT /R:3 /W:5 /NFL /NDL /NP

echo.
echo ============================================================================
if %ERRORLEVEL% LEQ 7 (
    echo SUCCESS! Runtime files copied.
    echo.
    echo You can now run Salaty.First.exe from:
    echo C:\Program Files (x86)\Salaty.First\Salaty.First.exe
) else (
    echo ERROR! Copy failed.
    echo.
    echo Please make sure:
    echo 1. You are running this script as Administrator
    echo 2. The installation folder exists
    echo 3. No other program is using the files
)
echo ============================================================================
echo.
pause

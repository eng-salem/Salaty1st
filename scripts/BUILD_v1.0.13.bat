@echo off
echo ================================================================
echo Salaty.First v1.0.13 - Setup Build Helper
echo ================================================================
echo.
echo This script will help you build the setup project in Visual Studio
echo.
echo IMPORTANT: You must have Visual Studio installed with:
echo - Visual Studio Installer Projects extension
echo - .NET Framework 4.8 development tools
echo.
echo Press any key to continue...
pause > nul
echo.
echo Opening Visual Studio with the Setup project...
echo.
start "C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\devenv.exe" "c:\Users\Administrator\Desktop\PrayerWidget\Setup\Setup.sln"
echo.
echo ================================================================
echo VISUAL STUDIO INSTRUCTIONS:
echo ================================================================
echo.
echo 1. In Visual Studio, make sure the Setup project is selected
echo 2. Right-click the Setup project and choose "Rebuild"
echo 3. Wait for the build to complete
echo 4. The MSI file will be created at:
echo    c:\Users\Administrator\Desktop\PrayerWidget\Setup\Release\salaty-setup.msi
echo.
echo After building, you can test the installation with:
echo   msiexec /i "c:\Users\Administrator\Desktop\PrayerWidget\Setup\Release\salaty-setup.msi" /passive
echo.
echo ================================================================
echo.
pause

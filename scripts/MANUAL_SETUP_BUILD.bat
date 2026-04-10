@echo off
echo ================================================================
echo Salaty.First v1.0.13 - Manual Setup Build
echo ================================================================
echo.

echo Cleaning previous build...
if exist "Setup\Release\salaty-setup.msi" del "Setup\Release\salaty-setup.msi"
if exist "Setup\Release\salaty-setup.msi" del "Setup\Debug\salaty-setup.msi"

echo.
echo IMPORTANT: You must complete these steps in Visual Studio:
echo.
echo 1. In Visual Studio, right-click "Setup" project
echo 2. Select "Properties" 
echo 3. Go to "Prerequisites" tab
echo 4. Ensure ".NET Framework 4.8" is checked
echo 5. Go to "File System" editor
echo 6. Right-click "Application Folder" -> Add -> Project Output
echo 7. Select "PrayerWidget" -> "Primary output"
echo 8. Right-click "User's Programs Menu" -> Create Shortcut
echo 9. Set shortcut Target to the Primary output
echo 10. Right-click "User's Desktop" -> Create Shortcut  
echo 11. Set shortcut Target to the Primary output
echo 12. Right-click Setup project -> Rebuild
echo.

echo After successful build, MSI will be at:
echo Setup\Release\salaty-setup.msi
echo.

echo Press any key when ready to check for MSI file...
pause

if exist "Setup\Release\salaty-setup.msi" (
    echo SUCCESS: MSI file found!
    echo File: Setup\Release\salaty-setup.msi
    echo Size: 
    dir "Setup\Release\salaty-setup.msi" | find "salaty-setup.msi"
) else (
    echo MSI file not found. Please complete the build steps above.
)

echo.
pause

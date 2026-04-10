@echo off
REM ============================================
REM Send Test Device Data to Turso Database
REM Using curl (built into Windows 10/11)
REM ============================================

echo Sending test device data to Turso database...
echo.

REM Get machine information
set MACHINE_NAME=%COMPUTERNAME%

REM Generate device ID using PowerShell
for /f "delims=" %%i in ('powershell -Command "[System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::Create().ComputeHash([System.Text.Encoding]::UTF8.GetBytes('%USERNAME%-%COMPUTERNAME%'))).Replace('-', '').Substring(0, 16).ToUpper()"') do set DEVICE_ID=%%i

REM Get .NET version
for /f "delims=" %%i in ('powershell -Command "[System.Environment]::Version.ToString()"') do set DOTNET_VERSION=%%i

REM Get OS version  
for /f "delims=" %%i in ('powershell -Command "[System.Environment]::OSVersion.VersionString"') do set OS_VERSION=%%i

set APP_VERSION=1.0.10
set COUNTRY=EG

echo Device Information:
echo   Device ID: %DEVICE_ID%
echo   Machine Name: %MACHINE_NAME%
echo   OS Version: %OS_VERSION%
echo   .NET Version: %DOTNET_VERSION%
echo   App Version: %APP_VERSION%
echo   Country: %COUNTRY%
echo.
echo Sending to Turso database...
echo.

REM Create SQL statement
set SQL=INSERT INTO device_counters (device_id, machine_name, os_version, dotnet_version, app_version, first_seen, last_seen, total_launches, country) VALUES ('%DEVICE_ID%', '%MACHINE_NAME%', '%OS_VERSION%', '%DOTNET_VERSION%', '%APP_VERSION%', datetime('now'), datetime('now'), 1, '%COUNTRY%') ON CONFLICT(device_id) DO UPDATE SET last_seen=datetime('now'), total_launches=total_launches+1, app_version='%APP_VERSION%'

REM Create JSON payload
set JSON={"statements":["%SQL%"]}

REM Send to Turso API using curl
curl -X POST "https://counter-manhag.aws-eu-west-1.turso.io/v2" ^
  -H "Authorization: Bearer YOUR_TURSO_AUTH_TOKEN_HERE" ^
  -H "Content-Type: application/json" ^
  -d "%JSON%" > "%TEMP%\turso_response.json" 2>&1

REM Check result
if %ERRORLEVEL% EQU 0 (
    echo.
    echo SUCCESS! Check response in: %TEMP%\turso_response.json
    type "%TEMP%\turso_response.json"
) else (
    echo.
    echo ERROR: curl failed with exit code %ERRORLEVEL%
    echo.
    echo NOTE: The Turso AWS endpoint may not support the v2 HTTP API.
    echo Please use the Turso dashboard instead:
    echo https://app.turso.tech/manhag/databases/counter/data
)

echo.
echo Press any key to exit...
pause > nul

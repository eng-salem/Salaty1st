@echo off
REM ============================================
REM Send Test Device Data to Turso Database
REM ============================================

echo Sending test device data to Turso database...

REM Get machine information
set MACHINE_NAME=%COMPUTERNAME%
set OS_VERSION=
for /f "tokens=4-5 delims= " %%i in ('ver') do set OS_VERSION=%%i %%j

REM Generate a simple device ID based on username and machine name
set DEVICE_ID=%USERNAME%-%COMPUTERNAME%

REM Get current datetime in UTC format
for /f "delims=" %%i in ('powershell -Command "Get-Date -Format 'yyyy-MM-dd HH:mm:ss'"') do set DATETIME=%%i

REM Create JSON payload
set JSON={"statements":[{"sql":"INSERT INTO device_counters (device_id, machine_name, os_version, dotnet_version, app_version, first_seen, last_seen, total_launches, country) VALUES ('%DEVICE_ID%', '%MACHINE_NAME%', '%OS_VERSION%', '5.0.17', '1.0.10', datetime('now'), datetime('now'), 1, 'EG') ON CONFLICT(device_id) DO UPDATE SET last_seen=datetime('now'), total_launches=total_launches+1, app_version='1.0.10'"}]}

REM Send to Turso API
echo Device ID: %DEVICE_ID%
echo Machine Name: %MACHINE_NAME%
echo OS Version: %OS_VERSION%
echo DateTime: %DATETIME%
echo.
echo Sending to Turso database...

powershell -Command "^
$token = 'YOUR_TURSO_AUTH_TOKEN_HERE'; ^
$url = 'libsql://counter-manhag.aws-eu-west-1.turso.io/v2'; ^
$json = '%JSON%'; ^
$headers = @{ ^
    'Authorization' = \"Bearer $token\"; ^
    'Content-Type' = 'application/json' ^
}; ^
try { ^
    $response = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body $json; ^
    Write-Host 'SUCCESS! Data sent to Turso database' -ForegroundColor Green; ^
    Write-Host ('Rows affected: ' + $response.results.rows_affected) -ForegroundColor Cyan; ^
} catch { ^
    Write-Host ('ERROR: ' + $_.Exception.Message) -ForegroundColor Red; ^
} ^
"

echo.
echo Done!
pause

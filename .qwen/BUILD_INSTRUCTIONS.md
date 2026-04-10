# PrayerWidget (Salaty.First) - Build Instructions

## Version Update Checklist

### 1. Update Version Numbers

**File: `PrayerWidget\My Project\AssemblyInfo.vb`**
```vb
<Assembly: AssemblyVersion("1.0.8.0")>
<Assembly: AssemblyFileVersion("1.0.8.0")>
```

**File: `PrayerWidget\version.json`**
```json
{
  "version": "1.0.8",
  "version_code": 8,
  "release_date": "2026-03-14"
}
```

**File: `Setup\Setup.vdproj`** (around line 2918)
```
"ProductVersion" = "8:1.0.8"
```

---

### 2. Generate New GUIDs for Setup

**File: `Setup\Setup.vdproj`** (around line 2910-2911)

Generate new GUIDs with PowerShell:
```powershell
powershell -c "New-Guid"  # Run twice for ProductCode and PackageCode
```

Update:
```
"ProductCode" = "8:{NEW-GUID-1}"
"PackageCode" = "8:{NEW-GUID-2}"
"UpgradeCode" = "8:{699C9032-A75D-43D5-B1EF-44B4E1694FDC}"  ← KEEP SAME!
```

⚠️ **IMPORTANT:** Never change `UpgradeCode` - it must stay the same for upgrades to work!

---

### 3. Build Application

```cmd
cd c:\Users\Administrator\Desktop\PrayerWidget\PrayerWidget
dotnet build PrayerWidget.sln -c Release
```

---

### 4. Publish Application

```cmd
cd c:\Users\Administrator\Desktop\PrayerWidget\PrayerWidget
dotnet publish PrayerWidget.vbproj -c Release -r win-x64 --self-contained false -o publish
```

---

### 5. Build Setup Project

**Option A: Using Visual Studio (Recommended)**
1. Open Visual Studio
2. Open `c:\Users\Administrator\Desktop\PrayerWidget\Setup\Setup.vdproj`
3. Right-click **Setup project** → **Rebuild**
4. Output: `Setup\Release\Setup.msi`

**Option B: Using Command Line**
```cmd
"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.com" ^
  c:\Users\Administrator\Desktop\PrayerWidget\Setup\Setup.vdproj ^
  /rebuild "Release|Any CPU"
```

---

### 6. Install/Upgrade

**For New Installation:**
```cmd
cd c:\Users\Administrator\Desktop\PrayerWidget\Setup\Release
msiexec /i Setup.msi /passive
```

**For Upgrade (with existing installation):**
```cmd
cd c:\Users\Administrator\Desktop\PrayerWidget\Setup\Release
msiexec /i Setup.msi REINSTALLMODE=voums REINSTALL=ALL /passive /norestart
```

**Silent Installation:**
```cmd
msiexec /i Setup.msi /quiet /norestart
```

**With Log File:**
```cmd
msiexec /i Setup.msi /passive /norestart /log "%TEMP%\PrayerWidget.log"
```

---

## Quick Build Script

Save as `build_release.bat`:

```batch
@echo off
set VERSION=1.0.8

echo =========================================
echo Building PrayerWidget v%VERSION%
echo =========================================

echo [1/4] Building application...
cd c:\Users\Administrator\Desktop\PrayerWidget\PrayerWidget
dotnet build PrayerWidget.sln -c Release

echo [2/4] Publishing application...
dotnet publish PrayerWidget.vbproj -c Release -r win-x64 --self-contained false -o publish

echo [3/4] Building setup project...
echo Please rebuild Setup.vdproj in Visual Studio

echo [4/4] Complete!
echo Setup location: c:\Users\Administrator\Desktop\PrayerWidget\Setup\Release\Setup.msi
echo.
echo To install: msiexec /i Setup\Release\Setup.msi REINSTALLMODE=voums REINSTALL=ALL /passive
pause
```

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.10 | 2026-03-14 | Cairo font, toast at athan, Ctrl+T test, upgrade fixes |
| 1.0.9 | 2026-03-14 | Cairo font, toast at athan, Ctrl+T test, upgrade fixes |
| 1.0.8 | 2026-03-14 | Setup upgrade fixes |
| 1.0.7 | 2026-03-12 | Cairo font in toast, larger font sizes |
| 1.0.6 | 2026-03-10 | Duaa after Athan feature |

---

## Troubleshooting

### Error 1638: "Another version is already installed"

Use force upgrade command:
```cmd
msiexec /i Setup.msi REINSTALLMODE=voums REINSTALL=ALL /passive
```

### Setup Build Fails

1. Ensure Visual Studio has Setup Project extension
2. Check that all files in publish folder exist
3. Verify paths in Setup.vdproj are correct

### Upgrade Doesn't Work

1. Check `UpgradeCode` is the same as installed version
2. Ensure `ProductVersion` is higher than installed version
3. Use `REINSTALLMODE=voums` parameter

---

## Key Settings Summary

| Setting | File | Rule |
|---------|------|------|
| ProductVersion | Setup.vdproj | CHANGE every release |
| ProductCode | Setup.vdproj | CHANGE every release (new GUID) |
| PackageCode | Setup.vdproj | CHANGE every rebuild (new GUID) |
| UpgradeCode | Setup.vdproj | NEVER CHANGE |
| AssemblyVersion | AssemblyInfo.vb | CHANGE every release |
| version | version.json | CHANGE every release |

# Visual Studio Setup Project - Major Upgrade Configuration Guide

## Overview

This guide explains how to configure a Visual Studio Setup Project (`.vdproj`) to perform a **Major Upgrade** that seamlessly removes previous versions of your application before installing the new version.

---

## Step 1: Extract the UpgradeCode

### Option A: From Installed Product (Registry)

```powershell
# Navigate to scripts directory
cd C:\Users\Administrator\Desktop\PrayerWidget\scripts

# Run the extraction script
.\Extract-UpgradeCode.ps1 -ProductName "salaty.first" -Method Registry
```

### Option B: From MSI File

```powershell
# Extract from existing MSI file
.\Extract-UpgradeCode.ps1 -MsiPath "C:\Users\Administrator\Desktop\salaty-setup.msi" -Method MsiFile
```

### Option C: Using Dark.exe Directly

```powershell
# If WiX Toolset is installed, use Dark.exe to extract Property table
dark.exe -nologo -d extracted.wxs -t Property "C:\Users\Administrator\Desktop\salaty-setup.msi"

# Then search for UpgradeCode in extracted.wxs
Select-String -Path extracted.wxs -Pattern "UpgradeCode"
```

### Expected Output

```
═══════════════════════════════════════════════════════════
EXTRACTED UPGRADE CODE:
═══════════════════════════════════════════════════════════
  UpgradeCode: {XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX}
═══════════════════════════════════════════════════════════
```

**⚠️ IMPORTANT:** Copy this UpgradeCode - you'll need it in the next steps.

---

## Step 2: Configure Visual Studio Setup Project

### 2.1 Open the Setup Project

1. Open your solution in Visual Studio:
   ```
   C:\Users\Administrator\Desktop\PrayerWidget\PrayerWidget\PrayerWidget.WithSetup.sln
   ```

2. In **Solution Explorer**, click on the **Setup** project

3. Press **F4** to open the **Properties Window** (not the right-click Properties menu)

### 2.2 Set the UpgradeCode

| Property | Value | Description |
|----------|-------|-------------|
| **UpgradeCode** | `{YOUR-EXTRACTED-UPGRADECODE}` | **MUST match the old MSI** |
| **RemovePreviousVersions** | `True` | Enables automatic removal |
| **DetectNewerInstalled** | `False` | Allow downgrade if needed |
| **Version** | Increment this (e.g., 1.0.10 → 1.0.11) | Must be higher than previous |

### 2.3 Critical Settings Checklist

```
□ UpgradeCode       = {SAME as previous version}
□ ProductCode       = {NEW - auto-generated on each build}
□ Version           = INCREMENTED (e.g., 1.0.9 → 1.0.10)
□ RemovePreviousVersions = TRUE
□ DetectNewerInstalled   = FALSE (unless you want to block downgrades)
```

---

## Step 3: Understanding the Upgrade Mechanism

### How Major Upgrade Works

```
┌─────────────────────────────────────────────────────────────────┐
│                    MAJOR UPGRADE FLOW                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. Windows Installer checks UpgradeCode in registry            │
│         ↓                                                       │
│  2. Finds existing product with matching UpgradeCode            │
│         ↓                                                       │
│  3. Compares Version numbers                                    │
│         ↓                                                       │
│  4. If RemovePreviousVersions = TRUE:                           │
│         → Uninstalls old version first                          │
│         → Then installs new version                             │
│         ↓                                                       │
│  5. New ProductCode is registered                               │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### UpgradeCode vs ProductCode

| Code Type | Purpose | Should Change? |
|-----------|---------|----------------|
| **UpgradeCode** | Identifies the product family across versions | **NO** - Keep constant |
| **ProductCode** | Identifies a specific version | **YES** - New each build |

---

## Step 4: Build the Setup Project

### 4.1 Increment Version Number

In the Setup project Properties (F4):

1. Find the **Version** property
2. Click on it
3. Click the **...** button that appears
4. Increment the **Revision** or **Build** number
5. Click **OK**

> ⚠️ **WARNING:** When you change the Version, Visual Studio will prompt:
> *"The ProductCode will be regenerated. Do you want to continue?"*
> 
> Click **YES** - this is expected and required!

### 4.2 Build Configuration

```
1. Right-click Setup project → Build
   OR
2. Build → Build Solution (Ctrl+Shift+B)
```

Output location:
```
C:\Users\Administrator\Desktop\PrayerWidget\Setup\bin\Debug\salaty-setup.msi
C:\Users\Administrator\Desktop\PrayerWidget\Setup\bin\Release\salaty-setup.msi
```

---

## Step 5: Testing the Upgrade

### Test Scenario

```powershell
# 1. Verify old version is installed
wmic product where "name like '%salaty%'" get Name, Version

# 2. Install new version (should auto-remove old)
msiexec /i "C:\path\to\new-salaty-setup.msi" /qb

# 3. Verify upgrade succeeded
wmic product where "name like '%salaty%'" get Name, Version
```

### Expected Behavior

| Scenario | Expected Result |
|----------|-----------------|
| Same UpgradeCode, Higher Version | ✓ Old removed, new installed |
| Same UpgradeCode, Same Version | ✗ Error: "Another version is already installed" |
| Same UpgradeCode, Lower Version | Depends on DetectNewerInstalled |
| Different UpgradeCode | ✗ Both versions coexist (side-by-side) |

---

## Troubleshooting

### Error: "Another version of this product is already installed"

**Cause:** Version number not incremented or ProductCode not rotated.

**Solution:**
1. Open Setup project
2. Press F4 for Properties
3. Increment the Version number
4. Click YES when prompted to regenerate ProductCode
5. Rebuild

### Error: "Setup cannot determine if a newer version is installed"

**Cause:** DetectNewerInstalled setting conflict.

**Solution:** Set `DetectNewerInstalled = False` unless you specifically need downgrade protection.

### Both Versions Installed (Side-by-Side)

**Cause:** UpgradeCode changed between versions.

**Solution:** 
1. Extract the original UpgradeCode from the old MSI
2. Set it in your new Setup project
3. Rebuild with incremented Version

### Previous Version Not Removed

**Cause:** RemovePreviousVersions not set correctly.

**Solution:** Verify `RemovePreviousVersions = True` in project properties.

---

## Quick Reference: Dark.exe Commands

```powershell
# Extract entire MSI to WiX source
dark.exe -x output_folder setup.msi

# Extract only Property table
dark.exe -t Property -d output.wxs setup.msi

# Extract specific tables
dark.exe -t Property -t Upgrade -d output.wxs setup.msi

# View all tables (outputs to console)
dark.exe -d output.wxs setup.msi
```

---

## Quick Reference: Orca.exe Usage

```
1. Open Orca.exe
2. File → Open → select your .msi
3. In left panel, select "Property" table
4. Find "UpgradeCode" row
5. Value column shows the UpgradeCode
```

---

## File Locations Summary

| File | Path |
|------|------|
| Extraction Script | `C:\Users\Administrator\Desktop\PrayerWidget\scripts\Extract-UpgradeCode.ps1` |
| Tool Installer | `C:\Users\Administrator\Desktop\PrayerWidget\scripts\Install-MsiTools.ps1` |
| Setup Project | `C:\Users\Administrator\Desktop\PrayerWidget\Setup\Setup.vdproj` |
| Old MSI | `C:\Users\Administrator\Desktop\salaty-setup.msi` |
| Output MSI (Debug) | `C:\Users\Administrator\Desktop\PrayerWidget\Setup\bin\Debug\salaty-setup.msi` |
| Output MSI (Release) | `C:\Users\Administrator\Desktop\PrayerWidget\Setup\bin\Release\salaty-setup.msi` |

---

## Summary Checklist

Before distributing your new installer:

```
□ UpgradeCode matches previous version
□ Version number is incremented
□ ProductCode has been regenerated (automatic)
□ RemovePreviousVersions = True
□ Build completed without errors
□ Upgrade tested on clean VM with old version installed
□ Rollback tested (install fails, system returns to original state)
```

---

## Additional Resources

- [Microsoft: Major Upgrade Rules](https://docs.microsoft.com/en-us/windows/win32/msi/major-upgrades)
- [WiX Toolset Documentation](https://wixtoolset.org/documentation/)
- [Windows Installer SDK](https://docs.microsoft.com/en-us/windows/win32/msi/windows-installer-portal)

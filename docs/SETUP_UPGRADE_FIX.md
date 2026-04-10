# Setup Project Upgrade Configuration

## Problem
Windows Installer refuses to install because it detects the same version is already installed.

## Root Cause
The setup project has `"RemovePreviousVersions" = "11:FALSE"` which prevents automatic upgrades.

---

## Solution: Configure Proper Upgrades

### Step 1: Update version.json
Increment the version number for each release:

```json
{
  "version": "1.0.3",
  "version_code": 4,
  "release_date": "2026-03-09"
}
```

### Step 2: Modify Setup Project Properties

**In Visual Studio:**

1. Open `PrayerWidget.sln` in Visual Studio
2. Right-click on **Salaty.setup** project → **Properties**
3. In the Properties window, find **Product** section
4. Set these properties:

| Property | Value | Description |
|----------|-------|-------------|
| **RemovePreviousVersions** | **True** | ✅ Enables automatic uninstall of old version |
| **DetectNewerInstalledVersion** | True | Prevents downgrading |
| **InstallAllUsers** | True | Install for all users |
| **ProductVersion** | 1.0.3 | Match version.json |

### Step 3: Rebuild Setup Project

```batch
cd c:\Users\Administrator\Desktop\PrayerWidget
build_setup.bat
```

---

## Manual Fix (For Testing Now)

If you need to test immediately without rebuilding:

### Option A: Uninstall Manually
1. Control Panel → Programs → Uninstall a Program
2. Find **Salaty.First**
3. Uninstall
4. Run setup.exe again

### Option B: Command Line Uninstall
```batch
# Find the product code
wmic product where "name like 'Salaty%'" get IdentifyingNumber,Name

# Uninstall using product code
msiexec /x {120856F9-BE55-4547-8700-0CAAC4F9581A} /quiet
```

### Option C: Install with REINSTALLMODE (Force Overwrite)
```batch
msiexec /i "Salaty.setup.msi" REINSTALL=ALL REINSTALLMODE=vomus /qb
```

**Warning:** This forces installation without uninstalling first, which may leave registry entries.

---

## For Production Releases

### Release Checklist

1. ✅ **Increment version** in `version.json`
2. ✅ **Update ProductVersion** in setup project
3. ✅ **Set RemovePreviousVersions = True**
4. ✅ **Rebuild setup project** (generates new ProductCode)
5. ✅ **Keep UpgradeCode the same** (for upgrade detection)
6. ✅ **Test upgrade** on clean system

### Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.3 | 2026-03-09 | Enabled automatic upgrades |
| 1.0.2 | 2026-03-08 | Islamic quotes, scrollable display |
| 1.0.1 | 2026-03-07 | SQLite permissions fix |
| 1.0.0 | - | Initial release |

---

## Technical Details

### UpgradeCode vs ProductCode

| Code Type | Purpose | Should Change? |
|-----------|---------|----------------|
| **UpgradeCode** | Identifies product family | ❌ NO - Keep same |
| **ProductCode** | Identifies specific version | ✅ YES - New each build |
| **PackageCode** | Identifies MSI package | ✅ YES - New each build |

### Current Values (Salaty.setup.vdproj)

```
UpgradeCode   = {303A6023-0743-464D-A122-1F595C402760} ← KEEP SAME
ProductCode   = {120856F9-BE55-4547-8700-0CAAC4F9581A} ← CHANGES EACH BUILD
PackageCode   = {627673B8-AB9A-4C99-AF04-E1BC8F592D0C} ← CHANGES EACH BUILD
```

### How Upgrades Work

1. **User runs new setup.exe**
2. **Windows Installer checks UpgradeCode**
3. **Finds existing installation** with same UpgradeCode
4. **Checks RemovePreviousVersions**
   - If **True**: Uninstalls old, installs new ✅
   - If **False**: Blocks installation ❌
5. **Installs new version**

---

## Troubleshooting

### Error: "Another version of this product is already installed"

**Cause:** RemovePreviousVersions is False

**Fix:** Set to True and rebuild

### Error: "Installation failed with error 1603"

**Cause:** Files in use or permission issues

**Fix:** 
1. Close the app
2. Run as Administrator
3. Check Windows Installer logs

### Upgrade Doesn't Remove Old Files

**Cause:** Files not in setup project

**Fix:** Add cleanup custom action or use major upgrade

---

## References

- [Microsoft Docs: Upgrades](https://docs.microsoft.com/en-us/windows/win32/msi/upgrades)
- [Microsoft Docs: RemovePreviousVersions](https://docs.microsoft.com/en-us/windows/win32/msi/removepreviousversions)
- [Windows Installer SDK](https://docs.microsoft.com/en-us/windows/win32/msi/windows-installer-portal)

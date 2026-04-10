# PrayerWidget - Runtime Files Comparison

## Problem

The installed application asks for .NET 5.0 runtime, but the Debug folder works without it.

## Why Debug Works Without Installing .NET

**Debug Folder Location:**
```
c:\Users\Administrator\Desktop\PrayerWidget\PrayerWidget\bin\Debug\net5.0-windows7.0\
```

**Files:** 22 items (smaller)
- Uses Visual Studio's shared .NET runtime
- Framework-dependent deployment
- Relies on development environment

## Why Installed Version Asks for .NET

**Release Folder Location:**
```
c:\Users\Administrator\Desktop\PrayerWidget\PrayerWidget\bin\Release\net5.0-windows7.0\win-x64\
```

**Files:** 291 items (~137 MB)
- Includes entire .NET 5.0 runtime
- Self-contained deployment
- Can run without .NET installed

## Solution

### Option 1: Install .NET 5.0 Runtime (Recommended)
1. Download from: https://dotnet.microsoft.com/download/dotnet/5.0
2. Install: **Windows x64 Desktop Runtime 5.0.x**
3. Run from: `C:\Program Files (x86)\Salaty.First\Salaty.First.exe`

### Option 2: Copy Runtime Files
Copy all files from Release folder to installation folder:
```
From: c:\Users\Administrator\Desktop\PrayerWidget\PrayerWidget\bin\Release\net5.0-windows7.0\win-x64\
To:   C:\Program Files (x86)\Salaty.First\
```

## Files Comparison

### Debug Has (22 files):
```
Hardcodet.NotifyIcon.Wpf.dll
Microsoft.Bcl.AsyncInterfaces.dll
Microsoft.Data.Sqlite.dll
Salaty.First.* (exe, dll, pdb, deps.json, runtimeconfig.json)
salaty.sqlite
SQLitePCLRaw.* (3 files)
System.Drawing.Common.dll
System.IO.Pipelines.dll
System.Runtime.CompilerServices.Unsafe.dll
System.Text.Encodings.Web.dll
System.Text.Json.dll
Icons/ (folder)
Resources/ (folder)
Translations/ (folder)
```

### Release Has Extra (269 additional files):
```
.NET Runtime:
- coreclr.dll, hostfxr.dll, hostpolicy.dll
- System.Private.CoreLib.dll, mscorlib.dll
- All System.*.dll files (100+ files)

WPF Framework:
- PresentationCore.dll, PresentationFramework.dll
- WindowsBase.dll, System.Xaml.dll
- UIAutomation*.dll

Windows Forms:
- System.Windows.Forms.dll
- System.Windows.Forms.Design.dll

C Runtime:
- api-ms-win-core-*.dll (28 files)
- api-ms-win-crt-*.dll (17 files)
- ucrtbase.dll, vcruntime140_cor3.dll

Native Libraries:
- D3DCompiler_47_cor3.dll
- PenImc_cor3.dll, wpfgfx_cor3.dll
- e_sqlite3.dll, clrcompression.dll

Language Resources:
- cs/, de/, es/, fr/, it/, ja/, ko/, pl/, pt-BR/, ru/, tr/, zh-Hans/, zh-Hant/
```

## Installation Folder Structure

After copying all files, `C:\Program Files (x86)\Salaty.First\` should contain:
- All 291 files from Release folder
- Same structure as `bin\Release\net5.0-windows7.0\win-x64\`

## Recommendation

**For End Users:** Install .NET 5.0 Runtime (smaller download, system-wide benefit)

**For Deployment:** Consider making the installer include all runtime files (larger MSI, but no extra installation step)

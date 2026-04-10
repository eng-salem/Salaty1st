# Salaty.First v1.0.13 - Build Summary

**Date:** March 23, 2026  
**Version:** 1.0.13  
**Status:** ✅ Ready for Visual Studio Build

---

## 🎯 Changes Made in v1.0.13

### **New Features:**
1. **Always on Top Setting**
   - Added to widget context menu (right-click)
   - Added to system tray context menu (right-click)
   - Bidirectional sync between both menus
   - Persists across application restarts

2. **Improved Context Menu Styling**
   - Replaced disabled menu items with proper separators
   - Cleaner, more professional appearance
   - Consistent spacing across all menus

### **Bug Fixes:**
- Fixed InvalidCastException in ApplyTranslations method
- Updated menu indices to handle new "Always on Top" item
- Fixed translation system compatibility

---

## 📦 Build Components

### **Application Build:** ✅ COMPLETE
- **Source:** `PrayerWidget\PrayerWidget.vbproj`
- **Configuration:** Release
- **Target:** .NET Framework 4.8
- **Output:** `bin\Release\net48\Salaty.First.exe` (633KB)
- **Self-Contained:** `bin\Release\net48\win-x64\publish\` (4.2MB)

### **Version Updates:** ✅ COMPLETE
- **AssemblyInfo.vb:** 1.0.13.0
- **version.json:** 1.0.13
- **Setup.vdproj:** 1.0.13

### **New GUIDs Generated:** ✅ COMPLETE
- **ProductCode:** `{2C9EECE3-F989-4BF4-8CDE-3EA2A3ECF34D}`
- **PackageCode:** `{B0D95DFB-66F6-4FFD-A6ED-413DAF4A2A60}`
- **UpgradeCode:** `{699C9032-A75D-43D5-B1EF-44B4E1694FDC}` (unchanged)

---

## 🔨 Visual Studio Build Instructions

### **Required Action:**

1. **Open Visual Studio**
2. **Open Project:**
   ```
   c:\Users\Administrator\Desktop\PrayerWidget\Setup\Setup.vdproj
   ```

3. **Build Setup:**
   - Right-click Setup project → **Rebuild**
   - OR Press **Ctrl+Shift+B** with Setup project selected

4. **Output Location:**
   ```
   c:\Users\Administrator\Desktop\PrayerWidget\Setup\Release\salaty-setup.msi
   ```

---

## 📋 Installation Commands

### **New Installation:**
```cmd
cd c:\Users\Administrator\Desktop\PrayerWidget\Setup\Release
msiexec /i salaty-setup.msi /passive
```

### **Upgrade from v1.0.12:**
```cmd
cd c:\Users\Administrator\Desktop\PrayerWidget\Setup\Release
msiexec /i salaty-setup.msi REINSTALLMODE=voums REINSTALL=ALL /passive /norestart
```

### **Silent Installation:**
```cmd
msiexec /i salaty-setup.msi /quiet /norestart
```

---

## 🧪 Testing Checklist

After building and installing:

- [ ] Widget appears on desktop
- [ ] Right-click widget → "Always on Top" option works
- [ ] Right-click system tray → "Always on Top" option works
- [ ] Both menus stay in sync when toggled
- [ ] Setting persists after restart
- [ ] Context menus show proper separators
- [ ] No crashes on startup
- [ ] Upgrade from previous version works

---

## 📤 Release Notes Summary

### **What's New in v1.0.13:**

✨ **Features:**
- Always on Top toggle in widget and system tray menus
- Bidirectional sync between menus
- Improved context menu styling with proper separators

🐛 **Bug Fixes:**
- Fixed InvalidCastException in ApplyTranslations method
- Updated translation system for new menu structure

🔧 **Technical:**
- Version bumped to 1.0.13
- New ProductCode and PackageCode for proper upgrades
- Maintained UpgradeCode for seamless upgrades

---

## ✅ Build Status Summary

```
✅ Version Update:            COMPLETE (v1.0.13)
✅ Application Build:         COMPLETE
✅ Application Publish:       COMPLETE
✅ GUID Generation:           COMPLETE
✅ Setup Project Updated:     COMPLETE
⏳ Setup Build:               PENDING (Visual Studio required)
```

---

**🎉 v1.0.13 is ready! Just build the setup in Visual Studio and you're done!**

**Next command:**
```
Open Visual Studio → Open Setup\Setup.vdproj → Rebuild
```

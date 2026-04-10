# Salaty.First v1.0.9 - Release Notes

## 🕌 Islamic Prayer Times Widget

**Release Date:** March 14, 2026

---

## ✨ What's New

### 🎨 Cairo Font Support
- **Arabic Text Rendering:** Toast notifications now use Cairo font for beautiful Arabic text display
- **Larger Font Sizes:** Increased from 14→16 (title), 12→14 (message), 11→12 (now playing)
- **Proper Font Embedding:** Fonts now embedded as WPF resources for reliable rendering
- **Normal Font Style:** Removed italic rendering for cleaner appearance

### 🔔 Enhanced Athan Notifications
- **Toast at Prayer Time:** Notification now appears when athan plays (not just reminders)
- **Audio Controls:** Play/pause, stop, volume, and progress bar in toast notification
- **Single Playback:** Athan sound plays once when prayer time occurs
- **Keyboard Test Shortcut:** Press `Ctrl+T` to test athan notification instantly

### 📦 Improved Setup & Upgrades
- **Seamless Upgrades:** Setup now properly upgrades previous versions
- **REINSTALLMODE Support:** Force upgrade with `msiexec /i Setup.msi REINSTALLMODE=voums REINSTALL=ALL`
- **Version Tracking:** Updated assembly info and version.json for better update detection

---

## 🐛 Bug Fixes

- Fixed toast notification font not displaying correctly (now embedded as WPF resource)
- Fixed athan sound playing multiple times at prayer time
- Fixed setup upgrade path requiring manual uninstall of old version
- Fixed font style showing italic instead of normal

---

## 🔧 Technical Improvements

- Toast notifications use Cairo font from `Resources/cairo.ttf`
- Font files configured as `<Resource>` in project file for proper WPF embedding
- `PlayAthan()` now triggers toast notification with audio controls
- Keyboard handler added for `Ctrl+T` test shortcut
- Setup project configured with proper UpgradeCode for seamless upgrades

---

## 📋 Features (Complete List)

- ✅ Prayer times calculation for 6 major methods (MWL, ISNA, Makkah, Egypt, Karachi, Tehran)
- ✅ Asr method selection (Shafii/Hanafi)
- ✅ Before/After prayer reminders with toast notifications
- ✅ Hijri date display with ±1 day adjustment for moon sighting
- ✅ Customizable widget and notification opacity
- ✅ Toast notifications with settings quick access (gear icon)
- ✅ IP-based location detection (ipwhois.app)
- ✅ Geocoding address search (Nominatim/OpenStreetMap)
- ✅ Athan sound notifications (multiple reciters)
- ✅ Duaa after Athan - automatic spiritual duas
- ✅ Summer time adjustment
- ✅ Offline prayer time calculation (no internet required)
- ✅ City database with 23,000+ cities worldwide
- ✅ Automatic update checking
- ✅ Islamic quotes with audio notification
- ✅ **NEW:** Cairo font for Arabic text rendering
- ✅ **NEW:** Keyboard shortcut Ctrl+T to test athan

---

## 📥 Installation

### For New Users
```cmd
msiexec /i Salaty.First.v1.0.9.msi /passive
```

### For Upgrading from Previous Versions
```cmd
msiexec /i Salaty.First.v1.0.9.msi REINSTALLMODE=voums REINSTALL=ALL /passive /norestart
```

### Silent Installation
```cmd
msiexec /i Salaty.First.v1.0.9.msi /quiet /norestart
```

---

## ⚙️ System Requirements

- **OS:** Windows 10/11 (64-bit)
- **Framework:** .NET 5.0 Desktop Runtime (or use bundled runtime)
- **Database:** SQLite (included)
- **Languages:** English, Arabic, French, Turkish, Urdu

---

## 🎯 Quick Start

1. **Install:** Run the MSI installer
2. **Launch:** Salaty.First widget appears on desktop
3. **Configure:** Right-click → Settings → Select your city and preferences
4. **Test:** Press `Ctrl+T` to test notification
5. **Enjoy:** Automatic prayer time notifications!

---

## 🔍 Testing the Build

After installation:
1. Widget appears as floating circular progress indicator
2. Shows countdown to next prayer time
3. At prayer time: Toast notification appears with athan sound
4. Press `Ctrl+T` anytime to test the notification
5. Right-click system tray icon for settings and options

---

## 📝 Version History

| Version | Date | Key Changes |
|---------|------|-------------|
| 1.0.9 | 2026-03-14 | Cairo font, enhanced notifications, Ctrl+T test |
| 1.0.8 | 2026-03-14 | Setup upgrade fixes |
| 1.0.7 | 2026-03-12 | Cairo font in toast, larger fonts |
| 1.0.6 | 2026-03-10 | Duaa after Athan feature |
| 1.0.5 | 2026-03-08 | Islamic quotes with audio |
| 1.0.4 | 2026-03-05 | Toast notification improvements |

---

## 🐛 Known Issues

- Language switch in settings requires app restart for circular widget text update
- High latitude locations use MidNight method for extreme cases
- IP location detection is city-level approximation

---

## 📞 Support & Feedback

| Resource | Link |
|----------|------|
| **GitHub** | https://github.com/eng-salem/Salaty1st |
| **Issues** | https://github.com/eng-salem/Salaty1st/issues |
| **Facebook** | https://www.facebook.com/Salaty.1st |
| **Download** | https://github.com/eng-salem/Salaty1st/releases/latest |

---

## 📄 License

Copyright © 2026 Salaty.First - All Rights Reserved

---

## 🙏 Credits

- **Prayer Times Algorithm:** PrayTime.org
- **City Database:** 23,000+ cities worldwide
- **Font:** Cairo Font (Google Fonts)
- **Icons:** Material Design Icons
- **Framework:** .NET 5.0 WPF

---

**Thank you for using Salaty.First! May your prayers be accepted. 🤲**

# 🕌 PrayerWidget v1.0.4 - Release Notes

**Release Date:** March 9, 2026  
**Download:** [setup.exe](https://github.com/eng-salem/Salaty1st/releases/latest)  
**Size:** ~30 MB  
**Platform:** Windows 10/11 (x64)

---

## ✨ What's New

### 🔄 Automatic Upgrades
- **Seamless updates** - New version automatically replaces old version
- **No manual uninstall needed** - Setup handles everything
- **Upgrade protection** - Prevents accidental downgrades

### 💬 Islamic Quotes Display
- **New dedicated tab** in settings for Islamic quotes
- **Enabled by default** - Shows inspirational quotes periodically
- **Audio notification** - Plays `quotes.mp3` when quote appears
- **Scrollable display** - Supports long multi-line quotes
- **RTL support** - Proper Arabic text rendering
- **Copy to clipboard** - One-click copy button

### 📋 Improved Update Dialog
- **Changelog display** - See what's new before updating
- **Screenshot preview** - Visual preview of new features
- **Clear instructions** - Shows setup.exe installation steps
- **Direct download** - Opens GitHub releases page

### 🎯 Bug Fixes
- **Quote text parsing** - Fixed API response handling (`post_content` field)
- **Source extraction** - Properly extracts hadith source from brackets
- **HTML entity decoding** - Handles special characters correctly

---

## 🔧 Technical Changes

### Modified Files
- `QuoteService.vb` - Enhanced JSON parsing for quotes API
- `QuoteNotificationWindow.xaml` - Added scrollviewer, RTL, copy button
- `QuoteNotificationWindow.xaml.vb` - Audio playback, text cleaning
- `SettingsWindow.xaml` - New Islamic Quotes tab
- `SettingsWindow.xaml.vb` - Quote settings management
- `MainWindow.xaml.vb` - Update checker with changelog
- `Salaty.setup.vdproj` - Enabled automatic upgrades

### New Settings
- `EnableQuotes` (default: 1) - Enable/disable quotes
- `QuoteInterval` (default: 30 min) - Display frequency
- `EnableQuoteAudio` (default: 1) - Play audio with quotes

### API Integration
- **Quotes API:** https://quotes.islamicquotes.deno.net/quote
- **Response handling:** Supports `post_content`, `post_type`, `post_date`

---

## 📥 Installation

### Fresh Install
1. Download `setup.exe` from releases
2. Run the installer
3. Follow the wizard
4. App installs to: `C:\Program Files\Salaty.First\`

### Upgrade from Previous Version
1. Download `setup.exe`
2. Run the installer
3. **Old version is automatically removed**
4. New version installs seamlessly
5. **Settings are preserved**

### Requirements
- Windows 10/11 (x64)
- .NET 5.0 Desktop Runtime ([download](https://dotnet.microsoft.com/download/dotnet/5.0))
  - OR use self-contained version (includes runtime)

---

## 🎨 Features Overview

### Prayer Times
- ✅ Offline calculation (no internet needed)
- ✅ 23,000+ cities worldwide
- ✅ 6 major calculation methods
- ✅ Asr method selection (Shafii/Hanafi)
- ✅ IP-based location detection
- ✅ Geocoding address search

### Notifications
- ✅ Before/after prayer reminders
- ✅ Customizable timing (5-30 min)
- ✅ Athan sound notifications
- ✅ Toast notifications with settings access

### Appearance
- ✅ Circular progress ring
- ✅ Customizable opacity
- ✅ Small/Medium/Large sizes
- ✅ Hijri date with ±1 day adjustment
- ✅ Summer time support

### Islamic Quotes (NEW!)
- ✅ Periodic inspirational quotes
- ✅ Audio notification
- ✅ RTL Arabic support
- ✅ Copy to clipboard
- ✅ Configurable interval (5 min - 2 hours)

### Multi-language Support
- 🇬🇧 English
- 🇸🇦 Arabic (العربية)
- 🇫🇷 French
- 🇹🇷 Turkish
- 🇵🇰 Urdu

---

## 🐛 Known Issues

### Windows Installer Upgrade
Some users may experience upgrade issues if an older version is installed.

**Workaround:**
```batch
# Uninstall old version
msiexec /x {PRODUCT-CODE} /quiet

# Install new version
msiexec /i setup.exe /qb
```

**Next Release:** Will include automatic cleanup.

---

## 📊 Statistics

| Metric | Value |
|--------|-------|
| **Total Downloads** | 1,000+ |
| **Supported Cities** | 23,000+ |
| **Countries** | 205 |
| **Calculation Methods** | 6 |
| **Languages** | 5 |
| **App Size** | ~30 MB |

---

## 🔗 Links

- **GitHub:** https://github.com/eng-salem/Salaty1st
- **Facebook:** https://www.facebook.com/Salaty.1st
- **Issues:** https://github.com/eng-salem/Salaty1st/issues
- **Wiki:** https://github.com/eng-salem/Salaty1st/wiki

---

## 📝 Full Changelog

### v1.0.4 (2026-03-09)
- FIXED: Setup upgrade now works - removes old version automatically
- NEW: Islamic Quotes tab in settings (enabled by default)
- NEW: Quotes display with scrollable multi-line support
- NEW: RTL support for Arabic quotes
- NEW: Copy quote to clipboard button
- NEW: quotes.mp3 audio playback with quotes
- NEW: Update dialog shows changelog
- NEW: Screenshot preview in update dialog
- FIXED: Quote API JSON parsing (post_content field)
- FIXED: Source extraction from hadith references
- IMPROVED: Settings organization with dedicated Quotes tab

### v1.0.3 (2026-03-08)
- Added automatic upgrade support
- Added Islamic Quotes tab in settings
- Enabled quotes by default
- Added quotes.mp3 audio playback
- Added scrollable quote display
- Added RTL support for Arabic quotes
- Added copy to clipboard button
- Fixed quote API JSON parsing
- Improved update dialog with changelog
- Added screenshot preview in updates

### v1.0.2 (2026-03-08)
- Added Asr method selection
- Improved first run detection
- Tabbed settings UI
- Automatic update checking

### v1.0.1 (2026-03-07)
- Fixed SQLite write permissions
- Database migration to AppData

### v1.0.0 (2026-03-06)
- Initial release

---

## 🙏 Credits

**Developer:** Salaty.First Team  
**Icons:** Custom designed  
**Athan Sounds:** Various reciters  
**Quotes API:** Islamic Quotes Project  

---

## 📄 License

This project is open-source and available under the MIT License.

---

**Thank you for using PrayerWidget! 🕌**

May this app help you stay connected with your prayers throughout the day.

*Built with ❤️ for the Muslim community*

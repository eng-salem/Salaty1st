# PrayerWidget Documentation

This folder contains all documentation files for the PrayerWidget (Salaty.First) project.

## 📁 File Organization

### Build & Release Documentation
| File | Description |
|------|-------------|
| `BUILD_SUMMARY_v1.0.10.md` | Build summary for v1.0.10 |
| `BUILD_SUMMARY_v1.0.9.md` | Build summary for v1.0.9 |
| `FINAL_BUILD_SUMMARY_v1.0.10.md` | Final build notes for v1.0.10 |
| `GITHUB_RELEASE_NOTES_v1.0.4.md` | Release notes v1.0.4 |
| `GITHUB_RELEASE_NOTES_v1.0.9.md` | Release notes v1.0.9 |
| `BUILD_STATUS_*.md` | Build status reports |

### Technical Documentation
| File | Description |
|------|-------------|
| `DATABASE_SCHEMA.md` | SQLite database schema |
| `DEVICE_COUNTER.md` | Turso device counter implementation |
| `ATHAN_SOUND_FIX.md` | Athan sound playback fix |
| `SETTINGS_TABLE_FIX.md` | Settings table loading fix |
| `TECHNICAL_CHANGES.md` | Technical changes log |
| `SIMULATION_TEST_FEATURE.md` | Simulation test feature docs |
| `SETUP_UPGRADE_FIX.md` | Setup upgrade fix |

### Setup & Installation
| File | Description |
|------|-------------|
| `SETUP_SUMMARY.md` | Setup project summary |
| `FIREWALL_URLS.md` | Firewall URLs for external APIs |
| `RUNTIME_FILES_COMPARISON.md` | Runtime files comparison |
| `DEBUG_VS_RELEASE_EXPLAINED.txt` | Debug vs Release configuration |

### Analytics & Testing
| File | Description |
|------|-------------|
| `QWEN.md` | Project context & Qwen memory |
| `SENDDATA_README.md` | Send data API docs |
| `SEND_TEST_DATA_INSTRUCTIONS.md` | Test data instructions |
| `QUICK_SEND_TEST_DATA.md` | Quick send test data guide |

---

# Scripts Folder

The `scripts` folder contains all batch files (.bat) and PowerShell scripts (.ps1) for building, installing, and testing.

## 🔧 Build Scripts
| File | Description |
|------|-------------|
| `build_setup.bat` | Build Windows Installer (MSI) |
| `build_v1.0.8.bat` | Legacy build script v1.0.8 |

## 📦 Installation Scripts
| File | Description |
|------|-------------|
| `install_v1.0.10.ps1` | PowerShell installer v1.0.10 |
| `force_install_v1.0.10.bat` | Force install v1.0.10 |
| `INSTALL_FIX.bat` | Installation fix script |
| `install_silent.bat` | Silent installation |
| `uninstall_old.bat` | Uninstall old version |
| `Copy_Runtime_Files.bat` | Copy .NET runtime files |

## 🧪 Testing Scripts
| File | Description |
|------|-------------|
| `send_test_data.ps1` | Send test data to Turso |
| `send_test_data.bat` | Send test data (batch) |
| `send_test_data_curl.bat` | Send test data (curl) |
| `send_test_data_dotnet.ps1` | Send test data (.NET) |

## 🔄 Update Scripts
| File | Description |
|------|-------------|
| `update.ps1` | Update script |
| `update2.ps1` | Update script v2 |
| `update_guids.ps1` | Update setup GUIDs |

---

## 📂 Project Structure

```
PrayerWidget/
├── PrayerWidget/           # Main WPF Application
├── Salaty.setup/           # Windows Installer Setup
├── docs/                   # 📄 Documentation (this folder)
├── scripts/                # 🔧 Build & Install Scripts
├── Data/                   # Database & data files
└── ...                     # Other project files
```

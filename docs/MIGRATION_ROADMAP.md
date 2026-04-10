# PrayerWidget Migration Roadmap: WPF to Avalonia UI (Ubuntu)

## Executive Summary

This roadmap outlines the migration of the PrayerWidget WPF application from .NET Framework 4.8 (Windows-only) to Avalonia UI with .NET 8 for cross-platform Ubuntu compatibility. The migration prioritizes minimal architectural disruption while ensuring cross-platform compatibility.

## Current Architecture Analysis

### Technology Stack
- **Framework**: .NET Framework 4.8 (VB.NET)
- **UI Framework**: WPF (Windows-only)
- **Database**: SQLite with Microsoft.Data.Sqlite 5.0.17
- **Key Dependencies**:
  - `System.Drawing.Common` 5.0.3 (❌ Windows-only on .NET 5+)
  - `Hardcodet.NotifyIcon.Wpf` 1.1.0 (❌ Windows-only)
  - `System.Management` 5.0.0 (❌ Windows-only)

### Critical Cross-Platform Issues

#### 1. System.Drawing Usage
- **Found**: `System.Drawing.Color.FromRgb()` calls in `MainWindow.xaml.vb`
- **Impact**: `System.Drawing.Common` is Windows-only from .NET 5+
- **Solution**: Replace with `Avalonia.Media.Color` or cross-platform alternatives

#### 2. Windows-Specific Dependencies
- **System.Management**: Used for hardware identification in `DeviceCounterService.vb`
- **Hardcodet.NotifyIcon.Wpf**: System tray functionality (Windows-only)
- **Registry Access**: Machine GUID retrieval from Windows Registry

#### 3. Win32 API Calls
- **Analysis**: No direct `DllImport` calls found
- **Window Positioning**: Handled through WPF window properties (cross-platform compatible)

## Migration Strategy

### Phase 1: Foundation Setup (Week 1-2)

#### 1.1 Create New Avalonia Project
```bash
# Target: .NET 8 for latest features and long-term support
dotnet new avalonia.app -n Salaty.Avalonia -f net8.0
dotnet new avalonia.mvvm -n Salaty.Avalonia.Core -f net8.0
```

#### 1.2 Project Structure
```
Salaty.Avalonia/
├── Salaty.Avalonia/           # UI Layer
│   ├── Views/                 # Avalonia Windows/Controls
│   ├── ViewModels/            # MVVM ViewModels
│   ├── Styles/                # Avalonia Styles
│   └── Assets/                # Images, Fonts, Icons
├── Salaty.Avalonia.Core/      # Business Logic
│   ├── Models/                # Domain Models
│   ├── Services/              # Business Services
│   └── Infrastructure/         # Data Access
└── Salaty.Avalonia.Tests/     # Unit Tests
```

### Phase 2: Cross-Platform Dependencies (Week 2-3)

#### 2.1 Replace System.Drawing
**Current Code**:
```vb
Private ReadOnly _brushBlue As New SolidColorBrush(WpfColor.FromRgb(79, 172, 254))
```

**Avalonia Equivalent**:
```vb
Private ReadOnly _brushBlue As New SolidColorBrush(Avalonia.Media.Color.FromRgb(79, 172, 254))
```

#### 2.2 Cross-Platform System Tray
**Replace**: `Hardcodet.NotifyIcon.Wpf`
**With**: `Avalonia.Controls.NotificationIcon` or custom implementation

#### 2.3 Hardware Identification (Cross-Platform)
**Current**: Windows Registry + MAC Address
**Options**:
- **Option A**: Use `System.Device.Location` (limited)
- **Option B**: Generate UUID based on available system info
- **Option C**: Store device ID in local file/database

**Recommended**: Option B - Cross-platform hardware fingerprinting

### Phase 3: UI Migration (Week 3-4)

#### 3.1 XAML Conversion
**WPF → Avalonia Mapping**:
- `Window` → `Window`
- `UserControl` → `UserControl`
- `Grid` → `Grid`
- `StackPanel` → `StackPanel`
- `TextBlock` → `TextBlock`
- `Button` → `Button`

#### 3.2 MVVM Implementation
- **Current**: Code-behind approach
- **Target**: MVVM pattern with `ReactiveUI` or `CommunityToolkit.Mvvm`

#### 3.3 Styling Migration
- **WPF Styles** → **Avalonia Styles**
- **Triggers** → **Selectors**
- **Templates** → **Templates** (similar syntax)

### Phase 4: Service Layer Migration (Week 4-5)

#### 4.1 Database Layer
**Current**: `Microsoft.Data.Sqlite` ✅ Already Cross-Platform
**Action**: No changes needed, but update to latest version

#### 4.2 HTTP Services
**Current**: `HttpClient` ✅ Already Cross-Platform
**Action**: No changes needed

#### 4.3 File System Operations
**Current**: `System.IO` ✅ Already Cross-Platform
**Consideration**: Path separators and user directories

### Phase 5: Testing & Deployment (Week 5-6)

#### 5.1 Unit Tests
- **Framework**: xUnit with `Avalonia.Testing`
- **Coverage**: Services, ViewModels, UI interactions

#### 5.2 Integration Tests
- **Database**: In-memory SQLite
- **HTTP**: Mocked responses

#### 5.3 Ubuntu Deployment
```bash
# Self-contained deployment
dotnet publish -c Release -r ubuntu.22.04-x64 --self-contained
```

## Architecture Decision Records (ADRs)

### ADR-001: UI Framework Selection
**Status**: Accepted
**Decision**: Use Avalonia UI over alternatives
**Rationale**: 
- XAML familiarity (minimal learning curve)
- Cross-platform support (Windows, Linux, macOS)
- Active community and commercial backing
- Similar performance to WPF

**Alternatives Considered**:
- **MAUI**: Too mobile-focused, limited desktop customization
- **Uno Platform**: Steeper learning curve, smaller community
- **QtSharp**: C++ dependencies, complex integration

### ADR-002: MVVM Framework
**Status**: Proposed
**Decision**: Use `CommunityToolkit.Mvvm`
**Rationale**:
- Lightweight, no external dependencies
- Microsoft-maintained
- Excellent performance
- Simple property and command implementation

**Alternatives Considered**:
- **ReactiveUI**: Powerful but steep learning curve
- **Caliburn.Micro**: Heavier, more complex setup
- **Prism**: Over-engineered for this application

### ADR-003: Cross-Platform Device Identification
**Status**: Proposed
**Decision**: Implement custom device fingerprinting
**Rationale**:
- No external dependencies
- Works across all platforms
- Privacy-focused (no sensitive data)

**Implementation**:
```vb
Private Function GenerateDeviceId() As String
    Dim sb As New StringBuilder()
    
    ' Cross-platform identifiers
    sb.Append(Environment.MachineName)
    sb.Append("|").Append(Environment.UserName)
    sb.Append("|").Append(Environment.OSVersion.ToString())
    
    ' Add available network interfaces
    For Each nic In NetworkInterface.GetAllNetworkInterfaces()
        If nic.OperationalStatus = OperationalStatus.Up Then
            sb.Append("|").Append(nic.GetPhysicalAddress().ToString())
        End If
    Next
    
    ' Hash for consistent ID
    Using sha256 = SHA256.Create()
        Return Convert.ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()))).Substring(0, 16)
    End Using
End Function
```

## Risk Assessment

### High Risk
1. **System.Drawing Dependencies**: Critical for UI rendering
2. **System Tray Functionality**: Core user experience feature
3. **Hardware Identification**: Analytics and licensing

### Medium Risk
1. **File System Paths**: Different conventions on Ubuntu
2. **Font Rendering**: Different font availability
3. **Audio Playback**: Cross-platform media handling

### Low Risk
1. **Database Operations**: Already cross-platform
2. **HTTP Services**: Already cross-platform
3. **Business Logic**: Platform-agnostic

## Migration Timeline

| Phase | Duration | Start | End | Deliverables |
|-------|----------|-------|-----|--------------|
| 1. Foundation | 2 weeks | Week 1 | Week 2 | New project structure, basic UI |
| 2. Dependencies | 1 week | Week 2 | Week 3 | Cross-platform libraries |
| 3. UI Migration | 2 weeks | Week 3 | Week 5 | Complete UI conversion |
| 4. Services | 1 week | Week 5 | Week 6 | Migrated service layer |
| 5. Testing | 1 week | Week 6 | Week 7 | Test coverage, deployment |
| **Total** | **7 weeks** | | | **Production-ready** |

## Success Criteria

### Functional
- [ ] All current features work on Ubuntu
- [ ] Prayer times calculation accurate
- [ ] Audio notifications functional
- [ ] Settings persistence works
- [ ] System tray integration works

### Non-Functional
- [ ] Startup time < 3 seconds
- [ ] Memory usage < 100MB
- [ ] CPU usage < 5% idle
- [ ] Responsive UI (60fps animations)

### Deployment
- [ ] Single executable deployment
- [ ] No external dependencies required
- [ ] Works on Ubuntu 20.04+ and 22.04+

## Next Steps

1. **Immediate**: Create new Avalonia project structure
2. **Week 1**: Set up CI/CD pipeline for cross-platform builds
3. **Week 2**: Begin UI migration with MainWindow
4. **Week 3**: Implement cross-platform device identification
5. **Week 4**: Complete service layer migration
6. **Week 5**: Comprehensive testing on Ubuntu
7. **Week 6**: Production deployment preparation

## Resources

### Documentation
- [Avalonia Documentation](https://avaloniaui.net/docs)
- [WPF to Avalonia Migration Guide](https://avaloniaui.net/docs/migration/wpf-to-avalonia)
- [.NET 8 Cross-Platform Development](https://learn.microsoft.com/en-us/dotnet/core/porting/)

### Tools
- **Avalonia Visual Studio Extension**
- **Avalonia XAML Designer**
- **dotnet-trace** for performance analysis
- **Avalonia.Testing** for UI tests

### Community
- [Avalonia Discord](https://discord.gg/avalonia)
- [GitHub Discussions](https://github.com/AvaloniaUI/Avalonia/discussions)
- [Stack Overflow](https://stackoverflow.com/questions/tagged/avalonia)

---

*This roadmap is a living document and will be updated as the migration progresses.*

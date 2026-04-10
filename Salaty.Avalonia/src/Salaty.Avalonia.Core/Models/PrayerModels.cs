// Migration Example: Core Models
// These models work cross-platform with minimal changes from WPF version

namespace Salaty.First.Core.Models;

/// <summary>
/// Daily prayer times model
/// MIGRATION: Same as WPF version - domain models are platform-agnostic
/// </summary>
public class DailyPrayerTimes
{
    public DateTime Date { get; set; }
    public DateTime Fajr { get; set; }
    public DateTime Sunrise { get; set; }
    public DateTime Dhuhr { get; set; }
    public DateTime Asr { get; set; }
    public DateTime Maghrib { get; set; }
    public DateTime Isha { get; set; }
    
    public string NextPrayerName { get; set; } = string.Empty;
    public DateTime NextPrayerTime { get; set; }
    public string PreviousPrayerName { get; set; } = string.Empty;
    public DateTime PreviousPrayerTime { get; set; }
    public string HijriDate { get; set; } = string.Empty;
    
    /// <summary>
    /// Calculates time remaining until next prayer
    /// </summary>
    public TimeSpan GetTimeUntilNextPrayer()
    {
        var now = DateTime.Now;
        if (NextPrayerTime > now)
        {
            return NextPrayerTime - now;
        }
        return TimeSpan.Zero;
    }
    
    /// <summary>
    /// Calculates percentage of time passed between prayers
    /// </summary>
    public double GetProgressPercentage()
    {
        var now = DateTime.Now;
        var totalDuration = (NextPrayerTime - PreviousPrayerTime).TotalSeconds;
        var elapsed = (now - PreviousPrayerTime).TotalSeconds;
        
        if (totalDuration <= 0) return 0;
        
        var percentage = (elapsed / totalDuration) * 100;
        return Math.Min(100, Math.Max(0, percentage));
    }
}

/// <summary>
/// Widget settings model
/// MIGRATION: Properties adapted for cross-platform window positioning
/// </summary>
public class WidgetSettings
{
    // Window position - cross-platform pixel coordinates
    public int WindowX { get; set; } = 100;
    public int WindowY { get; set; } = 100;
    public int WindowWidth { get; set; } = 400;
    public int WindowHeight { get; set; } = 300;
    
    // Appearance
    public double Opacity { get; set; } = 0.95;
    public bool ShowProgressRing { get; set; } = true;
    public bool AlwaysOnTop { get; set; } = true;
    public string Theme { get; set; } = "Dark";
    
    // Notifications
    public string AthanSound { get; set; } = "default";
    public bool ShowNotifications { get; set; } = true;
    public int ReminderMinutes { get; set; } = 15;
    
    // Location
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int CalculationMethod { get; set; } = 3; // Muslim World League
    
    // Language
    public string Language { get; set; } = "en";
    
    // MIGRATION: New property for system tray behavior (platform-specific)
    public SystemTrayBehavior SystemTrayBehavior { get; set; } = SystemTrayBehavior.Minimize;
}

/// <summary>
/// System tray behavior options
/// MIGRATION: Different behaviors for Windows vs Linux
/// </summary>
public enum SystemTrayBehavior
{
    Minimize,       // Minimize to taskbar (all platforms)
    Hide,           // Hide window but keep running (all platforms)
    NativeTray,     // Use native system tray (Windows, some Linux DEs)
    AppIndicator    // Use AppIndicator (GNOME, Ubuntu)
}

/// <summary>
/// Prayer reminder settings
/// </summary>
public class ReminderSettings
{
    public bool FajrEnabled { get; set; } = true;
    public bool DhuhrEnabled { get; set; } = true;
    public bool AsrEnabled { get; set; } = true;
    public bool MaghribEnabled { get; set; } = true;
    public bool IshaEnabled { get; set; } = true;
    
    public int MinutesBefore { get; set; } = 15;
    public bool PlaySound { get; set; } = true;
    public bool ShowToast { get; set; } = true;
}

/// <summary>
/// Calculation method for prayer times
/// </summary>
public enum CalculationMethod
{
    MuslimWorldLeague = 3,
    IslamicSocietyOfNorthAmerica = 2,
    MuslimWorldLeague2 = 1,
    UmmAlQuraUniversityMakkah = 4,
    EgyptianGeneralAuthorityOfSurvey = 5,
    InstituteOfGeophysicsUniversityOfTehran = 7,
    ShiaIthnaAshariJafari = 0,
    GulfRegion = 8,
    Kuwait = 9,
    Qatar = 10,
    MajlisUgamaIslamSingapore = 11,
    UnionDesOrganisationsIslamiquesDeFrance = 12,
    DiyanetİşleriBaşkanlığıTurkey = 13,
    SpiritualAdministrationOfMuslimsOfRussia = 14,
    MoonsightingCommittee = 15
}

/* MIGRATION NOTES:

Models are largely platform-agnostic and require minimal changes:

1. Value types (int, double, DateTime, string) - No changes needed
2. Enums - No changes needed
3. Logic methods - No changes needed

Main differences:
- Added SystemTrayBehavior enum for platform-specific behavior
- Window positioning uses PixelPoint (Avalonia) instead of Windows Forms Point
- Settings include platform-specific options

Database schema remains identical - SQLite works cross-platform.
*/

using System;
using System.Globalization;
using System.Threading.Tasks;
using SalatyMinimal.Models;
using System.Collections.Generic;
using System.Linq;
using PrayerCalculation;

namespace SalatyMinimal.Services
{
    public class PrayerService
    {
        private DailyPrayerTimes? _cachedPrayerTimes;
        private DateTime _lastCacheUpdate = DateTime.MinValue;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);
        private readonly RealPrayerApiService _apiService;
        private readonly SettingsService _settingsService;

        public PrayerService(SettingsService settingsService)
        {
            _settingsService = settingsService;
            _apiService = new RealPrayerApiService();
        }

        public async Task<DailyPrayerTimes> GetPrayerTimesAsync()
        {
            // Always clear cache when explicitly requested
            ClearCache();
            
            try
            {
                Console.WriteLine("Loading prayer times...");
                Console.WriteLine($"Current local time: {DateTime.Now:HH:mm:ss}");
                Console.WriteLine($"Current date: {DateTime.Today:yyyy-MM-dd}");
                
                var settings = _settingsService.Settings;
                Console.WriteLine($"[DEBUG] Current settings - Method: {settings.CalculationMethod}, Asr: {settings.AsrMethod}, Hijri: {settings.HijriAdjustment}");
                
                DailyPrayerTimes prayerTimes;
                
                if (!string.IsNullOrEmpty(settings.City) && !string.IsNullOrEmpty(settings.Country))
                {
                    prayerTimes = await _apiService.GetPrayerTimesByCityAsync(
                        settings.City, 
                        settings.Country, 
                        settings.CalculationMethod
                    );
                }
                else
                {
                    prayerTimes = await _apiService.GetPrayerTimesAsync(
                        settings.Latitude, 
                        settings.Longitude, 
                        settings.CalculationMethod
                    );
                }
                
                Console.WriteLine($"Loaded: Next prayer is {prayerTimes.NextPrayer} at {prayerTimes.NextPrayerTime:HH:mm:ss}");
                
                // Cache the result
                _cachedPrayerTimes = prayerTimes;
                _lastCacheUpdate = DateTime.Now;
                
                return prayerTimes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading prayer times: {ex.Message}");
                return GetFallbackPrayerTimes();
            }
        }

        private DailyPrayerTimes GetFallbackPrayerTimes()
        {
            Console.WriteLine("Using local prayer calculation...");
            
            var settings = _settingsService.Settings;
            Console.WriteLine($"[PRAYER CALC] Using local calculation for {settings.City}, {settings.Country}");
            Console.WriteLine($"[PRAYER CALC] Coordinates: {settings.Latitude:F6}, {settings.Longitude:F6}");
            Console.WriteLine($"[PRAYER CALC] Method: {settings.CalculationMethod}, Asr: {settings.AsrMethod}");
            
            // Use proper astronomical calculation
            var prayerTimes = PrayerCalculationEngine.CalculatePrayerTimes(
                settings.Latitude, 
                settings.Longitude, 
                settings.CalculationMethod, 
                settings.AsrMethod, 
                DateTime.Today
            );
            
            DetermineNextAndPreviousPrayer(prayerTimes);
            return prayerTimes;
        }

        private void DetermineNextAndPreviousPrayer(DailyPrayerTimes prayerTimes)
        {
            var now = DateTime.Now;
            var prayers = new List<(string name, DateTime time)>
            {
                ("Fajr", prayerTimes.Fajr),
                ("Sunrise", prayerTimes.Sunrise),
                ("Dhuhr", prayerTimes.Dhuhr),
                ("Asr", prayerTimes.Asr),
                ("Maghrib", prayerTimes.Maghrib),
                ("Isha", prayerTimes.Isha)
            };

            // Find next prayer
            DateTime? nextTime = null;
            string? nextName = null;
            
            foreach (var (name, time) in prayers)
            {
                if (time > now && (nextTime == null || time < nextTime))
                {
                    nextTime = time;
                    nextName = name;
                }
            }

            // If no prayer left today, next is tomorrow's Fajr
            if (nextTime == null)
            {
                nextTime = prayerTimes.Fajr.AddDays(1);
                nextName = "Fajr";
            }

            prayerTimes.NextPrayer = nextName;
            prayerTimes.NextPrayerTime = nextTime.Value;

            // Find previous prayer
            DateTime? prevTime = null;
            string? prevName = null;
            
            foreach (var (name, time) in prayers)
            {
                if (time <= now && (prevTime == null || time > prevTime))
                {
                    prevTime = time;
                    prevName = name;
                }
            }

            if (prevTime != null)
            {
                prayerTimes.PreviousPrayer = prevName;
                prayerTimes.PreviousPrayerTime = prevTime.Value;
            }
        }

        public TimeSpan GetTimeUntilNextPrayer(DailyPrayerTimes prayerTimes)
        {
            return prayerTimes.NextPrayerTime - DateTime.Now;
        }

        public double GetPrayerProgressPercentage(DailyPrayerTimes prayerTimes)
        {
            if (prayerTimes.NextPrayerTime == DateTime.MinValue || prayerTimes.PreviousPrayerTime == DateTime.MinValue)
                return 0;

            var totalDuration = prayerTimes.NextPrayerTime - prayerTimes.PreviousPrayerTime;
            var elapsed = DateTime.Now - prayerTimes.PreviousPrayerTime;
            
            // Progress should fill up as time passes from previous to next prayer
            // 0% = right after previous prayer (empty ring)
            // 100% = at prayer time (full ring)
            var progressPercent = (elapsed.TotalMilliseconds / totalDuration.TotalMilliseconds) * 100;
            
            return Math.Max(0, Math.Min(100, progressPercent));
        }

        public TimeSpan GetElapsedTime()
        {
            return DateTime.Now - DateTime.Today;
        }

        public string GetHijriDate()
        {
            try
            {
                // Use HijriCalendar with adjustment for local moon sighting
                var hijriCalendar = new HijriCalendar();
                var hijriDate = DateTime.Now;

                // Get Hijri date components
                var day = hijriCalendar.GetDayOfMonth(hijriDate);
                var month = hijriCalendar.GetMonth(hijriDate);
                var year = hijriCalendar.GetYear(hijriDate);

                // Apply adjustment from settings (-1, 0, or +1)
                // Egypt typically needs -1 adjustment for local moon sighting
                var adjustment = _settingsService.Settings.HijriAdjustment;
                day = day + adjustment;

                // Handle month boundaries
                if (day < 1)
                {
                    // Go to previous month
                    month = month - 1;
                    if (month < 1)
                    {
                        month = 12;
                        year = year - 1;
                    }
                    // Get days in previous month
                    var daysInPrevMonth = hijriCalendar.GetDaysInMonth(year, month);
                    day = daysInPrevMonth + day; // day is negative
                }
                else if (day > 29)
                {
                    // Approximate: go to next month (Hijri months are 29-30 days)
                    month = month + 1;
                    if (month > 12)
                    {
                        month = 1;
                        year = year + 1;
                    }
                    day = day - 29;
                }

                // Month names in English
                var monthNames = new[] 
                {
                    "Muharram", "Safar", "Rabi' al-Awwal", "Rabi' al-Thani",
                    "Jumada al-Awwal", "Jumada al-Thani", "Rajab", "Sha'ban",
                    "Ramadan", "Shawwal", "Dhu al-Qi'dah", "Dhu al-Hijjah"
                };

                var result = $"{day} {monthNames[month - 1]} {year}";
                
                Console.WriteLine($"Hijri calculation: {DateTime.Now:yyyy-MM-dd} -> {result}");
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating Hijri date: {ex.Message}");
                return ""; // Return empty string like VB.NET version
            }
        }

        public void ClearCache()
        {
            _cachedPrayerTimes = null;
            _lastCacheUpdate = DateTime.MinValue;
        }

        public async Task RefreshPrayerTimesAsync()
        {
            ClearCache();
            await GetPrayerTimesAsync();
        }
    }
}

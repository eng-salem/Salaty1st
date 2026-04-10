using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SalatyMinimal.Models;
using System.Collections.Generic;

namespace SalatyMinimal.Services
{
    public class RealPrayerApiService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.aladhan.com/v1";

        public RealPrayerApiService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<DailyPrayerTimes> GetPrayerTimesAsync(double latitude, double longitude, int method = 2)
        {
            try
            {
                Console.WriteLine($"[API] GetPrayerTimesAsync called with:");
                Console.WriteLine($"  Latitude: {latitude}");
                Console.WriteLine($"  Longitude: {longitude}");
                Console.WriteLine($"  Method: {method}");
                
                var today = DateTime.Today;
                // Add timezone parameter to get correct local times
                var timezone = TimeZoneInfo.Local.StandardName;
                var url = $"{BaseUrl}/timings/{today:dd-MM-yyyy}?latitude={latitude}&longitude={longitude}&method={method}&school=1&timezonestring=auto";
                
                Console.WriteLine($"Fetching prayer times from: {url}");
                Console.WriteLine($"Local timezone: {timezone}");
                Console.WriteLine($"System time: {DateTime.Now:HH:mm:ss}");
                Console.WriteLine($"UTC time: {DateTime.UtcNow:HH:mm:ss}");
                Console.WriteLine($"Timezone offset: {TimeZoneInfo.Local.GetUtcOffset(DateTime.Now)}");
                
                var response = await _httpClient.GetStringAsync(url);
                var apiResponse = JsonSerializer.Deserialize<AladhanResponse>(response);
                
                if (apiResponse?.Data != null)
                {
                    var timings = apiResponse.Data.Timings;
                    var meta = apiResponse.Data.Meta;
                    
                    Console.WriteLine($"API returned timezone: {meta?.Timezone}");
                    Console.WriteLine($"Raw prayer times from API:");
                    Console.WriteLine($"  Fajr: {timings.Fajr}");
                    Console.WriteLine($"  Dhuhr: {timings.Dhuhr}");
                    Console.WriteLine($"  Asr: {timings.Asr}");
                    Console.WriteLine($"  Maghrib: {timings.Maghrib}");
                    Console.WriteLine($"  Isha: {timings.Isha}");
                    
                    var prayerTimes = new DailyPrayerTimes
                    {
                        Date = today,
                        Fajr = ParseTimeWithTimezone(timings.Fajr, today, meta?.Timezone),
                        Sunrise = ParseTimeWithTimezone(timings.Sunrise, today, meta?.Timezone),
                        Dhuhr = ParseTimeWithTimezone(timings.Dhuhr, today, meta?.Timezone),
                        Asr = ParseTimeWithTimezone(timings.Asr, today, meta?.Timezone),
                        Maghrib = ParseTimeWithTimezone(timings.Maghrib, today, meta?.Timezone),
                        Isha = ParseTimeWithTimezone(timings.Isha, today, meta?.Timezone)
                    };
                    
                    Console.WriteLine($"Parsed prayer times with timezone:");
                    Console.WriteLine($"  Fajr: {prayerTimes.Fajr:HH:mm:ss}");
                    Console.WriteLine($"  Dhuhr: {prayerTimes.Dhuhr:HH:mm:ss}");
                    Console.WriteLine($"  Asr: {prayerTimes.Asr:HH:mm:ss}");
                    Console.WriteLine($"  Maghrib: {prayerTimes.Maghrib:HH:mm:ss}");
                    Console.WriteLine($"  Isha: {prayerTimes.Isha:HH:mm:ss}");
                    
                    Console.WriteLine($"Parsed prayer times with timezone:");
                    Console.WriteLine($"  Fajr: {prayerTimes.Fajr:HH:mm:ss}");
                    Console.WriteLine($"  Dhuhr: {prayerTimes.Dhuhr:HH:mm:ss}");
                    Console.WriteLine($"  Asr: {prayerTimes.Asr:HH:mm:ss}");
                    Console.WriteLine($"  Maghrib: {prayerTimes.Maghrib:HH:mm:ss}");
                    Console.WriteLine($"  Isha: {prayerTimes.Isha:HH:mm:ss}");
                    
                    DetermineNextAndPreviousPrayer(prayerTimes);
                    return prayerTimes;
                }
                
                throw new Exception("Invalid API response");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error: {ex.Message}");
                // Fallback to mock data
                return await GetFallbackPrayerTimesAsync();
            }
        }

        public async Task<DailyPrayerTimes> GetPrayerTimesByCityAsync(string city, string country, int method = 2)
        {
            try
            {
                Console.WriteLine($"[API] GetPrayerTimesByCityAsync called with:");
                Console.WriteLine($"  City: {city}");
                Console.WriteLine($"  Country: {country}");
                Console.WriteLine($"  Method: {method}");
                
                var today = DateTime.Today;
                var url = $"{BaseUrl}/timingsByCity?city={city}&country={country}&method={method}&school=1&timezonestring=auto";
                
                Console.WriteLine($"Fetching prayer times for {city}, {country}");
                
                var response = await _httpClient.GetStringAsync(url);
                var apiResponse = JsonSerializer.Deserialize<AladhanResponse>(response);
                
                if (apiResponse?.Data != null)
                {
                    var timings = apiResponse.Data.Timings;
                    var meta = apiResponse.Data.Meta;
                    
                    Console.WriteLine($"API returned timezone: {meta?.Timezone}");
                    Console.WriteLine($"Raw prayer times from API:");
                    Console.WriteLine($"  Fajr: {timings.Fajr}");
                    Console.WriteLine($"  Dhuhr: {timings.Dhuhr}");
                    Console.WriteLine($"  Asr: {timings.Asr}");
                    Console.WriteLine($"  Maghrib: {timings.Maghrib}");
                    Console.WriteLine($"  Isha: {timings.Isha}");
                    
                    var prayerTimes = new DailyPrayerTimes
                    {
                        Date = DateTime.Today,
                        Fajr = ParseTimeWithTimezone(timings.Fajr, DateTime.Today, meta?.Timezone),
                        Sunrise = ParseTimeWithTimezone(timings.Sunrise, DateTime.Today, meta?.Timezone),
                        Dhuhr = ParseTimeWithTimezone(timings.Dhuhr, DateTime.Today, meta?.Timezone),
                        Asr = ParseTimeWithTimezone(timings.Asr, DateTime.Today, meta?.Timezone),
                        Maghrib = ParseTimeWithTimezone(timings.Maghrib, DateTime.Today, meta?.Timezone),
                        Isha = ParseTimeWithTimezone(timings.Isha, DateTime.Today, meta?.Timezone)
                    };
                    
                    Console.WriteLine($"Parsed prayer times with timezone:");
                    Console.WriteLine($"  Fajr: {prayerTimes.Fajr:HH:mm:ss}");
                    Console.WriteLine($"  Dhuhr: {prayerTimes.Dhuhr:HH:mm:ss}");
                    Console.WriteLine($"  Asr: {prayerTimes.Asr:HH:mm:ss}");
                    Console.WriteLine($"  Maghrib: {prayerTimes.Maghrib:HH:mm:ss}");
                    Console.WriteLine($"  Isha: {prayerTimes.Isha:HH:mm:ss}");
                    
                    DetermineNextAndPreviousPrayer(prayerTimes);
                    return prayerTimes;
                }
                
                throw new Exception("Invalid API response");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error: {ex.Message}");
                return await GetFallbackPrayerTimesAsync();
            }
        }

        private async Task<DailyPrayerTimes> GetFallbackPrayerTimesAsync()
        {
            Console.WriteLine("Using fallback prayer times...");
            await Task.Delay(100);
            
            var today = DateTime.Today;
            var prayerTimes = new DailyPrayerTimes
            {
                Date = today,
                Fajr = today.AddHours(5).AddMinutes(30),
                Sunrise = today.AddHours(6).AddMinutes(45),
                Dhuhr = today.AddHours(12).AddMinutes(30),
                Asr = today.AddHours(15).AddMinutes(45),
                Maghrib = today.AddHours(18).AddMinutes(15),
                Isha = today.AddHours(19).AddMinutes(30)
            };
            
            DetermineNextAndPreviousPrayer(prayerTimes);
            return prayerTimes;
        }

        private DateTime ParseTimeWithTimezone(string timeString, DateTime date, string apiTimezone)
        {
            if (string.IsNullOrEmpty(timeString))
                return date;
                
            var parts = timeString.Split(':');
            if (parts.Length >= 2)
            {
                var hour = int.Parse(parts[0]);
                var minute = int.Parse(parts[1]);
                
                // Create a DateTime with the API timezone
                var result = date.AddHours(hour).AddMinutes(minute);
                
                // If API timezone is different from local, adjust
                if (!string.IsNullOrEmpty(apiTimezone))
                {
                    try
                    {
                        // Try to find the API timezone
                        var apiTz = TimeZoneInfo.FindSystemTimeZoneById(apiTimezone);
                        var localTz = TimeZoneInfo.Local;
                        
                        // Convert from API timezone to local timezone
                        var apiTime = DateTime.SpecifyKind(result, DateTimeKind.Unspecified);
                        result = TimeZoneInfo.ConvertTime(apiTime, apiTz, localTz);
                        
                        Console.WriteLine($"Timezone conversion: {timeString} in {apiTimezone} -> {result:HH:mm:ss} in {localTz.StandardName}");
                    }
                    catch (TimeZoneNotFoundException)
                    {
                        // If timezone not found, use the time as-is
                        Console.WriteLine($"Timezone {apiTimezone} not found, using time as-is: {result:HH:mm:ss}");
                    }
                }
                
                return result;
            }
            
            return date;
        }

        private DateTime ParseTime(string timeString, DateTime date)
        {
            return ParseTimeWithTimezone(timeString, date, null);
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

            DateTime? nextTime = null;
            string? nextName = null;
            
            Console.WriteLine($"[PrayerLogic] Current time: {now:HH:mm:ss}");
            Console.WriteLine($"[PrayerLogic] Available prayers:");
            foreach (var (name, time) in prayers)
            {
                Console.WriteLine($"  {name}: {time:HH:mm:ss}");
            }
            
            foreach (var (name, time) in prayers)
            {
                if (time > now && (nextTime == null || time < nextTime))
                {
                    nextTime = time;
                    nextName = name;
                }
            }
            
            if (nextTime == null)
            {
                nextTime = prayerTimes.Fajr.AddDays(1);
                nextName = "Fajr";
            }
            
            Console.WriteLine($"[PrayerLogic] Next prayer: {nextName} at {nextTime:HH:mm:ss}");
            
            prayerTimes.NextPrayer = nextName;
            prayerTimes.NextPrayerTime = nextTime.Value;

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
            
            Console.WriteLine($"[PrayerLogic] Previous prayer: {prevName} at {prevTime:HH:mm:ss}");
        }
    }

    // API Response Models
    public class AladhanResponse
    {
        public AladhanData Data { get; set; }
    }

    public class AladhanData
    {
        public AladhanTimings Timings { get; set; }
        public AladhanMeta Meta { get; set; }
    }

    public class AladhanTimings
    {
        public string Fajr { get; set; }
        public string Sunrise { get; set; }
        public string Dhuhr { get; set; }
        public string Asr { get; set; }
        public string Maghrib { get; set; }
        public string Isha { get; set; }
    }

    public class AladhanMeta
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Timezone { get; set; }
        public string Method { get; set; }
    }
}

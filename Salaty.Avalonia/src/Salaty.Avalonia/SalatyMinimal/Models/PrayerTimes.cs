using System;
using System.Collections.Generic;

namespace SalatyMinimal.Models
{
    public class DailyPrayerTimes
    {
        public DateTime Fajr { get; set; }
        public DateTime Sunrise { get; set; }
        public DateTime Dhuhr { get; set; }
        public DateTime Asr { get; set; }
        public DateTime Maghrib { get; set; }
        public DateTime Isha { get; set; }
        public string NextPrayer { get; set; } = "";
        public DateTime NextPrayerTime { get; set; }
        public string PreviousPrayer { get; set; } = "";
        public DateTime PreviousPrayerTime { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;
    }

    public class PrayerTimeInfo
    {
        public string Name { get; set; } = "";
        public DateTime Time { get; set; }
        public bool IsNext { get; set; }
        public bool IsPrevious { get; set; }
    }

    public class CityInfo
    {
        public string Name { get; set; } = "";
        public string Country { get; set; } = "";
        public string CountryCode { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Timezone { get; set; }
    }
}

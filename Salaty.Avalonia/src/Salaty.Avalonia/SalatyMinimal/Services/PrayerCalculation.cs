using System;
using System.Collections.Generic;
using SalatyMinimal.Models;

namespace SalatyMinimal.Services
{
    public class PrayerCalculationEngine
    {
        // Astronomical constants
        private const double Deg2Rad = Math.PI / 180.0;
        private const double Rad2Deg = 180.0 / Math.PI;
        
        public static DailyPrayerTimes CalculatePrayerTimes(double latitude, double longitude, int calculationMethod, int asrMethod, DateTime date)
        {
            Console.WriteLine($"[PRAYER CALC] Calculating for Lat: {latitude}, Lon: {longitude}, Method: {calculationMethod}, Asr: {asrMethod}");
            
            var prayerTimes = new DailyPrayerTimes
            {
                Date = date
            };
            
            // Calculate astronomical values
            var julianDate = GetJulianDate(date);
            var declination = GetSolarDeclination(julianDate);
            var equationOfTime = GetEquationOfTime(julianDate);
            
            // Calculate prayer times
            prayerTimes.Fajr = CalculateFajr(date, latitude, longitude, declination, equationOfTime, calculationMethod);
            prayerTimes.Sunrise = CalculateSunrise(date, latitude, longitude, declination, equationOfTime);
            prayerTimes.Dhuhr = CalculateDhuhr(date, latitude, longitude, declination, equationOfTime, calculationMethod);
            prayerTimes.Asr = CalculateAsr(date, latitude, longitude, declination, equationOfTime, calculationMethod, asrMethod);
            prayerTimes.Maghrib = CalculateMaghrib(date, latitude, longitude, declination, equationOfTime, calculationMethod);
            prayerTimes.Isha = CalculateIsha(date, latitude, longitude, declination, equationOfTime, calculationMethod);
            
            Console.WriteLine($"[PRAYER CALC] Calculated times:");
            Console.WriteLine($"  Fajr: {prayerTimes.Fajr:HH:mm:ss}");
            Console.WriteLine($"  Sunrise: {prayerTimes.Sunrise:HH:mm:ss}");
            Console.WriteLine($"  Dhuhr: {prayerTimes.Dhuhr:HH:mm:ss}");
            Console.WriteLine($"  Asr: {prayerTimes.Asr:HH:mm:ss}");
            Console.WriteLine($"  Maghrib: {prayerTimes.Maghrib:HH:mm:ss}");
            Console.WriteLine($"  Isha: {prayerTimes.Isha:HH:mm:ss}");
            
            return prayerTimes;
        }
        
        private static DateTime CalculateFajr(DateTime date, double latitude, double longitude, double declination, double equationOfTime, int method)
        {
            // Fajr angle calculation varies by method
            double fajrAngle = method switch
            {
                0 => 19.5,  // Jafari
                1 => 18.0,  // MWL
                2 => 18.0,  // ISNA
                3 => 18.0,  // Karachi
                4 => 18.5,  // Umm al-Qura
                5 => 19.5,  // Egyptian
                _ => 18.0   // Default MWL
            };
            
            return CalculateTimeFromAngle(date, latitude, longitude, fajrAngle, declination, equationOfTime, true);
        }
        
        private static DateTime CalculateDhuhr(DateTime date, double latitude, double longitude, double declination, double equationOfTime, int method)
        {
            // Dhuhr is when sun is at highest point
            return CalculateSolarNoon(date, latitude, longitude, equationOfTime);
        }
        
        private static DateTime CalculateAsr(DateTime date, double latitude, double longitude, double declination, double equationOfTime, int method, int asrMethod)
        {
            // Asr calculation varies by juristic method
            double asrAngle;
            
            if (asrMethod == 1) // Hanafi
            {
                // Hanafi: shadow length = object height * 2
                asrAngle = GetShadowAngle(date, latitude, declination) * 2;
            }
            else // Shafii (default)
            {
                // Shafii: shadow length = object height
                asrAngle = GetShadowAngle(date, latitude, declination);
            }
            
            return CalculateTimeFromAngle(date, latitude, longitude, asrAngle, declination, equationOfTime, false);
        }
        
        private static DateTime CalculateMaghrib(DateTime date, double latitude, double longitude, double declination, double equationOfTime, int method)
        {
            // Maghrib: sun angle = 0.833°
            return CalculateTimeFromAngle(date, latitude, longitude, 0.833, declination, equationOfTime, false);
        }
        
        private static DateTime CalculateIsha(DateTime date, double latitude, double longitude, double declination, double equationOfTime, int method)
        {
            // Isha angle calculation varies by method
            double ishaAngle = method switch
            {
                0 => 17.5,  // Jafari
                1 => 17.0,  // MWL
                2 => 17.0,  // ISNA
                3 => 17.0,  // Karachi
                4 => 17.5,  // Umm al-Qura
                5 => 17.5,  // Egyptian
                _ => 17.0   // Default MWL
            };
            
            return CalculateTimeFromAngle(date, latitude, longitude, ishaAngle, declination, equationOfTime, false);
        }
        
        private static DateTime CalculateSunrise(DateTime date, double latitude, double longitude, double declination, double equationOfTime)
        {
            // Sunrise: sun angle = 0.833°
            return CalculateTimeFromAngle(date, latitude, longitude, 0.833, declination, equationOfTime, true);
        }
        
        private static DateTime CalculateTimeFromAngle(DateTime date, double latitude, double longitude, double sunAngle, double declination, double equationOfTime, bool isSunrise)
        {
            // Calculate hour angle
            var hourAngle = Math.Acos((Math.Sin(sunAngle * Deg2Rad) - Math.Sin(latitude * Deg2Rad) * Math.Sin(declination * Deg2Rad)) / 
                         (Math.Cos(latitude * Deg2Rad) * Math.Cos(declination * Deg2Rad)));
            
            var hourAngleDeg = hourAngle * Rad2Deg;
            var transitTime = 12 - hourAngleDeg / 15;
            
            if (isSunrise)
                transitTime -= hourAngleDeg / 15;
            
            // Convert to UTC
            var utcTime = date.Date.AddHours(transitTime - longitude / 15);
            
            // Apply equation of time correction
            var correctedTime = utcTime.AddHours(equationOfTime / 60);
            
            // Convert to local time
            var localTime = correctedTime.AddHours(GetTimezoneOffset(latitude, longitude));
            
            return localTime;
        }
        
        private static DateTime CalculateSolarNoon(DateTime date, double latitude, double longitude, double equationOfTime)
        {
            // Solar noon occurs at transit time
            var julianDate = GetJulianDate(date);
            var declination = GetSolarDeclination(julianDate);
            
            // Calculate transit time (when sun crosses meridian)
            var hourAngle = Math.Acos(-Math.Tan(latitude * Deg2Rad) * Math.Tan(declination * Deg2Rad));
            var hourAngleDeg = hourAngle * Rad2Deg;
            var transitTime = 12 - hourAngleDeg / 15;
            
            // Convert to UTC
            var utcTime = date.Date.AddHours(transitTime - longitude / 15);
            
            // Apply equation of time correction
            var correctedTime = utcTime.AddHours(equationOfTime / 60);
            
            // Convert to local time
            var localTime = correctedTime.AddHours(GetTimezoneOffset(latitude, longitude));
            
            return localTime;
        }
        
        private static double GetShadowAngle(DateTime date, double latitude, double declination)
        {
            // Calculate shadow angle for Asr
            var julianDate = GetJulianDate(date);
            var hourAngle = (julianDate - 2451545.0) / 365.25 * 360;
            var sunAltitude = Math.Asin(Math.Sin(declination * Deg2Rad) * Math.Sin(latitude * Deg2Rad) + 
                                      Math.Cos(declination * Deg2Rad) * Math.Cos(latitude * Deg2Rad) * Math.Cos(hourAngle * Deg2Rad));
            
            // Shadow angle calculation
            var shadowAngle = Math.Atan(1 / (Math.Tan(sunAltitude * Deg2Rad) + 0.0167)) * Rad2Deg;
            
            return shadowAngle;
        }
        
        private static double GetJulianDate(DateTime date)
        {
            var a = (14 - date.Month) / 12;
            var y = date.Year + 4800 - a;
            var m = date.Month + 12 * a - 3;
            
            return date.Day + (153 * m + 2) / 5 + 365 * y + (y / 4);
        }
        
        private static double GetSolarDeclination(double julianDate)
        {
            // Calculate solar declination
            var n = julianDate - 2451545.0;
            var L = (280.460 + 0.9856474 * n) % 360;
            var g = 357.528 + 0.9856003 * n;
            
            var lambda = L + 1.915 * Math.Sin(g * Deg2Rad);
            var delta = 23.45 * Math.Sin(lambda * Deg2Rad);
            
            return Math.Asin(delta * Deg2Rad) * Rad2Deg;
        }
        
        private static double GetEquationOfTime(double julianDate)
        {
            // Calculate equation of time
            var n = julianDate - 2451545.0;
            var J = (357.529 + 0.9856003 * n) % 360;
            var M = 1.916 * Math.Sin(J * Deg2Rad) - 0.013 * Math.Sin(2 * J * Deg2Rad);
            
            var C = M + 102 * Math.Sin(2 * J * Deg2Rad) + 512 * Math.Sin(J * Deg2Rad);
            var equationOfTime = (C - J) / 2;
            
            return equationOfTime;
        }
        
        private static double GetTimezoneOffset(double latitude, double longitude)
        {
            // Simplified timezone calculation - in real implementation, 
            // this would use proper timezone databases
            // For Cairo: should return 2 (UTC+2)
            
            if (Math.Abs(latitude) < 30) // Near equator
                return Math.Round(longitude / 15);
            
            // For Egypt/Cairo region
            if (longitude >= 30 && longitude <= 45)
                return 2; // UTC+2 (Egypt timezone)
            
            return 0; // Default UTC
        }
    }
}

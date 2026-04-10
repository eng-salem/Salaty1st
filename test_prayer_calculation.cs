using System;
using SalatyMinimal.Models;

namespace TestPrayerCalculation
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Testing PrayerCalculation class...");
            
            // Test the calculation
            var result = PrayerCalculation.CalculatePrayerTimes(30.0444, 31.2357, 5, 0, DateTime.Today);
            
            Console.WriteLine($"Calculation successful: {result != null}");
            Console.WriteLine($"Fajr: {result.Fajr:HH:mm:ss}");
            Console.WriteLine($"Dhuhr: {result.Dhuhr:HH:mm:ss}");
        }
    }
}

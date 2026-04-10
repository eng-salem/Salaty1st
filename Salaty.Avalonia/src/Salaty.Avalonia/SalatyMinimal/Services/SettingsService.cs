using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SalatyMinimal.Models;

namespace SalatyMinimal.Services
{
    public class SettingsService
    {
        private readonly string _settingsPath;
        private WidgetSettings _settings;

        public SettingsService()
        {
            _settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Salaty",
                "settings.json"
            );
            
            LoadSettings();
        }

        public WidgetSettings Settings => _settings;

        public async Task SaveSettingsAsync()
        {
            try
            {
                var directory = Path.GetDirectoryName(_settingsPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_settingsPath, json);
                
                Console.WriteLine($"Settings saved to: {_settingsPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    _settings = JsonSerializer.Deserialize<WidgetSettings>(json) ?? GetDefaultSettings();
                }
                else
                {
                    _settings = GetDefaultSettings();
                    _ = SaveSettingsAsync();
                }
                
                Console.WriteLine($"Settings loaded. City: {_settings.City}, Method: {_settings.CalculationMethod}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
                _settings = GetDefaultSettings();
            }
        }

        private WidgetSettings GetDefaultSettings()
        {
            return new WidgetSettings
            {
                City = "Mecca",
                Country = "Saudi Arabia",
                Latitude = 21.3891,
                Longitude = 39.8579,
                Timezone = "Asia/Riyadh",
                CalculationMethod = 1, // Muslim World League (MWL)
                AsrMethod = 0, // Shafi'i
                HigherLatitudesMethod = 1, // Middle of the night
                EnableAthan = true,
                Volume = 80,
                AudioPath = "",
                AlwaysOnTop = true,
                ShowProgressBar = true,
                ShowHijriDate = true,
                WindowX = 100,
                WindowY = 100,
                Language = "en"
            };
        }

        public void UpdateSettings(WidgetSettings newSettings)
        {
            _settings = newSettings;
            _ = SaveSettingsAsync();
        }

        public void UpdateLocation(string city, string country, double latitude, double longitude, string timezone)
        {
            _settings.City = city;
            _settings.Country = country;
            _settings.Latitude = latitude;
            _settings.Longitude = longitude;
            _settings.Timezone = timezone;
            _ = SaveSettingsAsync();
        }

        public void UpdateCalculationMethod(int method, int asrMethod, int higherLatitudesMethod)
        {
            _settings.CalculationMethod = method;
            _settings.AsrMethod = asrMethod;
            _settings.HigherLatitudesMethod = higherLatitudesMethod;
            _ = SaveSettingsAsync();
        }

        public void UpdateHijriAdjustment(int adjustment)
        {
            _settings.HijriAdjustment = adjustment;
            _ = SaveSettingsAsync();
        }

        public void UpdateAudioSettings(bool enableAthan, int volume, string audioPath)
        {
            _settings.EnableAthan = enableAthan;
            _settings.Volume = volume;
            _settings.AudioPath = audioPath;
            _ = SaveSettingsAsync();
        }

        public void UpdateDisplaySettings(bool alwaysOnTop, bool showProgressBar, bool showHijriDate, string language)
        {
            _settings.AlwaysOnTop = alwaysOnTop;
            _settings.ShowProgressBar = showProgressBar;
            _settings.ShowHijriDate = showHijriDate;
            _settings.Language = language;
            _ = SaveSettingsAsync();
        }

        public void UpdateWindowPosition(int x, int y)
        {
            _settings.WindowX = x;
            _settings.WindowY = y;
            _ = SaveSettingsAsync();
        }
    }

    public class WidgetSettings
    {
        public string City { get; set; } = "Mecca";
        public string Country { get; set; } = "Saudi Arabia";
        public double Latitude { get; set; } = 21.3891;
        public double Longitude { get; set; } = 39.8579;
        public string Timezone { get; set; } = "Asia/Riyadh";
        public int CalculationMethod { get; set; } = 2;
        public int AsrMethod { get; set; } = 0;
        public int HigherLatitudesMethod { get; set; } = 1;
        public bool EnableAthan { get; set; } = true;
        public int Volume { get; set; } = 80;
        public string AudioPath { get; set; } = "";
        public bool AlwaysOnTop { get; set; } = true;
        public bool ShowProgressBar { get; set; } = true;
        public bool ShowHijriDate { get; set; } = true;
        public int WindowX { get; set; } = 100;
        public int WindowY { get; set; } = 100;
        public string Language { get; set; } = "en";
        public int HijriAdjustment { get; set; } = 0; // For local moon sighting (-1, 0, +1)
    }
}

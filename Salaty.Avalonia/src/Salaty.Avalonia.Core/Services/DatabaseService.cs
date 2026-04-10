// Migration Example: Database Service
// Shows how SQLite works cross-platform without changes

using Microsoft.Data.Sqlite;
using Salaty.First.Core.Models;
using System.Text.Json;

namespace Salaty.First.Core.Services;

/// <summary>
/// Database service for SQLite operations
/// MIGRATION: SQLite works identically on Windows and Linux!
/// </summary>
public class DatabaseService : IDisposable
{
    private readonly string _connectionString;
    private SqliteConnection? _connection;
    private bool _disposed;

    public DatabaseService(string dbPath)
    {
        // MIGRATION: Connection string format is identical across platforms
        _connectionString = $"Data Source={dbPath};Mode=ReadOnly;";
    }

    public void Initialize()
    {
        _connection = new SqliteConnection(_connectionString);
        _connection.Open();
    }

    /// <summary>
    /// Gets prayer times for a city from database
    /// MIGRATION: SQL queries work identically on all platforms
    /// </summary>
    public async Task<CityPrayerTimes?> GetPrayerTimesAsync(string cityName)
    {
        if (_connection == null) throw new InvalidOperationException("Database not initialized");

        const string sql = @"
            SELECT city_name, country, latitude, longitude, 
                   fajr_angle, isha_angle, timezone,
                   fajr_time, duhr_time, asr_time, maghrib_time, isha_time
            FROM prayer_times 
            WHERE city_name = @cityName
            LIMIT 1";

        using var command = _connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@cityName", cityName);

        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new CityPrayerTimes
            {
                CityName = reader.GetString(0),
                Country = reader.GetString(1),
                Latitude = reader.GetDouble(2),
                Longitude = reader.GetDouble(3),
                FajrAngle = reader.GetDouble(4),
                IshaAngle = reader.GetDouble(5),
                Timezone = reader.GetString(6),
                FajrTime = reader.GetString(7),
                DuhrTime = reader.GetString(8),
                AsrTime = reader.GetString(9),
                MaghribTime = reader.GetString(10),
                IshaTime = reader.GetString(11)
            };
        }

        return null;
    }

    /// <summary>
    /// Searches for cities by name
    /// </summary>
    public async Task<List<string>> SearchCitiesAsync(string searchTerm)
    {
        if (_connection == null) throw new InvalidOperationException("Database not initialized");

        const string sql = @"
            SELECT city_name || ', ' || country as display_name
            FROM prayer_times 
            WHERE city_name LIKE @searchTerm
            ORDER BY city_name
            LIMIT 20";

        using var command = _connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@searchTerm", $"%{searchTerm}%");

        var results = new List<string>();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(reader.GetString(0));
        }

        return results;
    }

    /// <summary>
    /// Gets all cities for a country
    /// </summary>
    public async Task<List<CityInfo>> GetCitiesByCountryAsync(string country)
    {
        if (_connection == null) throw new InvalidOperationException("Database not initialized");

        const string sql = @"
            SELECT city_name, latitude, longitude
            FROM prayer_times 
            WHERE country = @country
            ORDER BY city_name";

        using var command = _connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@country", country);

        var results = new List<CityInfo>();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(new CityInfo
            {
                Name = reader.GetString(0),
                Latitude = reader.GetDouble(1),
                Longitude = reader.GetDouble(2)
            });
        }

        return results;
    }

    /// <summary>
    /// Gets calculation parameters for a city
    /// </summary>
    public CalculationParameters GetCalculationParameters(string cityName)
    {
        if (_connection == null) throw new InvalidOperationException("Database not initialized");

        const string sql = @"
            SELECT fajr_angle, isha_angle, calculation_method
            FROM prayer_times 
            WHERE city_name = @cityName
            LIMIT 1";

        using var command = _connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@cityName", cityName);

        using var reader = command.ExecuteReader();

        if (reader.Read())
        {
            return new CalculationParameters
            {
                FajrAngle = reader.GetDouble(0),
                IshaAngle = reader.GetDouble(1),
                Method = (CalculationMethod)reader.GetInt32(2)
            };
        }

        // Default parameters
        return new CalculationParameters
        {
            FajrAngle = 18.0,
            IshaAngle = 17.0,
            Method = CalculationMethod.MuslimWorldLeague
        };
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _connection?.Close();
            _connection?.Dispose();
            _disposed = true;
        }
    }
}

// MIGRATION: Data models - identical to WPF version
// These work the same on all platforms

public class CityPrayerTimes
{
    public string CityName { get; set; } = "";
    public string Country { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double FajrAngle { get; set; }
    public double IshaAngle { get; set; }
    public string Timezone { get; set; } = "";
    public string FajrTime { get; set; } = "";
    public string DuhrTime { get; set; } = "";
    public string AsrTime { get; set; } = "";
    public string MaghribTime { get; set; } = "";
    public string IshaTime { get; set; } = "";
}

public class CityInfo
{
    public string Name { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class CalculationParameters
{
    public double FajrAngle { get; set; }
    public double IshaAngle { get; set; }
    public CalculationMethod Method { get; set; }
}

/* MIGRATION NOTES - Database Layer:

ZERO CHANGES NEEDED FOR:
- Connection strings
- SQL queries
- Transaction handling
- Data types
- Parameter binding
- Reader operations

SQLite is truly cross-platform. The same code works on:
- Windows (x86, x64, ARM64)
- Linux (x64, ARM64)
- macOS (x64, Apple Silicon)

MIGRATION STEPS:
1. Copy existing database code (no changes needed)
2. Update NuGet package to latest version (8.0.x)
3. Ensure database file is in correct location for platform

FILE PATHS:
- Windows: %LOCALAPPDATA%/Salaty/salaty.sqlite
- Linux: ~/.local/share/Salaty/salaty.sqlite
- macOS: ~/Library/Application Support/Salaty/salaty.sqlite

Avalonia provides Environment.SpecialFolder that maps correctly per platform.
*/

// Migration Example: Cross-Platform Device Identification Service
// Replaces Windows-specific registry and WMI calls

using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Salaty.First.Core.Services;

/// <summary>
/// Cross-platform device identification service
/// MIGRATION: Replaces Windows-only registry/WMI approach with cross-platform fingerprinting
/// </summary>
public class DeviceIdentificationService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _deviceId;
    private readonly string _tursoUrl;
    private readonly string _authToken;
    private bool _disposed;

    public DeviceIdentificationService(string tursoUrl, string authToken)
    {
        _tursoUrl = tursoUrl;
        _authToken = authToken;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}");
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
        
        // MIGRATION: Cross-platform device ID generation
        _deviceId = GenerateDeviceId();
    }

    /// <summary>
    /// Generates a unique device ID using cross-platform available information
    /// MIGRATION: No Windows registry or WMI - works on Linux, macOS, Windows
    /// </summary>
    private string GenerateDeviceId()
    {
        try
        {
            var sb = new StringBuilder();

            // 1. Machine name (available on all platforms)
            sb.Append(Environment.MachineName);
            sb.Append("|");

            // 2. User name (available on all platforms)
            sb.Append(Environment.UserName);
            sb.Append("|");

            // 3. Operating system (platform identifier)
            sb.Append(Environment.OSVersion.ToString());
            sb.Append("|");

            // 4. Processor count (hardware identifier)
            sb.Append(Environment.ProcessorCount);
            sb.Append("|");

            // 5. System page size (hardware identifier)
            sb.Append(Environment.SystemPageSize);

            // 6. Network interfaces (cross-platform MAC addresses)
            try
            {
                var nics = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
                                  nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                  nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                    .ToList();

                foreach (var nic in nics)
                {
                    var mac = nic.GetPhysicalAddress().ToString();
                    if (!string.IsNullOrEmpty(mac) && mac.Length >= 12)
                    {
                        sb.Append("|").Append(mac);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeviceIdentification] Network info unavailable: {ex.Message}");
            }

            // Generate hash for consistent ID
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
            var deviceId = Convert.ToHexString(hashBytes).Substring(0, 16);

            Console.WriteLine($"[DeviceIdentification] Generated ID: {deviceId}");
            return deviceId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DeviceIdentification] Error generating ID: {ex.Message}");
            // Fallback to GUID
            return Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper();
        }
    }

    /// <summary>
    /// Gets device information for analytics
    /// MIGRATION: Cross-platform device info collection
    /// </summary>
    private DeviceInfo GetDeviceInfo()
    {
        return new DeviceInfo
        {
            DeviceId = _deviceId,
            MachineName = SanitizeForSql(Environment.MachineName),
            OSVersion = SanitizeForSql(Environment.OSVersion.ToString()),
            Platform = GetPlatform(),
            DotNetVersion = Environment.Version.ToString(),
            AppVersion = GetAppVersion()
        };
    }

    /// <summary>
    /// Determines the current platform
    /// </summary>
    private string GetPlatform()
    {
        if (OperatingSystem.IsWindows()) return "Windows";
        if (OperatingSystem.IsLinux()) return "Linux";
        if (OperatingSystem.IsMacOS()) return "macOS";
        return "Unknown";
    }

    /// <summary>
    /// Sanitizes string for SQL to prevent injection
    /// </summary>
    private string SanitizeForSql(string input)
    {
        if (string.IsNullOrEmpty(input)) return "Unknown";
        return input.Replace("'", "''").Replace(";", "").Substring(0, Math.Min(input.Length, 100));
    }

    /// <summary>
    /// Gets application version
    /// </summary>
    private string GetAppVersion()
    {
        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return $"{version?.Major}.{version?.Minor}.{version?.Build}";
        }
        catch
        {
            return "1.0.0";
        }
    }

    /// <summary>
    /// Executes SQL against Turso database
    /// MIGRATION: Same approach works cross-platform (HTTP API)
    /// </summary>
    private async Task<bool> ExecuteSqlAsync(string sql, params object[] args)
    {
        try
        {
            var endpoint = $"{_tursoUrl}/v2/pipeline";

            // Replace placeholders with sanitized values
            var finalSql = sql;
            if (args != null && args.Length > 0)
            {
                foreach (var arg in args)
                {
                    var escapedValue = arg?.ToString()?.Replace("'", "''") ?? "NULL";
                    var placeholderIndex = finalSql.IndexOf('?');
                    if (placeholderIndex >= 0)
                    {
                        finalSql = finalSql.Substring(0, placeholderIndex) + $"'{escapedValue}'" + finalSql.Substring(placeholderIndex + 1);
                    }
                }
            }

            var requestBody = new
            {
                requests = new[]
                {
                    new
                    {
                        type = "execute",
                        stmt = new { sql = finalSql }
                    }
                }
            };

            var jsonBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("[DeviceIdentification] SQL executed successfully");
                return true;
            }

            Console.WriteLine($"[DeviceIdentification] HTTP error: {response.StatusCode} - {responseJson}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DeviceIdentification] SQL execution error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Initializes the database table
    /// </summary>
    public async Task InitializeTableAsync()
    {
        var sql = @"
            CREATE TABLE IF NOT EXISTS device_counters (
                device_id TEXT PRIMARY KEY,
                machine_name TEXT,
                os_version TEXT,
                platform TEXT,
                dotnet_version TEXT,
                app_version TEXT,
                first_seen TEXT,
                last_seen TEXT,
                total_launches INTEGER DEFAULT 0,
                country TEXT
            )";

        await ExecuteSqlAsync(sql);
        Console.WriteLine("[DeviceIdentification] Table initialized");
    }

    /// <summary>
    /// Increments the device counter on app launch
    /// </summary>
    public async Task IncrementCounterAsync(string country = "Unknown")
    {
        try
        {
            await InitializeTableAsync();
            var deviceInfo = GetDeviceInfo();

            var sql = @"
                INSERT INTO device_counters 
                    (device_id, machine_name, os_version, platform, dotnet_version, app_version, first_seen, last_seen, total_launches, country)
                VALUES (?, ?, ?, ?, ?, ?, datetime('now'), datetime('now'), 1, ?)
                ON CONFLICT(device_id) DO UPDATE SET
                    last_seen = datetime('now'),
                    total_launches = total_launches + 1,
                    app_version = excluded.app_version";

            await ExecuteSqlAsync(sql,
                deviceInfo.DeviceId,
                deviceInfo.MachineName,
                deviceInfo.OSVersion,
                deviceInfo.Platform,
                deviceInfo.DotNetVersion,
                deviceInfo.AppVersion,
                country);

            Console.WriteLine($"[DeviceIdentification] Counter incremented for: {deviceInfo.DeviceId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DeviceIdentification] Error incrementing counter: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
    }

    /// <summary>
    /// Device information structure
    /// </summary>
    private class DeviceInfo
    {
        public string DeviceId { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public string OSVersion { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string DotNetVersion { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
    }
}

/* COMPARISON: Old Windows-only vs New Cross-Platform

OLD (Windows-only):
- Uses Microsoft.Win32.Registry
- Uses System.Management (WMI)
- Windows registry keys for machine GUID
- Not compatible with Linux/macOS

NEW (Cross-platform):
- Uses Environment.* (works everywhere)
- Uses System.Net.NetworkInformation (cross-platform)
- Works on Windows, Linux, and macOS
- No external dependencies
- More privacy-friendly (no sensitive hardware access)
*/

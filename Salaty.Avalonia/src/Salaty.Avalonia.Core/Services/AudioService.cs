// Migration Example: Audio Service Abstraction
// Shows how to handle platform-specific audio playback

namespace Salaty.First.Core.Services;

/// <summary>
/// Audio service interface - abstracts platform-specific audio implementations
/// </summary>
public interface IAudioService
{
    Task PlayAsync(string audioPath);
    Task PlayAthanAsync(string prayerName);
    void Stop();
    bool IsPlaying { get; }
}

/// <summary>
/// Linux audio service implementation
/// MIGRATION: Uses available Linux audio systems
/// </summary>
public class LinuxAudioService : IAudioService, IDisposable
{
    private System.Diagnostics.Process? _currentProcess;
    private bool _disposed;

    public bool IsPlaying => _currentProcess != null && !_currentProcess.HasExited;

    public async Task PlayAsync(string audioPath)
    {
        if (!File.Exists(audioPath))
        {
            Console.WriteLine($"[LinuxAudio] Audio file not found: {audioPath}");
            return;
        }

        try
        {
            Stop(); // Stop any currently playing audio

            // Try different audio players in order of preference
            var players = new[] { "paplay", "aplay", "ogg123", "mpg123", "ffplay" };
            string? availablePlayer = null;

            foreach (var player in players)
            {
                if (IsCommandAvailable(player))
                {
                    availablePlayer = player;
                    break;
                }
            }

            if (availablePlayer == null)
            {
                Console.WriteLine("[LinuxAudio] No audio player found. Install pulseaudio-utils or alsa-utils.");
                return;
            }

            _currentProcess = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = availablePlayer,
                    Arguments = GetPlayerArguments(availablePlayer, audioPath),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            _currentProcess.Start();
            Console.WriteLine($"[LinuxAudio] Playing: {audioPath} using {availablePlayer}");

            // Wait for completion without blocking
            await _currentProcess.WaitForExitAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LinuxAudio] Error playing audio: {ex.Message}");
        }
    }

    public async Task PlayAthanAsync(string prayerName)
    {
        var audioPath = GetAthanPath(prayerName);
        await PlayAsync(audioPath);
    }

    public void Stop()
    {
        if (_currentProcess != null && !_currentProcess.HasExited)
        {
            try
            {
                _currentProcess.Kill();
                _currentProcess.WaitForExit(1000);
            }
            catch { }
            finally
            {
                _currentProcess.Dispose();
                _currentProcess = null;
            }
        }
    }

    private string GetAthanPath(string prayerName)
    {
        // MIGRATION: Store audio files in platform-appropriate location
        var basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Salaty",
            "Resources",
            "MP3");

        var fileName = prayerName.ToUpper() switch
        {
            "FAJR" => "fajr.mp3",
            "DUHR" or "DHUHR" => "duhr.mp3",
            "ASR" => "asr.mp3",
            "MAGHRIB" => "maghrib.mp3",
            "ISHA" => "isha.mp3",
            _ => "athan.mp3"
        };

        return Path.Combine(basePath, fileName);
    }

    private bool IsCommandAvailable(string command)
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "which",
                    Arguments = command,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private string GetPlayerArguments(string player, string audioPath)
    {
        return player switch
        {
            "paplay" => audioPath,  // PulseAudio
            "aplay" => audioPath,  // ALSA
            "ogg123" => $"-q {audioPath}",
            "mpg123" => $"-q {audioPath}",
            "ffplay" => $"-nodisp -autoexit {audioPath}",
            _ => audioPath
        };
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Stop();
            _disposed = true;
        }
    }
}

/// <summary>
/// Windows audio service implementation
/// MIGRATION: Uses NAudio or Windows APIs
/// </summary>
public class WindowsAudioService : IAudioService, IDisposable
{
    // Could use NAudio or Windows Media Foundation
    // For now, simplified implementation

    public bool IsPlaying { get; private set; }

    public async Task PlayAsync(string audioPath)
    {
        // Windows-specific implementation using MediaPlayer or NAudio
        await Task.Run(() =>
        {
            try
            {
                // MIGRATION: Could use System.Media.SoundPlayer for simple cases
                // or NAudio for more control
                using var player = new System.Media.SoundPlayer(audioPath);
                player.PlaySync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WindowsAudio] Error: {ex.Message}");
            }
        });
    }

    public Task PlayAthanAsync(string prayerName)
    {
        var audioPath = GetAthanPath(prayerName);
        return PlayAsync(audioPath);
    }

    public void Stop()
    {
        IsPlaying = false;
    }

    private string GetAthanPath(string prayerName)
    {
        var basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Salaty",
            "Resources",
            "MP3");

        var fileName = prayerName.ToUpper() switch
        {
            "FAJR" => "fajr.mp3",
            "DUHR" or "DHUHR" => "duhr.mp3",
            "ASR" => "asr.mp3",
            "MAGHRIB" => "maghrib.mp3",
            "ISHA" => "isha.mp3",
            _ => "athan.mp3"
        };

        return Path.Combine(basePath, fileName);
    }

    public void Dispose() { }
}

/* COMPARISON: Platform-Specific Audio

WPF (Windows-only):
- System.Media.SoundPlayer: Simple but limited
- Windows Media Player COM: Complex, Windows-only
- No good cross-platform options

Avalonia (Cross-platform):
- Abstraction layer with platform implementations
- Linux: paplay/aplay command-line tools
- Windows: SoundPlayer or NAudio
- macOS: AVAudioPlayer or NSSound

Each platform uses its native audio system while the interface remains consistent.
*/

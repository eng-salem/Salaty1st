using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SalatyMinimal.Services
{
    public interface IAudioService
    {
        Task PlayAsync(string audioPath);
        Task PlayAthanAsync(string prayerName);
        void Stop();
        bool IsPlaying { get; }
    }

    public class LinuxAudioService : IAudioService
    {
        private Process? _currentProcess;

        public bool IsPlaying => _currentProcess != null && !_currentProcess.HasExited;

        public async Task PlayAsync(string audioPath)
        {
            if (!File.Exists(audioPath))
            {
                Console.WriteLine($"Audio file not found: {audioPath}");
                return;
            }

            try
            {
                // Try different audio players in order of preference
                var players = new[] { "paplay", "aplay", "ogg123", "mpg123", "ffplay" };
                string? availablePlayer = null;

                foreach (var player in players)
                {
                    try
                    {
                        var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = player,
                                Arguments = "--version",
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            }
                        };
                        process.Start();
                        await process.WaitForExitAsync();
                        
                        if (process.ExitCode == 0)
                        {
                            availablePlayer = player;
                            break;
                        }
                    }
                    catch
                    {
                        // Player not available, try next
                    }
                }

                if (availablePlayer == null)
                {
                    Console.WriteLine("No suitable audio player found");
                    return;
                }

                // Play the audio file
                _currentProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = availablePlayer,
                        Arguments = $"\"{audioPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                Console.WriteLine($"Playing athan with: {availablePlayer}");
                _currentProcess.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing audio: {ex.Message}");
            }
        }

        public async Task PlayAthanAsync(string prayerName)
        {
            // For demo, we'll use system beep
            Console.WriteLine($"🎵 Playing Athan for {prayerName}");
            
            if (OperatingSystem.IsLinux())
            {
                try
                {
                    // Use system beep on Linux
                    var beepProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "beep",
                            Arguments = "-f 800 -l 200 -r 3", // 800Hz for 200ms, repeat 3 times
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    
                    try
                    {
                        beepProcess.Start();
                        await beepProcess.WaitForExitAsync();
                    }
                    catch
                    {
                        // Fallback to terminal bell
                        Console.WriteLine("\a\a\a"); // ASCII bell character repeated
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not play athan sound: {ex.Message}");
                    // Final fallback
                    Console.WriteLine("\a");
                }
            }
        }

        public void Stop()
        {
            if (_currentProcess != null && !_currentProcess.HasExited)
            {
                try
                {
                    _currentProcess.Kill();
                    _currentProcess.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error stopping audio: {ex.Message}");
                }
                finally
                {
                    _currentProcess = null;
                }
            }
        }
    }
}

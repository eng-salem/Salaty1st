using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SalatyMinimal.Services
{
    public class SystemTrayService
    {
        private Process? _indicatorProcess;
        private bool _isLinux;

        public SystemTrayService()
        {
            _isLinux = OperatingSystem.IsLinux();
        }

        public async Task<bool> InitializeAsync()
        {
            if (!_isLinux)
            {
                Console.WriteLine("System tray is only supported on Linux");
                return false;
            }

            try
            {
                // Check if libappindicator is available
                var checkProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ldconfig",
                        Arguments = "-p | grep libappindicator",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                checkProcess.Start();
                await checkProcess.WaitForExitAsync();
                
                if (checkProcess.ExitCode == 0)
                {
                    Console.WriteLine("System tray initialized successfully");
                    return true;
                }
                else
                {
                    Console.WriteLine("libappindicator not found. System tray disabled.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"System tray initialization failed: {ex.Message}");
                return false;
            }
        }

        public async Task CreateTrayIcon()
        {
            if (!_isLinux) return;

            try
            {
                // Create a simple system tray indicator using Python
                var pythonScript = @"
import gi
gi.require_version('Gtk', '3.0')
gi.require_version('AppIndicator3', '0.1')
from gi.repository import Gtk, AppIndicator3
import threading
import time

class SalatyTray:
    def __init__(self):
        self.indicator = AppIndicator3.Indicator.new(
            'salaty',
            'Salaty Prayer Widget',
            'applications-internet',
            AppIndicator3.IndicatorCategory.APPLICATION_STATUS
        )
        self.indicator.set_status(AppIndicator3.IndicatorStatus.ACTIVE)
        self.indicator.set_label('Salaty', 'app')
        
        self.menu = Gtk.Menu()
        
        # Refresh item
        refresh_item = Gtk.MenuItem(label='Refresh Prayer Times')
        refresh_item.connect('activate', self.on_refresh)
        self.menu.append(refresh_item)
        
        # Settings item
        settings_item = Gtk.MenuItem(label='Settings')
        settings_item.connect('activate', self.on_settings)
        self.menu.append(settings_item)
        
        self.menu.append(Gtk.SeparatorMenuItem())
        
        # Always on top
        self.always_on_top_item = Gtk.CheckMenuItem(label='Always on Top')
        self.always_on_top_item.set_active(True)
        self.always_on_top_item.connect('activate', self.on_always_on_top)
        self.menu.append(self.always_on_top_item)
        
        self.menu.append(Gtk.SeparatorMenuItem())
        
        # About item
        about_item = Gtk.MenuItem(label='About')
        about_item.connect('activate', self.on_about)
        self.menu.append(about_item)
        
        # Quit item
        quit_item = Gtk.MenuItem(label='Quit')
        quit_item.connect('activate', self.on_quit)
        self.menu.append(quit_item)
        
        self.indicator.set_menu(self.menu)
        
        # Keep the tray icon alive
        self.running = True
        Gtk.main()
    
    def on_refresh(self, widget):
        print('Refresh requested from tray')
    
    def on_settings(self, widget):
        print('Settings requested from tray')
    
    def on_always_on_top(self, widget):
        active = self.always_on_top_item.get_active()
        print(f'Always on top: {active}')
    
    def on_about(self, widget):
        dialog = Gtk.MessageDialog(
            parent=None,
            flags=Gtk.DialogFlags.MODAL,
            type=Gtk.MessageType.INFO,
            buttons=Gtk.ButtonsType.OK,
            message_format='Salaty Prayer Widget v1.0\n\nA modern prayer times widget for Linux\n\n© 2025 Salaty Project'
        )
        dialog.run()
        dialog.destroy()
    
    def on_quit(self, widget):
        self.running = False
        Gtk.main_quit()

if __name__ == '__main__':
    tray = SalatyTray()
";

                var tempScriptPath = Path.Combine(Path.GetTempPath(), "salaty_tray.py");
                await File.WriteAllTextAsync(tempScriptPath, pythonScript);

                _indicatorProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "python3",
                        Arguments = tempScriptPath,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                _indicatorProcess.Start();
                Console.WriteLine("System tray icon created");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create tray icon: {ex.Message}");
            }
        }

        public void ShowNotification(string title, string message)
        {
            if (!_isLinux) return;

            try
            {
                var notifyProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "notify-send",
                        Arguments = $"-i applications-internet -t 3000 '{title}' '{message}'",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                notifyProcess.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to show notification: {ex.Message}");
            }
        }

        public void UpdateStatus(string status)
        {
            if (!_isLinux || _indicatorProcess == null) return;

            try
            {
                // Update tray icon status (would need more complex Python integration)
                Console.WriteLine($"Tray status updated: {status}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to update tray status: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (_indicatorProcess != null && !_indicatorProcess.HasExited)
            {
                try
                {
                    _indicatorProcess.Kill();
                    _indicatorProcess.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error disposing tray: {ex.Message}");
                }
            }
        }
    }
}

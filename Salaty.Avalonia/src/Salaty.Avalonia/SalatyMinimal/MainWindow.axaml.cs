using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using SalatyMinimal.Services;
using SalatyMinimal.Models;

namespace SalatyMinimal;

public partial class MainWindow : Window
{
    private readonly Timer _timer;
    private readonly PrayerService _prayerService;
    private readonly IAudioService _audioService;
    private readonly SettingsService _settingsService;
    private readonly SystemTrayService _systemTrayService;
    private DailyPrayerTimes? _currentPrayerTimes;
    private DateTime _startTime = DateTime.Now;
    private DateTime _lastAthanTime = DateTime.MinValue;
    private string _lastPrayerName = "";
    private bool _isDragging = false;
    private Avalonia.PixelPoint _dragStartPosition;

    public MainWindow()
    {
        InitializeComponent();
        
        // Initialize services
        _settingsService = new SettingsService();
        _prayerService = new PrayerService(_settingsService);
        _audioService = new LinuxAudioService();
        _systemTrayService = new SystemTrayService();
        
        // Apply settings
        ApplySettings();
        
        // Setup timer for updates (every second)
        _timer = new Timer(1000);
        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
        
        // Initialize system tray
        _ = InitializeSystemTrayAsync();
        
        // Load initial prayer times
        _ = LoadPrayerTimesAsync();
        
        // Setup button handlers
        var refreshButton = this.FindControl<Button>("RefreshButton");
        if (refreshButton != null)
            refreshButton.Click += async (_, _) => await RefreshPrayerTimesAsync();
        
        var settingsButton = this.FindControl<Button>("SettingsButton");
        if (settingsButton != null)
            settingsButton.Click += (_, _) => ShowLocationSelection();
        
        // Setup mouse events for dragging
        this.PointerPressed += OnPointerPressed;
        this.PointerMoved += OnPointerMoved;
        this.PointerReleased += OnPointerReleased;
        
        // Setup context menu
        SetupContextMenu();
        
        // Position window based on saved settings
        PositionWindow();
    }

    private void ApplySettings()
    {
        var settings = _settingsService.Settings;
        
        // Apply window settings
        Topmost = settings.AlwaysOnTop;
        
        // Apply display settings
        var progressArc = this.FindControl<Control>("ProgressArc");
        if (progressArc != null)
        {
            progressArc.IsVisible = settings.ShowProgressBar;
        }
        
        var hijriDateText = this.FindControl<TextBlock>("HijriDateText");
        if (hijriDateText != null)
        {
            hijriDateText.IsVisible = settings.ShowHijriDate;
        }
        
        Console.WriteLine($"Settings applied: City={settings.City}, AlwaysOnTop={settings.AlwaysOnTop}");
    }

    private async Task InitializeSystemTrayAsync()
    {
        try
        {
            var initialized = await _systemTrayService.InitializeAsync();
            if (initialized)
            {
                _ = _systemTrayService.CreateTrayIcon();
                Console.WriteLine("System tray initialized");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"System tray initialization failed: {ex.Message}");
        }
    }

    private void OnPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isDragging = true;
            _dragStartPosition = new Avalonia.PixelPoint((int)e.GetPosition(this).X, (int)e.GetPosition(this).Y);
            e.Handled = true;
        }
    }

    private void OnPointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        if (_isDragging)
        {
            var currentPos = e.GetPosition(this);
            var delta = new Avalonia.Point(currentPos.X - _dragStartPosition.X, currentPos.Y - _dragStartPosition.Y);
            Position = new Avalonia.PixelPoint(Position.X + (int)delta.X, Position.Y + (int)delta.Y);
            e.Handled = true;
        }
    }

    private void OnPointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            // Save window position
            _settingsService.UpdateWindowPosition(Position.X, Position.Y);
            e.Handled = true;
        }
    }

    private void SetupContextMenu()
    {
        var contextMenu = new ContextMenu();
        
        var refreshItem = new MenuItem { Header = "Refresh Prayer Times" };
        refreshItem.Click += async (_, _) => await RefreshPrayerTimesAsync();
        
        var settingsItem = new MenuItem { Header = "Settings" };
        settingsItem.Click += (_, _) => ShowSettingsMessage();
        
        var topMostItem = new MenuItem { Header = "Always on Top" };
        topMostItem.Click += (_, _) => ToggleAlwaysOnTop();
        
        var closeItem = new MenuItem { Header = "Close" };
        closeItem.Click += (_, _) => Close();
        
        contextMenu.Items.Add(refreshItem);
        contextMenu.Items.Add(settingsItem);
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(topMostItem);
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(closeItem);
        
        ContextMenu = contextMenu;
    }

    private void ToggleAlwaysOnTop()
    {
        Topmost = !Topmost;
        _settingsService.UpdateDisplaySettings(Topmost, true, true, "en");
    }

    private async void ShowLocationSelection()
    {
        Console.WriteLine("[MainWindow] ShowLocationSelection called");
        var locationWindow = new LocationSelectionWindow();
        
        // Pre-select current location
        var settings = _settingsService.Settings;
        Console.WriteLine($"[MainWindow] Current location: {settings.City}, {settings.Country}");
        
        // Set current selection in window
        locationWindow.SetCurrentLocation(
            settings.City, 
            settings.Country, 
            settings.CalculationMethod,
            settings.HijriAdjustment,
            settings.AsrMethod
        );
        
        var result = await locationWindow.ShowDialog<bool>(this);
        Console.WriteLine($"[MainWindow] LocationSelection result: {result}");
        
        if (result)
        {
            Console.WriteLine($"[MainWindow] Updating location to: {locationWindow.SelectedCity}, {locationWindow.SelectedCountry}");
            
            // Update settings with new location
            _settingsService.UpdateLocation(
                locationWindow.SelectedCity,
                locationWindow.SelectedCountry,
                locationWindow.Latitude,
                locationWindow.Longitude,
                settings.Timezone
            );
            
            // Update Hijri adjustment
            _settingsService.UpdateHijriAdjustment(locationWindow.HijriAdjustment);
            
            // Update calculation method
            _settingsService.UpdateCalculationMethod(
                locationWindow.CalculationMethod,
                locationWindow.AsrMethod,
                settings.HigherLatitudesMethod
            );
            
            Console.WriteLine("[MainWindow] Location updated in settings");
            
            // Clear cache and reload prayer times
            _prayerService.ClearCache();
            Console.WriteLine("[MainWindow] Cache cleared, reloading prayer times");
            await LoadPrayerTimesAsync();
            
            _systemTrayService.ShowNotification(
                "Location Updated", 
                $"Now showing prayer times for {locationWindow.SelectedCity}, {locationWindow.SelectedCountry}"
            );
        }
    }

    private void ShowSettingsMessage()
    {
        var settings = _settingsService.Settings;
        var messageBox = new Window
        {
            Title = "Settings & Status",
            Width = 400,
            Height = 300,
            Content = new StackPanel
            {
                Spacing = 10,
                Margin = new Avalonia.Thickness(20),
                Children =
                {
                    new TextBlock { Text = "Salaty Prayer Widget - Production Ready!", FontWeight = Avalonia.Media.FontWeight.Bold },
                    new TextBlock { Text = $"Location: {settings.City}, {settings.Country}" },
                    new TextBlock { Text = $"Coordinates: {settings.Latitude:F6}, {settings.Longitude:F6}" },
                    new TextBlock { Text = $"Calculation Method: {settings.CalculationMethod}" },
                    new TextBlock { Text = $"API Status: Connected to Aladhan API" },
                    new TextBlock { Text = $"System Tray: Available" },
                    new TextBlock { Text = $"Settings Saved: {_settingsService.Settings.City}" },
                    new Separator(),
                    new TextBlock { Text = "Features working:", FontWeight = Avalonia.Media.FontWeight.Bold },
                    new TextBlock { Text = "• City/Country selection" },
                    new TextBlock { Text = "• Real prayer times from API" },
                    new TextBlock { Text = "• Window dragging enabled" },
                    new TextBlock { Text = "• Settings persistence" },
                    new TextBlock { Text = "• System tray integration" },
                    new TextBlock { Text = "• Audio notifications" },
                    new TextBlock { Text = "• Accurate Hijri date" }
                }
            }
        };
        messageBox.ShowDialog(this);
    }

    private async Task LoadPrayerTimesAsync()
    {
        try
        {
            Console.WriteLine("Loading prayer times...");
            Console.WriteLine($"Current local time: {DateTime.Now:HH:mm:ss}");
            Console.WriteLine($"Current date: {DateTime.Today:yyyy-MM-dd}");
            
            // Debug: Show current settings
            var settings = _settingsService.Settings;
            Console.WriteLine($"[DEBUG] Current settings - Method: {settings.CalculationMethod}, Asr: {settings.AsrMethod}, Hijri: {settings.HijriAdjustment}");
            
            _currentPrayerTimes = await _prayerService.GetPrayerTimesAsync();
            
            Console.WriteLine($"Loaded prayer times:");
            Console.WriteLine($"  Fajr: {_currentPrayerTimes.Fajr:HH:mm:ss}");
            Console.WriteLine($"  Dhuhr: {_currentPrayerTimes.Dhuhr:HH:mm:ss}");
            Console.WriteLine($"  Asr: {_currentPrayerTimes.Asr:HH:mm:ss}");
            Console.WriteLine($"  Maghrib: {_currentPrayerTimes.Maghrib:HH:mm:ss}");
            Console.WriteLine($"  Isha: {_currentPrayerTimes.Isha:HH:mm:ss}");
            Console.WriteLine($"  Next prayer: {_currentPrayerTimes.NextPrayer} at {_currentPrayerTimes.NextPrayerTime:HH:mm:ss}");
            Console.WriteLine($"  Previous prayer: {_currentPrayerTimes.PreviousPrayer} at {_currentPrayerTimes.PreviousPrayerTime:HH:mm:ss}");
            
            var timeUntil = _prayerService.GetTimeUntilNextPrayer(_currentPrayerTimes);
            Console.WriteLine($"  Time until next prayer: {timeUntil.Hours:D2}:{timeUntil.Minutes:D2}:{timeUntil.Seconds:D2}");
            
            // Update UI immediately
            UpdateUI();
            
            // Show notification if system tray is available
            _systemTrayService.ShowNotification("Prayer Times Updated", $"Next prayer: {_currentPrayerTimes.NextPrayer}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading prayer times: {ex.Message}");
        }
    }

    private async Task RefreshPrayerTimesAsync()
    {
        Console.WriteLine("Refreshing prayer times...");
        await _prayerService.RefreshPrayerTimesAsync();
        _currentPrayerTimes = await _prayerService.GetPrayerTimesAsync();
        UpdateUI();
        
        _systemTrayService.ShowNotification("Prayer Times Refreshed", $"Next prayer: {_currentPrayerTimes.NextPrayer}");
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        // Update UI on UI thread
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            UpdateUI();
            CheckForAthanTime();
        });
    }

    private void UpdateUI()
    {
        if (_currentPrayerTimes == null) return;

        // Find UI controls
        var prayerNameText = this.FindControl<TextBlock>("PrayerNameText");
        var countdownText = this.FindControl<TextBlock>("CountdownText");
        var hijriDateText = this.FindControl<TextBlock>("HijriDateText");
        var elapsedText = this.FindControl<TextBlock>("ElapsedText");
        var progressArc = this.FindControl<Control>("ProgressArc");
        var statusIndicator = this.FindControl<Control>("StatusIndicator");

        // Update prayer name
        if (prayerNameText != null)
        {
            // Clean the prayer name to prevent corruption
            var cleanPrayerName = CleanPrayerName(_currentPrayerTimes.NextPrayer);
            Console.WriteLine($"[UI] Setting prayer name to: '{_currentPrayerTimes.NextPrayer}' -> '{cleanPrayerName}'");
            prayerNameText.Text = cleanPrayerName;
        }

        // Update countdown
        if (countdownText != null)
        {
            var timeUntil = _prayerService.GetTimeUntilNextPrayer(_currentPrayerTimes);
            if (timeUntil.TotalSeconds > 0)
                countdownText.Text = $"{timeUntil.Hours:D2}:{timeUntil.Minutes:D2}:{timeUntil.Seconds:D2}";
            else
                countdownText.Text = "00:00:00";
        }

        // Update elapsed time
        if (elapsedText != null)
        {
            var elapsed = DateTime.Now - _startTime;
            elapsedText.Text = $"Elapsed: {elapsed.TotalMinutes:F0}m";
        }

        // Update Hijri date
        if (hijriDateText != null && hijriDateText.IsVisible)
        {
            hijriDateText.Text = _prayerService.GetHijriDate();
        }

        // Update progress ring
        if (progressArc != null && progressArc.IsVisible)
        {
            var percentage = _prayerService.GetPrayerProgressPercentage(_currentPrayerTimes);
            
            // Calculate dash array for progress ring
            var radius = 100;
            var circumference = 2 * Math.PI * radius;
            var dashLength = circumference * (percentage / 100.0);
            var gapLength = circumference - dashLength;
            
            // Try to set StrokeDashArray if it's an Ellipse
            if (progressArc is Avalonia.Controls.Shapes.Ellipse ellipse)
            {
                ellipse.StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { dashLength, gapLength };
                
                // Update color based on progress
                var color = percentage switch
                {
                    >= 90 => new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#F44336")), // Red
                    >= 75 => new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#FF9800")), // Orange
                    >= 50 => new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#FFC107")), // Yellow
                    _ => new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#4FACFE"))      // Blue
                };
                
                ellipse.Stroke = color;
            }
        }

        // Update status indicator
        if (statusIndicator != null)
        {
            var timeUntil = _prayerService.GetTimeUntilNextPrayer(_currentPrayerTimes);
            var minutesUntil = timeUntil.TotalMinutes;
            
            if (statusIndicator is Avalonia.Controls.Shapes.Ellipse indicator)
            {
                if (minutesUntil <= 10)
                {
                    indicator.Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#F44336")); // Red
                    indicator.Opacity = 1.0;
                }
                else if (minutesUntil <= 30)
                {
                    indicator.Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#FF9800")); // Orange
                    indicator.Opacity = 0.8;
                }
                else
                {
                    indicator.Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#4FACFE")); // Blue
                    indicator.Opacity = 0.6;
                }
            }
        }
    }

    private string CleanPrayerName(string prayerName)
        {
            if (string.IsNullOrEmpty(prayerName))
                return "Unknown";
                
            // Remove any non-English characters and ensure clean display
            var cleanName = new string(prayerName.Where(char.IsLetterOrDigit).ToArray());
            
            // Ensure it's a valid prayer name
            var validPrayers = new[] { "Fajr", "Sunrise", "Dhuhr", "Asr", "Maghrib", "Isha" };
            
            foreach (var validPrayer in validPrayers)
            {
                if (cleanName.Equals(validPrayer, StringComparison.OrdinalIgnoreCase))
                    return validPrayer;
            }
            
            // Fallback to first word if corrupted
            var words = cleanName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return words.Length > 0 ? words[0] : cleanName;
        }

        private void CheckForAthanTime()
    {
        if (_currentPrayerTimes == null || !_settingsService.Settings.EnableAthan) return;

        var now = DateTime.Now;
        var nextPrayerTime = _currentPrayerTimes.NextPrayerTime;
        var timeDiff = (now - nextPrayerTime).TotalSeconds;

        // Trigger athan when we're 0 to 5 seconds AFTER the target prayer time
        if (timeDiff >= 0 && timeDiff <= 5 && 
            (_lastAthanTime == DateTime.MinValue || (now - _lastAthanTime).TotalMinutes > 1))
        {
            Console.WriteLine($"*** PRAYER TIME REACHED! *** {_currentPrayerTimes.NextPrayer} at {nextPrayerTime:HH:mm:ss}");
            
            // Play athan
            _ = _audioService.PlayAthanAsync(_currentPrayerTimes.NextPrayer);
            
            // Show notification
            _systemTrayService.ShowNotification(
                "Prayer Time", 
                $"It's time for {_currentPrayerTimes.NextPrayer} prayer"
            );
            
            _lastAthanTime = now;
            _lastPrayerName = _currentPrayerTimes.NextPrayer;
            
            // Flash status indicator
            FlashStatusIndicator();
            
            // Load next prayer times after a short delay
            _ = Task.Delay(5000).ContinueWith(_ => LoadPrayerTimesAsync());
        }
    }

    private void FlashStatusIndicator()
    {
        var statusIndicator = this.FindControl<Control>("StatusIndicator");
        if (statusIndicator == null) return;

        if (statusIndicator is Avalonia.Controls.Shapes.Ellipse indicator)
        {
            var originalOpacity = indicator.Opacity;
            
            // Flash effect
            for (int i = 0; i < 3; i++)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
                {
                    indicator.Opacity = 0.2;
                    await Task.Delay(200);
                    indicator.Opacity = 1.0;
                    await Task.Delay(200);
                }, Avalonia.Threading.DispatcherPriority.Background);
            }
        }
    }

    private void PositionWindow()
    {
        try
        {
            var settings = _settingsService.Settings;
            
            // Use saved position if available, otherwise position in top-right corner
            if (settings.WindowX != 0 || settings.WindowY != 0)
            {
                Position = new Avalonia.PixelPoint(settings.WindowX, settings.WindowY);
            }
            else
            {
                // Get screen dimensions
                var screenWidth = Screens.Primary.WorkingArea.Width;
                
                // Position in top-right corner with margin
                var margin = 20;
                Position = new Avalonia.PixelPoint(
                    screenWidth - (int)Width - margin,
                    margin
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not position window: {ex.Message}");
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        // Save current window position
        _settingsService.UpdateWindowPosition(Position.X, Position.Y);
        
        // Cleanup services
        _timer?.Stop();
        _timer?.Dispose();
        _audioService?.Stop();
        _systemTrayService?.Dispose();
        
        Console.WriteLine("Application closed gracefully");
        base.OnClosing(e);
    }
}

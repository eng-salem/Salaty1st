using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalatyMinimal
{
    public partial class LocationSelectionWindow : Window
    {
        public string SelectedCity { get; private set; } = "Mecca";
        public string SelectedCountry { get; private set; } = "Saudi Arabia";
        public double Latitude { get; private set; } = 21.3891;
        public double Longitude { get; private set; } = 39.8579;
        public int HijriAdjustment { get; private set; } = 0;
        public int CalculationMethod { get; private set; } = 1; // Default to MWL
        public int AsrMethod { get; private set; } = 0; // Default to Shafii

        private ComboBox _cityComboBox;
        private ComboBox _countryComboBox;
        private ComboBox _hijriAdjustmentComboBox;
        private ComboBox _calculationMethodComboBox;
        private ComboBox _asrMethodComboBox;
        private TextBlock _coordinatesText;

        public LocationSelectionWindow()
        {
            // Set window properties
            Width = 450;
            Height = 600;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Title = "Select Location";
            
            // Create UI
            CreateLocationUI();
        }

        public void SetCurrentLocation(string currentCity, string currentCountry, int currentCalculationMethod = 1, int currentHijriAdjustment = 0, int currentAsrMethod = 0)
        {
            // Find and select current country
            for (int i = 0; i < _countryComboBox.Items.Count; i++)
            {
                if (_countryComboBox.Items[i] as string == currentCountry)
                {
                    _countryComboBox.SelectedIndex = i;
                    break;
                }
            }
            
            // Load cities for the country and select current city
            LoadCitiesForCountry(currentCountry);
            
            for (int i = 0; i < _cityComboBox.Items.Count; i++)
            {
                if (_cityComboBox.Items[i] as string == currentCity)
                {
                    _cityComboBox.SelectedIndex = i;
                    break;
                }
            }
            
            // Set current calculation method
            var methodIds = new[] { 0, 1, 2, 3, 4, 5 };
            for (int i = 0; i < methodIds.Length; i++)
            {
                if (methodIds[i] == currentCalculationMethod)
                {
                    _calculationMethodComboBox.SelectedIndex = i;
                    break;
                }
            }
            
            // Set current Hijri adjustment
            if (currentHijriAdjustment == -1)
                _hijriAdjustmentComboBox.SelectedIndex = 0;
            else if (currentHijriAdjustment == 1)
                _hijriAdjustmentComboBox.SelectedIndex = 2;
            else
                _hijriAdjustmentComboBox.SelectedIndex = 1; // Default to 0
            
            // Set current Asr method
            if (currentAsrMethod == 1)
                _asrMethodComboBox.SelectedIndex = 1;
            else
                _asrMethodComboBox.SelectedIndex = 0; // Default to Shafii
        }

        private void CreateLocationUI()
        {
            // Create scrollable content
            var scrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                Content = new StackPanel
                {
                    Spacing = 15,
                    Margin = new Avalonia.Thickness(20)
                }
            };
            
            var mainPanel = (StackPanel)scrollViewer.Content;

            // Buttons at the TOP
            var buttonPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Spacing = 10,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                Margin = new Avalonia.Thickness(0, 0, 0, 20)
            };
            
            var saveButton = new Button { Content = "Save", Width = 80, Background = Avalonia.Media.Brushes.Blue, Foreground = Avalonia.Media.Brushes.White };
            var cancelButton = new Button { Content = "Cancel", Width = 80 };
            var detectButton = new Button { Content = "Auto Detect", Width = 100 };
            
            saveButton.Click += (_, _) => SaveAndClose();
            cancelButton.Click += (_, _) => Close(false);
            detectButton.Click += async (_, _) => await AutoDetectLocation();
            
            buttonPanel.Children.Add(detectButton);
            buttonPanel.Children.Add(saveButton);
            buttonPanel.Children.Add(cancelButton);
            
            mainPanel.Children.Add(buttonPanel);

            // Title
            var title = new TextBlock
            {
                Text = "Select Your Location",
                FontSize = 20,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 0, 0, 20)
            };
            mainPanel.Children.Add(title);

            // Country Selection
            var countryGroup = new Border
            {
                BorderThickness = new Avalonia.Thickness(1),
                BorderBrush = Avalonia.Media.Brushes.Gray,
                CornerRadius = new Avalonia.CornerRadius(5),
                Padding = new Avalonia.Thickness(10),
                Margin = new Avalonia.Thickness(0, 10, 0, 10)
            };
            
            var countryPanel = new StackPanel { Spacing = 10 };
            
            var headerText = new TextBlock 
            { 
                Text = "Country", 
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Margin = new Avalonia.Thickness(0, 0, 0, 10)
            };
            
            var countryLabel = new TextBlock { Text = "Select your country:" };
            _countryComboBox = new ComboBox
            {
                Width = 350,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            };
            
            // Add popular countries with significant Muslim populations
            var countries = new[]
            {
                "Saudi Arabia", "Egypt", "United Arab Emirates", "Kuwait",
                "Qatar", "Bahrain", "Oman", "Jordan", "Lebanon", "Syria",
                "Iraq", "Iran", "Afghanistan", "Pakistan", "Bangladesh",
                "India", "Indonesia", "Malaysia", "Turkey", "Morocco",
                "Algeria", "Tunisia", "Libya", "Sudan", "Yemen",
                "United Kingdom", "United States", "Canada", "Australia",
                "Germany", "France", "Netherlands", "Belgium", "Sweden"
            };
            
            foreach (var country in countries)
            {
                _countryComboBox.Items.Add(country);
            }
            
            _countryComboBox.SelectedIndex = 0; // Default to Saudi Arabia
            _countryComboBox.SelectionChanged += OnCountryChanged;
            
            countryPanel.Children.Add(headerText);
            countryPanel.Children.Add(countryLabel);
            countryPanel.Children.Add(_countryComboBox);
            
            countryGroup.Child = countryPanel;
            mainPanel.Children.Add(countryGroup);

            // City Selection
            var cityGroup = new Border
            {
                BorderThickness = new Avalonia.Thickness(1),
                BorderBrush = Avalonia.Media.Brushes.Gray,
                CornerRadius = new Avalonia.CornerRadius(5),
                Padding = new Avalonia.Thickness(10),
                Margin = new Avalonia.Thickness(0, 10, 0, 10)
            };
            
            var cityPanel = new StackPanel { Spacing = 10 };
            
            var cityHeaderText = new TextBlock 
            { 
                Text = "City", 
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Margin = new Avalonia.Thickness(0, 0, 0, 10)
            };
            
            var cityLabel = new TextBlock { Text = "Select your city:" };
            _cityComboBox = new ComboBox
            {
                Width = 350,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            };
            
            // Load cities for default country
            LoadCitiesForCountry("Saudi Arabia");
            
            _cityComboBox.SelectionChanged += OnCityChanged;
            
            cityPanel.Children.Add(cityHeaderText);
            cityPanel.Children.Add(cityLabel);
            cityPanel.Children.Add(_cityComboBox);
            
            cityGroup.Child = cityPanel;
            mainPanel.Children.Add(cityGroup);

            // Coordinates Display
            var coordGroup = new Border
            {
                BorderThickness = new Avalonia.Thickness(1),
                BorderBrush = Avalonia.Media.Brushes.Gray,
                CornerRadius = new Avalonia.CornerRadius(5),
                Padding = new Avalonia.Thickness(10),
                Margin = new Avalonia.Thickness(0, 10, 0, 10)
            };
            
            var coordPanel = new StackPanel { Spacing = 5 };
            
            var coordHeaderText = new TextBlock 
            { 
                Text = "Coordinates", 
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Margin = new Avalonia.Thickness(0, 0, 0, 10)
            };
            
            _coordinatesText = new TextBlock
            {
                Text = $"Latitude: {Latitude:F6}, Longitude: {Longitude:F6}",
                FontFamily = new Avalonia.Media.FontFamily("monospace"),
                Margin = new Avalonia.Thickness(10, 5, 10, 5)
            };
            
            coordPanel.Children.Add(coordHeaderText);
            coordPanel.Children.Add(_coordinatesText);
            coordGroup.Child = coordPanel;
            mainPanel.Children.Add(coordGroup);

            // Hijri Date Adjustment
            var hijriGroup = new Border
            {
                BorderThickness = new Avalonia.Thickness(1),
                BorderBrush = Avalonia.Media.Brushes.Gray,
                CornerRadius = new Avalonia.CornerRadius(5),
                Padding = new Avalonia.Thickness(10),
                Margin = new Avalonia.Thickness(0, 10, 0, 10)
            };
            
            var hijriPanel = new StackPanel { Spacing = 10 };
            
            var hijriHeaderText = new TextBlock 
            { 
                Text = "Hijri Date Adjustment", 
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Margin = new Avalonia.Thickness(0, 0, 0, 10)
            };
            
            var hijriLabel = new TextBlock 
            { 
                Text = "Adjust for local moon sighting (-1, 0, or +1 days):",
                Margin = new Avalonia.Thickness(0, 0, 0, 5)
            };
            
            _hijriAdjustmentComboBox = new ComboBox
            {
                Width = 200,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            };
            
            // Add adjustment options
            _hijriAdjustmentComboBox.Items.Add("-1 (Previous day)");
            _hijriAdjustmentComboBox.Items.Add("0 (No adjustment)");
            _hijriAdjustmentComboBox.Items.Add("+1 (Next day)");
            _hijriAdjustmentComboBox.SelectedIndex = 1; // Default to 0
            
            hijriPanel.Children.Add(hijriHeaderText);
            hijriPanel.Children.Add(hijriLabel);
            hijriPanel.Children.Add(_hijriAdjustmentComboBox);
            hijriGroup.Child = hijriPanel;
            mainPanel.Children.Add(hijriGroup);

            // Prayer Calculation Method
            var methodGroup = new Border
            {
                BorderThickness = new Avalonia.Thickness(1),
                BorderBrush = Avalonia.Media.Brushes.Gray,
                CornerRadius = new Avalonia.CornerRadius(5),
                Padding = new Avalonia.Thickness(10),
                Margin = new Avalonia.Thickness(0, 10, 0, 10)
            };
            
            var methodPanel = new StackPanel { Spacing = 10 };
            
            var methodHeaderText = new TextBlock 
            { 
                Text = "Prayer Calculation Method", 
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Margin = new Avalonia.Thickness(0, 0, 0, 10)
            };
            
            var methodLabel = new TextBlock 
            { 
                Text = "Select prayer time calculation method:",
                Margin = new Avalonia.Thickness(0, 0, 0, 5)
            };
            
            _calculationMethodComboBox = new ComboBox
            {
                Width = 350,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            };
            
            // Add calculation methods (same as VB.NET version)
            var calculationMethods = new[]
            {
                new { Id = 0, Name = "Jafari" },
                new { Id = 1, Name = "Muslim World League (MWL)" },
                new { Id = 2, Name = "Islamic Society of North America (ISNA)" },
                new { Id = 3, Name = "University of Islamic Sciences, Karachi" },
                new { Id = 4, Name = "Umm al-Qura, Makkah" },
                new { Id = 5, Name = "Egyptian General Authority of Survey" }
            };
            
            foreach (var method in calculationMethods)
            {
                _calculationMethodComboBox.Items.Add($"{method.Name} (ID: {method.Id})");
            }
            
            _calculationMethodComboBox.SelectedIndex = 1; // Default to MWL (ID: 1)
            
            methodPanel.Children.Add(methodHeaderText);
            methodPanel.Children.Add(methodLabel);
            methodPanel.Children.Add(_calculationMethodComboBox);
            methodGroup.Child = methodPanel;
            mainPanel.Children.Add(methodGroup);

            // Asr Calculation Method
            var asrGroup = new Border
            {
                BorderThickness = new Avalonia.Thickness(1),
                BorderBrush = Avalonia.Media.Brushes.Gray,
                CornerRadius = new Avalonia.CornerRadius(5),
                Padding = new Avalonia.Thickness(10),
                Margin = new Avalonia.Thickness(0, 10, 0, 10)
            };
            
            var asrPanel = new StackPanel { Spacing = 10 };
            
            var asrHeaderText = new TextBlock 
            { 
                Text = "Asr Calculation Method", 
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Margin = new Avalonia.Thickness(0, 0, 0, 10)
            };
            
            var asrLabel = new TextBlock 
            { 
                Text = "Select juristic method for Asr calculation:",
                Margin = new Avalonia.Thickness(0, 0, 0, 5)
            };
            
            _asrMethodComboBox = new ComboBox
            {
                Width = 350,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            };
            
            // Add Asr calculation methods
            var asrMethods = new[]
            {
                new { Id = 0, Name = "Shafii (Standard)" },
                new { Id = 1, Name = "Hanafi (Later)" }
            };
            
            foreach (var method in asrMethods)
            {
                _asrMethodComboBox.Items.Add($"{method.Name} (ID: {method.Id})");
            }
            
            _asrMethodComboBox.SelectedIndex = 0; // Default to Shafii (Standard)
            
            asrPanel.Children.Add(asrHeaderText);
            asrPanel.Children.Add(asrLabel);
            asrPanel.Children.Add(_asrMethodComboBox);
            asrGroup.Child = asrPanel;
            mainPanel.Children.Add(asrGroup);

            // Quick Search
            var searchGroup = new Border
            {
                BorderThickness = new Avalonia.Thickness(1),
                BorderBrush = Avalonia.Media.Brushes.Gray,
                CornerRadius = new Avalonia.CornerRadius(5),
                Padding = new Avalonia.Thickness(10),
                Margin = new Avalonia.Thickness(0, 10, 0, 10)
            };
            
            var searchPanel = new StackPanel { Spacing = 10 };
            
            var searchHeaderText = new TextBlock 
            { 
                Text = "Quick Search", 
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Margin = new Avalonia.Thickness(0, 0, 0, 10)
            };
            
            var searchTextBox = new TextBox
            {
                Width = 350,
                Watermark = "Type city name to search...",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            };
            
            var searchButton = new Button
            {
                Content = "Search City",
                Width = 100,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            };
            
            searchButton.Click += (_, _) => SearchCity(searchTextBox.Text);
            
            searchPanel.Children.Add(searchHeaderText);
            searchPanel.Children.Add(searchTextBox);
            searchPanel.Children.Add(searchButton);
            
            searchGroup.Child = searchPanel;
            mainPanel.Children.Add(searchGroup);

            Content = scrollViewer;
        }

        private void OnCountryChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            if (_countryComboBox.SelectedItem is string selectedCountry)
            {
                LoadCitiesForCountry(selectedCountry);
            }
        }

        private void OnCityChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            if (_cityComboBox.SelectedItem is string selectedCity)
            {
                UpdateCoordinates(selectedCity, _countryComboBox.SelectedItem as string ?? "Saudi Arabia");
            }
        }

        private void LoadCitiesForCountry(string country)
        {
            _cityComboBox.Items.Clear();
            
            var cities = GetCitiesForCountry(country);
            foreach (var city in cities)
            {
                _cityComboBox.Items.Add(city);
            }
            
            _cityComboBox.SelectedIndex = 0;
        }

        private string[] GetCitiesForCountry(string country)
        {
            return country switch
            {
                "Saudi Arabia" => new[] { "Mecca", "Madinah", "Riyadh", "Jeddah", "Dammam", "Khobar", "Taif", "Tabuk" },
                "Egypt" => new[] { "Cairo", "Alexandria", "Giza", "Shubra El Kheima", "Port Said", "Suez", "Luxor", "Aswan" },
                "United Arab Emirates" => new[] { "Dubai", "Abu Dhabi", "Sharjah", "Al Ain", "Ajman", "Ras Al Khaimah", "Fujairah", "Umm Al Quwain" },
                "Kuwait" => new[] { "Kuwait City", "Hawalli", "Salmiya", "Jahra", "Mubarak Al Kabeer", "Ahmadi", "Farwaniya", "Failaka" },
                "Qatar" => new[] { "Doha", "Al Rayyan", "Al Wakrah", "Al Khor", "Dukhan", "Mesaieed", "Shahaniya", "Um Salal" },
                "Jordan" => new[] { "Amman", "Irbid", "Zarqa", "Aqaba", "Madaba", "Karak", "Tafilah", "Jerash" },
                "Pakistan" => new[] { "Karachi", "Lahore", "Faisalabad", "Rawalpindi", "Gujranwala", "Peshawar", "Multan", "Islamabad" },
                "India" => new[] { "Delhi", "Mumbai", "Bangalore", "Chennai", "Kolkata", "Hyderabad", "Pune", "Ahmedabad" },
                "Indonesia" => new[] { "Jakarta", "Surabaya", "Bandung", "Medan", "Semarang", "Palembang", "Makassar", "Batam" },
                "Malaysia" => new[] { "Kuala Lumpur", "George Town", "Ipoh", "Shah Alam", "Petaling Jaya", "Johor Bahru", "Malacca", "Kota Kinabalu" },
                "Turkey" => new[] { "Istanbul", "Ankara", "Izmir", "Bursa", "Adana", "Gaziantep", "Konya", "Antalya" },
                "United States" => new[] { "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio", "San Diego" },
                "United Kingdom" => new[] { "London", "Birmingham", "Manchester", "Leeds", "Glasgow", "Liverpool", "Sheffield", "Bristol" },
                "Canada" => new[] { "Toronto", "Montreal", "Vancouver", "Calgary", "Edmonton", "Ottawa", "Winnipeg", "Quebec" },
                "Australia" => new[] { "Sydney", "Melbourne", "Brisbane", "Perth", "Adelaide", "Gold Coast", "Canberra", "Newcastle" },
                _ => new[] { "Capital City", "Major City 1", "Major City 2", "Major City 3" }
            };
        }

        private void UpdateCoordinates(string city, string country)
        {
            // Coordinates for major cities (approximate)
            var coords = GetCityCoordinates(city, country);
            Latitude = coords.lat;
            Longitude = coords.lng;
            
            _coordinatesText.Text = $"Latitude: {Latitude:F6}, Longitude: {Longitude:F6}";
        }

        private (double lat, double lng) GetCityCoordinates(string city, string country)
        {
            return city.ToLower() switch
            {
                // Saudi Arabia
                "mecca" => (21.3891, 39.8579),
                "madinah" => (24.4686, 39.6110),
                "riyadh" => (24.7136, 46.6753),
                "jeddah" => (21.2854, 39.8876),
                "dammam" => (26.4269, 50.1038),
                
                // UAE
                "dubai" => (25.2048, 55.2708),
                "abu dhabi" => (24.4539, 54.3773),
                "sharjah" => (25.3467, 55.4217),
                
                // Egypt
                "cairo" => (30.0444, 31.2357),
                "alexandria" => (31.2001, 29.9187),
                
                // Jordan
                "amman" => (31.9539, 35.9106),
                
                // Pakistan
                "karachi" => (24.8607, 67.0011),
                "lahore" => (31.5204, 74.3587),
                "islamabad" => (33.6844, 73.0479),
                
                // India
                "delhi" => (28.7041, 77.1025),
                "mumbai" => (19.0760, 72.8777),
                
                // Turkey
                "istanbul" => (41.0082, 28.9784),
                "ankara" => (39.9334, 32.8597),
                
                // UK
                "london" => (51.5074, -0.1278),
                
                // USA
                "new york" => (40.7128, -74.0060),
                "los angeles" => (34.0522, -118.2437),
                
                // Canada
                "toronto" => (43.6532, -79.3832),
                
                // Australia
                "sydney" => (-33.8688, 151.2093),
                "melbourne" => (-37.8136, 144.9631),
                
                _ => (21.3891, 39.8579) // Default to Mecca
            };
        }

        private void SearchCity(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
            {
                return;
            }

            // Simple search - look for city in current country
            var currentCountry = _countryComboBox.SelectedItem as string ?? "Saudi Arabia";
            var cities = GetCitiesForCountry(currentCountry);
            
            var foundCity = cities.FirstOrDefault(c => c.ToLower().Contains(cityName.ToLower()));
            
            if (foundCity != null)
            {
                var index = _cityComboBox.Items.IndexOf(foundCity);
                if (index >= 0)
                {
                    _cityComboBox.SelectedIndex = index;
                }
            }
            else
            {
                // Search across all countries
                var allCountries = new[]
                {
                    "Saudi Arabia", "Egypt", "United Arab Emirates", "Jordan", "Pakistan",
                    "India", "Turkey", "United Kingdom", "United States", "Canada", "Australia"
                };
                
                foreach (var country in allCountries)
                {
                    cities = GetCitiesForCountry(country);
                    foundCity = cities.FirstOrDefault(c => c.ToLower().Contains(cityName.ToLower()));
                    
                    if (foundCity != null)
                    {
                        var countryIndex = _countryComboBox.Items.IndexOf(country);
                        if (countryIndex >= 0)
                        {
                            _countryComboBox.SelectedIndex = countryIndex;
                            LoadCitiesForCountry(country);
                            
                            var cityIndex = _cityComboBox.Items.IndexOf(foundCity);
                            if (cityIndex >= 0)
                            {
                                _cityComboBox.SelectedIndex = cityIndex;
                            }
                        }
                        break;
                    }
                }
            }
        }

        private async Task AutoDetectLocation()
        {
            var messageBox = new Window
            {
                Title = "Auto-Detecting Location",
                Width = 350,
                Height = 200,
                Content = new StackPanel
                {
                    Spacing = 15,
                    Margin = new Avalonia.Thickness(20),
                    Children =
                    {
                        new TextBlock { Text = "🌍 Auto-detecting your location...", FontWeight = Avalonia.Media.FontWeight.Bold },
                        new TextBlock { Text = "Using IP geolocation service..." },
                        new ProgressBar { IsIndeterminate = true, Width = 300 }
                    }
                }
            };
            
            // Show the progress dialog
            _ = messageBox.ShowDialog(this);
            
            try
            {
                // Use a free IP geolocation API
                var httpClient = new System.Net.Http.HttpClient();
                var response = await httpClient.GetStringAsync("http://ip-api.com/json/");
                
                // Parse the JSON response
                var json = System.Text.Json.JsonDocument.Parse(response);
                var root = json.RootElement;
                
                if (root.TryGetProperty("country", out var countryProp) && 
                    root.TryGetProperty("city", out var cityProp) &&
                    root.TryGetProperty("lat", out var latProp) &&
                    root.TryGetProperty("lon", out var lonProp))
                {
                    var detectedCountry = countryProp.GetString() ?? "Unknown";
                    var detectedCity = cityProp.GetString() ?? "Unknown";
                    var detectedLat = latProp.GetDouble();
                    var detectedLon = lonProp.GetDouble();
                    
                    // Find the country in our list - include Muslim-majority countries
                    string? foundCountry = null;
                    var allCountries = new[]
                    {
                        "Saudi Arabia", "Egypt", "United Arab Emirates", "Kuwait", "Qatar", "Bahrain", "Oman",
                        "Jordan", "Lebanon", "Syria", "Iraq", "Iran", "Afghanistan", "Pakistan", "Bangladesh",
                        "India", "Indonesia", "Malaysia", "Turkey", "Morocco", "Algeria", "Tunisia", "Libya",
                        "Sudan", "Yemen", "United Kingdom", "United States", "Canada", "Australia",
                        "Germany", "France", "Netherlands", "Belgium", "Sweden"
                    };
                    
                    foreach (var country in allCountries)
                    {
                        if (detectedCountry.Contains(country) || country.Contains(detectedCountry))
                        {
                            foundCountry = country;
                            break;
                        }
                    }
                    
                    // Default to first country if not found
                    if (foundCountry == null && _countryComboBox.Items.Count > 0)
                    {
                        foundCountry = _countryComboBox.Items[0] as string;
                    }
                    
                    if (foundCountry != null)
                    {
                        // Select the detected country
                        for (int i = 0; i < _countryComboBox.Items.Count; i++)
                        {
                            if (_countryComboBox.Items[i] as string == foundCountry)
                            {
                                _countryComboBox.SelectedIndex = i;
                                break;
                            }
                        }
                        
                        // Load cities for the country
                        LoadCitiesForCountry(foundCountry);
                        
                        // Try to find the detected city
                        for (int i = 0; i < _cityComboBox.Items.Count; i++)
                        {
                            var city = _cityComboBox.Items[i] as string;
                            if (city != null && city.Contains(detectedCity) || detectedCity.Contains(city))
                            {
                                _cityComboBox.SelectedIndex = i;
                                break;
                            }
                        }
                        
                        // Update coordinates
                        Latitude = detectedLat;
                        Longitude = detectedLon;
                        UpdateCoordinates(detectedCity, foundCountry);
                        
                        messageBox.Close();
                        
                        // Show success message
                        var successBox = new Window
                        {
                            Title = "Location Detected",
                            Width = 300,
                            Height = 150,
                            Content = new StackPanel
                            {
                                Spacing = 10,
                                Margin = new Avalonia.Thickness(20),
                                Children =
                                {
                                    new TextBlock { Text = "✅ Location detected successfully!", FontWeight = Avalonia.Media.FontWeight.Bold },
                                    new TextBlock { Text = $"{detectedCity}, {detectedCountry}" },
                                    new TextBlock { Text = $"Coordinates: {detectedLat:F6}, {detectedLon:F6}" }
                                }
                            }
                        };
                        await successBox.ShowDialog(this);
                    }
                }
            }
            catch (Exception ex)
            {
                messageBox.Close();
                
                var errorBox = new Window
                {
                    Title = "Auto-Detect Failed",
                    Width = 300,
                    Height = 150,
                    Content = new StackPanel
                    {
                        Spacing = 10,
                        Margin = new Avalonia.Thickness(20),
                        Children =
                        {
                            new TextBlock { Text = "❌ Auto-detect failed", FontWeight = Avalonia.Media.FontWeight.Bold },
                            new TextBlock { Text = "Please select location manually" },
                            new TextBlock { Text = $"Error: {ex.Message}" }
                        }
                    }
                };
                await errorBox.ShowDialog(this);
            }
        }

        private void SaveAndClose()
        {
            SelectedCity = _cityComboBox.SelectedItem as string ?? "Mecca";
            SelectedCountry = _countryComboBox.SelectedItem as string ?? "Saudi Arabia";
            
            // Get Hijri adjustment from ComboBox
            var adjustmentIndex = _hijriAdjustmentComboBox.SelectedIndex;
            if (adjustmentIndex == 0)
                HijriAdjustment = -1;
            else if (adjustmentIndex == 2)
                HijriAdjustment = 1;
            else
                HijriAdjustment = 0;
            
            // Get calculation method from ComboBox
            var methodIndex = _calculationMethodComboBox.SelectedIndex;
            var methodIds = new[] { 0, 1, 2, 3, 4, 5 }; // Corresponding to the methods order
            if (methodIndex >= 0 && methodIndex < methodIds.Length)
                CalculationMethod = methodIds[methodIndex];
            
            // Get Asr method from ComboBox
            AsrMethod = _asrMethodComboBox.SelectedIndex; // 0 = Shafii, 1 = Hanafi
            
            Console.WriteLine($"[LocationSelection] SaveAndClose called:");
            Console.WriteLine($"  SelectedCity: {SelectedCity}");
            Console.WriteLine($"  SelectedCountry: {SelectedCountry}");
            Console.WriteLine($"  Coordinates: {Latitude:F6}, {Longitude:F6}");
            Console.WriteLine($"  HijriAdjustment: {HijriAdjustment}");
            Console.WriteLine($"  CalculationMethod: {CalculationMethod}");
            Console.WriteLine($"  CityComboBox SelectedIndex: {_cityComboBox.SelectedIndex}");
            Console.WriteLine($"  CountryComboBox SelectedIndex: {_countryComboBox.SelectedIndex}");
            
            Close(true); // Return true to indicate location was selected
        }
    }
}

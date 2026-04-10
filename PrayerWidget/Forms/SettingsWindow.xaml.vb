Imports System
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Threading
Imports System.Windows.Media
Imports WpfColor = System.Windows.Media.Color
Imports Salaty.First.Models
Imports Salaty.First.Services

Public Class SettingsWindow
    Inherits Window

    Private _settingsManager As SettingsManager
    Private _currentSettings As WidgetSettings
    Private _apiMethods As Dictionary(Of Integer, String)
    Private _apiService As PrayerApiService
    Private _analyticsService As GoogleAnalyticsService
    Private _isFirstRun As Boolean = False
    Private _isLoading As Boolean = True
    Private _countries As List(Of CountryInfo)
    Private _cities As List(Of CityInfo)
    Private _isAutoDetecting As Boolean = False

    ' Track original values for restart detection
    Private _originalCity As String = ""
    Private _originalCountry As String = ""
    Private _currentLanguage As String = ""

    ' Test variables
    Private _testPlayer As MediaPlayer
    Private _isTesting As Boolean = False
    Private _currentToast As ToastNotification = Nothing
    Private _testNotificationTimer As DispatcherTimer

    Public Sub New(settingsManager As SettingsManager, Optional isFirstRun As Boolean = False)
        InitializeComponent()
        _settingsManager = settingsManager
        _currentSettings = _settingsManager.LoadSettings()
        _apiService = New PrayerApiService()
        _analyticsService = New GoogleAnalyticsService()
        _isFirstRun = isFirstRun

        InitializeLocalization()
        PopulateSizeComboBox()
        PopulateSoundComboBox()
        PopulateLanguageComboBox()
        PopulateCalculationMethods()
        PopulateCountryComboBox()
        LoadMethodsFromAPIAsync()
        PopulateControls()

        ' Auto-detect location on first run only
        If _isFirstRun Then
            SetDefaultNotificationsActive()
            Dispatcher.BeginInvoke(New Func(Of Task)(AddressOf AutoDetectLocationAsync), System.Windows.Threading.DispatcherPriority.Background)
        End If

        _isLoading = False
    End Sub

    Private Sub SetDefaultNotificationsActive()
        ' Prayer reminders - all enabled by default
        ' Before: 10 minutes
        ' After: 15 minutes
        _settingsManager.SaveSetting("FajrReminder_EnabledBefore", "1")
        _settingsManager.SaveSetting("FajrReminder_MinutesBefore", "10")
        _settingsManager.SaveSetting("FajrReminder_EnabledAfter", "1")
        _settingsManager.SaveSetting("FajrReminder_MinutesAfter", "15")
        
        _settingsManager.SaveSetting("DhuhrReminder_EnabledBefore", "1")
        _settingsManager.SaveSetting("DhuhrReminder_MinutesBefore", "10")
        _settingsManager.SaveSetting("DhuhrReminder_EnabledAfter", "1")
        _settingsManager.SaveSetting("DhuhrReminder_MinutesAfter", "15")
        
        _settingsManager.SaveSetting("AsrReminder_EnabledBefore", "1")
        _settingsManager.SaveSetting("AsrReminder_MinutesBefore", "10")
        _settingsManager.SaveSetting("AsrReminder_EnabledAfter", "1")
        _settingsManager.SaveSetting("AsrReminder_MinutesAfter", "15")
        
        _settingsManager.SaveSetting("MaghribReminder_EnabledBefore", "1")
        _settingsManager.SaveSetting("MaghribReminder_MinutesBefore", "10")
        _settingsManager.SaveSetting("MaghribReminder_EnabledAfter", "1")
        _settingsManager.SaveSetting("MaghribReminder_MinutesAfter", "15")
        
        _settingsManager.SaveSetting("IshaReminder_EnabledBefore", "1")
        _settingsManager.SaveSetting("IshaReminder_MinutesBefore", "10")
        _settingsManager.SaveSetting("IshaReminder_EnabledAfter", "1")
        _settingsManager.SaveSetting("IshaReminder_MinutesAfter", "15")
        
        ' Start with Windows - enabled by default
        _settingsManager.SaveSetting("StartWithWindows", "1")
    End Sub

    Private Sub InitializeLocalization()
        ' Add localization code here if needed
    End Sub

    Private Sub PopulateSizeComboBox()
        ' Size items are already defined in XAML
        ' No need to populate in code-behind
    End Sub

    Private Sub PopulateCountryComboBox()
        ' Load countries from database
        Try
            Dim sqlService = New SqlitePrayerService(DatabaseManager.GetDatabasePath())
            _countries = sqlService.GetCountries()

            CmbCountry.Items.Clear()
            CmbCountry.DisplayMemberPath = "Name"
            For Each country In _countries
                CmbCountry.Items.Add(country)
            Next

            Console.WriteLine($"[Settings] Loaded {_countries.Count} countries")
        Catch ex As Exception
            Console.WriteLine($"[Settings] Error loading countries: {ex.Message}")
        End Try
    End Sub

    Private Sub PopulateCityComboBox(countryCode As String)
        ' Load cities for selected country
        Try
            CmbCity.Items.Clear()
            CmbCity.DisplayMemberPath = "Name"

            If String.IsNullOrEmpty(countryCode) Then
                Return
            End If

            Dim sqlService = New SqlitePrayerService(DatabaseManager.GetDatabasePath())
            _cities = sqlService.GetCitiesByCountry(countryCode)

            For Each city In _cities
                CmbCity.Items.Add(city)
            Next

            Console.WriteLine($"[Settings] Loaded {_cities.Count} cities for {countryCode}")
        Catch ex As Exception
            Console.WriteLine($"[Settings] Error loading cities: {ex.Message}")
        End Try
    End Sub

    Private Sub CmbCountry_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        Dim selectedCountry = TryCast(CmbCountry.SelectedItem, CountryInfo)
        If selectedCountry IsNot Nothing Then
            ' Populate cities for selected country
            PopulateCityComboBox(selectedCountry.Code)
            ' Clear city selection when country changes
            CmbCity.SelectedItem = Nothing
        End If
    End Sub

    Private Sub PopulateSoundComboBox()
        CmbSound.Items.Add(New ComboBoxItem With {.Content = "None", .Tag = "none"})
        CmbSound.Items.Add(New ComboBoxItem With {.Content = "Default", .Tag = "default"})
        CmbDuaaSound.Items.Add(New ComboBoxItem With {.Content = "None", .Tag = "none"})
        CmbDuaaSound.Items.Add(New ComboBoxItem With {.Content = "Default", .Tag = "default"})

        ' Load athan sound files from Resources/MP3 folder
        Dim mp3Folder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "MP3")
        If System.IO.Directory.Exists(mp3Folder) Then
            ' Load athan files for CmbSound
            Dim athanFiles = System.IO.Directory.GetFiles(mp3Folder, "*_athan.mp3")
            For Each file In athanFiles
                Dim fileName = System.IO.Path.GetFileNameWithoutExtension(file)
                Dim displayName = ToTitleCase(fileName.Replace("_athan", "").Replace("_", " "))
                CmbSound.Items.Add(New ComboBoxItem With {
                    .Content = displayName,
                    .Tag = fileName
                })
            Next

            ' Load duaa files for CmbDuaaSound
            Dim duaaFiles = System.IO.Directory.GetFiles(mp3Folder, "*_duaa.mp3")
            For Each file In duaaFiles
                Dim fileName = System.IO.Path.GetFileNameWithoutExtension(file)
                Dim displayName = ToTitleCase(fileName.Replace("_duaa", "").Replace("_", " "))
                CmbDuaaSound.Items.Add(New ComboBoxItem With {
                    .Content = displayName,
                    .Tag = fileName
                })
            Next
        End If

        ' Select current athan sound
        Dim currentSound = _currentSettings.AthanSound
        For Each item In CmbSound.Items
            Dim comboItem = CType(item, ComboBoxItem)
            If comboItem.Tag.ToString() = currentSound Then
                CmbSound.SelectedItem = comboItem
                Exit For
            End If
        Next
    End Sub

    Private Function ToTitleCase(str As String) As String
        Dim culture = System.Globalization.CultureInfo.CurrentCulture
        Return culture.TextInfo.ToTitleCase(str.ToLower())
    End Function

    Private Sub PopulateLanguageComboBox()
        CmbLanguage.Items.Add(New ComboBoxItem With {.Content = "English", .Tag = "en"})
        CmbLanguage.Items.Add(New ComboBoxItem With {.Content = "العربية", .Tag = "ar"})
        CmbLanguage.Items.Add(New ComboBoxItem With {.Content = "Français", .Tag = "fr"})
        CmbLanguage.Items.Add(New ComboBoxItem With {.Content = "Türkçe", .Tag = "tr"})
        CmbLanguage.Items.Add(New ComboBoxItem With {.Content = "اردو", .Tag = "ur"})

        ' Select current language
        Dim currentLang = _settingsManager.GetSetting("Language", "en")
        _currentLanguage = currentLang
        Console.WriteLine($"[Settings] Original language stored: {_currentLanguage}")
        
        For Each item In CmbLanguage.Items
            Dim comboItem = CType(item, ComboBoxItem)
            If comboItem.Tag.ToString() = currentLang Then
                CmbLanguage.SelectedItem = comboItem
                Exit For
            End If
        Next
    End Sub

    Private Sub PopulateCalculationMethods()
        Try
            Dim methods = _settingsManager.GetCalculationMethods()
            CmbMethod.Items.Clear()
            For Each method In methods
                CmbMethod.Items.Add(New ComboBoxItem With {
                    .Content = method.Name,
                    .Tag = method.Id.ToString()
                })
            Next
        Catch ex As Exception
            Console.WriteLine("Error populating methods: " & ex.Message)
        End Try
    End Sub

    Private Sub PopulateControls()
        Try
            ' Location tab - select country and city in ComboBoxes
            _originalCity = _currentSettings.City
            _originalCountry = _currentSettings.Country
            Console.WriteLine($"[Settings] Original location stored: {_originalCity}/{_originalCountry}")

            ' Select country
            If Not String.IsNullOrEmpty(_originalCountry) Then
                For i As Integer = 0 To CmbCountry.Items.Count - 1
                    Dim country = TryCast(CmbCountry.Items(i), CountryInfo)
                    If country IsNot Nothing AndAlso (country.Name.Equals(_originalCountry, StringComparison.OrdinalIgnoreCase) OrElse
                                                      country.Code.Equals(_originalCountry, StringComparison.OrdinalIgnoreCase)) Then
                        CmbCountry.SelectedItem = country
                        Exit For
                    End If
                Next
            End If

            ' Select city (after country is set, cities are loaded)
            If Not String.IsNullOrEmpty(_originalCity) Then
                For i As Integer = 0 To CmbCity.Items.Count - 1
                    Dim city = TryCast(CmbCity.Items(i), CityInfo)
                    If city IsNot Nothing AndAlso city.Name.Equals(_originalCity, StringComparison.OrdinalIgnoreCase) Then
                        CmbCity.SelectedItem = city
                        Exit For
                    End If
                Next
            End If

            ' Set calculation method
            Dim methodId = _currentSettings.Method.ToString()
            For Each item In CmbMethod.Items
                Dim comboItem = CType(item, ComboBoxItem)
                If comboItem.Tag.ToString() = methodId Then
                    CmbMethod.SelectedItem = comboItem
                    Exit For
                End If
            Next

            ' Set Asr method
            Dim asrMethodTag = _currentSettings.AsrMethod.ToString()
            For Each item In CmbAsrMethod.Items
                Dim comboItem = CType(item, ComboBoxItem)
                If comboItem.Tag.ToString() = asrMethodTag Then
                    CmbAsrMethod.SelectedItem = comboItem
                    Exit For
                End If
            Next

            ' Appearance tab
            ' Set size
            For Each item In CmbSize.Items
                Dim comboItem = CType(item, ComboBoxItem)
                If comboItem.Tag.ToString() = _currentSettings.Size Then
                    CmbSize.SelectedItem = comboItem
                    Exit For
                End If
            Next

            ' Widget opacity
            SldWidgetOpacity.Value = _currentSettings.WidgetOpacity
            LblWidgetOpacity.Text = CInt(_currentSettings.WidgetOpacity) & "%"

            ' Notification opacity
            SldNotificationOpacity.Value = _currentSettings.NotificationOpacity
            LblNotificationOpacity.Text = CInt(_currentSettings.NotificationOpacity) & "%"

            ' Hijri adjustment
            SldHijriAdjustment.Value = _currentSettings.HijriAdjustment
            LblHijriAdjustment.Text = _currentSettings.HijriAdjustment.ToString()

            ' Show progress ring
            Dim showProgressRingValue = _settingsManager.GetDoubleSetting("ShowProgressRing", 1)
            ChkShowProgressRing.IsChecked = (showProgressRingValue = 1)
            Console.WriteLine($"Populating ShowProgressRing: DB value={showProgressRingValue}, IsChecked={ChkShowProgressRing.IsChecked}")

            ' Show widget
            Dim showWidget = _settingsManager.GetSetting("ShowWidget", "1")
            ChkShowWidget.IsChecked = (showWidget = "1")

            ' Always on Top is always enabled (hardcoded, not in settings)
            ' No checkbox shown - it's always True

            ' Start with Windows
            Dim startWithWindows = _settingsManager.GetSetting("StartWithWindows", "1")
            ChkStartWithWindows.IsChecked = (startWithWindows = "1")

            ' Summer time
            ChkSummerTime.IsChecked = _currentSettings.IsSummerTime

            ' Analytics
            ChkEnableAnalytics.IsChecked = _analyticsService.IsEnabled()

            ' Notifications tab
            SldAthanVolume.Value = CDbl(_settingsManager.GetDoubleSetting("AthanVolume", 100))
            LblVolumeValue.Text = CInt(SldAthanVolume.Value) & "%"

            ChkEnableDuaaAfterAthan.IsChecked = _currentSettings.EnableDuaaAfterAthan

            ' Duaa sound - load saved setting or default to mashary_duaa
            Dim duaaSound = _settingsManager.GetSetting("DuaaSound", "mashary_duaa")
            Console.WriteLine($"Populating Duaa sound, saved value: {duaaSound}")
            For Each item In CmbDuaaSound.Items
                Dim comboItem = CType(item, ComboBoxItem)
                If comboItem.Tag.ToString() = duaaSound Then
                    CmbDuaaSound.SelectedItem = comboItem
                    Console.WriteLine($"Selected Duaa sound: {duaaSound}")
                    Exit For
                End If
            Next

            ' If nothing selected, default to mashary_duaa
            If CmbDuaaSound.SelectedItem Is Nothing Then
                For Each item In CmbDuaaSound.Items
                    Dim comboItem = CType(item, ComboBoxItem)
                    If comboItem.Tag.ToString() = "mashary_duaa" Then
                        CmbDuaaSound.SelectedItem = comboItem
                        Console.WriteLine("Default selected: mashary_duaa")
                        Exit For
                    End If
                Next
            End If

            ' Islamic Quotes tab
            ChkEnableQuotes.IsChecked = _currentSettings.EnableQuotes

            ' Set quote interval combo box
            Dim quoteInterval = _currentSettings.QuoteInterval.ToString()
            For Each item In CmbQuoteInterval.Items
                Dim comboItem = CType(item, ComboBoxItem)
                If comboItem.Tag.ToString() = quoteInterval Then
                    CmbQuoteInterval.SelectedItem = comboItem
                    Exit For
                End If
            Next

            ChkEnableQuoteAudio.IsChecked = _currentSettings.EnableQuoteAudio

            ' Prayer Reminders tab - populate all prayer reminders
            PopulatePrayerReminder("Fajr", CmbFajrBefore, CmbFajrAfter)
            PopulatePrayerReminder("Dhuhr", CmbDhuhrBefore, CmbDhuhrAfter)
            PopulatePrayerReminder("Asr", CmbAsrBefore, CmbAsrAfter)
            PopulatePrayerReminder("Maghrib", CmbMaghribBefore, CmbMaghribAfter)
            PopulatePrayerReminder("Isha", CmbIshaBefore, CmbIshaAfter)

            Console.WriteLine("Controls populated successfully")
        Catch ex As Exception
            Console.WriteLine("Error populating controls: " & ex.Message)
        End Try
    End Sub

    Private Sub PopulatePrayerReminder(prayerName As String,
                                       cmbBefore As ComboBox, cmbAfter As ComboBox)
        Try
            ' Before - use "None" as disabled indicator
            Dim enabledBefore = _settingsManager.GetSetting($"{prayerName}Reminder_EnabledBefore", "1")
            Dim minutesBefore = _settingsManager.GetSetting($"{prayerName}Reminder_MinutesBefore", "10")
            
            If enabledBefore <> "1" Then
                ' Set to None if disabled
                For Each item In cmbBefore.Items
                    Dim comboItem = CType(item, ComboBoxItem)
                    If comboItem.Tag.ToString() = "0" Then
                        cmbBefore.SelectedItem = comboItem
                        Exit For
                    End If
                Next
            Else
                ' Set to the saved minutes value
                For Each item In cmbBefore.Items
                    Dim comboItem = CType(item, ComboBoxItem)
                    If comboItem.Tag.ToString() = minutesBefore Then
                        cmbBefore.SelectedItem = comboItem
                        Exit For
                    End If
                Next
            End If

            ' After - use "None" as disabled indicator
            Dim enabledAfter = _settingsManager.GetSetting($"{prayerName}Reminder_EnabledAfter", "1")
            Dim minutesAfter = _settingsManager.GetSetting($"{prayerName}Reminder_MinutesAfter", "15")
            
            If enabledAfter <> "1" Then
                ' Set to None if disabled
                For Each item In cmbAfter.Items
                    Dim comboItem = CType(item, ComboBoxItem)
                    If comboItem.Tag.ToString() = "0" Then
                        cmbAfter.SelectedItem = comboItem
                        Exit For
                    End If
                Next
            Else
                ' Set to the saved minutes value
                For Each item In cmbAfter.Items
                    Dim comboItem = CType(item, ComboBoxItem)
                    If comboItem.Tag.ToString() = minutesAfter Then
                        cmbAfter.SelectedItem = comboItem
                        Exit For
                    End If
                Next
            End If
        Catch ex As Exception
            Console.WriteLine($"Error populating {prayerName} reminder: {ex.Message}")
        End Try
    End Sub

    Private Async Sub LoadMethodsFromAPIAsync()
        Try
            _apiMethods = Await _apiService.GetCalculationMethodsAsync()
        Catch ex As Exception
            Console.WriteLine("Error loading calculation methods: " & ex.Message)
        End Try
    End Sub

    Private Async Function AutoDetectLocationAsync() As Task
        If _isAutoDetecting Then Return
        _isAutoDetecting = True

        Console.WriteLine("AutoDetectLocation: Starting IP-based location detection...")

        Try
            Dim city = Await _apiService.GetLocationFromIPAsync().ConfigureAwait(False)
            If city IsNot Nothing Then
                Application.Current.Dispatcher.Invoke(Sub()
                    ' Select country
                    SelectCountryByCode(city.CountryCode)

                    ' Select city
                    SelectCityByName(city.Name)

                    LblGeoStatus.Text = $"✓ Detected: {city.Name}"
                    LblGeoStatus.Foreground = New SolidColorBrush(Colors.LightGreen)
                    Console.WriteLine($"AutoDetectLocation: Detected {city.Name}")
                End Sub)
            Else
                Application.Current.Dispatcher.Invoke(Sub()
                    LblGeoStatus.Text = "Location not detected (check internet connection)"
                    LblGeoStatus.Foreground = New SolidColorBrush(Colors.Orange)
                End Sub)
            End If
        Catch ex As Exception
            Application.Current.Dispatcher.Invoke(Sub()
                LblGeoStatus.Text = "Auto-detect error: " & ex.Message
                LblGeoStatus.Foreground = New SolidColorBrush(Colors.Red)
                Console.WriteLine($"AutoDetectLocation: Error - {ex.Message}")
            End Sub)
        Finally
            _isAutoDetecting = False
        End Try
    End Function

    Private Sub SelectCountryByCode(countryCode As String)
        For i As Integer = 0 To CmbCountry.Items.Count - 1
            Dim country = TryCast(CmbCountry.Items(i), CountryInfo)
            If country IsNot Nothing AndAlso country.Code.Equals(countryCode, StringComparison.OrdinalIgnoreCase) Then
                CmbCountry.SelectedItem = country
                Return
            End If
        Next
    End Sub

    Private Sub SelectCityByName(cityName As String)
        For i As Integer = 0 To CmbCity.Items.Count - 1
            Dim city = TryCast(CmbCity.Items(i), CityInfo)
            If city IsNot Nothing AndAlso city.Name.Equals(cityName, StringComparison.OrdinalIgnoreCase) Then
                CmbCity.SelectedItem = city
                Return
            End If
        Next
    End Sub

    Private Function GetSelectedCityName() As String
        Dim selectedCity = TryCast(CmbCity.SelectedItem, CityInfo)
        If selectedCity IsNot Nothing Then
            Return selectedCity.Name
        End If
        ' If user typed custom text
        Return CmbCity.Text.Trim()
    End Function

    Private Function GetSelectedCountryName() As String
        Dim selectedCountry = TryCast(CmbCountry.SelectedItem, CountryInfo)
        If selectedCountry IsNot Nothing Then
            Return selectedCountry.Name
        End If
        ' If user typed custom text
        Return CmbCountry.Text.Trim()
    End Function

    Private Function GetSelectedCountryCode() As String
        Dim selectedCountry = TryCast(CmbCountry.SelectedItem, CountryInfo)
        If selectedCountry IsNot Nothing Then
            Return selectedCountry.Code
        End If
        Return ""
    End Function

    Private Function GetSelectedCityInfo() As CityInfo
        Return TryCast(CmbCity.SelectedItem, CityInfo)
    End Function

    ' Window drag handler
    Private Sub Window_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        If e.ChangedButton = MouseButton.Left Then
            Me.DragMove()
        End If
    End Sub

    ' Location detection buttons
    Private Async Sub BtnIP_Click(sender As Object, e As RoutedEventArgs)
        Console.WriteLine("[Location] BtnIP_Click - Starting IP location detection")
        BtnIP.IsEnabled = False
        BtnIP.Content = "Detecting..."
        LblGeoStatus.Text = "Detecting location..."
        LblGeoStatus.Foreground = New SolidColorBrush(WpfColor.FromRgb(79, 172, 254))

        Try
            Dim city = Await _apiService.GetLocationFromIPAsync().ConfigureAwait(False)

            Application.Current.Dispatcher.Invoke(Sub()
                If city IsNot Nothing Then
                    SelectCountryByCode(city.CountryCode)
                    SelectCityByName(city.Name)
                    LblGeoStatus.Text = $"✓ Found: {city.Name}"
                    LblGeoStatus.Foreground = New SolidColorBrush(Colors.LightGreen)
                    Console.WriteLine($"[Location] Success: {city.Name}")
                Else
                    LblGeoStatus.Text = "No location found"
                    LblGeoStatus.Foreground = New SolidColorBrush(Colors.Orange)
                    Console.WriteLine("[Location] No city returned from API")
                End If
                BtnIP.IsEnabled = True
                BtnIP.Content = "📍 Detect My Location"
            End Sub)
        Catch ex As Exception
            Application.Current.Dispatcher.Invoke(Sub()
                LblGeoStatus.Text = "Error: " & ex.Message
                LblGeoStatus.Foreground = New SolidColorBrush(Colors.Red)
                BtnIP.IsEnabled = True
                BtnIP.Content = "📍 Detect My Location"
                Console.WriteLine($"[Location] Exception: {ex.Message}")
            End Sub)
        End Try
    End Sub

    ' Size combo box handler
    Private Sub CmbSize_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If _isLoading Then Return
        If CmbSize.SelectedItem Is Nothing Then Return

        Dim selectedItem = CType(CmbSize.SelectedItem, ComboBoxItem)
        Dim sizeName = selectedItem.Tag.ToString()
        
        ' Get the full size preset with all dimensions
        Dim sizePreset = WidgetSettings.GetSizePreset(sizeName)
        
        ' Update all size-related properties
        _currentSettings.Size = sizePreset.Size
        _currentSettings.WindowWidth = sizePreset.WindowWidth
        _currentSettings.WindowHeight = sizePreset.WindowHeight
        _currentSettings.CornerRadius = sizePreset.CornerRadius
        _currentSettings.GridMargin = sizePreset.GridMargin
        _currentSettings.StrokeThickness = sizePreset.StrokeThickness
        
        Console.WriteLine($"Size changed to: {sizeName} ({sizePreset.WindowWidth}x{sizePreset.WindowHeight})")
    End Sub

    ' Language combo box handler
    Private Sub CmbLanguage_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If _isLoading Then Return
        If CmbLanguage.SelectedItem Is Nothing Then Return

        ' Save language preference immediately when changed
        Dim selectedLang = CType(CmbLanguage.SelectedItem, ComboBoxItem).Tag.ToString()
        LocalizationService.SetLanguage(selectedLang)
        Console.WriteLine($"[Settings] Language changed and saved: {selectedLang}")
    End Sub

    ' Opacity sliders
    Private Sub SldWidgetOpacity_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If _isLoading Then Return
        _currentSettings.WidgetOpacity = SldWidgetOpacity.Value
    End Sub

    Private Sub SldNotificationOpacity_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If _isLoading Then Return
        _currentSettings.NotificationOpacity = SldNotificationOpacity.Value
    End Sub

    ' Hijri adjustment buttons (+/-)
    Private Sub BtnHijriAdjustment_Click(sender As Object, e As RoutedEventArgs)
        Try
            Dim btn = CType(sender, Button)
            Dim tag = btn.Tag.ToString()

            If tag = "-1" Then
                ' Minus button
                If SldHijriAdjustment.Value > -1 Then
                    SldHijriAdjustment.Value = SldHijriAdjustment.Value - 1
                End If
            ElseIf tag = "1" Then
                ' Plus button
                If SldHijriAdjustment.Value < 1 Then
                    SldHijriAdjustment.Value = SldHijriAdjustment.Value + 1
                End If
            End If

            ' Update label and settings
            LblHijriAdjustment.Text = CInt(SldHijriAdjustment.Value).ToString()
            _currentSettings.HijriAdjustment = CInt(SldHijriAdjustment.Value)

            Console.WriteLine($"Hijri adjustment changed to: {_currentSettings.HijriAdjustment}")
        Catch ex As Exception
            Console.WriteLine("Error in BtnHijriAdjustment_Click: " & ex.Message)
        End Try
    End Sub

    Private Sub SldHijriAdjustment_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If _isLoading Then Return
        _currentSettings.HijriAdjustment = CInt(SldHijriAdjustment.Value)
        LblHijriAdjustment.Text = CInt(SldHijriAdjustment.Value).ToString()
    End Sub

    ' Test sound button
    Private Sub BtnTestSound_Click(sender As Object, e As RoutedEventArgs)
        If _isTesting Then
            StopTestSound()
            Return
        End If

        If CmbSound.SelectedItem Is Nothing Then
            MessageBox.Show("Please select an athan sound first.", "Test Sound", MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If

        Dim selectedItem = CType(CmbSound.SelectedItem, ComboBoxItem)
        Dim soundTag = selectedItem.Tag.ToString()

        If soundTag = "none" Then
            MessageBox.Show("Cannot test 'None' - select an actual athan sound.", "Test Sound", MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If

        BtnTestSound.IsEnabled = False
        Dim countdown As Integer = 10
        BtnTestSound.Content = countdown & "s..."

        _testNotificationTimer = New DispatcherTimer()
        _testNotificationTimer.Interval = TimeSpan.FromSeconds(1)

        AddHandler _testNotificationTimer.Tick, Sub(s, args)
                                                    countdown -= 1
                                                    If countdown > 0 Then
                                                        BtnTestSound.Content = countdown & "s..."
                                                    Else
                                                        _testNotificationTimer.Stop()
                                                        BtnTestSound.Content = "Now..."
                                                        ShowTestNotification()

                                                        Dim resetTimer As New DispatcherTimer()
                                                        resetTimer.Interval = TimeSpan.FromSeconds(3)
                                                        AddHandler resetTimer.Tick, Sub(s2, args2)
                                                                                        resetTimer.Stop()
                                                                                        BtnTestSound.IsEnabled = True
                                                                                        BtnTestSound.Content = "Test in 10s"
                                                                                    End Sub
                                                        resetTimer.Start()
                                                    End If
                                                End Sub

        _testNotificationTimer.Start()
    End Sub

    Private Sub StopTestSound()
        If _testPlayer IsNot Nothing Then
            _testPlayer.Stop()
            _testPlayer = Nothing
        End If
        _isTesting = False
        BtnTestSound.Content = "Test in 10s"
        BtnTestSound.IsEnabled = True
    End Sub

    Private Sub ShowTestNotification()
        Try
            Dim soundTag As String = "default"
            If CmbSound.SelectedItem IsNot Nothing Then
                soundTag = CType(CmbSound.SelectedItem, ComboBoxItem).Tag.ToString()
            End If
            If soundTag = "none" Then soundTag = "default"

            Console.WriteLine($"[TestSound] Selected sound tag: {soundTag}")
            
            Dim soundPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "MP3", soundTag & ".mp3")
            Console.WriteLine($"[TestSound] Looking for sound at: {soundPath}")
            Console.WriteLine($"[TestSound] File exists: {System.IO.File.Exists(soundPath)}")
            
            If Not System.IO.File.Exists(soundPath) Then
                Console.WriteLine("[TestSound] Primary sound file not found, searching for fallback...")
                Dim mp3Folder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "MP3")
                Console.WriteLine($"[TestSound] Searching in folder: {mp3Folder}")
                
                If System.IO.Directory.Exists(mp3Folder) Then
                    Dim athanFiles = System.IO.Directory.GetFiles(mp3Folder, "*_athan*.mp3")
                    Console.WriteLine($"[TestSound] Found {athanFiles.Length} athan files")
                    If athanFiles.Length > 0 Then
                        soundPath = athanFiles(0)
                        Console.WriteLine($"[TestSound] Using fallback: {soundPath}")
                    End If
                Else
                    Console.WriteLine("[TestSound] MP3 folder does not exist!")
                End If
            End If

            Dim volume As Double = SldAthanVolume.Value
            Console.WriteLine($"[TestSound] Volume: {volume}%")

            If System.IO.File.Exists(soundPath) Then
                Console.WriteLine("[TestSound] Creating toast notification with sound...")
                Dim toast As New ToastNotification("Test Prayer Reminder", "This is a test notification.", 95, _settingsManager, soundPath, volume)
                
                Dim screenWidth As Double = SystemParameters.WorkArea.Width + SystemParameters.WorkArea.Left
                Dim screenHeight As Double = SystemParameters.WorkArea.Height + SystemParameters.WorkArea.Top
                Dim taskbarHeight As Double = SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Height
                toast.Left = screenWidth - toast.Width - 10
                toast.Top = screenHeight - toast.Height - taskbarHeight - 10
                toast.Show()
                Console.WriteLine("[TestSound] Toast notification shown successfully")
            Else
                Console.WriteLine("[TestSound] No sound file available - showing toast without sound")
                Dim toast As New ToastNotification("Test Prayer Reminder", "This is a test notification (no sound).", 95, _settingsManager, Nothing, volume)
                
                Dim screenWidth As Double = SystemParameters.WorkArea.Width + SystemParameters.WorkArea.Left
                Dim screenHeight As Double = SystemParameters.WorkArea.Height + SystemParameters.WorkArea.Top
                Dim taskbarHeight As Double = SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Height
                toast.Left = screenWidth - toast.Width - 10
                toast.Top = screenHeight - toast.Height - taskbarHeight - 10
                toast.Show()
            End If
            
        Catch ex As Exception
            Console.WriteLine("[TestSound] Error: " & ex.Message)
            Console.WriteLine("[TestSound] Stack trace: " & ex.StackTrace)
            MessageBox.Show("Error showing test notification: " & ex.Message, "Test Reminder", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    ' Athan volume
    Private Sub SldAthanVolume_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If _isLoading Then Return
        LblVolumeValue.Text = CInt(SldAthanVolume.Value) & "%"
    End Sub

    ' Duaa after athan
    Private Sub ChkEnableDuaaAfterAthan_Checked(sender As Object, e As RoutedEventArgs)
        If _isLoading Then Return
        _currentSettings.EnableDuaaAfterAthan = True
    End Sub

    Private Sub ChkEnableDuaaAfterAthan_Unchecked(sender As Object, e As RoutedEventArgs)
        If _isLoading Then Return
        _currentSettings.EnableDuaaAfterAthan = False
    End Sub

    Private Sub BtnPlayDuaa_Click(sender As Object, e As RoutedEventArgs)
        ' Test duaa playback
        Try
            Dim duaaSound = _settingsManager.GetSetting("DuaaSound", "mashary_duaa")
            If String.IsNullOrEmpty(duaaSound) OrElse duaaSound = "default" Then
                duaaSound = GetFirstDuaaSound()
            End If
            If duaaSound = "none" Then
                MessageBox.Show("No duaa sound selected.", "Test Duaa", MessageBoxButton.OK, MessageBoxImage.Information)
                Return
            End If

            Dim duaaPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "MP3", duaaSound & ".mp3")

            If Not System.IO.File.Exists(duaaPath) Then
                MessageBox.Show($"Duaa file not found: {duaaPath}", "Test Duaa", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            If _testPlayer IsNot Nothing Then
                _testPlayer.Stop()
                _testPlayer = Nothing
            End If

            _testPlayer = New System.Windows.Media.MediaPlayer()
            _testPlayer.Open(New Uri(duaaPath))

            Dim volume As Double = CDbl(_settingsManager.GetDoubleSetting("AthanVolume", 100))
            _testPlayer.Volume = volume / 100.0

            AddHandler _testPlayer.MediaEnded, Sub(s, args)
                _testPlayer?.Stop()
                _testPlayer = Nothing
            End Sub

            AddHandler _testPlayer.MediaFailed, Sub(s, args)
                Console.WriteLine($"[TestDuaa] MediaFailed: {duaaPath}")
            End Sub

            _testPlayer.Play()
            Console.WriteLine($"[TestDuaa] Playing: {duaaPath}")
        Catch ex As Exception
            MessageBox.Show("Error playing duaa: " & ex.Message, "Test Duaa", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Function GetFirstDuaaSound() As String
        Try
            Dim mp3Folder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "MP3")
            If System.IO.Directory.Exists(mp3Folder) Then
                Dim files = System.IO.Directory.GetFiles(mp3Folder, "*_duaa.mp3")
                If files.Length > 0 Then
                    Array.Sort(files)
                    Return System.IO.Path.GetFileNameWithoutExtension(files(0))
                End If
            End If
        Catch
        End Try
        Return "mashary_duaa"
    End Function

    ' Test reminder buttons
    Private Sub BtnTestNow_Click(sender As Object, e As RoutedEventArgs)
        ShowTestReminderNotification()
    End Sub

    Private Sub ShowTestReminderNotification()
        Try
            Dim soundTag As String = "default"
            If CmbSound.SelectedItem IsNot Nothing Then
                soundTag = CType(CmbSound.SelectedItem, ComboBoxItem).Tag.ToString()
            End If
            If soundTag = "none" Then soundTag = "default"

            Dim soundPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "MP3", soundTag & ".mp3")
            If Not System.IO.File.Exists(soundPath) Then
                Dim mp3Folder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "MP3")
                Dim athanFiles = System.IO.Directory.GetFiles(mp3Folder, "*_athan*.mp3")
                If athanFiles.Length > 0 Then soundPath = athanFiles(0)
            End If

            Dim volume As Double = SldAthanVolume.Value

            Dim toast As New ToastNotification("Test Prayer Reminder", "This is a test notification.", 95, _settingsManager, If(System.IO.File.Exists(soundPath), soundPath, Nothing), volume)

            Dim screenWidth As Double = SystemParameters.WorkArea.Width + SystemParameters.WorkArea.Left
            Dim screenHeight As Double = SystemParameters.WorkArea.Height + SystemParameters.WorkArea.Top
            Dim taskbarHeight As Double = SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Height
            toast.Left = screenWidth - toast.Width - 10
            toast.Top = screenHeight - toast.Height - taskbarHeight - 10
            toast.Show()
        Catch ex As Exception
            MessageBox.Show("Error showing test notification: " & ex.Message, "Test Reminder", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    ' Quote test button
    Private Async Sub BtnTestQuote_Click(sender As Object, e As RoutedEventArgs)
        Try
            Console.WriteLine("[QuoteTest] === Starting quote test ===")
            Dim quoteService As New QuoteService(Nothing)
            Console.WriteLine("[QuoteTest] QuoteService created, fetching quote...")
            Dim quote = Await quoteService.GetRandomQuote()
            Console.WriteLine($"[QuoteTest] GetRandomQuote returned: {If(quote IsNot Nothing, "SUCCESS", "NOTHING")}")
            If quote IsNot Nothing Then
                Console.WriteLine($"[QuoteTest] Quote text: {quote.quote.Substring(0, Math.Min(50, quote.quote.Length))}...")
                Dim quoteWindow As New QuoteNotificationWindow(quote, 15, True)
                quoteWindow.Owner = Me
                quoteWindow.Show()
            Else
                MessageBox.Show("Failed to fetch quote from API. Check console output for details.", "Quote Test", MessageBoxButton.OK, MessageBoxImage.Warning)
            End If
        Catch ex As Exception
            Console.WriteLine($"[QuoteTest] Exception: {ex.GetType().Name}: {ex.Message}")
            Console.WriteLine($"[QuoteTest] StackTrace: {ex.StackTrace}")
            MessageBox.Show("Error: " & ex.Message & vbCrLf & vbCrLf & ex.StackTrace, "Quote Test", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    ' Save and Cancel buttons
    Private Sub BtnSave_Click(sender As Object, e As RoutedEventArgs)
        Console.WriteLine("[Settings] Save button clicked - saving settings...")
        SaveSettings()
        
        Console.WriteLine("[Settings] Settings saved, checking if restart needed...")
        
        ' Check if location was changed (requires restart)
        Dim newCity = GetSelectedCityName()
        Dim newCountry = GetSelectedCountryName()
        Dim locationChanged As Boolean = False
        If _originalCity <> newCity OrElse _originalCountry <> newCountry Then
            locationChanged = True
            Console.WriteLine($"[Settings] Location changed: {_originalCity}/{_originalCountry} -> {newCity}/{newCountry}")
        End If
        
        ' Check if language was changed (requires restart)
        Dim languageChanged = False
        If CmbLanguage.SelectedItem IsNot Nothing Then
            Dim selectedLang = CType(CmbLanguage.SelectedItem, ComboBoxItem).Tag.ToString()
            If selectedLang <> _currentLanguage Then
                languageChanged = True
                Console.WriteLine($"[Settings] Language changed: {_currentLanguage} -> {selectedLang}")
            End If
        End If
        
        Me.DialogResult = True
        Me.Close()

        ' Restart if location or language changed
        If locationChanged OrElse languageChanged Then
            ' Force garbage collect to release all file handles
            GC.Collect()
            GC.WaitForPendingFinalizers()
            GC.Collect()

            ' Give a small delay for file handles to release
            System.Threading.Thread.Sleep(500)

            Console.WriteLine("[Settings] Restart required - restarting application...")

            ' Start new process with same exe path
            Dim appPath = Application.ResourceAssembly.Location
            Dim psi As New System.Diagnostics.ProcessStartInfo With {
                .FileName = appPath,
                .UseShellExecute = True
            }
            System.Diagnostics.Process.Start(psi)

            ' Shutdown current app cleanly
            Application.Current.Shutdown()
        Else
            Console.WriteLine("[Settings] No restart needed")
        End If
    End Sub

    Private Sub BtnCancel_Click(sender As Object, e As RoutedEventArgs)
        Me.DialogResult = False
        Me.Close()
    End Sub

    Private Sub SaveSettings()
        Console.WriteLine("=== SAVE SETTINGS CALLED ===")

        _currentSettings.WidgetOpacity = SldWidgetOpacity.Value
        _currentSettings.NotificationOpacity = SldNotificationOpacity.Value
        _currentSettings.HijriAdjustment = CInt(SldHijriAdjustment.Value)
        _currentSettings.ShowProgressRing = ChkShowProgressRing.IsChecked = True
        ' Always on Top is always enabled - not saved to settings
        _currentSettings.HasCompletedSetup = True
        
        ' Save City and Country from ComboBox selections
        _currentSettings.City = GetSelectedCityName()
        _currentSettings.Country = GetSelectedCountryName()
        Console.WriteLine($"  City={_currentSettings.City}")
        Console.WriteLine($"  Country={_currentSettings.Country}")

        ' Save Size from CmbSize
        If CmbSize.SelectedItem IsNot Nothing Then
            _currentSettings.Size = CType(CmbSize.SelectedItem, ComboBoxItem).Tag.ToString()
            Console.WriteLine($"  Size={_currentSettings.Size}")
        End If

        ' Save AthanSound from CmbSound
        If CmbSound.SelectedItem IsNot Nothing Then
            _currentSettings.AthanSound = CType(CmbSound.SelectedItem, ComboBoxItem).Tag.ToString()
            Console.WriteLine($"  AthanSound={_currentSettings.AthanSound}")
        End If

        Console.WriteLine($"  ShowProgressRing={_currentSettings.ShowProgressRing}")
        Console.WriteLine($"  WidgetOpacity={_currentSettings.WidgetOpacity}")
        Console.WriteLine($"  DuaaSound={If(CmbDuaaSound.SelectedItem IsNot Nothing, CType(CmbDuaaSound.SelectedItem, ComboBoxItem).Tag.ToString(), "NULL")}")

        _settingsManager.SaveSettings(_currentSettings)

        Dim showWidgetValue As String = If(ChkShowWidget.IsChecked, "1", "0")
        _settingsManager.SaveSetting("ShowWidget", showWidgetValue)
        Console.WriteLine($"  ShowWidget={showWidgetValue}")

        ' Start with Windows
        Dim startWithWindowsValue As String = If(ChkStartWithWindows.IsChecked, "1", "0")
        _settingsManager.SaveSetting("StartWithWindows", startWithWindowsValue)
        Console.WriteLine($"  StartWithWindows={startWithWindowsValue}")

        _analyticsService.SetEnabled(ChkEnableAnalytics.IsChecked)

        Dim volumeValue As String = CInt(SldAthanVolume.Value).ToString()
        _settingsManager.SaveSetting("AthanVolume", volumeValue)

        _settingsManager.SaveSetting("EnableDuaaAfterAthan", If(ChkEnableDuaaAfterAthan.IsChecked, "1", "0"))
        Console.WriteLine($"  EnableDuaaAfterAthan={If(ChkEnableDuaaAfterAthan.IsChecked, "1", "0")}")

        If CmbDuaaSound.SelectedItem IsNot Nothing Then
            Dim duaaItem = CType(CmbDuaaSound.SelectedItem, ComboBoxItem)
            _settingsManager.SaveSetting("DuaaSound", duaaItem.Tag.ToString())
            Console.WriteLine($"  DuaaSound saved={duaaItem.Tag.ToString()}")
        End If

        SaveReminderSettings()
        SaveQuoteSettings()

        Console.WriteLine("Settings saved successfully")
        Console.WriteLine("===========================")
    End Sub

    Private Sub SaveReminderSettings()
        ' Save all prayer reminder settings
        SavePrayerReminder("Fajr", CmbFajrBefore, CmbFajrAfter)
        SavePrayerReminder("Dhuhr", CmbDhuhrBefore, CmbDhuhrAfter)
        SavePrayerReminder("Asr", CmbAsrBefore, CmbAsrAfter)
        SavePrayerReminder("Maghrib", CmbMaghribBefore, CmbMaghribAfter)
        SavePrayerReminder("Isha", CmbIshaBefore, CmbIshaAfter)
    End Sub

    Private Sub SavePrayerReminder(prayerName As String,
                                   cmbBefore As ComboBox, cmbAfter As ComboBox)
        ' Save Before - use "None" (Tag="0") as disabled indicator
        Dim enabledBefore As String = "1"
        Dim minutesBefore As String = "10"
        
        If cmbBefore.SelectedItem IsNot Nothing Then
            Dim comboItem = CType(cmbBefore.SelectedItem, ComboBoxItem)
            minutesBefore = comboItem.Tag.ToString()
            If comboItem.Tag.ToString() = "0" Then
                enabledBefore = "0"  ' Disabled when None is selected
            End If
        End If
        
        _settingsManager.SaveSetting($"{prayerName}Reminder_EnabledBefore", enabledBefore)
        _settingsManager.SaveSetting($"{prayerName}Reminder_MinutesBefore", minutesBefore)

        ' Save After - use "None" (Tag="0") as disabled indicator
        Dim enabledAfter As String = "1"
        Dim minutesAfter As String = "15"
        
        If cmbAfter.SelectedItem IsNot Nothing Then
            Dim comboItem = CType(cmbAfter.SelectedItem, ComboBoxItem)
            minutesAfter = comboItem.Tag.ToString()
            If comboItem.Tag.ToString() = "0" Then
                enabledAfter = "0"  ' Disabled when None is selected
            End If
        End If
        
        _settingsManager.SaveSetting($"{prayerName}Reminder_EnabledAfter", enabledAfter)
        _settingsManager.SaveSetting($"{prayerName}Reminder_MinutesAfter", minutesAfter)
    End Sub

    Private Sub SaveQuoteSettings()
        ' Save quote settings using model properties
        _currentSettings.EnableQuotes = ChkEnableQuotes.IsChecked
        _currentSettings.QuoteInterval = CInt(CType(CmbQuoteInterval.SelectedItem, ComboBoxItem).Tag.ToString())
        _currentSettings.EnableQuoteAudio = ChkEnableQuoteAudio.IsChecked
        
        ' Save to database
        _settingsManager.SaveSetting("EnableQuotes", If(_currentSettings.EnableQuotes, "1", "0"))
        _settingsManager.SaveSetting("QuoteInterval", _currentSettings.QuoteInterval.ToString())
        _settingsManager.SaveSetting("EnableQuoteAudio", If(_currentSettings.EnableQuoteAudio, "1", "0"))
        
        Console.WriteLine($"[SaveQuoteSettings] EnableQuotes={_currentSettings.EnableQuotes}, Interval={_currentSettings.QuoteInterval}, Audio={_currentSettings.EnableQuoteAudio}")
    End Sub

End Class

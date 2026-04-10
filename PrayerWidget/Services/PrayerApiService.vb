Imports System.Threading.Tasks
Imports Salaty.First.Models
Imports Salaty.First.Services
Imports System.IO

Public Class PrayerApiService
    Private ReadOnly _sqliteService As SqlitePrayerService
    Private ReadOnly _prayTime As PrayTime
    Private ReadOnly _geocodingService As GeocodingService

    Public Sub New()
        ' Use AppData path for write access (avoids permission issues in Program Files)
        Dim dbPath = DatabaseManager.GetDatabasePath()
        _sqliteService = New SqlitePrayerService(dbPath)
        _prayTime = New PrayTime()
        _geocodingService = New GeocodingService(_sqliteService)
    End Sub

    ''' <summary>
    ''' Find city by address using geocoding
    ''' </summary>
    Public Async Function FindCityByAddressAsync(address As String) As Task(Of CityInfo)
        Return Await _geocodingService.FindCityByAddressAsync(address)
    End Function

    ''' <summary>
    ''' Get location from IP address
    ''' </summary>
    Public Async Function GetLocationFromIPAsync() As Task(Of CityInfo)
        Return Await _geocodingService.GetLocationFromIPAsync()
    End Function

    ''' <summary>
    ''' Get country name from code
    ''' </summary>
    Public Function GetCountryName(countryCode As String) As String
        Return _geocodingService.GetCountryName(countryCode)
    End Function

    Public Function GetCalculationMethodsAsync() As Task(Of Dictionary(Of Integer, String))
        Dim methods As New Dictionary(Of Integer, String)()
        Dim dbMethods = _sqliteService.GetCalculationMethods()
        For Each method In dbMethods
            methods(method.Id) = method.Name
        Next
        Return Task.FromResult(methods)
    End Function

    Public Function GetPrayerTimesAsync(city As String, country As String, method As Integer, Optional isSummerTime As Boolean = False, Optional asrMethod As Integer = 0) As Task(Of DailyPrayerTimes)
        Try
            _prayTime.SetCalcMethod(method)
            _prayTime.SetAsrMethod(asrMethod)
            _prayTime.SetHighLatsMethod(PrayTime.MidNight)

            Dim coords = GetCityCoordinates(city, country)
            Console.WriteLine("=== PRAYER TIMES CALCULATION ===")
            Console.WriteLine($"City: {city}, Country: {country}, Method: {method}, Summer Time: {isSummerTime}")
            Console.WriteLine($"Coordinates: Lat={coords.lat}, Lng={coords.lng}, TZ={coords.timezone}")
            Console.WriteLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")

            Dim times = _prayTime.GetPrayerTimes(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, coords.lat, coords.lng, coords.timezone)

            Console.WriteLine($"Raw Times - Fajr: {times(0)}, Sunrise: {times(1)}, Dhuhr: {times(2)}, Asr: {times(3)}, Maghrib: {times(5)}, Isha: {times(6)}")

            Dim prayerTimes As New DailyPrayerTimes
            prayerTimes.Fajr = ParseTime(times(0))
            prayerTimes.Sunrise = ParseTime(times(1))
            prayerTimes.Dhuhr = ParseTime(times(2))
            prayerTimes.Asr = ParseTime(times(3))
            prayerTimes.Maghrib = ParseTime(times(5))
            prayerTimes.Isha = ParseTime(times(6))

            If isSummerTime Then
                Console.WriteLine("Applying Summer Time adjustment: +1 hour")
                prayerTimes.Fajr = prayerTimes.Fajr.AddHours(1)
                prayerTimes.Sunrise = prayerTimes.Sunrise.AddHours(1)
                prayerTimes.Dhuhr = prayerTimes.Dhuhr.AddHours(1)
                prayerTimes.Asr = prayerTimes.Asr.AddHours(1)
                prayerTimes.Maghrib = prayerTimes.Maghrib.AddHours(1)
                prayerTimes.Isha = prayerTimes.Isha.AddHours(1)
            End If

            prayerTimes = DetermineNextAndPreviousPrayer(prayerTimes)

            Console.WriteLine($"Parsed Times - Fajr: {prayerTimes.Fajr:HH:mm:ss}, Dhuhr: {prayerTimes.Dhuhr:HH:mm:ss}, Asr: {prayerTimes.Asr:HH:mm:ss}, Maghrib: {prayerTimes.Maghrib:HH:mm:ss}, Isha: {prayerTimes.Isha:HH:mm:ss}")
            Console.WriteLine($"Next Prayer: {prayerTimes.NextPrayer} at {prayerTimes.NextPrayerTime:HH:mm:ss}")
            Console.WriteLine($"Previous Prayer: {prayerTimes.PreviousPrayer} at {prayerTimes.PreviousPrayerTime:HH:mm:ss}")
            Console.WriteLine("=================================" & vbCrLf)

            Return Task.FromResult(prayerTimes)
        Catch ex As Exception
            Console.WriteLine("Calculation Error: " & ex.Message)
            Console.WriteLine(ex.StackTrace)
            Return Task.FromResult(GetDefaultPrayerTimes())
        End Try
    End Function

    Public Function GetCitiesByCountry(countryCode As String) As List(Of CityInfo)
        Return _sqliteService.GetCitiesByCountry(countryCode)
    End Function

    Public Function GetCountries() As List(Of CountryInfo)
        Return _sqliteService.GetCountries()
    End Function

    Public Function SearchCities(searchTerm As String) As List(Of CityInfo)
        Return _sqliteService.SearchCities(searchTerm)
    End Function

    Public Function GetUserSettings() As UserSettings
        Return _sqliteService.GetSettings()
    End Function

    Public Sub SaveUserSettings(settings As UserSettings)
        _sqliteService.SaveSettings(settings)
    End Sub

    Public Function GetAthanSounds() As List(Of String)
        Return _sqliteService.GetAthanSounds()
    End Function

    Private Function ParseTime(timeStr As String) As DateTime
        Try
            If String.IsNullOrEmpty(timeStr) OrElse timeStr = "----" Then
                Return DateTime.Now.Date.AddHours(12)
            End If
            Dim parts = timeStr.Split(":"c)
            Dim hour = Integer.Parse(parts(0))
            Dim minute = Integer.Parse(parts(1))
            ' Create time using DateTime.Now.Date to ensure consistent timezone handling
            Return DateTime.Now.Date.AddHours(hour).AddMinutes(minute)
        Catch
            Return DateTime.Now.Date.AddHours(12)
        End Try
    End Function

    Private Function GetCityCoordinates(city As String, country As String) As (lat As Double, lng As Double, timezone As Integer)
        Console.WriteLine($"[GetCityCoordinates] Searching for city: '{city}' (length={city.Length}) in country: '{country}'")
        Console.WriteLine($"[GetCityCoordinates] City bytes: {String.Join(",", System.Text.Encoding.UTF8.GetBytes(city))}")

        Dim cities = _sqliteService.SearchCities(city)
        Console.WriteLine($"[GetCityCoordinates] Found {cities.Count} cities")

        If cities.Count > 0 Then
            Dim foundCity = cities(0)
            Console.WriteLine($"[GetCityCoordinates] Using city: {foundCity.Name}, Lat={foundCity.Latitude}, Lng={foundCity.Longitude}, TZ={foundCity.Timezone}")
            Return (foundCity.Latitude, foundCity.Longitude, foundCity.Timezone)
        End If

        Console.WriteLine("[GetCityCoordinates] City not found in database, using hardcoded coordinates")
        Dim hardcoded = GetHardcodedCoordinates(city)
        Console.WriteLine($"[GetCityCoordinates] Hardcoded: Lat={hardcoded.lat}, Lng={hardcoded.lng}, TZ={hardcoded.timezone}")
        Return hardcoded
    End Function

    Private Function GetHardcodedCoordinates(city As String) As (lat As Double, lng As Double, timezone As Integer)
        Dim cityKey = city.ToLower().Trim()
        Select Case cityKey
            Case "london" : Return (51.5074, -0.1278, 0)
            Case "new york" : Return (40.7128, -74.006, -5)
            Case "makkah", "mecca" : Return (21.4225, 39.8262, 3)
            Case "madinah", "medina" : Return (24.5247, 39.5692, 3)
            Case "dubai" : Return (25.2048, 55.2708, 4)
            Case "cairo" : Return (30.0444, 31.2357, 2)
            Case "istanbul" : Return (41.0082, 28.9784, 3)
            Case "karachi" : Return (24.8607, 67.0011, 5)
            Case "lahore" : Return (31.5204, 74.3587, 5)
            Case "dhaka" : Return (23.8103, 90.4125, 6)
            Case "jakarta" : Return (-6.2088, 106.8456, 7)
            Case "kuala lumpur" : Return (3.139, 101.6869, 8)
            Case "paris" : Return (48.8566, 2.3522, 1)
            Case "berlin" : Return (52.52, 13.405, 1)
            Case "toronto" : Return (43.6532, -79.3832, -5)
            Case Else : Return (21.4225, 39.8262, 3)
        End Select
    End Function

    Private Function DetermineNextAndPreviousPrayer(prayerTimes As DailyPrayerTimes) As DailyPrayerTimes
        Dim now = DateTime.Now
        Console.WriteLine($"[DetermineNextAndPreviousPrayer] Current time: {now:HH:mm:ss}")
        Console.WriteLine($"[DetermineNextAndPreviousPrayer] Fajr: {prayerTimes.Fajr:HH:mm:ss}, Dhuhr: {prayerTimes.Dhuhr:HH:mm:ss}, Asr: {prayerTimes.Asr:HH:mm:ss}, Maghrib: {prayerTimes.Maghrib:HH:mm:ss}, Isha: {prayerTimes.Isha:HH:mm:ss}")
        
        Dim prayers As New List(Of PrayerTimeInfo)
        prayers.Add(New PrayerTimeInfo With {.Name = "Fajr", .Time = prayerTimes.Fajr})
        prayers.Add(New PrayerTimeInfo With {.Name = "Dhuhr", .Time = prayerTimes.Dhuhr})
        prayers.Add(New PrayerTimeInfo With {.Name = "Asr", .Time = prayerTimes.Asr})
        prayers.Add(New PrayerTimeInfo With {.Name = "Maghrib", .Time = prayerTimes.Maghrib})
        prayers.Add(New PrayerTimeInfo With {.Name = "Isha", .Time = prayerTimes.Isha})

        Dim nextPrayer = prayers.FirstOrDefault(Function(p) p.Time > now)
        If nextPrayer Is Nothing Then
            prayerTimes.NextPrayer = "Fajr"
            prayerTimes.NextPrayerTime = prayerTimes.Fajr.AddDays(1)
        Else
            prayerTimes.NextPrayer = nextPrayer.Name
            prayerTimes.NextPrayerTime = nextPrayer.Time
        End If

        Dim previousPrayer = prayers.LastOrDefault(Function(p) p.Time <= now)
        If previousPrayer Is Nothing Then
            ' Before Fajr: previous prayer is Isha from yesterday
            prayerTimes.PreviousPrayer = "Isha"
            prayerTimes.PreviousPrayerTime = prayerTimes.Isha.AddDays(-1)
            Console.WriteLine($"[DetermineNextAndPreviousPrayer] Before Fajr - Previous: {prayerTimes.PreviousPrayer} at {prayerTimes.PreviousPrayerTime:HH:mm:ss}")
        Else
            prayerTimes.PreviousPrayer = previousPrayer.Name
            prayerTimes.PreviousPrayerTime = previousPrayer.Time
            Console.WriteLine($"[DetermineNextAndPreviousPrayer] Previous: {prayerTimes.PreviousPrayer} at {prayerTimes.PreviousPrayerTime:HH:mm:ss}")
        End If
        
        Console.WriteLine($"[DetermineNextAndPreviousPrayer] Next: {prayerTimes.NextPrayer} at {prayerTimes.NextPrayerTime:HH:mm:ss}")

        Return prayerTimes
    End Function

    Private Function GetDefaultPrayerTimes() As DailyPrayerTimes
        Dim today = DateTime.Today
        Return New DailyPrayerTimes With {
            .Fajr = today.AddHours(5),
            .Dhuhr = today.AddHours(12),
            .Asr = today.AddHours(15),
            .Maghrib = today.AddHours(18),
            .Isha = today.AddHours(20),
            .NextPrayer = "Asr",
            .NextPrayerTime = DateTime.Now.AddHours(2),
            .PreviousPrayer = "Dhuhr",
            .PreviousPrayerTime = DateTime.Now.AddHours(-2)
        }
    End Function
End Class

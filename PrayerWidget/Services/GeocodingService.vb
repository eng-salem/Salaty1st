Imports System
Imports System.Net.Http
Imports System.Threading.Tasks
Imports System.IO

Namespace Services
    Public Class GeocodingService
        Private ReadOnly _sqliteService As SqlitePrayerService
        Private ReadOnly _httpClient As HttpClient

        Public Sub New(sqliteService As SqlitePrayerService)
            _sqliteService = sqliteService
            _httpClient = New HttpClient()
            _httpClient.Timeout = TimeSpan.FromSeconds(5) ' Reduced timeout to 5 seconds
        End Sub

        ''' <summary>
        ''' Get location from IP address using multiple APIs with fallback
        ''' </summary>
        Public Async Function GetLocationFromIPAsync() As Task(Of CityInfo)
            Dim cts As New Threading.CancellationTokenSource()
            cts.CancelAfter(TimeSpan.FromSeconds(5)) ' 5 second timeout

            Try
                ' Try primary API: ipwhois.app
                Dim city = Await GetLocationFromIPWhoisAsync(cts.Token)
                If city IsNot Nothing Then
                    Console.WriteLine("[Geocoding] Success with ipwhois.app")
                    Return city
                End If
            Catch ex As Exception
                Console.WriteLine("[Geocoding] ipwhois.app failed: " & ex.Message)
            End Try

            Try
                ' Fallback API: ipapi.co
                Dim city = Await GetLocationFromIPApiAsync(cts.Token)
                If city IsNot Nothing Then
                    Console.WriteLine("[Geocoding] Success with ipapi.co")
                    Return city
                End If
            Catch ex As Exception
                Console.WriteLine("[Geocoding] ipapi.co failed: " & ex.Message)
            End Try

            Console.WriteLine("[Geocoding] All APIs failed")
            Return Nothing
        End Function

        ''' <summary>
        ''' Get location from ipwhois.app
        ''' </summary>
        Private Async Function GetLocationFromIPWhoisAsync(cancellationToken As Threading.CancellationToken) As Task(Of CityInfo)
            Dim url = "https://ipwhois.app/json/?fields=latitude,longitude,city,country,country_code,country_capital"
            Console.WriteLine($"[Geocoding] Trying ipwhois.app: {url}")

            Try
                Using cts As New Threading.CancellationTokenSource()
                    cts.CancelAfter(TimeSpan.FromSeconds(5))
                    Dim response = Await _httpClient.GetStringAsync(url)
                    cancellationToken.ThrowIfCancellationRequested()
                    Console.WriteLine($"[Geocoding] ipwhois.app response: {response.Length} chars")

                    ' Parse JSON manually
                    Dim lat As Double = ParseJsonDouble(response, "latitude")
                    Dim lon As Double = ParseJsonDouble(response, "longitude")
                    Dim capital As String = ParseJsonString(response, "country_capital")
                    Dim countryCode As String = ParseJsonString(response, "country_code")

                    Console.WriteLine($"[Geocoding] ipwhois.app result: {capital}, {countryCode} ({lat}, {lon})")

                    Return FindNearestCity(lat, lon, cityName:=capital, countryCode:=countryCode)
                End Using
            Catch ex As Exception
                Console.WriteLine($"[Geocoding] ipwhois.app error: {ex.Message}")
                Throw
            End Try
        End Function

        ''' <summary>
        ''' Get location from ipapi.co (fallback)
        ''' </summary>
        Private Async Function GetLocationFromIPApiAsync(cancellationToken As Threading.CancellationToken) As Task(Of CityInfo)
            Dim url = "https://ipapi.co/json/"
            Console.WriteLine($"[Geocoding] Trying ipapi.co: {url}")

            Try
                Using cts As New Threading.CancellationTokenSource()
                    cts.CancelAfter(TimeSpan.FromSeconds(5))
                    Dim response = Await _httpClient.GetStringAsync(url)
                    cancellationToken.ThrowIfCancellationRequested()
                    Console.WriteLine($"[Geocoding] ipapi.co response: {response.Length} chars")

                    ' Parse JSON manually
                    Dim lat As Double = ParseJsonDouble(response, "latitude")
                    Dim lon As Double = ParseJsonDouble(response, "longitude")
                    Dim city As String = ParseJsonString(response, "city")
                    Dim country As String = ParseJsonString(response, "country_name")
                    Dim countryCode As String = ParseJsonString(response, "country_code")

                    Console.WriteLine($"[Geocoding] ipapi.co result: {city}, {country} ({lat}, {lon})")

                    Return FindNearestCity(lat, lon, cityName:=city, countryCode:=countryCode)
                End Using
            Catch ex As Exception
                Console.WriteLine($"[Geocoding] ipapi.co error: {ex.Message}")
                Throw
            End Try
        End Function

        ''' <summary>
        ''' Parse a double value from JSON string
        ''' </summary>
        Private Function ParseJsonDouble(json As String, propertyName As String) As Double
            Try
                ' Simple JSON parser for flat objects
                Dim searchKey = """" & propertyName & """:"
                Dim startIndex = json.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase)
                If startIndex < 0 Then Return 0

                startIndex += searchKey.Length
                ' Skip whitespace
                While startIndex < json.Length AndAlso Char.IsWhiteSpace(json(startIndex))
                    startIndex += 1
                End While

                ' Find end of value (comma, brace, or end of string)
                Dim endIndex = startIndex
                While endIndex < json.Length AndAlso json(endIndex) <> ","c AndAlso json(endIndex) <> "}"c
                    endIndex += 1
                End While

                Dim valueStr = json.Substring(startIndex, endIndex - startIndex).Trim()
                Dim result As Double
                If Double.TryParse(valueStr, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture, result) Then
                    Return result
                End If
            Catch ex As Exception
                Console.WriteLine("ParseJsonDouble error for " & propertyName & ": " & ex.Message)
            End Try
            Return 0
        End Function

        ''' <summary>
        ''' Parse a string value from JSON
        ''' </summary>
        Private Function ParseJsonString(json As String, propertyName As String) As String
            Try
                Dim quote As Char = Chr(34)
                Dim searchKey = """" & propertyName & """:"
                Dim startIndex = json.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase)
                If startIndex < 0 Then Return Nothing

                startIndex += searchKey.Length
                ' Skip whitespace
                While startIndex < json.Length AndAlso Char.IsWhiteSpace(json(startIndex))
                    startIndex += 1
                End While

                ' Value should be quoted string
                If json(startIndex) <> quote Then Return Nothing
                startIndex += 1

                ' Find end quote
                Dim endIndex = startIndex
                While endIndex < json.Length AndAlso json(endIndex) <> quote
                    endIndex += 1
                End While

                Return json.Substring(startIndex, endIndex - startIndex)
            Catch ex As Exception
                Console.WriteLine("ParseJsonString error for " & propertyName & ": " & ex.Message)
            End Try
            Return Nothing
        End Function

        ''' <summary>
        ''' Geocode an address string and find the nearest city from database
        ''' </summary>
        Public Async Function FindCityByAddressAsync(address As String) As Task(Of CityInfo)
            Try
                ' Use Nominatim (OpenStreetMap) free geocoding API
                Dim encodedAddress = Uri.EscapeDataString(address)
                Dim url = $"https://nominatim.openstreetmap.org/search?format=json&q={encodedAddress}&limit=5"

                ' Add required user agent header for Nominatim
                _httpClient.DefaultRequestHeaders.Clear()
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "PrayerWidget/1.0")
                Console.WriteLine("Geocoding url: " & url)

                Dim response = Await _httpClient.GetStringAsync(url)
                Console.WriteLine("Geocoding Response: " & response)

                ' Parse JSON array manually for better .NET Framework 4.8 compatibility
                Dim lat As Double = ParseJsonArrayDouble(response, "lat")
                Dim lon As Double = ParseJsonArrayDouble(response, "lon")

                If lat <> 0 OrElse lon <> 0 Then
                    Console.WriteLine($"Geocoded Location: {lat}, {lon}")
                    ' Find nearest city from database
                    Return FindNearestCity(lat, lon)
                End If

                Console.WriteLine("No results found for address: " & address)
                Return Nothing

            Catch ex As Exception
                Console.WriteLine("Geocoding error: " & ex.Message)
                Console.WriteLine(ex.StackTrace)
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Parse a double value from JSON array (first element)
        ''' </summary>
        Private Function ParseJsonArrayDouble(json As String, propertyName As String) As Double
            Try
                ' Find first object in array and extract property
                Dim searchKey = """" & propertyName & """:"
                Dim startIndex = json.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase)
                If startIndex < 0 Then Return 0

                startIndex += searchKey.Length
                ' Skip whitespace
                While startIndex < json.Length AndAlso Char.IsWhiteSpace(json(startIndex))
                    startIndex += 1
                End While

                ' Handle quoted string values (Nominatim returns lat/lon as strings)
                Dim isQuoted As Boolean = (json(startIndex) = """"c)
                If isQuoted Then startIndex += 1

                ' Find end of value
                Dim endIndex = startIndex
                Dim endChar As Char = If(isQuoted, """"c, ","c)
                While endIndex < json.Length AndAlso json(endIndex) <> endChar AndAlso json(endIndex) <> "}"c
                    endIndex += 1
                End While

                Dim valueLength = If(isQuoted, endIndex - startIndex, endIndex - startIndex)
                Dim valueStr = json.Substring(startIndex, valueLength).Trim()

                ' Remove quotes if present
                valueStr = valueStr.Trim("""")

                Dim result As Double
                If Double.TryParse(valueStr, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture, result) Then
                    Return result
                End If
            Catch ex As Exception
                Console.WriteLine("ParseJsonArrayDouble error for " & propertyName & ": " & ex.Message)
            End Try
            Return 0
        End Function

        ''' <summary>
        ''' Find nearest city from database using city name, country code, or coordinates
        ''' </summary>
        Public Function FindNearestCity(latitude As Double, longitude As Double, Optional cityName As String = Nothing, Optional countryCode As String = Nothing) As CityInfo
            Try
                ' Use sqlite service to find by city name, country code, or coordinates
                Dim nearestCity = _sqliteService.FindNearestCity(latitude, longitude, 500, cityName, countryCode)

                If nearestCity IsNot Nothing Then
                    Console.WriteLine($"Found city: {nearestCity.Name}, {nearestCity.CountryCode}")
                    Return nearestCity
                End If

                Console.WriteLine("No city found")
                Return Nothing

            Catch ex As Exception
                Console.WriteLine("Error finding nearest city: " & ex.Message)
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Calculate distance between two coordinates using Haversine formula
        ''' </summary>
        Private Function CalculateDistance(lat1 As Double, lon1 As Double, lat2 As Double, lon2 As Double) As Double
            Const R As Double = 6371 ' Earth's radius in kilometers

            Dim dLat = ToRadians(lat2 - lat1)
            Dim dLon = ToRadians(lon2 - lon1)

            Dim a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2)

            Dim c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a))

            Return R * c
        End Function

        Private Function ToRadians(degrees As Double) As Double
            Return degrees * Math.PI / 180
        End Function

        ''' <summary>
        ''' Get country name from code
        ''' </summary>
        Public Function GetCountryName(countryCode As String) As String
            Dim countries = _sqliteService.GetCountries()
            Dim country = countries.FirstOrDefault(Function(c) c.Code.ToUpper() = countryCode.ToUpper())
            Return If(country IsNot Nothing, country.Name, countryCode)
        End Function
    End Class
End Namespace

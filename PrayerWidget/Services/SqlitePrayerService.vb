Imports System
Imports System.Collections.Generic
Imports System.IO
Imports Microsoft.Data.Sqlite

Namespace Services
    Public Class SqlitePrayerService
        Private ReadOnly _connectionString As String
        Private ReadOnly _databasePath As String

        Public Sub New(databasePath As String)
            _databasePath = databasePath
            _connectionString = $"Data Source={databasePath}"
        End Sub

        Public Function GetConnection() As SqliteConnection
            Return New SqliteConnection(_connectionString)
        End Function

        Public Function DatabaseExists() As Boolean
            Return File.Exists(_databasePath)
        End Function

        ' Get all countries (for settings window)
        Public Function GetCountries() As List(Of CountryInfo)
            Dim countries As New List(Of CountryInfo)()
            If Not DatabaseExists() Then Return countries

            Using connection As New SqliteConnection(_connectionString)
                connection.Open()
                Dim cmd As New SqliteCommand("SELECT Country_Code, Country_Name, Method, AdjustHighLats FROM Countries ORDER BY Country_Name;", connection)
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        countries.Add(New CountryInfo With {
                            .Code = reader.GetString(0),
                            .Name = reader.GetString(1),
                            .DefaultMethod = If(reader.IsDBNull(2), 1, reader.GetInt32(2)),
                            .AdjustHighLats = If(reader.IsDBNull(3), 0, reader.GetInt32(3))
                        })
                    End While
                End Using
            End Using
            Return countries
        End Function

        ' Get cities by country code (for settings window)
        Public Function GetCitiesByCountry(countryCode As String) As List(Of CityInfo)
            Dim cities As New List(Of CityInfo)()
            If Not DatabaseExists() Then Return cities

            Using connection As New SqliteConnection(_connectionString)
                connection.Open()
                Dim cmd As New SqliteCommand("SELECT City_ID, City_Name, La, Lo, Country_Code, Tz FROM Cities WHERE Country_Code = @code ORDER BY City_Name;", connection)
                cmd.Parameters.AddWithValue("@code", countryCode)
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        cities.Add(New CityInfo With {
                            .Id = reader.GetInt32(0),
                            .Name = reader.GetString(1),
                            .Latitude = reader.GetDouble(2),
                            .Longitude = reader.GetDouble(3),
                            .CountryCode = reader.GetString(4),
                            .Timezone = If(reader.IsDBNull(5), 0, reader.GetInt32(5))
                        })
                    End While
                End Using
            End Using
            Return cities
        End Function

        ' Search cities by name (for settings window)
        Public Function SearchCities(searchTerm As String) As List(Of CityInfo)
            Dim cities As New List(Of CityInfo)()
            If Not DatabaseExists() Then Return cities

            Using connection As New SqliteConnection(_connectionString)
                connection.Open()
                Dim cmd As New SqliteCommand("SELECT City_ID, City_Name, La, Lo, Country_Code, Tz FROM Cities WHERE City_Name LIKE @term LIMIT 50;", connection)
                cmd.Parameters.AddWithValue("@term", "%" & searchTerm & "%")
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        cities.Add(New CityInfo With {
                            .Id = reader.GetInt32(0),
                            .Name = reader.GetString(1),
                            .Latitude = reader.GetDouble(2),
                            .Longitude = reader.GetDouble(3),
                            .CountryCode = reader.GetString(4),
                            .Timezone = If(reader.IsDBNull(5), 0, reader.GetInt32(5))
                        })
                    End While
                End Using
            End Using
            Return cities
        End Function

        ' Find nearest city by city name, country code, or coordinates
        Public Function FindNearestCity(latitude As Double, longitude As Double, maxDistanceKm As Double, Optional cityName As String = Nothing, Optional countryCode As String = Nothing) As CityInfo
            If Not DatabaseExists() Then Return Nothing

            Using connection As New SqliteConnection(_connectionString)
                connection.Open()
                connection.DefaultTimeout = 10

                ' 1. Search by city name AND country code together (most accurate)
                If Not String.IsNullOrEmpty(cityName) AndAlso Not String.IsNullOrEmpty(countryCode) Then
                    Console.WriteLine($"Searching by city + country: {cityName}, {countryCode}")
                    Dim cityCmd As New SqliteCommand(
                        "SELECT City_ID, City_Name, La, Lo, Country_Code, Tz FROM Cities " &
                        "WHERE City_Name LIKE @city AND Country_Code = @country LIMIT 1;", connection)
                    cityCmd.Parameters.AddWithValue("@city", "%" & cityName & "%")
                    cityCmd.Parameters.AddWithValue("@country", countryCode.ToUpper())

                    Using reader = cityCmd.ExecuteReader()
                        If reader.Read() Then
                            Return New CityInfo With {
                                .Id = reader.GetInt32(0),
                                .Name = reader.GetString(1),
                                .Latitude = reader.GetDouble(2),
                                .Longitude = reader.GetDouble(3),
                                .CountryCode = reader.GetString(4),
                                .Timezone = If(reader.IsDBNull(5), 0, reader.GetInt32(5))
                            }
                        End If
                    End Using
                End If

                ' 2. Search by country code only
                If Not String.IsNullOrEmpty(countryCode) Then
                    Dim countryCmd As New SqliteCommand(
                        "SELECT City_ID, City_Name, La, Lo, Country_Code, Tz FROM Cities " &
                        "WHERE Country_Code = @country LIMIT 1;", connection)
                    countryCmd.Parameters.AddWithValue("@country", countryCode.ToUpper())

                    Using reader = countryCmd.ExecuteReader()
                        If reader.Read() Then
                            Return New CityInfo With {
                                .Id = reader.GetInt32(0),
                                .Name = reader.GetString(1),
                                .Latitude = reader.GetDouble(2),
                                .Longitude = reader.GetDouble(3),
                                .CountryCode = reader.GetString(4),
                                .Timezone = If(reader.IsDBNull(5), 0, reader.GetInt32(5))
                            }
                        End If
                    End Using
                End If

                ' 3. Fallback: coordinate search (cast La/Lo to REAL for numeric comparison)
                Dim lat2 = Math.Round(latitude, 2)
                Dim lon2 = Math.Round(longitude, 2)
                Dim tol As Double = 0.5

                Dim coordCmd As New SqliteCommand(
                    "SELECT City_ID, City_Name, La, Lo, Country_Code, Tz FROM Cities " &
                    "WHERE CAST(La AS REAL) >= @minLat AND CAST(La AS REAL) <= @maxLat " &
                    "AND CAST(Lo AS REAL) >= @minLon AND CAST(Lo AS REAL) <= @maxLon " &
                    "ORDER BY ABS(CAST(La AS REAL) - @lat) + ABS(CAST(Lo AS REAL) - @lon) " &
                    "LIMIT 1;", connection)

                coordCmd.Parameters.AddWithValue("@minLat", lat2 - tol)
                coordCmd.Parameters.AddWithValue("@maxLat", lat2 + tol)
                coordCmd.Parameters.AddWithValue("@minLon", lon2 - tol)
                coordCmd.Parameters.AddWithValue("@maxLon", lon2 + tol)
                coordCmd.Parameters.AddWithValue("@lat", lat2)
                coordCmd.Parameters.AddWithValue("@lon", lon2)

                Using reader = coordCmd.ExecuteReader()
                    If reader.Read() Then
                        Return New CityInfo With {
                            .Id = reader.GetInt32(0),
                            .Name = reader.GetString(1),
                            .Latitude = reader.GetDouble(2),
                            .Longitude = reader.GetDouble(3),
                            .CountryCode = reader.GetString(4),
                            .Timezone = If(reader.IsDBNull(5), 0, reader.GetInt32(5))
                        }
                    End If
                End Using
            End Using

            Return Nothing
        End Function

        ' Get first city as fallback
        Private Function GetFirstCity() As CityInfo
            Try
                Using connection As New SqliteConnection(_connectionString)
                    connection.Open()
                    Dim cmd As New SqliteCommand("SELECT City_ID, City_Name, La, Lo, Country_Code, Tz FROM Cities LIMIT 1;", connection)
                    Using reader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Return New CityInfo With {
                                .Id = reader.GetInt32(0),
                                .Name = reader.GetString(1),
                                .Latitude = reader.GetDouble(2),
                                .Longitude = reader.GetDouble(3),
                                .CountryCode = reader.GetString(4),
                                .Timezone = If(reader.IsDBNull(5), 0, reader.GetInt32(5))
                            }
                        End If
                    End Using
                End Using
            Catch
            End Try
            Return Nothing
        End Function

        ' Get city by ID
        Public Function GetCityById(cityId As Integer) As CityInfo
            If Not DatabaseExists() Then Return Nothing

            Using connection As New SqliteConnection(_connectionString)
                connection.Open()
                ' Use Tz column (integer offset) instead of Time_Zone (string)
                Dim cmd As New SqliteCommand("SELECT City_ID, City_Name, La, Lo, Country_Code, Tz FROM Cities WHERE City_ID = @id;", connection)
                cmd.Parameters.AddWithValue("@id", cityId)
                Using reader = cmd.ExecuteReader()
                    If reader.Read() Then
                        ' Tz column is integer (column index 5)
                        Dim tzOffset = If(reader.IsDBNull(5), 0, reader.GetInt32(5))
                        Return New CityInfo With {
                            .Id = reader.GetInt32(0),
                            .Name = reader.GetString(1),
                            .Latitude = reader.GetDouble(2),
                            .Longitude = reader.GetDouble(3),
                            .CountryCode = reader.GetString(4),
                            .Timezone = tzOffset
                        }
                    End If
                End Using
            End Using

            Return Nothing
        End Function

        ' Get calculation methods
        Public Function GetCalculationMethods() As List(Of MethodInfo)
            Dim methods As New List(Of MethodInfo)()

            If Not DatabaseExists() Then Return GetDefaultMethods()

            Using connection As New SqliteConnection(_connectionString)
                connection.Open()
                Dim cmd As New SqliteCommand("SELECT ID, Name FROM Methods ORDER BY ID;", connection)
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        methods.Add(New MethodInfo With {
                            .Id = reader.GetInt32(0),
                            .Name = reader.GetString(1)
                        })
                    End While
                End Using
            End Using

            Return methods
        End Function

        ' Get user settings
        Public Function GetSettings() As UserSettings
            Dim settings As New UserSettings()

            If Not DatabaseExists() Then Return settings

            Using connection As New SqliteConnection(_connectionString)
                connection.Open()
                Dim cmd As New SqliteCommand("SELECT Name, Value FROM settings;", connection)
                Using reader = cmd.ExecuteReader()
                    Console.WriteLine("=== DATABASE SETTINGS ===")
                    While reader.Read()
                        Dim name = reader.GetString(0)
                        Dim value = reader.GetValue(1)
                        Console.WriteLine($"  {name} = {value}")

                        Select Case name
                            Case "City_ID"
                                ' Handle both integer ID and string city name
                                If Not Integer.TryParse(value.ToString(), Nothing) Then
                                    settings.CityName = value.ToString()
                                Else
                                    settings.CityId = Convert.ToInt32(value)
                                End If
                            Case "Country_Code", "Country_Name"
                                settings.CountryCode = value.ToString()
                            Case "Method"
                                settings.Method = Convert.ToInt32(value)
                            Case "City_Name"
                                settings.CityName = value.ToString()
                            Case "La"
                                settings.Latitude = Convert.ToDouble(value)
                            Case "Lo"
                                settings.Longitude = Convert.ToDouble(value)
                            Case "AsrMethod"
                                settings.AsrMethod = Convert.ToInt32(value)
                            Case "Summer", "IsSummerTime"
                                settings.IsSummer = Convert.ToInt32(value) = 1
                            Case "WidgetOpacity"
                                settings.WidgetOpacity = CDbl(value)
                            Case "NotificationOpacity"
                                settings.NotificationOpacity = CDbl(value)
                            Case "ShowProgressRing"
                                settings.ShowProgressRing = Convert.ToInt32(value) = 1
                            Case "HijriAdjustment"
                                settings.HijriAdjustment = Convert.ToInt32(value)
                            Case "AthanSound"
                                settings.AthanSound = value.ToString()
                            Case "EnableDuaaAfterAthan"
                                settings.EnableDuaaAfterAthan = Convert.ToInt32(value) = 1
                            Case "DuaaSound"
                                settings.DuaaSound = value.ToString()
                            Case "Size"
                                settings.Size = value.ToString()
                            Case "CornerRadius"
                                settings.CornerRadius = CDbl(value)
                            Case "GridMargin"
                                settings.GridMargin = Convert.ToInt32(value)
                            Case "StrokeThickness"
                                settings.StrokeThickness = CDbl(value)
                            Case "WindowWidth"
                                settings.WindowWidth = Convert.ToInt32(value)
                            Case "WindowHeight"
                                settings.WindowHeight = Convert.ToInt32(value)
                            Case "HasCompletedSetup"
                                settings.HasCompletedSetup = Convert.ToInt32(value) = 1
                        End Select
                    End While
                End Using
            End Using

            Return settings
        End Function

        ' Save user settings
        Public Sub SaveSettings(settings As UserSettings)
            If Not DatabaseExists() Then Return

            Using connection As New SqliteConnection(_connectionString)
                connection.Open()
                Dim transaction = connection.BeginTransaction()

                Try
                    ' Delete existing settings
                    Dim deleteCmd As New SqliteCommand("DELETE FROM settings;", connection, transaction)
                    deleteCmd.ExecuteNonQuery()

                    ' Insert new settings
                    Dim insertCmd As New SqliteCommand(
                        "INSERT INTO settings (Name, Value) VALUES (@name, @value);", connection, transaction)

                    insertCmd.Parameters.AddWithValue("@name", "City_ID")
                    insertCmd.Parameters.AddWithValue("@value", settings.CityId)
                    insertCmd.ExecuteNonQuery()

                    insertCmd.Parameters.Clear()
                    insertCmd.Parameters.AddWithValue("@name", "Country_Code")
                    insertCmd.Parameters.AddWithValue("@value", settings.CountryCode)
                    insertCmd.ExecuteNonQuery()

                    insertCmd.Parameters.Clear()
                    insertCmd.Parameters.AddWithValue("@name", "Method")
                    insertCmd.Parameters.AddWithValue("@value", settings.Method)
                    insertCmd.ExecuteNonQuery()

                    insertCmd.Parameters.Clear()
                    insertCmd.Parameters.AddWithValue("@name", "City_Name")
                    insertCmd.Parameters.AddWithValue("@value", settings.CityName)
                    insertCmd.ExecuteNonQuery()

                    insertCmd.Parameters.Clear()
                    insertCmd.Parameters.AddWithValue("@name", "La")
                    insertCmd.Parameters.AddWithValue("@value", settings.Latitude)
                    insertCmd.ExecuteNonQuery()

                    insertCmd.Parameters.Clear()
                    insertCmd.Parameters.AddWithValue("@name", "Lo")
                    insertCmd.Parameters.AddWithValue("@value", settings.Longitude)
                    insertCmd.ExecuteNonQuery()

                    insertCmd.Parameters.Clear()
                    insertCmd.Parameters.AddWithValue("@name", "AsrMethod")
                    insertCmd.Parameters.AddWithValue("@value", settings.AsrMethod)
                    insertCmd.ExecuteNonQuery()

                    transaction.Commit()
                Catch ex As Exception
                    transaction.Rollback()
                    Throw ex
                End Try
            End Using
        End Sub

        ' Get athan sounds
        Public Function GetAthanSounds() As List(Of String)
            Dim sounds As New List(Of String)()

            If Not DatabaseExists() Then Return sounds

            Using connection As New SqliteConnection(_connectionString)
                connection.Open()
                Dim cmd As New SqliteCommand("SELECT Name FROM Notification_Snds;", connection)
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        sounds.Add(reader.GetString(0))
                    End While
                End Using
            End Using

            Return sounds
        End Function

        Private Function GetDefaultMethods() As List(Of MethodInfo)
            Return New List(Of MethodInfo) From {
                New MethodInfo With {.Id = 0, .Name = "الجعفري"},
                New MethodInfo With {.Id = 1, .Name = "رابطة العالم الإسلامي"},
                New MethodInfo With {.Id = 5, .Name = "الهيئة المصرية العامة للمساحة"},
                New MethodInfo With {.Id = 3, .Name = "جامعة العلوم الإسلامية بكراتشي"},
                New MethodInfo With {.Id = 4, .Name = "تقويم أم القرى"},
                New MethodInfo With {.Id = 2, .Name = "الإتحاد الإسلامي بأمريكا الشمالية"}
            }
        End Function
    End Class

    Public Class CityInfo
        Public Property Id As Integer
        Public Property Name As String
        Public Property Latitude As Double
        Public Property Longitude As Double
        Public Property CountryCode As String
        Public Property Timezone As Integer
    End Class

    Public Class CountryInfo
        Public Property Code As String
        Public Property Name As String
        Public Property DefaultMethod As Integer
        Public Property AdjustHighLats As Integer
    End Class

    Public Class MethodInfo
        Public Property Id As Integer
        Public Property Name As String
    End Class

    Public Class UserSettings
        Public Property CityId As Integer = 5
        Public Property CountryCode As String = "EG"
        Public Property Method As Integer = 2
        Public Property CityName As String = "Cairo"
        Public Property Latitude As Double = 30.06263
        Public Property Longitude As Double = 31.24967
        Public Property AsrMethod As Integer = 0
        Public Property IsSummer As Boolean = False
        
        ' Widget settings
        Public Property WidgetOpacity As Double = 70
        Public Property NotificationOpacity As Double = 95
        Public Property ShowProgressRing As Boolean = True
        Public Property HijriAdjustment As Integer = 0
        Public Property AthanSound As String = "default"
        Public Property EnableDuaaAfterAthan As Boolean = False
        Public Property DuaaSound As String = "sha3rawy_duaa"
        Public Property Size As String = "Medium"
        Public Property CornerRadius As Double = 75
        Public Property GridMargin As Integer = 8
        Public Property StrokeThickness As Double = 3
        Public Property WindowWidth As Integer = 180
        Public Property WindowHeight As Integer = 180
        Public Property HasCompletedSetup As Boolean = False
    End Class
End Namespace

Imports System.IO
Imports Microsoft.Data.Sqlite
Imports Salaty.First.Services
Imports Salaty.First.Models

Public Class SettingsManager
    Private ReadOnly _sqliteService As SqlitePrayerService

    Public Sub New()
        ' Use AppData path for write access (avoids permission issues in Program Files)
        Dim dbPath = DatabaseManager.GetDatabasePath()
        _sqliteService = New SqlitePrayerService(dbPath)
    End Sub

    Public Function LoadSettings() As WidgetSettings
        Try
            ' Load prayer settings from database
            Dim dbSettings = _sqliteService.GetSettings()

            ' Ensure default values for missing settings (migration for existing users)
            EnsureDefaultSettings()

            ' Convert country code to country name
            Dim countryName As String = dbSettings.CountryCode
            If Not String.IsNullOrEmpty(dbSettings.CountryCode) AndAlso dbSettings.CountryCode.Length = 2 Then
                ' It's a country code, get the full name
                countryName = GetCountryNameFromCode(dbSettings.CountryCode)
            End If

            ' Create WidgetSettings from database settings
            Dim settings As New WidgetSettings With {
                .City = If(String.IsNullOrEmpty(dbSettings.CityName), "Cairo", dbSettings.CityName),
                .Country = If(String.IsNullOrEmpty(countryName), "Egypt", countryName),
                .Method = dbSettings.Method,
                .AsrMethod = CInt(GetDoubleSetting("AsrMethod", 0)),
                .HijriAdjustment = CInt(GetDoubleSetting("HijriAdjustment", 0)),
                .HasCompletedSetup = CBool(GetDoubleSetting("HasCompletedSetup", 0)),
                .IsSummerTime = dbSettings.IsSummer,
                .WidgetOpacity = GetDoubleSetting("WidgetOpacity", 70),
                .NotificationOpacity = GetDoubleSetting("NotificationOpacity", 95),
                .ShowProgressRing = CBool(GetDoubleSetting("ShowProgressRing", 1)),
                .Size = GetStringSetting("Size", "Medium"),
                .CornerRadius = CInt(GetDoubleSetting("CornerRadius", 75)),
                .GridMargin = CInt(GetDoubleSetting("GridMargin", 8)),
                .StrokeThickness = GetDoubleSetting("StrokeThickness", 3),
                .WindowWidth = CInt(GetDoubleSetting("WindowWidth", 180)),
                .WindowHeight = CInt(GetDoubleSetting("WindowHeight", 180)),
                .AthanSound = GetStringSetting("AthanSound", "default"),
                .EnableDuaaAfterAthan = CBool(GetDoubleSetting("EnableDuaaAfterAthan", 1)),
                .DuaaSound = GetStringSetting("DuaaSound", "mashary_duaa"),
                .EnableQuotes = CBool(GetDoubleSetting("EnableQuotes", 1)),
                .QuoteInterval = CInt(GetDoubleSetting("QuoteInterval", 1)),
                .EnableQuoteAudio = CBool(GetDoubleSetting("EnableQuoteAudio", 1))
            }

            Return settings
        Catch ex As Exception
            Console.WriteLine("Error loading settings: " & ex.Message)
            Return WidgetSettings.GetSizePreset("Medium")
        End Try
    End Function

    ''' <summary>
    ''' Get country name from country code using database
    ''' </summary>
    Private Function GetCountryNameFromCode(countryCode As String) As String
        Try
            Dim countries = _sqliteService.GetCountries()
            
            ' First try: match by country code
            Dim country = countries.FirstOrDefault(Function(c) c.Code.ToUpper() = countryCode.ToUpper())
            If country IsNot Nothing Then
                Console.WriteLine($"GetCountryNameFromCode: Found '{country.Name}' for code '{countryCode}'")
                Return country.Name
            End If
            
            ' Second try: the countryCode might already be a country name
            country = countries.FirstOrDefault(Function(c) c.Name.ToUpper() = countryCode.ToUpper())
            If country IsNot Nothing Then
                Console.WriteLine($"GetCountryNameFromCode: '{countryCode}' is already a country name")
                Return country.Name
            End If
            
        Catch ex As Exception
            Console.WriteLine("Error getting country name: " & ex.Message)
        End Try
        
        ' Fallback: try to capitalize the code as a name
        Console.WriteLine($"GetCountryNameFromCode: Using fallback for '{countryCode}'")
        Return countryCode
    End Function

    ''' <summary>
    ''' Ensure default settings exist for new/missing settings (migration)
    ''' Only sets defaults if setting doesn't exist - does NOT overwrite user settings
    ''' </summary>
    Private Sub EnsureDefaultSettings()
        Try
            ' Check if ShowProgressRing exists, if not set default to 1 (enabled)
            Dim showProgressRing = GetDoubleSetting("ShowProgressRing", -1)
            If showProgressRing = -1 Then
                SaveSetting("ShowProgressRing", "1")
                Console.WriteLine("Migration: Set ShowProgressRing=1 (default)")
            End If

            ' Check EnableDuaaAfterAthan
            Dim enableDuaa = GetDoubleSetting("EnableDuaaAfterAthan", -1)
            If enableDuaa = -1 Then
                SaveSetting("EnableDuaaAfterAthan", "1")
                Console.WriteLine("Migration: Set EnableDuaaAfterAthan=1 (default)")
            End If

            ' Check DuaaSound
            Dim duaaSound = GetStringSetting("DuaaSound", Nothing)
            If duaaSound Is Nothing Then
                SaveSetting("DuaaSound", "mashary_duaa")
                Console.WriteLine("Migration: Set DuaaSound=mashary_duaa (default)")
            End If

            ' Check StartWithWindows
            Dim startWithWindows = GetStringSetting("StartWithWindows", Nothing)
            If startWithWindows Is Nothing Then
                SaveSetting("StartWithWindows", "1")
                Console.WriteLine("Migration: Set StartWithWindows=1 (default)")
            End If

            ' Check prayer reminders - only set if not exists
            For Each prayer In {"Fajr", "Dhuhr", "Asr", "Maghrib", "Isha"}
                Dim enabledBefore = GetStringSetting($"{prayer}Reminder_EnabledBefore", Nothing)
                If enabledBefore Is Nothing Then
                    SaveSetting($"{prayer}Reminder_EnabledBefore", "1")
                    SaveSetting($"{prayer}Reminder_MinutesBefore", "10")
                    Console.WriteLine($"Migration: Set {prayer}Reminder defaults")
                End If
                
                Dim enabledAfter = GetStringSetting($"{prayer}Reminder_EnabledAfter", Nothing)
                If enabledAfter Is Nothing Then
                    SaveSetting($"{prayer}Reminder_EnabledAfter", "1")
                    SaveSetting($"{prayer}Reminder_MinutesAfter", "15")
                End If
            Next
        Catch ex As Exception
            Console.WriteLine("Error in EnsureDefaultSettings: " & ex.Message)
        End Try
    End Sub

    Public Sub SaveSettings(settings As WidgetSettings)
        Try
            ' First, save the Language setting before deleting all settings
            Dim savedLanguage = GetSetting("Language", "en")

            Using connection = _sqliteService.GetConnection()
                connection.Open()
                Dim transaction = connection.BeginTransaction()

                Try
                    ' Delete existing settings EXCEPT Language
                    Dim deleteCmd As New SqliteCommand("DELETE FROM settings WHERE Name != 'Language';", connection, transaction)
                    deleteCmd.ExecuteNonQuery()

                    ' Insert prayer settings
                    InsertSetting(connection, transaction, "City_ID", settings.City)
                    InsertSetting(connection, transaction, "Country_Name", settings.Country)
                    InsertSetting(connection, transaction, "Method", settings.Method.ToString())
                    InsertSetting(connection, transaction, "City_Name", settings.City)

                    ' Insert widget settings
                    InsertSetting(connection, transaction, "Size", settings.Size)
                    InsertSetting(connection, transaction, "CornerRadius", settings.CornerRadius.ToString())
                    InsertSetting(connection, transaction, "GridMargin", settings.GridMargin.ToString())
                    InsertSetting(connection, transaction, "StrokeThickness", settings.StrokeThickness.ToString())
                    InsertSetting(connection, transaction, "WindowWidth", settings.WindowWidth.ToString())
                    InsertSetting(connection, transaction, "WindowHeight", settings.WindowHeight.ToString())
                    InsertSetting(connection, transaction, "AthanSound", settings.AthanSound)
                    InsertSetting(connection, transaction, "EnableDuaaAfterAthan", If(settings.EnableDuaaAfterAthan, "1", "0"))
                    InsertSetting(connection, transaction, "DuaaSound", settings.DuaaSound)
                    InsertSetting(connection, transaction, "IsSummerTime", If(settings.IsSummerTime, "1", "0"))
                    InsertSetting(connection, transaction, "WidgetOpacity", settings.WidgetOpacity.ToString())
                    InsertSetting(connection, transaction, "NotificationOpacity", settings.NotificationOpacity.ToString())
                    InsertSetting(connection, transaction, "ShowProgressRing", If(settings.ShowProgressRing, "1", "0"))
                    InsertSetting(connection, transaction, "HijriAdjustment", settings.HijriAdjustment.ToString())
                    InsertSetting(connection, transaction, "AsrMethod", settings.AsrMethod.ToString())
                    InsertSetting(connection, transaction, "HasCompletedSetup", If(settings.HasCompletedSetup, "1", "0"))

                    Console.WriteLine($"Saved ShowProgressRing={settings.ShowProgressRing}, WidgetOpacity={settings.WidgetOpacity}, NotificationOpacity={settings.NotificationOpacity}")

                    transaction.Commit()
                Catch ex As Exception
                    transaction.Rollback()
                    Throw ex
                End Try
            End Using
        Catch ex As Exception
            Console.WriteLine("Error saving settings: " & ex.Message)
            MessageBox.Show("Error saving settings: " & ex.Message, "Error",
                           MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub InsertSetting(connection As SqliteConnection, 
                             transaction As SqliteTransaction,
                             name As String, value As String)
        Dim cmd As New SqliteCommand(
            "INSERT INTO settings (Name, Value) VALUES (@name, @value);", connection, transaction)
        cmd.Parameters.AddWithValue("@name", name)
        cmd.Parameters.AddWithValue("@value", value)
        cmd.ExecuteNonQuery()
    End Sub

    Private Function GetStringSetting(name As String, defaultValue As String) As String
        Try
            Using connection = _sqliteService.GetConnection()
                connection.Open()
                Dim cmd As New SqliteCommand(
                    "SELECT Value FROM settings WHERE Name = @name;", connection)
                cmd.Parameters.AddWithValue("@name", name)
                Dim result = cmd.ExecuteScalar()
                If result IsNot Nothing Then
                    Return result.ToString()
                End If
            End Using
        Catch ex As Exception
            Console.WriteLine("Error loading setting " & name & ": " & ex.Message)
        End Try
        Return defaultValue
    End Function

    Public Function GetDoubleSetting(name As String, defaultValue As Double) As Double
        Dim value = GetStringSetting(name, defaultValue.ToString())
        If Double.TryParse(value, Nothing) Then
            Return Double.Parse(value)
        End If
        Return defaultValue
    End Function

    ' Get setting as string (for ShowWidget)
    Public Function GetSetting(name As String, defaultValue As String) As String
        Return GetStringSetting(name, defaultValue)
    End Function

    ' Save individual setting (for reminders)
    Public Sub SaveSetting(name As String, value As String)
        Try
            Using connection = _sqliteService.GetConnection()
                connection.Open()
                ' Delete existing setting first
                Dim deleteCmd As New SqliteCommand("DELETE FROM settings WHERE Name = @name;", connection)
                deleteCmd.Parameters.AddWithValue("@name", name)
                deleteCmd.ExecuteNonQuery()

                ' Insert new setting
                Dim insertCmd As New SqliteCommand(
                    "INSERT INTO settings (Name, Value) VALUES (@name, @value);", connection)
                insertCmd.Parameters.AddWithValue("@name", name)
                insertCmd.Parameters.AddWithValue("@value", value)
                insertCmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception
            Console.WriteLine("Error saving setting " & name & ": " & ex.Message)
        End Try
    End Sub

    ' Get all countries from database
    Public Function GetCountries() As List(Of CountryInfo)
        Return _sqliteService.GetCountries()
    End Function

    ' Get cities by country
    Public Function GetCitiesByCountry(countryCode As String) As List(Of CityInfo)
        Return _sqliteService.GetCitiesByCountry(countryCode)
    End Function

    ' Search cities
    Public Function SearchCities(searchTerm As String) As List(Of CityInfo)
        Return _sqliteService.SearchCities(searchTerm)
    End Function

    ' Get calculation methods
    Public Function GetCalculationMethods() As List(Of MethodInfo)
        Return _sqliteService.GetCalculationMethods()
    End Function

    ' Get athan sounds
    Public Function GetAthanSounds() As List(Of String)
        Return _sqliteService.GetAthanSounds()
    End Function
End Class

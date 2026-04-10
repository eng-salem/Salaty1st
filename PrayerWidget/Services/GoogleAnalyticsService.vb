Imports System.Net.Http
Imports System.Threading.Tasks

''' <summary>
''' Simple anonymous usage counter using counterapi.dev v2
''' Free, no registration required, no hosting needed
''' Visit: https://counterapi.dev/
''' </summary>
Public Class GoogleAnalyticsService
    ' Your counterapi.dev v2 credentials
    Private ReadOnly _namespace As String = "mokaa-mnhgs-team-3199"
    Private ReadOnly _counterKey As String = "slt"
    Private ReadOnly _apiToken As String = "ut_qDLbsdap5n6exaduTOedzqIGJt33Elb6hGYB2Bp5"
    Private ReadOnly _httpClient As New HttpClient()
    Private _clientId As String

    Public Sub New()
        _clientId = GetOrCreateClientId()
        
        ' Setup HTTP client with authorization
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiToken}")
    End Sub

    ''' <summary>
    ''' Generate or retrieve a unique client ID for this user/machine
    ''' </summary>
    Private Function GetOrCreateClientId() As String
        Dim clientIdPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PrayerWidget", "client_id.txt")

        Try
            If System.IO.File.Exists(clientIdPath) Then
                Return System.IO.File.ReadAllText(clientIdPath)
            End If
        Catch ex As Exception
        End Try

        Dim clientId = Guid.NewGuid().ToString()

        Try
            Dim dir = System.IO.Path.GetDirectoryName(clientIdPath)
            If Not System.IO.Directory.Exists(dir) Then
                System.IO.Directory.CreateDirectory(dir)
            End If
            System.IO.File.WriteAllText(clientIdPath, clientId)
        Catch ex As Exception
        End Try

        Return clientId
    End Function

    ''' <summary>
    ''' Send a hit to increment the counter
    ''' </summary>
    Public Async Function TrackEvent(eventName As String) As Task
        If Not IsAnalyticsEnabled() Then
            Return
        End If

        Try
            ' Increment counter using counterapi.dev v2
            ' Format: https://api.counterapi.dev/v2/{namespace}/{key}/up
            Dim url = $"https://api.counterapi.dev/v2/{_namespace}/{_counterKey}/up"
            
            Using client As New HttpClient()
                client.Timeout = TimeSpan.FromSeconds(5)
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiToken}")
                Await client.GetAsync(url)
            End Using

        Catch ex As Exception
            ' Silently fail - analytics should not crash the app
            System.Diagnostics.Debug.WriteLine($"Analytics error: {ex.Message}")
        End Try
    End Function

    ''' <summary>
    ''' Track app launch
    ''' </summary>
    Public Function TrackAppLaunch() As Task
        Return TrackEvent("launch")
    End Function

    ''' <summary>
    ''' Track app close
    ''' </summary>
    Public Function TrackAppClose() As Task
        ' Skip close tracking to avoid delays on exit
        Return Task.CompletedTask
    End Function

    ''' <summary>
    ''' Check if analytics is enabled in settings
    ''' </summary>
    Private Function IsAnalyticsEnabled() As Boolean
        Try
            Dim settingsPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PrayerWidget", "settings.json")

            If System.IO.File.Exists(settingsPath) Then
                Dim json = System.IO.File.ReadAllText(settingsPath)
                Dim value = ParseJsonBoolean(json, "EnableAnalytics")
                If value.HasValue Then
                    Return value.Value
                End If
            End If
        Catch ex As Exception
        End Try

        Return True
    End Function

    ''' <summary>
    ''' Parse a boolean value from JSON
    ''' </summary>
    Private Function ParseJsonBoolean(json As String, propertyName As String) As Boolean?
        Try
            Dim searchKey = """" & propertyName & """:"
            Dim startIndex = json.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase)
            If startIndex < 0 Then Return Nothing

            startIndex += searchKey.Length
            ' Skip whitespace
            While startIndex < json.Length AndAlso Char.IsWhiteSpace(json(startIndex))
                startIndex += 1
            End While

            ' Check for true/false
            If json.Substring(startIndex).StartsWith("true", StringComparison.OrdinalIgnoreCase) Then
                Return True
            ElseIf json.Substring(startIndex).StartsWith("false", StringComparison.OrdinalIgnoreCase) Then
                Return False
            End If
        Catch ex As Exception
            Console.WriteLine("ParseJsonBoolean error: " & ex.Message)
        End Try
        Return Nothing
    End Function

    ''' <summary>
    ''' Parse an integer value from JSON
    ''' </summary>
    Private Function ParseJsonInteger(json As String, propertyName As String) As Integer?
        Try
            Dim searchKey = """" & propertyName & """:"
            Dim startIndex = json.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase)
            If startIndex < 0 Then Return Nothing

            startIndex += searchKey.Length
            ' Skip whitespace
            While startIndex < json.Length AndAlso Char.IsWhiteSpace(json(startIndex))
                startIndex += 1
            End While

            ' Find end of value
            Dim endIndex = startIndex
            While endIndex < json.Length AndAlso Char.IsDigit(json(endIndex))
                endIndex += 1
            End While

            If endIndex > startIndex Then
                Dim valueStr = json.Substring(startIndex, endIndex - startIndex)
                Dim result As Integer
                If Integer.TryParse(valueStr, result) Then
                    Return result
                End If
            End If
        Catch ex As Exception
            Console.WriteLine("ParseJsonInteger error: " & ex.Message)
        End Try
        Return Nothing
    End Function

    ''' <summary>
    ''' Enable or disable analytics
    ''' </summary>
    Public Sub SetEnabled(enabled As Boolean)
        Try
            Dim settingsPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PrayerWidget", "settings.json")

            Dim dir = System.IO.Path.GetDirectoryName(settingsPath)
            If Not System.IO.Directory.Exists(dir) Then
                System.IO.Directory.CreateDirectory(dir)
            End If

            Dim settings As New Dictionary(Of String, Object)
            If System.IO.File.Exists(settingsPath) Then
                Dim json = System.IO.File.ReadAllText(settingsPath)
                ' Parse existing settings manually
                settings = ParseJsonDictionary(json)
            End If

            settings("EnableAnalytics") = enabled

            ' Write simple JSON manually
            Dim newJson = WriteJsonDictionary(settings)
            System.IO.File.WriteAllText(settingsPath, newJson)
        Catch ex As Exception
            Console.WriteLine("SetEnabled error: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Parse a simple JSON dictionary
    ''' </summary>
    Private Function ParseJsonDictionary(json As String) As Dictionary(Of String, Object)
        Dim result As New Dictionary(Of String, Object)()
        Dim quote As Char = Chr(34)
        Try
            json = json.Trim()
            If json.StartsWith("{") Then json = json.Substring(1)
            If json.EndsWith("}") Then json = json.Substring(0, json.Length - 1)

            Dim i As Integer = 0
            While i < json.Length
                While i < json.Length AndAlso (Char.IsWhiteSpace(json(i)) OrElse json(i) = ","c)
                    i += 1
                End While
                If i >= json.Length Then Exit While

                If json(i) = quote Then
                    i += 1
                    Dim keyStart As Integer = i
                    While i < json.Length AndAlso json(i) <> quote
                        i += 1
                    End While
                    Dim key As String = json.Substring(keyStart, i - keyStart)
                    i += 1

                    While i < json.Length AndAlso (Char.IsWhiteSpace(json(i)) OrElse json(i) = ":"c)
                        i += 1
                    End While

                    If i < json.Length Then
                        If json(i) = quote Then
                            ' String value
                            i += 1
                            Dim valueStart As Integer = i
                            While i < json.Length AndAlso json(i) <> quote
                                i += 1
                            End While
                            result(key) = json.Substring(valueStart, i - valueStart)
                            i += 1
                        ElseIf Char.IsDigit(json(i)) OrElse json(i) = "-"c Then
                            ' Number
                            Dim valueStart As Integer = i
                            While i < json.Length AndAlso (Char.IsDigit(json(i)) OrElse json(i) = "."c OrElse json(i) = "-"c)
                                i += 1
                            End While
                            Dim valueStr = json.Substring(valueStart, i - valueStart)
                            Dim intValue As Integer
                            Dim doubleValue As Double
                            If Integer.TryParse(valueStr, intValue) Then
                                result(key) = intValue
                            ElseIf Double.TryParse(valueStr, doubleValue) Then
                                result(key) = doubleValue
                            End If
                        ElseIf json.Substring(i).StartsWith("true", StringComparison.OrdinalIgnoreCase) Then
                            result(key) = True
                            i += 4
                        ElseIf json.Substring(i).StartsWith("false", StringComparison.OrdinalIgnoreCase) Then
                            result(key) = False
                            i += 5
                        End If
                    End If
                Else
                    i += 1
                End If
            End While
        Catch ex As Exception
            Console.WriteLine("ParseJsonDictionary error: " & ex.Message)
        End Try
        Return result
    End Function

    ''' <summary>
    ''' Write a simple JSON dictionary
    ''' </summary>
    Private Function WriteJsonDictionary(dict As Dictionary(Of String, Object)) As String
        Dim sb As New System.Text.StringBuilder()
        Dim quote As Char = Chr(34)
        sb.Append("{")
        Dim first As Boolean = True
        For Each kvp In dict
            If Not first Then sb.Append(",")
            first = False
            sb.Append(quote).Append(kvp.Key).Append(quote).Append(":")
            If TypeOf kvp.Value Is String Then
                Dim strVal = kvp.Value.ToString().Replace(quote, "\" & quote)
                sb.Append(quote).Append(strVal).Append(quote)
            ElseIf TypeOf kvp.Value Is Boolean Then
                sb.Append(If(CType(kvp.Value, Boolean), "true", "false"))
            Else
                sb.Append(kvp.Value.ToString())
            End If
        Next
        sb.Append("}")
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Check if analytics is currently enabled
    ''' </summary>
    Public Function IsEnabled() As Boolean
        Return IsAnalyticsEnabled()
    End Function

    ''' <summary>
    ''' Get current count (for viewing stats)
    ''' </summary>
    Public Async Function GetCount() As Task(Of Integer)
        Try
            Dim url = $"https://api.counterapi.dev/v2/{_namespace}/{_counterKey}"

            Using client As New HttpClient()
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiToken}")
                Dim response = Await client.GetAsync(url)
                Dim json = Await response.Content.ReadAsStringAsync()
                Dim count = ParseJsonInteger(json, "count")
                If count.HasValue Then
                    Return count.Value
                End If
            End Using
        Catch ex As Exception
            Console.WriteLine("GetCount error: " & ex.Message)
        End Try
        Return 0
    End Function

    Protected Overrides Sub Finalize()
        _httpClient.Dispose()
        MyBase.Finalize()
    End Sub
End Class

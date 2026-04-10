Imports System
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading.Tasks
Imports Salaty.First.Services

''' <summary>
''' Service to fetch Islamic quotes from Turso libSQL database via HTTP API
''' Also tracks quote counter in device_counters table (separate database)
''' </summary>
Public Class QuoteService
    ' Use shared counter database for everything (quotes + device_counters)
    Private ReadOnly _tursoUrl As String = TursoConstants.CounterTursoUrl
    Private ReadOnly _authToken As String = TursoConstants.CounterAuthToken

    Private ReadOnly _httpClient As HttpClient
    Private ReadOnly _deviceCounterService As DeviceCounterService

    Public Sub New(deviceCounterService As DeviceCounterService)
        _httpClient = New HttpClient()
        _httpClient.Timeout = TimeSpan.FromSeconds(10)
        _deviceCounterService = deviceCounterService
    End Sub

    ''' <summary>
    ''' Get device ID from the shared DeviceCounterService
    ''' </summary>
    Private Function GetDeviceId() As String
        If _deviceCounterService IsNot Nothing Then
            Return _deviceCounterService.DeviceId
        End If
        Return "unknown"
    End Function

    ''' <summary>
    ''' Generic: Execute SQL via Turso HTTP API
    ''' </summary>
    Private Async Function ExecuteSqlAsync(sql As String) As Task(Of String)
        Try
            Dim endpoint = $"{_tursoUrl}/v2/pipeline"

            ' Manually build JSON to avoid serialization issues in .NET 4.8
            Dim escapedSql = sql.Replace("""", "\""")
            Dim jsonBody = $"{{""requests"":[{{""type"":""execute"",""stmt"":{{""sql"":""{escapedSql}""}}}}]}}"

            Console.WriteLine($"[QuoteService] Request: {jsonBody}")

            ' Create HTTP request with Authorization header
            Dim httpRequest = New HttpRequestMessage(HttpMethod.Post, endpoint)
            httpRequest.Headers.Add("Authorization", $"Bearer {_authToken}")
            httpRequest.Content = New StringContent(jsonBody, Encoding.UTF8, "application/json")

            Dim response = Await _httpClient.SendAsync(httpRequest)
            Dim responseJson = Await response.Content.ReadAsStringAsync()

            If response.IsSuccessStatusCode Then
                Console.WriteLine($"[QuoteService] SQL OK: {response.StatusCode}")
                Return responseJson
            Else
                Console.WriteLine($"[QuoteService] HTTP error: {response.StatusCode} - {responseJson}")
                Return Nothing
            End If

        Catch ex As Exception
            Console.WriteLine($"[QuoteService] SQL execution error: {ex.Message}")
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Get a random Islamic quote from Turso quotes table
    ''' </summary>
    Public Async Function GetRandomQuote() As Task(Of IslamicQuote)
        Try
            ' Fetch a random quote using ORDER BY RANDOM() LIMIT 1
            ' Note: column is "Name" with capital N
            Dim sql = "SELECT id, Name FROM quotes ORDER BY RANDOM() LIMIT 1"
            Dim responseJson = Await ExecuteSqlAsync(sql)

            If String.IsNullOrEmpty(responseJson) Then
                Console.WriteLine("[QuoteService] No response from Turso quotes DB")
                Return Nothing
            End If

            Console.WriteLine($"[QuoteService] Raw response: {responseJson.Substring(0, Math.Min(300, responseJson.Length))}")

            ' Parse response - Turso returns values as objects with type/value properties
            ' Example: {"results":[{"response":{"result":{"rows":[[{"type":"integer","value":"199"},{"type":"text","value":"..."}]]}}}]
            Dim jsonDoc = JsonDocument.Parse(responseJson)
            Dim root = jsonDoc.RootElement

            If root.TryGetProperty("results", Nothing) Then
                Dim results = root.GetProperty("results")
                If results.GetArrayLength() > 0 Then
                    Dim firstResult = results(0)
                    If firstResult.TryGetProperty("response", Nothing) Then
                        Dim response = firstResult.GetProperty("response")
                        If response.TryGetProperty("result", Nothing) Then
                            Dim result = response.GetProperty("result")
                            If result.TryGetProperty("rows", Nothing) Then
                                Dim rows = result.GetProperty("rows")
                                If rows.GetArrayLength() > 0 Then
                                    Dim row = rows(0)
                                    If row.GetArrayLength() >= 2 Then
                                        Dim quote As New IslamicQuote()

                                        ' Parse id (integer type)
                                        Dim idValue As Integer = 0
                                        If row(0).TryGetProperty("value", Nothing) Then
                                            Dim idStr = row(0).GetProperty("value").GetString()
                                            If Integer.TryParse(idStr, idValue) Then
                                                quote.id = idValue
                                            End If
                                        End If

                                        ' Parse quote text (text type)
                                        If row(1).TryGetProperty("value", Nothing) Then
                                            quote.quote = row(1).GetProperty("value").GetString()
                                        End If

                                        quote.source = ""
                                        quote.category = ""

                                        If Not String.IsNullOrEmpty(quote.quote) Then
                                            Console.WriteLine($"[QuoteService] Quote loaded: {quote.quote.Substring(0, Math.Min(50, quote.quote.Length))}...")
                                            Return quote
                                        End If
                                    Else
                                        Console.WriteLine($"[QuoteService] Not enough values in row: {row.GetArrayLength()}")
                                    End If
                                Else
                                    Console.WriteLine("[QuoteService] Empty rows array")
                                End If
                            Else
                                Console.WriteLine("[QuoteService] No 'rows' property in result")
                            End If
                        Else
                            Console.WriteLine("[QuoteService] No 'result' property in response")
                        End If
                    Else
                        Console.WriteLine("[QuoteService] No 'response' property in result")
                    End If
                Else
                    Console.WriteLine("[QuoteService] Empty results array")
                End If
            Else
                Console.WriteLine("[QuoteService] No 'results' property in response")
            End If

            Console.WriteLine("[QuoteService] No quote found in response")
        Catch ex As Exception
            Console.WriteLine("[QuoteService] Error fetching quote: " & ex.Message)
            Console.WriteLine("[QuoteService] Stack trace: " & ex.StackTrace)
        End Try
        Return Nothing
    End Function

    ''' <summary>
    ''' Increment the quote counter for this device in device_counters table
    ''' Uses the COUNTER database (separate from quotes database)
    ''' </summary>
    Public Async Function IncrementQuoteCounterAsync() As Task
        Try
            Dim deviceId = GetDeviceId()
            Dim escapedDeviceId = deviceId.Replace("'", "''")
            Dim sql = $"UPDATE device_counters SET quotecounter = COALESCE(quotecounter, 0) + 1 WHERE device_id = '{escapedDeviceId}'"
            Dim result = Await ExecuteSqlAsync(sql)

            If result IsNot Nothing Then
                Console.WriteLine($"[QuoteService] Quote counter incremented for device: {deviceId}")
            Else
                Console.WriteLine($"[QuoteService] Failed to increment quote counter")
            End If
        Catch ex As Exception
            Console.WriteLine($"[QuoteService] Error incrementing quote counter: {ex.Message}")
        End Try
    End Function

    Protected Overrides Sub Finalize()
        _httpClient.Dispose()
        MyBase.Finalize()
    End Sub
End Class

''' <summary>
''' Islamic quote model
''' </summary>
Public Class IslamicQuote
    Public Property id As Integer
    Public Property quote As String
    Public Property source As String
    Public Property category As String
End Class

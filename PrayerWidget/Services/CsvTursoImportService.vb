Imports System
Imports System.IO
Imports System.Net.Http
Imports System.Text
Imports System.Threading.Tasks
Imports Microsoft.Win32
Imports System.Text.Json

Namespace Services
    ''' <summary>
    ''' Service to import CSV data into Turso libSQL database via HTTP API
    ''' Uses Turso's REST API directly (no native DLL dependencies)
    ''' Documentation: https://docs.turso.tech/http/reference
    ''' </summary>
    Public Class CsvTursoImportService
        Implements IDisposable

        Private ReadOnly _tursoUrl As String
        Private ReadOnly _authToken As String
        Private _httpClient As HttpClient
        Private _disposed As Boolean = False

        ''' <summary>
        ''' Initialize the CSV import service with Turso credentials
        ''' </summary>
        ''' <param name="tursoUrl">Turso database URL (e.g., https://your-db.turso.io)</param>
        ''' <param name="authToken">Turso authentication token</param>
        Public Sub New(tursoUrl As String, authToken As String)
            ' Convert libsql:// to https:// for HTTP API calls
            If tursoUrl.StartsWith("libsql://", StringComparison.OrdinalIgnoreCase) Then
                _tursoUrl = tursoUrl.Replace("libsql://", "https://")
            ElseIf tursoUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) OrElse
                   tursoUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) Then
                _tursoUrl = tursoUrl
            Else
                _tursoUrl = "https://" & tursoUrl
            End If
            
            _authToken = authToken

            ' Create HTTP client for Turso REST API
            _httpClient = New HttpClient()
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}")
            _httpClient.Timeout = TimeSpan.FromSeconds(30)

            Console.WriteLine($"[CsvTursoImport] Service initialized with URL: {_tursoUrl}")
        End Sub

        ''' <summary>
        ''' Opens a file dialog to select a CSV file and imports it to Turso
        ''' </summary>
        ''' <param name="tableName">Target table name in Turso database</param>
        ''' <param name="columnMappings">Column mappings: CSV column index -> DB column name</param>
        ''' <param name="skipHeaders">Skip header row (default: True)</param>
        ''' <returns>Number of rows imported, or -1 if failed/cancelled</returns>
        Public Async Function ImportCsvToTursoAsync(
            tableName As String,
            columnMappings As Dictionary(Of Integer, String),
            Optional skipHeaders As Boolean = True) As Task(Of Integer)

            Try
                ' Open file dialog to select CSV
                Dim openFileDialog As New OpenFileDialog()
                openFileDialog.Title = "Select CSV File to Import"
                openFileDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*"
                openFileDialog.FilterIndex = 1

                Dim dialogResult = openFileDialog.ShowDialog()
                If dialogResult <> True Then
                    Console.WriteLine("[CsvTursoImport] User cancelled file selection")
                    Return -1
                End If

                Dim filePath = openFileDialog.FileName
                Console.WriteLine($"[CsvTursoImport] Selected file: {filePath}")

                ' Read and import CSV data
                Return Await ImportFileAsync(filePath, tableName, columnMappings, skipHeaders)

            Catch ex As Exception
                Console.WriteLine($"[CsvTursoImport] Error: {ex.Message}")
                Return -1
            End Try
        End Function

        ''' <summary>
        ''' Imports a CSV file to Turso database
        ''' </summary>
        ''' <param name="filePath">Path to CSV file</param>
        ''' <param name="tableName">Target table name</param>
        ''' <param name="columnMappings">Column mappings: CSV column index -> DB column name</param>
        ''' <param name="skipHeaders">Skip header row</param>
        ''' <returns>Number of rows imported</returns>
        Public Async Function ImportFileAsync(
            filePath As String,
            tableName As String,
            columnMappings As Dictionary(Of Integer, String),
            Optional skipHeaders As Boolean = True) As Task(Of Integer)

            Dim importedRows As Integer = 0

            Try
                If Not File.Exists(filePath) Then
                    Console.WriteLine($"[CsvTursoImport] File not found: {filePath}")
                    Return 0
                End If

                ' Read CSV file
                Dim lines = File.ReadAllLines(filePath)
                Console.WriteLine($"[CsvTursoImport] Read {lines.Length} lines from CSV")

                Dim startLine As Integer = 0
                If skipHeaders AndAlso lines.Length > 0 Then
                    startLine = 1
                    Console.WriteLine("[CsvTursoImport] Skipping header row")
                End If

                ' Process each row
                For i As Integer = startLine To lines.Length - 1
                    Dim line = lines(i).Trim()
                    If String.IsNullOrEmpty(line) Then
                        Continue For
                    End If

                    ' Parse CSV line (handle quoted fields)
                    Dim values = ParseCsvLine(line)

                    ' Build INSERT statement
                    Dim columns = columnMappings.Values.ToList()
                    Dim placeholders = columns.Select(Function(c) $"?").ToList()

                    Dim sql = $"INSERT INTO {tableName} ({String.Join(", ", columns)}) VALUES ({String.Join(", ", placeholders)})"

                    ' Prepare values for the mapped columns
                    Dim rowValues As New List(Of Object)()
                    For Each kvp In columnMappings
                        If kvp.Key < values.Count Then
                            rowValues.Add(values(kvp.Key))
                        Else
                            rowValues.Add(DBNull.Value)
                        End If
                    Next

                    ' Execute INSERT
                    Dim success = Await ExecuteSqlAsync(sql, rowValues.ToArray())
                    If success Then
                        importedRows += 1
                    Else
                        Console.WriteLine($"[CsvTursoImport] Failed to import row {i + 1}")
                    End If
                Next

                Console.WriteLine($"[CsvTursoImport] Successfully imported {importedRows} rows")
                Return importedRows

            Catch ex As Exception
                Console.WriteLine($"[CsvTursoImport] Import error: {ex.Message}")
                Return 0
            End Try
        End Function

        ''' <summary>
        ''' Parse a CSV line handling quoted fields
        ''' </summary>
        Private Function ParseCsvLine(line As String) As List(Of String)
            Dim values As New List(Of String)()
            Dim currentValue As New StringBuilder()
            Dim inQuotes As Boolean = False
            Dim quoteChar As Char = """"c

            For i As Integer = 0 To line.Length - 1
                Dim c = line(i)

                If c = quoteChar Then
                    If inQuotes AndAlso i < line.Length - 1 AndAlso line(i + 1) = quoteChar Then
                        ' Escaped quote
                        currentValue.Append(quoteChar)
                        i += 1
                    Else
                        ' Toggle quote mode
                        inQuotes = Not inQuotes
                    End If
                ElseIf c = ","c AndAlso Not inQuotes Then
                    ' End of field
                    values.Add(currentValue.ToString().Trim())
                    currentValue.Clear()
                Else
                    currentValue.Append(c)
                End If
            Next

            ' Add last field
            values.Add(currentValue.ToString().Trim())

            Return values
        End Function

        ''' <summary>
        ''' Execute SQL via Turso HTTP API
        ''' Turso pipeline format: { requests: [{ type: "execute", stmt: { sql: "..." } }] }
        ''' </summary>
        Private Async Function ExecuteSqlAsync(sql As String, Optional args As Object() = Nothing) As Task(Of Boolean)
            Try
                ' Turso HTTP API endpoint
                Dim endpoint = $"{_tursoUrl}/v2/pipeline"

                ' Embed args directly in SQL to avoid type encoding issues
                Dim finalSql = sql
                If args IsNot Nothing AndAlso args.Length > 0 Then
                    ' Replace ? placeholders with escaped values one by one
                    For Each arg In args
                        Dim escapedValue = If(arg Is DBNull.Value, "NULL", EscapeSqlValue(arg.ToString()))
                        Dim placeholderIndex = finalSql.IndexOf("?"c)
                        If placeholderIndex >= 0 Then
                            If arg Is DBNull.Value Then
                                finalSql = finalSql.Substring(0, placeholderIndex) & "NULL" & finalSql.Substring(placeholderIndex + 1)
                            Else
                                finalSql = finalSql.Substring(0, placeholderIndex) & $"'{escapedValue}'" & finalSql.Substring(placeholderIndex + 1)
                            End If
                        End If
                    Next
                End If

                ' Build stmt object without args
                Dim stmtObj As New Dictionary(Of String, Object) From {
                    {"sql", finalSql}
                }

                ' Build request with type and stmt
                Dim requests As New List(Of Object)()
                Dim request As New Dictionary(Of String, Object) From {
                    {"type", "execute"},
                    {"stmt", stmtObj}
                }
                requests.Add(request)

                Dim requestBody = New Dictionary(Of String, Object) From {
                    {"requests", requests}
                }

                Dim jsonBody = JsonSerializer.Serialize(requestBody)
                Dim content = New StringContent(jsonBody, Encoding.UTF8, "application/json")

                Dim response = Await _httpClient.PostAsync(endpoint, content)
                Dim responseJson = Await response.Content.ReadAsStringAsync()

                If response.IsSuccessStatusCode Then
                    Console.WriteLine("[CsvTursoImport] SQL executed successfully")
                    Return True
                Else
                    Console.WriteLine($"[CsvTursoImport] HTTP error: {response.StatusCode} - {responseJson}")
                    Return False
                End If

            Catch ex As Exception
                Console.WriteLine($"[CsvTursoImport] SQL execution error: {ex.Message}")
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Escape special characters in SQL string values
        ''' </summary>
        Private Function EscapeSqlValue(value As String) As String
            If String.IsNullOrEmpty(value) Then
                Return String.Empty
            End If
            ' Escape single quotes by doubling them
            Return value.Replace("'", "''")
        End Function

        ''' <summary>
        ''' Create table if it doesn't exist
        ''' </summary>
        Public Async Function CreateTableIfNotExistsAsync(tableName As String, columns As Dictionary(Of String, String)) As Task(Of Boolean)
            Try
                ' Build CREATE TABLE statement
                Dim columnDefs As New List(Of String)()
                For Each kvp In columns
                    columnDefs.Add($"{kvp.Key} {kvp.Value}")
                Next

                Dim sql = $"CREATE TABLE IF NOT EXISTS {tableName} ({String.Join(", ", columnDefs)})"
                Return Await ExecuteSqlAsync(sql)

            Catch ex As Exception
                Console.WriteLine($"[CsvTursoImport] Create table error: {ex.Message}")
                Return False
            End Try
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            If Not _disposed Then
                _httpClient?.Dispose()
                _disposed = True
            End If
        End Sub
    End Class
End Namespace

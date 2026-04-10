Imports System
Imports System.IO
Imports System.Net.Http
Imports System.Security.Cryptography
Imports System.Text
Imports System.Threading.Tasks
Imports Microsoft.Win32
Imports System.Text.Json

Namespace Services
    ''' <summary>
    ''' Service to track device usage using Turso libSQL database via HTTP API
    ''' Each device is identified by a unique hardware-based ID
    ''' Uses Turso's REST API directly (no native DLL dependencies)
    ''' Documentation: https://docs.turso.tech/http/reference
    ''' </summary>
    Public Class DeviceCounterService
        Implements IDisposable

        Private ReadOnly _tursoUrl As String = TursoConstants.CounterTursoUrl
        Private ReadOnly _authToken As String = TursoConstants.CounterAuthToken
        Private _httpClient As HttpClient
        Private ReadOnly _deviceId As String
        Private ReadOnly _deviceInfo As DeviceInfo
        Private _disposed As Boolean = False
        Private _isInitialized As Boolean = False

        ''' <summary>
        ''' Expose device ID for use by other services
        ''' </summary>
        Public ReadOnly Property DeviceId As String
            Get
                Return _deviceId
            End Get
        End Property

        ''' <summary>
        ''' Device information structure
        ''' </summary>
        Private Class DeviceInfo
            Property DeviceId As String
            Property MachineName As String
            Property OSVersion As String
            Property DOTNetVersion As String
            Property AppVersion As String
        End Class

        ''' <summary>
        ''' Initialize the device counter service (synchronous - generates device ID only)
        ''' HTTP client is created but no connection yet
        ''' </summary>
        Public Sub New()
            ' Generate unique device ID (synchronous, no DB connection yet)
            _deviceId = GenerateDeviceId()
            _deviceInfo = GetDeviceInfo()

            ' Create HTTP client for Turso REST API
            _httpClient = New HttpClient()
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}")
            _httpClient.Timeout = TimeSpan.FromSeconds(10)

            Console.WriteLine($"[DeviceCounter] Device ID: {_deviceId}")
            Console.WriteLine($"[DeviceCounter] Machine: {_deviceInfo.MachineName}")
            Console.WriteLine("[DeviceCounter] Service initialized (will connect on first use)")
        End Sub

        ''' <summary>
        ''' Generate a unique device ID based on hardware identifiers
        ''' </summary>
        Private Function GenerateDeviceId() As String
            Try
                Dim sb As New StringBuilder()

                ' 1. Machine GUID from registry
                Dim machineGuid = GetMachineGuid()
                If Not String.IsNullOrEmpty(machineGuid) Then
                    sb.Append(machineGuid)
                End If

                ' 2. MAC address
                Dim macAddress = GetMacAddress()
                If Not String.IsNullOrEmpty(macAddress) Then
                    sb.Append("|").Append(macAddress)
                End If

                If sb.Length = 0 Then
                    sb.Append(Environment.UserName)
                    sb.Append("|")
                    sb.Append(Environment.MachineName)
                    sb.Append("|")
                    sb.Append(Guid.NewGuid().ToString())
                End If

                Using sha256 As SHA256 = SHA256.Create()
                    Dim hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()))
                    Dim hexBuilder As New StringBuilder()
                    For i As Integer = 0 To 15
                        hexBuilder.Append(hashBytes(i).ToString("X2"))
                    Next
                    Return hexBuilder.ToString()
                End Using

            Catch ex As Exception
                Console.WriteLine($"[DeviceCounter] Error generating device ID: {ex.Message}")
                Return GetFallbackDeviceId()
            End Try
        End Function

        Private Function GetMachineGuid() As String
            Try
                Using key = Registry.LocalMachine.OpenSubKey("SOFTWARE\Microsoft\Cryptography")
                    If key IsNot Nothing Then
                        Return key.GetValue("MachineGuid")?.ToString()
                    End If
                End Using
            Catch ex As Exception
                Console.WriteLine($"[DeviceCounter] Could not read MachineGuid: {ex.Message}")
            End Try
            Return Nothing
        End Function

        Private Function GetMacAddress() As String
            Try
                Dim nics = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                For Each nic In nics
                    If nic.NetworkInterfaceType <> System.Net.NetworkInformation.NetworkInterfaceType.Loopback AndAlso
                       nic.NetworkInterfaceType <> System.Net.NetworkInformation.NetworkInterfaceType.Tunnel AndAlso
                       nic.OperationalStatus = System.Net.NetworkInformation.OperationalStatus.Up Then
                        Dim mac = nic.GetPhysicalAddress().ToString()
                        If Not String.IsNullOrEmpty(mac) AndAlso mac.Length = 12 Then
                            Return mac
                        End If
                    End If
                Next
            Catch ex As Exception
                Console.WriteLine($"[DeviceCounter] Could not read MAC address: {ex.Message}")
            End Try
            Return Nothing
        End Function

        Private Function GetFallbackDeviceId() As String
            Try
                Using key = Registry.CurrentUser.CreateSubKey("SOFTWARE\PrayerWidget")
                    If key IsNot Nothing Then
                        Dim existingId = key.GetValue("DeviceId")?.ToString()
                        If Not String.IsNullOrEmpty(existingId) Then
                            Return existingId
                        End If
                        Dim newId = Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper()
                        key.SetValue("DeviceId", newId)
                        Return newId
                    End If
                End Using
            Catch ex As Exception
                Console.WriteLine($"[DeviceCounter] Registry fallback failed: {ex.Message}")
            End Try
            Return Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper()
        End Function

        Private Function GetDeviceInfo() As DeviceInfo
            Return New DeviceInfo With {
                .DeviceId = _deviceId,
                .MachineName = Environment.MachineName,
                .OSVersion = Environment.OSVersion.ToString(),
                .DOTNetVersion = Environment.Version.ToString(),
                .AppVersion = GetAppVersion()
            }
        End Function

        Private Function GetAppVersion() As String
            Try
                Dim assembly = System.Reflection.Assembly.GetExecutingAssembly()
                Dim version = assembly.GetName().Version
                Return $"{version.Major}.{version.Minor}.{version.Build}"
            Catch
                Return "1.0.10"
            End Try
        End Function

        ''' <summary>
        ''' Execute SQL via Turso HTTP API
        ''' Turso pipeline format: { requests: [{ type: "execute", stmt: { sql: "..." } }] }
        ''' For simplicity, embed values directly in SQL (avoiding parameterized args complexity)
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
                        Dim escapedValue = arg.ToString().Replace("'", "''")
                        Dim placeholderIndex = finalSql.IndexOf("?"c)
                        If placeholderIndex >= 0 Then
                            finalSql = finalSql.Substring(0, placeholderIndex) & $"'{escapedValue}'" & finalSql.Substring(placeholderIndex + 1)
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
                    Console.WriteLine($"[DeviceCounter] SQL executed successfully")
                    Return True
                Else
                    Console.WriteLine($"[DeviceCounter] HTTP error: {response.StatusCode} - {responseJson}")
                    Return False
                End If

            Catch ex As Exception
                Console.WriteLine($"[DeviceCounter] SQL execution error: {ex.Message}")
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Initialize the counter table in Turso database
        ''' </summary>
        Public Async Function InitializeTableAsync() As Task
            If _isInitialized Then Return

            Dim sql = "CREATE TABLE IF NOT EXISTS device_counters (
                device_id TEXT PRIMARY KEY,
                machine_name TEXT,
                os_version TEXT,
                dotnet_version TEXT,
                app_version TEXT,
                first_seen TEXT,
                last_seen TEXT,
                total_launches INTEGER DEFAULT 0,
                country TEXT,
                city TEXT
            )"

            Await ExecuteSqlAsync(sql)
            _isInitialized = True
            Console.WriteLine("[DeviceCounter] Table initialized successfully")
        End Function

        ''' <summary>
        ''' Increment the counter for this device (call on app launch)
        ''' Uses UPSERT pattern for atomic increment
        ''' </summary>
        Public Async Function IncrementCounterAsync() As Task
            Try
                Await InitializeTableAsync()

                ' UPSERT: Insert new record or update existing one
                Dim sql = "INSERT INTO device_counters (device_id, machine_name, os_version, dotnet_version, app_version, first_seen, last_seen, total_launches, country)
                          VALUES (?, ?, ?, ?, ?, datetime('now'), datetime('now'), 1, ?)
                          ON CONFLICT(device_id) DO UPDATE SET
                              last_seen = datetime('now'),
                              total_launches = total_launches + 1,
                              app_version = excluded.app_version"

                Await ExecuteSqlAsync(sql, New Object() {
                    _deviceId,
                    _deviceInfo.MachineName,
                    _deviceInfo.OSVersion,
                    _deviceInfo.DOTNetVersion,
                    _deviceInfo.AppVersion,
                    "EG" ' Default country
                })

                Console.WriteLine($"[DeviceCounter] Counter incremented for device: {_deviceId}")

            Catch ex As Exception
                Console.WriteLine($"[DeviceCounter] Error incrementing counter: {ex.Message}")
            End Try
        End Function

        ''' <summary>
        ''' Update last_seen timestamp for this device (call on prayer times)
        ''' Only updates the last_seen field without incrementing total_launches
        ''' </summary>
        Public Async Function UpdateLastSeenAsync() As Task
            Try
                Await InitializeTableAsync()

                ' Update only last_seen timestamp
                Dim sql = "UPDATE device_counters SET last_seen = datetime('now') WHERE device_id = ?"

                Await ExecuteSqlAsync(sql, New Object() {_deviceId})

                Console.WriteLine($"[DeviceCounter] Last seen updated for device: {_deviceId}")

            Catch ex As Exception
                Console.WriteLine($"[DeviceCounter] Error updating last seen: {ex.Message}")
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

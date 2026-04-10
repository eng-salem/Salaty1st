Imports System.IO
Imports System.Reflection

Namespace Services
    ''' <summary>
    ''' Manages database file location - copies from install directory to user's AppData for write access
    ''' </summary>
    Public Class DatabaseManager
        Private Shared ReadOnly _appDataPath As String
        Private Shared ReadOnly _installPath As String

        Shared Sub New()
            ' AppData path: C:\Users\<user>\AppData\Local\PrayerWidget\salaty.sqlite
            _appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PrayerWidget",
                "salaty.sqlite")

            ' Install path: Where the app is installed (may be read-only)
            _installPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "salaty.sqlite")
        End Sub

        ''' <summary>
        ''' Gets the database path - ensures database is copied to AppData if needed
        ''' </summary>
        Public Shared Function GetDatabasePath() As String
            ' Ensure directory exists
            Dim appDataDir = Path.GetDirectoryName(_appDataPath)
            If Not Directory.Exists(appDataDir) Then
                Directory.CreateDirectory(appDataDir)
            End If

            ' Copy database to AppData if it doesn't exist or is older than install version
            If Not File.Exists(_appDataPath) OrElse IsInstallDbNewer() Then
                CopyDatabaseToAppData()
            End If

            Return _appDataPath
        End Function

        ''' <summary>
        ''' Gets the AppData database path without copying (for checking existence)
        ''' </summary>
        Public Shared Function GetAppDataPath() As String
            Return _appDataPath
        End Function

        ''' <summary>
        ''' Gets the install directory database path (read-only source)
        ''' </summary>
        Public Shared Function GetInstallPath() As String
            Return _installPath
        End Function

        ''' <summary>
        ''' Copies database from install directory to AppData
        ''' </summary>
        Private Shared Sub CopyDatabaseToAppData()
            Try
                If File.Exists(_installPath) Then
                    ' Ensure directory exists
                    Dim appDataDir = Path.GetDirectoryName(_appDataPath)
                    If Not Directory.Exists(appDataDir) Then
                        Directory.CreateDirectory(appDataDir)
                        Console.WriteLine($"Created AppData directory: {appDataDir}")
                    End If

                    ' Copy file, overwrite if exists
                    File.Copy(_installPath, _appDataPath, True)
                    Console.WriteLine($"✓ Database copied to AppData: {_appDataPath}")
                    Console.WriteLine($"  Install path: {_installPath}")
                Else
                    Console.WriteLine($"✗ Database not found at install path: {_installPath}")
                    Console.WriteLine($"  Base directory: {AppDomain.CurrentDomain.BaseDirectory}")
                End If
            Catch ex As Exception
                Console.WriteLine($"✗ Error copying database: {ex.GetType().Name}: {ex.Message}")
                Console.WriteLine($"  From: {_installPath}")
                Console.WriteLine($"  To: {_appDataPath}")
                ' Don't fall back to install path - it will fail on writes
                ' Let the app continue and handle the error when trying to write
            End Try
        End Sub

        ''' <summary>
        ''' Checks if install database is newer than AppData database
        ''' </summary>
        Private Shared Function IsInstallDbNewer() As Boolean
            If Not File.Exists(_installPath) OrElse Not File.Exists(_appDataPath) Then
                Return False
            End If

            Dim installTime = File.GetLastWriteTime(_installPath)
            Dim appDataTime = File.GetLastWriteTime(_appDataPath)

            Return installTime > appDataTime
        End Function

        ''' <summary>
        ''' Migrates existing settings from old location to AppData (called on first run)
        ''' </summary>
        Public Shared Sub MigrateSettingsIfNeeded()
            ' Settings will be saved to AppData automatically when using GetDatabasePath()
            ' No migration needed as settings are stored in the database itself
        End Sub
    End Class
End Namespace

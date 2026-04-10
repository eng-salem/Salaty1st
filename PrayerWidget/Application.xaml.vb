Imports System.IO
Imports System.Threading.Tasks
Imports Salaty.First.Services

Class Application
    Private _analyticsService As GoogleAnalyticsService
    Private _deviceCounterService As DeviceCounterService

    ''' <summary>
    ''' Public access to device counter service for updating last_seen on prayer times
    ''' </summary>
    Public ReadOnly Property DeviceCounterService As DeviceCounterService
        Get
            Return _deviceCounterService
        End Get
    End Property

    Private Sub Application_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
        ' Add global exception handler
        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf CurrentDomain_UnhandledException
        AddHandler DispatcherUnhandledException, AddressOf Application_DispatcherUnhandledException

        ' Redirect console output to file for debugging
        Dim logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PrayerWidget",
            "debug.log")

        Dim logDir = Path.GetDirectoryName(logPath)
        If Not Directory.Exists(logDir) Then
            Directory.CreateDirectory(logDir)
        End If

        Dim writer As New StreamWriter(logPath)
        writer.AutoFlush = True
        Console.SetOut(writer)
        Console.WriteLine("=== PrayerWidget Started ===")
        Console.WriteLine("Time: " & DateTime.Now)
        Console.WriteLine("Path: " & AppDomain.CurrentDomain.BaseDirectory)
        Console.WriteLine("Log: " & logPath)

        Try
            ' Initialize analytics
            _analyticsService = New GoogleAnalyticsService()

            ' Initialize device counter service (non-blocking)
            _deviceCounterService = New DeviceCounterService()

            ' Fire and forget - don't block startup
            TrackAppLaunchAsync()
        Catch ex As Exception
            Console.WriteLine($"[App] Startup error: {ex.GetType().Name}: {ex.Message}")
            Console.WriteLine($"[App] StackTrace: {ex.StackTrace}")
        End Try
    End Sub

    Private Sub Application_DispatcherUnhandledException(sender As Object, e As System.Windows.Threading.DispatcherUnhandledExceptionEventArgs)
        Console.WriteLine($"[App] Dispatcher Unhandled Exception: {e.Exception.GetType().Name}: {e.Exception.Message}")
        Console.WriteLine($"[App] StackTrace: {e.Exception.StackTrace}")
        If e.Exception.InnerException IsNot Nothing Then
            Console.WriteLine($"[App] Inner Exception: {e.Exception.InnerException.GetType().Name}: {e.Exception.InnerException.Message}")
            Console.WriteLine($"[App] Inner StackTrace: {e.Exception.InnerException.StackTrace}")
        End If
        e.Handled = True
        MessageBox.Show($"An error occurred: {e.Exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
    End Sub

    Private Sub CurrentDomain_UnhandledException(sender As Object, e As UnhandledExceptionEventArgs)
        Dim ex As Exception = TryCast(e.ExceptionObject, Exception)
        If ex IsNot Nothing Then
            Console.WriteLine($"[App] Unhandled Exception: {ex.GetType().Name}: {ex.Message}")
            Console.WriteLine($"[App] StackTrace: {ex.StackTrace}")
        End If
    End Sub

    Private Sub Application_Exit(sender As Object, e As ExitEventArgs) Handles Me.Exit
        ' Track app close
        If _analyticsService IsNot Nothing Then
            Try
                _analyticsService.TrackAppClose().Wait(TimeSpan.FromMilliseconds(500))
            Catch
            End Try
        End If

        ' Dispose device counter service
        If _deviceCounterService IsNot Nothing Then
            Try
                _deviceCounterService.Dispose()
            Catch
            End Try
        End If
    End Sub

    Private Async Sub TrackAppLaunchAsync()
        Try
            ' Track with Google Analytics (counterapi.dev)
            Await _analyticsService.TrackAppLaunch()

            ' Track with Turso device counter
            Await _deviceCounterService.InitializeTableAsync()
            Await _deviceCounterService.IncrementCounterAsync()

        Catch ex As Exception
            Console.WriteLine($"[App] Error tracking launch: {ex.Message}")
        End Try
    End Sub
End Class

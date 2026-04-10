Imports System.Windows.Threading
Imports System.Windows.Media
Imports Microsoft.Win32
Imports WpfColor = System.Windows.Media.Color

Public Class ToastNotification
    Private _isDragging As Boolean = False
    Private _isSwiping As Boolean = False
    Private _dragStartPoint As Point
    Private _swipeStartPoint As Point
    Private _settingsManager As SettingsManager
    Private _mediaPlayer As MediaPlayer
    Private _isPlaying As Boolean = False
    Private _updateTimer As DispatcherTimer
    Private _currentSoundPath As String
    Private _volume As Double = 100
    Private _isDarkTheme As Boolean = True

    ' Swipe to dismiss constants
    Private Const SWIPE_THRESHOLD As Double = 150
    Private Const SWIPE_ANIMATION_DURATION As Integer = 200 ' milliseconds

    Public Sub New(title As String, message As String, Optional opacity As Double = 95, Optional settingsManager As SettingsManager = Nothing, Optional soundPath As String = Nothing, Optional volume As Double = 100)
        InitializeComponent()

        TitleText.Text = title
        MessageText.Text = message
        _settingsManager = settingsManager
        _currentSoundPath = soundPath
        _volume = volume

        ' Detect Windows theme (dark/light)
        _isDarkTheme = IsWindowsDarkMode()
        ApplyTheme()

        ' Apply opacity to border background color
        ApplyOpacity(opacity)

        ' Hide media controls by default (only show when athan audio is playing)
        If MediaControlsBorder IsNot Nothing Then
            MediaControlsBorder.Visibility = Visibility.Collapsed
        End If

        ' Initialize media player if sound path provided
        If Not String.IsNullOrEmpty(soundPath) Then
            InitializeMediaPlayer(soundPath)
            ' Show media controls when actual audio is playing
            If MediaControlsBorder IsNot Nothing Then
                MediaControlsBorder.Visibility = Visibility.Visible
            End If
        End If
    End Sub

    ''' <summary>
    ''' Detect if Windows is using dark mode
    ''' </summary>
    Private Function IsWindowsDarkMode() As Boolean
        Try
            Using key = Registry.CurrentUser.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize")
                If key IsNot Nothing Then
                    Dim appsUseLightTheme = key.GetValue("AppsUseLightTheme")
                    If appsUseLightTheme IsNot Nothing Then
                        Return CInt(appsUseLightTheme) = 0
                    End If
                End If
            End Using
        Catch
        End Try
        Return True ' Default to dark mode
    End Function

    ''' <summary>
    ''' Apply Windows theme colors to notification
    ''' </summary>
    Private Sub ApplyTheme()
        Dim bgColor As WpfColor
        Dim textColor As WpfColor
        Dim borderColor As WpfColor

        If _isDarkTheme Then
            ' Dark mode colors
            bgColor = WpfColor.FromArgb(230, 32, 32, 32)
            textColor = WpfColor.FromRgb(255, 255, 255)
            borderColor = WpfColor.FromRgb(60, 60, 60)
        Else
            ' Light mode colors
            bgColor = WpfColor.FromArgb(230, 243, 243, 243)
            textColor = WpfColor.FromRgb(32, 32, 32)
            borderColor = WpfColor.FromRgb(200, 200, 200)
        End If

        NotificationBorder.Background = New SolidColorBrush(bgColor)
        NotificationBorder.BorderBrush = New SolidColorBrush(borderColor)
        MessageText.Foreground = New SolidColorBrush(textColor)
        TitleText.Foreground = New SolidColorBrush(WpfColor.FromRgb(79, 172, 254))

        Console.WriteLine($"[Toast] Theme: {If(_isDarkTheme, "Dark", "Light")}")
    End Sub

    Private Sub InitializeMediaPlayer(soundPath As String)
        Try
            _mediaPlayer = New MediaPlayer()

            ' Set up progress update timer
            _updateTimer = New DispatcherTimer()
            _updateTimer.Interval = TimeSpan.FromMilliseconds(500)
            AddHandler _updateTimer.Tick, AddressOf UpdateProgress

            ' Handle media opened - auto-play when loaded
            AddHandler _mediaPlayer.MediaOpened, Sub(s, e)
                Console.WriteLine($"[ToastNotification] MediaOpened: {soundPath}")
                _mediaPlayer.Volume = _volume ' Use passed volume (0-100 scale)
                
                ' Normal playback speed
                _isPlaying = True
                UpdatePlayPauseButton()
                _updateTimer.Start()
                NowPlayingText.Text = "Now Playing: Athan"
                
                ' Ensure media controls are visible
                If ProgressSlider IsNot Nothing Then ProgressSlider.Visibility = Visibility.Visible
                If BtnPlayPause IsNot Nothing Then BtnPlayPause.Visibility = Visibility.Visible
                If BtnStop IsNot Nothing Then BtnStop.Visibility = Visibility.Visible
                If VolumeSlider IsNot Nothing Then VolumeSlider.Visibility = Visibility.Visible
            End Sub

            ' Handle media ended
            AddHandler _mediaPlayer.MediaEnded, Sub(s, e)
                Console.WriteLine("[ToastNotification] MediaEnded - playback completed")
                _isPlaying = False
                UpdatePlayPauseButton()
                _updateTimer.Stop()
            End Sub

            ' Handle media failed - show toast even if sound fails
            AddHandler _mediaPlayer.MediaFailed, Sub(s, e)
                Console.WriteLine($"[ToastNotification] MediaFailed: {e.ErrorException?.Message}")
                _isPlaying = False
                UpdatePlayPauseButton()
                _updateTimer.Stop()
                NowPlayingText.Text = "Audio unavailable (file may be in use)"
                ' FIX 3: Keep media controls visible even if sound fails
                ' User needs play/stop buttons to control the toast
                If ProgressSlider IsNot Nothing Then ProgressSlider.Visibility = Visibility.Visible
                If BtnPlayPause IsNot Nothing Then BtnPlayPause.Visibility = Visibility.Visible
                If BtnStop IsNot Nothing Then BtnStop.Visibility = Visibility.Visible
                If VolumeSlider IsNot Nothing Then VolumeSlider.Visibility = Visibility.Visible
            End Sub

            ' Set initial volume slider position
            VolumeSlider.Value = _volume

            ' Open the media file
            Console.WriteLine($"[ToastNotification] Opening media file: {soundPath}")
            _mediaPlayer.Open(New Uri(soundPath))

        Catch ex As Exception
            Console.WriteLine($"[ToastNotification] Error initializing media player: {ex.Message}")
            NowPlayingText.Text = "Audio unavailable"
        End Try
    End Sub

    Private Sub UpdateProgress(sender As Object, e As EventArgs)
        If _mediaPlayer IsNot Nothing AndAlso _mediaPlayer.NaturalDuration.HasTimeSpan Then
            Dim position = _mediaPlayer.Position.TotalSeconds
            Dim duration = _mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds
            If duration > 0 Then
                ProgressSlider.Value = (position / duration) * 100
            End If
        End If
    End Sub

    Private Sub ApplyOpacity(opacity As Double)
        ' Convert opacity percentage to alpha (0-255) and apply to theme-based background
        Dim alpha As Byte = CByte(opacity / 100.0 * 255)

        ' Update the existing background color with new alpha
        If _isDarkTheme Then
            Dim bgColor As WpfColor = WpfColor.FromArgb(alpha, 32, 32, 32)
            NotificationBorder.Background = New SolidColorBrush(bgColor)
        Else
            Dim bgColor As WpfColor = WpfColor.FromArgb(alpha, 243, 243, 243)
            NotificationBorder.Background = New SolidColorBrush(bgColor)
        End If

        Console.WriteLine($"Toast opacity: {opacity}% -> Alpha={alpha}")
    End Sub

    Private Sub BtnClose_Click(sender As Object, e As RoutedEventArgs)
        ' Stop media and cleanup
        StopMedia()
        
        ' Also stop athan in MainWindow to prevent sound from continuing
        Try
            Dim mainWindow = Application.Current.MainWindow
            If mainWindow IsNot Nothing Then
                ' Use reflection to call StopAthan method
                Dim stopAthanMethod = mainWindow.GetType().GetMethod("StopAthan", Reflection.BindingFlags.Public Or Reflection.BindingFlags.Instance)
                If stopAthanMethod IsNot Nothing Then
                    stopAthanMethod.Invoke(mainWindow, Nothing)
                    Console.WriteLine("[ToastNotification] Stopped athan sound from MainWindow")
                End If
            End If
        Catch ex As Exception
            Console.WriteLine("[ToastNotification] Error stopping athan: " & ex.Message)
        End Try
        
        Me.Close()
    End Sub

    Private Sub BtnSettings_Click(sender As Object, e As RoutedEventArgs)
        ' Stop media
        StopMedia()
        
        ' Close the notification first
        Me.Close()

        ' Then open settings window
        Dim settingsWindow As New SettingsWindow(_settingsManager)
        settingsWindow.Owner = Application.Current.MainWindow

        ' Check if user saved changes
        Dim result As Boolean? = settingsWindow.ShowDialog()

        ' If settings were saved, check if we need to show the widget
        If result = True Then
            ' Reload the ShowWidget setting and apply it
            Dim showWidget As String = _settingsManager.GetSetting("ShowWidget", "1")
            Dim shouldBeVisible As Boolean = (showWidget = "1")

            Console.WriteLine($"Toast BtnSettings: ShowWidget={showWidget}, shouldBeVisible={shouldBeVisible}")

            ' Get main window and show it if needed
            Dim mainWindow = TryCast(Application.Current.MainWindow, MainWindow)
            If mainWindow IsNot Nothing Then
                If shouldBeVisible Then
                    mainWindow.Show()
                    mainWindow.Activate()
                    mainWindow._widgetVisible = True
                    mainWindow.UpdateToggleMenuText("Hide Widget")
                    Console.WriteLine("Toast: Widget shown")
                Else
                    mainWindow.Hide()
                    mainWindow._widgetVisible = False
                    mainWindow.UpdateToggleMenuText("Show Widget")
                    Console.WriteLine("Toast: Widget hidden")
                End If
            End If
        End If
    End Sub

    Private Sub BtnPlayPause_Click(sender As Object, e As RoutedEventArgs)
        If _mediaPlayer Is Nothing Then Return

        If _isPlaying Then
            _mediaPlayer.Pause()
            _isPlaying = False
        Else
            _mediaPlayer.Play()
            _isPlaying = True
            _updateTimer.Start()
        End If
        UpdatePlayPauseButton()
    End Sub

    Private Sub BtnStop_Click(sender As Object, e As RoutedEventArgs)
        StopMedia()
    End Sub

    Private Sub StopMedia()
        If _mediaPlayer IsNot Nothing Then
            _mediaPlayer.Stop()
            _isPlaying = False
            ProgressSlider.Value = 0
            _updateTimer.Stop()
            UpdatePlayPauseButton()
            NowPlayingText.Text = "Stopped"
        End If
    End Sub

    Private Sub UpdatePlayPauseButton()
        If _isPlaying Then
            BtnPlayPause.Content = "⏸" ' Pause icon
            BtnPlayPause.ToolTip = "Pause"
        Else
            BtnPlayPause.Content = "▶" ' Play icon
            BtnPlayPause.ToolTip = "Play"
        End If
    End Sub

    Private Sub VolumeSlider_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If _mediaPlayer IsNot Nothing Then
            ' VolumeSlider is 0-100, MediaPlayer.Volume is 0-1
            _mediaPlayer.Volume = VolumeSlider.Value / 100.0
        End If
    End Sub

    Private Sub Window_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        _dragStartPoint = e.GetPosition(Me)
        _swipeStartPoint = _dragStartPoint
        _isDragging = True
        _isSwiping = False
        Me.CaptureMouse()
    End Sub

    Private Sub Window_MouseMove(sender As Object, e As MouseEventArgs)
        If _isDragging AndAlso Me.IsMouseCaptured Then
            Dim currentPoint As Point = e.GetPosition(Me)
            Dim deltaX As Double = currentPoint.X - _dragStartPoint.X
            Dim deltaY As Double = currentPoint.Y - _dragStartPoint.Y

            ' Check if user is swiping right (horizontal drag more than vertical)
            If Math.Abs(deltaX) > Math.Abs(deltaY) AndAlso deltaX > 10 Then
                _isSwiping = True
                Dim swipeAmount As Double = Math.Max(0, Math.Min(deltaX, 200))
                SwipeTransform.X = swipeAmount
                Me.Opacity = 1.0 - (swipeAmount / 300.0)
                If swipeAmount >= SWIPE_THRESHOLD Then
                    Me.Close()
                End If
            ElseIf _isSwiping = False Then
                ' Regular drag move
                Me.Left += deltaX
                Me.Top += deltaY
                _dragStartPoint = currentPoint
            End If
        End If
    End Sub

    Private Sub Window_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
        ReleaseDrag()
    End Sub

    Private Sub Window_LostMouseCapture(sender As Object, e As MouseEventArgs)
        ReleaseDrag()
    End Sub

    Private Sub ReleaseDrag()
        If Not _isDragging Then Return
        _isDragging = False
        Me.ReleaseMouseCapture()

        ' If swiping but didn't reach threshold, animate back
        If _isSwiping Then
            _isSwiping = False
            Dim startX = SwipeTransform.X
            If startX > 0 Then
                AnimateSwipeBack(startX)
            End If
        End If
    End Sub

    Private Sub AnimateSwipeBack(startX As Double)
        Dim animationTimer As New DispatcherTimer()
        animationTimer.Interval = TimeSpan.FromMilliseconds(16) ' ~60fps
        Dim startTime As DateTime = DateTime.Now

        Dim handler As EventHandler = Nothing
        handler = Sub(s, args)
                      Dim elapsed = (DateTime.Now - startTime).TotalMilliseconds
                      Dim progress = Math.Min(1.0, elapsed / SWIPE_ANIMATION_DURATION)
                      Dim eased = 1 - Math.Pow(1 - progress, 3)
                      SwipeTransform.X = startX * (1 - eased)
                      Me.Opacity = 1.0 - (SwipeTransform.X / 300.0)

                      If progress >= 1.0 Then
                          animationTimer.Stop()
                          RemoveHandler animationTimer.Tick, handler
                          SwipeTransform.X = 0
                          Me.Opacity = 1.0
                      End If
                  End Sub

        AddHandler animationTimer.Tick, handler
        animationTimer.Start()
    End Sub

    Protected Overrides Sub OnClosed(e As EventArgs)
        MyBase.OnClosed(e)
        ' Cleanup
        If _updateTimer IsNot Nothing Then
            _updateTimer.Stop()
        End If
        StopMedia()
        
        ' Also stop athan in MainWindow to prevent sound from continuing
        Dim mainWindow = Application.Current.MainWindow
        If mainWindow IsNot Nothing Then
            Try
                Dim stopAthanMethod = mainWindow.GetType().GetMethod("StopAthan")
                If stopAthanMethod IsNot Nothing Then
                    stopAthanMethod.Invoke(mainWindow, Nothing)
                End If
            Catch
                ' Ignore errors - MainWindow may have different structure
            End Try
        End If
    End Sub
End Class

Imports System.Windows
Imports System.Windows.Threading
Imports System.Windows.Media
Imports System.Windows.Controls

Public Class QuoteNotificationWindow
    Inherits Window

    Private _autoCloseTimer As DispatcherTimer
    Private _mediaPlayer As MediaPlayer
    Private _settingsManager As SettingsManager
    Private _currentQuote As IslamicQuote

    Public Sub New(quote As IslamicQuote, Optional displaySeconds As Integer = 10, Optional playAudio As Boolean = True)
        InitializeComponent()

        _settingsManager = New SettingsManager()
        _currentQuote = quote

        ' Set quote text
        If quote IsNot Nothing Then
            ' Clean up quote text - remove extra whitespace and handle special characters
            Dim cleanQuote As String = CleanQuoteText(quote.quote)
            TxtQuote.Text = cleanQuote
            
            ' Set source if available
            If Not String.IsNullOrEmpty(quote.source) Then
                TxtSource.Text = $"- {quote.source}"
            Else
                TxtSource.Text = ""
            End If
        Else
            TxtQuote.Text = "Could not load quote at this time."
            TxtSource.Text = ""
        End If

        ' Position at bottom-right of screen
        PositionWindow()

        ' Play audio if enabled
        If playAudio Then
            PlayQuoteAudio()
        End If

        ' Auto-close after 60 seconds (1 minute) - increased from default for reading time
        Dim displayTime As Integer = If(displaySeconds > 0, displaySeconds, 60)
        StartAutoCloseTimer(displayTime)
    End Sub

    ''' <summary>
    ''' Clean quote text for display - preserves Arabic text and special characters
    ''' </summary>
    Private Function CleanQuoteText(text As String) As String
        If String.IsNullOrEmpty(text) Then Return text
        
        ' Remove any HTML tags if present
        Dim cleaned = System.Text.RegularExpressions.Regex.Replace(text, "<[^>]+>", String.Empty)
        
        ' Decode common HTML entities
        cleaned = cleaned.Replace("&quot;", """")
        cleaned = cleaned.Replace("&#39;", "'")
        cleaned = cleaned.Replace("&amp;", "&")
        cleaned = cleaned.Replace("&lt;", "<")
        cleaned = cleaned.Replace("&gt;", ">")
        
        ' Normalize whitespace but preserve line breaks for multi-line quotes
        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, "[ \t]+", " ")
        
        ' Trim each line separately to preserve multi-line structure
        Dim lines = cleaned.Split(New String() {Environment.NewLine, vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries)
        For i As Integer = 0 To lines.Length - 1
            lines(i) = lines(i).Trim()
        Next
        
        Return String.Join(Environment.NewLine, lines)
    End Function

    Private Sub PlayQuoteAudio()
        Try
            ' Check if quote audio is enabled in settings
            Dim enableQuoteAudio As String = _settingsManager.GetSetting("EnableQuoteAudio", "1")
            If enableQuoteAudio <> "1" Then
                Return
            End If

            ' Find quotes.mp3 in Resources/MP3 folder
            Dim mp3Folder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "MP3")
            Dim quotesSoundPath = System.IO.Path.Combine(mp3Folder, "quotes.mp3")

            If System.IO.File.Exists(quotesSoundPath) Then
                _mediaPlayer = New MediaPlayer()
                _mediaPlayer.Open(New Uri(quotesSoundPath))

                AddHandler _mediaPlayer.MediaOpened, Sub(s, e)
                    _mediaPlayer.Play()
                End Sub

                AddHandler _mediaPlayer.MediaFailed, Sub(s, e)
                    Console.WriteLine("Failed to load quotes.mp3")
                End Sub
            Else
                Console.WriteLine("quotes.mp3 not found at: " & quotesSoundPath)
            End If
        Catch ex As Exception
            Console.WriteLine("Error playing quote audio: " & ex.Message)
        End Try
    End Sub

    Private Sub PositionWindow()
        Dim screenWidth As Double = SystemParameters.WorkArea.Width + SystemParameters.WorkArea.Left
        Dim screenHeight As Double = SystemParameters.WorkArea.Height + SystemParameters.WorkArea.Top
        Dim taskbarHeight As Double = SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Height

        Me.Left = screenWidth - Me.Width - 10
        Me.Top = screenHeight - Me.Height - taskbarHeight - 10
    End Sub

    Private Sub StartAutoCloseTimer(seconds As Integer)
        _autoCloseTimer = New DispatcherTimer()
        _autoCloseTimer.Interval = TimeSpan.FromSeconds(seconds)
        AddHandler _autoCloseTimer.Tick, Sub(s, args)
            _autoCloseTimer.Stop()
            Me.Close()
        End Sub
        _autoCloseTimer.Start()
    End Sub

    Private Sub BtnClose_Click(sender As Object, e As RoutedEventArgs)
        If _autoCloseTimer IsNot Nothing Then
            _autoCloseTimer.Stop()
        End If
        Me.Close()
    End Sub

    Private Sub BtnCopy_Click(sender As Object, e As RoutedEventArgs)
        If _currentQuote IsNot Nothing AndAlso Not String.IsNullOrEmpty(_currentQuote.quote) Then
            ' Copy quote text to clipboard
            Dim quoteText As String = CleanQuoteText(_currentQuote.quote)
            
            ' Include source if available
            If Not String.IsNullOrEmpty(_currentQuote.source) Then
                quoteText &= Environment.NewLine & Environment.NewLine & _currentQuote.source
            End If
            
            Clipboard.SetText(quoteText)
            
            ' Show visual feedback
            BtnCopy.Content = "✓ Copied!"
            BtnCopy.ToolTip = "Copied to clipboard"
            
            ' Reset button after 2 seconds
            Dim resetTimer As New DispatcherTimer()
            resetTimer.Interval = TimeSpan.FromSeconds(2)
            AddHandler resetTimer.Tick, Sub(s, args)
                resetTimer.Stop()
                BtnCopy.Content = "📋 Copy"
                BtnCopy.ToolTip = "Copy quote to clipboard"
            End Sub
            resetTimer.Start()
        End If
    End Sub

    Protected Overrides Sub OnClosed(e As EventArgs)
        MyBase.OnClosed(e)
        If _autoCloseTimer IsNot Nothing Then
            _autoCloseTimer.Stop()
        End If
        If _mediaPlayer IsNot Nothing Then
            _mediaPlayer.Stop()
            _mediaPlayer = Nothing
        End If
    End Sub
End Class

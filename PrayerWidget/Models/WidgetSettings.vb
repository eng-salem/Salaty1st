Namespace Models
    Public Class WidgetSettings
        ' Widget Appearance
        Public Property Size As String
        Public Property WindowWidth As Integer
        Public Property WindowHeight As Integer
        Public Property GridMargin As Integer
        Public Property CornerRadius As Integer
        Public Property StrokeThickness As Double
        Public Property Circumference As Double
        Public Property TitleFontSize As Double
        Public Property PrayerFontSize As Double
        Public Property CountdownFontSize As Double

        ' Location & Calculation
        Public Property City As String
        Public Property Country As String
        Public Property Method As Integer
        Public Property MethodName As String
        
        ' Asr Method (0=Shafii, 1=Hanafi)
        Public Property AsrMethod As Integer = 0

        ' Sound
        Public Property AthanSound As String
        
        ' Per-prayer Athan sounds
        Public Property FajrAthanSound As String = "default"
        Public Property DhuhrAthanSound As String = "default"
        Public Property AsrAthanSound As String = "default"
        Public Property MaghribAthanSound As String = "default"
        Public Property IshaAthanSound As String = "default"

        ' Duaa after Athan
        Public Property EnableDuaaAfterAthan As Boolean = False
        Public Property DuaaSound As String = "sha3rawy_duaa"

        ' Volume settings
        Public Property Volume As Integer = 100
        Public Property FajrVolume As Integer = 100
        Public Property IshaVolume As Integer = 100
        Public Property EnableVolumeSchedule As Boolean = False
        
        ' Appearance
        Public Property WidgetOpacity As Double = 70
        Public Property NotificationOpacity As Double = 95
        Public Property NotificationOpacityMin As Double = 20

        ' Progress Ring Visibility
        Public Property ShowProgressRing As Boolean = True

        ' Window Behavior
        Public Property AlwaysOnTop As Boolean = True
        
        ' Islamic Quotes
        Public Property EnableQuotes As Boolean = True
        Public Property QuoteInterval As Integer = 1
        Public Property EnableQuoteAudio As Boolean = True

        ' Hijri date adjustment (-1, 0, or +1 day)
        Public Property HijriAdjustment As Integer = 0
        
        ' First run flag
        Public Property HasCompletedSetup As Boolean = False

        ' Summer Time (Daylight Saving)
        Public Property IsSummerTime As Boolean = False

        Public Sub New()
            ' Default: Medium
            Size = "Medium"
            WindowWidth = 180
            WindowHeight = 180
            GridMargin = 8
            CornerRadius = 75
            StrokeThickness = 3.0
            Circumference = 471.0
            TitleFontSize = 8.0
            PrayerFontSize = 24.0
            CountdownFontSize = 12.0

            ' Default Location
            City = "Cairo"
            Country = "Egypt"
            Method = 5 ' Umm Al-Qura
            MethodName = "Egyptian General Authority of Survey"

            ' Default Sound
            AthanSound = "default"
        End Sub

        Public Shared Function GetSizePreset(sizeName As String) As WidgetSettings
            Dim settings As New WidgetSettings()
            settings.Size = sizeName

            Select Case sizeName.ToUpper()
                Case "SMALL"
                    settings.WindowWidth = 150
                    settings.WindowHeight = 150
                    settings.GridMargin = 6
                    settings.CornerRadius = 60
                    settings.StrokeThickness = 2.5
                    settings.Circumference = 377.0
                    settings.TitleFontSize = 7.0
                    settings.PrayerFontSize = 20.0
                    settings.CountdownFontSize = 10.0

                Case "MEDIUM"
                    settings.WindowWidth = 180
                    settings.WindowHeight = 180
                    settings.GridMargin = 8
                    settings.CornerRadius = 75
                    settings.StrokeThickness = 3.0
                    settings.Circumference = 471.0
                    settings.TitleFontSize = 8.0
                    settings.PrayerFontSize = 24.0
                    settings.CountdownFontSize = 12.0

                Case "LARGE"
                    settings.WindowWidth = 240
                    settings.WindowHeight = 240
                    settings.GridMargin = 10
                    settings.CornerRadius = 100
                    settings.StrokeThickness = 4.0
                    settings.Circumference = 628.0
                    settings.TitleFontSize = 10.0
                    settings.PrayerFontSize = 32.0
                    settings.CountdownFontSize = 16.0
            End Select

            Return settings
        End Function


    End Class
End Namespace
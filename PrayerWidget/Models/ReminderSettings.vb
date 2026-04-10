Namespace Models
    Public Class PrayerReminder
        Public Property PrayerName As String
        Public Property EnabledBefore As Boolean
        Public Property EnabledAfter As Boolean
        Public Property MinutesBefore As Integer
        Public Property MinutesAfter As Integer
    End Class

    Public Class ReminderSettings
        Public Property FajrReminder As PrayerReminder
        Public Property DhuhrReminder As PrayerReminder
        Public Property AsrReminder As PrayerReminder
        Public Property MaghribReminder As PrayerReminder
        Public Property IshaReminder As PrayerReminder

        Public Sub New()
            FajrReminder = New PrayerReminder With {.PrayerName = "Fajr", .EnabledBefore = False, .EnabledAfter = False, .MinutesBefore = 10, .MinutesAfter = 15}
            DhuhrReminder = New PrayerReminder With {.PrayerName = "Dhuhr", .EnabledBefore = False, .EnabledAfter = False, .MinutesBefore = 10, .MinutesAfter = 15}
            AsrReminder = New PrayerReminder With {.PrayerName = "Asr", .EnabledBefore = False, .EnabledAfter = False, .MinutesBefore = 10, .MinutesAfter = 15}
            MaghribReminder = New PrayerReminder With {.PrayerName = "Maghrib", .EnabledBefore = False, .EnabledAfter = False, .MinutesBefore = 10, .MinutesAfter = 15}
            IshaReminder = New PrayerReminder With {.PrayerName = "Isha", .EnabledBefore = False, .EnabledAfter = False, .MinutesBefore = 10, .MinutesAfter = 15}
        End Sub

        Public Function GetReminder(prayerName As String) As PrayerReminder
            Select Case prayerName.ToUpper()
                Case "FAJR" : Return FajrReminder
                Case "DHUHR" : Return DhuhrReminder
                Case "ASR" : Return AsrReminder
                Case "MAGHRIB" : Return MaghribReminder
                Case "ISHA" : Return IshaReminder
                Case Else : Return Nothing
            End Select
        End Function
    End Class
End Namespace

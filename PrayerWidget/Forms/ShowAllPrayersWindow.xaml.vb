Imports System.Windows
Imports System.Windows.Input
Imports Salaty.First.Models
Imports Salaty.First.Services

Namespace Salaty.First.Forms
    Public Class ShowAllPrayersWindow
        Inherits Window

        Private _prayerTimes As DailyPrayerTimes
        Private _city As String
        Private _country As String

        Public Sub New(prayerTimes As DailyPrayerTimes, city As String, country As String)
            InitializeComponent()
            _prayerTimes = prayerTimes
            _city = city
            _country = country

            ' Initialize localization
            InitializeLocalization()

            ' Set location and date
            LblLocation.Text = $"{_city}, {_country}"
            LblDate.Text = DateTime.Today.ToString("dddd, MMMM dd, yyyy")

            ' Display prayer times
            DisplayPrayerTimes()
        End Sub

        Private Sub InitializeLocalization()
            ' Window title
            Me.Title = LocalizationService.GetString("app_title", "Salaty First")

            ' Header
            LblTitle.Text = LocalizationService.GetString("show_all_prayers", "Daily Prayer Times")

            ' Prayer names
            LblFajrName.Text = LocalizationService.GetString("prayer_fajr", "Fajr")
            LblSunriseName.Text = LocalizationService.GetString("prayer_sunrise", "Sunrise")
            LblDhuhrName.Text = LocalizationService.GetString("prayer_dhuhr", "Dhuhr")
            LblAsrName.Text = LocalizationService.GetString("prayer_asr", "Asr")
            LblMaghribName.Text = LocalizationService.GetString("prayer_maghrib", "Maghrib")
            LblIshaName.Text = LocalizationService.GetString("prayer_isha", "Isha")

            ' Close button
            BtnClose.Content = LocalizationService.GetString("cancel", "Close")
        End Sub

        Private Sub DisplayPrayerTimes()
            ' Fajr
            LblFajr.Text = _prayerTimes.Fajr.ToString("HH:mm")

            ' Sunrise
            LblSunrise.Text = _prayerTimes.Sunrise.ToString("HH:mm")

            ' Dhuhr
            LblDhuhr.Text = _prayerTimes.Dhuhr.ToString("HH:mm")

            ' Asr
            LblAsr.Text = _prayerTimes.Asr.ToString("HH:mm")

            ' Maghrib
            LblMaghrib.Text = _prayerTimes.Maghrib.ToString("HH:mm")

            ' Isha
            LblIsha.Text = _prayerTimes.Isha.ToString("HH:mm")
        End Sub

        Private Sub Window_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
            If e.ChangedButton = MouseButton.Left Then Me.DragMove()
        End Sub

        Private Sub BtnClose_Click(sender As Object, e As RoutedEventArgs)
            Me.Close()
        End Sub
    End Class
End Namespace

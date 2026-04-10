Imports System.Windows
Imports System.Windows.Controls
Imports System.Collections.Generic
Imports Salaty.First.Services

Public Class LanguageSelectionWindow
    Inherits Window

    Private _selectedLanguage As String = "en"

    Private ReadOnly Languages As New List(Of LanguageInfo) From {
        New LanguageInfo With {.Code = "ar", .Name = "العربية", .Flag = "🇸🇦"},
        New LanguageInfo With {.Code = "en", .Name = "English", .Flag = "🇬🇧"},
        New LanguageInfo With {.Code = "ur", .Name = "اردو", .Flag = "🇵🇰"},
        New LanguageInfo With {.Code = "fr", .Name = "Français", .Flag = "🇫🇷"},
        New LanguageInfo With {.Code = "tr", .Name = "Türkçe", .Flag = "🇹🇷"}
    }

    Public Sub New()
        InitializeComponent()
        InitializeLanguageCombo()
    End Sub

    Private Sub InitializeLanguageCombo()
        LanguageCombo.ItemsSource = Languages
        ' Default to English (index 1)
        LanguageCombo.SelectedIndex = 1
    End Sub

    Private Sub BtnContinue_Click(sender As Object, e As RoutedEventArgs)
        If LanguageCombo.SelectedItem IsNot Nothing Then
            Dim selected = DirectCast(LanguageCombo.SelectedItem, LanguageInfo)
            _selectedLanguage = selected.Code

            ' Save language preference
            LocalizationService.SetLanguage(_selectedLanguage)

            Me.DialogResult = True
            Me.Close()
        End If
    End Sub

    Public ReadOnly Property SelectedLanguage As String
        Get
            Return _selectedLanguage
        End Get
    End Property
End Class

Public Class LanguageInfo
    Public Property Code As String
    Public Property Name As String
    Public Property Flag As String
End Class

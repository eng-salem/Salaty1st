Imports System.Globalization
Imports System.IO

Namespace Services
    Public Class LocalizationService
        Private Shared _currentLanguage As String = "en"
        Private Shared _translations As Dictionary(Of String, String) = New Dictionary(Of String, String)()
        Private Shared ReadOnly _supportedLanguages As Dictionary(Of String, String) = New Dictionary(Of String, String) From {
            {"en", "English"},
            {"ar", "العربية"},
            {"fr", "Français"},
            {"tr", "Türkçe"},
            {"ur", "اردو"}
        }

        Public Shared ReadOnly Property CurrentLanguage As String
            Get
                Return _currentLanguage
            End Get
        End Property

        Public Shared ReadOnly Property SupportedLanguages As Dictionary(Of String, String)
            Get
                Return _supportedLanguages
            End Get
        End Property

        Public Shared Sub Initialize(Optional language As String = "en")
            _currentLanguage = language
            LoadTranslations(language)
        End Sub

        Public Shared Sub SetLanguage(language As String)
            If _supportedLanguages.ContainsKey(language) Then
                _currentLanguage = language
                LoadTranslations(language)
                SaveLanguagePreference(language)
            End If
        End Sub

        Public Shared Function GetString(key As String) As String
            If _translations.ContainsKey(key) Then
                Return _translations(key)
            End If
            Return key
        End Function

        Public Shared Function GetString(key As String, defaultValue As String) As String
            If _translations.ContainsKey(key) Then
                Return _translations(key)
            End If
            Return defaultValue
        End Function

        Private Shared Sub LoadTranslations(language As String)
            _translations.Clear()
            Try
                Dim translationsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Translations", $"{language}.json")
                If File.Exists(translationsPath) Then
                    Dim json = File.ReadAllText(translationsPath)
                    Dim loadedTranslations = ParseJsonStringDictionary(json)
                    If loadedTranslations IsNot Nothing Then
                        _translations = loadedTranslations
                    End If
                End If
            Catch ex As Exception
                Console.WriteLine($"Error loading translations for {language}: {ex.Message}")
            End Try
        End Sub

        ''' <summary>
        ''' Parse a simple JSON string dictionary (for translation files)
        ''' </summary>
        Private Shared Function ParseJsonStringDictionary(json As String) As Dictionary(Of String, String)
            Dim result As New Dictionary(Of String, String)()
            Dim quote As Char = Chr(34)
            Try
                ' Remove outer braces and whitespace
                json = json.Trim()
                If json.StartsWith("{") Then json = json.Substring(1)
                If json.EndsWith("}") Then json = json.Substring(0, json.Length - 1)

                Dim i As Integer = 0
                While i < json.Length
                    ' Skip whitespace and commas
                    While i < json.Length AndAlso (Char.IsWhiteSpace(json(i)) OrElse json(i) = ","c)
                        i += 1
                    End While
                    If i >= json.Length Then Exit While

                    ' Parse key
                    If json(i) = quote Then
                        i += 1
                        Dim keyStart As Integer = i
                        While i < json.Length AndAlso json(i) <> quote
                            i += 1
                        End While
                        Dim key As String = json.Substring(keyStart, i - keyStart)
                        i += 1 ' Skip closing quote

                        ' Skip colon and whitespace
                        While i < json.Length AndAlso (Char.IsWhiteSpace(json(i)) OrElse json(i) = ":"c)
                            i += 1
                        End While

                        ' Parse value
                        If i < json.Length AndAlso json(i) = quote Then
                            i += 1
                            Dim valueStart As Integer = i
                            Dim valueEnd As Integer = i
                            While valueEnd < json.Length
                                If json(valueEnd) = quote AndAlso json(valueEnd - 1) <> "\"c Then
                                    Exit While
                                End If
                                valueEnd += 1
                            End While
                            Dim value As String = json.Substring(valueStart, valueEnd - valueStart)
                            ' Unescape common sequences
                            value = value.Replace("\" & quote, quote).Replace("\n", vbCrLf).Replace("\r", "").Replace("\t", "    ").Replace("\\", "\")
                            result(key) = value
                            i = valueEnd + 1
                        End If
                    Else
                        i += 1
                    End If
                End While
            Catch ex As Exception
                Console.WriteLine("ParseJsonStringDictionary error: " & ex.Message)
            End Try
            Return result
        End Function

        Private Shared Sub SaveLanguagePreference(language As String)
            Try
                Dim settingsManager As New SettingsManager()
                settingsManager.SaveSetting("Language", language)
                Console.WriteLine($"Language saved: {language}")
            Catch ex As Exception
                Console.WriteLine($"Error saving language preference: {ex.Message}")
            End Try
        End Sub

        Public Shared Function GetSavedLanguage() As String
            Try
                Dim settingsManager As New SettingsManager()
                Dim savedLang = settingsManager.GetSetting("Language", "en")
                Console.WriteLine($"Loaded saved language: {savedLang}")
                Return savedLang
            Catch ex As Exception
                Console.WriteLine($"Error loading language: {ex.Message}, defaulting to en")
                Return "en"
            End Try
        End Function
    End Class
End Namespace

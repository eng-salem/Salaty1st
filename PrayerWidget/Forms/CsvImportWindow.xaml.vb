Imports System.Windows
Imports System.Windows.Input
Imports System.Windows.Threading
Imports Salaty.First.Services

Public Class CsvImportWindow
    Inherits Window

    Private _importService As CsvTursoImportService
    Private _isImporting As Boolean = False

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub Window_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        If e.ChangedButton = MouseButton.Left Then
            Me.DragMove()
        End If
    End Sub

    Private Async Sub BtnImport_Click(sender As Object, e As RoutedEventArgs)
        If _isImporting Then
            Return
        End If

        ' Validate inputs
        Dim tursoUrl = TxtDatabaseUrl.Text.Trim()
        Dim authToken = TxtAuthToken.Text.Trim()
        Dim tableName = TxtTableName.Text.Trim()

        If String.IsNullOrEmpty(tursoUrl) OrElse String.IsNullOrEmpty(authToken) OrElse String.IsNullOrEmpty(tableName) Then
            MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If

        ' Build column mappings
        Dim columnMappings As New Dictionary(Of Integer, String)()

        ' Add column mapping from UI (index 0)
        If Not String.IsNullOrEmpty(TxtCol0.Text) AndAlso Not String.IsNullOrEmpty(TxtDbCol0.Text) Then
            Dim colIndex As Integer = 0
            If Integer.TryParse(TxtCol0.Text, colIndex) Then
                columnMappings(colIndex) = TxtDbCol0.Text.Trim()
            End If
        End If

        If columnMappings.Count = 0 Then
            MessageBox.Show("Please specify at least one column mapping.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If

        ' Start import
        _isImporting = True
        BtnImport.IsEnabled = False
        BtnImport.Content = "⏳ Importing..."

        Try
            ' Create import service
            Using _importService = New CsvTursoImportService(tursoUrl, authToken)
                ' Create table if requested
                If ChkCreateTable.IsChecked = True Then
                    Dim tableSchema As New Dictionary(Of String, String) From {
                        {"id", "INTEGER PRIMARY KEY AUTOINCREMENT"},
                        {TxtDbCol0.Text.Trim(), "TEXT"}
                    }

                    Dim tableCreated = Await _importService.CreateTableIfNotExistsAsync(tableName, tableSchema)
                    If Not tableCreated Then
                        MessageBox.Show("Failed to create table. Continuing with import...", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                    End If
                End If

                ' Import CSV
                Dim rowsImported = Await _importService.ImportCsvToTursoAsync(tableName, columnMappings, ChkSkipHeaders.IsChecked = True)

                If rowsImported > 0 Then
                    MessageBox.Show($"Successfully imported {rowsImported} rows!", "Import Complete",
                                    MessageBoxButton.OK, MessageBoxImage.Information)
                    Me.DialogResult = True
                ElseIf rowsImported = -1 Then
                    MessageBox.Show("Import was cancelled or failed.", "Import Cancelled",
                                    MessageBoxButton.OK, MessageBoxImage.Warning)
                Else
                    MessageBox.Show("No rows were imported. Check the CSV file format.", "Import Complete",
                                    MessageBoxButton.OK, MessageBoxImage.Information)
                End If
            End Using

        Catch ex As Exception
            MessageBox.Show($"Error: {ex.Message}", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error)
        Finally
            _isImporting = False
            BtnImport.IsEnabled = True
            BtnImport.Content = "📥 Import CSV"
        End Try
    End Sub

    Private Sub BtnCancel_Click(sender As Object, e As RoutedEventArgs)
        If _isImporting Then
            Return
        End If
        Me.DialogResult = False
        Me.Close()
    End Sub
End Class

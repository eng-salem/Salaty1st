# CSV to Turso Import Service - Usage Examples

## Overview
The `CsvTursoImportService` allows you to select a CSV file and import its data into your Turso.io database using the HTTP API.

## Quick Start - Import Quotes

```vb
Imports Salaty.First.Services

' Your Turso credentials
Dim tursoUrl As String = "libsql://quotes-manhag.aws-eu-west-1.turso.io"
Dim authToken As String = "YOUR_TURSO_AUTH_TOKEN_HERE" ' Replace with actual token

Using importService As New CsvTursoImportService(tursoUrl, authToken)
    ' Create table first (if needed)
    Dim tableSchema As New Dictionary(Of String, String) From {
        {"id", "INTEGER PRIMARY KEY AUTOINCREMENT"},
        {"post_content", "TEXT NOT NULL"}
    }
    Await importService.CreateTableIfNotExistsAsync("quotes", tableSchema)

    ' Map CSV column: Index 0 -> "post_content" column
    ' (id is auto-generated)
    Dim columnMappings As New Dictionary(Of Integer, String) From {
        {0, "post_content"}
    }

    ' Open file dialog and import
    Dim rowsImported = Await importService.ImportCsvToTursoAsync("quotes", columnMappings)

    If rowsImported > 0 Then
        MessageBox.Show($"Successfully imported {rowsImported} quotes!")
    End If
End Using
```

## Basic Usage

### 1. Import with File Dialog (User Selects CSV)

```vb
Imports Salaty.First.Services

' Create the service with your Turso credentials
Dim tursoUrl As String = "https://your-database.turso.io"
Dim authToken As String = "your-auth-token-here"

Using importService As New CsvTursoImportService(tursoUrl, authToken)
    ' Define column mappings: CSV column index (0-based) -> Database column name
    Dim columnMappings As New Dictionary(Of Integer, String) From {
        {0, "post_content"}
    }

    ' Open file dialog and import
    Dim rowsImported = Await importService.ImportCsvToTursoAsync("quotes", columnMappings)

    If rowsImported > 0 Then
        MessageBox.Show($"Successfully imported {rowsImported} rows!")
    ElseIf rowsImported = -1 Then
        MessageBox.Show("Import was cancelled or failed.")
    End If
End Using
```

### 2. Import from Specific File Path

```vb
Imports Salaty.First.Services

Using importService As New CsvTursoImportService(tursoUrl, authToken)
    Dim columnMappings As New Dictionary(Of Integer, String) From {
        {0, "post_content"}
    }

    ' Import from a specific file path (no file dialog)
    Dim rowsImported = Await importService.ImportFileAsync(
        "C:\path\to\your\quotes.csv",
        "quotes",
        columnMappings,
        skipHeaders:=True
    )

    Console.WriteLine($"Imported {rowsImported} quotes")
End Using
```

## CSV Format Example

Your CSV file should look like this:

```csv
post_content
"The best of deeds is prayer."
"Charity never decreases wealth."
"Speak good or remain silent."
```

Or without headers (set `skipHeaders:=False`):

```csv
"The best of deeds is prayer."
"Charity never decreases wealth."
"Speak good or remain silent."
```

## Parameters

### ImportCsvToTursoAsync
- `tableName` (String): Target table name in Turso database
- `columnMappings` (Dictionary(Of Integer, String)): Maps CSV column indices to database column names
- `skipHeaders` (Boolean, optional): Skip the first row if it contains headers (default: True)

### ImportFileAsync
- `filePath` (String): Full path to the CSV file
- `tableName` (String): Target table name
- `columnMappings` (Dictionary): Column mappings
- `skipHeaders` (Boolean, optional): Skip header row

### CreateTableIfNotExistsAsync
- `tableName` (String): Table name to create
- `columns` (Dictionary(Of String, String)): Column definitions (name -> SQL type)

## Return Values

- **Positive integer**: Number of rows successfully imported
- **-1**: User cancelled file selection or error occurred
- **0**: No rows imported (empty file or all rows failed)

## Notes

- CSV column indices are **0-based** (first column = 0)
- The service handles quoted CSV fields with commas inside
- Single quotes in data are automatically escaped for SQL
- NULL values are supported (empty CSV fields become NULL)
- HTTP timeout is set to 30 seconds per request
- Each row is inserted individually
- `id` column is auto-incremented (no need to import it)

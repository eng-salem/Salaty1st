''' <summary>
''' Shared Turso database constants used across all services
''' </summary>
Public NotInheritable Class TursoConstants
    Private Sub New()
    End Sub

    ' Counter database - used for device_counters and quotes
    Public Const CounterTursoUrl As String = "https://counter-manhag.aws-eu-west-1.turso.io"
    ' IMPORTANT: Replace with actual token or load from environment/config
    Public Const CounterAuthToken As String = "YOUR_TURSO_AUTH_TOKEN_HERE"
End Class

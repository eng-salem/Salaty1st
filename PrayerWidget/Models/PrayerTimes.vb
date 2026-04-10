
Namespace Models
    Public Class PrayerTimeInfo
        Public Property Name As String
        Public Property Time As DateTime
        Public Property IsNext As Boolean
        Public Property IsPrevious As Boolean
    End Class

    Public Class DailyPrayerTimes
        Public Property Fajr As DateTime
        Public Property Sunrise As DateTime
        Public Property Dhuhr As DateTime
        Public Property Asr As DateTime
        Public Property Maghrib As DateTime
        Public Property Isha As DateTime
        Public Property NextPrayer As String
        Public Property NextPrayerTime As DateTime
        Public Property PreviousPrayer As String
        Public Property PreviousPrayerTime As DateTime
    End Class
End Namespace

Public Class PrayTime

    '------------------------ Constants --------------------------

    ' Calculation Methods

    Public Shared Jafari As Integer = 0

    ' Ithna Ashari

    Public Shared Karachi As Integer = 3 '1

    ' University of Islamic Sciences, Karachi

    Public Shared ISNA As Integer = 5 '2

    ' Islamic Society of North America (ISNA)

    Public Shared MWL As Integer = 1 '3

    ' Muslim World League (MWL)

    Public Shared Makkah As Integer = 4

    ' Umm al-Qura, Makkah

    Public Shared Egypt As Integer = 2 '5

    ' Egyptian General Authority of Survey

    Public Shared [Custom] As Integer = 6

    ' Custom Setting

    Public Shared Tehran As Integer = 7

    ' Institute of Geophysics, University of Tehran

    ' Juristic Methods

    Public Shared Shafii As Integer = 0

    ' Shafii (standard)

    Public Shared Hanafi As Integer = 1

    ' Hanafi

    ' Adjusting Methods for Higher Latitudes

    Public Shared None As Integer = 0

    ' No adjustment

    Public Shared MidNight As Integer = 1

    ' middle of night

    Public Shared OneSeventh As Integer = 2

    ' 1/7th of night

    Public Shared AngleBased As Integer = 3

    ' angle/60th of night

    ' Time Formats

    Public Shared Time24 As Integer = 0

    ' 24-hour format

    Public Shared Time12 As Integer = 1

    ' 12-hour format

    Public Shared Time12NS As Integer = 2

    ' 12-hour format with no suffix

    Public Shared Floating As Integer = 3

    ' floating point number

    ' Time Names

    Public Shared timeNames As [String]() = {"Fajr", "Sunrise", "Dhuhr", "Asr", "Sunset", "Maghrib", "Isha"}

    Shared ReadOnly InvalidTime As [String] = "----"

    ' The string used for inv





    '---------------------- Global Variables --------------------



    Private calcMethod As Integer = 3

    ' caculation method

    Private asrJuristic As Integer

    ' Juristic method for Asr

    Private dhuhrMinutes As Integer = 0

    ' minutes after mid-day for Dhuhr

    Private adjustHighLats As Integer = 1

    ' adjusting method for higher latitudes

    Private timeFormat As Integer = 0

    ' time format

    Private lat As Double

    ' latitude

    Private lng As Double

    ' longitude

    Private TimeZone As Integer

    ' time-zone

    Private JDate As Double

    ' Julian date

    Private ReadOnly Times As Integer()



    '--------------------- Technical Settings --------------------



    Private ReadOnly NumIterations As Integer = 1

    ' number of iterations needed to Compute times



    '------------------- Calc Method Parameters --------------------

    Private ReadOnly MethodParams As Double()()

    Public Sub New()

        Times = New Integer(6) {}

        MethodParams = New Double(7)() {}

        Me.MethodParams(Jafari) = New Double() {16, 0, 4, 0, 14}

        Me.MethodParams(Karachi) = New Double() {18, 1, 0, 0, 18}

        Me.MethodParams(ISNA) = New Double() {15, 1, 0, 0, 15}

        Me.MethodParams(MWL) = New Double() {18, 1, 0, 0, 17}

        Me.MethodParams(Makkah) = New Double() {18.5, 1, 0, 1, 90}

        Me.MethodParams(Egypt) = New Double() {19.5, 1, 0, 0, 17.5}

        Me.MethodParams(Tehran) = New Double() {17.7, 0, 4.5, 0, 14}

        Me.MethodParams([Custom]) = New Double() {18, 1, 0, 0, 17}

    End Sub











    ' return prayer times for a given date

    Public Function GetPrayerTimes(ByVal year As Integer, ByVal month As Integer, ByVal day As Integer, ByVal latitude As Double, ByVal longitude As Double, ByVal timeZone As Integer) As [String]()

        Return Me.GetDatePrayerTimes(year, month, day, latitude, longitude, timeZone)

    End Function

    ' Set the calculation method

    Public Sub SetCalcMethod(ByVal methodID As Integer)

        Me.calcMethod = methodID

    End Sub

    ' Set the juristic method for Asr

    Public Sub SetAsrMethod(ByVal methodID As Integer)

        If methodID < 0 OrElse methodID > 1 Then

            Return

        End If

        Me.asrJuristic = methodID

    End Sub

    ' Set the angle for calculating Fajr

    Public Sub SetFajrAngle(ByVal angle As Double)

        Me.SetCustomParams(New Integer() {CInt(Math.Truncate(angle)), -1, -1, -1, -1})

    End Sub

    ' Set the angle for calculating Maghrib

    Public Sub SetMaghribAngle(ByVal angle As Double)

        Me.SetCustomParams(New Integer() {-1, 0, CInt(Math.Truncate(angle)), -1, -1})

    End Sub

    ' Set the angle for calculating Isha

    Public Sub SetIshaAngle(ByVal angle As Double)

        Me.SetCustomParams(New Integer() {-1, -1, -1, 0, CInt(Math.Truncate(angle))})

    End Sub

    ' Set the minutes after mid-day for calculating Dhuhr

    Public Sub SetDhuhrMinutes(ByVal minutes As Integer)

        Me.dhuhrMinutes = minutes

    End Sub

    ' Set the minutes after Sunset for calculating Maghrib

    Public Sub SetMaghribMinutes(ByVal minutes As Integer)

        Me.SetCustomParams(New Integer() {-1, 1, minutes, -1, -1})

    End Sub

    ' Set the minutes after Maghrib for calculating Isha

    Public Sub SetIshaMinutes(ByVal minutes As Integer)

        Me.SetCustomParams(New Integer() {-1, -1, -1, 1, minutes})

    End Sub

    ' Set custom values for calculation parameters

    Public Sub SetCustomParams(ByVal param As Integer())

        For i As Integer = 0 To 4

            If param(i) = -1 Then

                Me.MethodParams([Custom])(i) = Me.MethodParams(Me.calcMethod)(i)

            Else

                Me.MethodParams([Custom])(i) = param(i)

            End If

        Next

        Me.calcMethod = [Custom]

    End Sub

    ' Set adjusting method for higher latitudes

    Public Sub SetHighLatsMethod(ByVal methodID As Integer)

        Me.adjustHighLats = methodID

    End Sub

    ' Set the time format

    Public Sub SetTimeFormat(ByVal timeFormat As Integer)

        Me.timeFormat = timeFormat

    End Sub

    ' convert float hours to 24h format

    Public Function FloatToTime24(ByVal time As Double) As [String]
        Try
            If time < 0 Then

                Return InvalidTime

            End If

            time = Me.FixHour(time + 0.5 / 60)

            ' add 0.5 minutes to round

            Dim hours As Double = Math.Floor(time)

            Dim minutes As Double = Math.Floor((time - hours) * 60)

            Return Me.TwoDigitsFormat(CInt(Math.Truncate(hours))) & ":" & Me.TwoDigitsFormat(CInt(Math.Truncate(minutes)))

        Catch ex As Exception
            ' Error handling removed - return default time
        End Try
        Return Nothing
    End Function

    ' convert float hours to 12h format

    Public Function FloatToTime12(ByVal time As Double, ByVal noSuffix As Boolean) As [String]

        If time < 0 Then

            Return InvalidTime

        End If

        time = Me.FixHour(time + 0.5 / 60)

        ' add 0.5 minutes to round

        Dim hours As Double = Math.Floor(time)

        Dim minutes As Double = Math.Floor((time - hours) * 60)

        Dim suffix As [String] = If(hours >= 12, " pm", " am")

        hours = (hours + 12 - 1) Mod 12 + 1

        Return CInt(Math.Truncate(hours)) & ":" & Me.TwoDigitsFormat(CInt(Math.Truncate(minutes))) & (If(noSuffix, "", suffix))

    End Function

    ' convert float hours to 12h format with no suffix

    Public Function FloatToTime12NS(ByVal time As Double) As [String]

        Return Me.FloatToTime12(time, True)

    End Function

    '---------------------- Compute Prayer Times -----------------------



    ' return prayer times for a given date

    Public Function GetDatePrayerTimes(ByVal year As Integer, ByVal month As Integer, ByVal day As Integer, ByVal latitude As Integer, ByVal longitude As Integer, ByVal timeZone As Double) As [String]()

        Me.lat = latitude

        Me.lng = longitude

        Me.TimeZone = timeZone

        Me.JDate = Me.JulianDate(year, month, day) - longitude / (15 * 24)

        Return Me.ComputeDayTimes()

    End Function

    ' Compute declination angle of sun and equation of time

    Public Function SunPosition(ByVal jd As Double) As Double()

        Dim D__1 As Double = jd - 2451545.0

        Dim g As Double = Me.FixAngle(357.529 + 0.98560028 * D__1)

        Dim q As Double = Me.FixAngle(280.459 + 0.98564736 * D__1)

        Dim L As Double = Me.FixAngle(q + 1.915 * Me.Dsin(g) + 0.02 * Me.Dsin(2 * g))

        Dim R As Double = 1.00014 - 0.01671 * Me.Dcos(g) - 0.00014 * Me.Dcos(2 * g)

        Dim e As Double = 23.439 - 0.00000036 * D__1

        Dim d__2 As Double = Me.Darcsin(Me.Dsin(e) * Me.Dsin(L))

        Dim RA As Double = Me.Darctan2(Me.Dcos(e) * Me.Dsin(L), Me.Dcos(L)) / 15

        RA = Me.FixHour(RA)

        Dim EqT As Double = q / 15 - RA

        Return New Double() {d__2, EqT}

    End Function

    ' Compute equation of time

    Public Function EquationOfTime(ByVal jd As Double) As Double

        Return Me.SunPosition(jd)(1)

    End Function

    ' Compute declination angle of sun

    Public Function SunDeclination(ByVal jd As Double) As Double

        Return Me.SunPosition(jd)(0)

    End Function

    ' Compute mid-day (Dhuhr, Zawal) time

    Public Function ComputeMidDay(ByVal t__1 As Double) As Double

        Dim T__2 As Double = Me.EquationOfTime(Me.JDate + t__1)

        Dim Z As Double = Me.FixHour(12 - T__2)

        Return Z

    End Function

    ' Compute time for a given angle G

    Public Function ComputeTime(ByVal G As Double, ByVal t As Double) As Double

        'System.out.println("G: "+G);

        Dim D As Double = Me.SunDeclination(Me.JDate + t)

        Dim Z As Double = Me.ComputeMidDay(t)

        Dim V As Double = (CDbl(1) / 15) * Me.Darccos((-Me.Dsin(G) - Me.Dsin(D) * Me.Dsin(Me.lat)) / (Me.Dcos(D) * Me.Dcos(Me.lat)))

        Return Z + (If(G > 90, -V, V))

    End Function

    ' Compute the time of Asr

    Public Function ComputeAsr(ByVal [step] As Integer, ByVal t As Double) As Double

        ' Shafii: step=1, Hanafi: step=2

        Dim D As Double = Me.SunDeclination(Me.JDate + t)

        Dim G As Double = -Me.Darccot([step] + Me.Dtan(Math.Abs(Me.lat - D)))

        Return Me.ComputeTime(G, t)

    End Function

    '---------------------- Compute Prayer Times -----------------------

    ' Compute prayer times at given julian date

    Public Function ComputeTimes(ByVal times As Double()) As Double()

        Dim t As Double() = Me.DayPortion(times)



        Dim Fajr As Double = Me.ComputeTime(180 - Me.MethodParams(Me.calcMethod)(0), t(0))

        Dim Sunrise As Double = Me.ComputeTime(180 - 0.833, t(1))

        Dim Dhuhr As Double = Me.ComputeMidDay(t(2))

        Dim Asr As Double = Me.ComputeAsr(1 + Me.asrJuristic, t(3))

        Dim Sunset As Double = Me.ComputeTime(0.833, t(4))



        Dim Maghrib As Double = Me.ComputeTime(Me.MethodParams(Me.calcMethod)(2), t(5))

        Dim Isha As Double = Me.ComputeTime(Me.MethodParams(Me.calcMethod)(4), t(6))

        Return New Double() {Fajr, Sunrise, Dhuhr, Asr, Sunset, Maghrib, Isha}

    End Function

    ' adjust Fajr, Isha and Maghrib for locations in higher latitudes

    Public Function AdjustHighLatTimes(ByVal times As Double()) As Double()

        Dim nightTime As Double = Me.GetTimeDifference(times(4), times(1))

        ' sunset to sunrise

        ' Adjust Fajr

        Dim FajrDiff As Double = Me.NightPortion(Me.MethodParams(Me.calcMethod)(0)) * nightTime

        If Me.GetTimeDifference(times(0), times(1)) > FajrDiff Then

            times(0) = times(1) - FajrDiff

        End If

        ' Adjust Isha

        Dim IshaAngle As Double = If((Me.MethodParams(Me.calcMethod)(3) = 0), Me.MethodParams(Me.calcMethod)(4), 18)

        Dim IshaDiff As Double = Me.NightPortion(IshaAngle) * nightTime

        If Me.GetTimeDifference(times(4), times(6)) > IshaDiff Then

            times(6) = times(4) + IshaDiff

        End If

        ' Adjust Maghrib

        Dim MaghribAngle As Double = If((MethodParams(Me.calcMethod)(1) = 0), Me.MethodParams(Me.calcMethod)(2), 4)

        Dim MaghribDiff As Double = Me.NightPortion(MaghribAngle) * nightTime

        If Me.GetTimeDifference(times(4), times(5)) > MaghribDiff Then

            times(5) = times(4) + MaghribDiff

        End If

        Return times

    End Function

    ' the night portion used for adjusting times in higher latitudes

    Public Function NightPortion(ByVal angle As Double) As Double

        Dim val As Double = 0

        If Me.adjustHighLats = AngleBased Then

            val = 1.0 / 60.0 * angle

        End If

        If Me.adjustHighLats = MidNight Then

            val = 1.0 / 2.0

        End If

        If Me.adjustHighLats = OneSeventh Then

            val = 1.0 / 7.0

        End If

        Return val

    End Function

    Public Function DayPortion(ByVal times As Double()) As Double()

        For i As Integer = 0 To times.Length - 1

            times(i) /= 24

        Next

        Return times

    End Function

    ' Compute prayer times at given julian date

    Public Function ComputeDayTimes() As [String]()

        Dim times As Double() = {5, 6, 12, 13, 18, 18, 18}

        'default times

        For i As Integer = 0 To Me.NumIterations - 1

            times = Me.ComputeTimes(times)

        Next

        times = Me.AdjustTimes(times)

        Return Me.AdjustTimesFormat(times)

    End Function



    ' adjust times in a prayer time array

    Public Function AdjustTimes(ByVal times As Double()) As Double()

        For i As Integer = 0 To 6

            times(i) += Me.TimeZone - Me.lng / 15

        Next

        times(2) += Me.dhuhrMinutes \ 60

        'Dhuhr

        If Me.MethodParams(Me.calcMethod)(1) = 1 Then

            ' Maghrib

            times(5) = times(4) + Me.MethodParams(Me.calcMethod)(2) / 60.0

        End If

        If Me.MethodParams(Me.calcMethod)(3) = 1 Then

            ' Isha

            times(6) = times(5) + Me.MethodParams(Me.calcMethod)(4) / 60.0

        End If

        If Me.adjustHighLats <> None Then

            times = Me.AdjustHighLatTimes(times)

        End If

        Return times

    End Function

    Public Function AdjustTimesFormat(ByVal times As Double()) As [String]()
        Try

            Dim formatted As [String]() = New [String](times.Length - 1) {}

            If Me.timeFormat = Floating Then

                For i As Integer = 0 To times.Length - 1

                    formatted(i) = times(i) & ""

                Next

                Return formatted

            End If

            For i As Integer = 0 To 6

                If Me.timeFormat = Time12 Then

                    formatted(i) = Me.FloatToTime12(times(i), True)

                ElseIf Me.timeFormat = Time12NS Then

                    formatted(i) = Me.FloatToTime12NS(times(i))

                Else

                    formatted(i) = Me.FloatToTime24(times(i))

                End If

            Next

            Return formatted

        Catch ex As Exception
            MsgBox("adjustTimesFormat" & ex.Message)
        End Try
        Return Nothing
    End Function

    '---------------------- Misc Functions -----------------------

    ' Compute the difference between two times

    Public Function GetTimeDifference(ByVal c1 As Double, ByVal c2 As Double) As Double

        Dim diff As Double = Me.FixHour(c2 - c1)



        Return diff

    End Function

    ' add a leading 0 if necessary

    Public Function TwoDigitsFormat(ByVal num As Integer) As [String]

        Return If((num < 10), "0" & num, num & "")

    End Function

    '---------------------- Julian Date Functions -----------------------

    ' calculate julian date from a calendar date

    Public Function JulianDate(ByVal year As Integer, ByVal month As Integer, ByVal day As Integer) As Double

        If month <= 2 Then

            year -= 1

            month += 12

        End If

        Dim A As Double = CDbl(Math.Floor(year / 100.0))

        Dim B As Double = 2 - A + Math.Floor(A / 4)

        Dim JD As Double = Math.Floor(365.25 * (year + 4716)) + Math.Floor(30.6001 * (month + 1)) + day + B - 1524.5

        Return JD

    End Function



    '---------------------- Time-Zone Functions -----------------------



    ' detect daylight saving in a given date

    Public Function UseDayLightSaving(ByVal year As Integer, ByVal month As Integer, ByVal day As Integer) As Boolean

        Return System.TimeZone.CurrentTimeZone.IsDaylightSavingTime(New DateTime(year, month, day))

    End Function

    ' ---------------------- Trigonometric Functions -----------------------

    ' degree sin

    Public Function Dsin(ByVal d As Double) As Double

        Return Math.Sin(Me.DegreeToRadian(d))

    End Function

    ' degree cos

    Public Function Dcos(ByVal d As Double) As Double

        Return Math.Cos(Me.DegreeToRadian(d))

    End Function

    ' degree tan

    Public Function Dtan(ByVal d As Double) As Double

        Return Math.Tan(Me.DegreeToRadian(d))

    End Function

    ' degree arcsin

    Public Function Darcsin(ByVal x As Double) As Double

        Return Me.RadianToDegree(Math.Asin(x))

    End Function

    ' degree arccos

    Public Function Darccos(ByVal x As Double) As Double

        Return Me.RadianToDegree(Math.Acos(x))

    End Function

    ' degree arctan

    Public Function Darctan(ByVal x As Double) As Double

        Return Me.RadianToDegree(Math.Atan(x))

    End Function

    ' degree arctan2

    Public Function Darctan2(ByVal y As Double, ByVal x As Double) As Double

        Return Me.RadianToDegree(Math.Atan2(y, x))

    End Function

    ' degree arccot

    Public Function Darccot(ByVal x As Double) As Double

        Return Me.RadianToDegree(Math.Atan(1 / x))

    End Function



    ' Radian to Degree

    Public Function RadianToDegree(ByVal radian As Double) As Double

        Return (radian * 180.0) / Math.PI

    End Function

    ' degree to radian

    Public Function DegreeToRadian(ByVal degree As Double) As Double

        Return (degree * Math.PI) / 180.0

    End Function

    Public Function FixAngle(ByVal angel As Double) As Double

        angel = angel - 360.0 * (Math.Floor(angel / 360.0))

        angel = If(angel < 0, angel + 360.0, angel)

        Return angel

    End Function

    ' range reduce hours to 0..23

    Public Function FixHour(ByVal hour As Double) As Double

        hour = hour - 24.0 * (Math.Floor(hour / 24.0))

        hour = If(hour < 0, hour + 24.0, hour)

        Return hour

    End Function
End Class
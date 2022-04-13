Imports NUnit.Framework
Imports System.Reflection

<Assembly: log4net.Config.XmlConfigurator(ConfigFile:="log4net.config")>

<TestFixture> Public Class TestBase

    'Protected Shared ReadOnly Logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
    Protected Shared ReadOnly Logger As log4net.ILog = log4net.LogManager.GetLogger(GetType(CenterDevice.Rest.ResponseHandler.BaseResponseHandler))

    Protected ReadOnly Property IOClient As CompuMaster.Scopevisio.Teamwork.TeamworkIOClient
        Get
            Static Result As CompuMaster.Scopevisio.Teamwork.TeamworkIOClient
            If Result Is Nothing Then
                Result = CreateTeamworkIOClient()
            End If
            Return Result
        End Get
    End Property

    Private Shared Function CreateTeamworkIOClient() As CompuMaster.Scopevisio.Teamwork.TeamworkIOClient
        Dim username As String = Settings.InputLine("username")
        Dim customerno As String = Settings.InputLine("customer no.")
        Dim password As String = Settings.InputLine("password")

        Assert.NotNull(username, "User credentials not found in environment or buffer files (run Sample app for creating buffer files in temp directory!)")
        Assert.NotNull(customerno, "User credentials not found in environment or buffer files (run Sample app for creating buffer files in temp directory!)")
        Assert.NotNull(password, "User credentials not found in environment or buffer files (run Sample app for creating buffer files in temp directory!)")

        Return New CompuMaster.Scopevisio.Teamwork.TeamworkIOClient(customerno, username, password)
    End Function


    ''' <summary>
    ''' Indent a string
    ''' </summary>
    ''' <param name="value"></param>
    ''' <returns></returns>
    Protected Shared Function Indent(value As String) As String
        If (String.IsNullOrWhiteSpace(value)) Then
            Return String.Empty
        Else
            Dim Result As String = "    " + value.Replace(ControlChars.Lf, ControlChars.Lf & "    ")
            If (Result.EndsWith(ControlChars.Lf & "    ")) Then
                Result = Result.Substring(0, Result.Length - ((ControlChars.Lf & "    ").Length - 1))
            End If
            Return Result
        End If
    End Function

    Protected Function TestAssembly() As System.Reflection.Assembly
        Return System.Reflection.Assembly.GetExecutingAssembly
    End Function

End Class

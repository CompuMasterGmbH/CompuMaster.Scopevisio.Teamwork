Imports NUnit.Framework

<TestFixture> Public Class TeamworkBasicTest

    Private ReadOnly Property IOClient As CompuMaster.Scopevisio.Teamwork.TeamworkIOClient
        Get
            Static Result As CompuMaster.Scopevisio.Teamwork.TeamworkIOClient
            If Result Is Nothing Then
                Result = CreateTeamworkIOClient()
            End If
            Return Result
        End Get
    End Property

    <Test> Public Sub GetTypes()
#Disable Warning BC42024 ' Nicht verwendete lokale Variable
        Dim IOClient As CompuMaster.Scopevisio.Teamwork.TeamworkIOClient
        Dim RestClient As CompuMaster.Scopevisio.CenterDeviceApi.TeamworkRestClient
        Dim OpenScopeClient As CompuMaster.Scopevisio.OpenApi.OpenScopeApiClient
#Enable Warning BC42024 ' Nicht verwendete lokale Variable
        Assert.Pass()
    End Sub

    Private Shared Function CreateTeamworkIOClient() As CompuMaster.Scopevisio.Teamwork.TeamworkIOClient
        Dim username As String = Settings.InputFromBufferFile("username")
        Dim customerno As String = Settings.InputFromBufferFile("customer no.")
        Dim password As String = Settings.InputFromBufferFile("password")

        Assert.NotNull(username, "User credentials not found in environment or buffer files (run Sample app for creating buffer files in temp directory!)")
        Assert.NotNull(customerno, "User credentials not found in environment or buffer files (run Sample app for creating buffer files in temp directory!)")
        Assert.NotNull(password, "User credentials not found in environment or buffer files (run Sample app for creating buffer files in temp directory!)")

        Return New CompuMaster.Scopevisio.Teamwork.TeamworkIOClient(customerno, username, password)
    End Function

    <Test> Public Sub ServerAccessAndAuthorization()
        Assert.NotNull(CreateTeamworkIOClient.CurrentAuthenticationContextUserID)
    End Sub

    <Test> Public Sub CurrentContextUserId()
        System.Console.WriteLine(IOClient.CurrentContextUserId)
        Assert.NotNull(IOClient.CurrentContextUserId)
    End Sub

    <Test> Public Sub CurrentAuthenticationContextUserID()
        System.Console.WriteLine(IOClient.CurrentAuthenticationContextUserID)
        Assert.NotNull(IOClient.CurrentAuthenticationContextUserID)
    End Sub

    <Test> Public Sub RootDirAccess()
        System.Console.WriteLine(System.Environment.NewLine & "## Initial directory listing - flat")
        System.Console.WriteLine(IOClient.RootDirectory.ToStringListing(False, True))

        System.Console.WriteLine(System.Environment.NewLine & "## Initial directory listing - recursive - without display of files")
        System.Console.WriteLine(IOClient.RootDirectory.ToStringListing(True, False))

        Assert.Pass("Success: no exception occured")
    End Sub

    <Test> Public Sub RootDirectory()
        System.Console.WriteLine(IOClient.RootDirectory.ToStringListing(False, True))
        IOClient.RootDirectory.GetDirectories(0, False)
        Assert.NotZero(IOClient.RootDirectory.GetDirectories(0, False).Length)
    End Sub

End Class

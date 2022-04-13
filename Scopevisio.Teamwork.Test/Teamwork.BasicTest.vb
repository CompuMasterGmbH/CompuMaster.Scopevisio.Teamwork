Imports NUnit.Framework

<TestFixture> Public Class TeamworkBasicTest
    Inherits TestBase

    <Test> Public Sub GetTypes()
#Disable Warning BC42024 ' Nicht verwendete lokale Variable
        Dim IOClient As CompuMaster.Scopevisio.Teamwork.TeamworkIOClient
        Dim RestClient As CompuMaster.Scopevisio.CenterDeviceApi.TeamworkRestClient
        Dim OpenScopeClient As CompuMaster.Scopevisio.OpenApi.OpenScopeApiClient
#Enable Warning BC42024 ' Nicht verwendete lokale Variable
        Assert.Pass()
    End Sub

    <Test> Public Sub ServerAccessAndAuthorization()
        Assert.NotNull(IOClient.CurrentAuthenticationContextUserID)
    End Sub

    <Test> Public Sub CurrentContextUserId()
        System.Console.WriteLine(IOClient.CurrentContextUserId)
        Assert.NotNull(IOClient.CurrentContextUserId)
    End Sub

    <Test> Public Sub CurrentAuthenticationContextUserID()
        System.Console.WriteLine(IOClient.CurrentAuthenticationContextUserID)
        Assert.NotNull(IOClient.CurrentAuthenticationContextUserID)
    End Sub

    ''' <summary>
    ''' OpenScope interface version check for OpenScope acting as Teamwork proxy to CenterDevice API
    ''' </summary>
    <Test> Public Sub OpenScopeInterfaceVersion()
        'API method call for getting API version info (authorized by access token from initial request)

        Dim VersionResult As CompuMaster.Scopevisio.OpenApi.Model.Version = OpenScopeClient.AdditionalApi.GetVersionWithHttpInfo().Data
        System.Console.WriteLine("Interface Version=" + VersionResult.ToString())

        System.Console.WriteLine("Interface BuildDate=" + VersionResult.BuildDate.ToString())
        Assert.GreaterOrEqual(Long.Parse(VersionResult.BuildDate.ToString("yyyyMMdd")), 20220000)

        System.Console.WriteLine("Interface BuildNumber=" + VersionResult.BuildNumber.ToString())
        Assert.GreaterOrEqual(VersionResult.BuildNumber, 0)

        System.Console.WriteLine("Interface CommitDate=" + VersionResult.CommitDate.ToString())
        Assert.GreaterOrEqual(Long.Parse(VersionResult.CommitDate.ToString("yyyyMMdd")), 20220000)

        System.Console.WriteLine("Interface CommitHash=" + VersionResult.CommitHash.ToString())
        Assert.IsNotEmpty(VersionResult.CommitHash)
    End Sub

    Private ReadOnly Property OpenScopeClient As CompuMaster.Scopevisio.OpenApi.OpenScopeApiClient
        Get
            Static _OpenScopeClient As CompuMaster.Scopevisio.OpenApi.OpenScopeApiClient
            If _OpenScopeClient Is Nothing Then
                Dim NewOpenScopeClient As New CompuMaster.Scopevisio.OpenApi.OpenScopeApiClient
                Dim username As String = Settings.InputLine("username")
                Dim customerno As String = Settings.InputLine("customer no.")
                Dim password As String = Settings.InputLine("password")
                Assert.NotNull(username, "User credentials not found in environment or buffer files (run Sample app for creating buffer files in temp directory!)")
                Assert.NotNull(customerno, "User credentials not found in environment or buffer files (run Sample app for creating buffer files in temp directory!)")
                Assert.NotNull(password, "User credentials not found in environment or buffer files (run Sample app for creating buffer files in temp directory!)")
                NewOpenScopeClient.AuthorizeWithUserCredentials(username, customerno, password)
                _OpenScopeClient = NewOpenScopeClient
            End If
            Return _OpenScopeClient
        End Get
    End Property

    ''' <summary>
    ''' Information on Teamwork's OpenScope context
    ''' </summary>
    <Test> Public Sub OpenScopeContext()
        Dim OrganisationResult As CompuMaster.Scopevisio.OpenApi.Model.Records(Of CompuMaster.Scopevisio.OpenApi.Model.Organisation) = OpenScopeClient.AdditionalApi.OrganisationJsonWithHttpInfo().Data
        System.Console.WriteLine("Organisationen={")

        Dim sb As New System.Text.StringBuilder()
        For Each Org As CompuMaster.Scopevisio.OpenApi.Model.Organisation In OrganisationResult.Items
            If OrganisationResult.Items.IndexOf(Org) > 0 Then
                System.Console.WriteLine()
            End If
            sb.AppendLine("Org[" & (OrganisationResult.Items.IndexOf(Org) + 1) & "/" & OrganisationResult.Items.Count & "]")
            sb.AppendLine("  Org.ID=" & Org.Id)
            sb.AppendLine("  Org.Name=" & Org.Name)
            sb.AppendLine("  Org.TeamworkTenant.ID=" & Org.TeamworkTenantId)
            sb.AppendLine("  Org.TeamworkTenant.Name=" & Org.TeamworkTenantName)
        Next
        System.Console.Write(Indent(sb.ToString()))

        System.Console.WriteLine("}")
        System.Console.WriteLine()

        'Show current context
        Dim ScopevisioAppContext As CompuMaster.Scopevisio.OpenApi.Model.AccountInfo = OpenScopeClient.AdditionalApi.GetApplicationContextWithHttpInfo().Data
        System.Console.WriteLine(ScopevisioAppContext.ToString())
        System.Console.WriteLine()
    End Sub

End Class

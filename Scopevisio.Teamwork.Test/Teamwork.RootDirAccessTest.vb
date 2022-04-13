Imports NUnit.Framework

<TestFixture> Public Class Teamwork.RootDirAccessTest
    Inherits TestBase

    <Test> Public Sub RootDirAccess()
        System.Console.WriteLine(System.Environment.NewLine & "## Initial directory listing - flat")
        System.Console.WriteLine(IOClient.RootDirectory.ToStringListing(False, True))

        System.Console.WriteLine(System.Environment.NewLine & "## Initial directory listing - recursive - without display of files")
        System.Console.WriteLine(IOClient.RootDirectory.ToStringListing(True, False))

        Assert.Pass("Success: no exception occured")
    End Sub

    <Test> Public Sub RootDirectory()
        System.Console.WriteLine(IOClient.RootDirectory.ToStringListing(False, True))
        System.Console.WriteLine("Found " & IOClient.RootDirectory.GetDirectories(0, False).Length & " sub directories")
        Assert.NotZero(IOClient.RootDirectory.GetDirectories(0, False).Length)
    End Sub

End Class

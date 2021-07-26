public sealed class Settings
{
    private const string AppTitle = "Scopevisio.Teamwork.Test";

    private static string BufferFilePath(string fieldName)
    {
        string HashedFieldName;

        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(fieldName);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (int i = 0; i <= hashBytes.Length - 1; i++)
                sb.Append(hashBytes[i].ToString("X2"));

            HashedFieldName = sb.ToString();
        }

        return System.IO.Path.Combine(System.IO.Path.GetTempPath(), "~Buffer." + AppTitle + "." + HashedFieldName + ".tmp");
    }

    public static void PersistInputValue(string fieldName, string value)
    {
        System.IO.File.WriteAllText(BufferFilePath(fieldName), value);
    }

    public static string InputFromBufferFile(string fieldName)
    {
        string BufferFile = BufferFilePath(fieldName);

        string EnvVarName = "TEST_" + fieldName.Replace(" ", "").Replace(".", "").ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(System.Environment.GetEnvironmentVariable(EnvVarName)))
        {
            return System.Environment.GetEnvironmentVariable(EnvVarName);
        }

        if (System.IO.File.Exists(BufferFile))
            return System.IO.File.ReadAllText(BufferFile);
        else
            return null;
    }
}

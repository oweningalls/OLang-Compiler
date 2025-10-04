public static class TestPathHelper
{
    public static string GetProjectRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (dir != null && !dir.GetFiles("*.csproj").Any())
        {
            dir = dir.Parent;
        }

        if (dir == null)
            throw new DirectoryNotFoundException("Could not find project root (no .csproj found).");

        return dir.FullName;
    }
}

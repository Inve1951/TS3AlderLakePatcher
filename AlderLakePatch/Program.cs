namespace AlderLakePatch;

public static class Program
{
    public static int Main()
    {
        var args = Environment.GetCommandLineArgs();
        if (args.Length != 2)
        {
            Console.Error.WriteLine($"Usage: dotnet {args[0]} <exe path>");
            return 1;
        }

        var filePath   = args[1];
        var intelPath  = Path.Combine(Path.GetDirectoryName(filePath) ?? ".", "IntelFix.dll");
        var backupName = Path.GetFileNameWithoutExtension(filePath) + ".bak";
        var backupPath = Path.Combine(Path.GetDirectoryName(filePath) ?? ".", backupName);
        try
        {
            File.Copy(filePath, backupPath, false);
        }
        catch (Exception ex)
        {
            if (ex is not IOException || !File.Exists(backupPath))
            {
                Console.Error.WriteLine($"Failed to write backup to '{backupPath}': {ex}");
                return 1;
            }
        }

        try
        {
            var peFile = new PeNet.PeFile(filePath);
            peFile.AddImport("IntelFix.dll", "_DllMain@12");
            File.WriteAllBytes(filePath, peFile.RawFile.ToArray());
            File.Copy("IntelFix.dll", intelPath, true);
            Console.Out.WriteLine("Executable patched successfully! Created a backup as " + backupName);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to patch executable: {ex}");
            return 1;
        }

        return 0;
    }
}

using ConsoleTables;

namespace Clic.Commands;

internal class Ls : CommandBase
{
    public override string Name => "ls";

    private readonly Func<string, string> convertString = (input) =>
    {
        if (input.Length > Console.WindowWidth / 2) return input[..20];
        return input;
    };

    public override async Task<CommandReturnValue> CommandInvoked(string[] args, CommandArguments ca)
    {
        WorkingFolder folder = ca.WorkingFolder;
        
        bool sizeRequested = false;
        string path = await folder.GetPath();

        if (args.Length != 0 && args[0] == "-d") // arguments check
        {
            sizeRequested = true;
            if (args.Length > 1)
            {
                if (Path.Exists(args[1]))
                {
                    path = args[1];
                }
                else
                {
                    ca.TextProvider.WriteError($"""\nPath "{args[0]}" not found""");
                    return new(-1);
                }
            }
        }
        else if (args.Length != 0)
        {
            if (Path.Exists(args[0]))
            {
                path = args[0];
            }
            else
            {
                ca.TextProvider.WriteError($"""\nPath "{args[0]}" not found""");
                return new(-1);
            }
        }

        ConsoleTable table = new("Name", "Größe [kB]", "Typ");
        DirectoryInfo directoryInfo = new(path);
        List<FileInfo> files = new(directoryInfo.EnumerateFiles());
        List<DirectoryInfo> directories = new(directoryInfo.EnumerateDirectories());
        List<string> names = [];
        List<double> sizes = [];

        foreach (FileInfo file in files)
        {
            names.Add(file.Name);
            sizes.Add(file.Length / 1024);
        }

        foreach (DirectoryInfo directory in directories)
        {
            names.Add(directory.Name);
        }

        Console.WriteLine();
        Console.WriteLine("Directory " + directoryInfo.FullName);
        Console.WriteLine();
        int i;
        for (i = 0; i < sizes.Count; i++)
        {
            double size = sizes[i];
            string name = names[i];
            table.AddRow(convertString(name), size, "Datei");
        }

        for (; i < names.Count; i++)
        {
            string name = names[i];
            if (sizeRequested)
            {
                long size = GetDirectorySize(directoryInfo.FullName + "\\" + name) / 1024;
                table.AddRow(convertString(name), size, "Ordner");
            }
            else
                table.AddRow(convertString(name), "", "Ordner");
        }

        table.Write(Format.Minimal);
        return new CommandReturnValue(0);
    }


    static readonly EnumerationOptions enumOptions = new()
    {
        IgnoreInaccessible = true,
        RecurseSubdirectories = false
    };
    static long GetDirectorySize(string path)
    {
        DirectoryInfo directoryInfo = new(path);
        List<FileInfo> files = new(directoryInfo.EnumerateFiles("*", enumOptions));

        var dirs = Directory.EnumerateDirectories(path, "*", enumOptions);
        long size = 0;

        foreach (var dir in dirs)
        {
            size += GetDirectorySize(dir);
        }
        foreach (var file in files)
        {
            size += file.Length;
        }

        return size;
    }
    public override void Setup()
    {
        
    }

    public override string GetHelpText()
    {
        throw new NotImplementedException();
    }
}

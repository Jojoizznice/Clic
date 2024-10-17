
namespace Clic.Commands;

internal class Cd : CommandBase
{
    public override string Name => "cd";

    public override async Task<CommandReturnValue> CommandInvoked(string[] args, CommandArguments ca)
    {
        WorkingFolder folder = ca.WorkingFolder;
        
        if (args.Length == 0)
        {
            Console.WriteLine(folder.GetPath());
            return new(0);
        }
        
        bool result = await folder.SetPath(args[0]);
        if (result)
        {
            return new(0);
        }

        string current = await folder.GetPath();
        string concat = $"{current}{(current.EndsWith('/') || current.EndsWith('\\') ? "" : "/" )}{args[0]}";

        result = await folder.SetPath(concat);
        if (result)
        {
            return new(0);
        }

        ca.TextProvider.WriteError($"\nDas System kann den angegebenen Pfad {concat} nicht finden");
        return new(-1);
    }

    public override void Setup()
    {

    }

    public override string GetHelpText()
    {
        return """
            Usage: cd ([path])

            Description: Sets 
            """;
    }
}

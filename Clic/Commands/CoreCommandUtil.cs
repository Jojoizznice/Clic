using System.Security.Cryptography;
using Clic.Commands.Variables;
using ConsoleTables;

namespace Clic.Commands;

internal class CoreCommandUtil
{
    public static Task ListCommands(CommandBase[] commands)
    {
        Console.WriteLine("Available Commands:\n");
        ConsoleTable consoleTable = new("command", "class", "aliases");

        foreach (var command in commands)
        {
            string aliases = "";
            foreach (string al in command.Aliases)
            {
                aliases += $"{al}, ";
            }

            aliases = aliases == string.Empty ? "~" : aliases[..^2];

            consoleTable.AddRow(command.Name, command.GetType().ToString(), aliases);
        }

        consoleTable.Write(Format.Minimal);
        return Task.CompletedTask;
    }

    public static Task CommandHelp(CommandBase[] commands, TextProvider textProvider, string[] args)
    {
        if (args.Length != 1)
        {
            textProvider.WriteError($"\nExcpected one command. If you want to see all available commands, use lscmd");

            return Task.CompletedTask;
        }

        CommandBase? c = null;

        foreach (var command in commands)
        {
            if (command.Name == args[0] || command.Aliases.Contains(args[0]))
            {
                c = command; break;
            }
        }

        if (c is null)
        {
            textProvider.WriteError($"\nCommand {args[0]} not found");

            return Task.CompletedTask;
        }

        textProvider.WriteLine("\n" + c.GetHelpText());

        return Task.CompletedTask;
    }
}

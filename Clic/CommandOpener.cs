using System.Text;
using Clic.Commands;
using Clic.Commands.Variables;

namespace Clic;

internal class CommandOpener
{
    private readonly CommandBase[] commands;
    private readonly TextProvider textProvider = new();

    public CommandOpener(CommandBase[] commands)
    {
        this.commands = commands;
    }

    public async Task OpenCommand(string input, CommandArguments commandArguments)
    {
        string command = input.Split(' ')[0].ToLower();
        string[] args = FormattedSplit(input);
        var (modCommand, modArgs) = ReplaceVariables(args, command, commandArguments.VariableHolder);

        foreach (var c in commands)
        {
            if (c.Name == modCommand || c.Aliases.Contains(modCommand))
            {
                await c.CommandInvoked(modArgs, commandArguments);
                Console.WriteLine();
                return;
            }
        }

        switch (modCommand)
        {
            case "lscmd":
                await CoreCommandUtil.ListCommands(commands);
                return;

            case "help":
                await CoreCommandUtil.CommandHelp(commands, commandArguments.TextProvider, modArgs);
                return;
        }

        textProvider.WriteError($"Command \"{modCommand}\" not found\n");
    }

    static string[] FormattedSplit(string input, bool includeCommand = false) 
        // Converts a command like "do this and that" to "do" and ["this", "and", "that"]
    {
        List<string> resultStrings = [];
        ReadOnlySpan<char> chars = input;

        int commandPos = chars.IndexOf(' ') + 1; // ignore the command and its space
        if (commandPos <= 0) // if there is only a command there is nothing to do.
        {
            return includeCommand ? [input.Trim()] : [];
        }

        if (includeCommand)
            resultStrings.Add(chars[..commandPos].ToString());
        chars = chars[commandPos..];

        while (true)
        {
            if (chars.Length == 0) break;

            if (chars.StartsWith(" "))
            {
                chars = chars[1..];
                continue;
            }

            if (chars.StartsWith("\""))
            {
                ExtractQuote(resultStrings, ref chars);
                continue;
            }

            int space = chars.IndexOf(" ");
            if (space < 0)
            {
                resultStrings.Add(chars.ToString());
                break;
            }

            var span = chars[..space];
            chars = chars[(space + 1)..]; // (space + 1) because we need to remove the space
            resultStrings.Add(span.ToString());
        }

        return [.. resultStrings];
    }

    private static void ExtractQuote(List<string> resultStrings, scoped ref ReadOnlySpan<char> chars)
    {
        chars = chars[1..];
        int secondQuotes = chars.IndexOf("\"");
        if (secondQuotes < 0)
        {
            chars = chars.ToString() + '\"'; // If no further qMarks are found, we assume everything until the end
            secondQuotes = chars.IndexOf("\"");
        }

        var span = chars[..secondQuotes];

        chars = chars[(secondQuotes + 1)..]; //secondQuotes + 1 to remove qMark

        resultStrings.Add(span.ToString());
    }

    /* This aint it because "hi     hi" => "hi hi"
     * // Split At Spaces And Not At Quotation Marks
    static string[] FormattedSplit(string input)
    {
        string[] inputSplit = input.Split(' ');
        List<string> finalString = [];
        StringBuilder buff = new();

        for (int i = 1; i < inputSplit.Length; i++) // We skip the first because everybody knows what was called
        {
            string line = inputSplit[i];
            // If there are no quotation marks we can skip this
            if (!line.StartsWith('"') && buff.Length == 0)
            {
                finalString.Add(line);
                continue;
            }

            // If a quotation block starts here we append
            if (line.StartsWith('"'))
            {
                buff.Append(line);
                continue;
            }

            // If a quotation block ends here we append the block and clear
            if (buff.Length != 0 && line.EndsWith('"')) 
            {
                buff.Append(" " + line);
                finalString.Add(line);
                buff.Clear();
                continue;
            }

            // If we are in a quotation block we just append
            if (buff.Length != 0)
            {
                buff.Append(line);
                continue;
            }

            throw new UnreachableException("Reached the end of CommandOpener.FormattedSplit()");
        }

        return [.. finalString];
    }*/

    private static (string command, string[] output) ReplaceVariables(string[] input, string command, VariableHolder variables)
    {
        string newComm = ReplaceVariableInString(command, variables);
        string[] newCommWithoutFragments = FormattedSplit(newComm, true); // if first element is var and is ie "echo hi" we only want echo

        newComm = newCommWithoutFragments[0].Trim();

        List<string> output = new(input.Length);
        if (newCommWithoutFragments.Length > 1)
        {
            output.AddRange(newCommWithoutFragments[1..]);
        }

        foreach (string item in input)
        {
            string replaced = ReplaceVariableInString(item, variables);
            output.Add(replaced);

        }

        return (newComm, [.. output]);
    }

    private static string ReplaceVariableInString(string text, VariableHolder variables)
    {
        int index = text.IndexOf('$');
        if (index < 0) return text;

        StringBuilder sb = new();
        ReadOnlySpan<char> chars = text;

        while (true)
        {
            if (chars.Length == 0) break;

            index = text.IndexOf('$');
            chars = chars[index..];
            sb.Append(chars[..index]);

            index = chars.IndexOf(' ') < 0 ? chars.Length - 1 : chars.IndexOf(' '); // avoid variables with no end (omg)
            ReadOnlySpan<char> varName = chars[1..(index + 1)]; // remove dollar
            Variable? var = variables.FindName(varName.ToString());

            string? varText = var?.GetValueString();
            varText ??= $"${varName}";
            sb.Append(varText);

            chars = chars[(index + 1)..];
        }

        return sb.ToString();
    }
}

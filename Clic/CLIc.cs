using Clic.Commands.Variables;

namespace Clic;

internal class CLIc
{
    private readonly WorkingFolder _workingFolder;
    private readonly InputHandler _inputHandler;
    private readonly CommandOpener _commandOpener;
    private readonly VariableHolder _variableHolder;
    private readonly TextProvider _textProvider;

    public CLIc(CLIcStartupArguments args)
    {
        Console.CursorVisible = false;
        _inputHandler = new();

        CommandBase[] commands = CommandBase.SetupCommands();
        _commandOpener = new(commands);

        _workingFolder = new(args.Path);
        _variableHolder = new();
        _textProvider = new();
    }

    public async Task StartConsole()
    {
        _textProvider.WriteLine(banner + "\n");
    
    Go:
        Console.Write(await _workingFolder.GetPath() + "; ");
        string input = await _inputHandler.GetInput();

        await _commandOpener.OpenCommand(input, await GatherCommandArguments());
        goto Go;
    }

    private Task<CommandArguments> GatherCommandArguments()
    {
        return Task.FromResult(new CommandArguments()
        {
            WorkingFolder = _workingFolder,
            VariableHolder = _variableHolder,
            TextProvider = _textProvider
        });
    }

    const string banner = " ██████╗██╗     ██╗ ██████╗\r\n██╔════╝██║     ██║██╔════╝\r\n██║     ██║     ██║██║     \r\n██║     ██║     ██║██║     \r\n╚██████╗███████╗██║╚██████╗\r\n ╚═════╝╚══════╝╚═╝ ╚═════╝";
}

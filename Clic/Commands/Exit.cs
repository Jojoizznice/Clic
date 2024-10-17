
namespace Clic.Commands;

internal class Exit : CommandBase
{
    public override string Name => "exit";

    public override Task<CommandReturnValue> CommandInvoked(string[] args, CommandArguments folder)
    {
        Environment.Exit(0);
        return Task.FromResult(new CommandReturnValue(null));
    }

    public override string GetHelpText()
    {
        throw new NotImplementedException();
    }

    public override void Setup()
    {
    }
}

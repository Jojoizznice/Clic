namespace Clic.Commands;

internal class Write : CommandBase
{
    public override string Name => "write";
    public override string[] Aliases { get; set; } = [
        "echo",
        "e"
        ];

    public override Task<CommandReturnValue> CommandInvoked(string[] args, CommandArguments folder)
    {        
        //StringBuilder sb = new();
        foreach (var item in args)
        {
            //sb.Append(item);
            Console.WriteLine(item);
        }

        //Console.WriteLine(sb.ToString());
        return Task.FromResult(new CommandReturnValue(0));
    }

    public override string GetHelpText()
    {
        return
            """
            1. USAGE

                write ([text])

            2. DESCRIPTION

                Prints the text from ([text]). If empty, a new line is printed

            """;
    }

    public override void Setup()
    {
        
    }
}

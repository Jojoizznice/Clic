using System.Reflection;

namespace Clic;

/// <summary>
/// This class is the Base for CLIc commands which all command classes must inherit from.
/// </summary>
internal abstract class CommandBase
{
    /// <summary>
    /// Contains the name of the command. Must be only lowercase ASCII letters
    /// could be nameof([class]).ToLower()
    /// </summary>
    public abstract string Name { get; }
    public virtual string[] Aliases { get; set; } = [];
    /// <summary>
    /// This method is called when the command is invoked through its respective <see cref="Name"/>
    /// </summary>
    /// <param name="args">A <see cref="string"/>[] representing the arguments representing the arguments given by the user</param>
    public abstract Task<CommandReturnValue> CommandInvoked(string[] args, CommandArguments ca);
    /// <summary>
    /// This method is called once at startup
    /// </summary>
    public abstract void Setup();
    public virtual bool IsAlias { get; } = false;

    public abstract string GetHelpText();

    protected CommandBase() { }

    internal static CommandBase[] SetupCommands()
    {
        var commandTypes = from t in Assembly.GetExecutingAssembly().GetTypes()
                           where t.Namespace!.Contains("Clic.Commands")
                           where t.BaseType == typeof(CommandBase)
                           select t;

        List<CommandBase> commands = [];

        foreach (var commandType in commandTypes)
        {
            CommandBase command = (CommandBase)commandType.GetConstructor([])!.Invoke(null)!;

            foreach (char c in command.Name)
            {
                if (!char.IsAscii(c) || !char.IsLower(c))
                {
                    throw new CommandException($"""The Name of command "{command.Name}" must only be lowercase ASCII letters""");
                }
            }

            command.Setup();
            commands.Add(command);
        }

        return [.. commands];
    }
}

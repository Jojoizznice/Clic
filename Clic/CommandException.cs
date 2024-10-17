namespace Clic;

internal class CommandException : SystemException
{
    public CommandException(string? message) : base(message)
    {
    }

    public CommandException(string? message, Exception innerException) : base(message, innerException)
    {
    }
}

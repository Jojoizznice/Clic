namespace Clic;

public class TextProvider
{
    public string ReadLine(string? text = null)
    {
        Console.WriteLine(text);
        return Console.ReadLine()!;
    }

    public void WriteLine(string? text = null) 
    {
        Console.WriteLine(text);
    }

    public void WriteError(string text)
    {
        ConsoleColor backgroundColor = Console.BackgroundColor;
        ConsoleColor foregroundColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.BackgroundColor = ConsoleColor.Black;

        Console.WriteLine(text);

        Console.ForegroundColor = foregroundColor;
        Console.BackgroundColor = backgroundColor;
    }

    public ConsoleKeyInfo ReadKey(bool intercept = true)
    {
        return Console.ReadKey(intercept);
    }
}

namespace Clic;

internal class InputHandler
{
    public async Task<string> GetInput()
    {
        CancellationTokenSource source = new();
        Task blink = DoCursorBlinking(source.Token);
        List<char> input = [];
        int cursorPos = Console.CursorLeft;

        string filler = new(' ', Console.WindowWidth - Console.CursorLeft);

        while (true)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            ConsoleKey key = keyInfo.Key;

            if (key is ConsoleKey.Enter)
            {
                break;
            }
            if (key is ConsoleKey.Backspace)
            {
                if (input.Count == 0)
                    continue;

                input.RemoveAt(input.Count - 1);
                RepaintInput(input, cursorPos, filler);
                continue;
            }
            
            input.Add(keyInfo.KeyChar);
            RepaintInput(input, cursorPos, filler);
        }

        source.Cancel();
        await blink;
        string val = new(input.ToArray());

        Console.WriteLine(); // Neccessary because we only use Console.Write()
        return val;
    }

    private static void RepaintInput(List<char> input, int cursorPos, string filler)
    {
        int width = filler.Length - 1;
        ReadOnlySpan<char> text = input.ToArray(); //new string(input.ToArray()); (old)
        if (width < text.Length)
        {
            int start = text.Length - width;
            text = text.Slice(start, width);
        }

        Console.CursorLeft = cursorPos;
        Console.Write(filler);
        Console.CursorLeft = cursorPos;
        Console.Write(new string(text));
    }

    public async Task DoCursorBlinking(CancellationToken token)
    {
        bool counter = false;
        
        while (true)
        {
            counter = !counter;
            char blink = counter ? '\u2588' : ' ';

            Console.Write(blink);
            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);

            try
            {
                await Task.Delay(600, token);
            }
            catch (TaskCanceledException)
            {
                Console.Write(' ');
                return;
            }

            if (token.IsCancellationRequested) 
            {
                Console.Write(' ');
                return; 
            }
        }
    }
}

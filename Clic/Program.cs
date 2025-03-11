using org.mariuszgromada.math.mxparser;

namespace Clic;

internal class Program
{
    static async Task Main(string[] args)
    {
        _testfile.Test();
        License.iConfirmNonCommercialUse("J Riedmayer");

        try
        {
            var clic = new CLIc(new());
            await clic.StartConsole();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            Console.WriteLine("\nDrücken Sie Enter zum Neustarten");

            ConsoleKeyInfo cki = Console.ReadKey();
            if (cki.Key == ConsoleKey.Enter)
            {
                Console.Clear();
                await Main([]);
            }
        }
    }
}

using Hangman.Console;
using System;
using System.Threading.Tasks;
using Hangman.Core.Providers.Db;
using Hangman.Core.Providers.Interface;
using Hangman.Core.Localizations;

/// <summary>
/// Provides the entry point for the console application and initializes
/// the services required to run the Hangman game.
/// </summary>
class Program
{
    /// <summary>
    /// Starts the console application, initializes its dependencies,
    /// and delegates the application flow to the game controller.
    /// </summary>
    static async Task Main()
    {
        try
        {
            IUiStrings uiStrings = SelectLanguage();

            IStatisticsService statsService = new SqliteHangmanService();
            ConsoleRenderer renderer = new ConsoleRenderer(uiStrings);
            ConsoleInput input = new ConsoleInput(uiStrings);

            GameController controller = new GameController(statsService, uiStrings, input, renderer);

            await controller.RunAsync();
        }
        catch (Exception ex)
        {
            // Keep the top-level handler as a final safety net so an unexpected failure
            // does not terminate the console application without giving the user any context.
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Ett allvarligt, oväntat fel inträffade:");
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }

        Console.WriteLine("\nProgrammet har avslutats. Tryck valfri tangent.");
        Console.ReadKey();
    }

    /// <summary>
    /// Provides the language selection before the application is initialized,
    /// ensuring that all subsequent UI components use the user's chosen localization.
    /// </summary>
    private static IUiStrings SelectLanguage()
    {
        Console.Clear();
        Console.WriteLine("Välj språk / Select language:");
        Console.WriteLine("1. Svenska");
        Console.WriteLine("2. English");
        Console.Write("Ditt val / Your choice (1-2): ");

        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.KeyChar == '1')
            {
                Console.WriteLine("1");
                return new SwedishUiStrings();
            }
            if (key.KeyChar == '2')
            {
                Console.WriteLine("2");
                return new EnglishUiStrings();
            }
        }
    }
}

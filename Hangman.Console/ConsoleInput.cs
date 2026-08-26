using Hangman.Core.Localizations;
using Hangman.Core.Models;
using System.Text;

namespace Hangman.Console
{
    /// <summary>
    /// Handles all user input from the console.
    /// </summary>
    public class ConsoleInput
    {
        private readonly IUiStrings _strings;

        public ConsoleInput(IUiStrings strings)
        {
            _strings = strings;
        }

        /// <summary>
        /// Asynchronously reads a single letter guess.
        /// Cancellation is controlled by the provided token, which is used by the game timer.
        /// Returns '\0' when the user presses Escape.
        /// Throws OperationCanceledException when the timer expires.
        /// </summary>
        public async Task<char> GetGuess(IEnumerable<char> usedLetters, CancellationToken token)
        {
            // The prompt is rendered as part of the complete game screen so that all console output
            // remains synchronized with the game's current state.
            while (!token.IsCancellationRequested)
            {
                if (System.Console.KeyAvailable)
                {
                    var key = System.Console.ReadKey(intercept: true);

                    if (key.Key == ConsoleKey.Escape)
                    {
                        lock (ConsoleRenderer.ConsoleLock)
                        {
                            System.Console.WriteLine();
                        }
                        return '\0'; // Escape
                    }

                    char letter = key.KeyChar;

                    if (!char.IsLetter(letter))
                    {
                        lock (ConsoleRenderer.ConsoleLock)
                        {
                            System.Console.ForegroundColor = ConsoleColor.Yellow;
                            System.Console.WriteLine(_strings.GetGuessInvalid(letter));
                            System.Console.ResetColor();
                        }
                        return (char)1; // Signal an invalid guess
                    }

                    char upperGuess = char.ToUpperInvariant(letter);

                    if (usedLetters.Contains(upperGuess))
                    {
                        lock (ConsoleRenderer.ConsoleLock)
                        {
                            System.Console.ForegroundColor = ConsoleColor.Yellow;
                            System.Console.WriteLine(_strings.GetGuessAlreadyGuessed(upperGuess));
                            System.Console.ResetColor();
                        }
                        return (char)1; // Signal an invalid guess
                    }

                    lock (ConsoleRenderer.ConsoleLock)
                    {
                        System.Console.WriteLine(upperGuess);
                    }
                    return upperGuess;
                }

                // Polling is used here because Console.ReadKey cannot be awaited or directly cancelled.
                // A short delay prevents this input loop from continuously consuming CPU while waiting.
                await Task.Delay(100, token);
            }

            throw new OperationCanceledException();
        }

        /// <summary>
        /// Reads a player name and ensures that it contains meaningful input.
        /// Returns null when the user presses Escape.
        /// </summary>
        public string? GetPlayerName(string prompt)
        {
            string? name;
            while (true)
            {
                name = GetInputString(prompt);

                if (name == null) return null;

                if (!string.IsNullOrWhiteSpace(name))
                {
                    return name.Trim();
                }

                lock (ConsoleRenderer.ConsoleLock)
                {
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    System.Console.WriteLine(_strings.GetPlayerNameEmpty);
                    System.Console.ResetColor();
                }
            }
        }

        /// <summary>
        /// Reads a string from the user while supporting Backspace and Escape.
        /// The method returns null when the current input operation is cancelled with Escape.
        /// </summary>
        public string? GetInputString(string prompt)
        {
            lock (ConsoleRenderer.ConsoleLock)
            {
                System.Console.Write(prompt);
            }
            var sb = new StringBuilder();

            while (true)
            {
                var key = System.Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Escape)
                {
                    lock (ConsoleRenderer.ConsoleLock)
                    {
                        System.Console.WriteLine($"\n{_strings.CommonFeedbackCancelling}");
                    }
                    return null;
                }

                if (key.Key == ConsoleKey.Enter)
                {
                    lock (ConsoleRenderer.ConsoleLock)
                    {
                        System.Console.WriteLine();
                    }
                    return sb.ToString();
                }

                if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
                {
                    sb.Length--;
                    lock (ConsoleRenderer.ConsoleLock)
                    {
                        System.Console.Write("\b \b");
                    }
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    sb.Append(key.KeyChar);
                    lock (ConsoleRenderer.ConsoleLock)
                    {
                        System.Console.Write(key.KeyChar);
                    }
                }
            }
        }

        /// <summary>
        /// Waits for a valid menu selection, such as '1' through '6'.
        /// Returns '\0' when Escape is allowed and pressed.
        /// </summary>
        public char GetMenuChoice(string validChars, bool allowEscape = false)
        {
            while (true)
            {
                var key = System.Console.ReadKey(intercept: true);

                if (allowEscape && key.Key == ConsoleKey.Escape)
                {
                    lock (ConsoleRenderer.ConsoleLock)
                    {
                        System.Console.WriteLine(_strings.CommonFeedbackCancelling);
                    }
                    return '\0';
                }

                if (validChars.Contains(key.KeyChar))
                {
                    lock (ConsoleRenderer.ConsoleLock)
                    {
                        System.Console.WriteLine(key.KeyChar);
                    }
                    return key.KeyChar;
                }
            }
        }

        /// <summary>
        /// Reads a Yes/No response.
        /// Returns true for 'J' (or 'Y'), and false for 'N' or Escape.
        /// </summary>
        public bool GetYesNo(string prompt)
        {
            lock (ConsoleRenderer.ConsoleLock)
            {
                System.Console.Write(prompt);
            }
            while (true)
            {
                var key = System.Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Escape ||
                    key.KeyChar == 'n' || key.KeyChar == 'N')
                {
                    lock (ConsoleRenderer.ConsoleLock)
                    {
                        System.Console.Write(_strings.FeedbackEndingLoop);
                    }
                    return false;
                }

                if (key.KeyChar == 'j' || key.KeyChar == 'J' ||
                    key.KeyChar == 'y' || key.KeyChar == 'Y')
                {
                    lock (ConsoleRenderer.ConsoleLock)
                    {
                        System.Console.WriteLine(_strings.FeedbackContinuing);
                    }
                    return true;
                }
            }
        }

        /// <summary>
        /// Displays the language selection menu used when adding custom words.
        /// Returns null when the user cancels the selection with Escape.
        /// </summary>
        public WordLanguage? SelectLanguage()
        {
            lock (ConsoleRenderer.ConsoleLock)
            {
                System.Console.Clear();
                System.Console.WriteLine(_strings.AddWordSelectLanguageTitle);
                System.Console.WriteLine(_strings.AddWordLanguageSwedish);
                System.Console.WriteLine(_strings.AddWordLanguageEnglish);
                System.Console.WriteLine(_strings.CommonPressEscapeToCancel);
                System.Console.Write(_strings.AddWordSelectLanguagePrompt);
            }

            var choice = GetMenuChoice("12", allowEscape: true);
            switch (choice)
            {
                case '1':
                    return WordLanguage.Swedish;
                case '2':
                    return WordLanguage.English;
                default:
                    return null;
            }
        }
    }
}

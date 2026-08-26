using Hangman.Core.Localizations;
using Hangman.Core;
using Hangman.Core.Models;

namespace Hangman.Console
{
    /// <summary>
    /// Handles all rendering to the console.
    /// Responsible for drawing menus, game screens, ASCII art, and feedback.
    /// </summary>
    public class ConsoleRenderer
    {
        // Shared lock ensures that concurrent timer, input, and rendering operations cannot corrupt the console output.
        public static readonly object ConsoleLock = new object();

        private readonly IUiStrings _strings;

        private static readonly string[] HangmanStages =
        {
            "\n    +---+\n    |   |\n        |\n        |\n        |\n        |\n    =======",
            "\n    +---+\n    |   |\n    O   |\n        |\n        |\n        |\n    =======",
            "\n    +---+\n    |   |\n    O   |\n    |   |\n        |\n        |\n    =======",
            "\n    +---+\n    |   |\n    O   |\n   /|   |\n        |\n        |\n    =======",
            "\n    +---+\n    |   |\n    O   |\n   /|\\  |\n        |\n        |\n    =======",
            "\n    +---+\n    |   |\n    O   |\n   /|\\  |\n   /    |\n        |\n    =======",
            "\n    +---+\n    |   |\n    O   |\n   /|\\  |\n   / \\  |\n        |\n    ======="
        };

        private const int TimerLeftPos = 50;
        private const int TimerTopPos = 1;

        private const int AnimLeftPos = 4;
        private const int AnimTopPos = 10; // Positioned below the gallows

        private static readonly string[] _animFrames =
        {
            "*creak...* ",
            " *creak...* ",
            "  *creak...* ",
            "   *creak...* ",
            "    *creak...* ",
            "     *creak...*",
            "    *creak...* ",
            "   *creak...* ",
            "  *creak...* ",
            " *creak...* ",
            "*creak...* ",
            "               "
        };
        private const int AnimFrameCount = 12;

        public ConsoleRenderer(IUiStrings strings)
        {
            _strings = strings;
        }

        public void Clear()
        {
            lock (ConsoleLock)
            {
                System.Console.Clear();
            }
        }

        public void ShowWelcomeScreen()
        {
            lock (ConsoleLock)
            {
                Clear();
                System.Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.WriteLine(_strings.WelcomeTitleArt);
                System.Console.ResetColor();
                System.Console.WriteLine(_strings.WelcomeMessage);
                System.Console.WriteLine(_strings.WelcomePressAnyKey);
            }
            System.Console.ReadKey(true);
        }

        public void ShowMainMenu()
        {
            lock (ConsoleLock)
            {
                Clear();
                System.Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.WriteLine(_strings.MainMenuTitleArt);
                System.Console.WriteLine(_strings.MainMenuGallowsArt);
                System.Console.ResetColor();
                System.Console.WriteLine();
                System.Console.WriteLine(_strings.MainMenuTitle);
                System.Console.WriteLine(_strings.MenuPlaySingle);
                System.Console.WriteLine(_strings.MenuPlayTournament);
                System.Console.WriteLine(_strings.MenuShowHighscores);
                System.Console.WriteLine(_strings.MenuAddWord);
                System.Console.WriteLine(_strings.MenuHelp);
                System.Console.WriteLine(_strings.MenuQuit);
                System.Console.Write(_strings.MenuChoicePrompt);
            }
        }

        /// <summary>
        /// Draws the game screen based on the current state of the game.
        /// </summary>
        public void DrawGameScreen(Game game, string playerGuessing, int currentLives, string feedbackMessage)
        {
            lock (ConsoleLock)
            {
                Clear();
                System.Console.WriteLine(_strings.RoundTitleGame);

                System.Console.ForegroundColor = ConsoleColor.Green;
                if (currentLives > 0)
                    System.Console.WriteLine(_strings.RoundActivePlayerWithLives(playerGuessing, currentLives));
                else
                    System.Console.WriteLine($"{_strings.RoundActivePlayer} {playerGuessing}");
                System.Console.ResetColor();

                DrawHangman(game.Mistakes);
                System.Console.WriteLine();

                string maskedWord = string.Join(" ", game.GetMaskedWord().ToCharArray());
                System.Console.WriteLine($"{_strings.RoundWord} {maskedWord}");
                System.Console.WriteLine();

                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine(_strings.RoundMistakes(game.Mistakes, game.AttemptsLeft + game.Mistakes));
                System.Console.ResetColor();

                System.Console.ForegroundColor = ConsoleColor.Yellow;
                var sortedLetters = game.UsedLetters.OrderBy(c => c);
                System.Console.WriteLine($"{_strings.RoundGuessedLetters} [ {string.Join(", ", sortedLetters)} ]");
                System.Console.ResetColor();

                System.Console.WriteLine("\n---------------------");

                if (!string.IsNullOrEmpty(feedbackMessage))
                {
                    if (feedbackMessage == _strings.RoundFeedbackCancelled)
                        System.Console.ForegroundColor = ConsoleColor.DarkGray;
                    else if (feedbackMessage.StartsWith(_strings.RoundFeedbackCorrectGuess('X').Substring(0, 5)))
                        System.Console.ForegroundColor = ConsoleColor.Green;
                    else
                        System.Console.ForegroundColor = ConsoleColor.Yellow;

                    System.Console.WriteLine(feedbackMessage);
                    System.Console.ResetColor();
                }

                // The prompt is rendered with the complete game screen rather than by ConsoleInput.
                // Keeping it here ensures the cursor is restored to a predictable position after timer updates.
                System.Console.Write(_strings.GetGuessPrompt);
            }
        }

        /// <summary>
        /// Draws the gallows based on the number of mistakes.
        /// </summary>
        public void DrawHangman(int mistakes)
        {
            lock (ConsoleLock)
            {
                int stage = Math.Clamp(mistakes, 0, HangmanStages.Length - 1);
                System.Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.WriteLine(HangmanStages[stage]);
                System.Console.ResetColor();
            }
        }

        /// <summary>
        /// Displays the end screen for a completed round.
        /// </summary>
        public void ShowEndScreen(Game game, string feedbackMessage)
        {
            lock (ConsoleLock)
            {
                Clear();
                DrawHangman(game.Mistakes);
                System.Console.WriteLine();

                if (game.Status == GameStatus.Won)
                {
                    System.Console.ForegroundColor = ConsoleColor.Green;
                    System.Console.WriteLine(_strings.EndScreenCongrats);
                }
                else // Lost
                {
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    if (feedbackMessage == _strings.RoundFeedbackCancelled)
                        System.Console.WriteLine(_strings.EndScreenCancelled);
                    else
                        System.Console.WriteLine(_strings.EndScreenLost);
                }
                System.Console.ResetColor();
                System.Console.WriteLine(_strings.EndScreenCorrectWord(game.Secret));
                System.Console.WriteLine();
            }
        }

        /// <summary>
        /// Displays the help screen.
        /// </summary>
        public void ShowHelpScreen()
        {
            lock (ConsoleLock)
            {
                Clear();
                System.Console.WriteLine(_strings.HelpTitle);
                System.Console.WriteLine(_strings.HelpLine1);
                System.Console.WriteLine(_strings.HelpLine2);
                System.Console.WriteLine(_strings.HelpModesTitle);
                System.Console.WriteLine(_strings.HelpModesLine1);
                System.Console.WriteLine(_strings.HelpModesLine2);
                System.Console.WriteLine(_strings.HelpSourcesTitle);
                System.Console.WriteLine(_strings.HelpSourcesLine1);
                System.Console.WriteLine(_strings.HelpSourcesLine2);
                System.Console.WriteLine(_strings.HelpSourcesLine3);
            }
            WaitForKey(_strings.CommonPressAnyKeyToContinue);
        }

        /// <summary>
        /// Displays the high-score list.
        /// </summary>
        public void ShowHighscores(List<HighscoreEntry> topScores)
        {
            lock (ConsoleLock)
            {
                Clear();
                System.Console.WriteLine(_strings.HighscoreTitle);

                if (!topScores.Any())
                {
                    System.Console.WriteLine(_strings.HighscoreNoneFound);
                }
                else
                {
                    foreach (var diffGroup in topScores.GroupBy(s => s.Difficulty))
                    {
                        System.Console.WriteLine(_strings.HighscoreDifficultyHeader(diffGroup.Key));
                        int rank = 1;
                        foreach (var score in diffGroup.OrderByDescending(s => s.ConsecutiveWins))
                        {
                            System.Console.WriteLine(_strings.HighscoreEntry(rank++, score.PlayerName, score.ConsecutiveWins));
                        }
                    }
                }
            }
            WaitForKey(_strings.CommonPressAnyKeyToContinue);
        }

        /// <summary>
        /// Displays a color-coded feedback message.
        /// </summary>
        public void ShowFeedback(string message, ConsoleColor color = ConsoleColor.Gray)
        {
            lock (ConsoleLock)
            {
                System.Console.ForegroundColor = color;
                System.Console.WriteLine(message);
                System.Console.ResetColor();
            }
        }

        /// <summary>
        /// Displays a standardized error message.
        /// </summary>
        public void ShowError(string message)
        {
            lock (ConsoleLock)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine(message);
                System.Console.ResetColor();
            }
        }

        /// <summary>
        /// Displays a prompt and waits for a key press.
        /// </summary>
        public void WaitForKey(string prompt)
        {
            lock (ConsoleLock)
            {
                System.Console.WriteLine(prompt);
            }
            System.Console.ReadKey(true);
        }

        /// <summary>
        /// Draws the timer display at a fixed console position.
        /// This is used by the parallel timer task without rebuilding the entire game screen.
        /// </summary>
        public void DrawTimer(int secondsLeft)
        {
            lock (ConsoleLock)
            {
                int originalLeft = System.Console.CursorLeft;
                int originalTop = System.Console.CursorTop;

                System.Console.SetCursorPosition(TimerLeftPos, TimerTopPos);
                System.Console.ForegroundColor = ConsoleColor.Magenta;
                System.Console.Write(_strings.RoundTimerDisplay(secondsLeft));
                System.Console.ResetColor();

                System.Console.SetCursorPosition(originalLeft, originalTop);
            }
        }

        /// <summary>
        /// Clears the timer display when the round has ended.
        /// </summary>
        public void ClearTimerArea()
        {
            lock (ConsoleLock)
            {
                int originalLeft = System.Console.CursorLeft;
                int originalTop = System.Console.CursorTop;

                System.Console.SetCursorPosition(TimerLeftPos, TimerTopPos);
                System.Console.Write(new string(' ', _strings.RoundTimerDisplay(99).Length));

                System.Console.SetCursorPosition(originalLeft, originalTop);
            }
        }

        /// <summary>
        /// Draws the parallel creaking gallows animation.
        /// This is used by the parallel timer task to provide visual feedback while time is running.
        /// </summary>
        public void DrawAnimation(int secondsLeft)
        {
            lock (ConsoleLock)
            {
                int frame = secondsLeft % AnimFrameCount;
                string text = _animFrames[frame];

                int originalLeft = System.Console.CursorLeft;
                int originalTop = System.Console.CursorTop;

                System.Console.SetCursorPosition(AnimLeftPos, AnimTopPos);
                System.Console.ForegroundColor = ConsoleColor.DarkGray;
                System.Console.Write(text);
                System.Console.ResetColor();

                System.Console.SetCursorPosition(originalLeft, originalTop);
            }
        }

        /// <summary>
        /// Clears the animation display when the round has ended.
        /// </summary>
        public void ClearAnimationArea()
        {
            lock (ConsoleLock)
            {
                int originalLeft = System.Console.CursorLeft;
                int originalTop = System.Console.CursorTop;

                System.Console.SetCursorPosition(AnimLeftPos, AnimTopPos);
                System.Console.Write(new string(' ', _animFrames[0].Length));

                System.Console.SetCursorPosition(originalLeft, originalTop);
            }
        }
    }
}

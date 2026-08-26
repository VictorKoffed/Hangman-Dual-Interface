using Hangman.Core.Localizations;
using Hangman.Core;
using Hangman.Core.Exceptions;
using Hangman.Core.Models;
using Hangman.Core.Providers.Api;
using Hangman.Core.Providers.Interface;
using Hangman.Core.Providers.Local;
using System.Threading;

namespace Hangman.Console
{
    /// <summary>
    /// Coordinates the application's console flow, game logic,
    /// and communication between input handling and rendering.
    /// </summary>
    public class GameController
    {
        private readonly IStatisticsService _statisticsService;
        private readonly IUiStrings _strings;
        private readonly ConsoleInput _input;
        private readonly ConsoleRenderer _renderer;

        public GameController(IStatisticsService statisticsService, IUiStrings strings, ConsoleInput input, ConsoleRenderer renderer)
        {
            _statisticsService = statisticsService;
            _strings = strings;
            _input = input;
            _renderer = renderer;
        }

        /// <summary>
        /// Starts the application and keeps the main menu active until the user chooses to exit.
        /// </summary>
        public async Task RunAsync()
        {
            _renderer.ShowWelcomeScreen();

            while (true)
            {
                _renderer.ShowMainMenu();
                var choice = _input.GetMenuChoice("123456");

                switch (choice)
                {
                    case '1':
                        await PlaySinglePlayerAsync();
                        break;
                    case '2':
                        await PlayTournamentAsync();
                        break;
                    case '3':
                        await ShowHighscoresAsync();
                        break;
                    case '4':
                        await AddCustomWordAsync();
                        break;
                    case '5':
                        _renderer.ShowHelpScreen();
                        break;
                    case '6':
                        _renderer.Clear();
                        _renderer.ShowFeedback(_strings.FeedbackThanksForPlaying);
                        return;
                }
            }
        }

        /// <summary>
        /// Coordinates the single-player flow, including consecutive-win tracking
        /// and persistence of the final high score.
        /// </summary>
        private async Task PlaySinglePlayerAsync()
        {
            var (provider, currentDifficulty) = SelectWordSource();
            if (provider == null || !currentDifficulty.HasValue) return;

            string? playerName = _input.GetPlayerName(_strings.PromptPlayerName);
            if (playerName == null) return;

            int consecutiveWins = 0;
            GameStatus roundResult;

            do
            {
                string? secret = await GetWordFromProviderAsync(provider);
                if (secret == null)
                {
                    roundResult = GameStatus.Lost;
                    break;
                }

                var (status, _) = await PlayRoundWithFeedbackAsync(playerName, secret, 0);
                roundResult = status;

                if (roundResult == GameStatus.Won)
                {
                    consecutiveWins++;
                    _renderer.ShowFeedback(_strings.FeedbackWonRound(consecutiveWins), ConsoleColor.Green);

                    if (!_input.GetYesNo(_strings.PromptContinuePlaying))
                    {
                        roundResult = GameStatus.Lost;
                    }
                }
                else if (consecutiveWins > 0)
                {
                    _renderer.WaitForKey(_strings.FeedbackPressAnyKeyToSave);
                }

            } while (roundResult == GameStatus.Won);

            if (consecutiveWins > 0)
            {
                var newScore = new HighscoreEntry(playerName, consecutiveWins, currentDifficulty.Value)
                {
                    PlayerName = playerName,
                    ConsecutiveWins = consecutiveWins,
                    Difficulty = currentDifficulty.Value
                };
                await _statisticsService.SaveHighscoreAsync(newScore);

                _renderer.ShowFeedback(_strings.FeedbackHighscoreSaved(consecutiveWins, currentDifficulty.Value), ConsoleColor.Green);
            }

            _renderer.WaitForKey(_strings.FeedbackReturningToMenu);
        }

        /// <summary>
        /// Coordinates the two-player tournament flow, including round progression,
        /// life management, cancellation, and final tournament results.
        /// </summary>
        private async Task PlayTournamentAsync()
        {
            _renderer.Clear();
            _renderer.ShowFeedback(_strings.TournamentTitle, ConsoleColor.Cyan);

            string? p1Name = _input.GetPlayerName(_strings.PromptPlayer1Name);
            if (p1Name == null) return;

            string? p2Name = _input.GetPlayerName(_strings.PromptPlayer2Name);
            if (p2Name == null) return;

            var (provider, _) = SelectWordSource();
            if (provider == null) return;

            var tournament = new TwoPlayerGame(p1Name, p2Name, provider);

            _renderer.Clear();
            _renderer.ShowFeedback(_strings.FeedbackTournamentStarting(
                p1Name, p2Name, provider.DifficultyName,
                tournament.CurrentGuesser!.Name, TwoPlayerGame.MaxLives
            ), ConsoleColor.Green);
            _renderer.WaitForKey(_strings.FeedbackPressToStartRound);

            string roundFeedback = string.Empty;

            while (tournament.TournamentStatus == GameStatus.InProgress)
            {
                Player currentGuesser = tournament.CurrentGuesser!;
                string? secret;
                try
                {
                    // StartNewRoundAsync now communicates a normal end-of-tournament state through
                    // its return value instead of using an exception as part of normal control flow.
                    secret = await tournament.StartNewRoundAsync();
                }
                catch (NoCustomWordsFoundException ex)
                {
                    _renderer.ShowError(_strings.ErrorNoCustomWordsFound(ex.Difficulty, ex.Language));
                    _renderer.WaitForKey(_strings.CommonPressAnyKeyToContinue);
                    return;
                }
                // Unexpected provider failures still need to be translated into user-facing feedback
                // so infrastructure details do not leak into the normal tournament flow.
                catch (Exception ex)
                {
                    _renderer.ShowError(_strings.ErrorCouldNotFetchTournamentWord(ex.Message));
                    _renderer.WaitForKey(_strings.CommonPressAnyKeyToContinue);
                    return;
                }

                // A null word indicates that the tournament has already reached a terminal state.
                // The game object is responsible for updating that state before returning control here.
                if (secret == null)
                {
                    break;
                }

                var (roundResult, feedback) = await PlayRoundWithFeedbackAsync(currentGuesser.Name, secret, currentGuesser.Lives);
                roundFeedback = feedback;

                // A cancelled round is treated as a loss so that tournament state remains consistent
                // with the outcome of the round rather than leaving the current player unresolved.
                if (roundResult == GameStatus.Lost && roundFeedback == _strings.RoundFeedbackCancelled)
                {
                    tournament.HandleRoundEnd(GameStatus.Lost);
                    _renderer.ShowFeedback(_strings.FeedbackTournamentCancelled, ConsoleColor.DarkGray);
                    break;
                }

                tournament.HandleRoundEnd(roundResult);

                if (tournament.TournamentStatus == GameStatus.InProgress)
                {
                    _renderer.ShowFeedback(_strings.FeedbackTournamentRoundEnded);
                    _renderer.ShowFeedback(_strings.FeedbackTournamentLives(
                        tournament.Player1.Name, tournament.Player1.Lives,
                        tournament.Player2.Name, tournament.Player2.Lives
                    ), ConsoleColor.Yellow);
                    _renderer.WaitForKey(_strings.FeedbackTournamentNextGuesser(tournament.CurrentGuesser!.Name));
                }
            }

            if (roundFeedback != _strings.RoundFeedbackCancelled)
            {
                ShowTournamentResult(tournament);
            }

            _renderer.WaitForKey(_strings.CommonPressAnyKeyToContinue);
        }

        /// <summary>
        /// Plays one Hangman round with the shared timer and animation.
        /// Returns both the final game status and the feedback associated with the round outcome.
        /// </summary>
        private async Task<(GameStatus Status, string Feedback)> PlayRoundWithFeedbackAsync(string playerGuessing, string secret, int currentLives)
        {
            var game = new Game(6);
            game.StartNew(secret);
            string feedbackMessage = string.Empty;

            _renderer.Clear();
            _renderer.ShowFeedback(_strings.RoundTitleNewRound, ConsoleColor.Cyan);

            // Render the initial state before starting the timer to establish a stable cursor position
            // and prevent concurrent timer output from racing with the first game-screen render.
            _renderer.DrawGameScreen(game, playerGuessing, currentLives, feedbackMessage);

            var roundCts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            var startTime = DateTime.UtcNow;

            var timerTask = Task.Run(async () =>
            {
                try
                {
                    while (!roundCts.Token.IsCancellationRequested)
                    {
                        var elapsed = DateTime.UtcNow - startTime;
                        var remaining = (int)(60 - elapsed.TotalSeconds);
                        if (remaining < 0) remaining = 0;

                        _renderer.DrawTimer(remaining);
                        _renderer.DrawAnimation(remaining);

                        await Task.Delay(1000, roundCts.Token);
                    }
                }
                catch (TaskCanceledException)
                {
                    // Cancellation is the expected completion path when the round ends before the timer expires.
                }
            }, roundCts.Token);


            try
            {
                while (game.Status == GameStatus.InProgress)
                {
                    try
                    {
                        char guess = await _input.GetGuess(game.UsedLetters, roundCts.Token);

                        if (guess == '\0') // Escape
                        {
                            game.ForceLose();
                            feedbackMessage = _strings.RoundFeedbackCancelled;
                            continue;
                        }

                        if (guess == (char)1) // Invalid guess
                        {
                            // GetGuess has already displayed the validation feedback to avoid duplicating console output.
                        }
                        else
                        {
                            bool wasCorrect = game.Guess(guess);
                            feedbackMessage = wasCorrect ?
                                _strings.RoundFeedbackCorrectGuess(guess) :
                                _strings.RoundFeedbackWrongGuess(guess);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // The timer expired, so the round must be converted into an explicit loss
                        // before control returns to the normal game-state rendering flow.
                        game.ForceLose();
                        feedbackMessage = _strings.RoundTimerExpired;
                    }

                    if (game.Status == GameStatus.InProgress)
                    {
                        _renderer.DrawGameScreen(game, playerGuessing, currentLives, feedbackMessage);
                        feedbackMessage = string.Empty;
                    }
                }
            }
            finally
            {
                if (!roundCts.Token.IsCancellationRequested)
                {
                    roundCts.Cancel();
                }
                await Task.WhenAny(timerTask, Task.Delay(200));

                _renderer.ClearTimerArea();
                _renderer.ClearAnimationArea();
                roundCts.Dispose();
            }


            _renderer.ShowEndScreen(game, feedbackMessage);

            if (game.Status == GameStatus.Lost && feedbackMessage != _strings.RoundFeedbackCancelled)
                _renderer.ShowFeedback($"{playerGuessing} {_strings.RoundLost}", ConsoleColor.Red);
            else if (game.Status == GameStatus.Won)
                _renderer.ShowFeedback($"{playerGuessing} {_strings.RoundWon}", ConsoleColor.Green);

            return (game.Status, feedbackMessage);
        }

        /// <summary>
        /// Retrieves the global high scores and delegates their presentation to the renderer.
        /// </summary>
        private async Task ShowHighscoresAsync()
        {
            _renderer.Clear();
            _renderer.ShowFeedback(_strings.HighscoreTitle, ConsoleColor.Cyan);
            _renderer.ShowFeedback(_strings.HighscoreFetching);
            try
            {
                var topScores = await _statisticsService.GetGlobalTopScoresAsync(10);
                _renderer.ShowHighscores(topScores);
            }
            catch (Exception ex)
            {
                _renderer.ShowError(_strings.CommonErrorDatabaseError(ex.Message));
                _renderer.WaitForKey(_strings.CommonPressAnyKeyToContinue);
            }
        }

        /// <summary>
        /// Validates and persists a custom word together with its derived difficulty and language.
        /// </summary>
        private async Task AddCustomWordAsync()
        {
            _renderer.Clear();
            _renderer.ShowFeedback(_strings.AddWordTitle, ConsoleColor.Cyan);
            _renderer.ShowFeedback(_strings.CommonPressEscapeToCancel);

            string? word = string.Empty;
            while (string.IsNullOrWhiteSpace(word) || !word.All(char.IsLetter))
            {
                word = _input.GetInputString(_strings.AddWordPrompt);
                if (word == null) return;

                if (string.IsNullOrWhiteSpace(word) || !word.All(char.IsLetter))
                    _renderer.ShowFeedback(_strings.AddWordInvalid, ConsoleColor.Yellow);
            }

            word = word.ToUpperInvariant();
            WordDifficulty difficulty = GetDifficultyByLength(word);

            WordLanguage? language = _input.SelectLanguage();
            if (language == null) return;

            try
            {
                var saver = new CustomWordProvider(difficulty, language.Value);
                await saver.AddWordAsync(word, difficulty, language.Value);
                _renderer.ShowFeedback(_strings.AddWordSuccess(word, difficulty, language.Value), ConsoleColor.Green);
            }
            catch (WordAlreadyExistsException ex)
            {
                _renderer.ShowError(_strings.ErrorWordAlreadyExists(ex.Word, ex.Difficulty, ex.Language));
            }
            catch (Exception ex)
            {
                _renderer.ShowError(_strings.CommonErrorDatabaseError(ex.Message));
            }

            _renderer.WaitForKey(_strings.CommonPressAnyKeyToContinue);
        }

        /// <summary>
        /// Determines the game's difficulty from the word length so that custom words
        /// follow the same difficulty rules as words supplied by the other providers.
        /// </summary>
        private WordDifficulty GetDifficultyByLength(string word)
        {
            if (word.Length <= 4) return WordDifficulty.Easy;
            if (word.Length <= 7) return WordDifficulty.Medium;
            return WordDifficulty.Hard;
        }

        /// <summary>
        /// Displays the final tournament outcome, including the winner, loser,
        /// draw state, and accumulated wins for both players.
        /// </summary>
        private void ShowTournamentResult(TwoPlayerGame tournament)
        {
            _renderer.Clear();
            _renderer.ShowFeedback(_strings.FeedbackTournamentEnded, ConsoleColor.Cyan);

            if (tournament.TournamentStatus == GameStatus.Draw)
            {
                _renderer.ShowFeedback(_strings.FeedbackTournamentDraw, ConsoleColor.Yellow);
            }
            else
            {
                Player? winner = tournament.GetWinner();
                if (winner != null)
                {
                    Player loser = (winner == tournament.Player1) ? tournament.Player2 : tournament.Player1;
                    _renderer.ShowFeedback(_strings.FeedbackTournamentWinner(winner.Name), ConsoleColor.Green);
                    _renderer.ShowFeedback(_strings.FeedbackTournamentLoser(loser.Name));
                }
                else
                {
                    _renderer.ShowFeedback(_strings.FeedbackTournamentUnexpectedEnd);
                }
            }

            _renderer.ShowFeedback(_strings.FeedbackTournamentFinalWins);
            _renderer.ShowFeedback(_strings.FeedbackTournamentPlayerWins(tournament.Player1.Name, tournament.Player1.Wins));
            _renderer.ShowFeedback(_strings.FeedbackTournamentPlayerWins(tournament.Player2.Name, tournament.Player2.Wins));
        }

        /// <summary>
        /// Retrieves a word from the selected provider while translating provider failures
        /// into feedback appropriate for the console application.
        /// </summary>
        private async Task<string?> GetWordFromProviderAsync(IAsyncWordProvider provider)
        {
            _renderer.Clear();
            _renderer.ShowFeedback(_strings.FeedbackFetchingWord(provider.DifficultyName));
            try
            {
                return await provider.GetWordAsync();
            }
            catch (NoCustomWordsFoundException ex)
            {
                _renderer.ShowError(_strings.ErrorNoCustomWordsFound(ex.Difficulty, ex.Language));
                _renderer.WaitForKey(_strings.CommonPressAnyKeyToContinue);
                return null;
            }
            catch (Exception ex)
            {
                _renderer.ShowError(_strings.ErrorCouldNotStartGame(ex.Message));
                _renderer.WaitForKey(_strings.CommonPressAnyKeyToContinue);
                return null;
            }
        }

        /// <summary>
        /// Prompts the user to select a difficulty and returns null when the selection is cancelled.
        /// </summary>
        private WordDifficulty? SelectDifficulty(string source)
        {
            _renderer.Clear();
            _renderer.ShowFeedback(_strings.SelectDifficultyTitle(source), ConsoleColor.Cyan);
            _renderer.ShowFeedback(_strings.SelectDifficultyEasy);
            _renderer.ShowFeedback(_strings.SelectDifficultyMedium);
            _renderer.ShowFeedback(_strings.SelectDifficultyHard);
            _renderer.ShowFeedback(_strings.CommonPressEscapeToCancel);
            System.Console.Write(_strings.SelectDifficultyPrompt);

            var choice = _input.GetMenuChoice("123", allowEscape: true);
            switch (choice)
            {
                case '1': return WordDifficulty.Easy;
                case '2': return WordDifficulty.Medium;
                case '3': return WordDifficulty.Hard;
                default: return null;
            }
        }

        /// <summary>
        /// Creates the appropriate word provider based on the user's selected source and difficulty.
        /// </summary>
        private (IAsyncWordProvider? Provider, WordDifficulty? Difficulty) SelectWordSource()
        {
            _renderer.Clear();
            _renderer.ShowFeedback(_strings.SelectWordSourceTitle, ConsoleColor.Cyan);
            _renderer.ShowFeedback(_strings.SelectWordSourceApi);
            _renderer.ShowFeedback(_strings.SelectWordSourceLocal);
            _renderer.ShowFeedback(_strings.SelectWordSourceCustomSwedish);
            _renderer.ShowFeedback(_strings.SelectWordSourceCustomEnglish);
            _renderer.ShowFeedback(_strings.CommonPressEscapeToCancel);
            System.Console.Write(_strings.SelectWordSourcePrompt);

            var choice = _input.GetMenuChoice("1234", allowEscape: true);
            WordDifficulty? difficulty;

            switch (choice)
            {
                case '1':
                    difficulty = SelectDifficulty(_strings.FeedbackWordSourceApi);
                    return difficulty.HasValue ? (new ApiWordProvider(difficulty.Value), difficulty) : (null, null);
                case '2':
                    difficulty = SelectDifficulty(_strings.FeedbackWordSourceLocal);
                    return difficulty.HasValue ? (new WordProvider(difficulty.Value), difficulty) : (null, null);
                case '3':
                    difficulty = SelectDifficulty(_strings.FeedbackWordSourceCustomSwedish);
                    return difficulty.HasValue ? (new CustomWordProvider(difficulty.Value, WordLanguage.Swedish), difficulty) : (null, null);
                case '4':
                    difficulty = SelectDifficulty(_strings.FeedbackWordSourceCustomEnglish);
                    return difficulty.HasValue ? (new CustomWordProvider(difficulty.Value, WordLanguage.English), difficulty) : (null, null);
                default:
                    return (null, null);
            }
        }
    }
}

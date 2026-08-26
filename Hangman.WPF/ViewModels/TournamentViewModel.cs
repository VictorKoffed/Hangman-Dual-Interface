using Hangman.Core;
using Hangman.Core.Exceptions;
using Hangman.Core.Models;
using Hangman.Core.Providers.Interface;
using Hangman.Core.Localizations;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Hangman.WPF.ViewModels
{
    /// <summary>
    /// Orchestrates the tournament mode between two players. 
    /// Manages the local game state, the timer, alternating turns, and coordinates 
    /// updates between the underlying core logic and the WPF UI.
    /// </summary>
    public class TournamentViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IAsyncWordProvider _wordProvider;
        private readonly Game _game;
        private readonly TwoPlayerGame _tournament;
        private readonly DispatcherTimer _timer;
        private readonly LocalizationProvider _strings;

        // Visual fluff: Frames for a simple ASCII animation to simulate a swinging rope.
        private static readonly string[] _animFrames =
        {
            "*creak...* ","* *creak...* ","  *creak...* ","   *creak...* ","    *creak...* ","     *creak...* ",
            "    *creak...* ","   *creak...* ","  *creak...* "," *creak...* ","*creak...* ","               "
        };
        private const int AnimFrameCount = 12;

        public LocalizationProvider Strings { get; }

        public Player Player1 => _tournament.Player1;
        public Player Player2 => _tournament.Player2;

        private string _currentGuesserName = "";
        public string CurrentGuesserName { get => _currentGuesserName; set { _currentGuesserName = value; OnPropertyChanged(); } }

        public string ActivePlayerText => _strings.RoundActivePlayer.Replace(":", "");

        private string _tournamentStatusMessage = "";
        public string TournamentStatusMessage { get => _tournamentStatusMessage; set { _tournamentStatusMessage = value; OnPropertyChanged(); } }

        private string _maskedWord = "Laddar...";
        public string MaskedWord { get => _maskedWord; set { _maskedWord = value; OnPropertyChanged(); } }

        private string _usedLetters = "";
        public string UsedLetters { get => _usedLetters; set { _usedLetters = value; OnPropertyChanged(); } }

        private string _gallowsImageSource = Pack("/Images/stage_0.png");
        public string GallowsImageSource { get => _gallowsImageSource; set { _gallowsImageSource = value; OnPropertyChanged(); } }

        private int _secondsLeft = 60;
        public int SecondsLeft { get => _secondsLeft; set { _secondsLeft = value; OnPropertyChanged(); OnPropertyChanged(nameof(TimerText)); } }

        public string TimerText => _strings.RoundTimerDisplay(SecondsLeft);

        private string _creakAnimationText = "";
        public string CreakAnimationText { get => _creakAnimationText; set { _creakAnimationText = value; OnPropertyChanged(); } }

        private bool _isRoundInProgress = false;
        public bool IsRoundInProgress
        {
            get => _isRoundInProgress;
            set
            {
                _isRoundInProgress = value;
                OnPropertyChanged();
                // When the round state changes (e.g., ends or starts), we must force the command binding 
                // to re-evaluate so the virtual keyboard buttons lock or unlock immediately.
                if (GuessCommand is RelayCommand rc) rc.RaiseCanExecuteChanged();
            }
        }

        private bool _isTournamentInProgress = true;
        public bool IsTournamentInProgress { get => _isTournamentInProgress; set { _isTournamentInProgress = value; OnPropertyChanged(); } }

        private string _tournamentEndMessage = "";
        public string TournamentEndMessage { get => _tournamentEndMessage; set { _tournamentEndMessage = value; OnPropertyChanged(); } }

        public ICommand GuessCommand { get; }
        public ICommand BackToMenuCommand { get; }
        public ICommand NextRoundCommand { get; }

        public char[] KeyboardLetters { get; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖ".ToCharArray();

        /// <summary>
        /// Initializes the tournament session, binds event listeners to the core game logic, and triggers the first round.
        /// </summary>
        public TournamentViewModel(MainViewModel mainViewModel, IAsyncWordProvider provider, GameSettings settings, LocalizationProvider strings)
        {
            _mainViewModel = mainViewModel;
            _wordProvider = provider;
            _strings = strings;
            Strings = strings;

            _tournament = new TwoPlayerGame(settings.PlayerName, settings.PlayerName2, _wordProvider);
            _game = new Game(6);

            // Subscribe to domain events. This keeps our ViewModel passively reacting to domain changes
            // rather than actively polling or embedding core game logic within the UI layer.
            _game.LetterGuessed += OnGameUpdated;
            _game.WrongLetterGuessed += OnGameUpdated;
            _game.GameEnded += OnRoundEnded;

            GuessCommand = new RelayCommand(MakeGuess, CanGuess);
            BackToMenuCommand = new RelayCommand(_ => ExitTournament());
            NextRoundCommand = new RelayCommand(async _ => await StartNewRound());

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;

            MaskedWord = _strings.FeedbackFetchingWord("...");

            // Fire-and-forget initialization of the first round.
            _ = StartNewRound();
        }

        /// <summary>
        /// Detaches all event handlers to prevent memory leaks.
        /// Core domain objects (like _game) or timers can outlive the UI component. 
        /// Unsubscribing ensures this ViewModel can be garbage collected when navigating away.
        /// </summary>
        private void CleanupEvents()
        {
            _timer.Stop();
            _timer.Tick -= Timer_Tick;

            if (_game != null)
            {
                _game.LetterGuessed -= OnGameUpdated;
                _game.WrongLetterGuessed -= OnGameUpdated;
                _game.GameEnded -= OnRoundEnded;
            }
        }

        private void ExitTournament()
        {
            CleanupEvents();
            _mainViewModel.NavigateToMenu();
        }

        /// <summary>
        /// Prepares the state for a new round and asynchronously fetches a new word.
        /// </summary>
        private async Task StartNewRound()
        {
            // Reset local UI states immediately to clear residual data from the previous round 
            // before the potentially slow async word-fetch blocks the progression.
            IsRoundInProgress = true;
            TournamentStatusMessage = string.Empty;
            MaskedWord = _strings.FeedbackFetchingWord("...");
            UsedLetters = "";
            GallowsImageSource = Pack("/Images/stage_0.png");
            CreakAnimationText = string.Empty;
            CurrentGuesserName = _tournament.CurrentPlayerName;

            OnPropertyChanged(nameof(ActivePlayerText));
            OnPropertyChanged(nameof(Player1));
            OnPropertyChanged(nameof(Player2));

            string? word;
            try
            {
                // Retrieve the next word for the round. The asynchronous method returns a nullable string 
                // where a null value gracefully signals the end of the tournament word pool.
                word = await _tournament.StartNewRoundAsync();
            }
            catch (NoCustomWordsFoundException ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        _strings.ErrorNoCustomWordsFound(ex.Difficulty, ex.Language),
                        _strings.SelectWordSourceTitle);
                });
                CleanupEvents();
                _mainViewModel.NavigateToMenu();
                return;
            }
            // Catching generic exceptions to handle unexpected network or API failures.
            // We avoid using exceptions (like InvalidOperationException) for standard flow control.
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        _strings.ErrorCouldNotFetchTournamentWord(ex.Message),
                        _strings.ErrorApiGeneric);
                });
                // Fallback word on API failure so the game doesn't crash completely.
                _game.StartNew("APIERROR");
                UpdateUiProperties();
                return;
            }

            // A null word signifies that the tournament has naturally concluded 
            // (e.g., word list exhausted or win criteria met).
            if (word == null)
            {
                EndTournament();
                return;
            }

            _game.StartNew(word);
            UpdateUiProperties();
            SecondsLeft = 60;
            _timer.Start();

            // Force WPF's input manager to immediately re-evaluate the CanExecute state 
            // of the guess buttons, enabling them for the new round.
            if (GuessCommand is RelayCommand rc) rc.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Handles the finalization of the tournament mode, evaluating the overall winner.
        /// </summary>
        private void EndTournament()
        {
            IsTournamentInProgress = false;
            IsRoundInProgress = false;

            CleanupEvents();

            CreakAnimationText = string.Empty;

            if (_tournament.TournamentStatus == GameStatus.Draw)
            {
                TournamentEndMessage = _strings.FeedbackTournamentDraw;
            }
            else
            {
                Player? winner = _tournament.GetWinner();
                TournamentEndMessage = winner != null
                    ? _strings.FeedbackTournamentWinner(winner.Name)
                    : _strings.FeedbackTournamentUnexpectedEnd;
            }

            TournamentEndMessage += $"\n\n{_strings.FeedbackTournamentFinalWins}\n" +
                                    $"{_strings.FeedbackTournamentPlayerWins(Player1.Name, Player1.Wins)}\n" +
                                    $"{_strings.FeedbackTournamentPlayerWins(Player2.Name, Player2.Wins)}";
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            SecondsLeft--;
            int frame = SecondsLeft % AnimFrameCount;
            CreakAnimationText = _animFrames[frame];

            if (SecondsLeft <= 0)
            {
                _timer.Stop();
                CreakAnimationText = string.Empty;
                TournamentStatusMessage = _strings.RoundTimerExpired;
                // Forcing a loss synchronizes the core domain state with the UI timeout event.
                _game.ForceLose();
            }
        }

        private bool CanGuess(object? parameter)
        {
            if (parameter is char letter)
            {
                // Disallow guessing if the round is over, or if the letter has already been played.
                return IsRoundInProgress && !_game.UsedLetters.Contains(letter);
            }
            return false;
        }

        private void MakeGuess(object? parameter)
        {
            if (parameter is char letter)
            {
                _game.Guess(letter);
                if (GuessCommand is RelayCommand rc) rc.RaiseCanExecuteChanged();
            }
        }

        private void OnGameUpdated(object? sender, char e)
        {
            UpdateUiProperties();
            if (GuessCommand is RelayCommand rc) rc.RaiseCanExecuteChanged();
        }

        private void OnRoundEnded(object? sender, GameStatus status)
        {
            _timer.Stop();
            IsRoundInProgress = false;
            CreakAnimationText = string.Empty;

            // CRITICAL: We must snapshot the active player's name BEFORE invoking HandleRoundEnd.
            // HandleRoundEnd transitions the game state to the next player's turn. If we construct 
            // the message after that call, it will attribute the win/loss to the wrong player.
            string playerNameWhoJustPlayed = _tournament.CurrentPlayerName;

            _tournament.HandleRoundEnd(status);

            if (status == GameStatus.Won)
            {
                // Use the preserved player name.
                TournamentStatusMessage = $"{playerNameWhoJustPlayed} {_strings.RoundWon} {_strings.EndScreenCorrectWord(_game.Secret)}";
            }
            else
            {
                if (string.IsNullOrEmpty(TournamentStatusMessage))
                    // Use the preserved player name.
                    TournamentStatusMessage = $"{playerNameWhoJustPlayed} {_strings.RoundLost} {_strings.EndScreenCorrectWord(_game.Secret)}";
            }

            UpdateUiProperties();
            OnPropertyChanged(nameof(Player1));
            OnPropertyChanged(nameof(Player2));
        }

        private void UpdateUiProperties()
        {
            MaskedWord = string.Join(" ", _game.GetMaskedWord().ToCharArray());
            UsedLetters = $"{_strings.RoundGuessedLetters} {string.Join(", ", _game.UsedLetters.OrderBy(c => c))}";
            GallowsImageSource = Pack($"/Images/stage_{_game.Mistakes}.png");
        }

        /// <summary>
        /// Helper method to format absolute WPF Pack URIs for resource resolution.
        /// Required by WPF to locate static assets like images compiled into the assembly.
        /// </summary>
        private static string Pack(string relative) =>
            $"pack://application:,,,/Hangman.WPF;component{relative}";
    }
}

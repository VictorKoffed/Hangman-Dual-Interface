using Hangman.Core.Models;
using System.Collections.Generic;
using System.Windows.Input;
using Hangman.Core.Localizations;

namespace Hangman.WPF.ViewModels
{
    /// <summary>
    /// Provides the configuration state and commands required to start a new game.
    /// </summary>
    public class GameSettingsViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private readonly GameMode _gameMode;

        public LocalizationProvider Strings { get; }

        public GameSettings CurrentSettings { get; set; }

        public bool IsTournamentMode { get; }

        /// <summary>
        /// Provides difficulty options together with their localized display names,
        /// allowing the UI to remain independent of the underlying enum values.
        /// </summary>
        public IEnumerable<KeyValuePair<WordDifficulty, string>> LocalizedDifficulties
        {
            get
            {
                yield return new KeyValuePair<WordDifficulty, string>(WordDifficulty.Easy, Strings.FeedbackDifficultyEasy);
                yield return new KeyValuePair<WordDifficulty, string>(WordDifficulty.Medium, Strings.FeedbackDifficultyMedium);
                yield return new KeyValuePair<WordDifficulty, string>(WordDifficulty.Hard, Strings.FeedbackDifficultyHard);
            }
        }

        public Dictionary<WordSource, string> WordSources { get; } = new()
        {
            { WordSource.Api, "Engelska (API)" },
            { WordSource.Local, "Svenska (Lokal)" },
            { WordSource.CustomSwedish, "Anpassad Ordlista (Svenska)" },
            { WordSource.CustomEnglish, "Anpassad Ordlista (Engelska)" }
        };

        public ICommand StartGameCommand { get; }
        public ICommand BackToMenuCommand { get; }

        /// <summary>
        /// Initializes the settings view model and establishes the appropriate
        /// default player configuration for the selected game mode.
        /// </summary>
        public GameSettingsViewModel(MainViewModel mainViewModel, GameMode mode, LocalizationProvider strings)
        {
            _mainViewModel = mainViewModel;
            _gameMode = mode;
            IsTournamentMode = (mode == GameMode.Tournament);
            Strings = strings;

            CurrentSettings = new GameSettings();

            if (IsTournamentMode)
            {
                CurrentSettings.PlayerName = Strings.DefaultPlayer1Name;
                CurrentSettings.PlayerName2 = Strings.DefaultPlayer2Name;
            }
            else
            {
                CurrentSettings.PlayerName = Strings.DefaultPlayerName;
            }

            StartGameCommand = new RelayCommand(StartGame);
            BackToMenuCommand = new RelayCommand(_ => _mainViewModel.NavigateToMenu());
        }

        /// <summary>
        /// Starts the configured game while ensuring required player names have
        /// meaningful localized defaults before navigation occurs.
        /// </summary>
        private void StartGame(object? _)
        {
            if (string.IsNullOrWhiteSpace(CurrentSettings.PlayerName))
            {
                CurrentSettings.PlayerName = IsTournamentMode ? Strings.DefaultPlayer1Name : Strings.DefaultPlayerName;
            }
            if (IsTournamentMode && string.IsNullOrWhiteSpace(CurrentSettings.PlayerName2))
            {
                CurrentSettings.PlayerName2 = Strings.DefaultPlayer2Name;
            }

            if (IsTournamentMode)
            {
                _mainViewModel.NavigateToTournament(CurrentSettings);
            }
            else
            {
                _mainViewModel.NavigateToGame(CurrentSettings);
            }
        }
    }
}

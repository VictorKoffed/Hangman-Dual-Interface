using Hangman.Core;
using Hangman.Core.Models;
using Hangman.Core.Providers.Api;
using Hangman.Core.Providers.Interface;
using Hangman.Core.Providers.Local;
using System.Windows.Input;
using Hangman.Core.Localizations;

namespace Hangman.WPF.ViewModels
{
    /// <summary>
    /// Serves as the root orchestrator for the WPF application's MVVM architecture.
    /// Manages the application's navigation state by dynamically switching between different child ViewModels.
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private BaseViewModel _currentViewModel = default!;
        private readonly IStatisticsService _statisticsService;

        public LocalizationProvider Strings { get; }

        /// <summary>
        /// Gets or sets the currently active ViewModel. 
        /// Modifying this property triggers a UI update via DataBinding, effectively navigating the user to a new view 
        /// without the overhead of opening and closing physical windows.
        /// </summary>
        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes the main application shell and sets up global dependencies.
        /// </summary>
        public MainViewModel(IStatisticsService statisticsService, LocalizationProvider localizationProvider)
        {
            _statisticsService = statisticsService;
            Strings = localizationProvider;

            // We force the application to start on the language selection screen.
            // This ensures all subsequent views have a resolved localization context before rendering.
            CurrentViewModel = new LanguageSelectionViewModel(this, Strings);
        }

        /// <summary>
        /// Transitions the active view to the main menu.
        /// </summary>
        public void NavigateToMenu()
        {
            // By passing 'this' (MainViewModel) to child ViewModels, we enable them to request navigation changes.
            // This is a lightweight alternative to using an EventAggregator or complex routing service.
            CurrentViewModel = new MenuViewModel(this, Strings);
        }

        public void NavigateToHighscores()
        {
            CurrentViewModel = new HighscoreViewModel(this, _statisticsService, Strings);
        }

        public void NavigateToAddWord()
        {
            CurrentViewModel = new AddWordViewModel(this, Strings);
        }

        public void NavigateToHelp()
        {
            CurrentViewModel = new HelpViewModel(this, Strings);
        }

        public void NavigateToGameSettings(GameMode mode)
        {
            CurrentViewModel = new GameSettingsViewModel(this, mode, Strings);
        }

        /// <summary>
        /// Instantiates a tournament session (two-player mode) utilizing the configured word source.
        /// </summary>
        public void NavigateToTournament(GameSettings settings)
        {
            IAsyncWordProvider provider = CreateProvider(settings);
            CurrentViewModel = new TournamentViewModel(this, provider, settings, Strings);
        }

        /// <summary>
        /// Instantiates a standard single-player game session utilizing the configured word source.
        /// </summary>
        public void NavigateToGame(GameSettings settings)
        {
            IAsyncWordProvider provider = CreateProvider(settings);
            CurrentViewModel = new GameViewModel(this, provider, _statisticsService, settings.PlayerName, Strings);
        }

        /// <summary>
        /// Factory method responsible for resolving the appropriate strategy for word generation.
        /// </summary>
        /// <param name="settings">The user-defined game parameters dictating the word source.</param>
        /// <returns>An implementation of IAsyncWordProvider.</returns>
        private IAsyncWordProvider CreateProvider(GameSettings settings)
        {
            // Utilizing the Strategy Pattern here completely decouples the core game logic from data retrieval mechanisms.
            // The GameViewModel does not need to know if words are fetched via HTTP, SQLite, or an in-memory list.
            switch (settings.Source)
            {
                case WordSource.Api:
                    return new ApiWordProvider(settings.Difficulty);
                case WordSource.Local:
                    return new WordProvider(settings.Difficulty);
                case WordSource.CustomSwedish:
                    return new CustomWordProvider(settings.Difficulty, WordLanguage.Swedish);
                case WordSource.CustomEnglish:
                    return new CustomWordProvider(settings.Difficulty, WordLanguage.English);
                default:
                    throw new System.NotImplementedException();
            }
        }
    }
}

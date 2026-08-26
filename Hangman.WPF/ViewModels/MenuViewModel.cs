using Hangman.Core.Models;
using System.Collections.Generic;
using System.Windows.Input;
using Hangman.Core.Localizations;

namespace Hangman.WPF.ViewModels
{
    /// <summary>
    /// Represents the main menu view in the MVVM architecture.
    /// Acts as the central navigation hub for the user, delegating the actual state changes 
    /// back to the root orchestrator (MainViewModel) to keep the navigation hierarchy flat and predictable.
    /// </summary>
    public class MenuViewModel : BaseViewModel
    {
        // Retained as a direct dependency to allow this child view to dictate root view changes.
        // This avoids the overhead of a complex EventAggregator or messaging system for a simple application flow.
        private readonly MainViewModel _mainViewModel;

        public LocalizationProvider Strings { get; }

        public ICommand StartSinglePlayerCommand { get; }
        public ICommand NavigateToHighscoresCommand { get; }
        public ICommand NavigateToAddWordCommand { get; }
        public ICommand StartTournamentCommand { get; }
        public ICommand NavigateToHelpCommand { get; }

        /// <summary>
        /// Initializes the menu commands and establishes localization.
        /// </summary>
        /// <param name="mainViewModel">The root ViewModel responsible for handling view transitions.</param>
        /// <param name="strings">The localization provider for resolving UI text dynamically.</param>
        public MenuViewModel(MainViewModel mainViewModel, LocalizationProvider strings)
        {
            _mainViewModel = mainViewModel;
            Strings = strings;

            // We explicitly pass the GameMode (SinglePlayer vs Tournament) when navigating to the settings view.
            // This architectural choice allows us to reuse a single GameSettingsViewModel for both game modes, 
            // adhering to the DRY (Don't Repeat Yourself) principle while preserving the user's selected context.
            StartSinglePlayerCommand = new RelayCommand(_ => _mainViewModel.NavigateToGameSettings(GameMode.SinglePlayer));
            StartTournamentCommand = new RelayCommand(_ => _mainViewModel.NavigateToGameSettings(GameMode.Tournament));

            NavigateToHighscoresCommand = new RelayCommand(_ => _mainViewModel.NavigateToHighscores());
            NavigateToAddWordCommand = new RelayCommand(_ => _mainViewModel.NavigateToAddWord());
            NavigateToHelpCommand = new RelayCommand(_ => _mainViewModel.NavigateToHelp());
        }
    }
}

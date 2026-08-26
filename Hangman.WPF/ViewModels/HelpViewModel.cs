using System.Windows.Input;
using Hangman.Core.Localizations;

namespace Hangman.WPF.ViewModels
{
    /// <summary>
    /// Provides the state and navigation command required by the help view.
    /// </summary>
    public class HelpViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        public ICommand BackToMenuCommand { get; }

        public LocalizationProvider Strings { get; }

        /// <summary>
        /// Initializes the help view model with the navigation and localization dependencies
        /// required to keep the view independent of application flow and text resources.
        /// </summary>
        public HelpViewModel(MainViewModel mainViewModel, LocalizationProvider strings)
        {
            _mainViewModel = mainViewModel;
            Strings = strings;
            BackToMenuCommand = new RelayCommand(_ => _mainViewModel.NavigateToMenu());
        }
    }
}

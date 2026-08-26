using Hangman.Core.Localizations;
using System.Windows.Input;

namespace Hangman.WPF.ViewModels
{
    /// <summary>
    /// Coordinates UI language selection and applies the selected localization
    /// strategy before returning the user to the main menu.
    /// </summary>
    public class LanguageSelectionViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private readonly LocalizationProvider _strings;

        public LocalizationProvider Strings { get; }

        public ICommand SelectSwedishCommand { get; }
        public ICommand SelectEnglishCommand { get; }

        /// <summary>
        /// Initializes the language selection ViewModel with the navigation and
        /// localization dependencies required by the language selection view.
        /// </summary>
        public LanguageSelectionViewModel(MainViewModel mainViewModel, LocalizationProvider strings)
        {
            _mainViewModel = mainViewModel;
            _strings = strings;
            Strings = strings;

            SelectSwedishCommand = new RelayCommand(SelectSwedish);
            SelectEnglishCommand = new RelayCommand(SelectEnglish);
        }

        private void SelectSwedish(object? _)
        {
            _strings.SetStrategy(new SwedishUiStrings());
            _mainViewModel.NavigateToMenu();
        }

        private void SelectEnglish(object? _)
        {
            _strings.SetStrategy(new EnglishUiStrings());
            _mainViewModel.NavigateToMenu();
        }
    }
}

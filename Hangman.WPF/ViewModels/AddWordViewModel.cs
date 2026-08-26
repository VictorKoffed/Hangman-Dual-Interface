using Hangman.Core.Exceptions;
using Hangman.Core.Models;
using Hangman.Core.Providers.Local;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Hangman.Core.Localizations;

namespace Hangman.WPF.ViewModels
{
    /// <summary>
    /// Provides the UI logic for adding custom words and communicating the result
    /// back to the user through localized feedback messages.
    /// </summary>
    public class AddWordViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private readonly LocalizationProvider _strings;

        public LocalizationProvider Strings { get; }

        private string _newWord = "";
        public string NewWord { get => _newWord; set { _newWord = value; OnPropertyChanged(); } }

        public IEnumerable<WordLanguage> Languages => System.Enum.GetValues<WordLanguage>();

        private WordLanguage _selectedLanguage = WordLanguage.Swedish;
        public WordLanguage SelectedLanguage { get => _selectedLanguage; set { _selectedLanguage = value; OnPropertyChanged(); } }

        private string _feedbackMessage = "";
        public string FeedbackMessage { get => _feedbackMessage; set { _feedbackMessage = value; OnPropertyChanged(); } }

        public ICommand AddWordCommand { get; }
        public ICommand BackToMenuCommand { get; }

        /// <summary>
        /// Initializes the ViewModel and connects the commands to the navigation
        /// and custom-word workflow used by the view.
        /// </summary>
        public AddWordViewModel(MainViewModel mainViewModel, LocalizationProvider strings)
        {
            _mainViewModel = mainViewModel;
            _strings = strings;
            Strings = strings;

            AddWordCommand = new RelayCommand(async _ => await AddWord(), _ => CanAddWord());
            BackToMenuCommand = new RelayCommand(_ => _mainViewModel.NavigateToMenu());
        }

        /// <summary>
        /// Determines whether the current input represents a valid custom word.
        /// Restricting input to letters keeps custom words compatible with the game's
        /// word-based guessing rules and prevents invalid entries from reaching persistence.
        /// </summary>
        private bool CanAddWord()
        {
            return !string.IsNullOrWhiteSpace(NewWord) && NewWord.All(char.IsLetter);
        }

        /// <summary>
        /// Validates, classifies, and persists the entered custom word while keeping
        /// all user-facing feedback within the localization layer.
        /// </summary>
        private async Task AddWord()
        {
            string word = NewWord.ToUpperInvariant();
            var difficulty = GetDifficultyByLength(word);

            try
            {
                var provider = new CustomWordProvider(difficulty, SelectedLanguage);
                await provider.AddWordAsync(word, difficulty, SelectedLanguage);

                FeedbackMessage = _strings.AddWordSuccess(word, difficulty, SelectedLanguage);
                NewWord = "";
            }
            catch (WordAlreadyExistsException ex)
            {
                FeedbackMessage = _strings.ErrorWordAlreadyExists(ex.Word, ex.Difficulty, ex.Language);
            }
            catch (System.Exception ex)
            {
                FeedbackMessage = _strings.CommonErrorDatabaseError(ex.Message);
            }
        }

        /// <summary>
        /// Maps the word length to the same difficulty ranges used by the game's
        /// word providers, ensuring custom words follow the application's established
        /// difficulty rules.
        /// </summary>
        private WordDifficulty GetDifficultyByLength(string word)
        {
            if (word.Length <= 4) return WordDifficulty.Easy;
            if (word.Length <= 7) return WordDifficulty.Medium;
            return WordDifficulty.Hard;
        }
    }
}

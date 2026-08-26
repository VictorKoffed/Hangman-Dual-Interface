using Hangman.Core.Models;
using Hangman.Core.Providers.Interface;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Hangman.Core.Localizations;
using System.Linq;
using System.Windows;

namespace Hangman.WPF.ViewModels
{
    /// <summary>
    /// Provides the state and commands required to display highscore data
    /// while keeping data access and navigation concerns outside the view.
    /// </summary>
    public class HighscoreViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IStatisticsService _statisticsService;
        private readonly LocalizationProvider _strings;

        public LocalizationProvider Strings { get; }

        public ObservableCollection<HighscoreEntry> Highscores { get; } = new ObservableCollection<HighscoreEntry>();

        private string _statusMessage = "";
        public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(); } }

        public ICommand BackToMenuCommand { get; }

        /// <summary>
        /// Initializes the ViewModel and starts loading highscores so the view
        /// can remain responsive while database access is performed asynchronously.
        /// </summary>
        public HighscoreViewModel(MainViewModel mainViewModel, IStatisticsService statisticsService, LocalizationProvider strings)
        {
            _mainViewModel = mainViewModel;
            _statisticsService = statisticsService;
            _strings = strings;
            Strings = strings;

            BackToMenuCommand = new RelayCommand(_ => _mainViewModel.NavigateToMenu());

            StatusMessage = _strings.HighscoreStatusLoading;
            Task.Run(LoadHighscores);
        }

        /// <summary>
        /// Loads the global highscores and updates the observable collection on
        /// the UI thread so the view can react immediately to the resulting state.
        /// </summary>
        private async Task LoadHighscores()
        {
            try
            {
                var scores = await _statisticsService.GetGlobalTopScoresAsync(10);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Highscores.Clear();
                    if (scores.Any())
                    {
                        foreach (var score in scores.OrderBy(s => s.Difficulty).ThenByDescending(s => s.ConsecutiveWins))
                        {
                            Highscores.Add(score);
                        }
                        StatusMessage = string.Empty;
                    }
                    else
                    {
                        StatusMessage = _strings.HighscoreStatusNoneFoundWpf;
                    }
                });
            }
            catch (System.Exception ex)
            {
                StatusMessage = _strings.HighscoreStatusError(ex.Message);
            }
        }
    }
}

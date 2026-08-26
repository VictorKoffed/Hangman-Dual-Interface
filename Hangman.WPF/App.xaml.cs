using System.Windows;
using Hangman.Core.Providers.Db;
using Hangman.Core.Providers.Interface;
using Hangman.WPF.ViewModels;
using Hangman.WPF.Views;
using Hangman.Core.Localizations;

namespace Hangman.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml.
    /// Acts as the Composition Root of the WPF application. This class is responsible for 
    /// manual Dependency Injection (DI) and bootstrapping the MVVM architecture before the UI is rendered.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Overrides the standard application startup sequence.
        /// By overriding this method and omitting the StartupUri in App.xaml, we gain full control 
        /// over the instantiation of the MainWindow and its required dependencies, enabling constructor injection.
        /// </summary>
        /// <param name="e">Contains the arguments and data related to the startup event.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize the persistent storage service here to ensure a single, application-wide instance 
            // (effectively acting as a Singleton) is shared across all ViewModels. This prevents multiple 
            // concurrent database connections or conflicting DbContexts.
            IStatisticsService statisticsService = new SqliteHangmanService();

            // Pre-load available localization strategies. 
            IUiStrings swedishStrings = new SwedishUiStrings();
            IUiStrings englishStrings = new EnglishUiStrings();

            // The LocalizationProvider acts as an observable wrapper around the raw string interfaces. 
            // This is a critical architectural choice: it allows dynamic language switching at runtime. 
            // Because the provider implements INotifyPropertyChanged, changing the underlying language 
            // will instantly trigger a UI refresh for all bound text elements without requiring an app restart.
            var localizationProvider = new LocalizationProvider(englishStrings);

            // Inject global dependencies into the root orchestrator (MainViewModel).
            // This orchestrator will subsequently pass these references down to child ViewModels as needed, 
            // keeping the dependency graph flat and explicit.
            var mainViewModel = new MainViewModel(statisticsService, localizationProvider);

            var window = new MainWindow
            {
                // Bind the root ViewModel to the root Window. 
                // This establishes the foundational DataContext for the entire visual tree, 
                // enabling data-driven navigation by dynamically swapping child ViewModels.
                DataContext = mainViewModel
            };

            window.Show();
        }
    }
}

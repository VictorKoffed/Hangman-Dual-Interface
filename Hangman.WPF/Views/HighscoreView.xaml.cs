using System.Windows.Controls;

namespace Hangman.WPF.Views
{
    /// <summary>
    /// Interaction logic for the HighscoreView UserControl.
    /// In strict adherence to the MVVM (Model-View-ViewModel) architectural pattern, 
    /// this code-behind file is kept intentionally devoid of any business logic, data retrieval, or state management.
    /// The responsibility of fetching and formatting the leaderboard data is completely delegated to the HighscoreViewModel, 
    /// communicating with this view solely through XAML DataBinding. This ensures maximum testability and a clean separation of concerns.
    /// </summary>
    public partial class HighscoreView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HighscoreView"/> class.
        /// </summary>
        public HighscoreView()
        {
            // Parses the corresponding XAML file and instantiates the UI component tree.
            // Note: Direct UI manipulations, database calls, or event subscriptions should be strictly avoided here 
            // to maintain the integrity of the MVVM architecture.
            InitializeComponent();
        }
    }
}

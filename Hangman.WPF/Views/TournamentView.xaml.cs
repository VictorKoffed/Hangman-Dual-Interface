using System.Windows.Controls;

namespace Hangman.WPF.Views
{
    /// <summary>
    /// Interaction logic for the TournamentView UserControl.
    /// In strict adherence to the MVVM (Model-View-ViewModel) architectural pattern, 
    /// this code-behind file is kept intentionally devoid of any business logic, state management, or event handlers.
    /// All tournament coordination, player turns, and UI state transitions are handled exclusively by the 
    /// TournamentViewModel via XAML DataBinding and Commands. This ensures maximum testability and maintains 
    /// a clear separation of concerns.
    /// </summary>
    public partial class TournamentView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentView"/> class.
        /// </summary>
        public TournamentView()
        {
            // Parses the corresponding XAML file and instantiates the UI component tree.
            // Note: Direct UI manipulations, logic evaluations, or event subscriptions must be strictly avoided here 
            // to preserve the integrity of the MVVM architecture.
            InitializeComponent();
        }
    }
}

using System.Windows.Controls;

namespace Hangman.WPF.Views
{
    /// <summary>
    /// Interaction logic for the GameView UserControl.
    /// In strict adherence to the MVVM (Model-View-ViewModel) architectural pattern, 
    /// this code-behind file is kept intentionally devoid of any business logic, event handlers, or state management.
    /// All game interactions, timer updates, and UI state changes are handled exclusively by the associated GameViewModel 
    /// via XAML DataBinding and Commands. This separation of concerns ensures maximum testability and maintainability of the core game loop.
    /// </summary>
    public partial class GameView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameView"/> class.
        /// </summary>
        public GameView()
        {
            // Parses the corresponding XAML file and instantiates the UI component tree.
            // Note: Direct UI manipulations or event subscriptions should be avoided here to maintain MVVM integrity.
            InitializeComponent();
        }
    }
}

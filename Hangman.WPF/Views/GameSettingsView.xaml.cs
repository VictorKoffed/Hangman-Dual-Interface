using System.Windows.Controls;

namespace Hangman.WPF.Views
{
    /// <summary>
    /// Interaction logic for the GameSettingsView UserControl.
    /// In accordance with the MVVM (Model-View-ViewModel) architectural pattern, 
    /// this code-behind file is kept strictly empty of business logic. 
    /// All state management, game configuration, and commands are handled by the 
    /// associated GameSettingsViewModel via XAML DataBinding. This separation 
    /// ensures the UI remains decoupled, maintainable, and highly testable.
    /// </summary>
    public partial class GameSettingsView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameSettingsView"/> class.
        /// </summary>
        public GameSettingsView()
        {
            // Parses the corresponding XAML file and instantiates the UI component tree.
            // Note: Direct UI manipulations or event subscriptions should be avoided here to maintain MVVM integrity.
            InitializeComponent();
        }
    }
}

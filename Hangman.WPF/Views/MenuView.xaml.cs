using System.Windows.Controls;

namespace Hangman.WPF.Views
{
    /// <summary>
    /// Interaction logic for the MenuView UserControl.
    /// In strict adherence to the MVVM (Model-View-ViewModel) architectural pattern, 
    /// this code-behind file is kept intentionally devoid of any business logic, routing, or state management.
    /// All user interactions and navigation flows triggered from the main menu are handled exclusively by the 
    /// MenuViewModel via XAML DataBinding and ICommand implementations. This clean separation ensures 
    /// the presentation layer remains decoupled and easily testable.
    /// </summary>
    public partial class MenuView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuView"/> class.
        /// </summary>
        public MenuView()
        {
            // Parses the corresponding XAML file and instantiates the UI component tree.
            // Note: Direct UI manipulations, click event subscriptions, or navigation logic should be strictly avoided here 
            // to maintain the integrity of the application's MVVM architecture.
            InitializeComponent();
        }
    }
}

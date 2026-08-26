// File: Hangman.WPF/Views/LanguageSelectionView.xaml.cs

using System.Windows.Controls;

namespace Hangman.WPF.Views
{
    /// <summary>
    /// Interaction logic for the LanguageSelectionView UserControl.
    /// In strict adherence to the MVVM (Model-View-ViewModel) architectural pattern, 
    /// this code-behind file is kept intentionally devoid of any business logic, state management, or event handlers.
    /// All language selection commands and UI state updates are handled exclusively by the associated LanguageSelectionViewModel 
    /// via XAML DataBinding. This separation of concerns ensures maximum testability and maintainability.
    /// </summary>
    public partial class LanguageSelectionView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageSelectionView"/> class.
        /// </summary>
        public LanguageSelectionView()
        {
            // Parses the corresponding XAML file and instantiates the UI component tree.
            // Note: Direct UI manipulations or event subscriptions should be strictly avoided here 
            // to maintain the integrity of the MVVM architecture.
            InitializeComponent();
        }
    }
}

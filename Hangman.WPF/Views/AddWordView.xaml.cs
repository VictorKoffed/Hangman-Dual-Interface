using System.Windows.Controls;

namespace Hangman.WPF.Views
{
    /// <summary>
    /// Interaction logic for the AddWordView UserControl.
    /// In strict adherence to the MVVM (Model-View-ViewModel) architectural pattern, 
    /// this code-behind file is kept intentionally devoid of any business logic, event handlers, or state management.
    /// All interactions, validations, and data persistence operations are handled exclusively by the associated AddWordViewModel 
    /// via XAML DataBinding and Commands. This separation of concerns ensures maximum testability and maintainability.
    /// </summary>
    public partial class AddWordView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddWordView"/> class.
        /// </summary>
        public AddWordView()
        {
            // Parses the corresponding XAML file and instantiates the UI component tree.
            // Note: Resist the temptation to add direct UI manipulations or click events here.
            InitializeComponent();
        }
    }
}

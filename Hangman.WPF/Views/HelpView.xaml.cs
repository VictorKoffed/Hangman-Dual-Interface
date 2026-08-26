using System.Windows.Controls;

namespace Hangman.WPF.Views
{
    /// <summary>
    /// Interaction logic for the HelpView UserControl.
    /// Adhering strictly to the MVVM (Model-View-ViewModel) architectural pattern, 
    /// this code-behind file is kept intentionally devoid of any business logic, state management, or event handlers.
    /// All textual content, data bindings, and navigation commands are routed through the DataContext (HelpViewModel), 
    /// ensuring the view remains purely a declarative presentation layer that is highly decoupled and maintainable.
    /// </summary>
    public partial class HelpView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HelpView"/> class.
        /// </summary>
        public HelpView()
        {
            // Parses the associated XAML file to instantiate the UI component tree.
            // Note: Avoid placing any direct UI manipulation or logic here to preserve the strict separation of concerns required by MVVM.
            InitializeComponent();
        }
    }
}

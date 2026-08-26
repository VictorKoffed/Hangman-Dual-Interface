using System.Windows;

namespace Hangman.WPF.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// Acts as the root application shell and viewport. In strict adherence to the MVVM architectural pattern, 
    /// this code-behind file is kept completely devoid of business logic, state management, or UI event handlers.
    /// Navigation and content rendering are controlled externally by dynamically swapping the DataContext (MainViewModel).
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            // Parses the corresponding XAML file and instantiates the UI component tree.
            InitializeComponent();
            
            // All business and routing logic has been deliberately moved to MainViewModel and App.xaml.cs.
            // This ensures the view layer remains purely declarative, maximizing testability and maintainability 
            // by preventing tight coupling between the UI elements and the underlying application state.
        }
    }
}

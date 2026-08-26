/*
 * FILE COMMENT: This class provides the foundation for the MVVM pattern by implementing
 * INotifyPropertyChanged. This standard boilerplate for WPF/MVVM was developed
 * with assistance from a large language model (AI).
 */

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Hangman.WPF.ViewModels
{
    /// <summary>
    /// Provides the shared property-change notification infrastructure required
    /// by ViewModels so that WPF bindings can stay synchronized with application state.
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Notifies the WPF binding system that a property value has changed.
        /// The caller member name is captured automatically to reduce the risk of
        /// mismatched property names when ViewModels update their state.
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

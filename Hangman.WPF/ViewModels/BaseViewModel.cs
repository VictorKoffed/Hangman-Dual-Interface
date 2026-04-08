/*
 * FILKOMMENTAR: Denna klass utgör grunden för MVVM-mönstret genom att implementera
 * INotifyPropertyChanged. Denna standardboilerpate för WPF/MVVM har utvecklats
 * med assistans från en stor språkmodell (AI).
 */

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Hangman.WPF.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
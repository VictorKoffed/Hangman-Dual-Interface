/*
 * FILE COMMENT: This class provides the standard implementation of the
 * ICommand interface (RelayCommand/DelegateCommand) to bind UI events
 * to ViewModels. This MVVM boilerplate was developed with assistance from
 * a large language model (AI).
 */

using System;
using System.Windows.Input;

namespace Hangman.WPF.ViewModels
{
    /// <summary>
    /// A standard implementation of ICommand for MVVM.
    /// This acts as a bridge between the View (XAML) and the ViewModel, allowing UI controls (like Buttons) 
    /// to trigger actions in the ViewModel without tight coupling or code-behind event handlers.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic. Can be null if the command is always executable.</param>
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            // We enforce a strict fail-fast policy here. A command without an execution action is fundamentally invalid 
            // and would cause silent failures in the UI if allowed to instantiate.
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            // By delegating this to the WPF CommandManager, we ensure that the UI automatically re-evaluates 
            // the execution state of bound controls (e.g., enabling/disabling buttons) during application idle time 
            // or when keyboard/mouse focus changes occur.
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. May be null.</param>
        /// <returns>True if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object? parameter) =>
            // If no validation delegate was provided during initialization, we assume the command 
            // is perpetually valid and available for execution.
            _canExecute == null || _canExecute(parameter);

        /// <summary>
        /// Invokes the underlying delegate associated with this command.
        /// </summary>
        /// <param name="parameter">Data used by the command. May be null.</param>
        public void Execute(object? parameter) =>
            _execute(parameter);

        /// <summary>
        /// Call this method to manually force the UI to re-evaluate the CanExecute state.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            // This is crucial for performance optimization. It allows ViewModels to forcefully invalidate 
            // the command state exactly when backing data changes, rather than waiting for WPF's global 
            // InputManager to passively detect a potential state change.
            CommandManager.InvalidateRequerySuggested();
        }
    }
}

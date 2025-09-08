using System;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels
{
    public class RelayCommand(Action execute, Func<bool> canExecute = null) : ICommand
    {
        private readonly Action _execute = execute;
        private readonly Func<bool> _canExecute = canExecute;

        public bool CanExecute(object parameter) => this._canExecute();

        public void Execute(object parameter) => this._execute();

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        public static void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public class RelayCommand<T>(Action<T> execute, Func<T, bool> canExecute = null) : ICommand
    {
        private readonly Action<T> _execute = execute;
        private readonly Func<T, bool> _canExecute = canExecute;

        public bool CanExecute(object parameter) => this._canExecute?.Invoke((T)parameter) ?? true;

        public void Execute(object parameter) => this._execute((T)parameter);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}

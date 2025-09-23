using System;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// コマンド
    /// </summary>
    /// <param name="executed">コマンド実行デリゲート</param>
    /// <param name="canExecute">コマンド実行可否デリゲート</param>
    public class RelayCommand(Action executed, Func<bool> canExecute = null) : ICommand
    {
        private readonly Action _execute = executed;
        private readonly Func<bool> _canExecute = canExecute;

        public bool CanExecute(object parameter) => this._canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => this._execute?.Invoke();

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

    /// <summary>
    /// パラメータ付コマンド
    /// </summary>
    /// <typeparam name="T">パラメータの型</typeparam>
    /// <param name="executed">コマンド実行デリゲート</param>
    /// <param name="canExecute">コマンド実行可否デリゲート</param>
    public class RelayCommand<T>(Action<T> executed, Func<T, bool> canExecute = null) : ICommand
    {
        private readonly Action<T> _execute = executed;
        private readonly Func<T, bool> _canExecute = canExecute;

        public bool CanExecute(object parameter) => this._canExecute?.Invoke((T)parameter) ?? true;

        public void Execute(object parameter) => this._execute?.Invoke((T)parameter);

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

using HouseholdAccountBook.Adapters.Logger;
using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// コマンド
    /// </summary>
    /// <param name="executed">コマンド実行デリゲート</param>
    /// <param name="canExecute">コマンド実行可否デリゲート</param>
    /// <param name="fileName">出力元ファイル名</param>
    /// <param name="methodName">出力元関数名</param>
    public class RelayCommand(Action executed, Func<bool> canExecute = null, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = "") : ICommand
    {
        private readonly Action _execute = executed;
        private readonly Func<bool> _canExecute = canExecute;
        private readonly string _fileName = fileName;
        private readonly string _methodName = methodName;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) => this._canExecute?.Invoke() ?? true;

        public void Execute(object parameter)
        {
            using FuncLog funcLog = new(args: parameter, fileName: this._fileName, methodName: this._methodName);
            this._execute?.Invoke();
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
    public class RelayCommand<T>(Action<T> executed, Func<T, bool> canExecute = null, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = "") : ICommand
    {
        private readonly Action<T> _execute = executed;
        private readonly Func<T, bool> _canExecute = canExecute;
        private readonly string _fileName = fileName;
        private readonly string _methodName = methodName;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) => this._canExecute?.Invoke((T)parameter) ?? true;

        public void Execute(object parameter)
        {
            using FuncLog funcLog = new(args: parameter, fileName: this._fileName, methodName: this._methodName);
            this._execute?.Invoke((T)parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}

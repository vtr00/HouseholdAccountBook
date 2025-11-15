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
        private readonly Action mExecute = executed;
        private readonly Func<bool> mCanExecute = canExecute;
        private readonly string mFileName = fileName;
        private readonly string mEthodName = methodName;

        public event EventHandler CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) => this.mCanExecute?.Invoke() ?? true;

        public void Execute(object parameter)
        {
            using FuncLog funcLog = new(args: parameter, fileName: this.mFileName, methodName: this.mEthodName);
            this.mExecute?.Invoke();
        }

        public static void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }

    /// <summary>
    /// パラメータ付コマンド
    /// </summary>
    /// <typeparam name="T">パラメータの型</typeparam>
    /// <param name="executed">コマンド実行デリゲート</param>
    /// <param name="canExecute">コマンド実行可否デリゲート</param>
    public class RelayCommand<T>(Action<T> executed, Func<T, bool> canExecute = null, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = "") : ICommand
    {
        private readonly Action<T> mExecute = executed;
        private readonly Func<T, bool> mCanExecute = canExecute;
        private readonly string mFileName = fileName;
        private readonly string mEthodName = methodName;

        public event EventHandler CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) => this.mCanExecute?.Invoke((T)parameter) ?? true;

        public void Execute(object parameter)
        {
            using FuncLog funcLog = new(args: parameter, fileName: this.mFileName, methodName: this.mEthodName);
            this.mExecute?.Invoke((T)parameter);
        }

        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }
}

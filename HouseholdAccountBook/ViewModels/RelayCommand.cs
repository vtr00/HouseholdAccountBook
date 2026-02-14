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
    public class RelayCommand(Action<FuncLog> executed, Func<bool> canExecute = null, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = "") : ICommand
    {
        #region フィールド
        private readonly Action<FuncLog> mExecute = executed;
        private readonly Func<bool> mCanExecute = canExecute;
        private readonly string mFileName = fileName;
        private readonly string mMethodName = methodName;
        #endregion

        #region イベント
        public event EventHandler CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        #endregion

        /// <summary>
        /// <see cref="RelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executed">コマンド実行デリゲート</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        public RelayCommand(Action executed, Func<bool> canExecute = null, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = "")
            : this(funcLog => executed(), canExecute, fileName, methodName)
        { }
        /// <summary>
        /// <see cref="RelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        public RelayCommand(Func<bool> canExecute = null, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = "")
            : this(static funcLog => { }, canExecute, fileName, methodName)
        { }

        public bool CanExecute(object parameter) => this.mCanExecute?.Invoke() ?? true;

        public void Execute(object parameter)
        {
            using FuncLog funcLog = new(args: parameter, fileName: this.mFileName, methodName: this.mMethodName);
            this.mExecute?.Invoke(funcLog);
        }

        public static void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }

    /// <summary>
    /// パラメータ付コマンド
    /// </summary>
    /// <typeparam name="T">パラメータの型</typeparam>
    /// <param name="executed">コマンド実行デリゲート</param>
    /// <param name="canExecute">コマンド実行可否デリゲート</param>
    public class RelayCommand<T>(Action<T, FuncLog> executed, Func<T, bool> canExecute = null, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = "") : ICommand
    {
        #region フィールド
        private readonly Action<T, FuncLog> mExecute = executed;
        private readonly Func<T, bool> mCanExecute = canExecute;
        private readonly string mFileName = fileName;
        private readonly string mEthodName = methodName;
        #endregion

        #region イベント
        public event EventHandler CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        #endregion

        /// <summary>
        /// <see cref="RelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executed">コマンド実行デリゲート</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        public RelayCommand(Action<T> executed, Func<T, bool> canExecute = null, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = "")
            : this((parameter, funcLog) => executed(parameter), canExecute, fileName, methodName)
        { }
        /// <summary>
        /// <see cref="RelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        public RelayCommand(Func<T, bool> canExecute, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = "")
            : this(static (parameter, funcLog) => { }, canExecute, fileName, methodName)
        { }

        public bool CanExecute(object parameter) => this.mCanExecute?.Invoke((T)parameter) ?? true;

        public void Execute(object parameter)
        {
            using FuncLog funcLog = new(args: parameter, fileName: this.mFileName, methodName: this.mEthodName);
            this.mExecute?.Invoke((T)parameter, funcLog);
        }

        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }
}

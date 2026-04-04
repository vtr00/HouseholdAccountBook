using HouseholdAccountBook.Infrastructure.Logger;
using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// [同期] コマンド
    /// </summary>
    /// <param name="execute">コマンド実行デリゲート</param>
    /// <param name="canExecute">コマンド実行可否デリゲート</param>
    /// <param name="fileName">出力元ファイル名</param>
    /// <param name="memberName">出力元関数名</param>
    public class RelayCommand(Action<FuncLog> execute, Func<bool> canExecute = null,
                              [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "") : ICommand
    {
        #region フィールド
        /// <summary>
        /// コマンド実行デリゲート
        /// </summary>
        private readonly Action<FuncLog> mExecute = execute;
        /// <summary>
        /// コマンド実行可否デリゲート
        /// </summary>
        private readonly Func<bool> mCanExecute = canExecute;
        /// <summary>
        /// 呼び出し元ファイル名
        /// </summary>
        private readonly string mFileName = fileName;
        /// <summary>
        /// 呼び出し元メンバー名
        /// </summary>
        private readonly string mMemberName = memberName;

        /// <summary>
        /// 実行中か
        /// </summary>
        private bool mIsExecuting = false;
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
        /// <param name="execute">コマンド実行デリゲート</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public RelayCommand(Action execute, Func<bool> canExecute = null,
                            [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : this(_ => execute(), canExecute, fileName, memberName)
        { }
        /// <summary>
        /// <see cref="RelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public RelayCommand(Func<bool> canExecute = null,
                            [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : this(static _ => { }, canExecute, fileName, memberName)
        { }

        public bool CanExecute(object parameter) => !this.mIsExecuting && (this.mCanExecute?.Invoke() ?? true);

        public void Execute(object parameter)
        {
            using FuncLog funcLog = new(args: parameter, fileName: this.mFileName, methodName: this.mMemberName);

            this.mIsExecuting = true;
            RaiseCanExecuteChanged();
            try {
                this.mExecute?.Invoke(funcLog);
            }
            finally {
                this.mIsExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public static void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }

    /// <summary>
    /// [同期] パラメータ付コマンド
    /// </summary>
    /// <typeparam name="T">パラメータの型</typeparam>
    /// <param name="execute">コマンド実行デリゲート</param>
    /// <param name="canExecute">コマンド実行可否デリゲート</param>
    public class RelayCommand<T>(Action<T, FuncLog> execute, Func<T, bool> canExecute = null, 
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "") : ICommand
    {
        #region フィールド
        /// <summary>
        /// コマンド実行デリゲート
        /// </summary>
        private readonly Action<T, FuncLog> mExecute = execute;
        /// <summary>
        /// コマンド実行可否デリゲート
        /// </summary>
        private readonly Func<T, bool> mCanExecute = canExecute;
        /// <summary>
        /// 呼び出し元ファイル名
        /// </summary>
        private readonly string mFileName = fileName;
        /// <summary>
        /// 呼び出し元メンバー名
        /// </summary>
        private readonly string mMemberName = memberName;

        /// <summary>
        /// 実行中か
        /// </summary>
        private bool mIsExecuting = false;
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
        /// <param name="execute">コマンド実行デリゲート</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null,
                            [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : this((parameter, _) => execute(parameter), canExecute, fileName, memberName)
        { }
        /// <summary>
        /// <see cref="RelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public RelayCommand(Func<T, bool> canExecute,
                            [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : this(static (parameter, _) => { }, canExecute, fileName, memberName)
        { }

        public bool CanExecute(object parameter) => !this.mIsExecuting && (parameter is not T p || (this.mCanExecute?.Invoke(p) ?? true));

        public void Execute(object parameter)
        {
            using FuncLog funcLog = new(args: parameter, fileName: this.mFileName, methodName: this.mMemberName);

            this.mIsExecuting = true;
            RaiseCanExecuteChanged();
            try {
                this.mExecute?.Invoke((T)parameter, funcLog);
            }
            finally {
                this.mIsExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public static void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }
}

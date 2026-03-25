using HouseholdAccountBook.Infrastructure.Logger;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// [非同期] コマンド
    /// </summary>
    /// <param name="executeAsync">コマンド非同期実行デリゲート</param>
    /// <param name="canExecute">コマンド実行可否デリゲート</param>
    /// <param name="allowConcurrent">多重実行可能か</param>
    /// <param name="fileName">出力元ファイル名</param>
    /// <param name="memberName">出力元関数名</param>
    public class AsyncRelayCommand(Func<FuncLog, CancellationToken, Task> executeAsync, Func<bool> canExecute, bool allowConcurrent = true, [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "") : ICommand, IDisposable
    {
        #region フィールド
        /// <summary>
        /// コマンド実行デリゲート
        /// </summary>
        private readonly Func<FuncLog, CancellationToken, Task> mExecuteAsync = executeAsync;
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
        /// コマンド実行中か
        /// </summary>
        private bool mIsExecuting = false;
        /// <summary>
        /// 多重実行可能か
        /// </summary>
        private readonly bool mAllowConcurrent = allowConcurrent;

        /// <summary>
        /// 多重読込処理防止トークン源
        /// </summary>
        private CancellationTokenSource mSelectionCts;
        #endregion

        #region イベント
        public event EventHandler CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        #endregion

        /// <summary>
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド実行デリゲート</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="allowConcurrent">多重実行可能か</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<Task> executeAsync, Func<bool> canExecute = null, bool allowConcurrent = true, [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : this((_1, _2) => executeAsync(), canExecute, allowConcurrent, fileName, memberName)
        { }
        /// <summary>
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド実行デリゲート</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="allowConcurrent">多重実行可能か</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<CancellationToken, Task> executeAsync, Func<bool> canExecute = null, bool allowConcurrent = true, [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : this((_, token) => executeAsync(token), canExecute, allowConcurrent, fileName, memberName)
        { }

        ~AsyncRelayCommand()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            this.mSelectionCts?.Cancel();
            this.mSelectionCts?.Dispose();
            GC.SuppressFinalize(this);
        }

        public bool CanExecute(object parameter) => (!this.mIsExecuting || this.mAllowConcurrent) && (this.mCanExecute?.Invoke() ?? true);

        public async void Execute(object parameter)
        {
            using FuncLog funcLog = new(args: parameter, fileName: this.mFileName, methodName: this.mMemberName);
            if (!this.CanExecute(parameter)) {
                Log.Debug($"{nameof(this.CanExecute)} is false");
                return;
            }

            this.mSelectionCts?.Cancel();
            this.mSelectionCts?.Dispose();
            this.mSelectionCts = new();
            CancellationToken token = this.mSelectionCts.Token;

            this.mIsExecuting = true;
            RaiseCanExecuteChanged();
            try {
                await this.mExecuteAsync?.Invoke(funcLog, token);
            }
            catch (OperationCanceledException) {
                Log.Debug($"{this.mMemberName} Canceled.");
            }
            finally {
                this.mIsExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public static void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }

    /// <summary>
    /// [非同期] パラメータ付コマンド
    /// </summary>
    /// <param name="executeAsync">コマンド非同期実行デリゲート</param>
    /// <param name="canExecute">コマンド実行可否デリゲート</param>
    /// <param name="allowConcurrent">多重実行可能か</param>
    /// <param name="fileName">出力元ファイル名</param>
    /// <param name="memberName">出力元関数名</param>
    public class AsyncRelayCommand<T>(Func<T, FuncLog, CancellationToken, Task> executeAsync, Func<T, bool> canExecute, bool allowConcurrent = true, [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "") : ICommand, IDisposable
    {
        #region フィールド
        /// <summary>
        /// コマンド実行デリゲート
        /// </summary>
        private readonly Func<T, FuncLog, CancellationToken, Task> mExecuteAsync = executeAsync;
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
        /// <summary>
        /// 多重実行可能か
        /// </summary>
        private readonly bool mAllowConcurrent = allowConcurrent;

        /// <summary>
        /// 多重読込処理防止トークン源
        /// </summary>
        private CancellationTokenSource mSelectionCts;
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
        /// <param name="executeAsync">コマンド非同期実行デリゲート</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="allowConcurrent">多重実行可能か</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<T, Task> executeAsync, Func<T, bool> canExecute = null, bool allowConcurrent = true, [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : this((parameter, _1, _2) => executeAsync(parameter), canExecute, allowConcurrent, fileName, memberName)
        { }
        /// <summary>
        /// <see cref="RelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド非同期実行デリゲート</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="allowConcurrent">多重実行可能か</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<T, CancellationToken, Task> executeAsync, Func<T, bool> canExecute = null, bool allowConcurrent = true, [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : this((parameter, _, token) => executeAsync(parameter, token), canExecute, allowConcurrent, fileName, memberName)
        { }

        ~AsyncRelayCommand()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            this.mSelectionCts?.Cancel();
            this.mSelectionCts?.Dispose();
            GC.SuppressFinalize(this);
        }

        public bool CanExecute(object parameter) => (!this.mIsExecuting || this.mAllowConcurrent) && (this.mCanExecute?.Invoke((T)parameter) ?? true);

        public async void Execute(object parameter)
        {
            using FuncLog funcLog = new(args: parameter, fileName: this.mFileName, methodName: this.mMemberName);

            this.mSelectionCts?.Cancel();
            this.mSelectionCts?.Dispose();
            this.mSelectionCts = new();
            CancellationToken token = this.mSelectionCts.Token;

            this.mIsExecuting = true;
            RaiseCanExecuteChanged();
            try {
                await this.mExecuteAsync?.Invoke((T)parameter, funcLog, token);
            }
            catch (OperationCanceledException) {
                Log.Debug($"{this.mMemberName} Canceled.");
            }
            finally {
                this.mIsExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public static void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }
}

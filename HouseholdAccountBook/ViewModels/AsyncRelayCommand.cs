using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Views.Extensions;
using HouseholdAccountBook.Views.Windows;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 実行モード
    /// </summary>
    public enum ExecutionMode
    {
        /// <summary>
        /// 多重実行可
        /// </summary>
        AllowConcurrent,
        /// <summary>
        /// 多重実行不可(コマンド実行中はコマンド無効)
        /// </summary>
        DisallowConcurrent,
        /// <summary>
        /// 進捗ウィンドウ表示
        /// </summary>
        ProgressWindow
    }

    /// <summary>
    /// [非同期] パラメータ付コマンド
    /// </summary>
    public class AsyncRelayCommand<T> : ICommand, IDisposable
    {
        #region フィールド
        /// <summary>
        /// コマンド実行デリゲート
        /// </summary>
        private readonly Func<T, FuncLog, CancellationToken, IProgress<int>, Task> mExecuteAsync;
        /// <summary>
        /// コマンド実行可否デリゲート
        /// </summary>
        private readonly Func<T, bool> mCanExecute;
        /// <summary>
        /// 実行モード(コマンド実行デリゲートがプログレス受付可能か)
        /// </summary>
        private readonly ExecutionMode mMode;
        /// <summary>
        /// コマンドがキャンセル可能か(コマンド実行デリゲートがトークン受付可能か)
        /// </summary>
        protected readonly bool mCanCancel = true;
        /// <summary>
        /// 呼び出し元ファイル名
        /// </summary>
        private readonly string mFileName;
        /// <summary>
        /// 呼び出し元メンバー名
        /// </summary>
        private readonly string mMemberName;

        /// <summary>
        /// 実行中か
        /// </summary>
        private bool mIsExecuting = false;

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
        /// <param name="executeAsync">コマンド非同期実行デリゲート(FuncLogあり、中断あり、進捗表示あり)</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="mode">実行モード</param>
        /// <param name="canCancel">キャンセル可能か</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        protected AsyncRelayCommand(Func<T, FuncLog, CancellationToken, IProgress<int>, Task> executeAsync, Func<T, bool> canExecute, ExecutionMode mode, bool canCancel,
                                    [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
        {
            this.mExecuteAsync = executeAsync;
            this.mCanExecute = canExecute;
            this.mMode = mode;
            this.mCanCancel = canCancel;
            this.mFileName = fileName;
            this.mMemberName = memberName;
        }

        /// <summary>
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド実行デリゲート(FuncLogあり、中断なし、進捗表示なし)</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="mode">実行モード</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<T, FuncLog, Task> executeAsync, Func<T, bool> canExecute = null, ExecutionMode mode = ExecutionMode.AllowConcurrent,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : this(async (param, funcLog, _, progress) => { progress?.Report(-1); await executeAsync(param, funcLog); }, canExecute, mode, false, fileName, memberName)
        { }
        /// <summary>
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド実行デリゲート(FuncLogあり、中断なし、進捗表示あり)</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<T, FuncLog, IProgress<int>, Task> executeAsync, Func<T, bool> canExecute = null,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : this(async (param, funcLog, _, progress) => await executeAsync(param, funcLog, progress), canExecute, ExecutionMode.ProgressWindow, false, fileName, memberName)
        { }
        /// <summary>
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド実行デリゲート(FuncLogあり、中断あり、進捗表示なし)</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="mode">実行モード</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<T, FuncLog, CancellationToken, Task> executeAsync, Func<T, bool> canExecute = null, ExecutionMode mode = ExecutionMode.AllowConcurrent,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : this(async (param, funcLog, token, progress) => { progress?.Report(-1); await executeAsync(param, funcLog, token); }, canExecute, mode, true, fileName, memberName)
        { }
        /// <summary>
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド実行デリゲート(FuncLogあり、中断あり、進捗表示あり)</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<T, FuncLog, CancellationToken, IProgress<int>, Task> executeAsync, Func<T, bool> canExecute = null,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : this(async (param, funcLog, token, progress) => await executeAsync(param, funcLog, token, progress), canExecute, ExecutionMode.ProgressWindow, true, fileName, memberName)
        { }

        /// <summary>
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド実行デリゲート(中断なし、進捗表示なし)</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="mode">実行モード</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<T, Task> executeAsync, Func<T, bool> canExecute = null, ExecutionMode mode = ExecutionMode.AllowConcurrent,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : this(async (param, _, _, progress) => { progress?.Report(-1); await executeAsync(param); }, canExecute, mode, false, fileName, memberName)
        { }
        /// <summary>
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド実行デリゲート(中断なし、進捗表示あり)</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<T, IProgress<int>, Task> executeAsync, Func<T, bool> canExecute = null,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : this(async (param, _, _, progress) => await executeAsync(param, progress), canExecute, ExecutionMode.ProgressWindow, false, fileName, memberName)
        { }
        /// <summary>
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド実行デリゲート(中断あり、進捗表示なし)</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="mode">実行モード</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<T, CancellationToken, Task> executeAsync, Func<T, bool> canExecute = null, ExecutionMode mode = ExecutionMode.AllowConcurrent,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : this(async (param, _, token, progress) => { progress?.Report(-1); await executeAsync(param, token); }, canExecute, mode, true, fileName, memberName)
        { }
        /// <summary>
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド実行デリゲート(中断あり、進捗表示あり)</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<T, CancellationToken, IProgress<int>, Task> executeAsync, Func<T, bool> canExecute = null,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : this(async (param, _, token, progress) => await executeAsync(param, token, progress), canExecute, ExecutionMode.ProgressWindow, true, fileName, memberName)
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

        public bool CanExecute(object parameter) => (!this.mIsExecuting || this.mMode == ExecutionMode.AllowConcurrent) &&
                                                    ((parameter is T p ? this.mCanExecute?.Invoke(p) : this.mCanExecute?.Invoke(default)) ?? true);

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

            // 実行開始通知
            this.mIsExecuting = true;
            RaiseCanExecuteChanged();

            CancellationTokenSource cts = this.mSelectionCts; // キャンセル通知用
            TaskCompletionSource tcs = new(); // タスク実行結果通知用
            bool isRendered = false; // 再入不可用
            async Task ExecWrapper(ProgressWindow progressWindow)
            {
                using FuncLog funcLog = new(fileName: this.mFileName, methodName: this.mMemberName);

                try {
                    if (this.mExecuteAsync != null) {
                        await this.mExecuteAsync.Invoke((T)parameter, funcLog, cts.Token, progressWindow?.Progress);
                    }
                    _ = tcs.TrySetResult();
                }
                catch (OperationCanceledException) {
                    _ = tcs.TrySetCanceled();
                }
                catch (Exception ex) {
                    _ = tcs.TrySetException(ex);
                }
                finally {
                    progressWindow?.CloseOnCode();
                }
            }

            if (this.mMode == ExecutionMode.ProgressWindow) {
                // 進捗ウィンドウをメインウィンドウの中央位置に表示する
                ProgressWindow window = new(this.mCanCancel ? cts : null) {
                    Name = "ProgressWindow",
                    Owner = Application.Current.MainWindow
                };
                window.MoveOwnersCenter();

                // Window表示後に処理開始
                window.ContentRendered += async (sender, e) => {
                    if (isRendered) { return; }
                    isRendered = true;

                    await ExecWrapper(window);
                };

                // モーダル表示(ここでUI操作はブロック)
                _ = window.ShowDialog();
            }

            // (進捗ウィンドウ非表示の場合)非同期処理を待機 または (進捗ウィンドウ表示の場合)例外を処理する
            try {
                await (this.mMode != ExecutionMode.ProgressWindow ? ExecWrapper(null) : tcs.Task);
            }
            catch (OperationCanceledException) {
                // ここに至るまでにキャンセルが処理されていないなら例外をrethrowする
                Log.Debug($"{this.mMemberName} Canceled.", this.mFileName, this.mMemberName);
                throw;
            }
            catch (Exception) {
                Log.Error("Unhandled Exception Occurred.", this.mFileName, this.mMemberName);
                throw;
            }
            finally {
                // 実行終了通知
                this.mIsExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public static void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }

    /// <summary>
    /// [非同期] コマンド
    /// </summary>
    public class AsyncRelayCommand : AsyncRelayCommand<object>
    {
        /// <summary>
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド実行デリゲート(中断なし、進捗表示なし)</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="mode">実行モード</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<FuncLog, Task> executeAsync, Func<bool> canExecute = null, ExecutionMode mode = ExecutionMode.AllowConcurrent,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : base(async (_, funcLog) => await executeAsync(funcLog), _ => canExecute?.Invoke() ?? true, mode, fileName, memberName)
        { }
        /// <summary>
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド実行デリゲート(中断なし、進捗表示あり)</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<FuncLog, IProgress<int>, Task> executeAsync, Func<bool> canExecute = null,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : base(async (_, funcLog, progress) => await executeAsync(funcLog, progress), _ => canExecute?.Invoke() ?? true, fileName, memberName)
        { }
        /// <summary>
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド実行デリゲート(中断あり、進捗表示なし)</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="mode">実行モード</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<FuncLog, CancellationToken, Task> executeAsync, Func<bool> canExecute = null, ExecutionMode mode = ExecutionMode.AllowConcurrent,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : base(async (_, funcLog, token) => await executeAsync(funcLog, token), _ => canExecute?.Invoke() ?? true, mode, fileName, memberName)
        { }
        /// <summary>
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド実行デリゲート(中断あり、進捗表示あり)</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<FuncLog, CancellationToken, IProgress<int>, Task> executeAsync, Func<bool> canExecute = null,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : base(async (_, funcLog, token, progress) => await executeAsync(funcLog, token, progress), _ => canExecute?.Invoke() ?? true, fileName, memberName)
        { }
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// <param name="executeAsync">コマンド実行デリゲート(中断なし、進捗表示なし)</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="mode">実行モード</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<Task> executeAsync, Func<bool> canExecute = null, ExecutionMode mode = ExecutionMode.AllowConcurrent,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : base(async _ => await executeAsync(), _ => canExecute?.Invoke() ?? true, mode, fileName, memberName)
        { }
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// <param name="executeAsync">コマンド実行デリゲート(中断なし、進捗表示あり)</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<IProgress<int>, Task> executeAsync, Func<bool> canExecute = null,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : base(async (_, progress) => await executeAsync(progress), _ => canExecute?.Invoke() ?? true, fileName, memberName)
        { }
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// <param name="executeAsync">コマンド実行デリゲート(中断あり、進捗表示なし)</param>
        /// <param name="mode">実行モード</param>
        public AsyncRelayCommand(Func<CancellationToken, Task> executeAsync, Func<bool> canExecute = null, ExecutionMode mode = ExecutionMode.AllowConcurrent,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : base(async (_, token) => await executeAsync(token), _ => canExecute?.Invoke() ?? true, mode, fileName, memberName)
        { }
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// <param name="executeAsync">コマンド実行デリゲート(中断あり、進捗表示あり)</param>
        public AsyncRelayCommand(Func<CancellationToken, IProgress<int>, Task> executeAsync, Func<bool> canExecute = null,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : base(async (_, token, progress) => await executeAsync(token, progress), _ => canExecute?.Invoke() ?? true, fileName, memberName)
        { }
    }
}

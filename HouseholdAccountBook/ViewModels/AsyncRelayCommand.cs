using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities.Args.RequestEventArgs;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.ViewModels.Abstract;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
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
        /// 多重実行不可
        /// </summary>
        /// <remarks>requestableがある場合は強制される</remarks>
        DisallowConcurrent
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
        /// 処理中状態サービス
        /// </summary>
        private readonly BusyService mBusyService;

        /// <summary>
        /// 実行モード(コマンド実行デリゲートがプログレス受付可能か)
        /// </summary>
        private readonly ExecutionMode mMode;
        /// <summary>
        /// コマンドがキャンセル可能か(コマンド実行デリゲートがトークン受付可能か)
        /// </summary>
        private readonly bool mCanCancel = true;
        /// <summary>
        /// プログレスダイアログ要求可能オブジェクト
        /// </summary>
        private readonly IProgressDialogRequestable mRequestable;

        /// <summary>
        /// 実行中か
        /// </summary>
        private bool mIsExecuting = false;
        /// <summary>
        /// 多重読込処理防止トークン源
        /// </summary>
        private CancellationTokenSource mSelectionCts;

        /// <summary>
        /// 呼び出し元ファイル名
        /// </summary>
        private readonly string mFileName;
        /// <summary>
        /// 呼び出し元メンバー名
        /// </summary>
        private readonly string mMemberName;
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
        /// <param name="requestable">進捗ダイアログ要求可能オブジェクト</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="busyService">処理中状態サービス</param>
        /// <param name="mode">実行モード</param>
        /// <param name="canCancel">キャンセル可能か</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        protected AsyncRelayCommand(Func<T, FuncLog, CancellationToken, IProgress<int>, Task> executeAsync, IProgressDialogRequestable requestable, Func<T, bool> canExecute,
                                    BusyService busyService, ExecutionMode mode, bool canCancel,
                                    [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
        {
            using FuncLog funcLog = new(level: Log.LogLevel.Trace, fileName: fileName, methodName: memberName);

            if (requestable != null && mode != ExecutionMode.DisallowConcurrent) { throw new ArgumentNullException(nameof(requestable)); }

            this.mExecuteAsync = executeAsync;
            this.mCanExecute = canExecute;
            this.mBusyService = busyService;
            this.mMode = mode;
            this.mCanCancel = canCancel;
            this.mRequestable = requestable;
            this.mFileName = fileName;
            this.mMemberName = memberName;
        }

        /// <summary>
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド非同期実行デリゲート(FuncLogあり、中断なし、進捗表示なし)</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="busyService">処理中状態サービス</param>
        /// <param name="mode">実行モード</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<T, FuncLog, Task> executeAsync, Func<T, bool> canExecute = null,
                                 BusyService busyService = null, ExecutionMode mode = ExecutionMode.AllowConcurrent,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : this(async (param, funcLog, _, progress) => { progress?.Report(-1); await executeAsync(param, funcLog); }, null, canExecute, busyService, mode, false, fileName, memberName)
        { }
        /// <summary>
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド非同期実行デリゲート(FuncLogあり、中断なし、進捗表示あり)</param>
        /// <param name="requestable">進捗ダイアログ要求可能オブジェクト</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="busyService">処理中状態サービス</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<T, FuncLog, IProgress<int>, Task> executeAsync, IProgressDialogRequestable requestable, Func<T, bool> canExecute = null,
                                 BusyService busyService = null,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : this(async (param, funcLog, _, progress) => await executeAsync(param, funcLog, progress), requestable, canExecute, busyService, ExecutionMode.DisallowConcurrent, false, fileName, memberName)
        { }
        /// <summary>
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド実行デリゲート(FuncLogあり、中断あり、進捗表示なし)</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="busyService">処理中状態サービス</param>
        /// <param name="mode">実行モード</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<T, FuncLog, CancellationToken, Task> executeAsync, Func<T, bool> canExecute = null,
                                 BusyService busyService = null, ExecutionMode mode = ExecutionMode.AllowConcurrent,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : this(async (param, funcLog, token, progress) => { progress?.Report(-1); await executeAsync(param, funcLog, token); }, null, canExecute, busyService, mode, true, fileName, memberName)
        { }
        /// <summary>
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド実行デリゲート(FuncLogあり、中断あり、進捗表示あり)</param>
        /// <param name="requestable">進捗ダイアログ要求可能オブジェクト</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="busyService">処理中状態サービス</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<T, FuncLog, CancellationToken, IProgress<int>, Task> executeAsync, IProgressDialogRequestable requestable, Func<T, bool> canExecute = null,
                                 BusyService busyService = null,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : this(async (param, funcLog, token, progress) => await executeAsync(param, funcLog, token, progress), requestable, canExecute, busyService, ExecutionMode.DisallowConcurrent, true, fileName, memberName)
        { }

        /// <summary>
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド実行デリゲート(FuncLogなし、中断なし、進捗表示なし)</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="busyService">処理中状態サービス</param>
        /// <param name="mode">実行モード</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<T, Task> executeAsync, Func<T, bool> canExecute = null,
                                 BusyService busyService = null, ExecutionMode mode = ExecutionMode.AllowConcurrent,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : this(async (param, _, _, progress) => { progress?.Report(-1); await executeAsync(param); }, null, canExecute, busyService, mode, false, fileName, memberName)
        { }
        /// <summary>
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド実行デリゲート(FuncLogなし、中断なし、進捗表示あり)</param>
        /// <param name="requestable">進捗ダイアログ要求可能オブジェクト</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="busyService">処理中状態サービス</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<T, IProgress<int>, Task> executeAsync, IProgressDialogRequestable requestable, Func<T, bool> canExecute = null,
                                 BusyService busyService = null,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : this(async (param, _, _, progress) => await executeAsync(param, progress), requestable, canExecute, busyService, ExecutionMode.DisallowConcurrent, false, fileName, memberName)
        { }
        /// <summary>
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド実行デリゲート(FuncLogなし、中断あり、進捗表示なし)</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="busyService">処理中状態サービス</param>
        /// <param name="mode">実行モード</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<T, CancellationToken, Task> executeAsync, Func<T, bool> canExecute = null,
                                 BusyService busyService = null, ExecutionMode mode = ExecutionMode.AllowConcurrent,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : this(async (param, _, token, progress) => { progress?.Report(-1); await executeAsync(param, token); }, null, canExecute, busyService, mode, true, fileName, memberName)
        { }
        /// <summary>
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド実行デリゲート(FuncLogなし、中断あり、進捗表示あり)</param>
        /// <param name="requestable">進捗ダイアログ要求可能オブジェクト</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="busyService">処理中状態サービス</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<T, CancellationToken, IProgress<int>, Task> executeAsync, IProgressDialogRequestable requestable = null, Func<T, bool> canExecute = null,
                                 BusyService busyService = null,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : this(async (param, _, token, progress) => await executeAsync(param, token, progress), requestable, canExecute, busyService, ExecutionMode.DisallowConcurrent, true, fileName, memberName)
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
            Task completionTask = null; // タスク実行結果通知用

            if (this.mRequestable != null) {
                ProgressDialogRequestEventArgs e = new() {
                    FuncAsync = async (token, progress) => await this.mExecuteAsync.Invoke((T)parameter, funcLog, token, progress),
                    CanCancel = this.mCanCancel,
                    TokenSource = cts,
                };
                completionTask = this.mRequestable.ProgressDialogRequest(e);
            }

            // (進捗ウィンドウ非表示の場合)非同期処理を待機 または (進捗ウィンドウ表示の場合)例外を処理する
            try {
                using IDisposable disposable = this.mBusyService?.Enter();
                await (this.mRequestable == null ? this.mExecuteAsync.Invoke((T)parameter, funcLog, cts.Token, null) : completionTask);
            }
            catch (OperationCanceledException) {
                Log.Info($"{this.mMemberName} Canceled.", this.mFileName, this.mMemberName);
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
        /// <param name="busyService">処理中状態サービス</param>
        /// <param name="mode">実行モード</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<FuncLog, Task> executeAsync, Func<bool> canExecute = null,
                                 BusyService busyService = null, ExecutionMode mode = ExecutionMode.AllowConcurrent,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : base(async (_, funcLog) => await executeAsync(funcLog), _ => canExecute?.Invoke() ?? true, busyService, mode, fileName, memberName)
        { }
        /// <summary>
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド実行デリゲート(中断なし、進捗表示あり)</param>
        /// <param name="requestable">進捗ダイアログ要求可能オブジェクト</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="busyService">処理中状態サービス</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<FuncLog, IProgress<int>, Task> executeAsync, IProgressDialogRequestable requestable, Func<bool> canExecute = null,
                                 BusyService busyService = null,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : base(async (_, funcLog, progress) => await executeAsync(funcLog, progress), requestable, _ => canExecute?.Invoke() ?? true, busyService, fileName, memberName)
        { }
        /// <summary>
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド実行デリゲート(中断あり、進捗表示なし)</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="busyService">処理中状態サービス</param>
        /// <param name="mode">実行モード</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<FuncLog, CancellationToken, Task> executeAsync, Func<bool> canExecute = null,
                                 BusyService busyService = null, ExecutionMode mode = ExecutionMode.AllowConcurrent,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : base(async (_, funcLog, token) => await executeAsync(funcLog, token), _ => canExecute?.Invoke() ?? true, busyService, mode, fileName, memberName)
        { }
        /// <summary>
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// </summary>
        /// <param name="executeAsync">コマンド実行デリゲート(中断あり、進捗表示あり)</param>
        /// <param name="requestable">進捗ダイアログ要求可能オブジェクト</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="busyService">処理中状態サービス</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<FuncLog, CancellationToken, IProgress<int>, Task> executeAsync, IProgressDialogRequestable requestable, Func<bool> canExecute = null,
                                 BusyService busyService = null,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : base(async (_, funcLog, token, progress) => await executeAsync(funcLog, token, progress), requestable, _ => canExecute?.Invoke() ?? true, busyService, fileName, memberName)
        { }
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// <param name="executeAsync">コマンド実行デリゲート(中断なし、進捗表示なし)</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="busyService">処理中状態サービス</param>
        /// <param name="mode">実行モード</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<Task> executeAsync, Func<bool> canExecute = null,
                                 BusyService busyService = null, ExecutionMode mode = ExecutionMode.AllowConcurrent,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : base(async _ => await executeAsync(), _ => canExecute?.Invoke() ?? true, busyService, mode, fileName, memberName)
        { }
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// <param name="executeAsync">コマンド実行デリゲート(中断なし、進捗表示あり)</param>
        /// <param name="requestable">進捗ダイアログ要求可能オブジェクト</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="busyService">処理中状態サービス</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<IProgress<int>, Task> executeAsync, IProgressDialogRequestable requestable, Func<bool> canExecute = null,
                                 BusyService busyService = null,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : base(async (_, progress) => await executeAsync(progress), requestable, _ => canExecute?.Invoke() ?? true, busyService, fileName, memberName)
        { }
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// <param name="executeAsync">コマンド実行デリゲート(中断あり、進捗表示なし)</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="busyService">処理中状態サービス</param>
        /// <param name="mode">実行モード</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<CancellationToken, Task> executeAsync, Func<bool> canExecute = null,
                                 BusyService busyService = null, ExecutionMode mode = ExecutionMode.AllowConcurrent,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : base(async (_, token) => await executeAsync(token), _ => canExecute?.Invoke() ?? true, busyService, mode, fileName, memberName)
        { }
        /// <see cref="AsyncRelayCommand"/> のインスタンスを生成します
        /// <param name="executeAsync">コマンド実行デリゲート(中断あり、進捗表示あり)</param>
        /// <param name="requestable">進捗ダイアログ要求可能オブジェクト</param>
        /// <param name="canExecute">コマンド実行可否デリゲート</param>
        /// <param name="busyService">処理中状態サービス</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public AsyncRelayCommand(Func<CancellationToken, IProgress<int>, Task> executeAsync, IProgressDialogRequestable requestable, Func<bool> canExecute = null,
                                 BusyService busyService = null,
                                 [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
            : base(async (_, token, progress) => await executeAsync(token, progress), requestable, _ => canExecute?.Invoke() ?? true, busyService, fileName, memberName)
        { }
    }
}

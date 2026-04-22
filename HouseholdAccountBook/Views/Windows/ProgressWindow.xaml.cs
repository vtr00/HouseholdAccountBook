using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.ViewModels;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HouseholdAccountBook.Views.Windows
{
    /// <summary>
    /// ProgressWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ProgressWindow : Window
    {
        /// <summary>
        /// 進捗度
        /// </summary>
        public IProgress<int> Progress { get; private set; }

        /// <summary>
        /// 完了済タスク
        /// </summary>
        public Task CompletionTask { get; private set; }

        /// <summary>
        /// 描画済か
        /// </summary>
        private bool mIsRendered = false;
        /// <summary>
        /// コード側からの閉じ要求か
        /// </summary>
        private bool mAllowClose = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="funcAsync">実行対象の非同期処理</param>
        /// <param name="canCancel">キャンセル可能か</param>
        /// <param name="cts">キャンセル用トークン源(Null不可)</param>
        public ProgressWindow(Window owner, Func<CancellationToken, IProgress<int>, Task> funcAsync, bool canCancel, CancellationTokenSource cts)
        {
            using FuncLog funcLog = new(new { canCancel, cts });

            this.Owner = owner;
            this.Name = UiConstants.WindowNameStr[nameof(ProgressWindow)];

            this.InitializeComponent();

            ArgumentNullException.ThrowIfNull(cts, nameof(cts));

            if (canCancel) {
                this.WVM.Canceled += (sender, e) => cts.Cancel();
                CommandManager.InvalidateRequerySuggested();
            }

            this.ContentRendered += async (sender, e) => {
                // Window表示後に処理開始
                if (this.mIsRendered) { return; }
                this.mIsRendered = true;

                TaskCompletionSource tcs = new();
                try {
                    if (funcAsync != null) {
                        await funcAsync.Invoke(cts.Token, this.Progress);
                    }
                    _ = tcs.TrySetResult();
                }
                catch (OperationCanceledException) {
                    _ = tcs.TrySetCanceled();
                }
                catch (Exception exp) {
                    _ = tcs.TrySetException(exp);
                }
                finally {
                    this.CompletionTask = tcs.Task;
                    this.mAllowClose = true;
                    this.Close();
                }
            };

            this.Progress = new Progress<int>(value => this.WVM.ProgressValue = value);
            this.Progress.Report(-1);
        }

        /// <summary>
        /// Alt+F4等によるユーザー操作での閉じを防止する
        /// </summary>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!this.mAllowClose) {
                e.Cancel = true;
            }
        }
    }
}

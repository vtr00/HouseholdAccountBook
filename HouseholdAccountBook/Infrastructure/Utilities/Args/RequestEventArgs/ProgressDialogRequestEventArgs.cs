using System;
using System.Threading;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Infrastructure.Utilities.Args.RequestEventArgs
{
    /// <summary>
    /// 進捗ダイアログ要求イベント時の引数
    /// </summary>
    public class ProgressDialogRequestEventArgs
    {
        /// <summary>
        /// 実行対象非同期処理
        /// </summary>
        public Func<CancellationToken, IProgress<int>, Task> FuncAsync { get; set; }

        /// <summary>
        /// キャンセル可能か
        /// </summary>
        public bool CanCancel { get; set; }
        /// <summary>
        /// キャンセル用トークン源
        /// </summary>
        public CancellationTokenSource TokenSource { get; set; }

        /// <summary>
        /// 完了済タスク
        /// </summary>
        public Task CompletionTask { get; set; }
    }
}

using HouseholdAccountBook.Infrastructure.Utilities;
using System;
using System.Threading;

namespace HouseholdAccountBook.Infrastructure.Services
{
    /// <summary>
    /// 同一プロセス内のスレッド間での排他を using で提供するサービス
    /// </summary>
    public static class LockService
    {
        static readonly object mGate = new();

        /// <summary>
        /// 排他処理を開始する
        /// </summary>
        /// <returns>排他終了実行用クラス</returns>
        public static IDisposable Enter()
        {
            Monitor.Enter(mGate);

            return new ActionOnDispose(static () => Monitor.Exit(mGate));
        }
    }
}

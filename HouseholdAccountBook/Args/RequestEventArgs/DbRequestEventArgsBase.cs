using HouseholdAccountBook.Adapters.DbHandlers;
using System;

namespace HouseholdAccountBook.Args.RequestEventArgs
{
    /// <summary>
    /// DBリクエストイベント時のイベント引数の基底クラス
    /// </summary>
    public abstract class DbRequestEventArgsBase : EventArgs
    {
        /// <summary>
        /// DBハンドラファクトリ
        /// </summary>
        public DbHandlerFactory DbHandlerFactory { get; set; }
    }
}

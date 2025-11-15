using HouseholdAccountBook.Adapters.DbHandler;
using System;

namespace HouseholdAccountBook.Others.RequestEventArgs
{
    /// <summary>
    /// DBリクエストイベント引数基底クラス
    /// </summary>
    public abstract class DbRequestEventArgsBase : EventArgs
    {
        /// <summary>
        /// DBハンドラファクトリ
        /// </summary>
        public DbHandlerFactory DbHandlerFactory { get; set; }
    }
}

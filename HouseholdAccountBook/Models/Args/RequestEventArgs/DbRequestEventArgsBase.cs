using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using System;

namespace HouseholdAccountBook.Models.Args.RequestEventArgs
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

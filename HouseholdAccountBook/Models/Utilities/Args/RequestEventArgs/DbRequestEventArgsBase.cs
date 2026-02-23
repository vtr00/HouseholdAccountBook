using HouseholdAccountBook.Models.Infrastructure.DbHandlers;
using System;

namespace HouseholdAccountBook.Models.Utilities.Args.RequestEventArgs
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

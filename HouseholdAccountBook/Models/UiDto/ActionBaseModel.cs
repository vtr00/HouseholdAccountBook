using HouseholdAccountBook.Models.ValueObjects;
using System;
using System.Diagnostics;

namespace HouseholdAccountBook.Models.UiDto
{
    /// <summary>
    /// 帳簿項目 基本Model
    /// </summary>
    [DebuggerDisplay("Id: {ActionId} Date: {ActTime} Amount: {Amount}")]
    public class ActionBaseModel(ActionIdObj id, DateTime actTime, AmountObj amount)
    {
        /// <summary>
        /// 帳簿項目ID
        /// </summary>
        public ActionIdObj ActionId { get; init; } = id;

        /// <summary>
        /// 日時
        /// </summary>
        public DateTime ActTime { get; init; } = actTime;
        /// <summary>
        /// 金額(主単位)
        /// </summary>
        public AmountObj Amount { get; init; } = amount;

        /// <summary>
        /// 収入(主単位)
        /// </summary>
        public AmountObj? Income => 0m < this.Amount ? this.Amount : null;
        /// <summary>
        /// 支出(主単位)
        /// </summary>
        public AmountObj? Expenses => this.Amount < 0m ? -this.Amount : null;
    }
}

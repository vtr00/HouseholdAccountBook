using HouseholdAccountBook.Models.ValueObjects;
using System;
using System.Diagnostics;

namespace HouseholdAccountBook.Models.UiDto
{
    /// <summary>
    /// 帳簿項目 基本Model
    /// </summary>
    [DebuggerDisplay("Id: {ActionId} Date: {ActTime} Amount: {Amount}")]
    public class ActionBaseModel(ActionIdObj id, DateTime actTime, decimal amount)
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
        /// 金額
        /// </summary>
        public decimal Amount { get; init; } = amount;

        /// <summary>
        /// 収入
        /// </summary>
        public decimal? Income => 0 < this.Amount ? this.Amount : null;
        /// <summary>
        /// 支出
        /// </summary>
        public decimal? Expenses => this.Amount < 0 ? -this.Amount : null;
    }
}

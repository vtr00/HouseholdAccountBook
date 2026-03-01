using HouseholdAccountBook.Models.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace HouseholdAccountBook.Models.UiDto
{
    /// <summary>
    /// 帳簿項目 基本Model
    /// </summary>
    public class ActionBaseModel(ActionIdObj id, DateTime actTime, decimal amount)
    {
        /// <summary>
        /// 帳簿項目ID
        /// </summary>
        public ActionIdObj ActionId { get; init; } = id;

        /// <summary>
        /// 時刻
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

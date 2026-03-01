using HouseholdAccountBook.Models.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace HouseholdAccountBook.Models.DomainModels
{
    /// <summary>
    /// 残高付き帳簿項目Model
    /// </summary>
    public class ActionWithBalanceModel
    {
        /// <summary>
        /// 帳簿項目
        /// </summary>
        public ActionModel Action { get; init; } = new();

        /// <summary>
        /// 残高
        /// </summary>
        public decimal Balance { get; init; } = 0;
    }
}

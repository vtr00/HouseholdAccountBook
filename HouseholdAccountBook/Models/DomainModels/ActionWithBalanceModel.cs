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
        public ActionModel Action { get; init; }

        /// <summary>
        /// 残高
        /// </summary>
        public int Balance { get; init; }
    }
}

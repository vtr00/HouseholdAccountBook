using HouseholdAccountBook.Models.ValueObjects;
using System.Diagnostics;

namespace HouseholdAccountBook.Models.UiDto
{
    /// <summary>
    /// 残高付き帳簿項目Model
    /// </summary>
    [DebuggerDisplay("Id: {Action.ActionId} Date: {Action.ActTime} Amount: {Action.Amount}")]
    public class ActionWithBalanceModel
    {
        #region プロパティ
        /// <summary>
        /// 帳簿項目
        /// </summary>
        public ActionModel Action { get; init; } = new();

        /// <summary>
        /// 残高(主単位)
        /// </summary>
        public AmountObj Balance { get; init; } = new(0m);
        #endregion
    }
}

using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.ViewModels.Abstract;
using System;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// 帳簿項目VM
    /// </summary>
    public class ActionViewModel(ActionWithBalanceModel model) : BindableBase, ISelectable
    {
        #region プロパティ
        /// <summary>
        /// 残高付き帳簿項目Model
        /// </summary>
        public ActionWithBalanceModel ActionWithBalance { get; init; } = model;

        /// <summary>
        /// CSVと一致したか
        /// </summary>
        #region IsMatch
        public bool IsMatch {
            get;
            set => this.SetProperty(ref field, value);
        } = model.Action.IsMatch;
        #endregion

        /// <summary>
        /// 選択されているか
        /// </summary>
        #region SelectFlag
        public bool SelectFlag {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 将来の日付か
        /// </summary>
        public bool IsFuture => this.ActionWithBalance.Action.ActTime > DateTime.Now;
        #endregion
    }
}

using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;
using System;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// 帳簿項目VM
    /// </summary>
    /// <param name="model">残高付き帳簿項目Model</param>
    public class ActionViewModel(ActionWithBalanceModel model) : BindableBase, ISelectable
    {
        #region プロパティ
        /// <summary>
        /// 残高付き帳簿項目Model
        /// </summary>
        public ActionWithBalanceModel ActionWithBalance { get; init; } = model;

        public ActionIdObj ActionId => this.ActionWithBalance.Action.ActionId;

        public GroupIdObj GroupId => this.ActionWithBalance.Action.GroupId;

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
        public bool IsFuture => DateTime.Now < this.ActionWithBalance.Action.ActTime;
        #endregion
    }
}

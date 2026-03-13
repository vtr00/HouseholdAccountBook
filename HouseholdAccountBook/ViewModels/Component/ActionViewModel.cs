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
        #endregion

        #region プロパティ(アクセス専用)
        /// <summary>
        /// 帳簿項目ID
        /// </summary>
        public ActionIdObj ActionId => this.ActionWithBalance.Action.ActionId;
        /// <summary>
        /// グループID
        /// </summary>
        public GroupIdObj GroupId => this.ActionWithBalance.Action.GroupId;

        /// <summary>
        /// 日時
        /// </summary>
        public DateTime ActTime => this.ActionWithBalance.Action.ActTime;
        /// <summary>
        /// 金額
        /// </summary>
        public decimal Amount => this.ActionWithBalance.Action.Amount;

        /// <summary>
        /// 収入
        /// </summary>
        public decimal? Income => this.ActionWithBalance.Action.Income;
        /// <summary>
        /// 支出
        /// </summary>
        public decimal? Expenses => this.ActionWithBalance.Action.Expenses;

        /// <summary>
        /// 帳簿名
        /// </summary>
        public string BookName => this.ActionWithBalance.Action.Book.Name;
        /// <summary>
        /// 分類名
        /// </summary>
        public string CategoryName => this.ActionWithBalance.Action.Category.Name;
        /// <summary>
        /// 項目名
        /// </summary>
        public string ItemName => this.ActionWithBalance.Action.Item.Name;

        /// <summary>
        /// 店名
        /// </summary>
        public string Shop => this.ActionWithBalance.Action.Shop?.Name;
        /// <summary>
        /// 備考
        /// </summary>
        public string Remark => this.ActionWithBalance.Action.Remark?.Remark;
        #endregion

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
    }
}

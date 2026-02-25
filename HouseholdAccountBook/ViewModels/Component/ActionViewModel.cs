using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.DomainModels;
using HouseholdAccountBook.ViewModels.Abstract;
using System;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// 帳簿項目VM
    /// </summary>
    public class ActionViewModel : BindableBase, ISelectable
    {
        #region プロパティ
        /// <summary>
        /// 残高付き帳簿項目Model
        /// </summary>
        public ActionWithBalanceModel ActionWithBalance { get; set; }

        /// <summary>
        /// CSVと一致したか
        /// </summary>
        #region IsMatch
        public bool IsMatch {
            get;
            set => this.SetProperty(ref field, value);
        }
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

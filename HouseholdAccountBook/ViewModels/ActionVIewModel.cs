using HouseholdAccountBook.Interfaces;
using Prism.Mvvm;
using System;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 帳簿項目VM
    /// </summary>
    public class ActionViewModel : BindableBase, IMultiSelectable
    {
        #region プロパティ
        /// <summary>
        /// 帳簿項目ID
        /// </summary>
        public int ActionId { get; set; }
        /// <summary>
        /// 時刻
        /// </summary>
        public DateTime ActTime { get; set; }
        /// <summary>
        /// 収支種別
        /// </summary>
        public BalanceKind BalanceKind { get; set; }
        /// <summary>
        /// カテゴリID
        /// </summary>
        public int CategoryId { get; set; }
        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; }
        /// <summary>
        /// 項目名
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 収入
        /// </summary>
        public int? Income { get; set; }
        /// <summary>
        /// 支出
        /// </summary>
        public int? Outgo { get; set; }
        /// <summary>
        /// 残高
        /// </summary>
        public int Balance { get; set; }
        /// <summary>
        /// 店舗名
        /// </summary>
        public string ShopName { get; set; }
        /// <summary>
        /// 備考
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// グループID
        /// </summary>
        public int? GroupId { get; set; }

        /// <summary>
        /// CSVと一致したか
        /// </summary>
        #region IsMatch
        public bool IsMatch
        {
            get { return this._IsMatch; }
            set { SetProperty(ref this._IsMatch, value); }
        }
        private bool _IsMatch = default(bool);
        #endregion

        /// <summary>
        /// 選択されているか
        /// </summary>
        #region IsSelected
        public bool IsSelected
        {
            get { return this._IsSelected; }
            set { SetProperty(ref this._IsSelected, value); }
        }
        private bool _IsSelected = default(bool);
        #endregion

        /// <summary>
        /// 将来の日付か
        /// </summary>
        public bool IsFuture {
            get {
                return this.ActTime > DateTime.Now;
            }
        }
        #endregion
    }
}

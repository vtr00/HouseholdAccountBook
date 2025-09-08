using HouseholdAccountBook.ViewModels.Abstract;
using System;
using static HouseholdAccountBook.Models.DbConstants;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// 帳簿項目VM
    /// </summary>
    public class ActionViewModel : BindableBase, ISelectable
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
        /// 帳簿ID
        /// </summary>
        public int BookId { get; set; }
        /// <summary>
        /// 分類ID
        /// </summary>
        public int CategoryId { get; set; }
        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; }
        /// <summary>
        /// 帳簿名
        /// </summary>
        public string BookName { get; set; }
        /// <summary>
        /// 分類名
        /// </summary>
        public string CategoryName { get; set; }
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
        public int? Expenses { get; set; }
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
            get => this._IsMatch;
            set => this.SetProperty(ref this._IsMatch, value);
        }
        private bool _IsMatch = default;
        #endregion

        /// <summary>
        /// 選択されているか
        /// </summary>
        #region SelectFlag
        public bool SelectFlag
        {
            get => this._SelectFlag;
            set => this.SetProperty(ref this._SelectFlag, value);
        }
        private bool _SelectFlag = default;
        #endregion

        /// <summary>
        /// 将来の日付か
        /// </summary>
        public bool IsFuture => this.ActTime > DateTime.Now;
        #endregion
    }
}

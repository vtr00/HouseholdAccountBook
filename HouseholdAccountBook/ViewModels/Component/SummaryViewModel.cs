using HouseholdAccountBook.ViewModels.Abstract;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// 概要VM
    /// </summary>
    public class SummaryViewModel : BindableBase
    {
        #region プロパティ
        /// <summary>
        /// 収支種別
        /// </summary>
        public int BalanceKind { get; set; } = -1;
        /// <summary>
        /// 収支名
        /// </summary>
        public string BalanceName { get; set; } = string.Empty;
        /// <summary>
        /// 分類ID
        /// </summary>
        public int CategoryId { get; set; } = -1;
        /// <summary>
        /// 分類名
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;
        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; } = -1;
        /// <summary>
        /// 項目名
        /// </summary>
        public string ItemName { get; set; } = string.Empty;
        /// <summary>
        /// その他名称
        /// </summary>
        public string OtherName { get; set; } = string.Empty;

        /// <summary>
        /// 表示名
        /// </summary>
        public string Name {
            get {
                return this.ItemName != string.Empty
                    ? $"  {this.ItemName}"
                    : this.CategoryName != string.Empty
                        ? this.CategoryName
                        : this.BalanceName != string.Empty
                            ? this.BalanceName
                            : this.OtherName;
            }
        }

        /// <summary>
        /// 合計
        /// </summary>
        public int Total { get; set; }
        #endregion
    }
}

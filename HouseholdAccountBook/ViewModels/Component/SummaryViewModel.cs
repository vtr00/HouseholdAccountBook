using HouseholdAccountBook.Models.DomainModels;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// 概要VM
    /// </summary>
    public class SummaryViewModel
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
        /// 分類
        /// </summary>
        public CategoryModel Category { get; set; } = new CategoryModel();
        /// <summary>
        /// 項目
        /// </summary>
        public ItemModel Item { get; set; } = new ItemModel();
        /// <summary>
        /// その他名称
        /// </summary>
        public string OtherName { get; set; } = string.Empty;

        /// <summary>
        /// 表示名
        /// </summary>
        public string Name {
            get {
                return this.Item.Name != string.Empty
                    ? $"  {this.Item.Name}"
                    : this.Category.Name != string.Empty
                        ? this.Category.Name
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

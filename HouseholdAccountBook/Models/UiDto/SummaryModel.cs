using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels;

namespace HouseholdAccountBook.Models.UiDto
{
    /// <summary>
    /// 概要VM
    /// </summary>
    public class SummaryModel
    {
        #region プロパティ
        /// <summary>
        /// 収支名
        /// </summary>
        public string BalanceName => this.Category.BalanceKind == BalanceKind.Others ? string.Empty : UiConstants.BalanceKindStr[this.Category.BalanceKind];

        /// <summary>
        /// 分類
        /// </summary>
        public CategoryModel Category { get; init; } = new(CategoryIdObj.System, string.Empty, BalanceKind.Others);
        /// <summary>
        /// 項目
        /// </summary>
        public ItemModel Item { get; init; } = new(ItemIdObj.System, string.Empty);
        /// <summary>
        /// その他名称
        /// </summary>
        public string OtherName { get; init; } = string.Empty;

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
        public decimal Total { get; init; } = 0;
        #endregion
    }
}

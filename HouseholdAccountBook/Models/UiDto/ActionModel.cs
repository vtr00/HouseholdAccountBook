using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Component;
using System;

namespace HouseholdAccountBook.Models.UiDto
{
    /// <summary>
    /// 帳簿項目Model
    /// </summary>
    public class ActionModel
    {
        #region プロパティ
        /// <summary>
        /// グループID
        /// </summary>
        public GroupIdObj GroupId { get; init; } = -1;

        /// <summary>
        /// 帳簿
        /// </summary>
        public BookModel Book { get; init; } = new(-1, string.Empty);
        /// <summary>
        /// 分類
        /// </summary>
        public CategoryModel Category { get; init; } = new(-1, string.Empty, BalanceKind.Others);
        /// <summary>
        /// 項目
        /// </summary>
        public ItemModel Item { get; init; } = new(-1, string.Empty);

        /// <summary>
        /// 帳簿項目 基本Model
        /// </summary>
        public ActionBaseModel Base { get; init; } = new(-1, DateTime.Now, 0);

        /// <summary>
        /// 店舗名
        /// </summary>
        public ShopModel Shop { get; init; } = string.Empty;
        /// <summary>
        /// 備考
        /// </summary>
        public RemarkModel Remark { get; init; } = string.Empty;

        /// <summary>
        /// 一致フラグ
        /// </summary>
        public bool IsMatch { get; init; } = false;
        #endregion

        #region プロパティ(アクセス専用)
        /// <summary>
        /// 帳簿項目ID
        /// </summary>
        public ActionIdObj ActionId => this.Base?.ActionId;
        /// <summary>
        /// 時刻
        /// </summary>
        public DateTime? ActTime => this.Base?.ActTime;
        /// <summary>
        /// 金額
        /// </summary>
        public decimal? Amount => this.Base?.Amount;

        /// <summary>
        /// 収入
        /// </summary>
        public decimal? Income => this.Base?.Income;
        /// <summary>
        /// 支出
        /// </summary>
        public decimal? Expenses => this.Base?.Expenses;
        #endregion

        public ActionModel() { }

        public ActionModel WithChanges(ActionBaseModel baseModel)
        {
            return new() {
                GroupId = this.GroupId,
                Book = this.Book,
                Category = this.Category,
                Item = this.Item,
                Base = baseModel,
                Shop = this.Shop,
                Remark = this.Remark
            };
        }
    }
}

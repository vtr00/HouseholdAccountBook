using System;

namespace HouseholdAccountBook.Models.DomainModels
{
    /// <summary>
    /// 帳簿項目Model
    /// </summary>
    public class ActionModel
    {
        #region プロパティ
        /// <summary>
        /// 帳簿項目ID
        /// </summary>
        public int ActionId { get; init; }
        /// <summary>
        /// グループID
        /// </summary>
        public int? GroupId { get; init; }

        /// <summary>
        /// 時刻
        /// </summary>
        public DateTime ActTime { get; init; }
        /// <summary>
        /// 帳簿
        /// </summary>
        public BookModel Book { get; init; }
        /// <summary>
        /// 分類
        /// </summary>
        public CategoryModel Category { get; init; }
        /// <summary>
        /// 項目
        /// </summary>
        public ItemModel Item { get; init; }
        /// <summary>
        /// 金額
        /// </summary>
        public AmountModel Amount { get; init; }
        /// <summary>
        /// 店舗
        /// </summary>
        public ShopModel Shop { get; init; }
        /// <summary>
        /// 備考
        /// </summary>
        public RemarkModel Remark { get; init; }
        #endregion
    }
}

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
        public int ActionId { get; set; }
        /// <summary>
        /// グループID
        /// </summary>
        public int? GroupId { get; set; }

        /// <summary>
        /// 時刻
        /// </summary>
        public DateTime ActTime { get; set; }
        /// <summary>
        /// 帳簿
        /// </summary>
        public BookModel Book { get; set; }
        /// <summary>
        /// 分類
        /// </summary>
        public CategoryModel Category { get; set; }
        /// <summary>
        /// 項目
        /// </summary>
        public ItemModel Item { get; set; }
        /// <summary>
        /// 金額
        /// </summary>
        public AmountModel Amount { get; set; }
        /// <summary>
        /// 店舗
        /// </summary>
        public ShopModel Shop { get; set; }
        /// <summary>
        /// 備考
        /// </summary>
        public RemarkModel Remark { get; set; }
        #endregion
    }
}

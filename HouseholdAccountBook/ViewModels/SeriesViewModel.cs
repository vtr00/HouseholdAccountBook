using System.Collections.Generic;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 系列VM
    /// </summary>
    public class SeriesViewModel
    {
        /// <summary>
        /// 収支種別
        /// </summary>
        public int BalanceKind { get; set; }
        /// <summary>
        /// カテゴリID
        /// </summary>
        public int CategoryId { get; set; }
        /// <summary>
        /// カテゴリ名
        /// </summary>
        public string CategoryName { get; set; }
        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; }
        /// <summary>
        /// 項目名
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 値
        /// </summary>
        public List<int> Values { get; set; }
        /// <summary>
        /// 平均
        /// </summary>
        public int? Average { get; set; }
        /// <summary>
        /// 合計
        /// </summary>
        public int? Summary { get; set; }
    }
}

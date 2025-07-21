using HouseholdAccountBook.Dto.Abstract;
using System;

namespace HouseholdAccountBook.Dto.DbTable
{
    /// <summary>
    /// 店舗名テーブルDTO
    /// </summary>
    public class HstShopDto : TableDtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HstShopDto() { }

        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; } = 0;
        /// <summary>
        /// 店舗名
        /// </summary>
        public string ShopName { get; set; } = string.Empty;
        /// <summary>
        /// 使用日時
        /// </summary>
        public DateTime UsedTime { get; set; } = DateTime.Now;
    }
}

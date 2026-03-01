using HouseholdAccountBook.Infrastructure.DB.DbDto.Abstract;
using System;

namespace HouseholdAccountBook.Infrastructure.DB.DbDto.DbTable
{
    /// <summary>
    /// 店舗名DTO
    /// </summary>
    public class HstShopDto : CommonTableDtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HstShopDto() { }

        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; }
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

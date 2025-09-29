using HouseholdAccountBook.Adapters.Dto.Abstract;
using System;

namespace HouseholdAccountBook.Adapters.Dto.Others
{
    /// <summary>
    /// 店舗情報DTO
    /// </summary>
    public class ShopInfoDto : DtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ShopInfoDto() : base() { }

        /// <summary>
        /// 店名
        /// </summary>
        public string ShopName { get; set; } = string.Empty;
        /// <summary>
        /// 使用回数
        /// </summary>
        public int Count { get; set; } = 0;
        /// <summary>
        /// 使用時刻
        /// </summary>
        public DateTime? UsedTime { get; set; } = null;
    }
}

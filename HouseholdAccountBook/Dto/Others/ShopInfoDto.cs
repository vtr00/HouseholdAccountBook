using System;

namespace HouseholdAccountBook.Dto.Others
{
    /// <summary>
    /// 店舗情報DTO
    /// </summary>
    public class ShopInfoDto
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ShopInfoDto() { }

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

using HouseholdAccountBook.Infrastructure.DB.DbDto.Abstract;
using System;

namespace HouseholdAccountBook.Infrastructure.DB.DbDto.Others
{
    /// <summary>
    /// 店舗情報DTO
    /// </summary>
    public class ShopInfoDto : VirTableDtoBase
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
        public int Count { get; set; }
        /// <summary>
        /// 最新帳簿項目日時
        /// </summary>
        public DateTime? CurrentActTime { get; set; }
    }
}

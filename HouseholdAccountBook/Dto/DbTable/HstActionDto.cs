using HouseholdAccountBook.Dto.Abstract;
using System;

namespace HouseholdAccountBook.Dto.DbTable
{
    /// <summary>
    /// HstActionDto
    /// </summary>
    public class HstActionDto : DtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HstActionDto() : base() { }

        /// <summary>
        /// 帳簿項目ID
        /// </summary>
        public int ActionId { get; set; } = -1;
        /// <summary>
        /// 帳簿ID
        /// </summary>
        public int BookId { get; set; } = -1;
        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; } = -1;
        /// <summary>
        /// 項目日時
        /// </summary>
        public DateTime ActTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 項目値
        /// </summary>
        public int ActValue { get; set; } = 0;
        /// <summary>
        /// 店舗名
        /// </summary>
        public string ShopName { get; set; } = null;
        /// <summary>
        /// 一致フラグ
        /// </summary>
        public int IsMatch { get; set; } = 0;
        /// <summary>
        /// グループID
        /// </summary>
        public int? GroupId { get; set; } = null;
        /// <summary>
        /// 備考
        /// </summary>
        public string Remark { get; set; } = null;
    }
}

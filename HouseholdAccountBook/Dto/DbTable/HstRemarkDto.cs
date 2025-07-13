using HouseholdAccountBook.Dto.Abstract;
using System;

namespace HouseholdAccountBook.Dto.DbTable
{
    /// <summary>
    /// HstRemarkDto
    /// </summary>
    public class HstRemarkDto : DtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HstRemarkDto() { }

        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; } = 0;
        /// <summary>
        /// 備考
        /// </summary>
        public string Remark { get; set; } = string.Empty;
        /// <summary>
        /// 備考種別
        /// </summary>
        public int RemarkKind { get; set; } = 0;
        /// <summary>
        /// 使用日時
        /// </summary>
        public DateTime UsedTime { get; set; } = DateTime.Now;
    }
}

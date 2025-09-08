using HouseholdAccountBook.Models.Dto.Abstract;
using System;

namespace HouseholdAccountBook.Models.Dto.Others
{
    /// <summary>
    /// 備考情報DTO
    /// </summary>
    public class RemarkInfoDto : DtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RemarkInfoDto() : base() { }

        /// <summary>
        /// 備考
        /// </summary>
        public string Remark { get; set; } = string.Empty;
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

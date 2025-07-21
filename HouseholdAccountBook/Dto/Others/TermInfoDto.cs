using HouseholdAccountBook.Dto.Abstract;
using System;

namespace HouseholdAccountBook.Dto.Others
{
    /// <summary>
    /// 期間情報DTO
    /// </summary>
    public class TermInfoDto : DtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TermInfoDto() : base() { }

        /// <summary>
        /// 最初の日時
        /// </summary>
        public DateTime FirstTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 最後の日時
        /// </summary>
        public DateTime LastTime { get; set; } = DateTime.Now;
    }
}

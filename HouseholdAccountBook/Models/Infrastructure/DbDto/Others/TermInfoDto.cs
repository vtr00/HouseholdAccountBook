using HouseholdAccountBook.Models.Infrastructure.DbDto.Abstract;
using System;

namespace HouseholdAccountBook.Models.Infrastructure.DbDto.Others
{
    /// <summary>
    /// 期間情報DTO
    /// </summary>
    public class TermInfoDto : VirTableDtoBase
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

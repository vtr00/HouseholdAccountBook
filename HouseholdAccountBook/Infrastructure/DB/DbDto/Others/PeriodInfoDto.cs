using HouseholdAccountBook.Infrastructure.DB.DbDto.Abstract;
using System;

namespace HouseholdAccountBook.Infrastructure.DB.DbDto.Others
{
    /// <summary>
    /// 期間情報DTO
    /// </summary>
    public class PeriodInfoDto : VirTableDtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PeriodInfoDto() : base() { }

        /// <summary>
        /// 最初の日時
        /// </summary>
        public DateTime FirstTime { get; set; }
        /// <summary>
        /// 最後の日時
        /// </summary>
        public DateTime LastTime { get; set; }
    }
}

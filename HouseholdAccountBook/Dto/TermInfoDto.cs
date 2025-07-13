using System;

namespace HouseholdAccountBook.Dto
{
    /// <summary>
    /// 期間情報DTO
    /// </summary>
    public class TermInfoDto
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TermInfoDto() { }

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

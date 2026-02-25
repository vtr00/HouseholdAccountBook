using System;

namespace HouseholdAccountBook.Models.Infrastructure
{
    /// <summary>
    /// 帳簿項目CSV DTO
    /// </summary>
    public class ActionCsvDto
    {
        #region プロパティ
        /// <summary>
        /// 日付
        /// </summary>
        public DateTime Date { get; init; }

        /// <summary>
        /// 値
        /// </summary>
        public int Value { get; init; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; init; }
        #endregion
    }
}

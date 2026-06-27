using HouseholdAccountBook.Models.ValueObjects;
using System;

namespace HouseholdAccountBook.Models.UiDto
{
    /// <summary>
    /// 帳簿項目CSVモデル
    /// </summary>
    public class ActionCsvModel
    {
        #region プロパティ
        /// <summary>
        /// 日付
        /// </summary>
        public DateTime Date { get; init; }

        /// <summary>
        /// 値(主単位)
        /// </summary>
        public AmountObj Value { get; init; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; init; }
        #endregion
    }
}

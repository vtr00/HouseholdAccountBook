using HouseholdAccountBook.Models.ValueObjects;
using System;
using System.Diagnostics;

namespace HouseholdAccountBook.Models.UiDto
{
    /// <summary>
    /// 備考Model
    /// </summary>
    /// <param name="Remark">備考</param>
    [DebuggerDisplay("{Remark}")]
    public record class RemarkModel(string Remark)
    {
        #region プロパティ
        /// <summary>
        /// 項目ID
        /// </summary>
        public ItemIdObj ItemId { get; set; }

        /// <summary>
        /// 使用回数
        /// </summary>
        public int UsedCount { get; init; }

        /// <summary>
        /// 最新帳簿項目日時
        /// </summary>
        public DateTime? CurrentActTime { get; init; }
        #endregion

        public override string ToString() => this.Remark;

        public static implicit operator string(RemarkModel remark) => remark?.Remark;
        public static implicit operator RemarkModel(string remark) => new(remark);
    }
}

using System;

namespace HouseholdAccountBook.Models.UiDto
{
    /// <summary>
    /// 備考Model
    /// </summary>
    /// <param name="Remark">備考</param>
    public record class RemarkModel(string Remark)
    {
        #region プロパティ
        /// <summary>
        /// 使用回数
        /// </summary>
        public int UsedCount { get; init; }

        /// <summary>
        /// 最終使用日
        /// </summary>
        public DateTime? UsedTime { get; init; }
        #endregion

        public override string ToString() => this.Remark;

        public static implicit operator string(RemarkModel remark) => remark.Remark;
        public static implicit operator RemarkModel(string remark) => new(remark);
    }
}

using System;

namespace HouseholdAccountBook.Models.UiDto
{
    /// <summary>
    /// 備考Model
    /// </summary>
    public class RemarkModel(string remark)
    {
        #region プロパティ
        /// <summary>
        /// 備考
        /// </summary>
        public string Remark { get; init; } = remark;

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

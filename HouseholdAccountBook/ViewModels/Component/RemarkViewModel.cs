using System;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// 備考VM
    /// </summary>
    public class RemarkViewModel
    {
        #region プロパティ
        /// <summary>
        /// 備考
        /// </summary>
        public string Remark { get; init; }

        /// <summary>
        /// 使用回数
        /// </summary>
        public int UsedCount { get; init; }

        /// <summary>
        /// 最終使用日
        /// </summary>
        public DateTime? UsedTime { get; init; }
        #endregion
    }
}

using System;

namespace HouseholdAccountBook.ViewModels
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
        public string Remark { get; set; }

        /// <summary>
        /// 使用回数
        /// </summary>
        public int UsedCount { get; set; }

        /// <summary>
        /// 最終使用日
        /// </summary>
        public DateTime? UsedTime { get; set; }
        #endregion
    }
}

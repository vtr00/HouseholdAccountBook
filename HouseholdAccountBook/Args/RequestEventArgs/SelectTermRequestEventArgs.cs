using HouseholdAccountBook.Enums;
using System;

namespace HouseholdAccountBook.Args.RequestEventArgs
{
    /// <summary>
    /// 期間選択要求イベント時のイベント引数
    /// </summary>
    public class SelectTermRequestEventArgs : DbRequestEventArgsBase
    {
        /// <summary>
        /// 期間種別
        /// </summary>
        public TermKind TermKind { get; set; }
        /// <summary>
        /// 選択月
        /// </summary>
        public DateTime? Month { get; set; }
        /// <summary>
        /// 開始日
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// 終了日
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// 期間選択結果
        /// </summary>
        public bool Result { get; set; }
    }
}

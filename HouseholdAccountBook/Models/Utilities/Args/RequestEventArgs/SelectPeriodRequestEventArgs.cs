using HouseholdAccountBook.Models.ValueObjects;
using System;

namespace HouseholdAccountBook.Models.Utilities.Args.RequestEventArgs
{
    /// <summary>
    /// 期間選択要求イベント時のイベント引数
    /// </summary>
    public class SelectPeriodRequestEventArgs : DbRequestEventArgsBase
    {
        /// <summary>
        /// 期間種別
        /// </summary>
        public PeriodKind TermKind { get; set; }
        /// <summary>
        /// 選択月
        /// </summary>
        public DateOnly? Month { get; set; }

        /// <summary>
        /// 期間
        /// </summary>
        public PeriodObj<DateOnly> Period { get; set; }

        /// <summary>
        /// 期間選択結果
        /// </summary>
        public bool Result { get; set; }
    }
}

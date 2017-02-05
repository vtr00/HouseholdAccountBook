using System;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 日付金額VM
    /// </summary>
    public class DateValueViewModel
    {
        /// <summary>
        /// 日付
        /// </summary>
        public DateTime ActDate { get; set; } = DateTime.Now;
        /// <summary>
        /// 金額
        /// </summary>
        public int? ActValue { get; set; }
    }

}

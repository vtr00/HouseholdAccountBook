using System;

namespace HouseholdAccountBook.Extentions
{
    /// <summary>
    /// DateTime拡張
    /// </summary>
    public static partial class DateTimeExtensions
    {
        /// <summary>
        /// 月初めを取得する
        /// </summary>
        /// <param name="dateTime">対象の日付</param>
        /// <returns>月初め</returns>
        public static DateTime FirstDateOfMonth (this DateTime dateTime)
        {
            DateTime ans = new DateTime(dateTime.Year, dateTime.Month, 1);
            return ans;
        }

        /// <summary>
        /// 年初めを取得する
        /// </summary>
        /// <param name="dateTime">対象の日付</param>
        /// <returns>年初め</returns>
        public static DateTime FirstDateOfYear (this DateTime dateTime)
        {
            DateTime ans = new DateTime(dateTime.Year, 1, 1);
            return ans;
        }

        /// <summary>
        /// 会計年度初めを取得する
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="yearsFirstMonth"></param>
        /// <returns></returns>
        public static DateTime FirstDateOfFiscalYear(this DateTime dateTime, int firstMonthOfFiscalYear)
        {
            DateTime ans = dateTime.AddMonths(-(firstMonthOfFiscalYear - 1));
            ans = new DateTime(ans.Year, firstMonthOfFiscalYear, 1);
            return ans;
        }
    }
}

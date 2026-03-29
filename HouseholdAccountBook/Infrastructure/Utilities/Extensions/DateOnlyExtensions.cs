using System;
using System.Diagnostics;

namespace HouseholdAccountBook.Infrastructure.Utilities.Extensions
{
    /// <summary>
    /// <see cref="DateOnly"/> の拡張メソッドを提供します
    /// </summary>
    public static class DateOnlyExtensions
    {
        /// <summary>
        /// 年始めを取得する
        /// </summary>
        /// <param name="date">対象の日付</param>
        /// <returns>年始め</returns>
        public static DateOnly GetFirstDateOfYear(this DateOnly date)
        {
            DateOnly ans = new(date.Year, 1, 1);
            return ans;
        }

        /// <summary>
        /// 年終わりを取得する
        /// </summary>
        /// <param name="date">対象の日付</param>
        /// <returns>年終わり</returns>
        public static DateOnly GetLastDateOfYear(this DateOnly date)
        {
            DateOnly ans = new DateOnly(date.Year, 1, 1).AddYears(1).AddDays(-1);
            return ans;
        }

        /// <summary>
        /// 会計年度始めを取得する
        /// </summary>
        /// <param name="date">対象の日付</param>
        /// <param name="firstMonthOfFiscalYear">開始月</param>
        /// <returns>会計年度始め</returns>
        public static DateOnly GetFirstDateOfFiscalYear(this DateOnly date, int firstMonthOfFiscalYear)
        {
            firstMonthOfFiscalYear = Math.Max(1, Math.Min(firstMonthOfFiscalYear, 12));

            DateOnly ans = date.AddMonths(-(firstMonthOfFiscalYear - 1));
            ans = new DateOnly(ans.Year, firstMonthOfFiscalYear, 1);
            return ans;
        }

        /// <summary>
        /// 会計年度終わりを取得する
        /// </summary>
        /// <param name="date">対象の日付</param>
        /// <param name="firstMonthOfFiscalYear">開始月</param>
        /// <returns>会計年度終わり</returns>
        public static DateOnly GetLastDateOfFiscalYear(this DateOnly date, int firstMonthOfFiscalYear)
        {
            firstMonthOfFiscalYear = Math.Max(1, Math.Min(firstMonthOfFiscalYear, 12));

            DateOnly ans = date.AddMonths(-(firstMonthOfFiscalYear - 1));
            ans = new DateOnly(ans.Year, firstMonthOfFiscalYear, 1).AddYears(1).AddDays(-1);
            return ans;
        }

        /// <summary>
        /// 月始めを取得する
        /// </summary>
        /// <param name="date">対象の日付</param>
        /// <returns>月始め</returns>
        public static DateOnly GetFirstDateOfMonth(this DateOnly date)
        {
            DateOnly ans = new(date.Year, date.Month, 1);
            return ans;
        }

        /// <summary>
        /// 月終わりを取得する
        /// </summary>
        /// <param name="date">対象の日付</param>
        /// <returns>月終わり</returns>
        public static DateOnly GetLastDateOfMonth(this DateOnly date)
        {
            DateOnly ans = new DateOnly(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
            return ans;
        }

        /// <summary>
        /// 月内の指定した日を取得する(日数を上回れば丸める)
        /// </summary>
        /// <param name="date">対象の日付</param>
        /// <param name="day">指定する日</param>
        /// <returns>月内の指定された日</returns>
        public static DateOnly GetDateInMonth(this DateOnly date, int day)
        {
            day = Math.Min(day, DateTime.DaysInMonth(date.Year, date.Month));
            DateOnly ans = new(date.Year, date.Month, day);
            return ans;
        }

        /// <summary>
        /// 過去の日付か
        /// </summary>
        /// <param name="date">対象の日付</param>
        /// <returns></returns>
        public static bool IsPost(this DateOnly date) => date < DateOnly.FromDateTime(DateTime.Now);
        /// <summary>
        /// 今日を取得する
        /// </summary>
        public static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

        /// <summary>
        /// 期間内かどうかを取得する
        /// </summary>
        /// <param name="startDate1">期間1開始</param>
        /// <param name="endDate1">期間1終了</param>
        /// <param name="startDate2">期間2開始</param>
        /// <param name="endDate2">期間2終了</param>
        /// <returns>期間内かどうか</returns>
        public static bool IsWithIn(DateOnly? startDate1, DateOnly? endDate1, DateOnly? startDate2, DateOnly? endDate2)
        {
            Debug.Assert(startDate1 == null || endDate1 == null || startDate1 < endDate1);
            Debug.Assert(startDate2 == null || endDate2 == null || startDate2 < endDate2);

            return !(startDate1 != null && endDate2 != null && (endDate2 < startDate1))
                && !(startDate2 != null && endDate1 != null && (endDate1 < startDate2));
        }
    }
}

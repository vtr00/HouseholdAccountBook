using System;

namespace HouseholdAccountBook.Extentions
{
    /// <summary>
    /// <see cref="DateTime"/> 拡張
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// 月初めを取得する
        /// </summary>
        /// <param name="dateTime">対象の日付</param>
        /// <returns>月初め</returns>
        public static DateTime GetFirstDateOfMonth (this DateTime dateTime)
        {
            DateTime ans = new DateTime(dateTime.Year, dateTime.Month, 1);
            return ans;
        }

        /// <summary>
        /// 年初めを取得する
        /// </summary>
        /// <param name="dateTime">対象の日付</param>
        /// <returns>年初め</returns>
        public static DateTime GetFirstDateOfYear (this DateTime dateTime)
        {
            DateTime ans = new DateTime(dateTime.Year, 1, 1);
            return ans;
        }

        /// <summary>
        /// 会計年度初めを取得する
        /// </summary>
        /// <param name="dateTime">対象の日付</param>
        /// <param name="yearsFirstMonth">初め月</param>
        /// <returns>会計年度初め</returns>
        public static DateTime GetFirstDateOfFiscalYear(this DateTime dateTime, int firstMonthOfFiscalYear)
        {
            DateTime ans = dateTime.AddMonths(-(firstMonthOfFiscalYear - 1));
            ans = new DateTime(ans.Year, firstMonthOfFiscalYear, 1);
            return ans;
        }

        /// <summary>
        /// 月内の指定した日を取得する(日数を上回れば丸める)
        /// </summary>
        /// <param name="dateTime">対象の日付</param>
        /// <param name="day">指定する日</param>
        /// <returns>月内の指定された日</returns>
        public static DateTime GetDateInMonth(this DateTime dateTime, int day)
        {
            day = Math.Min(day, DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
            DateTime ans = new DateTime(dateTime.Year, dateTime.Month, day);
            return ans;
        }

        /// <summary>
        /// 休日かどうかを取得する
        /// </summary>
        /// <param name="dateTime">対象の日付</param>
        /// <returns>休日かどうか</returns>
        public static bool IsHoliday(this DateTime dateTime)
        {
            bool ans = false;
            switch (dateTime.DayOfWeek) {
                case DayOfWeek.Saturday:
                case DayOfWeek.Sunday:
                    ans = true;
                    break;
                default:
                    ans = false; //TODO: 祝日対応
                    break;
            }
            return ans;
        }
    }
}

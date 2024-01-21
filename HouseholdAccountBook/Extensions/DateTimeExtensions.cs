using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Extensions
{
    /// <summary>
    /// <see cref="DateTime"/> の拡張メソッドを提供します
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// 国民の祝日リスト
        /// </summary>
        private static readonly List<DateTime> holidayList = new List<DateTime>();

        /// <summary>
        /// 国民の祝日リストを取得する
        /// </summary>
        public static async Task DownloadHolidayListAsync()
        {
            Properties.Settings settings = Properties.Settings.Default;

            Uri uri = new Uri(settings.App_NationalHolidayCsv_Uri);

            CsvConfiguration csvConfig = new CsvConfiguration(System.Globalization.CultureInfo.CurrentCulture) {
                HasHeaderRecord = true,
                MissingFieldFound = (mffa) => { }
            };

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            using (WebClient client = new WebClient()) {
                Stream stream = await client.OpenReadTaskAsync(uri);
                if (stream.CanRead) {
                    holidayList.Clear();
                    try {
                        // CSVファイルを読み込む
                        using (CsvReader reader = new CsvReader(new StreamReader(stream, Encoding.GetEncoding("Shift_JIS")), csvConfig)) {
                            while (reader.Read()) {
                                if (reader.TryGetField(settings.App_NationalHolidayCsv_DateIndex, out string dateString)) {
                                    if (DateTime.TryParse(dateString, out DateTime dateTime)) {
                                        holidayList.Add(dateTime);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception) { }
                }
            }
        }

        /// <summary>
        /// 年始めを取得する
        /// </summary>
        /// <param name="dateTime">対象の日付</param>
        /// <returns>年始め</returns>
        public static DateTime GetFirstDateOfYear(this DateTime dateTime)
        {
            DateTime ans = new DateTime(dateTime.Year, 1, 1);
            return ans;
        }

        /// <summary>
        /// 年終わりを取得する
        /// </summary>
        /// <param name="dateTime">対象の日付</param>
        /// <returns>年終わり</returns>
        public static DateTime GetLastDateOfYear(this DateTime dateTime)
        {
            DateTime ans = new DateTime(dateTime.Year, 1, 1).AddYears(1).AddMilliseconds(-1);
            return ans;
        }

        /// <summary>
        /// 会計年度始めを取得する
        /// </summary>
        /// <param name="dateTime">対象の日付</param>
        /// <param name="yearsFirstMonth">開始月</param>
        /// <returns>会計年度始め</returns>
        public static DateTime GetFirstDateOfFiscalYear(this DateTime dateTime, int firstMonthOfFiscalYear)
        {
            firstMonthOfFiscalYear = Math.Max(1, Math.Min(firstMonthOfFiscalYear, 12));

            DateTime ans = dateTime.AddMonths(-(firstMonthOfFiscalYear - 1));
            ans = new DateTime(ans.Year, firstMonthOfFiscalYear, 1);
            return ans;
        }

        /// <summary>
        /// 会計年度終わりを取得する
        /// </summary>
        /// <param name="dateTime">対象の日付</param>
        /// <param name="firstMonthOfFiscalYear">開始月</param>
        /// <returns>会計年度終わり</returns>
        public static DateTime GetLastDateOfFiscalYear(this DateTime dateTime, int firstMonthOfFiscalYear)
        {
            firstMonthOfFiscalYear = Math.Max(1, Math.Min(firstMonthOfFiscalYear, 12));

            DateTime ans = dateTime.AddMonths(-(firstMonthOfFiscalYear - 1));
            ans = new DateTime(ans.Year, firstMonthOfFiscalYear, 1).AddYears(1).AddMilliseconds(-1);
            return ans;
        }

        /// <summary>
        /// 月始めを取得する
        /// </summary>
        /// <param name="dateTime">対象の日付</param>
        /// <returns>月始め</returns>
        public static DateTime GetFirstDateOfMonth(this DateTime dateTime)
        {
            DateTime ans = new DateTime(dateTime.Year, dateTime.Month, 1);
            return ans;
        }

        /// <summary>
        /// 月終わりを取得する
        /// </summary>
        /// <param name="dateTime">対象の日付</param>
        /// <returns>月終わり</returns>
        public static DateTime GetLastDateOfMonth(this DateTime dateTime)
        {
            DateTime ans = new DateTime(dateTime.Year, dateTime.Month, 1).AddMonths(1).AddMilliseconds(-1);
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
        /// 国民の祝日かどうかを取得する
        /// </summary>
        /// <param name="dateTime">対象の日付</param>
        /// <returns>国民の祝日かどうか</returns>
        public static bool IsNationalHoliday(this DateTime dateTime)
        {
            bool ans = holidayList.Contains(dateTime);
            return ans;
        }

        /// <summary>
        /// 期間内かどうかを取得する
        /// </summary>
        /// <param name="startDateTime1">期間1開始</param>
        /// <param name="endDateTime1">期間1終了</param>
        /// <param name="startDateTime2">期間2開始</param>
        /// <param name="endDateTime2">期間2終了</param>
        /// <returns>期間内かどうか</returns>
        public static bool IsWithIn(DateTime? startDateTime1, DateTime? endDateTime1, DateTime? startDateTime2, DateTime? endDateTime2)
        {
            Debug.Assert(startDateTime1 == null || endDateTime1 == null || startDateTime1 < endDateTime1);
            Debug.Assert(startDateTime2 == null || endDateTime2 == null || startDateTime2 < endDateTime2);

            if (startDateTime1 != null && endDateTime2 != null && endDateTime2 < startDateTime1) {
                return false;
            }
            if (startDateTime2 != null && endDateTime1 != null && endDateTime1 < startDateTime2) {
                return false;
            }
            return true;
        }
    }
}

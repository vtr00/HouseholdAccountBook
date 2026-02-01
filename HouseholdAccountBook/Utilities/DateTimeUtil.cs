using System;

namespace HouseholdAccountBook.Utilities
{
    /// <summary>
    /// <see cref="DateTime"/> の拡張メソッドを提供します
    /// </summary>
    public static class DateTimeUtil
    {
        /// <summary>
        /// 設定に応じた年の単位を取得する
        /// </summary>
        /// <param name="startMonth">会計年度開始月</param>
        /// <returns>年の単位</returns>
        public static string GetYearUnit(int startMonth) => startMonth == 1 ? Properties.Resources.Unit_Year : Properties.Resources.Unit_FiscalYear;
        /// <summary>
        /// 設定に応じた年の単位(前置)を取得する
        /// </summary>
        /// <param name="startMonth">会計年度開始月</param>
        /// <returns></returns>
        public static string GetYearPreUnit(int startMonth) => startMonth == 1 ? string.Empty : Properties.Resources.Unit_FiscalYear_Pre;
        /// <summary>
        /// 設定に応じた年の単位(後置)を取得する
        /// </summary>
        /// <param name="startMonth">会計年度開始月</param>
        /// <returns></returns>
        public static string GetYearPostUnit(int startMonth) => startMonth == 1 ? string.Empty : Properties.Resources.Unit_FiscalYear_Post;
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace HouseholdAccountBook.Converters
{
    /// <summary>
    /// DateTime - 曜日変換
    /// </summary>
    public class DateTimeToDayOfWeekConverter : IValueConverter
    {
        private static readonly Dictionary<DayOfWeek, string> dictionary = new Dictionary<DayOfWeek, string>() {
            {DayOfWeek.Sunday, "日"},
            {DayOfWeek.Monday, "月"},
            {DayOfWeek.Tuesday, "火"},
            {DayOfWeek.Wednesday, "水"},
            {DayOfWeek.Thursday, "木"},
            {DayOfWeek.Friday, "金"},
            {DayOfWeek.Saturday, "土"}
        };

        /// <summary>
        /// DateTime -> 曜日変換
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime) {
                return dictionary[dateTime.DayOfWeek];
            }
            else {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 曜日 -> DateTime変換
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}

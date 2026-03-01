using System;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Windows.Data;

namespace HouseholdAccountBook.Views.Converters
{
    /// <summary>
    /// DateTime - 曜日変換
    /// </summary>
    public class DateTimeToDayOfWeekConverter : IValueConverter
    {
        /// <summary>
        /// DateTime -> 曜日変換
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) {
                return null;
            }
            if (value is DateTime dateTime) {
                return culture.DateTimeFormat.GetAbbreviatedDayName(dateTime.DayOfWeek);
            }
            else if (value is DateOnly dateOnly) {
                return culture.DateTimeFormat.GetAbbreviatedDayName(dateOnly.DayOfWeek);
            }
            throw new NotImplementedException();
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
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

}

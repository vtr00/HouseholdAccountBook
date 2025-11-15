using System;
using System.Globalization;
using System.Windows.Data;
using static HouseholdAccountBook.Extensions.DateTimeExtensions;

namespace HouseholdAccountBook.Converters
{
    /// <summary>
    /// DateTime - 国民の祝日変換
    /// </summary>
    public class DateTimeToIsNationalHolidayConverter : IValueConverter
    {
        /// <summary>
        /// DateTime -> 国民の祝日変換
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is DateTime dateTime ? (object)dateTime.IsNationalHoliday() : throw new NotImplementedException();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

}

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
        {
            if(value is DateTime dateTime) {
                return dateTime.IsNationalHoliday();
            }
            else {
                throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}

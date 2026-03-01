using HouseholdAccountBook.Models.Utilities.Extensions;
using System;
using System.Globalization;
using System.Windows.Data;

namespace HouseholdAccountBook.Views.Converters
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
            if (value is null) {
                return null;
            }
            else if (value is DateTime dateTime) {
                return dateTime.IsNationalHoliday();
            }
            else if (value is DateOnly dateOnly) {
                return dateOnly.IsNationalHoliday();
            }

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

}

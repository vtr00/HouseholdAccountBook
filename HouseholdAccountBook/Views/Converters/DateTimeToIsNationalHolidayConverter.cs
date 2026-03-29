using HouseholdAccountBook.Infrastructure.Utilities.Extensions;
using HouseholdAccountBook.Models.AppServices;
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
                return HolidayService.Instance.IsNationalHoliday(dateTime.ToDateOnly());
            }
            else if (value is DateOnly dateOnly) {
                return HolidayService.Instance.IsNationalHoliday(dateOnly);
            }

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}

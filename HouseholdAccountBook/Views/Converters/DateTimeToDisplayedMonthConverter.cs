using System;
using System.Globalization;
using System.Windows.Data;

namespace HouseholdAccountBook.Views.Converters
{
    /// <summary>
    /// DateTime - 表示月変換
    /// </summary>
    public class DateTimeToDisplayedMonthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) {
                return null;
            }
            else if (value is DateTime dateTime) {
                return dateTime.ToString("yyyy-MM");
            }
            else if (value is DateOnly dateOnly) {
                return dateOnly.ToString("yyyy-MM");
            }
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}

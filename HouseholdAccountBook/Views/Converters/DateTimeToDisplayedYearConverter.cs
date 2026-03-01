using System;
using System.Globalization;
using System.Windows.Data;

namespace HouseholdAccountBook.Views.Converters
{
    /// <summary>
    /// DateTime - 表示年変換
    /// </summary>
    public class DateTimeToDisplayedYearConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) {
                return null;
            }
            else if (value is DateTime dateTime) {
                return $"{dateTime:yyyy}";
            }
            else if (value is DateOnly dateOnly) {
                return $"{dateOnly:yyyy}";
            }
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

}

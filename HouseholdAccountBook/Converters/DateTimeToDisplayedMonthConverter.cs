using System;
using System.Globalization;
using System.Windows.Data;

namespace HouseholdAccountBook.Converters
{
    /// <summary>
    /// DateTime->表示月変換
    /// </summary>
    public class DateTimeToDisplayedMonthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime) {
#if DEBUG
                return dateTime.ToString("yyyy/MM/dd HH:mm:ss");
#else
                return dateTime.ToString("yyyy/MM");
#endif
            }
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}

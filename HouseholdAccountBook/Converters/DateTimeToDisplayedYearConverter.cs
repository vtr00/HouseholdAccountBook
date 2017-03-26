using System;
using System.Globalization;
using System.Windows.Data;

namespace HouseholdAccountBook.Converters
{
    /// <summary>
    /// DateTime->表示年変換
    /// </summary>
    public class DateTimeToDisplayedYearConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
#if DEBUG
            return ((DateTime)value).ToString("yyyy/MM/dd HH:mm:ss");
#else
            return ((DateTime)value).ToString("yyyy年");
#endif
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}

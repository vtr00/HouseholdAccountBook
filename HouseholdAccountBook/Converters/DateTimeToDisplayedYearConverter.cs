using System;
using System.Globalization;
using System.Windows.Data;

namespace HouseholdAccountBook.Converters
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
                Properties.Settings settings = Properties.Settings.Default;
                if (settings.App_StartMonth == 1) {
                    return dateTime.ToString("yyyy");
                } else {
                    return "'" + dateTime.ToString("yyyy");
                }

            }
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}

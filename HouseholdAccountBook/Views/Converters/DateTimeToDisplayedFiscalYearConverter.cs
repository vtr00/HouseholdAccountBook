using System;
using System.Globalization;
using System.Windows.Data;

namespace HouseholdAccountBook.Views.Converters
{
    /// <summary>
    /// DateTime - 表示年度変換
    /// </summary>
    public class DateTimeToDisplayedFiscalYearConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) {
                return null;
            }

            Properties.Settings settings = Properties.Settings.Default;
            string unit_pre = settings.App_StartMonth == 1 ? "" : Properties.Resources.Unit_FiscalYear_Pre;
            string unit_post = settings.App_StartMonth == 1 ? "" : Properties.Resources.Unit_FiscalYear_Post;

            if (value is DateTime dateTime) {
                return $"{unit_pre}{dateTime:yyyy}{unit_post}";
            }
            else if (value is DateOnly dateOnly) {
                return $"{unit_pre}{dateOnly:yyyy}{unit_post}";
            }
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}

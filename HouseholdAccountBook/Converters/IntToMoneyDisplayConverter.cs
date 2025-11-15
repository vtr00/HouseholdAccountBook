using System;
using System.Globalization;
using System.Windows.Data;

namespace HouseholdAccountBook.Converters
{
    /// <summary>
    /// int - 金額表示変換
    /// </summary>
    public class IntToMoneyDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            string param = parameter as string;

            if (value is int tmp) {
                return string.Format("{0:#,0}", param == "abs" ? Math.Abs(tmp) : tmp);
            }
            if (value is double tmp2) {
                return string.Format("{0:#,0.###}", param == "abs" ? Math.Abs(tmp2) : tmp2);
            }

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}

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
                if (param == "abs") {
                    return string.Format("{0:#,0}", Math.Abs(tmp));
                }
                else {
                    return string.Format("{0:#,0}", tmp);
                }
            }

            if (value is double tmp2) {
                if (param == "abs") {
                    return string.Format("{0:#,0.###}", Math.Abs(tmp2));
                }
                else {
                    return string.Format("{0:#,0.###}", tmp2);
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

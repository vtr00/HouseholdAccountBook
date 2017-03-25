using System;
using System.Globalization;
using System.Windows.Data;

namespace HouseholdAccountBook.Converter
{
    /// <summary>
    /// int->金額表示変換
    /// </summary>
    public class IntToMoneyDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            string param = parameter as string;

            if (param == "abs") {
                return string.Format("{0:#,0}", Math.Abs((int)value));
            }
            else {
                return string.Format("{0:#,0}", (int)value);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

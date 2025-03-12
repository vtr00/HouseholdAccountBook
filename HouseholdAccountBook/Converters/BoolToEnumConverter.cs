using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HouseholdAccountBook.Converters
{
    public class BoolToEnumConverter : IValueConverter
    {
        /// <summary>
        /// Enum -> Bool変換
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is string paramStr)) {
                return DependencyProperty.UnsetValue;
            }

            if (Enum.IsDefined(value.GetType(), value) == false) {
                return DependencyProperty.UnsetValue;
            }

            object paramValue = Enum.Parse(value.GetType(), paramStr);

            return (int)paramValue == (int)value;
        }

        /// <summary>
        /// Bool -> Enum変換
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is string paramStr)) {
                return DependencyProperty.UnsetValue;
            }

            if (!(value is bool isChecked)) {
                return DependencyProperty.UnsetValue;
            }

            return isChecked ? Enum.Parse(targetType, paramStr) : DependencyProperty.UnsetValue;
        }
    }
}


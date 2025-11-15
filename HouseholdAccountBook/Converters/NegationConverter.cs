using System;
using System.Globalization;
using System.Windows.Data;

namespace HouseholdAccountBook.Converters
{
    /// <summary>
    /// true - false変換
    /// </summary>
    public class NegationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is bool bl ? (object)!bl : throw new NotImplementedException();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is bool bl ? (object)!bl : throw new NotImplementedException();
    }
}

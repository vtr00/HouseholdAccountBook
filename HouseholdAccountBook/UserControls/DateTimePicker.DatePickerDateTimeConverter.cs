using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace HouseholdAccountBook.UserControls
{
    public partial class DateTimePicker : DatePicker
    {
        /// <summary>
        /// コンバータ
        /// </summary>
        private class DatePickerDateTimeConverter : IValueConverter
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            /// <param name="targetType"></param>
            /// <param name="parameter"></param>
            /// <param name="culture"></param>
            /// <returns></returns>
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var formatStr = ((Tuple<DateTimePicker, string>)parameter).Item2;
                var selectedDate = (DateTime?)value;
                return DateTimeToString(formatStr, selectedDate);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            /// <param name="targetType"></param>
            /// <param name="parameter"></param>
            /// <param name="culture"></param>
            /// <returns></returns>
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var tupleParam = (Tuple<DateTimePicker, string>)parameter;
                var dateStr = (string)value;
                return StringToDateTime(tupleParam.Item1, tupleParam.Item2, dateStr);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="formatStr"></param>
            /// <param name="selectedDate"></param>
            /// <returns></returns>
            public static string DateTimeToString(string formatStr, DateTime? selectedDate)
            {
                return selectedDate.HasValue ? selectedDate.Value.ToString(formatStr) : null;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="dateTimePicker"></param>
            /// <param name="formatStr"></param>
            /// <param name="dateStr"></param>
            /// <returns></returns>
            public static DateTime? StringToDateTime(DateTimePicker dateTimePicker, string formatStr, string dateStr)
            {
                var canParse = DateTime.TryParseExact(dateStr, formatStr, CultureInfo.CurrentCulture,
                                      DateTimeStyles.None, out DateTime date);

                if (!canParse) {
                    canParse = DateTime.TryParse(dateStr, CultureInfo.CurrentCulture, DateTimeStyles.None, out date);
                }

                return canParse ? date : dateTimePicker.SelectedDate;
            }
        }
    }
}

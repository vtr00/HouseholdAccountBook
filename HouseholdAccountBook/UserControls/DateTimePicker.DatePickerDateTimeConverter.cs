﻿using System;
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
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var formatStr = ((Tuple<DateTimePicker, string>)parameter).Item2;
                var selectedDate = (DateTime?)value;
                return DateTimeToString(formatStr, selectedDate);
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var tupleParam = ((Tuple<DateTimePicker, string>)parameter);
                var dateStr = (string)value;
                return StringToDateTime(tupleParam.Item1, tupleParam.Item2, dateStr);
            }

            public static string DateTimeToString(string formatStr, DateTime? selectedDate)
            {
                return selectedDate.HasValue ? selectedDate.Value.ToString(formatStr) : null;
            }

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

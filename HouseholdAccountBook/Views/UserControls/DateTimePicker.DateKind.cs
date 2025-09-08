using System.Windows.Controls;

namespace HouseholdAccountBook.Views.UserControls
{
    public partial class DateTimePicker : DatePicker
    {
        /// <summary>
        /// 日付種別
        /// </summary>
        private enum DateKind
        {
            /// <summary>
            /// 年
            /// </summary>
            Year,
            /// <summary>
            /// 月
            /// </summary>
            Month,
            /// <summary>
            /// 日
            /// </summary>
            Day
        }
    }
}

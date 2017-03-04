using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace HouseholdAccountBook.UserControls
{
    /// <summary>
    /// 表示フォーマットを指定可能な <see cref="DatePicker"/>
    /// </summary>
    /// <remarks>
    /// http://stackoverflow.com/questions/1798513/wpf-toolkit-datepicker-month-year-only
    /// http://qiita.com/st450/items/294ae366ca698e7627a5
    /// </remarks>
    public class DateTimePicker : DatePicker
    {
        /// <summary>
        /// カレンダーが開く前に選択されていたテキストボックス内の位置
        /// </summary>
        private int selectedPositionBeforeCalendarOpened;

        /// <summary>
        /// 種別
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

        /// <summary>
        /// <see cref="DateTimePicker">DateTimePicker</see> クラスの新しいインスタンスを初期化します。
        /// </summary>
        public DateTimePicker() : base()
        {
            this.Loaded += (sender, e) => {
                TextBox textBox = GetTemplateTextBox(this);
                textBox.SetValue(InputMethod.IsInputMethodEnabledProperty, false);
                textBox.TextChanged += TextBox_TextChanged;
                textBox.SelectionChanged += TextBox_SelectionChanged;
                textBox.PreviewKeyDown += TextBox_OnPreviewKeyDown;
                textBox.MouseWheel += TextBox_MouseWheel;
            };
            this.CalendarOpened += DateTimePicker_OnCalendarOpened;
            this.CalendarClosed += DateTimePicker_OnCalendarClosed;
        }

        #region 依存プロパティ
        /// <summary>
        /// <see cref="DateFormat">DateFormat</see> 依存関係プロパティを識別します。
        /// </summary>
        public static readonly DependencyProperty DateFormatProperty =
            DependencyProperty.RegisterAttached(
                PropertyName<DateTimePicker>.Get(x => x.DateFormat), 
                typeof(string), 
                typeof(DateTimePicker),
                new PropertyMetadata(OnDateFormatChanged));

        /// <summary>
        /// 表示フォーマット(yyyy,MM,ddのみ。順序、デリミタには縛りなし)
        /// </summary>
        public string DateFormat {
            get {
                return (string)this.GetValue(DateFormatProperty);
            }
            set {
                this.SetValue(DateFormatProperty, value);
            }
        }
        #endregion

        #region イベントハンドラ
        /// <summary>
        /// 末尾(日)が選択されるようにする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.SelectionStart = textBox.Text.Length;
        }

        /// <summary>
        /// 受付可能なキー入力について処理する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            DateTimePicker dateTimePicker = (DateTimePicker)textBox.TemplatedParent;

            switch (e.Key) {
                case Key.NumPad0:
                case Key.NumPad1:
                case Key.NumPad2:
                case Key.NumPad3:
                case Key.NumPad4:
                case Key.NumPad5:
                case Key.NumPad6:
                case Key.NumPad7:
                case Key.NumPad8:
                case Key.NumPad9: {
                        int input = e.Key - Key.NumPad0;
                        textBox.TextChanged -= TextBox_TextChanged;
                        TryToInputNumber(textBox, dateTimePicker, input);
                        textBox.TextChanged += TextBox_TextChanged;
                        e.Handled = true;
                    }
                    break;
                case Key.D0:
                case Key.D1:
                case Key.D2:
                case Key.D3:
                case Key.D4:
                case Key.D5:
                case Key.D6:
                case Key.D7:
                case Key.D8:
                case Key.D9: {
                        int input = e.Key - Key.D0;
                        textBox.TextChanged -= TextBox_TextChanged;
                        TryToInputNumber(textBox, dateTimePicker, input);
                        textBox.TextChanged += TextBox_TextChanged;
                        e.Handled = true;
                    }
                    break;
                case Key.Right: {
                        // 選択を右に移動する
                        textBox.SelectionChanged -= TextBox_SelectionChanged;
                        if (textBox.SelectionStart + textBox.SelectionLength != textBox.Text.Length) {
                            textBox.SelectionStart = textBox.SelectionStart + textBox.SelectionLength + 2;
                        }
                        SelectAsRange(textBox);
                        textBox.SelectionChanged += TextBox_SelectionChanged;
                        e.Handled = true;
                    }
                    break;
                case Key.Left: {
                        // 選択を左に移動する
                        textBox.SelectionChanged -= TextBox_SelectionChanged;
                        if (textBox.SelectionStart != 0) {
                            textBox.SelectionStart -= 2;
                        }
                        SelectAsRange(textBox);
                        textBox.SelectionChanged += TextBox_SelectionChanged;
                        e.Handled = true;
                    }
                    break;
                case Key.Up: {
                        // 選択している箇所の数字をインクリメントする
                        textBox.TextChanged -= TextBox_TextChanged;
                        IncrementSelectedNumber(textBox, dateTimePicker);
                        textBox.TextChanged += TextBox_TextChanged;
                        e.Handled = true;
                    }
                    break;
                case Key.Down: {
                        // 選択している箇所の数字をデクリメントする
                        textBox.TextChanged -= TextBox_TextChanged;
                        DecrementSelectedNumber(textBox, dateTimePicker);
                        textBox.TextChanged += TextBox_TextChanged;
                        e.Handled = true;
                    }
                    break;
                default:
                    // 上記以外の入力は受け付けない
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// 選択が変更されたときに「/」内をまとめて選択する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.SelectionChanged -= TextBox_SelectionChanged;
            SelectAsRange(textBox);
            textBox.SelectionChanged += TextBox_SelectionChanged;
        }
        
        /// <summary>
        /// ホイールの回転方向に合わせて選択している数字を増減する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            textBox.TextChanged -= TextBox_TextChanged;
            if (e.Delta < 0) {
                DecrementSelectedNumber(textBox, this);
            }
            else if (e.Delta > 0) {
                IncrementSelectedNumber(textBox, this);
            }
            textBox.TextChanged += TextBox_TextChanged;
        }

        /// <summary>
        /// カレンダーを開くときに、テキスト選択位置を保存する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DateTimePicker_OnCalendarOpened(object sender, RoutedEventArgs e)
        {
            var dateTimePicker = sender as DateTimePicker;
            var textBox = GetTemplateTextBox(dateTimePicker);
            selectedPositionBeforeCalendarOpened = textBox.SelectionStart;
        }

        /// <summary>
        /// カレンダーを閉じたときに、保存したテキスト選択位置を反映する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DateTimePicker_OnCalendarClosed(object sender, RoutedEventArgs e)
        {
            var dateTimePicker = (DateTimePicker)sender;
            var textBox = GetTemplateTextBox(dateTimePicker);
            textBox.SelectionStart = selectedPositionBeforeCalendarOpened;
        }
        #endregion

        /// <summary>
        /// 表示フォーマット変更時処理
        /// </summary>
        /// <param name="dobj"></param>
        /// <param name="e"></param>
        private static void OnDateFormatChanged(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
        {
            DateTimePicker dateTimePicker = (DateTimePicker)dobj;
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action<DateTimePicker>(ApplyDateFormat), dateTimePicker);
        }

        /// <summary>
        /// 表示フォーマット変更を適用する
        /// </summary>
        /// <param name="dateTimePicker">適用の対象</param>
        private static void ApplyDateFormat(DateTimePicker dateTimePicker)
        {
            Binding binding = new Binding(SelectedDateProperty.Name) {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                RelativeSource = new RelativeSource { AncestorType = typeof(DateTimePicker) },
                Converter = new DatePickerDateTimeConverter(),
                ConverterParameter = new Tuple<DateTimePicker, string>(dateTimePicker, dateTimePicker.DateFormat)
            };
            TextBox textBox = GetTemplateTextBox(dateTimePicker);
            textBox.SetBinding(TextBox.TextProperty, binding);
        }

        /// <summary>
        /// DateTimePicker内のテキストボックスを取得する
        /// </summary>
        /// <param name="control"></param>
        /// <returns>テキストボックス</returns>
        private static TextBox GetTemplateTextBox(Control control)
        {
            control.ApplyTemplate();
            return (TextBox)control.Template.FindName("PART_TextBox", control);
        }

        /// <summary>
        /// 選択している数字をインクリメントする
        /// </summary>
        /// <param name="textBox">テキストボックス</param>
        /// <param name="dateTimePicker">デートタイムピッカー</param>
        private void IncrementSelectedNumber(TextBox textBox, DateTimePicker dateTimePicker)
        {
            textBox.SelectionChanged -= TextBox_SelectionChanged;
            int start = textBox.SelectionStart;
            int length = textBox.SelectionLength;
            DateKind kind = GetKindOfSelection(textBox, dateTimePicker);
            switch (kind) {
                case DateKind.Year:
                    dateTimePicker.SelectedDate = dateTimePicker.SelectedDate.Value.AddYears(1);
                    break;
                case DateKind.Month:
                    dateTimePicker.SelectedDate = dateTimePicker.SelectedDate.Value.AddMonths(1);
                    break;
                case DateKind.Day:
                    dateTimePicker.SelectedDate = dateTimePicker.SelectedDate.Value.AddDays(1);
                    break;
            }
            string formatStr = dateTimePicker.DateFormat;
            textBox.Text = DatePickerDateTimeConverter.DateTimeToString(formatStr, dateTimePicker.SelectedDate);
            textBox.SelectionStart = start;
            textBox.SelectionLength = length;
            textBox.SelectionChanged += TextBox_SelectionChanged;
        }

        /// <summary>
        /// 選択している数字をデクリメントする
        /// </summary>
        /// <param name="textBox">テキストボックス</param>
        /// <param name="dateTimePicker">デートタイムピッカー</param>
        private void DecrementSelectedNumber(TextBox textBox, DateTimePicker dateTimePicker)
        {
            textBox.SelectionChanged -= TextBox_SelectionChanged;
            int start = textBox.SelectionStart;
            int length = textBox.SelectionLength;
            DateKind kind = GetKindOfSelection(textBox, dateTimePicker);
            switch (kind) {
                case DateKind.Year:
                    dateTimePicker.SelectedDate = dateTimePicker.SelectedDate.Value.AddYears(-1);
                    break;
                case DateKind.Month:
                    dateTimePicker.SelectedDate = dateTimePicker.SelectedDate.Value.AddMonths(-1);
                    break;
                case DateKind.Day:
                    dateTimePicker.SelectedDate = dateTimePicker.SelectedDate.Value.AddDays(-1);
                    break;
            }
            string formatStr = dateTimePicker.DateFormat;
            textBox.Text = DatePickerDateTimeConverter.DateTimeToString(formatStr, dateTimePicker.SelectedDate);
            textBox.SelectionStart = start;
            textBox.SelectionLength = length;
            textBox.SelectionChanged += TextBox_SelectionChanged;
        }

        /// <summary>
        /// 「/」内をまとめて選択する
        /// </summary>
        /// <param name="textBox">テキストボックス</param>
        private static void SelectAsRange(TextBox textBox)
        {
            string forward = textBox.Text.Substring(0, textBox.SelectionStart);
            string backward = textBox.Text.Substring(textBox.SelectionStart, textBox.Text.Length - textBox.SelectionStart);

            int start = forward.LastIndexOf('/') + 1;
            int end = forward.Length + (backward.IndexOf('/') >= 0 ? backward.IndexOf('/') : backward.Length);

            textBox.SelectionStart = start;
            textBox.SelectionLength = end - start;
        }

        /// <summary>
        /// 選択されている箇所の種類を特定する
        /// </summary>
        /// <param name="textBox">テキストボックス</param>
        /// <param name="dateTimePicker">デートタイムピッカー</param>
        /// <returns>選択されている箇所の種類</returns>
        private static DateKind GetKindOfSelection(TextBox textBox, DateTimePicker dateTimePicker)
        {
            string formatStr = dateTimePicker.DateFormat;
            int start = textBox.SelectionStart;
            int length = textBox.SelectionLength;

            string selectedFormat = formatStr.Substring(start, length);
            DateKind kind;
            switch (selectedFormat) {
                case "yyyy": {
                        kind = DateKind.Year;
                    }
                    break;
                case "MM": {
                        kind = DateKind.Month;
                    }
                    break;
                case "dd": {
                        kind = DateKind.Day;
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
            return kind;
        }

        /// <summary>
        /// 選択された箇所への数字の入力を試みる
        /// </summary>
        /// <param name="textBox">テキストボックス</param>
        /// <param name="dateTimePicker">デートタイムピッカー</param>
        /// <param name="number">入力された数値</param>
        /// <returns>入力に成功したか</returns>
        private bool TryToInputNumber(TextBox textBox, DateTimePicker dateTimePicker, int number)
        {
            textBox.SelectionChanged -= TextBox_SelectionChanged;
            bool ans = false;

            try {
                int start = textBox.SelectionStart;
                int length = textBox.SelectionLength;
                DateKind kind = GetKindOfSelection(textBox, dateTimePicker);
                DateTime dt = dateTimePicker.SelectedDate.Value;
                switch (kind) {
                    case DateKind.Year:
                        try {
                            dateTimePicker.SelectedDate = new DateTime(dt.Year % 10 + number, dt.Month, dt.Day);
                        }
                        catch (ArgumentOutOfRangeException) {
                            dateTimePicker.SelectedDate = new DateTime(number, dt.Month, dt.Day);
                        }
                        break;
                    case DateKind.Month:
                        try {
                            dateTimePicker.SelectedDate = new DateTime(dt.Year, dt.Month % 10 + number, dt.Day);
                        }
                        catch (ArgumentOutOfRangeException) {
                            dateTimePicker.SelectedDate = new DateTime(dt.Year, number, dt.Day);
                        }
                        break;
                    case DateKind.Day:
                        try {
                            dateTimePicker.SelectedDate = new DateTime(dt.Year, dt.Month, dt.Day % 10 + number);
                        }
                        catch (ArgumentOutOfRangeException) {
                            dateTimePicker.SelectedDate = new DateTime(dt.Year, dt.Month, number);
                        }
                        break;
                }
                string formatStr = dateTimePicker.DateFormat;
                textBox.Text = DatePickerDateTimeConverter.DateTimeToString(formatStr, dateTimePicker.SelectedDate);
                textBox.SelectionStart = start;
                textBox.SelectionLength = length;
                ans = true;
            }
            catch (ArgumentOutOfRangeException) { }

            textBox.SelectionChanged += TextBox_SelectionChanged;
            return ans;
        }

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
                DateTime date;
                var canParse = DateTime.TryParseExact(dateStr, formatStr, CultureInfo.CurrentCulture,
                                                      DateTimeStyles.None, out date);

                if (!canParse) {
                    canParse = DateTime.TryParse(dateStr, CultureInfo.CurrentCulture, DateTimeStyles.None, out date);
                }

                return canParse ? date : dateTimePicker.SelectedDate;
            }
        }
    }
}

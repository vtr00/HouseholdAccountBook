using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// </remarks>
    public partial class DateTimePicker : DatePicker
    {
        /// <summary>
        /// カレンダーが開く前に選択されていたテキストボックス内の位置
        /// </summary>
        private int selectedPositionBeforeCalendarOpened;

        /// <summary>
        /// この要素のデートフォーマットが変更されたときに発生します。
        /// </summary>
        public event DependencyPropertyChangedEventHandler DateFormatChanged;

        /// <summary>
        /// <see cref="DateTimePicker"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        public DateTimePicker() : base()
        {
            this.Initialized += (sender, e) => {
                TextBox textBox = GetTemplateTextBox(this);

                textBox.SetValue(InputMethod.IsInputMethodEnabledProperty, false);
                textBox.TextChanged += this.TextBox_TextChanged;
                textBox.PreviewKeyDown += this.TextBox_PreviewKeyDown;
                textBox.SelectionChanged += this.TextBox_SelectionChanged;
                textBox.MouseWheel += this.TextBox_MouseWheel;
            };
            
            this.CalendarOpened += this.DateTimePicker_CalendarOpened;
            this.CalendarClosed += this.DateTimePicker_CalendarClosed;
        }

        #region 依存関係プロパティ
        /// <summary>
        /// <see cref="DateFormat"/> 依存関係プロパティを識別します。
        /// </summary>
        #region DateFormatProperty
        public static readonly DependencyProperty DateFormatProperty = DependencyProperty.RegisterAttached(
                nameof(DateFormat),
                typeof(string),
                typeof(DateTimePicker),
                new PropertyMetadata(DateTimePicker_DateFormatChanged));
        #endregion
        /// <summary>
        /// 日付フォーマット(yyyy,MM,ddのみ。区切り文字は/または-。順序に縛りなし)
        /// </summary>
        #region DateFormat
        public string DateFormat
        {
            get => (string)this.GetValue(DateFormatProperty);
            set => this.SetValue(DateFormatProperty, value);
        }
        #endregion
        #endregion

        #region イベントハンドラ
        /// <summary>
        /// 末尾(日)が選択されるようにする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Log.Info();

            TextBox textBox = sender as TextBox;
            textBox.SelectionStart = textBox.Text.Length;
        }

        /// <summary>
        /// 受付可能なキー入力について処理する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
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
                        textBox.TextChanged -= this.TextBox_TextChanged;
                        this.TryToInputNumber(textBox, dateTimePicker, input);
                        textBox.TextChanged += this.TextBox_TextChanged;
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
                        textBox.TextChanged -= this.TextBox_TextChanged;
                        this.TryToInputNumber(textBox, dateTimePicker, input);
                        textBox.TextChanged += this.TextBox_TextChanged;
                        e.Handled = true;
                    }
                    break;
                case Key.Right: {
                        // 選択を右に移動する
                        textBox.SelectionChanged -= this.TextBox_SelectionChanged;
                        if (textBox.SelectionStart + textBox.SelectionLength != textBox.Text.Length) {
                            textBox.SelectionStart = textBox.SelectionStart + textBox.SelectionLength + 2;
                        }
                        SelectAsRange(textBox);
                        textBox.SelectionChanged += this.TextBox_SelectionChanged;
                        e.Handled = true;
                    }
                    break;
                case Key.Left: {
                        // 選択を左に移動する
                        textBox.SelectionChanged -= this.TextBox_SelectionChanged;
                        if (textBox.SelectionStart != 0) {
                            textBox.SelectionStart -= 2;
                        }
                        SelectAsRange(textBox);
                        textBox.SelectionChanged += this.TextBox_SelectionChanged;
                        e.Handled = true;
                    }
                    break;
                case Key.Up: {
                        // 選択している箇所の数字をインクリメントする
                        textBox.TextChanged -= this.TextBox_TextChanged;
                        this.IncreaceSelectedNumber(textBox, dateTimePicker);
                        textBox.TextChanged += this.TextBox_TextChanged;
                        e.Handled = true;
                    }
                    break;
                case Key.Down: {
                        // 選択している箇所の数字をデクリメントする
                        textBox.TextChanged -= this.TextBox_TextChanged;
                        this.DecreaceSelectedNumber(textBox, dateTimePicker);
                        textBox.TextChanged += this.TextBox_TextChanged;
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
        /// 選択が変更されたときに区切り文字間をまとめて選択する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.SelectionChanged -= this.TextBox_SelectionChanged;
            SelectAsRange(textBox);
            textBox.SelectionChanged += this.TextBox_SelectionChanged;
        }

        /// <summary>
        /// ホイールの回転方向に合わせて選択している数字を増減する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            textBox.TextChanged -= this.TextBox_TextChanged;
            if (e.Delta < 0) {
                this.DecreaceSelectedNumber(textBox, this);
            }
            else if (e.Delta > 0) {
                this.IncreaceSelectedNumber(textBox, this);
            }
            textBox.TextChanged += this.TextBox_TextChanged;

            e.Handled = true;
        }

        /// <summary>
        /// カレンダーを開くときに、テキスト選択位置を保存する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DateTimePicker_CalendarOpened(object sender, RoutedEventArgs e)
        {
            var dateTimePicker = sender as DateTimePicker;
            var textBox = GetTemplateTextBox(dateTimePicker);
            this.selectedPositionBeforeCalendarOpened = textBox.SelectionStart;
        }

        /// <summary>
        /// カレンダーを閉じたときに、保存したテキスト選択位置を反映する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DateTimePicker_CalendarClosed(object sender, RoutedEventArgs e)
        {
            var dateTimePicker = (DateTimePicker)sender;
            var textBox = GetTemplateTextBox(dateTimePicker);
            textBox.SelectionStart = this.selectedPositionBeforeCalendarOpened;
        }

        /// <summary>
        /// 表示フォーマット変更時処理
        /// </summary>
        /// <param name="dobj"></param>
        /// <param name="e"></param>
        private static void DateTimePicker_DateFormatChanged(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
        {
            DateTimePicker dateTimePicker = (DateTimePicker)dobj;

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action<DateTimePicker>(DateTimePicker.ApplyDateFormat), dateTimePicker);

            dateTimePicker.DateFormatChanged?.Invoke(dobj, e);
        }
        #endregion

        /// <summary>
        /// 表示フォーマット変更を適用する
        /// </summary>
        /// <param name="dateTimePicker">適用の対象</param>
        private static void ApplyDateFormat(DateTimePicker dateTimePicker)
        {
            // 選択された日付をフォーマットを適用してテキストボックスに表示する
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
        /// <see cref="DateTimePicker"/> 内のテキストボックスを取得する
        /// </summary>
        /// <param name="dateTimePicker"></param>
        /// <returns>テキストボックス</returns>
        private static TextBox GetTemplateTextBox(DateTimePicker dateTimePicker)
        {
            dateTimePicker.ApplyTemplate();
            return (TextBox)dateTimePicker.Template.FindName("PART_TextBox", dateTimePicker);
        }

        /// <summary>
        /// 選択している数字をインクリメントする
        /// </summary>
        /// <param name="textBox">テキストボックス</param>
        /// <param name="dateTimePicker">デートタイムピッカー</param>
        private void IncreaceSelectedNumber(TextBox textBox, DateTimePicker dateTimePicker)
        {
            textBox.SelectionChanged -= this.TextBox_SelectionChanged;

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

            textBox.SelectionChanged += this.TextBox_SelectionChanged;
        }

        /// <summary>
        /// 選択している数字をデクリメントする
        /// </summary>
        /// <param name="textBox">テキストボックス</param>
        /// <param name="dateTimePicker">デートタイムピッカー</param>
        private void DecreaceSelectedNumber(TextBox textBox, DateTimePicker dateTimePicker)
        {
            textBox.SelectionChanged -= this.TextBox_SelectionChanged;

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

            textBox.SelectionChanged += this.TextBox_SelectionChanged;
        }

        /// <summary>
        /// 区切り文字間をまとめて選択する
        /// </summary>
        /// <param name="textBox">テキストボックス</param>
        private static void SelectAsRange(TextBox textBox)
        {
            List<char> charList = new List<char>();
            if (-1 != textBox.Text.LastIndexOf('/')) charList.Add('/');
            if (-1 != textBox.Text.LastIndexOf('-')) charList.Add('-');
            char separetor = charList.Count == 1 ? charList[0] : '-';

            string forward = textBox.Text.Substring(0, textBox.SelectionStart);
            string backward = textBox.Text.Substring(textBox.SelectionStart, textBox.Text.Length - textBox.SelectionStart);

            int start = forward.LastIndexOf(separetor) + 1;
            int end = forward.Length + (backward.IndexOf(separetor) >= 0 ? backward.IndexOf(separetor) : backward.Length);

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
            textBox.SelectionChanged -= this.TextBox_SelectionChanged;

            bool ans = false;

            try {
                int start = textBox.SelectionStart;
                int length = textBox.SelectionLength;
                DateKind kind = GetKindOfSelection(textBox, dateTimePicker);
                DateTime dt = dateTimePicker.SelectedDate.Value;
                switch (kind) {
                    case DateKind.Year:
                        try {
                            dateTimePicker.SelectedDate = new DateTime((dt.Year % 1000) * 10 + number, dt.Month, dt.Day);
                        }
                        catch (ArgumentOutOfRangeException) {
                            dateTimePicker.SelectedDate = new DateTime(number, dt.Month, dt.Day);
                        }
                        break;
                    case DateKind.Month:
                        try {
                            dateTimePicker.SelectedDate = new DateTime(dt.Year, (dt.Month % 10) * 10 + number, dt.Day);
                        }
                        catch (ArgumentOutOfRangeException) {
                            dateTimePicker.SelectedDate = new DateTime(dt.Year, number, dt.Day);
                        }
                        break;
                    case DateKind.Day:
                        try {
                            dateTimePicker.SelectedDate = new DateTime(dt.Year, dt.Month, (dt.Day % 10) * 10 + number);
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

            textBox.SelectionChanged += this.TextBox_SelectionChanged;

            return ans;
        }
    }
}

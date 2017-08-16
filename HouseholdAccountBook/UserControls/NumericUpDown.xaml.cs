using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HouseholdAccountBook.UserControls
{
    /// <summary>
    /// NumericUpDown.xaml の相互作用ロジック
    /// </summary>
    public partial class NumericUpDown : UserControl
    {
        #region 依存関係プロパティ
        /// <summary>
        /// <see cref="Value"/> 依存関係プロパティを識別します。
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
                PropertyName<NumericUpDown>.Get(x => x.Value),
                typeof(int?),
                typeof(NumericUpDown),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
            );
        /// <summary>
        /// 値
        /// </summary>
        public int? Value
        {
            get { return (int?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// <see cref="Stride"/> 依存関係プロパティを識別します。
        /// </summary>
        public static readonly DependencyProperty StrideProperty = DependencyProperty.Register(
                PropertyName<NumericUpDown>.Get(x => x.Stride),
                typeof(int),
                typeof(NumericUpDown),
                new PropertyMetadata(1)
            );
        /// <summary>
        /// 刻み幅
        /// </summary>
        public int Stride
        {
            get { return (int)GetValue(StrideProperty); }
            set { SetValue(StrideProperty, value); }
        }

        /// <summary>
        /// <see cref="NullValue"/> 依存関係プロパティを識別します。
        /// </summary>
        public static readonly DependencyProperty NullValueProperty = DependencyProperty.Register(
            PropertyName<NumericUpDown>.Get(x => x.NullValue), 
            typeof(int), 
            typeof(NumericUpDown), 
            new PropertyMetadata(0));
        /// <summary>
        /// NULL値
        /// </summary>
        public int NullValue
        {
            get { return (int)GetValue(NullValueProperty); }
            set { SetValue(NullValueProperty, value); }
        }

        /// <summary>
        /// <see cref="MaxValue"/> 依存関係プロパティを識別します。
        /// </summary>
        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(
                PropertyName<NumericUpDown>.Get(x => x.MaxValue),
                typeof(int),
                typeof(NumericUpDown),
                new PropertyMetadata(int.MaxValue)
            );
        /// <summary>
        /// 最大値
        /// </summary>
        public int MaxValue
        {
            get { return (int)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        /// <summary>
        /// <see cref="MinValue"/> 依存関係プロパティを識別します。
        /// </summary>
        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register(
                PropertyName<NumericUpDown>.Get(x => x.MinValue),
                typeof(int),
                typeof(NumericUpDown),
                new PropertyMetadata(int.MinValue)
            );
        /// <summary>
        /// 最小値
        /// </summary>
        public int MinValue
        {
            get { return (int)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }
        #endregion

        /// <summary>
        /// <see cref="NumericUpDown"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        public NumericUpDown() 
        {
            InitializeComponent();
        }

        #region イベントハンドラ
        #region コマンド
        /// <summary>
        /// ▲ボタン押下可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IncreaseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.Value == null ? this.NullValue < this.MaxValue : this.Value < this.MaxValue;
        }

        /// <summary>
        /// ▲ボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IncreaseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            IncreaceNumber();
            e.Handled = true;
        }

        /// <summary>
        /// ▼ボタン押下可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DecreaseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.Value == null ? this.NullValue > this.MinValue : this.Value > this.MinValue;
        }

        /// <summary>
        /// ▼ボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DecreaseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DecreaceNumber();
            e.Handled = true;
        }

        /// <summary>
        /// 数字入力ボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonInputCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TextBox textBox = _textBox;

            switch (this.UVM.InputedKind) {
                case NumericInputButton.InputKind.Number:
                    int value = this.UVM.InputedValue.Value;
                    if (this.Value == null) {
                        this.Value = value;
                        textBox.SelectionStart = 1;
                    }
                    else {
                        int selectionStart = textBox.SelectionStart;
                        int selectionEnd = selectionStart + textBox.SelectionLength;
                        string forwardText = textBox.Text.Substring(0, selectionStart);
                        string selectedText = textBox.SelectedText;
                        string backwardText = textBox.Text.Substring(selectionEnd, textBox.Text.Length - selectionEnd);

                        this.Value = int.Parse(string.Format("{0}{1}{2}", forwardText, value, backwardText));
                        textBox.SelectionStart = selectionStart + 1;
                    }
                    break;
                case NumericInputButton.InputKind.BackSpace:
                    if (this.Value == 0) {
                        this.Value = null;
                    }
                    else {
                        int selectionStart = textBox.SelectionStart;
                        int selectionLength = textBox.SelectionLength;
                        int selectionEnd = selectionStart + textBox.SelectionLength;
                        string forwardText = textBox.Text.Substring(0, selectionStart);
                        string backwardText = textBox.Text.Substring(selectionEnd, textBox.Text.Length - selectionEnd);

                        if (selectionLength != 0) {
                            this.Value = int.Parse(string.Format("{0}{1}", forwardText, backwardText));
                            textBox.SelectionStart = selectionStart;
                        }
                        else if (selectionStart != 0) {
                            string newText = string.Format("{0}{1}", forwardText.Substring(0, selectionStart - 1), backwardText);
                            this.Value = string.Empty == newText ? (int?)null : int.Parse(newText);
                            textBox.SelectionStart = selectionStart - 1;
                        }
                    }
                    break;
                case NumericInputButton.InputKind.Clear:
                    this.Value = null;
                    break;
            }
            textBox.Focus();

            e.Handled = true;
        }
        #endregion

        /// <summary>
        /// 入力された値を表示前にチェックする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            bool yes_parse = false;
            if (sender != null) {
                // 既存のテキストボックス文字列に、新規に一文字追加された時、その文字列が数値として意味があるかどうかをチェック
                {
                    string tmp = textBox.Text + e.Text;
                    if (tmp == string.Empty) {
                        yes_parse = true;
                    }
                    else {
                        yes_parse = Int32.TryParse(tmp, out int xx);

                        // 範囲内かどうかチェック
                        if (yes_parse) {
                            if (xx < this.MinValue || this.MaxValue < xx) {
                                yes_parse = false;
                            }
                        }
                    }
                }
            }
            // 更新したい場合は false, 更新したくない場合は true
            e.Handled = !yes_parse;
        }

        /// <summary>
        /// Popupを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            _popup.IsOpen = true;
            _popup.Focus();
        }

        /// <summary>
        /// ホイールの回転方向に合わせて数字を増減する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) {
                IncreaceNumber();
            }
            else if (e.Delta < 0) {
                DecreaceNumber();
            }
        }
        #endregion

        /// <summary>
        /// 数値をインクリメントする
        /// </summary>
        private void IncreaceNumber() {
            if (this.Value == null) {
                this.Value = this.MinValue;
            }
            else if ((this.Value + this.Stride) <= this.MaxValue) {
                this.Value += this.Stride;
            }
            else {
                this.Value = this.MaxValue;
            }
        }

        /// <summary>
        /// 数値をデクリメントする
        /// </summary>
        private void DecreaceNumber() 
        {
            if (this.Value == null) {
                this.Value = this.MaxValue;
            }
            else if ((this.Value - this.Stride) >= this.MinValue) {
                this.Value -= this.Stride;
            }
            else {
                this.Value = this.MinValue;
            }
        }
    }
}

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
        #region ValueProperty
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
                nameof(Value),
                typeof(int?),
                typeof(NumericUpDown),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
            );
        #endregion
        /// <summary>
        /// 値
        /// </summary>
        #region Value
        public int? Value
        {
            get => (int?)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }
        #endregion

        /// <summary>
        /// <see cref="Stride"/> 依存関係プロパティを識別します。
        /// </summary>
        #region StrideProperty
        public static readonly DependencyProperty StrideProperty = DependencyProperty.Register(
                nameof(Stride),
                typeof(int),
                typeof(NumericUpDown),
                new PropertyMetadata(1)
            );
        #endregion
        /// <summary>
        /// 刻み幅
        /// </summary>
        #region Stride
        public int Stride
        {
            get => (int)this.GetValue(StrideProperty);
            set => this.SetValue(StrideProperty, value);
        }
        #endregion

        /// <summary>
        /// <see cref="NullValue"/> 依存関係プロパティを識別します。
        /// </summary>
        #region NullValueProperty
        public static readonly DependencyProperty NullValueProperty = DependencyProperty.Register(
            nameof(NullValue),
            typeof(int),
            typeof(NumericUpDown),
            new PropertyMetadata(0));
        #endregion
        /// <summary>
        /// NULL値
        /// </summary>
        #region NullValue
        public int NullValue
        {
            get => (int)this.GetValue(NullValueProperty);
            set => this.SetValue(NullValueProperty, value);
        }
        #endregion

        /// <summary>
        /// <see cref="MaxValue"/> 依存関係プロパティを識別します。
        /// </summary>
        #region MaxValueProperty
        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(
                nameof(MaxValue),
                typeof(int),
                typeof(NumericUpDown),
                new PropertyMetadata(int.MaxValue)
            );
        #endregion
        /// <summary>
        /// 最大値
        /// </summary>
        #region MaxValue
        public int MaxValue
        {
            get => (int)this.GetValue(MaxValueProperty);
            set => this.SetValue(MaxValueProperty, value);
        }
        #endregion

        /// <summary>
        /// <see cref="MinValue"/> 依存関係プロパティを識別します。
        /// </summary>
        #region MinValueProperty
        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register(
                nameof(MinValue),
                typeof(int),
                typeof(NumericUpDown),
                new PropertyMetadata(int.MinValue)
            );
        #endregion
        /// <summary>
        /// 最小値
        /// </summary>
        #region MinValue
        public int MinValue
        {
            get => (int)this.GetValue(MinValueProperty);
            set => this.SetValue(MinValueProperty, value);
        }
        #endregion
        #endregion

        /// <summary>
        /// <see cref="NumericUpDown"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        public NumericUpDown()
        {
            this.InitializeComponent();
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
            this.IncreaceNumber();
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
            this.DecreaceNumber();
            e.Handled = true;
        }

        /// <summary>
        /// 数字入力ボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonInputCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TextBox textBox = this._textBox;

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

                        if (int.TryParse(string.Format("{0}{1}{2}", forwardText, value, backwardText), out int outValue)) {
                            this.Value = outValue;
                            textBox.SelectionStart = selectionStart + 1;
                        }
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
                            if (int.TryParse(string.Format("{0}{1}", forwardText, backwardText), out int outValue)) {
                                this.Value = outValue;
                                textBox.SelectionStart = selectionStart;
                            }
                        }
                        else if (selectionStart != 0) {
                            string newText = string.Format("{0}{1}", forwardText.Substring(0, selectionStart - 1), backwardText);
                            if (string.Empty == newText || int.TryParse(newText, out int outValue)) {
                                this.Value = string.Empty == newText ? (int?)null : int.Parse(newText);
                                textBox.SelectionStart = selectionStart - 1;
                            }
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
            this.UVM.IsOpen = true;
        }

        /// <summary>
        /// ホイールの回転方向に合わせて数字を増減する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) {
                this.IncreaceNumber();
            }
            else if (e.Delta < 0) {
                this.DecreaceNumber();
            }
        }

        /// <summary>
        /// コントロールからフォーカスが外れたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumericUpDown_LostFocus(object sender, RoutedEventArgs e)
        {
            this.UVM.NumericUpDownFocused = false;
        }

        /// <summary>
        /// コントロールがフォーカスを得たとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumericUpDown_GotFocus(object sender, RoutedEventArgs e)
        {
            this.UVM.NumericUpDownFocused = true;
        }

        /// <summary>
        /// Popupからフォーカスが外れたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Popup_LostFocus(object sender, RoutedEventArgs e)
        {
            this.UVM.PopupFocused = false;
        }

        /// <summary>
        /// Popupがフォーカスを得たとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Popup_GotFocus(object sender, RoutedEventArgs e)
        {
            this.UVM.PopupFocused = true;
        }
        #endregion

        /// <summary>
        /// 数値をインクリメントする
        /// </summary>
        private void IncreaceNumber()
        {
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

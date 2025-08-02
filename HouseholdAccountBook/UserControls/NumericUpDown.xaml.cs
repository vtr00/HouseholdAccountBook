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
        /// ValueがnullのときValueを何と見做すか
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

        /// <summary>
        /// <see cref="IsAcceptableMouseWheel"/> 依存関係プロパティを識別します。
        /// </summary>
        #region IsAcceptableMouseWheelProperty
        public static readonly DependencyProperty IsAcceptableMouseWheelProperty = DependencyProperty.Register(
                nameof(IsAcceptableMouseWheel), 
                typeof(bool), 
                typeof(NumericUpDown), 
                new PropertyMetadata(true)
            );
        #endregion
        /// <summary>
        /// マウスホイールによる入力を受け付けるか
        /// </summary>
        #region IsAcceptableMouseWheel
        public bool IsAcceptableMouseWheel
        {
            get => (bool)this.GetValue(IsAcceptableMouseWheelProperty);
            set => this.SetValue(IsAcceptableMouseWheelProperty, value);
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
            this.IncreaseNumber();
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
            this.DecreaseNumber();
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

            int? tmpValue = this.Value;
            int tmpSelectionStart = textBox.SelectionStart;

            switch (this.UVM.InputedKind) {
                case NumericInputButton.InputKind.Number:
                    int value = this.UVM.InputedValue.Value;
                    if (tmpValue == null) {
                        tmpValue = value;
                        tmpSelectionStart = 1;
                    }
                    else {
                        int selectionStart = textBox.SelectionStart;
                        int selectionEnd = selectionStart + textBox.SelectionLength;
                        string forwardText = textBox.Text[..selectionStart];
                        string backwardText = textBox.Text[selectionEnd..];

                        if (int.TryParse(string.Format($"{forwardText}{value}{backwardText}"), out int outValue)) {
                            tmpValue = outValue;
                            tmpSelectionStart = selectionStart + 1;
                        }
                    }
                    break;
                case NumericInputButton.InputKind.BackSpace:
                    if (tmpValue == 0) {
                        tmpValue = null;
                    }
                    else {
                        int selectionStart = textBox.SelectionStart;
                        int selectionLength = textBox.SelectionLength;
                        int selectionEnd = selectionStart + textBox.SelectionLength;
                        string forwardText = textBox.Text[..selectionStart];
                        string backwardText = textBox.Text[selectionEnd..];

                        if (selectionLength != 0) {
                            if (int.TryParse(string.Format($"{forwardText}{backwardText}"), out int outValue)) {
                                tmpValue = outValue;
                                tmpSelectionStart = selectionStart;
                            }
                        }
                        else if (selectionStart != 0) {
                            string newText = string.Format($"{forwardText[..(selectionStart - 1)]}{backwardText}");
                            if (string.Empty == newText) {
                                tmpValue = null;
                                tmpSelectionStart = selectionStart - 1;
                            }
                            else if (int.TryParse(newText, out int outValue)) {
                                tmpValue = outValue;
                                tmpSelectionStart = selectionStart - 1;
                            }
                        }
                    }
                    break;
                case NumericInputButton.InputKind.Clear:
                    tmpValue = null;
                    break;
            }

            if ((this.MinValue <= tmpValue && tmpValue <= this.MaxValue) || tmpValue == null) {
                this.Value = tmpValue;
                textBox.SelectionStart = tmpSelectionStart;

                textBox.Focus();
            }

            e.Handled = true;
        }
        #endregion

        /// <summary>
        /// テキストボックスにテキストが入力される前
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
                        yes_parse = int.TryParse(tmp, out int xx);

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
        /// テキストボックスでホイールが回転したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!this.IsAcceptableMouseWheel) return;

            // 方向に合わせて数字を増減する
            if (e.Delta > 0) {
                this.IncreaseNumber();
            }
            else if (e.Delta < 0) {
                this.DecreaseNumber();
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
        /// <see cref="System.Windows.Controls.Primitives.Popup"/> からフォーカスが外れたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Popup_LostFocus(object sender, RoutedEventArgs e)
        {
            this.UVM.PopupFocused = false;
        }

        /// <summary>
        /// <see cref="System.Windows.Controls.Primitives.Popup"/> がフォーカスを得たとき
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
        private void IncreaseNumber()
        {
            if (this.Value == null) {
                if (this.NullValue + this.Stride <= this.MaxValue) {
                    this.Value = this.NullValue + this.Stride;
                }
            }
            else {
                if ((this.Value + this.Stride) <= this.MaxValue) {
                    this.Value += this.Stride;
                }
                else {
                    this.Value = this.MaxValue;
                }
            }
        }

        /// <summary>
        /// 数値をデクリメントする
        /// </summary>
        private void DecreaseNumber()
        {
            if (this.Value == null) {
                if (this.NullValue - this.Stride >= this.MinValue) {
                    this.Value = this.NullValue - this.Stride;
                }
            }
            else {
                if ((this.Value - this.Stride) >= this.MinValue) {
                    this.Value -= this.Stride;
                }
                else {
                    this.Value = this.MinValue;
                }
            }
        }
    }
}

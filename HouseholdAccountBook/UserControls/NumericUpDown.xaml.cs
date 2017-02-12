using HouseholdAccountBook.Windows;
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
        #region 依存プロパティ
        /// <summary>
        /// 値プロパティ
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
                PropertyName<NumericUpDown>.Get(x => x.Value),
                typeof(int?),
                typeof(NumericUpDown),
                new PropertyMetadata(null)
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
        /// 刻み幅プロパティ
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
        /// NULL値プロパティ
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
        /// 最大値プロパティ
        /// </summary>
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
                PropertyName<NumericUpDown>.Get(x => x.Maximum),
                typeof(int),
                typeof(NumericUpDown),
                new PropertyMetadata(int.MaxValue)
            );
        /// <summary>
        /// 最大値
        /// </summary>
        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        /// <summary>
        /// 最小値プロパティ
        /// </summary>
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
                PropertyName<NumericUpDown>.Get(x => x.Minimum),
                typeof(int),
                typeof(NumericUpDown),
                new PropertyMetadata(int.MinValue)
            );
        /// <summary>
        /// 最小値
        /// </summary>
        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NumericUpDown()
        {
            InitializeComponent();
        }

        #region コマンド
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IncreaseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.Value == null ? (this.NullValue + this.Stride) <= this.Maximum : (this.Value + this.Stride) <= this.Maximum;
        }

        /// <summary>
        /// ▲ボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IncreaseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.Value == null) {
                this.Value = this.NullValue;
            }
            else {
                this.Value += this.Stride;
            }
            e.Handled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DecreaseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.Value == null ? (this.NullValue - this.Stride) >= this.Minimum : (this.Value - this.Stride) >= this.Minimum;
        }

        /// <summary>
        /// ▼ボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DecreaseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.Value == null) {
                this.Value = this.NullValue;
            }
            else {
                this.Value -= this.Stride;
            }
            e.Handled = true;
        }

        /// <summary>
        /// 数字ボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumberInputCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ContentControl control = e.OriginalSource as ContentControl;
            int value = Int32.Parse(control.Content.ToString());

            if (this.Value == null) {
                this.Value = value;
            }
            else {
                this.Value = this.Value * 10 + value;
            }
            e.Handled = true;
        }

        /// <summary>
        /// BackSpaceボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackSpaceInputCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.Value != null) {
                this.Value /= 10;
            }
            if (this.Value == 0) {
                this.Value = null;
            }
            e.Handled = true;
        }

        /// <summary>
        /// Clearボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Value = null;
            e.Handled = true;
        }
        #endregion

        #region イベントハンドラ
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
                    int xx;
                    string tmp = textBox.Text + e.Text;
                    if (tmp == string.Empty) {
                        yes_parse = true;
                    }
                    else {
                        yes_parse = Int32.TryParse(tmp, out xx);

                        // 範囲内かどうかチェック
                        if (yes_parse) {
                            if (xx < this.Minimum || this.Maximum < xx) {
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
        /// 矢印キーで数値を操作する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key) {
                case Key.Up:
                    if (this.Value == null) {
                        this.Value = 0;
                    }
                    else if ((this.Value + this.Stride) <= this.Maximum) {
                        this.Value += this.Stride;
                    }
                    e.Handled = true;
                    break;
                case Key.Down:
                    if (this.Value == null) {
                        this.Value = 0;
                    }
                    else if ((this.Value - this.Stride) >= this.Minimum) {
                        this.Value -= this.Stride;
                    }
                    e.Handled = true;
                    break;
                default:
                    break;
            }
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
        #endregion

    }
}

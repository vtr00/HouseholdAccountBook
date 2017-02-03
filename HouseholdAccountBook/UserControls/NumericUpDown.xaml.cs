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
        private NumberInputWindow niw = null;

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

        #region イベントハンドラ
        /// <summary>
        /// 数値入力用ウィンドウを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            if (niw != null) {
                niw.Close();
            }
            niw = new NumberInputWindow();
            Point position = this.PointToScreen(new Point(0, this.ActualHeight));
            niw.Left = position.X;
            niw.Top = position.Y;
            niw.NumberInput += (sender2, e2) => {
                if (this.Value == null) {
                    this.Value = e2.Value;
                }
                else {
                    this.Value = this.Value * 10 + e2.Value;
                }
            };
            niw.BackSpaceInput += (sender2, e2) => {
                if (this.Value != null) {
                    this.Value /= 10;
                }
                if(this.Value == 0) {
                    this.Value = null;
                }
            };
            niw.Clear += (sender2, e2) => {
                this.Value = null;
            };
            niw.Show();
        }

        /// <summary>
        /// 数値入力用ウィンドウを非表示にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            if(niw != null) {
                niw.Close();
            }
        }

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
        private void IncreaseCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.Value == null) {
                this.Value = this.NullValue;
            }
            else {
                this.Value += this.Stride;
            }
            e.Handled = false;
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
        private void DecreaseCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.Value == null) {
                this.Value = this.NullValue;
            }
            else {
                this.Value -= this.Stride;
            }
            e.Handled = false;
        }
        #endregion
    }
}

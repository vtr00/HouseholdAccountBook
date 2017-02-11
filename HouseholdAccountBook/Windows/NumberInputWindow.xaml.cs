using HouseholdAccountBook.UserEventArgs;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HouseholdAccountBook.Windows
{
    /// <summary>
    /// NumberInputWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class NumberInputWindow : Window
    {
        #region イベントハンドラ
        /// <summary>
        /// 数字ボタンがクリックされたときに発生します。
        /// </summary>
        public event EventHandler<ValueInputEventArgs> NumberInput;

        /// <summary>
        /// BackSpaceボタンがクリックされたときに発生します。
        /// </summary>
        public event EventHandler BackSpaceInput;

        /// <summary>
        /// Clearボタンがクリックされたときに発生します。
        /// </summary>
        public event EventHandler Clear;
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NumberInputWindow()
        {
            InitializeComponent();
        }

        #region コマンド
        /// <summary>
        /// NumberInputイベントを呼び出す
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumberInput_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ContentControl control = e.OriginalSource as ContentControl;
            int number = Int32.Parse(control.Content.ToString());
            
            NumberInput?.Invoke(this, new ValueInputEventArgs(number));
        }

        /// <summary>
        /// BackSpaceInputイベントを呼び出す
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackSpaceInput_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            BackSpaceInput?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Clearイベントを呼び出す
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Clear_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Clear?.Invoke(this, new EventArgs());
        }
        #endregion

        #region イベントハンドラ
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key) {
                case Key.D0:
                case Key.D1:
                case Key.D2:
                case Key.D3:
                case Key.D4:
                case Key.D5:
                case Key.D6:
                case Key.D7:
                case Key.D8:
                case Key.D9:
                    NumberInput?.Invoke(this, new ValueInputEventArgs(e.Key - Key.D0));
                    break;
                case Key.NumPad0:
                case Key.NumPad1:
                case Key.NumPad2:
                case Key.NumPad3:
                case Key.NumPad4:
                case Key.NumPad5:
                case Key.NumPad6:
                case Key.NumPad7:
                case Key.NumPad8:
                case Key.NumPad9:
                    NumberInput?.Invoke(this, new ValueInputEventArgs(e.Key - Key.NumPad0));
                    break;
                case Key.Back:
                    BackSpaceInput?.Invoke(this, new EventArgs());
                    break;
            }
        }
        #endregion
    }
}

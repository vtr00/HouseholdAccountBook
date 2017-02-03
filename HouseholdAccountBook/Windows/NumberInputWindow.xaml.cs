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
        /// <summary>
        /// 數字ボタンがクリックされたときに発生します。
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

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NumberInputWindow()
        {
            InitializeComponent();
        }

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
    }
}

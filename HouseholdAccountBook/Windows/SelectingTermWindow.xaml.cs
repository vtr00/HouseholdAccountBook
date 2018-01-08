using HouseholdAccountBook.UserEventArgs;
using System;
using System.Windows;
using System.Windows.Input;

namespace HouseholdAccountBook.Windows
{
    /// <summary>
    /// SelectingTermWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SelectingDailyTermWindow : Window
    {
        /// <summary>
        /// <see cref="SelectingDailyTermWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">終了日</param>
        public SelectingDailyTermWindow(DateTime startDate, DateTime endDate)
        {
            InitializeComponent();

            this.WVM.StartDate = startDate;
            this.WVM.EndDate = endDate;
        }

        /// <summary>
        /// OKボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// キャンセルボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}

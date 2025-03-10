﻿using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace HouseholdAccountBook.Windows
{
    /// <summary>
    /// VersuinWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class VersionWindow : Window
    {
        /// <summary>
        /// <see cref="VersionWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        public VersionWindow()
        {
            this.InitializeComponent();
        }

        #region イベントハンドラ
        #region コマンド
        /// <summary>
        /// ウィンドウを閉じる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        /// <summary>
        /// ハイパーリンククリック時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        #region ウィンドウ
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VersionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            HistoryLog.ScrollToEnd();
        }
        #endregion
        #endregion
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HouseholdAccountBook.Extensions;

namespace HouseholdAccountBook.Windows
{
    /// <summary>
    /// TermWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class TermWindow : Window
    {
        /// <summary>
        /// <see cref="TermWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">終了日</param>
        public TermWindow(DateTime startDate, DateTime endDate)
        {
            this.InitializeComponent();

            this.WVM.StartDate = startDate;
            this.WVM.EndDate = endDate;
            this.termRadioButton.IsChecked = true;
        }

        /// <summary>
        /// <see cref="TermWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="dateWithinMonth">月内日付</param>
        public TermWindow(DateTime dateWithinMonth)
        {
            this.InitializeComponent();

            this.WVM.StartDate = dateWithinMonth.GetFirstDateOfMonth();
            this.WVM.EndDate = this.WVM.StartDate.AddMonths(1).AddMilliseconds(-1);
            this.monthRadioButton.IsChecked = true;
        }

        #region イベントハンドラ
        #region ウィンドウ
        /// <summary>
        /// ウィンドウ読込完了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TermWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // xamlで指定するとCalendarが正しく表示されないため、ここで指定する
            this.calendar.DisplayMode = CalendarMode.Year;
        }

        /// <summary>
        /// ウィンドウ終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TermWindow_Closed(object sender, EventArgs e)
        {
            this.SaveWindowSetting();
        }
        #endregion

        /// <summary>
        /// カレンダー表示モード変更時: 月選択時に開始/終了日付を確定させる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Calendar_DisplayModeChanged(object sender, CalendarModeChangedEventArgs e)
        {
            if (e.NewMode == CalendarMode.Month) {
                this.WVM.StartDate = calendar.DisplayDate.GetFirstDateOfMonth();
                this.WVM.EndDate = this.WVM.StartDate.AddMonths(1).AddMilliseconds(-1);
                this.calendar.DisplayMode = CalendarMode.Year;

                Mouse.Capture(null);
            }
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
        #endregion

        #region 設定反映用の関数
        /// <summary>
        /// ウィンドウ設定を読み込む
        /// </summary>
        public void LoadWindowSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (settings.App_IsPositionSaved && (0 <= settings.TermWindow_Left && 0 <= settings.TermWindow_Top)) {
                this.Left = settings.TermWindow_Left;
                this.Top = settings.TermWindow_Top;
            }
            else {
                this.MoveOwnersCenter();
            }
        }

        /// <summary>
        /// ウィンドウ設定を保存する
        /// </summary>
        public void SaveWindowSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (settings.App_IsPositionSaved) {
                settings.TermWindow_Left = this.Left;
                settings.TermWindow_Top = this.Top;
            }
            settings.Save();
        }
        #endregion
    }
}

using HouseholdAccountBook.Adapters.DbHandler;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Others;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HouseholdAccountBook.Views.Windows
{
    /// <summary>
    /// TermWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class TermWindow : Window
    {
        #region コンストラクタ
        /// <summary>
        /// <see cref="TermWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="dateWithinMonth">月内日付</param>
        public TermWindow(Window owner, DbHandlerFactory dbHandlerFactory, DateTime dateWithinMonth)
        {
            this.Owner = owner;
            this.Name = "Term";
            WindowLocationManager.Instance.Add(this);

            this.InitializeComponent();

            this.AddCommonEventHandlers();
            // ロード時処理はコンストラクタで設定しておく
            this.Loaded += (sender, e) => {
                // xamlで指定するとCalendarが正しく表示されないため、ここで指定する
                this.calendar.DisplayMode = CalendarMode.Year;

                this.WVM.StartDate = dateWithinMonth.GetFirstDateOfMonth();
                this.WVM.EndDate = dateWithinMonth.GetLastDateOfMonth();
                this.selectedMonthRadioButton.IsChecked = true;
            };

            this.WVM.Initialize(this.GetWaitCursorManagerFactory(), dbHandlerFactory);
        }

        /// <summary>
        /// <see cref="TermWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">終了日</param>
        public TermWindow(Window owner, DbHandlerFactory dbHandlerFactory, DateTime startDate, DateTime endDate)
        {
            this.Owner = owner;
            this.Name = "Term";
            WindowLocationManager.Instance.Add(this);

            this.InitializeComponent();

            this.AddCommonEventHandlers();
            // ロード時処理はコンストラクタで設定しておく
            this.Loaded += (sender, e) => {
                // xamlで指定するとCalendarが正しく表示されないため、ここで指定する
                this.calendar.DisplayMode = CalendarMode.Year;

                this.WVM.StartDate = startDate;
                this.WVM.EndDate = endDate;
                this.selectedTermRadioButton.IsChecked = true;
            };

            this.WVM.Initialize(this.GetWaitCursorManagerFactory(), dbHandlerFactory);
        }
        #endregion

        #region イベントハンドラ
        #region ウィンドウ
        private void TermWindow_Closed(object sender, EventArgs e)
        {
            if (this.selectedMonthRadioButton.IsChecked.Value) {
                this.WVM.StartDate = this.WVM.StartDate.GetFirstDateOfMonth();
                this.WVM.EndDate = this.WVM.StartDate.GetLastDateOfMonth();
            }
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
                this.WVM.StartDate = this.calendar.DisplayDate.GetFirstDateOfMonth();
                this.WVM.EndDate = this.calendar.DisplayDate.GetLastDateOfMonth();
                this.calendar.DisplayMode = CalendarMode.Year;

                _ = Mouse.Capture(null);
            }
        }
        #endregion
    }
}

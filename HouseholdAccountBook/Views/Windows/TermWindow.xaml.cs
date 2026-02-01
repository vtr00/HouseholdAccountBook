using HouseholdAccountBook.Adapters.DbHandlers;
using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Utilities;
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
            : this(owner, dbHandlerFactory, dateWithinMonth, null, null) => this.selectedMonthRadioButton.IsChecked = true;

        /// <summary>
        /// <see cref="TermWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">終了日</param>
        public TermWindow(Window owner, DbHandlerFactory dbHandlerFactory, DateTime startDate, DateTime endDate)
            : this(owner, dbHandlerFactory, null, startDate, endDate) => this.selectedTermRadioButton.IsChecked = true;

        /// <summary>
        /// <see cref="TermWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="dateWithinMonth">月内日付</param>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">終了日</param>
        private TermWindow(Window owner, DbHandlerFactory dbHandlerFactory, DateTime? dateWithinMonth, DateTime? startDate, DateTime? endDate)
        {
            using FuncLog funcLog = new(new { dateWithinMonth, startDate, endDate });

            this.Owner = owner;
            this.Name = UiConstants.WindowNameStr[nameof(TermWindow)];
            WindowLocationManager.Instance.Add(this);

            this.InitializeComponent();
            this.AddCommonEventHandlersToVM();

            this.WVM.Initialize(this.GetWaitCursorManagerFactory(), dbHandlerFactory);

            // ロード時処理はコンストラクタで設定しておく
            this.Loaded += (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.Loaded));

                // xamlで指定するとCalendarが正しく表示されないため、ここで指定する
                this.calendar.DisplayMode = CalendarMode.Year;

                if (dateWithinMonth.HasValue) {
                    this.WVM.StartDate = dateWithinMonth.Value.GetFirstDateOfMonth();
                    this.WVM.EndDate = dateWithinMonth.Value.GetLastDateOfMonth();
                }
                else {
                    this.WVM.StartDate = startDate.Value;
                    this.WVM.EndDate = endDate.Value;
                }
                this.WVM.AddEventHandlers();
            };
        }
        #endregion

        #region イベントハンドラ
        #region ウィンドウ
        private void TermWindow_Closed(object sender, EventArgs e)
        {
            using FuncLog funcLog = new();

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
            using FuncLog funcLog = new();

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

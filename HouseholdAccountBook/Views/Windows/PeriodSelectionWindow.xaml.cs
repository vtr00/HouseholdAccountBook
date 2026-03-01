using HouseholdAccountBook.Models.Infrastructure.DbHandlers;
using HouseholdAccountBook.Models.Infrastructure.Logger;
using HouseholdAccountBook.Models.Utilities.Extensions;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels;
using HouseholdAccountBook.Views.Extensions;
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
        public TermWindow(Window owner, DbHandlerFactory dbHandlerFactory, DateOnly dateWithinMonth)
            : this(owner, dbHandlerFactory, dateWithinMonth, null) => this.selectedMonthRadioButton.IsChecked = true;

        /// <summary>
        /// <see cref="TermWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">終了日</param>
        public TermWindow(Window owner, DbHandlerFactory dbHandlerFactory, PeriodObj<DateOnly> period)
            : this(owner, dbHandlerFactory, null, period) => this.selectedPeriodRadioButton.IsChecked = true;

        /// <summary>
        /// <see cref="TermWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="dateWithinMonth">月内日付</param>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">終了日</param>
        private TermWindow(Window owner, DbHandlerFactory dbHandlerFactory, DateOnly? dateWithinMonth, PeriodObj<DateOnly> period)
        {
            using FuncLog funcLog = new(new { dateWithinMonth, period });

            this.Owner = owner;
            this.Name = UiConstants.WindowNameStr[nameof(TermWindow)];
            WindowLocationManager.Instance.Add(this);

            this.InitializeComponent();
            this.AddCommonEventHandlersToVM();

            this.WVM.Initialize(new WaitCursorManagerFactory(this), dbHandlerFactory);

            // ロード時処理はコンストラクタで設定しておく
            this.Loaded += (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.Loaded));

                // xamlで指定するとCalendarが正しく表示されないため、ここで指定する
                this.calendar.DisplayMode = CalendarMode.Year;

                this.WVM.SelectedPeriod = dateWithinMonth.HasValue
                    ? new(dateWithinMonth.Value.GetFirstDateOfMonth(), dateWithinMonth.Value.GetLastDateOfMonth())
                    : period;
                this.WVM.AddEventHandlers();
            };
        }
        #endregion

        #region イベントハンドラ
        #region ウィンドウ
        private void PeriodSelectionWindow_Closed(object sender, EventArgs e)
        {
            using FuncLog funcLog = new();

            if (this.selectedMonthRadioButton.IsChecked.Value) {
                this.WVM.SelectedPeriod = new(this.WVM.SelectedMonth.GetFirstDateOfMonth().ToDateOnly(), this.WVM.SelectedMonth.GetLastDateOfMonth().ToDateOnly());
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
                DateOnly displayDate = this.calendar.DisplayDate.ToDateOnly();
                this.WVM.SelectedPeriod = new(displayDate.GetFirstDateOfMonth(), displayDate.GetLastDateOfMonth());
                this.calendar.DisplayMode = CalendarMode.Year;

                _ = Mouse.Capture(null);
            }
        }
        #endregion
    }
}

using HouseholdAccountBook.DbHandler;
using HouseholdAccountBook.DbHandler.Abstract;
using HouseholdAccountBook.Extensions;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.Windows
{
    /// <summary>
    /// TermWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class TermWindow : Window
    {
        #region フィールド
        /// <summary>
        /// DBハンドラファクトリ
        /// </summary>
        private readonly DbHandlerFactory dbHandlerFactory;
        /// <summary>
        /// 選択された開始日
        /// </summary>
        private readonly DateTime? selectedStartDate;
        /// <summary>
        /// 選択された終了日
        /// </summary>
        private readonly DateTime? selectedEndDate;
        /// <summary>
        /// 選択された期間種別
        /// </summary>
        private readonly TermKind selectedTermKind;
        #endregion

        #region コンストラクタ
        /// <summary>
        /// <see cref="TermWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="dateWithinMonth">月内日付</param>
        public TermWindow(DbHandlerFactory dbHandlerFactory, DateTime dateWithinMonth)
        {
            this.dbHandlerFactory = dbHandlerFactory;

            this.InitializeComponent();

            this.selectedStartDate = dateWithinMonth.GetFirstDateOfMonth();
            this.selectedEndDate = dateWithinMonth.GetLastDateOfMonth();
            this.selectedTermKind = TermKind.Monthly;
        }

        /// <summary>
        /// <see cref="TermWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">終了日</param>
        public TermWindow(DbHandlerFactory dbHandlerFactory, DateTime startDate, DateTime endDate)
        {
            this.dbHandlerFactory = dbHandlerFactory;

            this.InitializeComponent();

            this.selectedStartDate = startDate;
            this.selectedEndDate = endDate;
            this.selectedTermKind = TermKind.Selected;
        }
        #endregion

        #region イベントハンドラ
        #region コマンド
        /// <summary>
        /// 指定月選択時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckSelectedMonthCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // NOP
            // ウィンドウ終了時に開始/終了日を指定する
        }

        /// <summary>
        /// 今月選択時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ThisMonthCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.WVM.StartDate = DateTime.Today.GetFirstDateOfMonth();
            this.WVM.EndDate = DateTime.Today.GetLastDateOfMonth();
        }

        /// <summary>
        /// 指定期間選択時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckSelectedTermCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // NOP
        }

        /// <summary>
        /// 全期間選択時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AllTermCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Tuple<DateTime, DateTime> firstLastDate = await this.LoadFirstLastDate();
            this.WVM.StartDate = firstLastDate.Item1;
            this.WVM.EndDate = firstLastDate.Item2;
        }
        #endregion

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

            switch (this.selectedTermKind) {
                case TermKind.Monthly:
                    this.WVM.StartDate = this.selectedStartDate.Value;
                    this.WVM.EndDate = this.selectedEndDate.Value;
                    this.selectedMonthRadioButton.IsChecked = true;
                    break;
                case TermKind.Selected:
                    this.WVM.StartDate = this.selectedStartDate.Value;
                    this.WVM.EndDate = this.selectedEndDate.Value;
                    this.selectedTermRadioButton.IsChecked = true;
                    break;
            }
        }

        /// <summary>
        /// ウィンドウ終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TermWindow_Closed(object sender, EventArgs e)
        {
            if (this.selectedMonthRadioButton.IsChecked.Value) {
                this.WVM.StartDate = this.WVM.StartDate.GetFirstDateOfMonth();
                this.WVM.EndDate = this.WVM.StartDate.GetLastDateOfMonth();
            }

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
                this.WVM.EndDate = calendar.DisplayDate.GetLastDateOfMonth();
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

        /// <summary>
        /// 帳簿項目の初日/最終日を取得する
        /// </summary>
        /// <returns>初日/最終日のペア</returns>
        private async Task<Tuple<DateTime, DateTime>> LoadFirstLastDate()
        {
            DateTime firstTime = DateTime.Today;
            DateTime lastTime = DateTime.Today;
            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                DbReader reader = await dbHandler.ExecQueryAsync(@"
SELECT MIN(act_time) as first_time, MAX(act_time) as last_time
FROM hst_action
WHERE del_flg = 0;");
                reader.ExecARow((record) => {
                    firstTime = record.ToDateTime("first_time");
                    lastTime = record.ToDateTime("last_time");
                });
            }
            return new Tuple<DateTime, DateTime>(firstTime, lastTime);
        }
    }
}

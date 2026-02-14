using HouseholdAccountBook.Adapters;
using HouseholdAccountBook.Adapters.DbHandlers;
using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Utilities;
using HouseholdAccountBook.ViewModels.Component;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HouseholdAccountBook.Views.Windows
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        #region フィールド
        /// <summary>
        /// DBハンドラファクトリ
        /// </summary>
        private readonly DbHandlerFactory mDbHandlerFactory;

        /// <summary>
        /// 移動登録ウィンドウ
        /// </summary>
        private MoveRegistrationWindow mMRW;
        /// <summary>
        /// 項目登録ウィンドウ
        /// </summary>
        private ActionRegistrationWindow mARW;
        /// <summary>
        /// 項目リスト登録ウィンドウ
        /// </summary>
        private ActionListRegistrationWindow mALRW;
        /// <summary>
        /// CSV比較ウィンドウ
        /// </summary>
        private CsvComparisonWindow mCCW;
        #endregion

        #region プロパティ
        /// <summary>
        /// 子ウィンドウを開いているか
        /// </summary>
        private bool ChildrenWindowOpened => this.mMRW != null || this.mARW != null || this.mALRW != null || (this.mCCW != null && this.mCCW.IsVisible);
        /// <summary>
        /// 登録ウィンドウを開いているか
        /// </summary>
        private bool RegistrationWindowOpened => this.mMRW != null || this.mARW != null || this.mALRW != null;
        #endregion

        /// <summary>
        /// <see cref="MainWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="dbHandlerFactory">DAOハンドラファクトリ</param>
        public MainWindow(DbHandlerFactory dbHandlerFactory)
        {
            using FuncLog funcLog = new();

            this.mDbHandlerFactory = dbHandlerFactory;
            this.Name = UiConstants.WindowNameStr[nameof(MainWindow)];
            WindowLocationManager.Instance.Add(this);

            this.InitializeComponent();
            this.AddCommonEventHandlersToVM();
            this.AddEventHandlersToVM();

            this.WVM.Initialize(new WaitCursorManagerFactory(this), dbHandlerFactory);

            this.Loaded += async (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.Loaded));

                using (WaitCursorManager wcm = new WaitCursorManagerFactory(this).Create(methodName: nameof(this.Loaded))) {
                    await this.WVM.LoadAsync();
                }
                this.WVM.AddEventHandlers();
            };
        }

        /// <summary>
        /// WVMにイベントハンドラを追加する
        /// </summary>
        private void AddEventHandlersToVM()
        {
            using FuncLog funcLog = new();

            // スクロール要求イベントを登録する
            this.WVM.ScrollRequested += (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.WVM.ScrollRequested));

                if (e.Value < 0) {
                    this._actionDataGrid.ScrollToTop();
                }
                else {
                    this._actionDataGrid.ScrollToButtom();
                }
            };
            // 移動追加要求イベントを登録する
            this.WVM.AddMoveRequested += (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.WVM.AddMoveRequested));

                this.mMRW = new(this, e.DbHandlerFactory, e.InitialBookId, e.InitialMonth, e.InitialDate);
                this.mMRW.Registrated += e.Registered;
                this.mMRW.Closed += (sender, e) => {
                    this.mMRW = null;
                    _ = this.Activate();
                    _ = this._actionDataGrid.Focus();
                };
                this.mMRW.Show();
            };
            // 項目追加要求イベントを登録する
            this.WVM.AddActionRequested += (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.WVM.AddActionRequested));

                this.mARW = new(this, e.DbHandlerFactory, e.InitialBookId, e.InitialMonth, e.InitialDate);
                this.mARW.Registrated += e.Registered;
                this.mARW.Closed += (sender, e) => {
                    this.mARW = null;
                    _ = this.Activate();
                    _ = this._actionDataGrid.Focus();
                };
                this.mARW.Show();
            };
            // 項目リスト追加要求イベントを登録する
            this.WVM.AddActionListRequested += (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.WVM.AddActionListRequested));

                this.mALRW = new(this, e.DbHandlerFactory, e.InitialBookId, e.InitialMonth, e.InitialDate);
                this.mALRW.Registrated += e.Registered;
                this.mALRW.Closed += (sender, e) => {
                    this.mALRW = null;
                    _ = this.Activate();
                    _ = this._actionDataGrid.Focus();
                };
                this.mALRW.Show();
            };
            // 移動追加要求イベントを登録する
            this.WVM.CopyMoveRequested += (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.WVM.CopyMoveRequested));

                this.mMRW = new(this, e.DbHandlerFactory, e.TargetGroupId, RegistrationKind.Copy);
                this.mMRW.Registrated += e.Registered;
                this.mMRW.Closed += (sender, e) => {
                    this.mMRW = null;
                    _ = this.Activate();
                    _ = this._actionDataGrid.Focus();
                };
                this.mMRW.Show();
            };
            // 項目複製要求イベントを登録する
            this.WVM.CopyActionRequested += (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.WVM.CopyActionRequested));

                this.mARW = new(this, e.DbHandlerFactory, e.TargetActionId, RegistrationKind.Copy);
                this.mARW.Registrated += e.Registered;
                this.mARW.Closed += (sender, e) => {
                    this.mARW = null;
                    _ = this.Activate();
                    _ = this._actionDataGrid.Focus();
                };
                this.mARW.Show();
            };
            // 移動編集要求イベントを登録する
            this.WVM.EditMoveRequested += (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.WVM.EditMoveRequested));

                this.mMRW = new(this, e.DbHandlerFactory, e.TargetGroupId, RegistrationKind.Edit);
                this.mMRW.Registrated += e.Registered;
                this.mMRW.Closed += (sender, e) => {
                    this.mMRW = null;
                    _ = this.Activate();
                    _ = this._actionDataGrid.Focus();
                };
                this.mMRW.Show();
            };
            // 項目編集要求イベントを登録する
            this.WVM.EditActionRequested += (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.WVM.EditActionRequested));

                this.mARW = new(this, e.DbHandlerFactory, e.TargetActionId, RegistrationKind.Edit);
                this.mARW.Registrated += e.Registered;
                this.mARW.Closed += (sender, e) => {
                    this.mARW = null;
                    _ = this.Activate();
                    _ = this._actionDataGrid.Focus();
                };
                this.mARW.Show();
            };
            // 項目リスト編集要求イベントを登録する
            this.WVM.EditActionListRequested += (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.WVM.EditActionListRequested));

                this.mALRW = new(this, e.DbHandlerFactory, e.TargetGroupId);
                this.mALRW.Registrated += e.Registered;
                this.mALRW.Closed += (sender, e) => {
                    this.mALRW = null;
                    _ = this.Activate();
                    _ = this._actionDataGrid.Focus();
                };
                this.mALRW.Show();
            };
            // 期間選択要求イベントを登録する
            this.WVM.SelectTermRequested += (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.WVM.SelectTermRequested));

                TermWindow stw = null;
                switch (e.TermKind) {
                    case TermKind.Monthly:
                        stw = new TermWindow(this, e.DbHandlerFactory, e.Month.Value);
                        break;
                    case TermKind.Selected:
                        stw = new TermWindow(this, e.DbHandlerFactory, e.StartDate, e.EndDate);
                        break;
                }
                stw.SetIsModal(true);
                e.Result = stw.ShowDialog() == true;
                if (e.Result) {
                    e.StartDate = stw.WVM.StartDate;
                    e.EndDate = stw.WVM.EndDate;
                }
            };
            // 設定要求イベントを登録する
            this.WVM.SettingsRequested += (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.WVM.SettingsRequested));

                SettingsWindow sw = new(this, e.DbHandlerFactory);
                sw.SetIsModal(true);
                e.Result = sw.ShowDialog() == true;
            };
            // CSV比較要求イベントを登録する
            this.WVM.CompareCsvFileRequested += (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.WVM.CompareCsvFileRequested));

                if (this.mCCW is null) {
                    this.mCCW = new CsvComparisonWindow(this, e.DbHandlerFactory, e.InitialBookId);
                    // 帳簿項目の一致フラグ変更時のイベントを登録する
                    this.mCCW.IsMatchChanged += (sender, e) => {
                        using FuncLog funcLog = new(methodName: nameof(this.mCCW.IsMatchChanged));

                        ActionViewModel vm = this.WVM.BookTabVM.ActionVMList.FirstOrDefault(tmpVM => tmpVM.ActionId == e.Value1);
                        // UI上の表記だけを更新する
                        _ = (vm?.IsMatch = e.Value2);
                    };
                    // 帳簿項目変更時のイベントを登録する
                    this.mCCW.ActionChanged += async (sender, e) => {
                        using FuncLog funcLog = new(methodName: nameof(this.mCCW.ActionChanged));

                        using (WaitCursorManager wcm = new WaitCursorManagerFactory(this).Create()) {
                            // 帳簿一覧タブを更新する
                            await this.WVM.BookTabVM.LoadAsync(e.Value, isScroll: false, isUpdateActDateLastEdited: true);
                        }
                    };
                    // 帳簿変更時のイベントを登録する
                    this.mCCW.BookChanged += (sender, e) => {
                        using FuncLog funcLog = new(methodName: nameof(this.mCCW.BookChanged));

                        var selectedVM = this.WVM.BookVMList.FirstOrDefault(vm => vm.Id == e.NewValue);
                        if (selectedVM != null) {
                            this.WVM.SelectedBookVM = selectedVM;
                        }
                    };
                    // ウィンドウ非表示時イベントを登録する
                    this.mCCW.Hided += (sender, e) => {
                        using FuncLog funcLog = new(methodName: nameof(this.mCCW.Hided));

                        _ = this.Activate();
                        _ = this._actionDataGrid.Focus();
                    };
                }
                else {
                    this.mCCW.SetSelectedBookId(e.InitialBookId);
                }

                this.mCCW.Show();
            };
            // バージョン情報表示要求イベントを登録する
            this.WVM.ShowVersionRequested += (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.WVM.ShowVersionRequested));

                VersionWindow vw = new(this);
                vw.SetIsModal(true);
                _ = vw.ShowDialog();
            };

            this.WVM.IsChildrenWindowOpenedRequested += () => this.ChildrenWindowOpened;
            this.WVM.IsRegistrationWindowOpenedRequested += () => this.RegistrationWindowOpened;
        }

        #region イベントハンドラ
        #region ウィンドウ
        /// <summary>
        /// ウィンドウクローズ時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            using FuncLog funcLog = new();

            // 他のウィンドウを開いているときは閉じない
            if (this.ChildrenWindowOpened) {
                e.Cancel = true;
                return;
            }

            this.Hide();

            Properties.Settings settings = Properties.Settings.Default;
            DateTime? currentBackUp = settings.App_BackUpCondition == (int)BackUpCondition.Updated ? this.WVM.BookTabVM.CurrentBackUp : null;
            if (await DbBackUpManager.Instance.ExecuteAtMainWindowClosing(currentBackUp)) {
                settings.App_BackUpCurrentAtClosing = DateTime.Now;
                settings.Save();
                Log.Info($"Update BackUpCurrentAtClosing: {settings.App_BackUpCurrentAtClosing}");
            }
        }

        /// <summary>
        /// ウィンドウ状態変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MainWindow_StateChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
            DateTime? currentBackUp = settings.App_BackUpCondition == (int)BackUpCondition.Updated ? this.WVM.BookTabVM.CurrentBackUp : null;
            if (await DbBackUpManager.Instance.ExecuteAtMainWindowStateChanged(this.WindowState, currentBackUp)) {
                settings.App_BackUpCurrentAtMinimizing = DateTime.Now;
                settings.Save();
                this.WVM.BookTabVM.RaiseCurrentBackUpChanged();
                Log.Info($"Update BackUpCurrentAtMinimizing: {settings.App_BackUpCurrentAtMinimizing}");

                DbBackUpManager.Instance.BackUpCurrentAtMinimizing = settings.App_BackUpCurrentAtMinimizing;
            }
        }
        #endregion

        /// <summary>
        /// 月別一覧ダブルクリック時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MonthlyListDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            using FuncLog funcLog = new();

            if (this.WVM.SelectedTab != Tabs.MonthlyListTab) { return; }

            if (e.MouseDevice.DirectlyOver is FrameworkElement fe) {
                if (fe.Parent is DataGridCell cell) {
                    int col = cell.Column.DisplayIndex;

                    if (col is >= 1 and <= 12) {
                        // 選択された月の帳簿タブを開く
                        Log.Info($"{this.WVM.DisplayedYear:yyyy-MM-dd} + month:{col - 1}");
                        this.WVM.DisplayedMonth = this.WVM.DisplayedYear.AddMonths(col - 1);
                        Log.Info($"{this.WVM.DisplayedMonth:yyyy-MM-dd}");
                        this.WVM.SelectedTab = Tabs.BooksTab;
                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>
        /// 年別一覧ダブルクリック時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void YearlyListDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            using FuncLog funcLog = new();

            if (this.WVM.SelectedTab != Tabs.YearlyListTab) { return; }

            if (e.MouseDevice.DirectlyOver is FrameworkElement fe) {
                if (fe.Parent is DataGridCell cell) {
                    int col = cell.Column.DisplayIndex;

                    if (col is >= 1 and <= 10) {
                        // 選択された年の月別一覧タブを開く
                        Properties.Settings settings = Properties.Settings.Default;

                        Log.Info($"{this.WVM.DisplayedStartYear:yyyy-MM-dd} + year:{col - 1}");
                        this.WVM.DisplayedYear = this.WVM.DisplayedStartYear.GetFirstDateOfFiscalYear(settings.App_StartMonth).AddYears(col - 1);
                        Log.Info($"{this.WVM.DisplayedYear:yyyy-MM-dd}");
                        this.WVM.SelectedTab = Tabs.MonthlyListTab;
                        e.Handled = true;
                    }
                }
            }
        }
        #endregion
    }
}

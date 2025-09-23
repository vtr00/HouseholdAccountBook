using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Models.DbHandler;
using HouseholdAccountBook.Models.Logger;
using HouseholdAccountBook.Others;
using HouseholdAccountBook.ViewModels.Component;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static HouseholdAccountBook.Extensions.FrameworkElementExtensions;

namespace HouseholdAccountBook.Views.Windows
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        #region フィールド
        /// <summary>
        /// 移動登録ウィンドウ
        /// </summary>
        private MoveRegistrationWindow mrw;
        /// <summary>
        /// 項目登録ウィンドウ
        /// </summary>
        private ActionRegistrationWindow arw;
        /// <summary>
        /// 項目リスト登録ウィンドウ
        /// </summary>
        private ActionListRegistrationWindow alrw;
        /// <summary>
        /// CSV比較ウィンドウ
        /// </summary>
        private CsvComparisonWindow ccw;

        /// <summary>
        /// ウィンドウの境界(最終補正値)
        /// </summary>
        private Rect lastBounds = new();

        /// <summary>
        /// ウィンドウログ
        /// </summary>
        private readonly WindowLog windowLog = null;
        #endregion

        #region プロパティ
        /// <summary>
        /// 子ウィンドウを開いているか
        /// </summary>
        private bool ChildrenWindowOpened => this.mrw != null || this.arw != null || this.alrw != null || (this.ccw != null && this.ccw.IsVisible);
        /// <summary>
        /// 登録ウィンドウを開いているか
        /// </summary>
        private bool RegistrationWindowOpened => this.mrw != null || this.arw != null || this.alrw != null;
        #endregion

        /// <summary>
        /// <see cref="MainWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="dbHandlerFactory">DAOハンドラファクトリ</param>
        public MainWindow(DbHandlerFactory dbHandlerFactory)
        {
            this.windowLog = new WindowLog(this);
            this.windowLog.Log("Constructor", true);

            this.InitializeComponent();

            this.LoadWindowSetting();

            this.AddCommonEventHandlers();
            this.Loaded += async (sender, e) => {
                Log.Info();

                this.windowLog.Log("WindowLoaded", true);

                await this.WVM.LoadAsync();
                this.WVM.ScrollRequested += (sender, e) => {
                    if (e.Value < 0) {
                        this._actionDataGrid.ScrollToTop();
                    }
                    else {
                        this._actionDataGrid.ScrollToButtom();
                    }
                };
                this.WVM.AddMoveRequested += (sender, e) => {
                    this.mrw = new(this, e.DbHandlerFactory, e.BookId, e.Month, e.Date);
                    this.mrw.Registrated += e.Registered;
                    _ = this.mrw.ShowDialog();

                    this.mrw = null;
                    _ = this.Activate();
                    _ = this._actionDataGrid.Focus();

                };
                this.WVM.AddActionRequested += (sender, e) => {
                    this.arw = new(this, e.DbHandlerFactory, e.BookId, e.Month, e.Date);
                    this.arw.Registrated += e.Registered;
                    _ = this.arw.ShowDialog();

                    this.arw = null;
                    _ = this.Activate();
                    _ = this._actionDataGrid.Focus();
                };
                this.WVM.AddActionListRequested += (sender, e) => {
                    this.alrw = new(this, e.DbHandlerFactory, e.BookId, e.Month, e.Date);
                    this.alrw.Registrated += e.Registered;
                    _ = this.alrw.ShowDialog();

                    this.alrw = null;
                    _ = this.Activate();
                    _ = this._actionDataGrid.Focus();
                };
                this.WVM.CopyMoveRequested += (sender, e) => {
                    this.mrw = new(this, e.DbHandlerFactory, e.GroupId, RegistrationKind.Copy);
                    this.mrw.Registrated += e.Registered;
                    _ = this.mrw.ShowDialog();

                    this.mrw = null;
                    _ = this.Activate();
                    _ = this._actionDataGrid.Focus();
                };
                this.WVM.CopyActionRequested += (sender, e) => {
                    this.arw = new(this, e.DbHandlerFactory, e.ActionId, RegistrationKind.Copy);
                    this.arw.Registrated += e.Registered;
                    _ = this.arw.ShowDialog();

                    this.arw = null;
                    _ = this.Activate();
                    _ = this._actionDataGrid.Focus();
                };
                this.WVM.EditMoveRequested += (sender, e) => {
                    this.mrw = new(this, e.DbHandlerFactory, e.GroupId, RegistrationKind.Edit);
                    this.mrw.Registrated += e.Registered;
                    _ = this.mrw.ShowDialog();

                    this.mrw = null;
                    _ = this.Activate();
                    _ = this._actionDataGrid.Focus();
                };
                this.WVM.EditActionRequested += (sender, e) => {
                    this.arw = new(this, e.DbHandlerFactory, e.ActionId, RegistrationKind.Edit);
                    this.arw.Registrated += e.Registered;
                    _ = this.arw.ShowDialog();

                    this.arw = null;
                    _ = this.Activate();
                    _ = this._actionDataGrid.Focus();
                };
                this.WVM.EditActionListRequested += (sender, e) => {
                    this.alrw = new(this, e.DbHandlerFactory, e.GroupId, RegistrationKind.Edit);
                    this.alrw.Registrated += e.Registered;
                    _ = this.alrw.ShowDialog();

                    this.alrw = null;
                    _ = this.Activate();
                    _ = this._actionDataGrid.Focus();
                };
                this.WVM.SelectTermRequested += (sender, e) => {
                    TermWindow stw = null;
                    switch (e.TermKind) {
                        case TermKind.Monthly:
                            stw = new TermWindow(this, e.DbHandlerFactory, e.Month.Value);
                            break;
                        case TermKind.Selected:
                            stw = new TermWindow(this, e.DbHandlerFactory, e.StartDate, e.EndDate);
                            break;
                    }
                    e.Result = stw.ShowDialog() == true;
                    if (e.Result) {
                        e.StartDate = stw.WVM.StartDate;
                        e.EndDate = stw.WVM.EndDate;
                    }
                };
                this.WVM.SettingsRequested += (sender, e) => {
                    SettingsWindow sw = new(this, e.DbHandlerFactory);
                    e.Result = sw.ShowDialog() == true;
                };
                this.WVM.CompareCsvFileRequested += (sender, e) => {
                    if (this.ccw is null) {
                        this.ccw = new CsvComparisonWindow(this, e.DbHandlerFactory, e.BookId);
                        // 帳簿項目の一致フラグ変更時のイベントを登録する
                        this.ccw.IsMatchChanged += (sender2, e2) => {
                            ActionViewModel vm = this.WVM.BookTabVM.ActionVMList.FirstOrDefault(tmpVM => tmpVM.ActionId == e2.Value1);
                            if (vm != null) {
                                // UI上の表記だけを更新する
                                vm.IsMatch = e2.Value2;
                            }
                        };
                        // 帳簿項目変更時のイベントを登録する
                        this.ccw.ActionChanged += async (sender3, e3) => {
                            using (WaitCursorManager wcm = this.GetWaitCursorManagerFactory().Create()) {
                                // 帳簿一覧タブを更新する
                                await this.WVM.BookTabVM.UpdateBookTabDataAsync(isScroll: false, isUpdateActDateLastEdited: true);
                            }
                        };
                        // 帳簿変更時のイベントを登録する
                        this.ccw.BookChanged += (sender4, e4) => {
                            var selectedVM = this.WVM.BookVMList.FirstOrDefault(vm => vm.Id == e4.Value);
                            if (selectedVM != null) {
                                this.WVM.SelectedBookVM = selectedVM;
                            }
                        };
                        // ウィンドウ非表示時イベントを登録する
                        this.ccw.Hided += (sender5, e5) => {
                            _ = this.Activate();
                        };
                    }

                    this.ccw.Show();
                };
                this.WVM.ShowVersionRequested += (sender, e) => {
                    VersionWindow vw = new(this);
                    _ = vw.ShowDialog();
                };

                this.WVM.IsChildrenWindowOpenedRequested += () => this.ChildrenWindowOpened;
                this.WVM.IsRegistrationWindowOpenedRequested += () => this.RegistrationWindowOpened;
            };

            this.WVM.Initialize(this.GetWaitCursorManagerFactory(), dbHandlerFactory);

            this.Name = "Main";
        }

        #region イベントハンドラ
        #region ウィンドウ
        /// <summary>
        /// ラッパ関数の呼び出し回数
        /// </summary>
        private int wrapperCount = 0;
        /// <summary>
        /// ウィンドウのサイズと位置を変更する際のラッパ関数
        /// </summary>
        /// <param name="func">ウィンドウのサイズと位置を変更する関数</param>
        private bool ChangedLocationOrSizeWrapper(Func<bool> func)
        {
            if (this.wrapperCount == 0) {
                this.SizeChanged -= this.MainWindow_SizeChanged;
                this.LocationChanged -= this.MainWindow_LocationChanged;
            }
            this.wrapperCount++;

            bool ret = func.Invoke();

            this.wrapperCount--;
            if (this.wrapperCount == 0) {
                this.SizeChanged += this.MainWindow_SizeChanged;
                this.LocationChanged += this.MainWindow_LocationChanged;
            }

            return ret;
        }

        /// <summary>
        /// ウィンドウ初期化完了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Initialized(object sender, EventArgs e)
        {
            this.windowLog.Log("Initialized", true);

            this.lastBounds = this.RestoreBounds;
        }

        /// <summary>
        /// ウィンドウクローズ時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 他のウィンドウを開いているときは閉じない
            if (this.ChildrenWindowOpened) {
                e.Cancel = true;
                return;
            }

            this.Hide();

            Properties.Settings settings = Properties.Settings.Default;
            settings.Reload();
            if (settings.App_BackUpFlagAtClosing) {
                // 通知しても即座に終了するため通知しない
                _ = await this.WVM.CreateBackUpFileAsync();
            }
        }

        /// <summary>
        /// ウィンドウ状態変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized) {
                Properties.Settings settings = Properties.Settings.Default;
                if (settings.App_BackUpFlagAtMinimizing) {
                    Log.Info(string.Format($"BackUpCurrentAtMinimizing: {settings.App_BackUpCurrentAtMinimizing}"));
                    DateTime nextBackup = settings.App_BackUpCurrentAtMinimizing.AddMinutes(settings.App_BackUpIntervalMinAtMinimizing);
                    Log.Info(string.Format($"NextBackup: {nextBackup}"));

                    if (nextBackup <= DateTime.Now) {
                        bool result = await this.WVM.CreateBackUpFileAsync(true);
                        if (result != false) {
                            settings.App_BackUpCurrentAtMinimizing = DateTime.Now;
                            settings.Save();
                            Log.Info(string.Format($"Update BackUpCurrentAtMinimizing: {settings.App_BackUpCurrentAtMinimizing}"));
                        }
                    }
                }
            }

            this.windowLog.Log("WindowStateChanged", true);
            _ = this.ModifyLocationOrSize();
        }

        /// <summary>
        /// ウィンドウ位置変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            this.windowLog.Log("WindowLocationChanged", true);
            _ = this.ModifyLocationOrSize();
        }

        /// <summary>
        /// ウィンドウサイズ変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.windowLog.Log("WindowSizeChanged", true);
            _ = this.ModifyLocationOrSize();
        }
        #endregion

        /// <summary>
        /// 月別一覧ダブルクリック時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MonthlyListDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Log.Info();

            if (this.WVM.SelectedTab != Tabs.MonthlyListTab) return;

            if (e.MouseDevice.DirectlyOver is FrameworkElement fe) {
                if (fe.Parent is DataGridCell cell) {
                    int col = cell.Column.DisplayIndex;

                    if (1 <= col && col <= 12) {
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
            Log.Info();

            if (this.WVM.SelectedTab != Tabs.YearlyListTab) return;

            if (e.MouseDevice.DirectlyOver is FrameworkElement fe) {
                if (fe.Parent is DataGridCell cell) {
                    int col = cell.Column.DisplayIndex;

                    if (1 <= col && col <= 10) {
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

        /// <summary>
        /// ウィンドウ位置またはサイズを修正する
        /// </summary>
        private bool ModifyLocationOrSize()
        {
            bool ret = this.ChangedLocationOrSizeWrapper(() => {
                bool ret2 = true;

                /// 位置調整
                if (30000 < Math.Max(Math.Abs(this.Left), Math.Abs(this.Top))) {
                    double tmpTop = this.Top;
                    double tmpLeft = this.Left;
                    if (30000 < Math.Max(Math.Abs(this.lastBounds.Left), Math.Abs(this.lastBounds.Top))) {
                        this.Left = this.lastBounds.Left;
                        this.Top = this.lastBounds.Top;
                    }
                    else {
                        // ディスプレイの中央に移動する
                        this.MoveOwnersCenter();
                    }

                    if (tmpTop != this.Top || tmpLeft != this.Left) {
                        this.windowLog.Log("WindowLocationModified", true);
                    }
                    else {
                        this.windowLog.Log("FailedToModifyLocation", true);
                        ret2 = false;
                    }
                }

                /// サイズ調整
                if (this.Height < 40 || this.Width < 40) {
                    double tmpHeight = this.Height;
                    double tmpWidth = this.Width;
                    if (40 < this.lastBounds.Height && 40 < this.lastBounds.Width) {
                        this.Height = this.lastBounds.Height;
                        this.Width = this.lastBounds.Width;
                    }
                    else {
                        this.Height = 700;
                        this.Width = 1050;
                    }

                    if (tmpHeight != this.Height || tmpWidth != this.Width) {
                        this.windowLog.Log("WindowSizeModified", true);
                    }
                    else {
                        this.windowLog.Log("FailedToModifySize", true);
                        ret2 = false;
                    }
                }

                return ret2;
            });

            this.lastBounds = this.RestoreBounds;

            return ret;
        }
    }
}

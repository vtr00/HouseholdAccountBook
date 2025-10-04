using HouseholdAccountBook.Adapters.DbHandler;
using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Others;
using HouseholdAccountBook.ViewModels.Component;
using System;
using System.ComponentModel;
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
            this.Name = "Main";
            WindowLocationManager.Instance.Add(this);

            this.InitializeComponent();

            this.AddCommonEventHandlers();
            this.Loaded += async (sender, e) => {
                Log.Info("Loaded");

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
                    this.mrw.Closed += (sender, e) => {
                        this.mrw = null;
                        _ = this.Activate();
                        _ = this._actionDataGrid.Focus();
                    };
                    this.mrw.Show();
                };
                this.WVM.AddActionRequested += (sender, e) => {
                    this.arw = new(this, e.DbHandlerFactory, e.BookId, e.Month, e.Date);
                    this.arw.Registrated += e.Registered;
                    this.arw.Closed += (sender, e) => {
                        this.arw = null;
                        _ = this.Activate();
                        _ = this._actionDataGrid.Focus();
                    };
                    this.arw.Show();

                };
                this.WVM.AddActionListRequested += (sender, e) => {
                    this.alrw = new(this, e.DbHandlerFactory, e.BookId, e.Month, e.Date);
                    this.alrw.Registrated += e.Registered;
                    this.alrw.Closed += (sender, e) => {
                        this.alrw = null;
                        _ = this.Activate();
                        _ = this._actionDataGrid.Focus();
                    };
                    this.alrw.Show();
                };
                this.WVM.CopyMoveRequested += (sender, e) => {
                    this.mrw = new(this, e.DbHandlerFactory, e.GroupId, RegistrationKind.Copy);
                    this.mrw.Registrated += e.Registered;
                    this.mrw.Closed += (sender, e) => {
                        this.mrw = null;
                        _ = this.Activate();
                        _ = this._actionDataGrid.Focus();
                    };
                    this.mrw.Show();
                };
                this.WVM.CopyActionRequested += (sender, e) => {
                    this.arw = new(this, e.DbHandlerFactory, e.ActionId, RegistrationKind.Copy);
                    this.arw.Registrated += e.Registered;
                    this.arw.Closed += (sender, e) => {
                        this.arw = null;
                        _ = this.Activate();
                        _ = this._actionDataGrid.Focus();
                    };
                    this.arw.Show();
                };
                this.WVM.EditMoveRequested += (sender, e) => {
                    this.mrw = new(this, e.DbHandlerFactory, e.GroupId, RegistrationKind.Edit);
                    this.mrw.Registrated += e.Registered;
                    this.mrw.Closed += (sender, e) => {
                        this.mrw = null;
                        _ = this.Activate();
                        _ = this._actionDataGrid.Focus();
                    };
                    this.mrw.Show();
                };
                this.WVM.EditActionRequested += (sender, e) => {
                    this.arw = new(this, e.DbHandlerFactory, e.ActionId, RegistrationKind.Edit);
                    this.arw.Registrated += e.Registered;
                    this.arw.Closed += (sender, e) => {
                        this.arw = null;
                        _ = this.Activate();
                        _ = this._actionDataGrid.Focus();
                    };
                    this.arw.Show();
                };
                this.WVM.EditActionListRequested += (sender, e) => {
                    this.alrw = new(this, e.DbHandlerFactory, e.GroupId, RegistrationKind.Edit);
                    this.alrw.Registrated += e.Registered;
                    this.alrw.Closed += (sender, e) => {
                        this.alrw = null;
                        _ = this.Activate();
                        _ = this._actionDataGrid.Focus();
                    };
                    this.alrw.Show();
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
                        this.ccw.IsMatchChanged += (sender, e) => {
                            ActionViewModel vm = this.WVM.BookTabVM.ActionVMList.FirstOrDefault(tmpVM => tmpVM.ActionId == e.Value1);
                            if (vm != null) {
                                // UI上の表記だけを更新する
                                vm.IsMatch = e.Value2;
                            }
                        };
                        // 帳簿項目変更時のイベントを登録する
                        this.ccw.ActionChanged += async (sender, e) => {
                            using (WaitCursorManager wcm = this.GetWaitCursorManagerFactory().Create()) {
                                // 帳簿一覧タブを更新する
                                await this.WVM.BookTabVM.LoadAsync(isScroll: false, isUpdateActDateLastEdited: true);
                            }
                        };
                        // 帳簿変更時のイベントを登録する
                        this.ccw.BookChanged += (sender, e) => {
                            var selectedVM = this.WVM.BookVMList.FirstOrDefault(vm => vm.Id == e.Value);
                            if (selectedVM != null) {
                                this.WVM.SelectedBookVM = selectedVM;
                            }
                        };
                        // ウィンドウ非表示時イベントを登録する
                        this.ccw.Hided += (sender, e) => {
                            _ = this.Activate();
                            _ = this._actionDataGrid.Focus();
                        };
                    }
                    else {
                        this.ccw.WVM.SelectedBookVM = this.ccw.WVM.BookVMList.FirstOrDefault(vm => vm.Id == e.BookId);
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
            e.Cancel = true;

            // 他のウィンドウを開いているときは閉じない
            if (this.ChildrenWindowOpened) {
                return;
            }

            this.Closing -= this.MainWindow_Closing;
            this.Hide();

            Properties.Settings settings = Properties.Settings.Default;
            settings.Reload();
            if (settings.App_BackUpFlagAtClosing) {
                // 通知しても即座に終了するため通知しない
                _ = await this.WVM.CreateBackUpFileAsync();
            }

            this.Close();
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
    }
}

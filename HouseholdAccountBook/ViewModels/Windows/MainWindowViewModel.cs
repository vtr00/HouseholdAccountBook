﻿using HouseholdAccountBook.Adapters.Dao.Abstract;
using HouseholdAccountBook.Adapters.Dao.DbTable;
using HouseholdAccountBook.Adapters.Dao.KHDbTable;
using HouseholdAccountBook.Adapters.DbHandler;
using HouseholdAccountBook.Adapters.DbHandler.Abstract;
using HouseholdAccountBook.Adapters.Dto.Abstract;
using HouseholdAccountBook.Adapters.Dto.DbTable;
using HouseholdAccountBook.Adapters.Dto.KHDbTable;
using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.DbHandler;
using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Others;
using HouseholdAccountBook.Others.RequestEventArgs;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using HouseholdAccountBook.ViewModels.WindowsParts;
using Notification.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static HouseholdAccountBook.Adapters.FileConstants;
using static HouseholdAccountBook.Views.UiConstants;

namespace HouseholdAccountBook.ViewModels.Windows
{
    /// <summary>
    /// メインウィンドウVM
    /// </summary>
    public class MainWindowViewModel : WindowViewModelBase
    {
        #region フィールド
        /// <summary>
        /// 表示日付の更新中か
        /// </summary>
        private bool onUpdateDisplayedDate = false;
        #endregion

        #region イベント
        /// <summary>
        /// タブ選択変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<int>> SelectedTabChanged = default;
        /// <summary>
        /// 帳簿選択変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<int?>> SelectedBookChanged = default;
        /// <summary>
        /// グラフ種別1選択変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<GraphKind1>> SelectedGraphKind1Changed = default;
        /// <summary>
        /// グラフ種別2選択変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<GraphKind2>> SelectedGraphKind2Changed = default;
        /// <summary>
        /// 系列選択変更時イベント
        /// </summary>
        public event EventHandler SelectedSeriesChanged
        {
            add {
                this.MonthlySummaryTabVM.SelectedSeriesChanged += value;
                this.YearlySummaryTabVM.SelectedSeriesChanged += value;
                this.DailyGraphTabVM.SelectedSeriesChanged += value;
                this.MonthlyGraphTabVM.SelectedSeriesChanged += value;
                this.YearlyGraphTabVM.SelectedSeriesChanged += value;
            }
            remove {
                this.MonthlySummaryTabVM.SelectedSeriesChanged -= value;
                this.YearlySummaryTabVM.SelectedSeriesChanged -= value;
                this.DailyGraphTabVM.SelectedSeriesChanged -= value;
                this.MonthlyGraphTabVM.SelectedSeriesChanged -= value;
                this.YearlyGraphTabVM.SelectedSeriesChanged -= value;
            }
        }

        /// <summary>
        /// スクロール要求時イベント
        /// </summary>
        public event EventHandler<EventArgs<int>> ScrollRequested
        {
            add => this.BookTabVM.ScrollRequested += value;
            remove => this.BookTabVM.ScrollRequested -= value;
        }

        /// <summary>
        /// 移動追加要求時イベント
        /// </summary>
        public event EventHandler<AddMoveRequestEventArgs> AddMoveRequested
        {
            add => this.BookTabVM.AddMoveRequested += value;
            remove => this.BookTabVM.AddMoveRequested -= value;
        }
        /// <summary>
        /// 帳簿項目追加要求時イベント
        /// </summary>
        public event EventHandler<AddActionRequestEventArgs> AddActionRequested
        {
            add => this.BookTabVM.AddActionRequested += value;
            remove => this.BookTabVM.AddActionRequested -= value;
        }
        /// <summary>
        /// 帳簿項目リスト追加要求時イベント
        /// </summary>
        public event EventHandler<AddActionListRequestEventArgs> AddActionListRequested
        {
            add => this.BookTabVM.AddActionListRequested += value;
            remove => this.BookTabVM.AddActionListRequested -= value;
        }
        /// <summary>
        /// 移動複製要求時イベント
        /// </summary>
        public event EventHandler<CopyMoveRequestEventArgs> CopyMoveRequested
        {
            add => this.BookTabVM.CopyMoveRequested += value;
            remove => this.BookTabVM.CopyMoveRequested -= value;
        }
        /// <summary>
        /// 帳簿項目複製要求時イベント
        /// </summary>
        public event EventHandler<CopyActionRequestEventArgs> CopyActionRequested
        {
            add => this.BookTabVM.CopyActionRequested += value;
            remove => this.BookTabVM.CopyActionRequested -= value;
        }
        /// <summary>
        /// 移動編集要求時イベント
        /// </summary>
        public event EventHandler<EditMoveRequestEventArgs> EditMoveRequested
        {
            add => this.BookTabVM.EditMoveRequested += value;
            remove => this.BookTabVM.EditMoveRequested -= value;
        }
        /// <summary>
        /// 帳簿項目編集要求時イベント
        /// </summary>
        public event EventHandler<EditActionRequestEventArgs> EditActionRequested
        {
            add => this.BookTabVM.EditActionRequested += value;
            remove => this.BookTabVM.EditActionRequested -= value;
        }
        /// <summary>
        /// 帳簿項目リスト編集要求時イベント
        /// </summary>
        public event EventHandler<EditActionListRequestEventArgs> EditActionListRequested
        {
            add => this.BookTabVM.EditActionListRequested += value;
            remove => this.BookTabVM.EditActionListRequested -= value;
        }
        /// <summary>
        /// 期間選択要求時イベント
        /// </summary>
        public event EventHandler<SelectTermRequestEventArgs> SelectTermRequested;
        /// <summary>
        /// 設定要求時イベント
        /// </summary>
        public event EventHandler<SettingsRequestEventArgs> SettingsRequested;
        /// <summary>
        /// CSVファイル比較要求時コマンド
        /// </summary>
        public event EventHandler<CompareCsvFileRequestEventArgs> CompareCsvFileRequested;
        /// <summary>
        /// バージョン表示要求時コマンド
        /// </summary>
        public event EventHandler ShowVersionRequested;

        /// <summary>
        /// 子ウィンドウオープン確認要求イベント
        /// </summary>
        public event Func<bool> IsChildrenWindowOpenedRequested;
        /// <summary>
        /// 登録ウィンドウオープン確認要求イベント
        /// </summary>
        public event Func<bool> IsRegistrationWindowOpenedRequested;
        #endregion

        #region プロパティ
        #region プロパティ(共通)
        /// <summary>
        /// 選択されたDB種別
        /// </summary>
        #region SelectedDBKind
        public DBKind SelectedDBKind
        {
            get => this._SelectedDBKind;
            set {
                if (this.SetProperty(ref this._SelectedDBKind, value)) {
                    this.RaisePropertyChanged(nameof(this.IsPostgreSQL));
                    this.RaisePropertyChanged(nameof(this.IsSQLite));
                }
            }
        }
        private DBKind _SelectedDBKind = default;
        #endregion

        /// <summary>
        /// PostgreSQLか
        /// </summary>
        #region IsPostgreSQL
        public bool IsPostgreSQL => this.SelectedDBKind == DBKind.PostgreSQL;
        #endregion
        /// <summary>
        /// SQLiteか
        /// </summary>
        #region IsSQLite
        public bool IsSQLite => this.SelectedDBKind == DBKind.SQLite;
        #endregion

        /// <summary>
        /// 選択されたタブインデックス
        /// </summary>
        #region SelectedTabIndex
        public int SelectedTabIndex
        {
            get => this._SelectedTabIndex;
            set {
                var oldValue = this._SelectedTabIndex;
                if (this.SetProperty(ref this._SelectedTabIndex, value)) {
                    this.SelectedTabChanged?.Invoke(this, new() { OldValue = oldValue, NewValue = value });
                    this.RaisePropertyChanged(nameof(this.DisplayedStart));
                    this.RaisePropertyChanged(nameof(this.DisplayedEnd));
                }
            }
        }
        private int _SelectedTabIndex = default;
        #endregion
        /// <summary>
        /// 選択されたタブ種別
        /// </summary>
        #region SelectedTab
        public Tabs SelectedTab
        {
            get => (Tabs)this._SelectedTabIndex;
            set => this.SelectedTabIndex = (int)value;
        }
        #endregion

        /// <summary>
        /// 帳簿VMリスト
        /// </summary>
        #region BookVMList
        public ObservableCollection<BookViewModel> BookVMList
        {
            get => this._BookVMList;
            set => this.SetProperty(ref this._BookVMList, value);
        }
        private ObservableCollection<BookViewModel> _BookVMList = default;
        #endregion
        /// <summary>
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookViewModel SelectedBookVM
        {
            get => this._SelectedBookVM;
            set {
                var oldVM = this._SelectedBookVM;
                if (this.SetProperty(ref this._SelectedBookVM, value)) {
                    this.SelectedBookChanged?.Invoke(this, new() { OldValue = oldVM?.Id, NewValue = value?.Id });
                }
            }
        }
        private BookViewModel _SelectedBookVM;
        #endregion

        /// <summary>
        /// 選択された収支種別
        /// </summary>
        public int? SelectedBalanceKind { get; set; } = default;
        /// <summary>
        /// 選択された分類ID
        /// </summary>
        public int? SelectedCategoryId { get; set; } = default;
        /// <summary>
        /// 選択された項目ID
        /// </summary>
        public int? SelectedItemId { get; set; } = default;

        /// <summary>
        /// 表示開始
        /// </summary>
        public DateTime DisplayedStart => this.SelectedTab switch {
            Tabs.BooksTab or Tabs.DailyGraphTab => this.StartDate,
            Tabs.MonthlyListTab or Tabs.MonthlyGraphTab => this.DisplayedStartMonth,
            Tabs.YearlyGraphTab or Tabs.YearlyListTab => this.DisplayedStartYear,
            _ => this.StartDate,
        };
        /// <summary>
        /// 表示終了
        /// </summary>
        public DateTime DisplayedEnd => this.SelectedTab switch {
            Tabs.BooksTab or Tabs.DailyGraphTab => this.EndDate,
            Tabs.MonthlyListTab or Tabs.MonthlyGraphTab => this.DisplayedEndMonth,
            Tabs.YearlyGraphTab or Tabs.YearlyListTab => this.DisplayedEndYear,
            _ => this.EndDate,
        };

        /// <summary>
        /// 会計開始月
        /// </summary>
        #region FiscalStartMonth
        public int FiscalStartMonth
        {
            get => this._fiscalStartMonth;
            set {
                if (this.SetProperty(ref this._fiscalStartMonth, value)) {
                    this.RaisePropertyChanged(nameof(this.DisplayedStart));
                    this.RaisePropertyChanged(nameof(this.DisplayedEnd));
                    this.RaisePropertyChanged(nameof(this.DisplayedMonth));
                    this.RaisePropertyChanged(nameof(this.DisplayedMonths));
                    this.RaisePropertyChanged(nameof(this.DisplayedYear));
                    this.RaisePropertyChanged(nameof(this.DisplayedYears));
                }
            }
        }
        private int _fiscalStartMonth = 4;
        #endregion

        /// <summary>
        /// 表示区間種別
        /// </summary>
        #region DisplayedTermKind
        public TermKind DisplayedTermKind
        {
            get {
                DateTime lastDate = this.StartDate.GetLastDateOfMonth();
                return (this.StartDate.Day == 1 && DateTime.Equals(this.EndDate.Date, lastDate.Date)) ? TermKind.Monthly : TermKind.Selected;
            }
        }
        #endregion

        /// <summary>
        /// 表示月
        /// </summary>
        #region DisplayedMonth
        public DateTime? DisplayedMonth
        {
            get => this.DisplayedTermKind switch {
                TermKind.Monthly => (DateTime?)this.StartDate,
                TermKind.Selected => null,
                _ => null,
            };
            set {
                if (this.DisplayedMonth != value && value is not null) {
                    // 開始日/終了日を更新する
                    this.StartDate = value.Value.GetFirstDateOfMonth();
                    this.EndDate = value.Value.GetLastDateOfMonth();

                    if (!this.onUpdateDisplayedDate) {
                        this.onUpdateDisplayedDate = true;
                        // 表示月の年度の最初の月を表示年とする
                        this.DisplayedYear = value.Value.GetFirstDateOfFiscalYear(this.FiscalStartMonth);
                        this.onUpdateDisplayedDate = false;
                    }

                    this.RaisePropertyChanged(nameof(this.DisplayedStart));
                    this.RaisePropertyChanged(nameof(this.DisplayedEnd));
                    this.RaisePropertyChanged(nameof(this.DisplayedMonth));
                    this.RaisePropertyChanged(nameof(this.DisplayedMonths));
                    this.RaisePropertyChanged(nameof(this.DisplayedYear));
                    this.RaisePropertyChanged(nameof(this.DisplayedYears));
                }
            }
        }
        #endregion

        /// <summary>
        /// 表示開始日
        /// </summary>
        #region StartDate
        public DateTime StartDate
        {
            get => this._StartDate;
            set {
                if (this.SetProperty(ref this._StartDate, value)) {
                    this.RaisePropertyChanged(nameof(this.DisplayedMonth));
                }
            }
        }
        private DateTime _StartDate = DateTime.Now.GetFirstDateOfMonth();
        #endregion
        /// <summary>
        /// 表示終了日
        /// </summary>
        #region EndDate
        public DateTime EndDate
        {
            get => this._EndDate;
            set {
                if (this.SetProperty(ref this._EndDate, value)) {
                    this.RaisePropertyChanged(nameof(this.DisplayedMonth));
                }
            }
        }
        private DateTime _EndDate = DateTime.Now.GetLastDateOfMonth();
        #endregion

        /// <summary>
        /// 表示年
        /// </summary>
        #region DisplayedYear
        public DateTime DisplayedYear
        {
            get => this._DisplayedYear;
            set {
                DateTime oldDisplayedYear = this._DisplayedYear;
                if (this.SetProperty(ref this._DisplayedYear, value)) {
                    if (!this.onUpdateDisplayedDate) {
                        this.onUpdateDisplayedDate = true;
                        int yearDiff = value.GetFirstDateOfFiscalYear(this.FiscalStartMonth).Year - oldDisplayedYear.GetFirstDateOfFiscalYear(this.FiscalStartMonth).Year;
                        if (this.DisplayedMonth != null) {
                            // 表示年の差分を表示月に反映する
                            this.DisplayedMonth = this.DisplayedMonth.Value.AddYears(yearDiff);
                        }
                        this.onUpdateDisplayedDate = false;
                    }

                    this.RaisePropertyChanged(nameof(this.DisplayedStart));
                    this.RaisePropertyChanged(nameof(this.DisplayedEnd));
                    this.RaisePropertyChanged(nameof(this.DisplayedMonths));
                    this.RaisePropertyChanged(nameof(this.DisplayedYears));
                }
            }
        }
        private DateTime _DisplayedYear = DateTime.Now;
        #endregion

        /// <summary>
        /// 表示月リスト(月別一覧の月)
        /// </summary>
        #region DisplayedMonths
        public ObservableCollection<DateTime> DisplayedMonths
        {
            get {
                DateTime tmpMonth = this.DisplayedYear.GetFirstDateOfFiscalYear(this.FiscalStartMonth);

                // 表示する月の文字列を作成する
                ObservableCollection<DateTime> displayedMonths = [];
                for (int i = 0; i < 12; ++i) {
                    displayedMonths.Add(tmpMonth);
                    tmpMonth = tmpMonth.AddMonths(1);
                }
                return displayedMonths;
            }
        }
        #endregion

        /// <summary>
        /// 表示開始月
        /// </summary>
        public DateTime DisplayedStartMonth => this.DisplayedMonths.First();
        /// <summary>
        /// 表示終了月
        /// </summary>
        public DateTime DisplayedEndMonth => this.DisplayedMonths.Last();

        /// <summary>
        /// 表示年リスト(年別一覧の年)
        /// </summary>
        #region DisplayedYears
        public ObservableCollection<DateTime> DisplayedYears
        {
            get {
                DateTime tmpYear = this.DisplayedYear.GetFirstDateOfFiscalYear(this.FiscalStartMonth).AddYears(-9);
                ObservableCollection<DateTime> displayedYears = [];
                for (int i = 0; i < 10; ++i) {
                    displayedYears.Add(tmpYear);
                    tmpYear = tmpYear.AddYears(1);
                }
                return displayedYears;
            }
        }
        #endregion

        /// <summary>
        /// 表示開始年
        /// </summary>
        public DateTime DisplayedStartYear => this.DisplayedYears.First();
        /// <summary>
        /// 表示終了年
        /// </summary>
        public DateTime DisplayedEndYear => this.DisplayedYears.Last();
        #endregion

        #region プロパティ(グラフ共通)
        /// <summary>
        /// グラフ種別1辞書
        /// </summary>
        #region GraphKind1Dic
        public Dictionary<GraphKind1, string> GraphKind1Dic { get; } = GraphKind1Str;
        #endregion
        /// <summary>
        /// 選択されたグラフ種別1
        /// </summary>
        #region SelectedGraphKind1
        public GraphKind1 SelectedGraphKind1
        {
            get => this._SelectedGraphKind1;
            set {
                var oldValue = this._SelectedGraphKind1;
                if (this.SetProperty(ref this._SelectedGraphKind1, value)) {
                    this.SelectedGraphKind1Changed?.Invoke(this, new() { OldValue = oldValue, NewValue = value });
                }
            }
        }
        private GraphKind1 _SelectedGraphKind1 = default;
        #endregion
        /// <summary>
        /// 選択されたグラフ種別1インデックス
        /// </summary>
        #region SelectedGraphKind1Index
        public int SelectedGraphKind1Index
        {
            get => (int)this._SelectedGraphKind1;
            set => this.SelectedGraphKind1 = (GraphKind1)value;
        }
        #endregion

        /// <summary>
        /// グラフ種別2辞書
        /// </summary>
        #region GraphKind2Dic
        public Dictionary<GraphKind2, string> GraphKind2Dic { get; } = GraphKind2Str;
        #endregion
        /// <summary>
        /// 選択されたグラフ種別2
        /// </summary>
        #region SelectedGraphKind2
        public GraphKind2 SelectedGraphKind2
        {
            get => this._SelectedGraphKind2;
            set {
                var oldValue = this._SelectedGraphKind2;
                if (this.SetProperty(ref this._SelectedGraphKind2, value)) {
                    this.SelectedGraphKind2Changed?.Invoke(this, new() { OldValue = oldValue, NewValue = value });
                }
            }
        }
        private GraphKind2 _SelectedGraphKind2 = default;
        #endregion
        /// <summary>
        /// 選択されたグラフ種別2インデックス
        /// </summary>
        #region SelectedGraphKind2Index
        public int SelectedGraphKind2Index
        {
            get => (int)this._SelectedGraphKind2;
            set => this.SelectedGraphKind2 = (GraphKind2)value;
        }
        #endregion
        #endregion

        #region タブVM
        /// <summary>
        /// 帳簿タブVM
        /// </summary>
        public MainWindowBookTabViewModel BookTabVM { get; private set; }
        /// <summary>
        /// 日別グラフタブVM
        /// </summary>
        public MainWindowGraphTabViewModel DailyGraphTabVM { get; private set; }
        /// <summary>
        /// 月別一覧タブVM
        /// </summary>
        public MainWindowListTabViewModel MonthlySummaryTabVM { get; private set; }
        /// <summary>
        /// 月別グラフタブVM
        /// </summary>
        public MainWindowGraphTabViewModel MonthlyGraphTabVM { get; private set; }
        /// <summary>
        /// 年別一覧タブVM
        /// </summary>
        public MainWindowListTabViewModel YearlySummaryTabVM { get; private set; }
        /// <summary>
        /// 年別グラフタブVM
        /// </summary>
        public MainWindowGraphTabViewModel YearlyGraphTabVM { get; private set; }
        #endregion

        #region コマンド
        #region ファイルコマンド
        /// <summary>
        /// ファイルコマンド
        /// </summary>
        public ICommand FileMenuCommand => new RelayCommand(null, this.FileMenuCommand_CanExecute);
        /// <summary>
        /// 記帳風月インポートコマンド
        /// </summary>
        public ICommand ImportKichoFugetsuDbCommand => new RelayCommand(this.ImportKichoFugetsuDbCommand_Executed, this.ImportKichoFugetsuDbCommand_CanExecute);
        /// <summary>
        /// PostgreSQLファイルインポートコマンド
        /// </summary>
        public ICommand ImportPostgreSQLFileCommand => new RelayCommand(this.ImportPostgreSQLFileCommand_Executed, this.ImportPostgreSQLFileCommand_CanExecute);
        /// <summary>
        /// カスタムファイルインポートコマンド
        /// </summary>
        public ICommand ImportCustomFileCommand => new RelayCommand(this.ImportCustomFileCommand_Executed, this.ImportCustomFileCommand_CanExecute);
        /// <summary>
        /// SQLiteファイルインポートコマンド
        /// </summary>
        public ICommand ImportSQLiteFileCommand => new RelayCommand(this.ImportSQLiteFileCommand_Executed, this.ImportSQLiteFileCommand_CanExecute);
        /// <summary>
        /// カスタムファイルエクスポートコマンド
        /// </summary>
        public ICommand ExportCustomFileCommand => new RelayCommand(this.ExportCustomFileCommand_Executed, this.ExportCustomFileCommand_CanExecute);
        /// <summary>
        /// SQLファイルエクスポートコマンド
        /// </summary>
        public ICommand ExportSQLFileCommand => new RelayCommand(this.ExportSQLFileCommand_Executed, this.ExportSQLFileCommand_CanExecute);
        /// <summary>
        /// Excelファイルエクスポートコマンド
        /// </summary>
        public ICommand ExportExcelFileCommand => new RelayCommand(this.ExportExcelFileCommand_Executed, this.ExportExcelFileCommand_CanExecute);
        /// <summary>
        /// バックアップコマンド
        /// </summary>
        public ICommand BackUpCommand => new RelayCommand(this.BackUpCommand_Executed, this.BackUpCommand_CanExecute);
        /// <summary>
        /// ウィンドウ終了コマンド
        /// </summary>
        public ICommand ExitWindowCommand => new RelayCommand(this.ExitWindowCommand_Executed, this.ExitWindowCommand_CanExecute);
        #endregion

        #region 表示コマンド
        /// <summary>
        /// 表示コマンド
        /// </summary>
        public ICommand ShowMenuCommand => new RelayCommand(null, this.ShowMenuCommand_CanExecute);
        /// <summary>
        /// 帳簿項目タブ表示コマンド
        /// </summary>
        public ICommand ShowBookTabCommand => new RelayCommand(this.ShowBookTabCommand_Executed, this.ShowBookTabCommand_CanExecute);
        /// <summary>
        /// 日別グラフタブ表示コマンド
        /// </summary>
        public ICommand ShowDailyGraphTabCommand => new RelayCommand(this.ShowDailyGraphTabCommand_Executed, this.ShowDailyGraphTabCommand_CanExecute);
        /// <summary>
        /// 月別リストタブ表示コマンド
        /// </summary>
        public ICommand ShowMonthlyListTabCommand => new RelayCommand(this.ShowMonthlyListTabCommand_Executed, this.ShowMonthlyListTabCommand_CanExecute);
        /// <summary>
        /// 月別グラフタブ表示コマンド
        /// </summary>
        public ICommand ShowMonthlyGraphTabCommand => new RelayCommand(this.ShowMonthlyGraphTabCommand_Executed, this.ShowMonthlyGraphTabCommand_CanExecute);
        /// <summary>
        /// 年別リストタブ表示コマンド
        /// </summary>
        public ICommand ShowYearlyListTabCommand => new RelayCommand(this.ShowYearlyListTabCommand_Executed, this.ShowYearlyListTabCommand_CanExecute);
        /// <summary>
        /// 年別グラフタブ表示コマンド
        /// </summary>
        public ICommand ShowYearlyGraphTabCommand => new RelayCommand(this.ShowYearlyGraphTabCommand_Executed, this.ShowYearlyGraphTabCommand_CanExecute);
        /// <summary>
        /// 先月表示コマンド
        /// </summary>
        public ICommand GoToLastMonthCommand => new RelayCommand(this.GoToLastMonthCommand_Executed, this.GoToLastMonthCommand_CanExecute);
        /// <summary>
        /// 期間選択コマンド
        /// </summary>
        public ICommand SelectTermCommand => new RelayCommand(this.SelectTermCommand_Executed, this.SelectTermCommand_CanExecute);
        /// <summary>
        /// 翌月表示コマンド
        /// </summary>
        public ICommand GoToNextMonthCommand => new RelayCommand(this.GoToNextMonthCommand_Executed, this.GoToNextMonthCommand_CanExecute);
        /// <summary>
        /// 今月表示コマンド
        /// </summary>
        public ICommand GoToThisMonthCommand => new RelayCommand(this.GoToThisMonthCommand_Executed, this.GoToThisMonthCommand_CanExecute);
        /// <summary>
        /// 前年表示コマンド
        /// </summary>
        public ICommand GoToLastYearCommand => new RelayCommand(this.GoToLastYearCommand_Executed, this.GoToLastYearCommand_CanExecute);
        /// <summary>
        /// 翌年表示コマンド
        /// </summary>
        public ICommand GoToNextYearCommand => new RelayCommand(this.GoToNextYearCommand_Executed, this.GoToNextYearCommand_CanExecute);
        /// <summary>
        /// 今年表示コマンド
        /// </summary>
        public ICommand GoToThisYearCommand => new RelayCommand(this.GoToThisYearCommand_Executed, this.GoToThisYearCommand_CanExecute);
        /// <summary>
        /// 更新コマンド
        /// </summary>
        public ICommand UpdateCommand => new RelayCommand(this.UpdateCommand_Executed);
        #endregion

        #region ツールコマンド
        /// <summary>
        /// ツールメニューコマンド
        /// </summary>
        public ICommand ToolMenuCommand => new RelayCommand(null, this.ToolCommand_CanExecute);
        /// <summary>
        /// 設定コマンド
        /// </summary>
        public ICommand SettingsCommand => new RelayCommand(this.SettingsCommand_Executed, this.SettingsWindowCommand_CanExecute);
        /// <summary>
        /// 帳簿内ツールコマンド
        /// </summary>
        public ICommand ToolInBookCommand => new RelayCommand(null, this.ToolInBookCommand_CanExecute);
        /// <summary>
        /// CSVファイル比較コマンド
        /// </summary>
        public ICommand CompareCsvFileCommand => new RelayCommand(this.CompareCsvFileCommand_Executed, this.CompareCsvFileCommand_CanExecute);
        #endregion

        #region ヘルプコマンド
        /// <summary>
        /// ヘルプメニューコマンド
        /// </summary>
        public ICommand HelpMenuCommand => new RelayCommand(null, this.HelpMenuCommand_CanExecute);
        /// <summary>
        /// バージョン表示コマンド
        /// </summary>
        public ICommand ShowVersionCommand => new RelayCommand(this.OpenVersionWindowCommand_Executed, this.OpenVersionWindowCommand_CanExecute);
        #endregion
        #endregion
        #endregion

        #region イベントハンドラ
        #region ファイルメニュー
        /// <summary>
        /// ファイルコマンド実行可能か
        /// </summary>
        /// <returns></returns>
        public bool FileMenuCommand_CanExecute() => !this.IsChildrenWindowOpened();

        /// <summary>
        /// 記帳風月インポートコマンド実行可能か
        /// </summary>
        /// <returns></returns>
        public bool ImportKichoFugetsuDbCommand_CanExecute() => !this.IsChildrenWindowOpened();
        /// <summary>
        /// 記帳風月インポートコマンド処理
        /// </summary>
        public async void ImportKichoFugetsuDbCommand_Executed()
        {
            Properties.Settings settings = Properties.Settings.Default;
            (string directory, string fileName) = PathExtensions.GetSeparatedPath(settings.App_Import_KichoFugetsu_FilePath, App.GetCurrentDir());

            var e = new OpenFileDialogRequestEventArgs() {
                InitialDirectory = directory,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = Properties.Resources.FileSelectFilter_MdbFile + "|*.mdb"
            };
            if (!this.OpenFileDialogRequest(e)) return;

            if (MessageBox.Show(Properties.Resources.Message_DeleteOldDataNotification, Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) != MessageBoxResult.OK) {
                return;
            }

            settings.App_Import_KichoFugetsu_FilePath = e.FileName;
            settings.Save();

            bool isOpen = false;
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                OleDbHandler.ConnectInfo info = new() {
                    Provider = settings.App_Access_Provider,
                    FilePath = e.FileName
                };

                await using (OleDbHandler dbHandlerOle = await new DbHandlerFactory(info).CreateAsync() as OleDbHandler)
                await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                    isOpen = dbHandlerOle.IsOpen;

                    if (isOpen) {
                        static async Task Mapping<DTO1, DTO2>(IReadTableDao<DTO1> src, IWriteTableDao<DTO2> dest, Func<IEnumerable<DTO1>, IEnumerable<DTO2>> converter) where DTO1 : DtoBase where DTO2 : DtoBase
                        {
                            var srcDtoList = await src.FindAllAsync();
                            _ = await dest.DeleteAllAsync();
                            if (srcDtoList is IEnumerable<ISequentialIDDto> srcSeqDtoList && dest is ISequentialIDDao<ISequentialIDDto> destSeq) {
                                // シーケンスを更新する
                                await destSeq.SetIdSequenceAsync(srcSeqDtoList);
                            }
                            _ = await dest.BulkInsertAsync(converter(srcDtoList));
                        }

                        await dbHandler.ExecTransactionAsync(async () => {
                            CbmBookDao cbmBookDao = new(dbHandlerOle);
                            MstBookDao mstBookDao = new(dbHandler);
                            await Mapping(cbmBookDao, mstBookDao, src => src.Select(dto => new MstBookDto(dto)));

                            CbmCategoryDao cbmCategoryDao = new(dbHandlerOle);
                            MstCategoryDao mstCategoryDao = new(dbHandler);
                            await Mapping(cbmCategoryDao, mstCategoryDao, src => src.Select(dto => new MstCategoryDto(dto)));

                            CbmItemDao cbmItemDao = new(dbHandlerOle);
                            MstItemDao mstItemDao = new(dbHandler);
                            await Mapping(cbmItemDao, mstItemDao, src => src.Select(dto => new MstItemDto(dto)));

                            CbtActDao cbtActDao = new(dbHandlerOle);
                            HstActionDao hstActionDao = new(dbHandler);
                            await Mapping(cbtActDao, hstActionDao, src => src.Select(dto => new HstActionDto(dto)));

                            HstGroupDao hstGroupDao = new(dbHandler);
                            var cbtActDtoList = await cbtActDao.FindAllAsync();
                            _ = await hstGroupDao.DeleteAllAsync();
                            int maxGroupId = 0; // グループIDの最大値
                            IEnumerable<HstGroupDto> hstGroupDtoList = [];
                            foreach (CbtActDto cbtActDto in cbtActDtoList) {
                                // groupId が存在しないなら次のレコードへ
                                if (cbtActDto.GROUP_ID == 0) { continue; }
                                int groupId = cbtActDto.GROUP_ID;

                                // groupId のレコードが存在しないとき
                                if (!hstGroupDtoList.Any(dto => dto.GroupId == groupId)) {
                                    // グループIDの最大値を更新する
                                    if (maxGroupId < groupId) { maxGroupId = groupId; }

                                    // グループの種類を調べる
                                    var cbtActDtoList2 = cbtActDtoList.Where(dto => dto.GROUP_ID == groupId);
                                    GroupKind groupKind = GroupKind.Repeat;
                                    int? tmpBookId = null;
                                    foreach (CbtActDto cbtActDto2 in cbtActDtoList2) {
                                        if (tmpBookId == null) { // 1つ目のレコードの帳簿IDを記録する
                                            tmpBookId = cbtActDto2.BOOK_ID;
                                        }
                                        else { // 2つ目のレコードの帳簿IDが1つ目と一致するか
                                            if (tmpBookId != cbtActDto2.BOOK_ID) {
                                                // 帳簿が一致しない場合は移動
                                                groupKind = GroupKind.Move;
                                            }
                                            else {
                                                // 帳簿が一致する場合は繰り返し
                                                groupKind = GroupKind.Repeat;
                                            }
                                            break;
                                        }
                                    }

                                    // グループテーブルのレコードを追加する
                                    hstGroupDtoList = hstGroupDtoList.Append(new HstGroupDto {
                                        GroupId = groupId,
                                        GroupKind = (int)groupKind
                                    });
                                }
                            }
                            await hstGroupDao.SetIdSequenceAsync(maxGroupId);
                            _ = await hstGroupDao.BulkInsertAsync(hstGroupDtoList);

                            HstShopDao hstShopDao = new(dbHandler);
                            _ = await hstShopDao.DeleteAllAsync();

                            CbtNoteDao cbtNoteDao = new(dbHandlerOle);
                            HstRemarkDao hstRemarkDao = new(dbHandler);
                            await Mapping(cbtNoteDao, hstRemarkDao, src => src.Select(dto => new HstRemarkDto(dto)));

                            CbrBookDao cbrBookDao = new(dbHandlerOle);
                            RelBookItemDao relBookItemDao = new(dbHandler);
                            await Mapping(cbrBookDao, relBookItemDao, src => src.Select(dto => new RelBookItemDto(dto)));
                        });
                    }
                }

                if (isOpen) {
                    await this.UpdateAsync(isUpdateBookList: true, isScroll: true, isUpdateActDateLastEdited: true);
                }
            }

            if (isOpen) {
                _ = MessageBox.Show(Properties.Resources.Message_FinishToImport, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
            else {
                _ = MessageBox.Show(Properties.Resources.Message_FoultToImport, Properties.Resources.Title_Conformation, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        /// <summary>
        /// PostgreSQLファイルインポートコマンド実行可能か
        /// </summary>
        /// <returns></returns>
        public bool ImportPostgreSQLFileCommand_CanExecute() => this.IsSQLite && !this.IsChildrenWindowOpened();
        /// <summary>
        /// PostgreSQLファイルインポートコマンド処理
        /// </summary>
        public async void ImportPostgreSQLFileCommand_Executed()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (MessageBox.Show(Properties.Resources.Message_DeleteOldDataNotification, Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) != MessageBoxResult.OK) {
                return;
            }

            bool result = false;
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                NpgsqlDbHandler.ConnectInfo info = new() {
                    Host = settings.App_Postgres_Host,
                    Port = settings.App_Postgres_Port,
                    UserName = settings.App_Postgres_UserName,
                    Password = settings.App_Postgres_Password,
#if DEBUG
                    DatabaseName = settings.App_Postgres_DatabaseName_Debug,
#else
                    DatabaseName = settings.App_Postgres_DatabaseName,
#endif
                    Role = settings.App_Postgres_Role
                };

                await using (NpgsqlDbHandler dbHandlerNpgsql = await new DbHandlerFactory(info).CreateAsync() as NpgsqlDbHandler)
                await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                    result = dbHandlerNpgsql.IsOpen;

                    if (result) {
                        static async Task Mapping<DTO>(IReadTableDao<DTO> src, IWriteTableDao<DTO> dest) where DTO : DtoBase
                        {
                            var srcDtoList = await src.FindAllAsync();
                            _ = await dest.DeleteAllAsync();
                            if (srcDtoList is IEnumerable<ISequentialIDDto> srcSeqDtoList && dest is ISequentialIDDao<ISequentialIDDto> destSeq) {
                                // シーケンスを更新する
                                await destSeq.SetIdSequenceAsync(srcSeqDtoList);
                            }
                            _ = await dest.BulkInsertAsync(srcDtoList);
                        }

                        await dbHandler.ExecTransactionAsync(async () => {
                            MstBookDao mstBookDao1 = new(dbHandlerNpgsql);
                            MstBookDao mstBookDao2 = new(dbHandler);
                            await Mapping(mstBookDao1, mstBookDao2);

                            MstCategoryDao mstCategoryDao1 = new(dbHandlerNpgsql);
                            MstCategoryDao mstCategoryDao2 = new(dbHandler);
                            await Mapping(mstCategoryDao1, mstCategoryDao2);

                            MstItemDao mstItemDao1 = new(dbHandlerNpgsql);
                            MstItemDao mstItemDao2 = new(dbHandler);
                            await Mapping(mstItemDao1, mstItemDao2);

                            HstActionDao hstActionDao1 = new(dbHandlerNpgsql);
                            HstActionDao hstActionDao2 = new(dbHandler);
                            await Mapping(hstActionDao1, hstActionDao2);

                            HstGroupDao hstGroupDao1 = new(dbHandlerNpgsql);
                            HstGroupDao hstGroupDao2 = new(dbHandler);
                            await Mapping(hstGroupDao1, hstGroupDao2);

                            HstShopDao hstShopDao1 = new(dbHandlerNpgsql);
                            HstShopDao hstShopDao2 = new(dbHandler);
                            await Mapping(hstShopDao1, hstShopDao2);

                            HstRemarkDao hstRemarkDao1 = new(dbHandlerNpgsql);
                            HstRemarkDao hstRemarkDao2 = new(dbHandler);
                            await Mapping(hstRemarkDao1, hstRemarkDao2);

                            RelBookItemDao relBookItemDao1 = new(dbHandlerNpgsql);
                            RelBookItemDao relBookItemDao2 = new(dbHandler);
                            await Mapping(relBookItemDao1, relBookItemDao2);
                        });
                    }
                }

                if (result) {
                    await this.UpdateAsync(isUpdateBookList: true, isScroll: true, isUpdateActDateLastEdited: true);
                }
            }

            if (result) {
                _ = MessageBox.Show(Properties.Resources.Message_FinishToImport, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
            else {
                _ = MessageBox.Show(Properties.Resources.Message_FoultToImport, Properties.Resources.Title_Conformation, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        /// <summary>
        /// カスタムファイルインポートコマンド実行可能か
        /// </summary>
        /// <returns></returns>
        public bool ImportCustomFileCommand_CanExecute()
        {
            Properties.Settings settings = Properties.Settings.Default;
            return this.IsPostgreSQL && settings.App_Postgres_RestoreExePath != string.Empty && !this.IsChildrenWindowOpened();
        }
        /// <summary>
        /// カスタムファイルインポートコマンド処理
        /// </summary>
        public async void ImportCustomFileCommand_Executed()
        {
            Properties.Settings settings = Properties.Settings.Default;
            (string directory, string fileName) = PathExtensions.GetSeparatedPath(settings.App_Import_CustomFormat_FilePath, App.GetCurrentDir());

            var e = new OpenFileDialogRequestEventArgs() {
                InitialDirectory = directory,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = Properties.Resources.FileSelectFilter_CustomFormatFile + "|*.*"
            };

            if (!this.OpenFileDialogRequest(e)) return;

            if (MessageBox.Show(Properties.Resources.Message_DeleteOldDataNotification, Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) != MessageBoxResult.OK) {
                return;
            }

            settings.App_Import_CustomFormat_FilePath = e.FileName;
            settings.Save();

            int exitCode = -1;
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                    var mstBookDao = new MstBookDao(dbHandler);
                    var mstCategoryDao = new MstCategoryDao(dbHandler);
                    var mstItemDao = new MstItemDao(dbHandler);
                    var hstActionDao = new HstActionDao(dbHandler);
                    var hstGroupDao = new HstGroupDao(dbHandler);
                    var hstShopDao = new HstShopDao(dbHandler);
                    var hstRemarkDao = new HstRemarkDao(dbHandler);
                    var relBookItemDao = new RelBookItemDao(dbHandler);

                    // 既存のデータを削除する
                    _ = await mstBookDao.DeleteAllAsync();
                    _ = await mstCategoryDao.DeleteAllAsync();
                    _ = await mstItemDao.DeleteAllAsync();
                    _ = await hstActionDao.DeleteAllAsync();
                    _ = await hstGroupDao.DeleteAllAsync();
                    _ = await hstShopDao.DeleteAllAsync();
                    _ = await hstRemarkDao.DeleteAllAsync();
                    _ = await relBookItemDao.DeleteAllAsync();
                }

                exitCode = await ExecuteRestorePostgreSQL(this.dbHandlerFactory, e.FileName);

                if (exitCode == 0) {
                    await this.UpdateAsync(isUpdateBookList: true, isScroll: true, isUpdateActDateLastEdited: true);
                }
            }

            if (exitCode == 0) {
                _ = MessageBox.Show(Properties.Resources.Message_FinishToImport, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
            else {
                _ = MessageBox.Show(Properties.Resources.Message_FoultToImport, Properties.Resources.Title_Conformation, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        /// <summary>
        /// SQLiteファイルインポートコマンド実行可能か
        /// </summary>
        /// <returns></returns>
        public bool ImportSQLiteFileCommand_CanExecute() => (this.IsPostgreSQL || this.IsSQLite) && !this.IsChildrenWindowOpened();
        /// <summary>
        /// SQLiteファイルインポートコマンド処理
        /// </summary>
        public async void ImportSQLiteFileCommand_Executed()
        {
            Properties.Settings settings = Properties.Settings.Default;
            (string directory, string fileName) = PathExtensions.GetSeparatedPath(settings.App_SQLite_DBFilePath, App.GetCurrentDir());

            OpenFileDialogRequestEventArgs e = new() {
                InitialDirectory = directory,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = Properties.Resources.FileSelectFilter_SQLiteFile + "|*.db;*.sqlite;*.sqlite3"
            };

            if (!this.OpenFileDialogRequest(e)) return;

            if (MessageBox.Show(Properties.Resources.Message_DeleteOldDataNotification, Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) != MessageBoxResult.OK) {
                return;
            }

            settings.App_Import_SQLite_FilePath = e.FileName;
            settings.Save();

            bool result = false;
            switch (settings.App_SelectedDBKind) {
                case (int)DBKind.SQLite: {
                    try {
                        // ファイルをコピーするだけ
                        File.Copy(e.FileName, settings.App_SQLite_DBFilePath, true);
                        result = true;
                    }
                    catch (Exception) { }
                    break;
                }
                case (int)DBKind.PostgreSQL: {
                    using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                        SQLiteDbHandler.ConnectInfo info = new() {
                            FilePath = e.FileName
                        };
                        await using (SQLiteDbHandler dbHandlerSqlite = await new DbHandlerFactory(info).CreateAsync() as SQLiteDbHandler)
                        await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                            result = dbHandlerSqlite.IsOpen;

                            if (result) {
                                static async Task Mapping<DTO>(IReadTableDao<DTO> src, IWriteTableDao<DTO> dest) where DTO : DtoBase
                                {
                                    var srcDtoList = await src.FindAllAsync();
                                    _ = await dest.DeleteAllAsync();
                                    if (srcDtoList is IEnumerable<ISequentialIDDto> srcSeqDtoList && dest is ISequentialIDDao<ISequentialIDDto> destSeq) {
                                        // シーケンスを更新する
                                        await destSeq.SetIdSequenceAsync(srcSeqDtoList);
                                    }
                                    _ = await dest.BulkInsertAsync(srcDtoList);
                                }

                                await dbHandler.ExecTransactionAsync(async () => {
                                    MstBookDao mstBookDao1 = new(dbHandlerSqlite);
                                    MstBookDao mstBookDao2 = new(dbHandler);
                                    await Mapping(mstBookDao1, mstBookDao2);

                                    MstCategoryDao mstCategoryDao1 = new(dbHandlerSqlite);
                                    MstCategoryDao mstCategoryDao2 = new(dbHandler);
                                    await Mapping(mstCategoryDao1, mstCategoryDao2);

                                    MstItemDao mstItemDao1 = new(dbHandlerSqlite);
                                    MstItemDao mstItemDao2 = new(dbHandler);
                                    await Mapping(mstItemDao1, mstItemDao2);

                                    HstActionDao hstActionDao1 = new(dbHandlerSqlite);
                                    HstActionDao hstActionDao2 = new(dbHandler);
                                    await Mapping(hstActionDao1, hstActionDao2);

                                    HstGroupDao hstGroupDao1 = new(dbHandlerSqlite);
                                    HstGroupDao hstGroupDao2 = new(dbHandler);
                                    await Mapping(hstGroupDao1, hstGroupDao2);

                                    HstShopDao hstShopDao1 = new(dbHandlerSqlite);
                                    HstShopDao hstShopDao2 = new(dbHandler);
                                    await Mapping(hstShopDao1, hstShopDao2);

                                    HstRemarkDao hstRemarkDao1 = new(dbHandlerSqlite);
                                    HstRemarkDao hstRemarkDao2 = new(dbHandler);
                                    await Mapping(hstRemarkDao1, hstRemarkDao2);

                                    RelBookItemDao relBookItemDao1 = new(dbHandlerSqlite);
                                    RelBookItemDao relBookItemDao2 = new(dbHandler);
                                    await Mapping(relBookItemDao1, relBookItemDao2);
                                });
                            }
                        }
                    }
                    break;
                }
                default:
                    break;
            }

            if (result) {
                await this.UpdateAsync(isUpdateBookList: true, isScroll: true, isUpdateActDateLastEdited: true);
            }

            if (result) {
                _ = MessageBox.Show(Properties.Resources.Message_FinishToImport, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
            else {
                _ = MessageBox.Show(Properties.Resources.Message_FoultToImport, Properties.Resources.Title_Conformation, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        /// <summary>
        /// カスタムファイルエクスポートコマンド実行可能か
        /// </summary>
        /// <returns></returns>
        public bool ExportCustomFileCommand_CanExecute()
        {
            Properties.Settings settings = Properties.Settings.Default;
            return this.IsPostgreSQL && settings.App_Postgres_DumpExePath != string.Empty && !this.IsChildrenWindowOpened();
        }
        /// <summary>
        /// カスタムファイルエクスポートコマンド処理
        /// </summary>
        public async void ExportCustomFileCommand_Executed()
        {
            Properties.Settings settings = Properties.Settings.Default;
            (string directory, string fileName) = PathExtensions.GetSeparatedPath(settings.App_Export_CustomFormat_FilePath, App.GetCurrentDir());

            SaveFileDialogRequestEventArgs e = new() {
                InitialDirectory = directory,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = Properties.Resources.FileSelectFilter_CustomFormatFile + "|*.*"
            };
            if (!this.SaveFileDialogRequest(e)) return;

            settings.App_Export_CustomFormat_FilePath = e.FileName;
            settings.Save();

            int? exitCode = -1;
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                exitCode = await ExecuteDumpPostgreSQL(this.dbHandlerFactory, e.FileName, PostgresFormat.Custom);
            }

            if (exitCode == 0) {
                _ = MessageBox.Show(Properties.Resources.Message_FinishToExport, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
            else {
                _ = MessageBox.Show(Properties.Resources.Message_FoultToExport, Properties.Resources.Title_Conformation, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        /// <summary>
        /// SQLファイルエクスポートコマンド実行可能か
        /// </summary>
        /// <returns></returns>
        public bool ExportSQLFileCommand_CanExecute()
        {
            Properties.Settings settings = Properties.Settings.Default;
            return this.IsPostgreSQL && settings.App_Postgres_DumpExePath != string.Empty && !this.IsChildrenWindowOpened();
        }
        /// <summary>
        /// SQLファイルエクスポートコマンド処理
        /// </summary>
        public async void ExportSQLFileCommand_Executed()
        {
            Properties.Settings settings = Properties.Settings.Default;

            (string directory, string fileName) = PathExtensions.GetSeparatedPath(settings.App_Export_SQLFilePath, App.GetCurrentDir());

            SaveFileDialogRequestEventArgs e = new() {
                InitialDirectory = directory,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = Properties.Resources.FileSelectFilter_SqlFile + "|*.sql"
            };
            if (!this.SaveFileDialogRequest(e)) return;

            settings.App_Export_SQLFilePath = e.FileName;
            settings.Save();

            int? exitCode = -1;
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                exitCode = await ExecuteDumpPostgreSQL(this.dbHandlerFactory, e.FileName, PostgresFormat.Plain);
            }

            if (exitCode == 0) {
                _ = MessageBox.Show(Properties.Resources.Message_FinishToExport, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
            else {
                _ = MessageBox.Show(Properties.Resources.Message_FoultToExport, Properties.Resources.Title_Conformation, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        /// <summary>
        /// Excelファイルエクスポートコマンド実行可能か
        /// </summary>
        /// <returns></returns>
#pragma warning disable CA1822 // メンバーを static に設定します
        public bool ExportExcelFileCommand_CanExecute() => false;
#pragma warning restore CA1822 // メンバーを static に設定します
        /// <summary>
        /// Excelファイルエクスポートコマンド処理
        /// </summary>
        public void ExportExcelFileCommand_Executed()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// バックアップコマンド実行可能か
        /// </summary>
        /// <returns></returns>
        public bool BackUpCommand_CanExecute() => !this.IsChildrenWindowOpened();
        /// <summary>
        /// バックアップコマンド処理
        /// </summary>
        public async void BackUpCommand_Executed()
        {
            bool result = false;
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                result = await this.CreateBackUpFileAsync();
            }

            if (result) {
                _ = MessageBox.Show(Properties.Resources.Message_FinishToBackup, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
            else {
                _ = MessageBox.Show(Properties.Resources.Message_FoultToBackup, Properties.Resources.Title_Conformation, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        /// <summary>
        /// ウィンドウ終了コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        public bool ExitWindowCommand_CanExecute() => !this.IsChildrenWindowOpened();
        /// <summary>
        /// ウィンドウ終了コマンド処理
        /// </summary>
        public void ExitWindowCommand_Executed() => this.CloseRequest(new CloseRequestEventArgs(true));
        #endregion

        #region 表示メニュー
        /// <summary>
        /// 表示メニューコマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool ShowMenuCommand_CanExecute() => true;

        #region タブ切替
        /// <summary>
        /// 帳簿タブ表示コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool ShowBookTabCommand_CanExecute() => this.SelectedTab != Tabs.BooksTab;
        /// <summary>
        /// 帳簿タブ表示コマンド処理
        /// </summary>
        private void ShowBookTabCommand_Executed() => this.SelectedTab = Tabs.BooksTab;

        /// <summary>
        /// 日別グラフタブ表示コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool ShowDailyGraphTabCommand_CanExecute() => this.SelectedTab != Tabs.DailyGraphTab;
        /// <summary>
        /// 日別グラフタブ表示コマンド処理
        /// </summary>
        private void ShowDailyGraphTabCommand_Executed() => this.SelectedTab = Tabs.DailyGraphTab;

        /// <summary>
        /// 月別一覧タブ表示コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool ShowMonthlyListTabCommand_CanExecute() => this.SelectedTab != Tabs.MonthlyListTab;
        /// <summary>
        /// 月別一覧タブ表示コマンド処理
        /// </summary>
        private void ShowMonthlyListTabCommand_Executed() => this.SelectedTab = Tabs.MonthlyListTab;

        /// <summary>
        /// 月別グラフタブ表示コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool ShowMonthlyGraphTabCommand_CanExecute() => this.SelectedTab != Tabs.MonthlyGraphTab;
        /// <summary>
        /// 月別グラフタブ表示コマンド処理
        /// </summary>
        private void ShowMonthlyGraphTabCommand_Executed() => this.SelectedTab = Tabs.MonthlyGraphTab;

        /// <summary>
        /// 年別一覧タブ表示コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool ShowYearlyListTabCommand_CanExecute() => this.SelectedTab != Tabs.YearlyListTab;
        /// <summary>
        /// 年別一覧タブ表示コマンド処理
        /// </summary>
        private void ShowYearlyListTabCommand_Executed() => this.SelectedTab = Tabs.YearlyListTab;

        /// <summary>
        /// 年別グラフタブ表示コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool ShowYearlyGraphTabCommand_CanExecute() => this.SelectedTab != Tabs.YearlyGraphTab;
        /// <summary>
        /// 年別グラフタブ表示コマンド処理
        /// </summary>
        private void ShowYearlyGraphTabCommand_Executed() => this.SelectedTab = Tabs.YearlyGraphTab;
        #endregion

        #region 月間表示
        /// <summary>
        /// 先月表示コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        /// <remarks>帳簿/日別一覧タブを選択している</remarks>
        private bool GoToLastMonthCommand_CanExecute() => (this.SelectedTab == Tabs.BooksTab || this.SelectedTab == Tabs.DailyGraphTab) && this.DisplayedTermKind == TermKind.Monthly;
        /// <summary>
        /// 先月表示コマンド処理
        /// </summary>
        private async void GoToLastMonthCommand_Executed()
        {
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                this.DisplayedMonth = this.DisplayedMonth.Value.AddMonths(-1);
                await this.UpdateAsync(isUpdateBookList: true, isScroll: true);
            }
        }

        /// <summary>
        /// 今月表示コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool GoToThisMonthCommand_CanExecute()
        {
            DateTime thisMonth = DateTime.Today.GetFirstDateOfMonth();
            // 帳簿/月間一覧タブを選択している かつ 今月が表示されていない
            return (this.SelectedTab == Tabs.BooksTab || this.SelectedTab == Tabs.DailyGraphTab) &&
                   (this.DisplayedTermKind == TermKind.Selected || !(thisMonth <= this.DisplayedMonth && this.DisplayedMonth < thisMonth.AddMonths(1)));
        }
        /// <summary>
        /// 今月表示コマンド処理
        /// </summary>
        private async void GoToThisMonthCommand_Executed()
        {
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                this.DisplayedMonth = DateTime.Now.GetFirstDateOfMonth();
                await this.UpdateAsync(isUpdateBookList: true, isScroll: true);
            }
        }

        /// <summary>
        /// 翌月表示コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        /// <remarks>帳簿/月間一覧タブを選択している</remarks>
        private bool GoToNextMonthCommand_CanExecute() => (this.SelectedTab == Tabs.BooksTab || this.SelectedTab == Tabs.DailyGraphTab) && this.DisplayedTermKind == TermKind.Monthly;
        /// <summary>
        /// 翌月表示コマンド処理
        /// </summary>
        private async void GoToNextMonthCommand_Executed()
        {
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                this.DisplayedMonth = this.DisplayedMonth.Value.AddMonths(1);
                await this.UpdateAsync(isUpdateBookList: true, isScroll: true);
            }
        }

        /// <summary>
        /// 期間選択コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool SelectTermCommand_CanExecute() => !this.IsChildrenWindowOpened();
        /// <summary>
        /// 期間選択コマンド処理
        /// </summary>
        private async void SelectTermCommand_Executed()
        {
            SelectTermRequestEventArgs e = new() {
                DbHandlerFactory = this.dbHandlerFactory,
                TermKind = this.DisplayedTermKind,
                Month = this.DisplayedMonth,
                StartDate = this.StartDate,
                EndDate = this.EndDate,
            };

            this.SelectTermRequested?.Invoke(this, e);
            if (e.Result) {
                using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                    this.StartDate = e.StartDate;
                    this.EndDate = e.EndDate;

                    await this.UpdateAsync(isUpdateBookList: true, isScroll: true);
                }
            }
        }
        #endregion

        #region 年間表示
        /// <summary>
        /// 前年表示コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        /// <remarks>月別一覧/月別グラフ/年別一覧/年別グラフタブを選択している</remarks>
        private bool GoToLastYearCommand_CanExecute() => this.SelectedTab is Tabs.MonthlyListTab or Tabs.MonthlyGraphTab or Tabs.YearlyListTab or Tabs.YearlyGraphTab;
        /// <summary>
        /// 前年表示コマンド処理
        /// </summary>
        private async void GoToLastYearCommand_Executed()
        {
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                this.DisplayedYear = this.DisplayedYear.AddYears(-1);
                await this.UpdateAsync(isUpdateBookList: true);
            }
        }

        /// <summary>
        /// 今年表示コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool GoToThisYearCommand_CanExecute()
        {
            DateTime thisYear = DateTime.Now.GetFirstDateOfFiscalYear(this.FiscalStartMonth);
            // 月別一覧/月別グラフ/年別一覧/年別グラフタブを選択している かつ 今年が表示されていない
            return (this.SelectedTab is Tabs.MonthlyListTab or Tabs.MonthlyGraphTab or Tabs.YearlyListTab or Tabs.YearlyGraphTab) &&
                   !(thisYear <= this.DisplayedYear && this.DisplayedYear < thisYear.AddYears(1));
        }
        /// <summary>
        /// 今年表示コマンド処理
        /// </summary>
        private async void GoToThisYearCommand_Executed()
        {
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                this.DisplayedYear = DateTime.Now.GetFirstDateOfFiscalYear(this.FiscalStartMonth);
                await this.UpdateAsync(isUpdateBookList: true);
            }
        }

        /// <summary>
        /// 来年表示コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        /// <remarks>月別一覧/月別グラフ/年別一覧/年別グラフタブを選択している</remarks>
        private bool GoToNextYearCommand_CanExecute() => this.SelectedTab is Tabs.MonthlyListTab or Tabs.MonthlyGraphTab or Tabs.YearlyListTab or Tabs.YearlyGraphTab;
        /// <summary>
        /// 来年表示コマンド処理
        /// </summary>
        private async void GoToNextYearCommand_Executed()
        {
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                this.DisplayedYear = this.DisplayedYear.AddYears(1);
                await this.UpdateAsync(isUpdateBookList: true);
            }
        }
        #endregion

        /// <summary>
        /// 画面更新コマンド処理
        /// </summary>
        private async void UpdateCommand_Executed()
        {
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                await this.UpdateAsync(isUpdateBookList: true);
            }
        }
        #endregion

        #region ツールメニュー
        /// <summary>
        /// ツールコマンド実行可能か
        /// </summary>
        private bool ToolCommand_CanExecute() => !this.IsChildrenWindowOpened();

        /// <summary>
        /// 設定コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool SettingsWindowCommand_CanExecute() => !this.IsChildrenWindowOpened();
        /// <summary>
        /// 設定コマンド処理
        /// </summary>
        private async void SettingsCommand_Executed()
        {
            SettingsRequestEventArgs e = new() {
                DbHandlerFactory = this.dbHandlerFactory
            };
            this.SettingsRequested?.Invoke(this, e);

            if (e.Result) {
                using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                    this.FiscalStartMonth = Properties.Settings.Default.App_StartMonth;
                    await DateTimeExtensions.DownloadHolidayListAsync();
                    await this.UpdateAsync(isUpdateBookList: true);
                }
            }
        }

        /// <summary>
        /// 帳簿内ツールコマンド実行可能か
        /// </summary>
        private bool ToolInBookCommand_CanExecute() => this.SelectedTab == Tabs.BooksTab;

        /// <summary>
        /// CSV比較コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool CompareCsvFileCommand_CanExecute() => !this.IsChildrenWindowOpened();
        /// <summary>
        /// CSV比較コマンド処理
        /// </summary>
        private void CompareCsvFileCommand_Executed()
        {
            CompareCsvFileRequestEventArgs e = new() {
                DbHandlerFactory = this.dbHandlerFactory,
                BookId = this.SelectedBookVM.Id
            };
            this.CompareCsvFileRequested?.Invoke(this, e);
        }
        #endregion

        #region ヘルプメニュー
        private bool HelpMenuCommand_CanExecute() => true;

        /// <summary>
        /// バージョン表示コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool OpenVersionWindowCommand_CanExecute() => !this.IsChildrenWindowOpened();
        /// <summary>
        /// バージョン表示コマンド処理
        /// </summary>
        private void OpenVersionWindowCommand_Executed() => this.ShowVersionRequested?.Invoke(this, EventArgs.Empty);
        #endregion
        #endregion

        /// <summary>
        /// 子ウィンドウが開いているか
        /// </summary>
        /// <returns></returns>
        public bool IsChildrenWindowOpened() => this.IsChildrenWindowOpenedRequested?.Invoke() ?? false;
        /// <summary>
        /// 登録ウィンドウが開いているか
        /// </summary>
        /// <returns></returns>
        public bool IsRegistrationWindowOpened() => this.IsRegistrationWindowOpenedRequested?.Invoke() ?? false;

        /// <summary>
        /// <see cref="MainWindowViewModel"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        public MainWindowViewModel()
        {
            using FuncLog funcLog = new();

            this.BookTabVM = new(this, Tabs.BooksTab);
            this.DailyGraphTabVM = new(this, Tabs.DailyGraphTab);
            this.MonthlySummaryTabVM = new(this, Tabs.MonthlyListTab);
            this.MonthlyGraphTabVM = new(this, Tabs.MonthlyGraphTab);
            this.YearlySummaryTabVM = new(this, Tabs.YearlyListTab);
            this.YearlyGraphTabVM = new(this, Tabs.YearlyGraphTab);

            this.childrenVM.AddRange(
                [
                    this.BookTabVM,
                    this.DailyGraphTabVM,
                    this.MonthlySummaryTabVM,
                    this.MonthlyGraphTabVM,
                    this.YearlySummaryTabVM,
                    this.YearlyGraphTabVM
                ]
            );
        }

        public override void AddEventHandlers()
        {
            using FuncLog funcLog = new();

            Properties.Settings settings = Properties.Settings.Default;

            // タブ選択変更時
            this.SelectedTabChanged += async (sender, e) => {
                using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, methodName: nameof(this.SelectedTabChanged));

                using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                    settings.MainWindow_SelectedTabIndex = e.NewValue;
                    settings.Save();

                    await this.UpdateAsync(isUpdateBookList: true, isScroll: true);
                }
            };
            // 帳簿選択変更時
            this.SelectedBookChanged += async (sender, e) => {
                using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, methodName: nameof(this.SelectedBookChanged));

                using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                    settings.MainWindow_SelectedBookId = e.NewValue ?? -1;
                    settings.Save();

                    await this.UpdateAsync(isScroll: true);
                }
            };
            // グラフ種別1選択変更時
            this.SelectedGraphKind1Changed += async (sender, e) => {
                using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, methodName: nameof(this.SelectedGraphKind1));

                using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                    settings.MainWindow_SelectedGraphKindIndex = (int)e.NewValue;
                    settings.Save();

                    await this.UpdateAsync();
                }
            };
            // グラフ種別2選択変更時
            this.SelectedGraphKind2Changed += async (sender, e) => {
                using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, methodName: nameof(this.SelectedGraphKind2Changed));

                using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                    settings.MainWindow_SelectedGraphKind2Index = (int)e.NewValue;
                    settings.Save();

                    await this.UpdateAsync();
                }
            };
            // 系列選択変更時
            this.SelectedSeriesChanged += (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.SelectedSeriesChanged));

                using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                    switch (this.SelectedTab) {
                        case Tabs.DailyGraphTab:
                            this.DailyGraphTabVM.UpdateSelectedGraph();
                            break;
                        case Tabs.MonthlyGraphTab:
                            this.MonthlyGraphTabVM.UpdateSelectedGraph();
                            break;
                        case Tabs.YearlyGraphTab:
                            this.YearlyGraphTabVM.UpdateSelectedGraph();
                            break;
                    }
                }
            };
        }

        public override async Task LoadAsync()
        {
            using FuncLog funcLog = new();

            Properties.Settings settings = Properties.Settings.Default;
            this.FiscalStartMonth = settings.App_StartMonth;

            await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                this.SelectedDBKind = dbHandler.DBKind;
            }

            // 帳簿リスト更新
            await this.UpdateBookListAsync(settings.MainWindow_SelectedBookId);

            // タブ選択
            if (settings.MainWindow_SelectedTabIndex != -1) {
                this.SelectedTabIndex = settings.MainWindow_SelectedTabIndex;
            }
            // グラフ種別1選択
            if (settings.MainWindow_SelectedGraphKindIndex != -1) {
                this.SelectedGraphKind1Index = settings.MainWindow_SelectedGraphKindIndex;
            }
            // グラフ種別2選択
            if (settings.MainWindow_SelectedGraphKind2Index != -1) {
                this.SelectedGraphKind2Index = settings.MainWindow_SelectedGraphKind2Index;
            }

            Log.Vars(vars: new { this.SelectedTabIndex, this.SelectedGraphKind1Index, this.SelectedGraphKind2Index });

            await this.UpdateAsync(isScroll: true, isUpdateActDateLastEdited: true);
        }

        #region ウィンドウ設定プロパティ
        public override Size WindowSizeSetting
        {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return new Size(settings.MainWindow_Width, settings.MainWindow_Height);
            }
            set {
                Properties.Settings settings = Properties.Settings.Default;
                settings.MainWindow_Width = value.Width;
                settings.MainWindow_Height = value.Height;
                settings.Save();
            }
        }

        public override Point WindowPointSetting
        {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return new Point(settings.MainWindow_Left, settings.MainWindow_Top);
            }
            set {
                Properties.Settings settings = Properties.Settings.Default;
                settings.MainWindow_Left = value.X;
                settings.MainWindow_Top = value.Y;
                settings.Save();
            }
        }

        public override int WindowStateSetting
        {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return settings.MainWindow_WindowState;
            }
            set {
                Properties.Settings settings = Properties.Settings.Default;
                settings.MainWindow_WindowState = value;
                settings.Save();
            }
        }
        #endregion

        /// <summary>
        /// 画面更新(タブ非依存)
        /// </summary>
        /// <param name="isUpdateBookList">帳簿リストを更新するか</param>
        /// <param name="isScroll">帳簿項目一覧をスクロールするか</param>
        /// <param name="isUpdateActDateLastEdited">最後に操作した帳簿項目を更新するか</param>
        public async Task UpdateAsync(bool isUpdateBookList = false, bool isScroll = false, bool isUpdateActDateLastEdited = false)
        {
            using FuncLog funcLog = new(new { isUpdateBookList, isScroll, isUpdateActDateLastEdited });

            if (isUpdateBookList) await this.UpdateBookListAsync();

            switch (this.SelectedTab) {
                case Tabs.BooksTab:
                    await this.BookTabVM.LoadAsync(isScroll: isScroll, isUpdateActDateLastEdited: isUpdateActDateLastEdited);
                    break;
                case Tabs.DailyGraphTab:
                    await this.DailyGraphTabVM.LoadAsync();
                    break;
                case Tabs.MonthlyListTab:
                    await this.MonthlySummaryTabVM.LoadAsync();
                    break;
                case Tabs.MonthlyGraphTab:
                    await this.MonthlyGraphTabVM.LoadAsync();
                    break;
                case Tabs.YearlyListTab:
                    await this.YearlySummaryTabVM.LoadAsync();
                    break;
                case Tabs.YearlyGraphTab:
                    await this.YearlyGraphTabVM.LoadAsync();
                    break;
            }
        }

        /// <summary>
        /// 帳簿リストを更新する
        /// </summary>
        /// <param name="bookId">選択対象の帳簿ID</param>
        public async Task UpdateBookListAsync(int? bookId = null)
        {
            using FuncLog funcLog = new();

            ViewModelLoader loader = new(this.dbHandlerFactory);
            int? tmpBookId = bookId ?? this.SelectedBookVM?.Id;
            var tmpBookVMList = await loader.LoadBookListAsync(Properties.Resources.ListName_AllBooks, this.DisplayedStart, this.DisplayedEnd);
            this.SelectedBookVM = tmpBookVMList.FirstOrElementAtOrDefault(vm => vm.Id == tmpBookId, 0); // 先に選択しておく
            this.BookVMList = tmpBookVMList;
        }

        #region ダンプ/リストア
        /// <summary>
        /// バックアップファイルを作成する
        /// </summary>
        /// <param name="notifyResult">実行結果を通知する</param>
        /// <param name="waitForFinish">完了を待つか</param>
        /// <param name="backUpNum">バックアップ数</param>
        /// <param name="backUpFolderPath">バックアップ用フォルダパス</param>
        /// <returns>成功/失敗</returns>
        public async Task<bool> CreateBackUpFileAsync(bool notifyResult = false, bool waitForFinish = true, int? backUpNum = null, string backUpFolderPath = null)
        {
            using FuncLog funcLog = new(new { notifyResult, waitForFinish, backUpNum, backUpFolderPath });

            Properties.Settings settings = Properties.Settings.Default;

            int tmpBackUpNum = backUpNum ?? settings.App_BackUpNum;
            string tmpBackUpFolderPath = backUpFolderPath ?? settings.App_BackUpFolderPath;

            if (tmpBackUpFolderPath == string.Empty) {
                return false;
            }

            bool result;
            string backUpFileExt = PostgreSQLBackupFileExt;
            if (0 < tmpBackUpNum) {
                Log.Debug("Create backup file.");
                // フォルダが存在しなければ作成する
                if (!Directory.Exists(tmpBackUpFolderPath)) {
                    _ = Directory.CreateDirectory(tmpBackUpFolderPath);
                }

                DBKind selectedDBKind = (DBKind)settings.App_SelectedDBKind;
                switch (selectedDBKind) {
                    case DBKind.PostgreSQL: {
                        string backupFilePath = Path.Combine(tmpBackUpFolderPath, PostgreSQLBackupFileName);
                        backUpFileExt = PostgreSQLBackupFileExt;
                        int? exitCode = null;

                        if (settings.App_Postgres_DumpExePath != string.Empty) {
                            exitCode = await ExecuteDumpPostgreSQL(this.dbHandlerFactory, backupFilePath, PostgresFormat.Custom, notifyResult, waitForFinish);
                        }

                        result = exitCode == 0;
                        break;
                    }
                    case DBKind.SQLite: {
                        string backupFilePath = Path.Combine(tmpBackUpFolderPath, SQLiteBackupFileName);
                        backUpFileExt = SQLiteBackupFileExt;
                        try {
                            File.Copy(settings.App_SQLite_DBFilePath, backupFilePath, true);
                            result = true;
                        }
                        catch {
                            result = false;
                        }
                        break;
                    }
                    default:
                        result = false;
                        break;
                }
            }
            else {
                result = true;
            }

            if (Directory.Exists(tmpBackUpFolderPath)) {
                // サイズ0のバックアップを削除する
                Log.Debug("Delete size 0 files.");
                List<string> filePathList = [.. Directory.GetFiles(tmpBackUpFolderPath, $"*.{backUpFileExt}", SearchOption.TopDirectoryOnly)];
                foreach (string filePath in filePathList) {
                    FileInfo fileInfo = new(filePath);
                    if (fileInfo.Length == 0) {
                        fileInfo.Delete();
                    }
                }

                // 古いバックアップを削除する
                Log.Debug("Delete old backup files.");
                filePathList = [.. Directory.GetFiles(tmpBackUpFolderPath, $"*.{backUpFileExt}", SearchOption.TopDirectoryOnly)];
                if (filePathList.Count > tmpBackUpNum) {
                    filePathList.Sort();

                    for (int i = 0; i < filePathList.Count - tmpBackUpNum; ++i) {
                        File.Delete(filePathList[i]);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// PostgreSQLのダンプを実行する
        /// </summary>
        /// <param name="backupFilePath">バックアップファイルパス</param>
        /// <param name="format">ダンプフォーマット</param>
        /// <param name="notifyResult">実行結果を通知する</param>
        /// <param name="waitForFinish">処理の完了を待機する</param>
        /// <returns>成功/失敗/不明</returns>
        private static async Task<int?> ExecuteDumpPostgreSQL(DbHandlerFactory dbHandlerFactory, string backupFilePath, PostgresFormat format, bool notifyResult = false, bool waitForFinish = true)
        {
            using FuncLog funcLog = new(new { backupFilePath, format, notifyResult, waitForFinish });

            Properties.Settings settings = Properties.Settings.Default;

            int? result = -1;
            await using (DbHandlerBase dbHandler = await dbHandlerFactory.CreateAsync()) {
                if (dbHandler is NpgsqlDbHandler npgsqlDbHandler) {
                    result = notifyResult
                        ? await npgsqlDbHandler.ExecuteDump(backupFilePath, settings.App_Postgres_DumpExePath, (PostgresPasswordInput)settings.App_Postgres_Password_Input, format,
                            exitCode => {
                                // ダンプ結果を通知する
                                if (exitCode == 0) {
                                    NotificationManager nm = new();
                                    NotificationContent nc = new() {
                                        Title = Properties.Resources.Title_MainWindow,
                                        Message = Properties.Resources.Message_FinishToBackup,
                                        Type = NotificationType.Success
                                    };
                                    nm.Show(nc, expirationTime: new TimeSpan(0, 0, 10));
                                }
                                else if (exitCode != null) {
                                    NotificationManager nm = new();
                                    NotificationContent nc = new() {
                                        Title = Properties.Resources.Title_MainWindow,
                                        Message = Properties.Resources.Message_FoultToBackup,
                                        Type = NotificationType.Error
                                    };
                                    nm.Show(nc, expirationTime: new TimeSpan(0, 0, 10));
                                }
                            }, waitForFinish)
                        : await npgsqlDbHandler.ExecuteDump(backupFilePath, settings.App_Postgres_DumpExePath, (PostgresPasswordInput)settings.App_Postgres_Password_Input, format, null, waitForFinish);
                }
            }
            return result;
        }

        /// <summary>
        /// PostgreSQLのリストアを実行する
        /// </summary>
        /// <param name="backupFilePath">バックアップファイルパス</param>
        /// <returns>成功/失敗</returns>
        private static async Task<int> ExecuteRestorePostgreSQL(DbHandlerFactory dbHandlerFactory, string backupFilePath)
        {
            using FuncLog funcLog = new(new { backupFilePath });

            Properties.Settings settings = Properties.Settings.Default;

            int result = -1;
            await using (DbHandlerBase dbHandler = await dbHandlerFactory.CreateAsync()) {
                if (dbHandler is NpgsqlDbHandler npgsqlDbHandler) {
                    result = await npgsqlDbHandler.ExecuteRestore(backupFilePath, settings.App_Postgres_RestoreExePath, (PostgresPasswordInput)settings.App_Postgres_Password_Input);
                }
            }
            return result;
        }
        #endregion
    }
}

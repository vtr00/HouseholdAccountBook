using HouseholdAccountBook.Infrastructure.DB;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Infrastructure.Utilities.Args;
using HouseholdAccountBook.Infrastructure.Utilities.Args.RequestEventArgs;
using HouseholdAccountBook.Infrastructure.Utilities.Extensions;
using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.DbHandlers;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using HouseholdAccountBook.ViewModels.WindowsParts;
using HouseholdAccountBook.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static HouseholdAccountBook.ViewModels.UiConstants;

namespace HouseholdAccountBook.ViewModels.Windows
{
    /// <summary>
    /// メインウィンドウVM
    /// </summary>
    public class MainWindowViewModel : WindowViewModelBase
    {
        #region フィールド
        /// <summary>
        /// アプリサービス
        /// </summary>
        private AppService mService;
        /// <summary>
        /// DBインポートサービス
        /// </summary>
        private DbImportService mDbImportService;

        /// <summary>
        /// 表示日付の更新中か(表示月/表示年の更新用)
        /// </summary>
        private bool mOnUpdateDisplayedDate;
        #endregion

        #region イベント
        /// <summary>
        /// タブ選択変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<int>> SelectedTabChanged;
        /// <summary>
        /// グラフ種別1選択変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<GraphKind1>> SelectedGraphKind1Changed;
        /// <summary>
        /// グラフ種別2選択変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<GraphKind2>> SelectedGraphKind2Changed;
        /// <summary>
        /// 系列選択変更時イベント
        /// </summary>
        public event EventHandler SelectedSeriesChanged {
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
        public event EventHandler<EventArgs<int>> ScrollRequested {
            add => this.BookTabVM.ScrollRequested += value;
            remove => this.BookTabVM.ScrollRequested -= value;
        }

        /// <summary>
        /// 移動追加要求時イベント
        /// </summary>
        public event EventHandler<AddMoveRequestEventArgs> AddMoveRequested {
            add => this.BookTabVM.AddMoveRequested += value;
            remove => this.BookTabVM.AddMoveRequested -= value;
        }
        /// <summary>
        /// 帳簿項目追加要求時イベント
        /// </summary>
        public event EventHandler<AddActionRequestEventArgs> AddActionRequested {
            add => this.BookTabVM.AddActionRequested += value;
            remove => this.BookTabVM.AddActionRequested -= value;
        }
        /// <summary>
        /// 帳簿項目リスト追加要求時イベント
        /// </summary>
        public event EventHandler<AddActionListRequestEventArgs> AddActionListRequested {
            add => this.BookTabVM.AddActionListRequested += value;
            remove => this.BookTabVM.AddActionListRequested -= value;
        }
        /// <summary>
        /// 移動複製要求時イベント
        /// </summary>
        public event EventHandler<CopyMoveRequestEventArgs> CopyMoveRequested {
            add => this.BookTabVM.CopyMoveRequested += value;
            remove => this.BookTabVM.CopyMoveRequested -= value;
        }
        /// <summary>
        /// 帳簿項目複製要求時イベント
        /// </summary>
        public event EventHandler<CopyActionRequestEventArgs> CopyActionRequested {
            add => this.BookTabVM.CopyActionRequested += value;
            remove => this.BookTabVM.CopyActionRequested -= value;
        }
        /// <summary>
        /// 移動編集要求時イベント
        /// </summary>
        public event EventHandler<EditMoveRequestEventArgs> EditMoveRequested {
            add => this.BookTabVM.EditMoveRequested += value;
            remove => this.BookTabVM.EditMoveRequested -= value;
        }
        /// <summary>
        /// 帳簿項目編集要求時イベント
        /// </summary>
        public event EventHandler<EditActionRequestEventArgs> EditActionRequested {
            add => this.BookTabVM.EditActionRequested += value;
            remove => this.BookTabVM.EditActionRequested -= value;
        }
        /// <summary>
        /// 帳簿項目リスト編集要求時イベント
        /// </summary>
        public event EventHandler<EditActionListRequestEventArgs> EditActionListRequested {
            add => this.BookTabVM.EditActionListRequested += value;
            remove => this.BookTabVM.EditActionListRequested -= value;
        }
        /// <summary>
        /// 期間選択要求時イベント
        /// </summary>
        public event EventHandler<SelectPeriodRequestEventArgs> SelectPeriodRequested;
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

        #region Bindingプロパティ
        #region プロパティ(共通)
        /// <summary>
        /// 操作ログファイルメニューリスト
        /// </summary>
        public ObservableCollection<MenuItemViewModel> OperationLogFileMenuList { get; init; } = [];

        /// <summary>
        /// 選択されたDB種別
        /// </summary>
        #region SelectedDBKind
        public DBKind SelectedDBKind {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    this.RaisePropertyChanged(nameof(this.IsPostgreSQL));
                    this.RaisePropertyChanged(nameof(this.IsSQLite));
                }
            }
        }
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
        public int SelectedTabIndex {
            get;
            set {
                int oldValue = field;
                if (this.SetProperty(ref field, value)) {
                    this.SelectedTabChanged?.Invoke(this, new() { OldValue = oldValue, NewValue = value });
                    this.RaisePropertyChanged(nameof(this.DisplayedStart));
                    this.RaisePropertyChanged(nameof(this.DisplayedEnd));
                }
            }
        }
        #endregion
        /// <summary>
        /// 選択されたタブ種別
        /// </summary>
        #region SelectedTab
        public Tabs SelectedTab {
            get => (Tabs)this.SelectedTabIndex;
            set => this.SelectedTabIndex = (int)value;
        }
        #endregion

        /// <summary>
        /// 帳簿セレクタVM
        /// </summary>
        public SingleSelectorViewModel<BookModel, BookIdObj> BookSelectorVM { get; } = new(vm => vm?.Id);

        /// <summary>
        /// 選択された収支種別
        /// </summary>
        public BalanceKind? SelectedBalanceKind { get; set; }
        /// <summary>
        /// 選択された分類ID
        /// </summary>
        public CategoryIdObj SelectedCategoryId { get; set; }
        /// <summary>
        /// 選択された項目ID
        /// </summary>
        public ItemIdObj SelectedItemId { get; set; }

        /// <summary>
        /// 表示開始日付
        /// </summary>
        #region DisplayedStart
        public DateOnly DisplayedStart => this.SelectedTab switch {
            Tabs.BooksTab or Tabs.DailyGraphTab => this.StartDate,
            Tabs.MonthlyListTab or Tabs.MonthlyGraphTab => this.DisplayedStartMonth,
            Tabs.YearlyGraphTab or Tabs.YearlyListTab => this.DisplayedStartYear,
            _ => this.StartDate,
        };
        #endregion
        /// <summary>
        /// 表示終了日付
        /// </summary>
        #region DisplayedEnd
        public DateOnly DisplayedEnd => this.SelectedTab switch {
            Tabs.BooksTab or Tabs.DailyGraphTab => this.EndDate,
            Tabs.MonthlyListTab or Tabs.MonthlyGraphTab => this.DisplayedEndMonth,
            Tabs.YearlyGraphTab or Tabs.YearlyListTab => this.DisplayedEndYear,
            _ => this.EndDate
        };
        /// <summary>
        /// 表示期間
        /// </summary>
        public PeriodObj<DateOnly> DisplayedPeriod => this.SelectedTab switch {
            Tabs.BooksTab or Tabs.DailyGraphTab => new(this.StartDate, this.EndDate),
            Tabs.MonthlyListTab or Tabs.MonthlyGraphTab => new(this.DisplayedStartMonth, this.DisplayedEndMonth),
            Tabs.YearlyGraphTab or Tabs.YearlyListTab => new(this.DisplayedStartYear, this.DisplayedEndYear),
            _ => new(this.StartDate, this.EndDate)
        };
        #endregion

        /// <summary>
        /// 会計開始月
        /// </summary>
        #region FiscalStartMonth
        public int FiscalStartMonth {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    this.RaisePropertyChanged(nameof(this.DisplayedStart));
                    this.RaisePropertyChanged(nameof(this.DisplayedEnd));
                    this.RaisePropertyChanged(nameof(this.DisplayedMonth));
                    this.RaisePropertyChanged(nameof(this.DisplayedMonths));
                    this.RaisePropertyChanged(nameof(this.DisplayedYear));
                    this.RaisePropertyChanged(nameof(this.DisplayedYears));
                }
            }
        } = 4;
        #endregion

        /// <summary>
        /// 表示区間種別
        /// </summary>
        #region DisplayedTermKind
        public PeriodKind DisplayedPeriodKind {
            get {
                DateOnly lastDate = this.StartDate.GetLastDateOfMonth();
                return (this.StartDate.Day == 1 && this.EndDate == lastDate) ? PeriodKind.Monthly : PeriodKind.Selected;
            }
        }
        #endregion

        /// <summary>
        /// 表示月
        /// </summary>
        #region DisplayedMonth
        public DateOnly? DisplayedMonth {
            get => this.DisplayedPeriodKind switch {
                PeriodKind.Monthly => (DateOnly?)this.StartDate,
                PeriodKind.Selected => null,
                _ => null,
            };
            set {
                if (this.DisplayedMonth != value && value is not null) {
                    // 開始日/終了日を更新する
                    this.StartDate = value.Value.GetFirstDateOfMonth();
                    this.EndDate = value.Value.GetLastDateOfMonth();

                    if (!this.mOnUpdateDisplayedDate) {
                        this.mOnUpdateDisplayedDate = true;
                        // 表示月の年度の最初の月を表示年とする
                        this.DisplayedYear = value.Value.GetFirstDateOfFiscalYear(this.FiscalStartMonth);
                        this.mOnUpdateDisplayedDate = false;
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
        public DateOnly StartDate {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    this.RaisePropertyChanged(nameof(this.DisplayedMonth));
                }
            }
        } = DateOnlyExtensions.Today.GetFirstDateOfMonth();
        #endregion
        /// <summary>
        /// 表示終了日
        /// </summary>
        #region EndDate
        public DateOnly EndDate {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    this.RaisePropertyChanged(nameof(this.DisplayedMonth));
                }
            }
        } = DateOnlyExtensions.Today.GetLastDateOfMonth();
        #endregion

        /// <summary>
        /// 表示年
        /// </summary>
        #region DisplayedYear
        public DateOnly DisplayedYear {
            get;
            set {
                DateOnly oldDisplayedYear = field;
                if (this.SetProperty(ref field, value)) {
                    if (!this.mOnUpdateDisplayedDate) {
                        this.mOnUpdateDisplayedDate = true;
                        int yearDiff = value.GetFirstDateOfFiscalYear(this.FiscalStartMonth).Year - oldDisplayedYear.GetFirstDateOfFiscalYear(this.FiscalStartMonth).Year;
                        if (this.DisplayedMonth != null) {
                            // 表示年の差分を表示月に反映する
                            this.DisplayedMonth = this.DisplayedMonth.Value.AddYears(yearDiff);
                        }
                        this.mOnUpdateDisplayedDate = false;
                    }

                    this.RaisePropertyChanged(nameof(this.DisplayedStart));
                    this.RaisePropertyChanged(nameof(this.DisplayedEnd));
                    this.RaisePropertyChanged(nameof(this.DisplayedMonths));
                    this.RaisePropertyChanged(nameof(this.DisplayedYears));
                }
            }
        } = DateOnlyExtensions.Today;
        #endregion

        /// <summary>
        /// 表示月リスト(月別一覧の月)
        /// </summary>
        #region DisplayedMonths
        public ObservableCollection<DateOnly> DisplayedMonths {
            get {
                DateOnly tmpMonth = this.DisplayedYear.GetFirstDateOfFiscalYear(this.FiscalStartMonth);

                // 表示する月の文字列を作成する
                ObservableCollection<DateOnly> displayedMonths = [];
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
        public DateOnly DisplayedStartMonth => this.DisplayedMonths.First();
        /// <summary>
        /// 表示終了月
        /// </summary>
        public DateOnly DisplayedEndMonth => this.DisplayedMonths.Last();

        /// <summary>
        /// 表示年リスト(年別一覧の年)
        /// </summary>
        #region DisplayedYears
        public ObservableCollection<DateOnly> DisplayedYears {
            get {
                DateOnly tmpYear = this.DisplayedYear.GetFirstDateOfFiscalYear(this.FiscalStartMonth).AddYears(-9);
                ObservableCollection<DateOnly> displayedYears = [];
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
        public DateOnly DisplayedStartYear => this.DisplayedYears.First();
        /// <summary>
        /// 表示終了年
        /// </summary>
        public DateOnly DisplayedEndYear => this.DisplayedYears.Last();
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
        public GraphKind1 SelectedGraphKind1 {
            get;
            set {
                var oldValue = field;
                if (this.SetProperty(ref field, value)) {
                    this.SelectedGraphKind1Changed?.Invoke(this, new() { OldValue = oldValue, NewValue = value });
                }
            }
        }
        #endregion
        /// <summary>
        /// 選択されたグラフ種別1インデックス
        /// </summary>
        #region SelectedGraphKind1Index
        public int SelectedGraphKind1Index {
            get => (int)this.SelectedGraphKind1;
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
        public GraphKind2 SelectedGraphKind2 {
            get;
            set {
                var oldValue = field;
                if (this.SetProperty(ref field, value)) {
                    this.SelectedGraphKind2Changed?.Invoke(this, new() { OldValue = oldValue, NewValue = value });
                }
            }
        }
        #endregion
        /// <summary>
        /// 選択されたグラフ種別2インデックス
        /// </summary>
        #region SelectedGraphKind2Index
        public int SelectedGraphKind2Index {
            get => (int)this.SelectedGraphKind2;
            set => this.SelectedGraphKind2 = (GraphKind2)value;
        }
        #endregion
        #endregion

        #region タブVM
        /// <summary>
        /// 帳簿タブVM
        /// </summary>
        public MainWindowBookTabViewModel BookTabVM { get; private init; }
        /// <summary>
        /// 日別グラフタブVM
        /// </summary>
        public MainWindowGraphTabViewModel DailyGraphTabVM { get; private init; }
        /// <summary>
        /// 月別一覧タブVM
        /// </summary>
        public MainWindowListTabViewModel MonthlySummaryTabVM { get; private init; }
        /// <summary>
        /// 月別グラフタブVM
        /// </summary>
        public MainWindowGraphTabViewModel MonthlyGraphTabVM { get; private init; }
        /// <summary>
        /// 年別一覧タブVM
        /// </summary>
        public MainWindowListTabViewModel YearlySummaryTabVM { get; private init; }
        /// <summary>
        /// 年別グラフタブVM
        /// </summary>
        public MainWindowGraphTabViewModel YearlyGraphTabVM { get; private init; }
        #endregion

        #region コマンド
        #region ファイルコマンド
        /// <summary>
        /// ファイルコマンド
        /// </summary>
        public ICommand FileMenuCommand => new RelayCommand();
        /// <summary>
        /// インポートコマンド
        /// </summary>
        public ICommand ImportCommand => new RelayCommand(this.ImportCommand_CanExecute);
        /// <summary>
        /// 記帳風月インポートコマンド
        /// </summary>
        public ICommand ImportKichoFugetsuDbCommand => new RelayCommand(this.ImportKichoFugetsuDbCommand_Executed, this.ImportKichoFugetsuDbCommand_CanExecute);
        /// <summary>
        /// PostgreSQL -> SQLite インポートコマンド
        /// </summary>
        public ICommand ImportPostgreSQLCommand => new RelayCommand(this.ImportPostgreSQLCommand_Executed, this.ImportPostgreSQLCommand_CanExecute);
        /// <summary>
        /// カスタムファイル -> PostgreSQL インポートコマンド
        /// </summary>
        public ICommand ImportCustomFileCommand => new RelayCommand(this.ImportCustomFileCommand_Executed, this.ImportCustomFileCommand_CanExecute);
        /// <summary>
        /// SQLiteファイル -> PostgreSQL/SQLite インポートコマンド
        /// </summary>
        public ICommand ImportSQLiteFileCommand => new RelayCommand(this.ImportSQLiteFileCommand_Executed, this.ImportSQLiteFileCommand_CanExecute);
        /// <summary>
        /// エクスポートコマンド
        /// </summary>
        public ICommand ExportCommand => new RelayCommand(this.ExportCommand_CanExecute);
        /// <summary>
        /// カスタムファイルエクスポートコマンド
        /// </summary>
        public ICommand ExportCustomFileCommand => new RelayCommand(this.ExportCustomFileCommand_Executed, this.ExportCustomFileCommand_CanExecute);
        /// <summary>
        /// SQLファイルエクスポートコマンド
        /// </summary>
        public ICommand ExportSQLFileCommand => new RelayCommand(this.ExportSQLFileCommand_Executed, this.ExportSQLFileCommand_CanExecute);
        /// <summary>
        /// バックアップコマンド
        /// </summary>
        public ICommand BackUpCommand => new RelayCommand(this.BackUpCommand_Executed, this.BackUpCommand_CanExecute);
        /// <summary>
        /// 操作ログファイルコマンド
        /// </summary>
        public ICommand OperationLogFileCommand => new RelayCommand(this.OperationLogFileCommand_CanExecute);
        /// <summary>
        /// ウィンドウ終了コマンド
        /// </summary>
        public ICommand ExitWindowCommand => new RelayCommand(this.ExitWindowCommand_Executed, this.ExitWindowCommand_CanExecute);
        #endregion

        #region 表示コマンド
        /// <summary>
        /// 表示コマンド
        /// </summary>
        public ICommand ShowMenuCommand => new RelayCommand(this.ShowMenuCommand_CanExecute);
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
        public ICommand ToolMenuCommand => new RelayCommand(this.ToolCommand_CanExecute);
        /// <summary>
        /// 設定コマンド
        /// </summary>
        public ICommand SettingsCommand => new RelayCommand(this.SettingsCommand_Executed, this.SettingsWindowCommand_CanExecute);
        /// <summary>
        /// 帳簿内ツールコマンド
        /// </summary>
        public ICommand ToolInBookCommand => new RelayCommand(this.ToolInBookCommand_CanExecute);
        /// <summary>
        /// CSVファイル比較コマンド
        /// </summary>
        public ICommand CompareCsvFileCommand => new RelayCommand(this.CompareCsvFileCommand_Executed, this.CompareCsvFileCommand_CanExecute);
        #endregion

        #region ヘルプコマンド
        /// <summary>
        /// ヘルプメニューコマンド
        /// </summary>
        public ICommand HelpMenuCommand => new RelayCommand(this.HelpMenuCommand_CanExecute);
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
        /// インポートコマンド実行可能か
        /// </summary>
        /// <returns></returns>
        public bool ImportCommand_CanExecute() => !this.IsChildrenWindowOpened();

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
            (string directory, string fileName) = PathUtil.GetSeparatedPath(settings.App_Import_KichoFugetsu_FilePath, App.GetCurrentDir());

            OpenFileDialogRequestEventArgs e = new() {
                InitialDirectory = directory,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = Properties.Resources.FileSelectFilter_MdbFile + "|*.mdb"
            };
            if (!this.OpenFileDialogRequest(e)) { return; }

            if (MessageBox.Show(Properties.Resources.Message_DeleteOldDataNotification, Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) != MessageBoxResult.OK) {
                return;
            }

            settings.App_Import_KichoFugetsu_FilePath = e.FileName;
            settings.Save();

            bool result = false;
            int actRowsDiff = 0;
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                OleDbHandler.ConnectInfo info = new(settings.App_Access_Provider) {
                    DataSource = e.FileName
                };
                (result, actRowsDiff) = await this.mDbImportService.ImportKichoFugetsuDbAsync(info);

                if (result) {
                    await this.UpdateAsync(isUpdateBookList: true, isScroll: true, isUpdateActDateLastEdited: true);
                }
            }

            _ = result
                ? MessageBox.Show(Properties.Resources.Message_FinishToImport + (0 < actRowsDiff ? Environment.NewLine + Properties.Resources.Message_DeletedZeroValueInformation : string.Empty),
                    Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK)
                : MessageBox.Show(Properties.Resources.Message_FoultToImport, Properties.Resources.Title_Conformation, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        }

        /// <summary>
        /// PostgreSQL -> SQLite インポートコマンド実行可能か
        /// </summary>
        /// <returns></returns>
        public bool ImportPostgreSQLCommand_CanExecute() => this.IsSQLite && !this.IsChildrenWindowOpened();
        /// <summary>
        /// PostgreSQL -> SQLite インポートコマンド処理
        /// </summary>
        public async void ImportPostgreSQLCommand_Executed()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (MessageBox.Show(Properties.Resources.Message_DeleteOldDataNotification, Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) != MessageBoxResult.OK) {
                return;
            }

            bool result = false;
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
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
                result = await this.mDbImportService.ImportPostgreSQLAsync(info);

                if (result) {
                    await this.UpdateAsync(isUpdateBookList: true, isScroll: true, isUpdateActDateLastEdited: true);
                }
            }

            _ = result
                ? MessageBox.Show(Properties.Resources.Message_FinishToImport, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK)
                : MessageBox.Show(Properties.Resources.Message_FoultToImport, Properties.Resources.Title_Conformation, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        }

        /// <summary>
        /// カスタムファイル -> PostgreSQL インポートコマンド実行可能か
        /// </summary>
        /// <returns></returns>
        public bool ImportCustomFileCommand_CanExecute() => this.IsPostgreSQL && DbBackUpManager.Instance.PostgresRestoreExePath != string.Empty && !this.IsChildrenWindowOpened();
        /// <summary>
        /// カスタムファイル -> PostgreSQL インポートコマンド処理
        /// </summary>
        public async void ImportCustomFileCommand_Executed()
        {
            Properties.Settings settings = Properties.Settings.Default;
            (string directory, string fileName) = PathUtil.GetSeparatedPath(settings.App_Import_CustomFormat_FilePath, App.GetCurrentDir());

            OpenFileDialogRequestEventArgs e = new() {
                InitialDirectory = directory,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = Properties.Resources.FileSelectFilter_CustomFormatFile + "|*.*"
            };

            if (!this.OpenFileDialogRequest(e)) { return; }

            if (MessageBox.Show(Properties.Resources.Message_DeleteOldDataNotification, Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) != MessageBoxResult.OK) {
                return;
            }

            settings.App_Import_CustomFormat_FilePath = e.FileName;
            settings.Save();

            bool result = false;
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                result = await this.mDbImportService.ImportCustomFileAsync(e.FileName);

                if (result) {
                    await this.UpdateAsync(isUpdateBookList: true, isScroll: true, isUpdateActDateLastEdited: true);
                }
            }

            _ = result
                ? MessageBox.Show(Properties.Resources.Message_FinishToImport, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK)
                : MessageBox.Show(Properties.Resources.Message_FoultToImport, Properties.Resources.Title_Conformation, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        }

        /// <summary>
        /// SQLiteファイル -> PostgreSQL/SQLite インポートコマンド実行可能か
        /// </summary>
        /// <returns></returns>
        public bool ImportSQLiteFileCommand_CanExecute() => (this.IsPostgreSQL || this.IsSQLite) && !this.IsChildrenWindowOpened();
        /// <summary>
        /// SQLiteファイル -> PostgreSQL/SQLite インポートコマンド処理
        /// </summary>
        public async void ImportSQLiteFileCommand_Executed()
        {
            Properties.Settings settings = Properties.Settings.Default;
            (string directory, string fileName) = PathUtil.GetSeparatedPath(settings.App_SQLite_DBFilePath, App.GetCurrentDir());

            OpenFileDialogRequestEventArgs e = new() {
                InitialDirectory = directory,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = Properties.Resources.FileSelectFilter_SQLiteFile + "|*.db;*.sqlite;*.sqlite3"
            };

            if (!this.OpenFileDialogRequest(e)) { return; }

            if (MessageBox.Show(Properties.Resources.Message_DeleteOldDataNotification, Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) != MessageBoxResult.OK) {
                return;
            }

            settings.App_Import_SQLite_FilePath = e.FileName;
            settings.Save();

            bool result = false;
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                result = await this.mDbImportService.ImportSQLiteAsync(this.SelectedDBKind, e.FileName, settings.App_SQLite_DBFilePath);

                if (result) {
                    await this.UpdateAsync(isUpdateBookList: true, isScroll: true, isUpdateActDateLastEdited: true);
                }
            }

            _ = result
                ? MessageBox.Show(Properties.Resources.Message_FinishToImport, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK)
                : MessageBox.Show(Properties.Resources.Message_FoultToImport, Properties.Resources.Title_Conformation, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        }

        /// <summary>
        /// エクスポートコマンド実行可能か
        /// </summary>
        /// <returns></returns>
        public bool ExportCommand_CanExecute() => !this.IsChildrenWindowOpened();

        /// <summary>
        /// カスタムファイルエクスポートコマンド実行可能か
        /// </summary>
        /// <returns></returns>
        public bool ExportCustomFileCommand_CanExecute() => this.IsPostgreSQL && DbBackUpManager.Instance.PostgresDumpExePath != string.Empty && !this.IsChildrenWindowOpened();
        /// <summary>
        /// カスタムファイルエクスポートコマンド処理
        /// </summary>
        public async void ExportCustomFileCommand_Executed()
        {
            Properties.Settings settings = Properties.Settings.Default;
            (string directory, string fileName) = PathUtil.GetSeparatedPath(settings.App_Export_CustomFormat_FilePath, App.GetCurrentDir());

            SaveFileDialogRequestEventArgs e = new() {
                InitialDirectory = directory,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = Properties.Resources.FileSelectFilter_CustomFormatFile + "|*.*"
            };
            if (!this.SaveFileDialogRequest(e)) { return; }

            settings.App_Export_CustomFormat_FilePath = e.FileName;
            settings.Save();

            bool result = false;
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                result = await DbBackUpManager.Instance.ExecuteDumpPostgreSQLAsync(e.FileName, PostgresFormat.Custom) == true;
            }

            _ = result
                ? MessageBox.Show(Properties.Resources.Message_FinishToExport, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK)
                : MessageBox.Show(Properties.Resources.Message_FoultToExport, Properties.Resources.Title_Conformation, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        }

        /// <summary>
        /// SQLファイルエクスポートコマンド実行可能か
        /// </summary>
        /// <returns></returns>
        public bool ExportSQLFileCommand_CanExecute() => this.IsPostgreSQL && DbBackUpManager.Instance.PostgresDumpExePath != string.Empty && !this.IsChildrenWindowOpened();
        /// <summary>
        /// SQLファイルエクスポートコマンド処理
        /// </summary>
        public async void ExportSQLFileCommand_Executed()
        {
            Properties.Settings settings = Properties.Settings.Default;

            (string directory, string fileName) = PathUtil.GetSeparatedPath(settings.App_Export_SQLFilePath, App.GetCurrentDir());

            SaveFileDialogRequestEventArgs e = new() {
                InitialDirectory = directory,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = Properties.Resources.FileSelectFilter_SqlFile + "|*.sql"
            };
            if (!this.SaveFileDialogRequest(e)) { return; }

            settings.App_Export_SQLFilePath = e.FileName;
            settings.Save();

            bool result = false;
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                result = await DbBackUpManager.Instance.ExecuteDumpPostgreSQLAsync(e.FileName, PostgresFormat.Plain) == true;
            }

            _ = result
                ? MessageBox.Show(Properties.Resources.Message_FinishToExport, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK)
                : MessageBox.Show(Properties.Resources.Message_FoultToExport, Properties.Resources.Title_Conformation, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
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
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                result = await DbBackUpManager.Instance.CreateBackUpFileAsync(backUpNum: -1);

                if (result) {
                    Properties.Settings settings = Properties.Settings.Default;
                    settings.App_BackUpCurrentBySelf = DateTime.Now;
                    settings.Save();
                    this.BookTabVM.RaiseCurrentBackUpChanged();
                }
            }

            _ = result
                ? MessageBox.Show(Properties.Resources.Message_FinishToBackup, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK)
                : MessageBox.Show(Properties.Resources.Message_FoultToBackup, Properties.Resources.Title_Conformation, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        }

        /// <summary>
        /// 操作ログファイルコマンド実行可能か
        /// </summary>
        /// <returns></returns>
        public bool OperationLogFileCommand_CanExecute() => 0 < this.OperationLogFileMenuList.Count;

        /// <summary>
        /// ウィンドウ終了コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        public bool ExitWindowCommand_CanExecute() => !this.IsChildrenWindowOpened();
        /// <summary>
        /// ウィンドウ終了コマンド処理
        /// </summary>
        public void ExitWindowCommand_Executed() => this.CloseRequest(new DialogCloseRequestEventArgs(true));
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
        private bool GoToLastMonthCommand_CanExecute() => (this.SelectedTab == Tabs.BooksTab || this.SelectedTab == Tabs.DailyGraphTab) && this.DisplayedPeriodKind == PeriodKind.Monthly;
        /// <summary>
        /// 先月表示コマンド処理
        /// </summary>
        private async void GoToLastMonthCommand_Executed()
        {
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
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
            DateOnly thisMonth = DateOnlyExtensions.Today.GetFirstDateOfMonth();
            // 帳簿/月間一覧タブを選択している かつ 今月が表示されていない
            return (this.SelectedTab == Tabs.BooksTab || this.SelectedTab == Tabs.DailyGraphTab) &&
                   (this.DisplayedPeriodKind == PeriodKind.Selected || !(thisMonth <= this.DisplayedMonth && this.DisplayedMonth < thisMonth.AddMonths(1)));
        }
        /// <summary>
        /// 今月表示コマンド処理
        /// </summary>
        private async void GoToThisMonthCommand_Executed()
        {
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                this.DisplayedMonth = DateOnlyExtensions.Today.GetFirstDateOfMonth();
                await this.UpdateAsync(isUpdateBookList: true, isScroll: true);
            }
        }

        /// <summary>
        /// 翌月表示コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        /// <remarks>帳簿/月間一覧タブを選択している</remarks>
        private bool GoToNextMonthCommand_CanExecute() => (this.SelectedTab == Tabs.BooksTab || this.SelectedTab == Tabs.DailyGraphTab) && this.DisplayedPeriodKind == PeriodKind.Monthly;
        /// <summary>
        /// 翌月表示コマンド処理
        /// </summary>
        private async void GoToNextMonthCommand_Executed()
        {
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
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
            SelectPeriodRequestEventArgs e = new() {
                DbHandlerFactory = this.mDbHandlerFactory,
                TermKind = this.DisplayedPeriodKind,
                Month = this.DisplayedMonth,
                Period = new(this.StartDate, this.EndDate)
            };

            this.SelectPeriodRequested?.Invoke(this, e);
            if (e.Result) {
                using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                    this.StartDate = e.Period.Start;
                    this.EndDate = e.Period.End;

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
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
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
            DateOnly thisYear = DateOnlyExtensions.Today.GetFirstDateOfFiscalYear(this.FiscalStartMonth);
            // 月別一覧/月別グラフ/年別一覧/年別グラフタブを選択している かつ 今年が表示されていない
            return (this.SelectedTab is Tabs.MonthlyListTab or Tabs.MonthlyGraphTab or Tabs.YearlyListTab or Tabs.YearlyGraphTab) &&
                   !(thisYear <= this.DisplayedYear && this.DisplayedYear < thisYear.AddYears(1));
        }
        /// <summary>
        /// 今年表示コマンド処理
        /// </summary>
        private async void GoToThisYearCommand_Executed()
        {
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                this.DisplayedYear = DateOnlyExtensions.Today.GetFirstDateOfFiscalYear(this.FiscalStartMonth);
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
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
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
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
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
                DbHandlerFactory = this.mDbHandlerFactory
            };
            this.SettingsRequested?.Invoke(this, e);

            if (e.Result) {
                using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                    Properties.Settings settings = Properties.Settings.Default;
                    this.FiscalStartMonth = settings.App_StartMonth;
                    if (!await DateTimeExtensions.DownloadHolidayListAsync(settings.App_NationalHolidayCsv_Uri, settings.App_NationalHolidayCsv_TextEncoding, settings.App_NationalHolidayCsv_DateIndex)) {
                        // 祝日取得失敗を通知する
                        NotificationUtil.NotifyFailingToGetHolidayList();
                    }

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
                DbHandlerFactory = this.mDbHandlerFactory,
                InitialBookId = this.BookSelectorVM.SelectedKey
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

            this.mChildrenVM.AddRange(
                [
                    this.BookTabVM,
                    this.DailyGraphTabVM,
                    this.MonthlySummaryTabVM,
                    this.MonthlyGraphTabVM,
                    this.YearlySummaryTabVM,
                    this.YearlyGraphTabVM
                ]
            );

            this.BookSelectorVM.LoaderAsync = async () => await this.mService.LoadBookListAsync(this.DisplayedPeriod, Properties.Resources.ListName_AllBooks);
        }

        public override void Initialize(WaitCursorManagerFactory waitCursorManagerFactory, DbHandlerFactory dbHandlerFactory)
        {
            base.Initialize(waitCursorManagerFactory, dbHandlerFactory);

            this.mService = new(this.mDbHandlerFactory);
            this.mDbImportService = new(this.mDbHandlerFactory);
        }

        public override void AddEventHandlers()
        {
            using FuncLog funcLog = new();

            Properties.Settings settings = Properties.Settings.Default;

            // タブ選択変更時
            this.SelectedTabChanged += async (sender, e) => {
                using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, methodName: nameof(this.SelectedTabChanged));

                using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create(methodName: nameof(this.SelectedTabChanged))) {
                    settings.MainWindow_SelectedTabIndex = e.NewValue;
                    settings.Save();

                    await this.UpdateAsync(isUpdateBookList: true, isScroll: true);
                }
            };
            // 帳簿選択変更時
            this.BookSelectorVM.SelectedChanged += async (sender, e) => {
                using WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create(methodName: nameof(this.BookSelectorVM.SelectedChanged));

                settings.MainWindow_SelectedBookId = (int?)e.NewValue ?? -1;
                settings.Save();

                await this.UpdateAsync(isScroll: true);
            };
            // グラフ種別1選択変更時
            this.SelectedGraphKind1Changed += async (sender, e) => {
                using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, methodName: nameof(this.SelectedGraphKind1Changed));

                using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create(methodName: nameof(this.SelectedGraphKind1Changed))) {
                    settings.MainWindow_SelectedGraphKindIndex = (int)e.NewValue;
                    settings.Save();

                    await this.UpdateAsync();
                }
            };
            // グラフ種別2選択変更時
            this.SelectedGraphKind2Changed += async (sender, e) => {
                using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, methodName: nameof(this.SelectedGraphKind2Changed));

                using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create(methodName: nameof(this.SelectedGraphKind2Changed))) {
                    settings.MainWindow_SelectedGraphKind2Index = (int)e.NewValue;
                    settings.Save();

                    await this.UpdateAsync();
                }
            };
            // 系列選択変更時
            this.SelectedSeriesChanged += (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.SelectedSeriesChanged));

                using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create(methodName: nameof(this.SelectedSeriesChanged))) {
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

            this.mChildrenVM.ForEach(childVM => {
                childVM.OpenFolderDialogRequested += (sender, e) => this.OpenFolderDialogRequest(e);
                childVM.OpenFileDialogRequested += (sender, e) => this.OpenFileDialogRequest(e);
                childVM.SaveFileDialogRequested += (sender, e) => this.SaveFileDialogRequest(e);
                childVM.AddEventHandlers();
            });
        }

        public override async Task LoadAsync()
        {
            using FuncLog funcLog = new();

            Properties.Settings settings = Properties.Settings.Default;
            this.FiscalStartMonth = settings.App_StartMonth;

            this.SelectedDBKind = this.mDbHandlerFactory.DBKind;

            // 帳簿リスト更新
            await this.BookSelectorVM.LoadAsync(settings.MainWindow_SelectedBookId);

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
        protected override (double, double) WindowSizeSettingRaw {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return (settings.MainWindow_Width, settings.MainWindow_Height);
            }
            set {
                Properties.Settings settings = Properties.Settings.Default;
                settings.MainWindow_Width = value.Item1;
                settings.MainWindow_Height = value.Item2;
                settings.Save();
            }
        }

        public override Point WindowPointSetting {
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

        public override int WindowStateSetting {
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

            this.UpdateOperationLogFileMenuList();
            if (isUpdateBookList) {
                await this.BookSelectorVM.LoadAsync();
            }

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
        /// 操作ログファイルメニューリストを更新する
        /// </summary>
        public void UpdateOperationLogFileMenuList()
        {
            this.OperationLogFileMenuList.Clear();
            List<string> logFileList = [.. LogImpl.GetLogFiles()];
            logFileList.Reverse();
            int count = 0;
            foreach (string logFile in logFileList) {
                count++;
                this.OperationLogFileMenuList.Add(new MenuItemViewModel {
                    Header = $"{count}: {Path.GetFileName(logFile).Replace("_", "__")}",
                    Command = new RelayCommand(() => {
                        try {
                            _ = Process.Start(new ProcessStartInfo {
                                FileName = logFile,
                                UseShellExecute = true
                            });
                        }
                        catch (Exception) { }
                    })
                });
                if (10 <= count) { break; }
            }
        }
    }
}

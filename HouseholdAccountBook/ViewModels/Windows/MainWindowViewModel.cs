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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
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
        private AppCommonService mService;
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
        /// 帳簿選択変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<BookIdObj>> SelectedBookChanged;
        /// <summary>
        /// タブ選択変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<Tabs>> SelectedTabChanged;
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
        public event EventHandler<ChangedEventArgs<(BalanceKind?, CategoryIdObj, ItemIdObj)?>> SelectedSeriesChanged {
            add {
                this.BookTabVM.SelectedSeriesChanged += value;
                this.DailyGraphTabVM.SelectedSeriesChanged += value;
                this.MonthlySummaryTabVM.SelectedSeriesChanged += value;
                this.MonthlyGraphTabVM.SelectedSeriesChanged += value;
                this.YearlySummaryTabVM.SelectedSeriesChanged += value;
                this.YearlyGraphTabVM.SelectedSeriesChanged += value;
            }
            remove {
                this.BookTabVM.SelectedSeriesChanged -= value;
                this.DailyGraphTabVM.SelectedSeriesChanged -= value;
                this.MonthlySummaryTabVM.SelectedSeriesChanged -= value;
                this.MonthlyGraphTabVM.SelectedSeriesChanged -= value;
                this.YearlySummaryTabVM.SelectedSeriesChanged -= value;
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
        public ObservableCollection<MenuItemViewModel> OperationLogFileMenuList { get; } = [];

        /// <summary>
        /// 選択されたDB種別
        /// </summary>
        public DBKind SelectedDBKind {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    this.RaisePropertyChanged(nameof(this.IsPostgreSQL));
                    this.RaisePropertyChanged(nameof(this.IsSQLite));
                }
            }
        }
        /// <summary>
        /// PostgreSQLか
        /// </summary>
        public bool IsPostgreSQL => this.SelectedDBKind == DBKind.PostgreSQL;
        /// <summary>
        /// SQLiteか
        /// </summary>
        public bool IsSQLite => this.SelectedDBKind == DBKind.SQLite;

        /// <summary>
        /// 選択されたタブインデックス
        /// </summary>
        public int SelectedTabIndex {
            get => (int)this.SelectedTab;
            set => this.SelectedTab = EnumUtil.SafeParseEnum(value, Tabs.BooksTab);
        }
        /// <summary>
        /// 選択されたタブ種別
        /// </summary>
        public Tabs SelectedTab {
            get;
            set {
                Tabs oldValue = field;
                if (this.SetProperty(ref field, value)) {
                    this.SelectedTabChanged?.Invoke(this, new() { OldValue = oldValue, NewValue = value });
                    this.RaisePropertyChanged(nameof(this.SelectedTabIndex));
                    this.RaisePropertyChanged(nameof(this.DisplayedStart));
                    this.RaisePropertyChanged(nameof(this.DisplayedEnd));
                }
            }
        }

        /// <summary>
        /// 帳簿セレクタVM
        /// </summary>
        public SelectorViewModel<BookModel, BookIdObj> BookSelectorVM => field ??= new(static vm => vm?.Id, this.mBusyService);

        /// <summary>
        /// 選択された収支種別
        /// </summary>
        public BalanceKind? SelectedBalanceKind { get; private set; }
        /// <summary>
        /// 選択された分類ID
        /// </summary>
        public CategoryIdObj SelectedCategoryId { get; private set; }
        /// <summary>
        /// 選択された項目ID
        /// </summary>
        public ItemIdObj SelectedItemId { get; private set; }

        /// <summary>
        /// 表示開始日付
        /// </summary>
        public DateOnly DisplayedStart => this.SelectedTab switch {
            Tabs.BooksTab or Tabs.DailyGraphTab => this.StartDate,
            Tabs.MonthlyListTab or Tabs.MonthlyGraphTab => this.DisplayedStartMonth,
            Tabs.YearlyGraphTab or Tabs.YearlyListTab => this.DisplayedStartYear,
            _ => this.StartDate,
        };
        /// <summary>
        /// 表示終了日付
        /// </summary>
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

        /// <summary>
        /// 会計開始月
        /// </summary>
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

        /// <summary>
        /// 表示区間種別
        /// </summary>
        public PeriodKind DisplayedPeriodKind {
            get {
                DateOnly lastDate = this.StartDate.GetLastDateOfMonth();
                return (this.StartDate.Day == 1 && this.EndDate == lastDate) ? PeriodKind.Monthly : PeriodKind.Selected;
            }
        }

        /// <summary>
        /// 表示月
        /// </summary>
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

        /// <summary>
        /// 表示開始日
        /// </summary>
        public DateOnly StartDate {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    this.RaisePropertyChanged(nameof(this.DisplayedMonth));
                }
            }
        } = DateOnlyExtensions.Today.GetFirstDateOfMonth();
        /// <summary>
        /// 表示終了日
        /// </summary>
        public DateOnly EndDate {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    this.RaisePropertyChanged(nameof(this.DisplayedMonth));
                }
            }
        } = DateOnlyExtensions.Today.GetLastDateOfMonth();

        /// <summary>
        /// 表示年
        /// </summary>
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

        /// <summary>
        /// 表示月リスト(月別一覧の月)
        /// </summary>
        public IList<DateOnly> DisplayedMonths {
            get {
                DateOnly tmpMonth = this.DisplayedYear.GetFirstDateOfFiscalYear(this.FiscalStartMonth);

                // 表示する月の文字列を作成する
                IList<DateOnly> displayedMonths = [];
                for (int i = 0; i < 12; ++i) {
                    displayedMonths.Add(tmpMonth);
                    tmpMonth = tmpMonth.AddMonths(1);
                }
                return displayedMonths;
            }
        }
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
        public IList<DateOnly> DisplayedYears {
            get {
                DateOnly tmpYear = this.DisplayedYear.GetFirstDateOfFiscalYear(this.FiscalStartMonth).AddYears(-9);
                IList<DateOnly> displayedYears = [];
                for (int i = 0; i < 10; ++i) {
                    displayedYears.Add(tmpYear);
                    tmpYear = tmpYear.AddYears(1);
                }
                return displayedYears;
            }
        }
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
        /// グラフ種別1セレクタVM
        /// </summary>
        public SelectorViewModel<KeyValuePair<GraphKind1, string>, GraphKind1> GraphKind1SelectorVM => field ??= new(static p => p.Key);
        /// <summary>
        /// 選択されたグラフ種別1インデックス
        /// </summary>
        public int SelectedGraphKind1Index {
            get => (int)this.GraphKind1SelectorVM.SelectedKey;
            set => this.GraphKind1SelectorVM.SelectedKey = EnumUtil.SafeParseEnum(value, GraphKind1.IncomeAndExpensesGraph);
        }

        /// <summary>
        /// グラフ種別2セレクタVM
        /// </summary>
        public SelectorViewModel<KeyValuePair<GraphKind2, string>, GraphKind2> GraphKind2SelectorVM => field ??= new(static p => p.Key);
        /// <summary>
        /// 選択されたグラフ種別2インデックス
        /// </summary>
        public int SelectedGraphKind2Index {
            get => (int)this.GraphKind2SelectorVM.SelectedKey;
            set => this.GraphKind2SelectorVM.SelectedKey = EnumUtil.SafeParseEnum(value, GraphKind2.CategoryGraph);
        }
        #endregion

        #region タブVM
        /// <summary>
        /// 帳簿タブVM
        /// </summary>
        public MainWindowBookTabViewModel BookTabVM => field ??= new(this, Tabs.BooksTab);
        /// <summary>
        /// 日別グラフタブVM
        /// </summary>
        public MainWindowGraphTabViewModel DailyGraphTabVM => field ??= new(this, Tabs.DailyGraphTab);
        /// <summary>
        /// 月別一覧タブVM
        /// </summary>
        public MainWindowListTabViewModel MonthlySummaryTabVM => field ??= new(this, Tabs.MonthlyListTab);
        /// <summary>
        /// 月別グラフタブVM
        /// </summary>
        public MainWindowGraphTabViewModel MonthlyGraphTabVM => field ??= new(this, Tabs.MonthlyGraphTab);
        /// <summary>
        /// 年別一覧タブVM
        /// </summary>
        public MainWindowListTabViewModel YearlySummaryTabVM => field ??= new(this, Tabs.YearlyListTab);
        /// <summary>
        /// 年別グラフタブVM
        /// </summary>
        public MainWindowGraphTabViewModel YearlyGraphTabVM => field ??= new(this, Tabs.YearlyGraphTab);
        #endregion
        #endregion

        #region コマンド
        #region ファイルコマンド
        /// <summary>
        /// ファイルコマンド
        /// </summary>
        public ICommand FileMenuCommand => field ??= new RelayCommand();

        /// <summary>
        /// インポートコマンド
        /// </summary>
        public ICommand ImportCommand => field ??= new RelayCommand(() => !this.IsChildrenWindowOpened());

        /// <summary>
        /// 記帳風月インポートコマンド
        /// </summary>
        public ICommand ImportKichoFugetsuDbCommand => field ??= new AsyncRelayCommand(
            this.ImportKichoFugetsuDbCommand_ExecuteAsync, this,
            () => !this.IsChildrenWindowOpened(), this.mBusyService);
        /// <summary>
        /// 記帳風月インポートコマンド処理
        /// </summary>
        /// <param name="token">キャンセル用トークン</param>
        /// <param name="progress">進捗</param>
        public async Task ImportKichoFugetsuDbCommand_ExecuteAsync(CancellationToken token, IProgress<int> progress)
        {
            (string directory, string fileName) = PathUtil.GetSeparatedPath(UserSettingService.Instance.KichoFugetsuFilePath, App.GetCurrentDir());

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

            UserSettingService.Instance.KichoFugetsuFilePath = e.FileName;

            bool? result;
            int actRowsDiff = 0;
            OleDbHandler.ConnectInfo info = new(UserSettingService.Instance.KichoFugetsuConnectInfo.Provider) {
                DataSource = e.FileName
            };
            try {
                (result, actRowsDiff) = await this.mDbImportService.ImportKichoFugetsuDbAsync(info, token, progress);
            }
            catch (OperationCanceledException) {
                result = null;
            }

            if (result == true) {
                await this.UpdateAsync(isUpdateBookList: true, isScroll: true, isUpdateActDateLastEdited: true);
            }

            switch (result) {
                case true:
                    _ = MessageBox.Show(Properties.Resources.Message_FinishToImport + (0 < actRowsDiff ? Environment.NewLine + Properties.Resources.Message_DeletedZeroValueInformation : string.Empty),
                                        Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                    break;
                case null:
                    _ = MessageBox.Show(Properties.Resources.Message_CanceledToImport, Properties.Resources.Title_Warning, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                    break;
                case false:
                    _ = MessageBox.Show(Properties.Resources.Message_FoultToImport, Properties.Resources.Title_Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    break;
            }
        }

        /// <summary>
        /// PostgreSQL -> SQLite インポートコマンド
        /// </summary>
        public ICommand ImportPostgreSQLCommand => field ??= new AsyncRelayCommand(
            this.ImportPostgreSQLCommand_ExecuteAsync, this,
            () => this.IsSQLite && !this.IsChildrenWindowOpened(), this.mBusyService);
        /// <summary>
        /// PostgreSQL -> SQLite インポートコマンド処理
        /// </summary>
        /// <param name="token">キャンセル用トークン</param>
        /// <param name="progress">進捗</param>
        public async Task ImportPostgreSQLCommand_ExecuteAsync(CancellationToken token, IProgress<int> progress)
        {
            if (MessageBox.Show(Properties.Resources.Message_DeleteOldDataNotification, Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) != MessageBoxResult.OK) {
                return;
            }

            bool? result;
            NpgsqlDbHandler.ConnectInfo info = UserSettingService.Instance.NpgsqlConnectInfo;
            try {
                result = await this.mDbImportService.ImportPostgreSQLAsync(info, token, progress);
            }
            catch (OperationCanceledException) {
                result = null;
            }

            if (result == true) {
                await this.UpdateAsync(isUpdateBookList: true, isScroll: true, isUpdateActDateLastEdited: true);
            }

            switch (result) {
                case true:
                    _ = MessageBox.Show(Properties.Resources.Message_FinishToImport, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                    break;
                case null:
                    _ = MessageBox.Show(Properties.Resources.Message_CanceledToImport, Properties.Resources.Title_Warning, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                    break;
                case false:
                    _ = MessageBox.Show(Properties.Resources.Message_FoultToImport, Properties.Resources.Title_Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    break;
            }
        }

        /// <summary>
        /// カスタムファイル -> PostgreSQL インポートコマンド
        /// </summary>
        public ICommand ImportCustomFileCommand => field ??= new AsyncRelayCommand(
            this.ImportCustomFileCommand_ExecuteAsync, this,
            () => this.IsPostgreSQL && !string.IsNullOrEmpty(DbBackUpManager.Instance.NpgsqlBackupConfig?.RestoreExePath) && !this.IsChildrenWindowOpened(),
            this.mBusyService);
        /// <summary>
        /// カスタムファイル -> PostgreSQL インポートコマンド処理
        /// </summary>
        /// <param name="progress">進捗</param>
        public async Task ImportCustomFileCommand_ExecuteAsync(IProgress<int> progress)
        {
            (string directory, string fileName) = PathUtil.GetSeparatedPath(UserSettingService.Instance.ImportCustomFilePath, App.GetCurrentDir());

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

            UserSettingService.Instance.ImportCustomFilePath = e.FileName;

            bool result = await this.mDbImportService.ImportCustomFileAsync(e.FileName);
            if (result) {
                await this.UpdateAsync(isUpdateBookList: true, isScroll: true, isUpdateActDateLastEdited: true);
            }

            _ = result
                ? MessageBox.Show(Properties.Resources.Message_FinishToImport, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK)
                : MessageBox.Show(Properties.Resources.Message_FoultToImport, Properties.Resources.Title_Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        }

        /// <summary>
        /// SQLiteファイル -> PostgreSQL/SQLite インポートコマンド
        /// </summary>
        public ICommand ImportSQLiteFileCommand => field ??= new AsyncRelayCommand(
            this.ImportSQLiteFileCommand_ExecuteAsync, this,
            () => (this.IsPostgreSQL || this.IsSQLite) && !this.IsChildrenWindowOpened(), this.mBusyService);
        /// <summary>
        /// SQLiteファイル -> PostgreSQL/SQLite インポートコマンド処理
        /// </summary>
        /// <param name="token">キャンセル用トークン</param>
        /// <param name="progress">進捗</param>
        public async Task ImportSQLiteFileCommand_ExecuteAsync(CancellationToken token, IProgress<int> progress)
        {
            (string directory, string fileName) = PathUtil.GetSeparatedPath(UserSettingService.Instance.ImportSQLiteFilePath, App.GetCurrentDir());

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

            UserSettingService.Instance.ImportSQLiteFilePath = e.FileName;

            bool? result = false;
            try {
                switch (this.SelectedDBKind) {
                    case DBKind.SQLite: {
                        result = DbImportService.ImportSQLite(e.FileName, UserSettingService.Instance.SQLiteConnectInfo.FilePath, token, progress);
                        break;
                    }
                    case DBKind.PostgreSQL: {
                        result = await this.mDbImportService.ImportSQLiteAsync(e.FileName, token, progress);
                        break;
                    }
                }
            }
            catch (OperationCanceledException) {
                result = null;
            }

            if (result == true) {
                await this.UpdateAsync(isUpdateBookList: true, isScroll: true, isUpdateActDateLastEdited: true);
            }

            switch (result) {
                case true:
                    _ = MessageBox.Show(Properties.Resources.Message_FinishToImport, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                    break;
                case null:
                    _ = MessageBox.Show(Properties.Resources.Message_CanceledToImport, Properties.Resources.Title_Warning, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                    break;
                case false:
                    _ = MessageBox.Show(Properties.Resources.Message_FoultToImport, Properties.Resources.Title_Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    break;
            }
        }

        /// <summary>
        /// エクスポートコマンド
        /// </summary>
        public ICommand ExportCommand => field ??= new RelayCommand(() => !this.IsChildrenWindowOpened());

        /// <summary>
        /// カスタムファイルエクスポートコマンド
        /// </summary>
        public ICommand ExportCustomFileCommand => field ??= new AsyncRelayCommand(
            this.ExportCustomFileCommand_ExecuteAsync, this,
            () => this.IsPostgreSQL && !string.IsNullOrEmpty(DbBackUpManager.Instance.NpgsqlBackupConfig?.DumpExePath) && !this.IsChildrenWindowOpened(),
            this.mBusyService);
        /// <summary>
        /// カスタムファイルエクスポートコマンド処理
        /// </summary>
        /// <param name="progress">進捗</param>
        public async Task ExportCustomFileCommand_ExecuteAsync(IProgress<int> progress)
        {
            (string directory, string fileName) = PathUtil.GetSeparatedPath(UserSettingService.Instance.ExportCustomFilePath, App.GetCurrentDir());

            SaveFileDialogRequestEventArgs e = new() {
                InitialDirectory = directory,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = Properties.Resources.FileSelectFilter_CustomFormatFile + "|*.*"
            };
            if (!this.SaveFileDialogRequest(e)) { return; }

            UserSettingService.Instance.ExportCustomFilePath = e.FileName;

            bool result = await DbBackUpManager.Instance.ExecuteDumpPostgreSQLAsync(e.FileName, PostgresFormat.Custom) == true;

            _ = result
                ? MessageBox.Show(Properties.Resources.Message_FinishToExport, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK)
                : MessageBox.Show(Properties.Resources.Message_FoultToExport, Properties.Resources.Title_Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        }

        /// <summary>
        /// SQLファイルエクスポートコマンド
        /// </summary>
        public ICommand ExportSQLFileCommand => field ??= new AsyncRelayCommand(
            this.ExportSQLFileCommand_ExecuteAsync, this,
            () => this.IsPostgreSQL && !string.IsNullOrEmpty(DbBackUpManager.Instance.NpgsqlBackupConfig?.DumpExePath) && !this.IsChildrenWindowOpened(),
            this.mBusyService);
        /// <summary>
        /// SQLファイルエクスポートコマンド処理
        /// </summary>
        /// <param name="progress">進捗</param>
        public async Task ExportSQLFileCommand_ExecuteAsync(IProgress<int> progress)
        {
            (string directory, string fileName) = PathUtil.GetSeparatedPath(UserSettingService.Instance.ExportSQLFilePath, App.GetCurrentDir());

            SaveFileDialogRequestEventArgs e = new() {
                InitialDirectory = directory,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = Properties.Resources.FileSelectFilter_SqlFile + "|*.sql"
            };
            if (!this.SaveFileDialogRequest(e)) { return; }

            UserSettingService.Instance.ExportSQLFilePath = e.FileName;

            bool result = await DbBackUpManager.Instance.ExecuteDumpPostgreSQLAsync(e.FileName, PostgresFormat.Plain) == true;

            _ = result
                ? MessageBox.Show(Properties.Resources.Message_FinishToExport, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK)
                : MessageBox.Show(Properties.Resources.Message_FoultToExport, Properties.Resources.Title_Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        }

        /// <summary>
        /// バックアップコマンド
        /// </summary>
        public ICommand BackUpCommand => field ??= new AsyncRelayCommand(this.BackUpCommand_ExecuteAsync, this, () => !this.IsChildrenWindowOpened(), this.mBusyService);
        /// <summary>
        /// バックアップコマンド処理
        /// </summary>
        public async Task BackUpCommand_ExecuteAsync(IProgress<int> progress)
        {
            bool result = await DbBackUpManager.Instance.CreateBackUpFileAsync(backUpNum: -1);

            if (result) {
                UserSettingService.Instance.CurrentBackUpBySelf = DateTime.Now;
                this.BookTabVM.RaiseCurrentBackUpChanged();
            }

            _ = result
                ? MessageBox.Show(Properties.Resources.Message_FinishToBackup, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK)
                : MessageBox.Show(Properties.Resources.Message_FoultToBackup, Properties.Resources.Title_Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        }

        /// <summary>
        /// 操作ログファイルコマンド
        /// </summary>
        public ICommand OperationLogFileCommand => field ??= new RelayCommand(() => 0 < this.OperationLogFileMenuList.Count);

        /// <summary>
        /// ウィンドウ終了コマンド
        /// </summary>
        public ICommand ExitWindowCommand => field ??= new RelayCommand(this.ExitWindowCommand_Execute, () => !this.IsChildrenWindowOpened());
        /// <summary>
        /// ウィンドウ終了コマンド処理
        /// </summary>
        public void ExitWindowCommand_Execute() => this.CloseRequest(new DialogCloseRequestEventArgs(true));
        #endregion

        #region 表示コマンド
        /// <summary>
        /// 表示コマンド
        /// </summary>
        public ICommand ShowMenuCommand => field ??= new RelayCommand();

        /// <summary>
        /// 帳簿項目タブ表示コマンド
        /// </summary>
        public ICommand ShowBookTabCommand => field ??= new RelayCommand(() => this.SelectedTab = Tabs.BooksTab, () => this.SelectedTab != Tabs.BooksTab);

        /// <summary>
        /// 日別グラフタブ表示コマンド
        /// </summary>
        public ICommand ShowDailyGraphTabCommand => field ??= new RelayCommand(() => this.SelectedTab = Tabs.DailyGraphTab, () => this.SelectedTab != Tabs.DailyGraphTab);

        /// <summary>
        /// 先月表示コマンド
        /// </summary>
        /// <remarks>帳簿/月間一覧タブを選択している</remarks>
        public ICommand GoToLastMonthCommand => field ??= new AsyncRelayCommand(
            this.GoToLastMonthCommand_ExecuteAsync,
            () => (this.SelectedTab == Tabs.BooksTab || this.SelectedTab == Tabs.DailyGraphTab) && this.DisplayedPeriodKind == PeriodKind.Monthly, this.mBusyService);
        /// <summary>
        /// 先月表示コマンド処理
        /// </summary>
        private async Task GoToLastMonthCommand_ExecuteAsync()
        {
            this.DisplayedMonth = this.DisplayedMonth.Value.AddMonths(-1);
            await this.UpdateAsync(isUpdateBookList: true, isScroll: true);
        }

        /// <summary>
        /// 今月表示コマンド
        /// </summary>
        public ICommand GoToThisMonthCommand => field ??= new AsyncRelayCommand(this.GoToThisMonthCommand_ExecuteAsync, this.GoToThisMonthCommand_CanExecute, this.mBusyService);
        /// <summary>
        /// 今月表示コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool GoToThisMonthCommand_CanExecute()
        {
            DateOnly thisMonth = DateOnlyExtensions.Today.GetFirstDateOfMonth();
            // 帳簿/月間一覧タブを選択している かつ 今月が表示されていない
            return (this.SelectedTab == Tabs.BooksTab || this.SelectedTab == Tabs.DailyGraphTab) &&
                   (this.DisplayedPeriodKind == PeriodKind.Selected || this.DisplayedMonth < thisMonth || thisMonth.AddMonths(1) <= this.DisplayedMonth);
        }
        /// <summary>
        /// 今月表示コマンド処理
        /// </summary>
        private async Task GoToThisMonthCommand_ExecuteAsync()
        {
            this.DisplayedMonth = DateOnlyExtensions.Today.GetFirstDateOfMonth();
            await this.UpdateAsync(isUpdateBookList: true, isScroll: true);
        }

        /// <summary>
        /// 来月表示コマンド
        /// </summary>
        /// <remarks>帳簿/日別一覧タブを選択している</remarks>
        public ICommand GoToNextMonthCommand => field ??= new AsyncRelayCommand(
            this.GoToNextMonthCommand_ExecuteAsync,
            () => (this.SelectedTab == Tabs.BooksTab || this.SelectedTab == Tabs.DailyGraphTab) && this.DisplayedPeriodKind == PeriodKind.Monthly, this.mBusyService);
        /// <summary>
        /// 来月表示コマンド処理
        /// </summary>
        private async Task GoToNextMonthCommand_ExecuteAsync()
        {
            this.DisplayedMonth = this.DisplayedMonth.Value.AddMonths(1);
            await this.UpdateAsync(isUpdateBookList: true, isScroll: true);
        }

        /// <summary>
        /// 期間選択コマンド
        /// </summary>
        public ICommand SelectPeriodCommand => field ??= new AsyncRelayCommand(this.SelectPeriodCommand_ExecuteAsync, () => !this.IsChildrenWindowOpened(), this.mBusyService);
        /// <summary>
        /// 期間選択コマンド処理
        /// </summary>
        private async Task SelectPeriodCommand_ExecuteAsync()
        {
            SelectPeriodRequestEventArgs e = new() {
                DbHandlerFactory = this.mDbHandlerFactory,
                TermKind = this.DisplayedPeriodKind,
                Month = this.DisplayedMonth,
                Period = new(this.StartDate, this.EndDate)
            };

            this.SelectPeriodRequested?.Invoke(this, e);
            if (e.Result) {
                this.StartDate = e.Period.Start;
                this.EndDate = e.Period.End;

                await this.UpdateAsync(isUpdateBookList: true, isScroll: true);
            }
        }

        /// <summary>
        /// 月別一覧タブ表示コマンド
        /// </summary>
        public ICommand ShowMonthlyListTabCommand => field ??= new RelayCommand(() => this.SelectedTab = Tabs.MonthlyListTab, () => this.SelectedTab != Tabs.MonthlyListTab);

        /// <summary>
        /// 月別グラフタブ表示コマンド
        /// </summary>
        public ICommand ShowMonthlyGraphTabCommand => field ??= new RelayCommand(() => this.SelectedTab = Tabs.MonthlyGraphTab, () => this.SelectedTab != Tabs.MonthlyGraphTab);

        /// <summary>
        /// 年別一覧タブ表示コマンド
        /// </summary>
        public ICommand ShowYearlyListTabCommand => field ??= new RelayCommand(() => this.SelectedTab = Tabs.YearlyListTab, () => this.SelectedTab != Tabs.YearlyListTab);

        /// <summary>
        /// 年別グラフタブ表示コマンド
        /// </summary>
        public ICommand ShowYearlyGraphTabCommand => field ??= new RelayCommand(() => this.SelectedTab = Tabs.YearlyGraphTab, () => this.SelectedTab != Tabs.YearlyGraphTab);

        /// <summary>
        /// 去年表示コマンド
        /// </summary>
        /// <remarks>月別一覧/月別グラフ/年別一覧/年別グラフタブを選択している</remarks>
        public ICommand GoToLastYearCommand => field ??= new AsyncRelayCommand(
            this.GoToLastYearCommand_ExecuteAsync,
            () => this.SelectedTab is Tabs.MonthlyListTab or Tabs.MonthlyGraphTab or Tabs.YearlyListTab or Tabs.YearlyGraphTab, this.mBusyService);
        /// <summary>
        /// 去年表示コマンド処理
        /// </summary>
        private async Task GoToLastYearCommand_ExecuteAsync()
        {
            this.DisplayedYear = this.DisplayedYear.AddYears(-1);
            await this.UpdateAsync(isUpdateBookList: true);
        }

        /// <summary>
        /// 今年表示コマンド
        /// </summary>
        public ICommand GoToThisYearCommand => field ??= new AsyncRelayCommand(this.GoToThisYearCommand_ExecuteAsync, this.GoToThisYearCommand_CanExecute, this.mBusyService);
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
        private async Task GoToThisYearCommand_ExecuteAsync()
        {
            this.DisplayedYear = DateOnlyExtensions.Today.GetFirstDateOfFiscalYear(this.FiscalStartMonth);
            await this.UpdateAsync(isUpdateBookList: true);
        }

        /// <summary>
        /// 来年表示コマンド
        /// </summary>
        /// <remarks>月別一覧/月別グラフ/年別一覧/年別グラフタブを選択している</remarks>
        public ICommand GoToNextYearCommand => field ??= new AsyncRelayCommand(
            this.GoToNextYearCommand_ExecuteAsync,
            () => this.SelectedTab is Tabs.MonthlyListTab or Tabs.MonthlyGraphTab or Tabs.YearlyListTab or Tabs.YearlyGraphTab, this.mBusyService);
        /// <summary>
        /// 来年表示コマンド処理
        /// </summary>
        private async Task GoToNextYearCommand_ExecuteAsync()
        {
            this.DisplayedYear = this.DisplayedYear.AddYears(1);
            await this.UpdateAsync(isUpdateBookList: true);
        }

        /// <summary>
        /// 更新コマンド
        /// </summary>
        public ICommand UpdateCommand => field ??= new AsyncRelayCommand(async () => await this.UpdateAsync(isUpdateBookList: true), null, this.mBusyService);
        #endregion

        #region デバッグコマンド
        /// <summary>
        /// デバッグコマンド
        /// </summary>
        public ICommand DebugCommand => field ??= new RelayCommand(() => !this.IsChildrenWindowOpened());

        /// <summary>
        /// 未処理例外スローコマンド
        /// </summary>
        public ICommand ThrowUnhandledExceptionCommand => field ??= new RelayCommand(static () => throw new Exception("for debug"), () => true);

        /// <summary>
        /// 未観測タスク例外スローコマンド
        /// </summary>
        public ICommand ThrowUnobservedTaskExceptionCommand => field ??= new RelayCommand(static () => new Task(static () => throw new Exception("for debug")).Start(), () => true);
        #endregion

        #region ツールコマンド
        /// <summary>
        /// ツールメニューコマンド
        /// </summary>
        public ICommand ToolMenuCommand => field ??= new RelayCommand(() => !this.IsChildrenWindowOpened());

        /// <summary>
        /// 設定コマンド
        /// </summary>
        public ICommand SettingsCommand => field ??= new AsyncRelayCommand(this.SettingsCommand_ExecuteAsync, () => !this.IsChildrenWindowOpened(), this.mBusyService);
        /// <summary>
        /// 設定コマンド処理
        /// </summary>
        private async Task SettingsCommand_ExecuteAsync()
        {
            SettingsRequestEventArgs e = new() {
                DbHandlerFactory = this.mDbHandlerFactory
            };
            this.SettingsRequested?.Invoke(this, e);

            if (e.Result) {
                this.FiscalStartMonth = UserSettingService.Instance.FiscalStartMonth;
                if (!await HolidayService.Instance.DownloadHolidayListAsync(UserSettingService.Instance.HolidayCSVConfig)) {
                    // 祝日取得失敗を通知する
                    NotificationService.NotifyFailingToGetHolidayList();
                }

                await this.UpdateAsync(isUpdateBookList: true);
            }
        }

        /// <summary>
        /// 帳簿内ツールコマンド
        /// </summary>
        public ICommand ToolInBookCommand => field ??= new RelayCommand(() => this.SelectedTab == Tabs.BooksTab);

        /// <summary>
        /// CSVファイル比較コマンド
        /// </summary>
        public ICommand CompareCsvFileCommand => field ??= new RelayCommand(this.CompareCsvFileCommand_Execute, () => !this.IsChildrenWindowOpened());
        /// <summary>
        /// CSV比較コマンド処理
        /// </summary>
        private void CompareCsvFileCommand_Execute()
        {
            CompareCsvFileRequestEventArgs e = new() {
                DbHandlerFactory = this.mDbHandlerFactory,
                InitialBookId = this.BookSelectorVM.SelectedKey
            };
            this.CompareCsvFileRequested?.Invoke(this, e);
        }
        #endregion

        #region ヘルプコマンド
        /// <summary>
        /// ヘルプメニューコマンド
        /// </summary>
        public ICommand HelpMenuCommand => field ??= new RelayCommand(() => true);

        /// <summary>
        /// 更新の確認コマンド
        /// </summary>
        public ICommand CheckUpdateCommand => field ??= new AsyncRelayCommand(static async () => await App.CheckLatestVersionAsync(true));

        /// <summary>
        /// リリースノートコマンド
        /// </summary>
        public ICommand OpenReleaseNoteCommand => field ??= new RelayCommand(this.OpenReleaseNoteCommand_Execute);
        /// <summary>
        /// リリースノートコマンド処理
        /// </summary>
        private void OpenReleaseNoteCommand_Execute()
        {
            string releasesUrl = "https://github.com/vtr00/HouseholdAccountBook/releases";
            if (releasesUrl != null) {
                Log.Info($"Open releases url: {releasesUrl}");
                try {
                    _ = Process.Start(new ProcessStartInfo() {
                        FileName = releasesUrl,
                        UseShellExecute = true
                    });
                }
                catch (Exception) { }
            }
        }

        /// <summary>
        /// バージョン表示コマンド
        /// </summary>
        public ICommand ShowVersionCommand => field ??= new RelayCommand(() => this.ShowVersionRequested?.Invoke(this, EventArgs.Empty), () => !this.IsChildrenWindowOpened());
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
        }

        public override void Initialize(DbHandlerFactory dbHandlerFactory)
        {
            base.Initialize(dbHandlerFactory);

            this.mService = new(this.mDbHandlerFactory);
            this.mDbImportService = new(this.mDbHandlerFactory);

            this.BookSelectorVM.SetLoader(async () => await this.mService.LoadBookListAsync(this.DisplayedPeriod, Properties.Resources.ListName_AllBooks));
            this.GraphKind1SelectorVM.SetLoader(() => GraphKind1Str);
            this.GraphKind2SelectorVM.SetLoader(() => GraphKind2Str);
        }

        public override void AddEventHandlers()
        {
            using FuncLog funcLog = new();

            // タブ選択変更時
            this.SelectedTabChanged += async (sender, e) => {
                using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, methodName: nameof(this.SelectedTabChanged));

                using IDisposable disposable = this.mBusyService.Enter();
                UserSettingService.Instance.SelectedTab = e.NewValue;

                await this.UpdateAsync(isUpdateBookList: true, isScroll: true);
            };
            // 帳簿選択変更時
            this.BookSelectorVM.SelectionChanged += async (sender, e) => {
                this.SelectedBookChanged?.Invoke(sender, e);

                UserSettingService.Instance.SelectedBookId = e.NewValue ?? BookIdObj.System;

                await this.UpdateAsync(isUpdateBookList: false, isScroll: true);
            };
            // グラフ種別1選択変更時
            this.GraphKind1SelectorVM.SelectionChanged += async (sender, e) => {
                this.SelectedGraphKind1Changed?.Invoke(sender, e);

                UserSettingService.Instance.SelectedGraphKind1 = e.NewValue;

                await this.UpdateAsync(isUpdateBookList: false);
            };
            // グラフ種別2選択変更時
            this.GraphKind2SelectorVM.SelectionChanged += async (sender, e) => {
                this.SelectedGraphKind2Changed?.Invoke(sender, e);

                UserSettingService.Instance.SelectedGraphKind2 = e.NewValue;

                await this.UpdateAsync(isUpdateBookList: false);
            };
            // 系列選択変更時
            this.SelectedSeriesChanged += (sender, e) => {
                using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, methodName: nameof(this.SelectedSeriesChanged));

                this.SelectedBalanceKind = e.NewValue?.Item1;
                this.SelectedCategoryId = e.NewValue?.Item2;
                this.SelectedItemId = e.NewValue?.Item3;
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
            using IDisposable disposable = this.mBusyService.Enter();

            this.FiscalStartMonth = UserSettingService.Instance.FiscalStartMonth;

            this.SelectedDBKind = this.mDbHandlerFactory.DBKind;

            // 帳簿リスト更新
            await this.BookSelectorVM.LoadAsync(UserSettingService.Instance.SelectedBookId);
            // タブ選択
            this.SelectedTab = UserSettingService.Instance.SelectedTab;
            // グラフ種別1更新
            await this.GraphKind1SelectorVM.LoadAsync(UserSettingService.Instance.SelectedGraphKind1);
            // グラフ種別2更新
            await this.GraphKind2SelectorVM.LoadAsync(UserSettingService.Instance.SelectedGraphKind2);

            Log.Vars(vars: new { this.SelectedTabIndex, this.SelectedGraphKind1Index, this.SelectedGraphKind2Index });

            await this.UpdateAsync(isUpdateBookList: false, isScroll: true, isUpdateActDateLastEdited: true);
        }

        #region ウィンドウ設定プロパティ
        protected override (double, double) WindowSizeSettingRaw {
            get => UserSettingService.Instance.MainWindowSize;
            set => UserSettingService.Instance.MainWindowSize = value;
        }

        public override Point WindowPointSetting {
            get => UserSettingService.Instance.MainWindowPoint;
            set => UserSettingService.Instance.MainWindowPoint = value;
        }

        public override int WindowStateSetting {
            get => UserSettingService.Instance.MainWindowState;
            set => UserSettingService.Instance.MainWindowState = value;
        }
        #endregion

        /// <summary>
        /// 画面更新(タブ非依存)
        /// </summary>
        /// <param name="isUpdateBookList">帳簿リストを更新するか</param>
        /// <param name="isScroll">帳簿項目一覧をスクロールするか</param>
        /// <param name="isUpdateActDateLastEdited">最後に操作した帳簿項目を更新するか</param>
        public async Task UpdateAsync(bool isUpdateBookList = true, bool isScroll = false, bool isUpdateActDateLastEdited = false)
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

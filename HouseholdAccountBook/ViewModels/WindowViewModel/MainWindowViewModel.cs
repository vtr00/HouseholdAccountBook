using HouseholdAccountBook.Extensions;
using OxyPlot;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// メインウィンドウVM
    /// </summary>
    public class MainWindowViewModel : BindableBase
    {
        #region フィールド
        /// <summary>
        /// 表示日付の更新中か
        /// </summary>
        private bool onUpdateDisplayedDate = false;
        #endregion

        #region イベント
        /// <summary>
        /// 帳簿選択変更時イベント
        /// </summary>
        public event Action SelectedBookChanged = default;
        /// <summary>
        /// グラフ種別選択変更時イベント
        /// </summary>
        public event Action SelectedGraphKindChanged = default;
        /// <summary>
        /// タブ選択変更時イベント
        /// </summary>
        public event Action SelectedTabChanged = default;
        /// <summary>
        /// 系列選択変更時イベント
        /// </summary>
        public event Action SelectedSeriesChanged = default;
        #endregion

        #region プロパティ
        #region プロパティ(共通)
        /// <summary>
        /// 選択されたタブインデックス
        /// </summary>
        #region SelectedTabIndex
        public int SelectedTabIndex
        {
            get => this._SelectedTabIndex;
            set {
                if (this.SetProperty(ref this._SelectedTabIndex, value)) {
                    this.SelectedTabChanged?.Invoke();
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
                if (this.SetProperty(ref this._SelectedBookVM, value)) {
                    this.SelectedBookChanged?.Invoke();
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
        /// 表示開始日
        /// </summary>
        public DateTime DisplayedStartDate {
            get {
                int startMonth = Properties.Settings.Default.App_StartMonth;
                switch (this.SelectedTab) {
                    case Tabs.BooksTab:
                    case Tabs.DailyGraphTab:
                        return this.StartDate;
                    case Tabs.MonthlyListTab:
                    case Tabs.MonthlyGraphTab:
                        return this.DisplayedYear.GetFirstDateOfFiscalYear(startMonth);
                    case Tabs.YearlyGraphTab:
                    case Tabs.YearlyListTab:
                        return DateTime.Now.GetFirstDateOfFiscalYear(startMonth).AddYears(-10);
                    default:
                        return this.StartDate;
                }
            }
        }
        /// <summary>
        /// 表示終了日
        /// </summary>
        public DateTime DisplayedEndDate {
            get {
                int startMonth = Properties.Settings.Default.App_StartMonth;
                switch (this.SelectedTab) {
                    case Tabs.BooksTab:
                    case Tabs.DailyGraphTab:
                        return this.EndDate;
                    case Tabs.MonthlyListTab:
                    case Tabs.MonthlyGraphTab:
                        return this.DisplayedYear.GetLastDateOfFiscalYear(startMonth);
                    case Tabs.YearlyGraphTab:
                    case Tabs.YearlyListTab:
                        return DateTime.Now.GetLastDateOfFiscalYear(startMonth);
                    default:
                        return this.EndDate;
                }
            }
        }
        /// <summary>
        /// 今日
        /// </summary>
        public DateTime ToDay => DateTime.Now;
        #endregion

        #region プロパティ(グラフ)
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
                if (this.SetProperty(ref this._SelectedGraphKind1, value)) {
                    this.SelectedGraphKindChanged?.Invoke();
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
                if (this.SetProperty(ref this._SelectedGraphKind2, value)) {
                    this.SelectedGraphKindChanged?.Invoke();
                    this.SelectedSeriesChanged?.Invoke();
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

        #region 帳簿タブ
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
            get {
                switch (this.DisplayedTermKind) {
                    case TermKind.Monthly:
                        return this.StartDate;
                    case TermKind.Selected:
                        return null;
                    default:
                        return null;
                }
            }
            set {
                DateTime? oldDisplayedMonth = this.DisplayedMonth;
                if (value != null) {
                    // 開始日/終了日を更新する
                    this.StartDate = value.Value.GetFirstDateOfMonth();
                    this.EndDate = value.Value.GetLastDateOfMonth();

                    if (!this.onUpdateDisplayedDate) {
                        this.onUpdateDisplayedDate = true;
                        // 表示月の年度の最初の月を表示年とする
                        this.DisplayedYear = value.Value.GetFirstDateOfFiscalYear(Properties.Settings.Default.App_StartMonth);
                        this.onUpdateDisplayedDate = false;
                    }
                }
                if (oldDisplayedMonth != this.DisplayedMonth) {
                    this.RaisePropertyChanged();
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
                this.SetProperty(ref this._StartDate, value);
                this.RaisePropertyChanged(nameof(this.DisplayedMonth));
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
                this.SetProperty(ref this._EndDate, value);
                this.RaisePropertyChanged(nameof(this.DisplayedMonth));
            }
        }
        private DateTime _EndDate = DateTime.Now.GetLastDateOfMonth();
        #endregion

        /// <summary>
        /// 帳簿項目VMリスト
        /// </summary>
        #region ActionVMList
        public ObservableCollection<ActionViewModel> ActionVMList
        {
            get => this._ActionVMList;
            set {
                this.SetProperty(ref this._ActionVMList, value);
                this.UpdateDisplayedActionVMList();
            }
        }
        private ObservableCollection<ActionViewModel> _ActionVMList = default;
        #endregion
        /// <summary>
        /// 表示対象の帳簿項目VMリスト
        /// </summary>
        #region DisplayedActionVMList
        public ObservableCollection<ActionViewModel> DisplayedActionVMList
        {
            get => this._DisplayedActionVMList;
            set => this.SetProperty(ref this._DisplayedActionVMList, value);
        }
        private ObservableCollection<ActionViewModel> _DisplayedActionVMList = default;
        #endregion

        /// <summary>
        /// 選択された帳簿項目VM(先頭)
        /// </summary>
        #region SelectedActionVM
        public ActionViewModel SelectedActionVM
        {
            get => this._SelectedActionVM;
            set => this.SetProperty(ref this._SelectedActionVM, value);
        }
        private ActionViewModel _SelectedActionVM = default;
        #endregion
        /// <summary>
        /// 選択された帳簿項目VMリスト
        /// </summary>
        #region SelectedActionVMList
        public ObservableCollection<ActionViewModel> SelectedActionVMList
        {
            get => this._SelectedActionVMList;
            set {
                if (value != null) {
                    //Log.Debug($"Old SelectedActionVMList.Count: {this._SelectedActionVMList.Count}");
                    //Log.Debug($"New SelectedActionVMList.Count: {value.Count}");

                    List<ActionViewModel> added = new List<ActionViewModel>(value.Except(this._SelectedActionVMList));
                    List<ActionViewModel> removed = new List<ActionViewModel>(this._SelectedActionVMList.Except(value));

                    //Log.Debug($"added.Count: {added.Count}");
                    //Log.Debug($"removed.Count: {removed.Count}");

                    foreach (ActionViewModel vm in added) {
                        this._SelectedActionVMList.Add(vm);
                    }
                    foreach (ActionViewModel vm in removed) {
                        this._SelectedActionVMList.Remove(vm);
                    }
                }
                else {
                    //Log.Debug($"Old SelectedActionVMList.Count: {this._SelectedActionVMList.Count}");
                    //Log.Debug($"New SelectedActionVMList.Count: 0(null)");

                    // null の場合はリストを空にする(ClearだとBehaviorが意図した挙動にならない)
                    while (this._SelectedActionVMList.Count > 0) {
                        this._SelectedActionVMList.RemoveAt(0);
                    }
                }
            }
        }
        private readonly ObservableCollection<ActionViewModel> _SelectedActionVMList = new ObservableCollection<ActionViewModel>();
        #endregion

        /// <summary>
        /// CSVと一致したか
        /// </summary>
        #region IsMatch
        public bool? IsMatch
        {
            get {
                int count = this.SelectedActionVMList.Count((vm) => vm.IsMatch);
                if (count == 0) return false;
                else if (count == this.SelectedActionVMList.Count()) return true;
                return null;
            }
            set {
                if (value.HasValue) {
                    foreach (ActionViewModel vm in this.SelectedActionVMList) {
                        vm.IsMatch = value.Value;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 最後に操作した帳簿項目の日付
        /// </summary>
        #region ActDateLastEdited
        public DateTime? ActDateLastEdited
        {
            get => this._ActDateLastEdited;
            set => this.SetProperty(ref this._ActDateLastEdited, value);
        }
        private DateTime? _ActDateLastEdited = null;
        #endregion

        /// <summary>
        /// 選択されたデータの平均値
        /// </summary>
        #region AverageValue
        public double? AverageValue => this.Count != 0 ? (double?)this.SumValue / this.Count : null;
        #endregion
        /// <summary>
        /// 選択されたデータの個数
        /// </summary>
        #region Count
        public int Count => this.SelectedActionVMList.Count((vm) => { return vm.Income != null || vm.Expenses != null; });
        #endregion
        /// <summary>
        /// 選択されたデータの合計値
        /// </summary>
        #region SumValue
        public int SumValue => this.IncomeSumValue + this.ExpensesSumValue;
        #endregion
        /// <summary>
        /// 選択されたデータの収入合計値
        /// </summary>
        #region IncomeSumValue
        public int IncomeSumValue => this.SelectedActionVMList.Sum((vm) => vm.Income ?? 0);
        #endregion
        /// <summary>
        /// 選択されたデータの支出合計値
        /// </summary>
        #region ExpensesSumValue
        public int ExpensesSumValue => this.SelectedActionVMList.Sum((vm) => -vm.Expenses ?? 0);
        #endregion

        /// <summary>
        /// 概要VMリスト
        /// </summary>
        #region SummaryVMList
        public ObservableCollection<SummaryViewModel> SummaryVMList
        {
            get => this._SummaryVMList;
            set {
                this.SetProperty(ref this._SummaryVMList, value);
                this.UpdateDisplayedActionVMList();
            }
        }
        private ObservableCollection<SummaryViewModel> _SummaryVMList = default;
        #endregion
        /// <summary>
        /// 選択された概要VM
        /// </summary>
        #region SelectedSummaryVM
        public SummaryViewModel SelectedSummaryVM
        {
            get => this._SelectedSummaryVM;
            set {
                if (this.SetProperty(ref this._SelectedSummaryVM, value)) {
                    this.SelectedBalanceKind = value?.BalanceKind;
                    this.SelectedCategoryId = value?.CategoryId;
                    this.SelectedItemId = value?.ItemId;

                    this.UpdateDisplayedActionVMList();
                }
            }
        }
        private SummaryViewModel _SelectedSummaryVM = default;
        #endregion

        /// <summary>
        /// 帳簿項目を概要によって絞り込むか
        /// </summary>
        #region UseFilter
        public bool UseFilter
        {
            get => this._UseFilter;
            set {
                if (this.SetProperty(ref this._UseFilter, value)) {
                    this.UpdateDisplayedActionVMList();
                }
            }
        }
        private bool _UseFilter = false;
        #endregion

        /// <summary>
        /// 選択された検索種別
        /// </summary>
        #region SelectedFindKind
        public FindKind SelectedFindKind {
            get => this._SelectedFindKind;
            set {
                if (this.SetProperty(ref this._SelectedFindKind, value)) {
                    this.RaisePropertyChanged(nameof(this.ShowFindBox));
                    this.RaisePropertyChanged(nameof(this.ShowReplaceBox));
                }
            }
        }
        private FindKind _SelectedFindKind = FindKind.None;
        #endregion
        /// <summary>
        /// 検索欄を表示するか
        /// </summary>
        #region ShowFindBox
        public bool ShowFindBox => this.SelectedFindKind != FindKind.None;
        #endregion
        /// <summary>
        /// 検索入力テキスト
        /// </summary>
        #region FindInputText
        public string FindInputText
        {
            get => this._FindInputText;
            set => this.SetProperty(ref this._FindInputText, value);
        }
        private string _FindInputText = string.Empty;
        #endregion
        /// <summary>
        /// 検索テキスト(設定時に絞り込み)
        /// </summary>
        #region FindText
        public string FindText {
            get => this._FindText;
            set {
                if (this.SetProperty(ref this._FindText, value)) {
                    this.UpdateDisplayedActionVMList();
                }
            }
        }
        private string _FindText = string.Empty;
        #endregion
        /// <summary>
        /// 置換欄を表示するか
        /// </summary>
        #region ShowReplaceBox
        public bool ShowReplaceBox => this.SelectedFindKind == FindKind.Replace;
        #endregion
        /// <summary>
        /// 置換テキスト
        /// </summary>
        #region ReplaceText
        public string ReplaceText
        {
            get => this._ReplaceText;
            set => this.SetProperty(ref this._ReplaceText, value);
        }
        private string _ReplaceText = string.Empty;
        #endregion
        #endregion

        #region 日別グラフタブ
        /// <summary>
        /// 日別グラフ系列VMリスト
        /// </summary>
        #region DailyGraphSeriesVMList
        public ObservableCollection<SeriesViewModel> DailyGraphSeriesVMList
        {
            get => this._DailyGraphSeriesVMList;
            set => this.SetProperty(ref this._DailyGraphSeriesVMList, value);
        }
        private ObservableCollection<SeriesViewModel> _DailyGraphSeriesVMList = default;
        #endregion

        /// <summary>
        /// 選択された日別グラフ系列VM
        /// </summary>
        #region SelectedDailyGraphSeriesVM
        public SeriesViewModel SelectedDailyGraphSeriesVM
        {
            get => this._SelectedDailyGraphSeriesVM;
            set {
                if(this.SetProperty(ref this._SelectedDailyGraphSeriesVM, value)) {
                    this.SelectedBalanceKind = value.BalanceKind;
                    this.SelectedCategoryId = value.CategoryId;
                    this.SelectedItemId = value.ItemId;

                    this.SelectedSeriesChanged?.Invoke();
                }
            }
        }
        private SeriesViewModel _SelectedDailyGraphSeriesVM = default;
        #endregion

        /// <summary>
        /// 日別グラフプロットモデル
        /// </summary>
        #region DailyGraphPlotModel
        public PlotModel DailyGraphPlotModel
        {
            get => this._DailyGraphPlotModel;
            set => this.SetProperty(ref this._DailyGraphPlotModel, value);
        }
        private PlotModel _DailyGraphPlotModel = new PlotModel() {
            Title = Properties.Resources.GraphTitle_DailyGraph,
            IsLegendVisible = false
        };
        #endregion

        /// <summary>
        /// 選択項目日別グラフプロットモデル
        /// </summary>
        #region SelectedDailyGraphPlotModel
        public PlotModel SelectedDailyGraphPlotModel
        {
            get => this._SelectedDailyGraphPlotModel;
            set => this.SetProperty(ref this._SelectedDailyGraphPlotModel, value);
        }
        private PlotModel _SelectedDailyGraphPlotModel = new PlotModel() {
            Title = Properties.Resources.GraphTitle_SeparetelyGraph,
            IsLegendVisible = false
        };
        #endregion
        #endregion

        #region 月別一覧タブ
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
                        int startMonth = Properties.Settings.Default.App_StartMonth;
                        int yearDiff = value.GetFirstDateOfFiscalYear(startMonth).Year - oldDisplayedYear.GetFirstDateOfFiscalYear(startMonth).Year;

                        if (this.DisplayedMonth != null) {
                            // 表示年の差分を表示月に反映する
                            this.DisplayedMonth = this.DisplayedMonth.Value.AddYears(yearDiff);
                        }
                        this.onUpdateDisplayedDate = false;
                    }
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
            get => this._DisplayedMonths;
            set => this.SetProperty(ref this._DisplayedMonths, value);
        }
        private ObservableCollection<DateTime> _DisplayedMonths = default;
        #endregion

        /// <summary>
        /// 月別系列VMリスト
        /// </summary>
        #region MonthlySeriesVMList
        public ObservableCollection<SeriesViewModel> MonthlySeriesVMList
        {
            get => this._MonthlySeriesVMList;
            set => this.SetProperty(ref this._MonthlySeriesVMList, value);
        }
        private ObservableCollection<SeriesViewModel> _MonthlySeriesVMList = default;
        #endregion

        /// <summary>
        /// 選択された月別系列VM
        /// </summary>
        #region SelectedMonthlySeriesVM
        public SeriesViewModel SelectedMonthlySeriesVM
        {
            get => this._SelectedMonthlySeriesVM;
            set {
                if(this.SetProperty(ref this._SelectedMonthlySeriesVM, value)) {
                    this.SelectedBalanceKind = value.BalanceKind;
                    this.SelectedCategoryId = value.CategoryId;
                    this.SelectedItemId = value.ItemId;

                    this.SelectedSeriesChanged?.Invoke();
                }
            }
        }
        private SeriesViewModel _SelectedMonthlySeriesVM = default;
        #endregion
        #endregion

        #region 月別グラフタブ
        /// <summary>
        /// 月別グラフ系列VMリスト
        /// </summary>
        #region MonthlyGraphSeriesVMList
        public ObservableCollection<SeriesViewModel> MonthlyGraphSeriesVMList
        {
            get => this._MonthlyGraphSeriesVMList;
            set => this.SetProperty(ref this._MonthlyGraphSeriesVMList, value);
        }
        private ObservableCollection<SeriesViewModel> _MonthlyGraphSeriesVMList = default;
        #endregion

        /// <summary>
        /// 選択された月別グラフ系列VM
        /// </summary>
        #region SelectedMonthlyGraphSeriesVM
        public SeriesViewModel SelectedMonthlyGraphSeriesVM
        {
            get => this._SelectedMonthlyGraphSeriesVM;
            set {
                if (this.SetProperty(ref this._SelectedMonthlyGraphSeriesVM, value)) {
                    this.SelectedBalanceKind = value.BalanceKind;
                    this.SelectedCategoryId = value.CategoryId;
                    this.SelectedItemId = value.ItemId;

                    this.SelectedSeriesChanged?.Invoke();
                }
            }
        }
        private SeriesViewModel _SelectedMonthlyGraphSeriesVM = default;
        #endregion

        /// <summary>
        /// 月別グラフプロットモデル
        /// </summary>
        #region MonthlyGraphPlotModel
        public PlotModel MonthlyGraphPlotModel
        {
            get => this._MonthlyGraphPlotModel;
            set => this.SetProperty(ref this._MonthlyGraphPlotModel, value);
        }
        private PlotModel _MonthlyGraphPlotModel = new PlotModel() {
            Title = Properties.Resources.GraphTitle_MonthlyGraph,
            IsLegendVisible = false
        };
        #endregion

        /// <summary>
        /// 選択項目月別グラフプロットモデル
        /// </summary>
        #region SelectedMonthlyGraphPlotModel
        public PlotModel SelectedMonthlyGraphPlotModel
        {
            get => this._SelectedMonthlyGraphPlotModel;
            set => this.SetProperty(ref this._SelectedMonthlyGraphPlotModel, value);
        }
        private PlotModel _SelectedMonthlyGraphPlotModel = new PlotModel() {
            Title = Properties.Resources.GraphTitle_SeparetelyGraph,
            IsLegendVisible = false
        };
        #endregion
        #endregion

        #region 年別一覧タブ
        /// <summary>
        /// 表示年リスト(年別一覧の年)
        /// </summary>
        #region DisplayedYears
        public ObservableCollection<DateTime> DisplayedYears
        {
            get => this._DisplayedYears;
            set => this.SetProperty(ref this._DisplayedYears, value);
        }
        private ObservableCollection<DateTime> _DisplayedYears = default;
        #endregion

        /// <summary>
        /// 年別系列VMリスト
        /// </summary>
        #region YearlySeriesVMList
        public ObservableCollection<SeriesViewModel> YearlySeriesVMList
        {
            get => this._YearlySeriesVMList;
            set => this.SetProperty(ref this._YearlySeriesVMList, value);
        }
        private ObservableCollection<SeriesViewModel> _YearlySeriesVMList = default;
        #endregion

        /// <summary>
        /// 選択された年別系列VM
        /// </summary>
        #region SelectedYearlySummaryVM
        public SeriesViewModel SelectedYearlySeriesVM
        {
            get => this._SelectedYearlySeriesVM;
            set {
                if (this.SetProperty(ref this._SelectedYearlySeriesVM, value)) {
                    this.SelectedBalanceKind = value.BalanceKind;
                    this.SelectedCategoryId = value.CategoryId;
                    this.SelectedItemId = value.ItemId;

                    this.SelectedSeriesChanged?.Invoke();
                }
            }
        }
        private SeriesViewModel _SelectedYearlySeriesVM = default;
        #endregion
        #endregion

        #region 年別グラフタブ
        /// <summary>
        /// 年別グラフ系列VMリスト
        /// </summary>
        #region YearlyGraphSeriesVMList
        public ObservableCollection<SeriesViewModel> YearlyGraphSeriesVMList
        {
            get => this._YearlyGraphSeriesVMList;
            set => this.SetProperty(ref this._YearlyGraphSeriesVMList, value);
        }
        private ObservableCollection<SeriesViewModel> _YearlyGraphSeriesVMList = default;
        #endregion

        /// <summary>
        /// 選択された年別グラフ系列VM
        /// </summary>
        #region SelectedYearlyGraphSummaryVM
        public SeriesViewModel SelectedYearlyGraphSeriesVM
        {
            get => this._SelectedYearlyGraphSeriesVM;
            set {
                if (this.SetProperty(ref this._SelectedYearlyGraphSeriesVM, value)) {
                    this.SelectedBalanceKind = value.BalanceKind;
                    this.SelectedCategoryId = value.CategoryId;
                    this.SelectedItemId = value.ItemId;

                    this.SelectedSeriesChanged?.Invoke();
                }
            }
        }
        private SeriesViewModel _SelectedYearlyGraphSeriesVM = default;
        #endregion

        /// <summary>
        /// 年別グラフプロットモデル
        /// </summary>
        #region YearlyGraphPlotModel
        public PlotModel YearlyGraphPlotModel
        {
            get => this._YearlyGraphPlotModel;
            set => this.SetProperty(ref this._YearlyGraphPlotModel, value);
        }
        private PlotModel _YearlyGraphPlotModel = new PlotModel() {
            Title = Properties.Resources.GraphTitle_YearlyGraph,
            IsLegendVisible = false
        };
        #endregion

        /// <summary>
        /// 選択項目年別グラフプロットモデル
        /// </summary>
        #region SelectedYearlyGraphPlotModel
        public PlotModel SelectedYearlyGraphPlotModel
        {
            get => this._SelectedYearlyGraphPlotModel;
            set => this.SetProperty(ref this._SelectedYearlyGraphPlotModel, value);
        }
        private PlotModel _SelectedYearlyGraphPlotModel = new PlotModel() {
            Title = Properties.Resources.GraphTitle_SeparetelyGraph,
            IsLegendVisible = false
        };
        #endregion
        #endregion

        #region グラフコントローラ
        /// <summary>
        /// コントローラ
        /// </summary>
        #region Controller
        public PlotController Controller
        {
            get => this._Controller;
            set => this.SetProperty(ref this._Controller, value);
        }
        private PlotController _Controller = new PlotController();
        #endregion
        #endregion
        #endregion

        /// <summary>
        /// <see cref="MainWindowViewModel"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        public MainWindowViewModel()
        {
            this.Controller.BindMouseEnter(PlotCommands.HoverPointsOnlyTrack);

            this.SelectedActionVMList.CollectionChanged += (sender, e) => {
                this.RaiseSelectedActionVMListChanged();
            };
        }

        /// <summary>
        /// 表示年変更を通知する
        /// </summary>
        public void RaiseDisplayedYearChanged()
        {
            this.RaisePropertyChanged(nameof(this.DisplayedYear));
        }

        /// <summary>
        /// 選択帳簿項目変更を通知する
        /// </summary>
        public void RaiseSelectedActionVMListChanged()
        {
            this.RaisePropertyChanged(nameof(this.AverageValue));
            this.RaisePropertyChanged(nameof(this.Count));
            this.RaisePropertyChanged(nameof(this.SumValue));
            this.RaisePropertyChanged(nameof(this.IncomeSumValue));
            this.RaisePropertyChanged(nameof(this.ExpensesSumValue));

            this.RaisePropertyChanged(nameof(this.IsMatch));
        }

        /// <summary>
        /// <see cref="DisplayedActionVMList"/> を更新する
        /// </summary>
        /// <remarks>表示されている項目のみ選択する</remarks>
        private void UpdateDisplayedActionVMList()
        {
            ObservableCollection<ActionViewModel> tmp = this._ActionVMList;

            if (this._UseFilter) {   // フィルタ有効の場合
                // 概要が選択されている場合
                if (this._SelectedSummaryVM != null) {
                    // 収支が選択されている場合
                    if ((BalanceKind)this._SelectedSummaryVM.BalanceKind != BalanceKind.Others) {
                        if (this._SelectedSummaryVM.ItemId != -1) {             // 項目名が選択されている
                            tmp = new ObservableCollection<ActionViewModel>(tmp.Where((vm) => {
                                return vm.ItemId == this._SelectedSummaryVM.ItemId || vm.ActionId == -1;
                            }));
                        }
                        else if (this._SelectedSummaryVM.CategoryId != -1) {    // 分類名が選択されている
                            tmp = new ObservableCollection<ActionViewModel>(tmp.Where((vm) => {
                                return vm.CategoryId == this._SelectedSummaryVM.CategoryId || vm.ActionId == -1;
                            }));
                        }
                        else {                                                  // 収支種別が選択されている
                            tmp = new ObservableCollection<ActionViewModel>(tmp.Where((vm) => {
                                return vm.BalanceKind == (BalanceKind)this._SelectedSummaryVM.BalanceKind || vm.ActionId == -1;
                            }));
                        }
                    }
                }
            }

            // 検索テキストで絞り込む
            if (this.FindText != string.Empty) {
                tmp = new ObservableCollection<ActionViewModel>(tmp.Where((vm) => {
                    return vm.ShopName.Contains(this.FindText) || vm.Remark.Contains(this.FindText);
                }));
            }

            this.DisplayedActionVMList = tmp;

            // 選択項目を表示項目に限定する
            this.SelectedActionVMList = new ObservableCollection<ActionViewModel>(this._SelectedActionVMList.Where((vm) => {
                return this.DisplayedActionVMList.Contains(vm);
            }));
        }
    }
}

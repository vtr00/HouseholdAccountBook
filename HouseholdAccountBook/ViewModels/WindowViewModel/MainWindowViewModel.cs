using HouseholdAccountBook.Extentions;
using OxyPlot;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// メインウィンドウVM
    /// </summary>
    public class MainWindowViewModel : BindableBase
    {
        /// <summary>
        /// 表示日付の更新中か
        /// </summary>
        private bool onUpdateDisplayedDate = false;

        #region プロパティ(共通)
        /// <summary>
        /// 選択されたタブインデックス
        /// </summary>
        #region SelectedTabIndex
        public int SelectedTabIndex
        {
            get { return this._SelectedTabIndex; }
            set {
                if (SetProperty(ref this._SelectedTabIndex, value)) {
                    this.SelectedTab = (Tabs)value;
                }
            }
        }
        private int _SelectedTabIndex = default(int);
        #endregion
        /// <summary>
        /// 選択されたタブ種別
        /// </summary>
        #region SelectedTab
        public Tabs SelectedTab
        {
            get { return this._SelectedTab; }
            set {
                if (SetProperty(ref this._SelectedTab, value)) {
                    this.SelectedTabIndex = (int)value;
                }
            }
        }
        private Tabs _SelectedTab = default(Tabs);
        #endregion

        /// <summary>
        /// 帳簿VMリスト
        /// </summary>
        #region BookVMList
        public ObservableCollection<BookViewModel> BookVMList
        {
            get { return this._BookVMList; }
            set { SetProperty(ref this._BookVMList, value); }
        }
        private ObservableCollection<BookViewModel> _BookVMList = default(ObservableCollection<BookViewModel>);
        #endregion
        /// <summary>
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookViewModel SelectedBookVM
        {
            get { return this._SelectedBookVM; }
            set { SetProperty(ref this._SelectedBookVM, value); }
        }
        private BookViewModel _SelectedBookVM;
        #endregion
        #endregion

        #region 帳簿タブ
        #region プロパティ
        /// <summary>
        /// 表示区間種別
        /// </summary>
        #region DisplayedTermKind
        public TermKind DisplayedTermKind
        {
            get {
                DateTime lastDate = this.StartDate.GetFirstDateOfMonth().AddMonths(1).AddMilliseconds(-1);
                if(this.StartDate.Day == 1 && DateTime.Equals(this.EndDate.Date, lastDate.Date)) {
                    return TermKind.Monthly;
                }
                else {
                    return TermKind.Selected;
                }
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
                switch(this.DisplayedTermKind) {
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
                if(value != null) {
                    // 開始日/終了日を更新する
                    this.StartDate = value.Value.GetFirstDateOfMonth();
                    this.EndDate = value.Value.GetFirstDateOfMonth().AddMonths(1).AddMilliseconds(-1);

                    if (!this.onUpdateDisplayedDate) {
                        this.onUpdateDisplayedDate = true;
                        // 表示月の年度の最初の月を表示年とする
                        this.DisplayedYear = value.Value.GetFirstDateOfFiscalYear(Properties.Settings.Default.App_StartMonth);
                        this.onUpdateDisplayedDate = false;
                    }
                }
                if(oldDisplayedMonth != this.DisplayedMonth) {
                    RaisePropertyChanged();
                }
            }
        }
        private static string nameDisplayedMonth = PropertyName<MainWindowViewModel>.Get(x => x.DisplayedMonth);
        #endregion

        /// <summary>
        /// 表示開始日
        /// </summary>
        #region StartDate
        public DateTime StartDate
        {
            get { return this._StartDate; }
            set {
                SetProperty(ref this._StartDate, value);
                RaisePropertyChanged(nameDisplayedMonth);
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
            get { return this._EndDate; }
            set {
                SetProperty(ref this._EndDate, value);
                RaisePropertyChanged(nameDisplayedMonth);
            }
        }
        private DateTime _EndDate = DateTime.Now.GetFirstDateOfMonth().AddMonths(1).AddMilliseconds(-1);
        #endregion
        
        /// <summary>
        /// 帳簿項目VMリスト
        /// </summary>
        #region ActionVMList
        public ObservableCollection<ActionViewModel> ActionVMList
        {
            get { return this._ActionVMList; }
            set {
                SetProperty(ref this._ActionVMList, value);
                UpdateDisplayedActionVMList();
            }
        }
        private ObservableCollection<ActionViewModel> _ActionVMList = default(ObservableCollection<ActionViewModel>);
        #endregion
        /// <summary>
        /// 表示対象の帳簿項目VMリスト
        /// </summary>
        #region DisplayedActionVMList
        public ObservableCollection<ActionViewModel> DisplayedActionVMList
        {
            get { return this._DisplayedActionVMList; }
            set { SetProperty(ref this._DisplayedActionVMList, value); }
        }
        private ObservableCollection<ActionViewModel> _DisplayedActionVMList = default(ObservableCollection<ActionViewModel>);
        #endregion

        /// <summary>
        /// 選択された帳簿項目VM(先頭)
        /// </summary>
        #region SelectedActionVM
        public ActionViewModel SelectedActionVM
        {
            get { return this._SelectedActionVM; }
            set { SetProperty(ref this._SelectedActionVM, value); }
        }
        private ActionViewModel _SelectedActionVM = default(ActionViewModel);
        #endregion
        /// <summary>
        /// 選択された帳簿項目VMリスト
        /// </summary>
        #region SelectedActionVMList
        public ObservableCollection<ActionViewModel> SelectedActionVMList { get; } = new ObservableCollection<ActionViewModel>();
        #endregion
        
        /// <summary>
        /// 最後に操作した帳簿項目の日付
        /// </summary>
        #region ActDateLastEdited
        public DateTime? ActDateLastEdited
        {
            get { return this._ActDateLastEdited; }
            set { SetProperty(ref this._ActDateLastEdited, value); }
        }
        private DateTime? _ActDateLastEdited = null;
        #endregion

        /// <summary>
        /// 平均値
        /// </summary>
        #region AverageValue
        public double? AverageValue
        {
            get { return this._AverageValue; }
            private set { SetProperty(ref this._AverageValue, value); }
        }
        private double? _AverageValue = default(double?);
        #endregion
        /// <summary>
        /// データの個数
        /// </summary>
        #region Amount
        public int Amount
        {
            get { return this._Amount; }
            set { SetProperty(ref this._Amount, value); }
        }
        private int _Amount = default(int);
        #endregion
        /// <summary>
        /// 合計値
        /// </summary>
        #region SumValue
        public int? SumValue
        {
            get { return this._SumValue; }
            private set { SetProperty(ref this._SumValue, value); }
        }
        private int? _SumValue = default(int?);
        #endregion

        /// <summary>
        /// 合計項目VMリスト
        /// </summary>
        #region SummaryVMList
        public ObservableCollection<SummaryViewModel> SummaryVMList
        {
            get { return this._SummaryVMList; }
            set {
                SetProperty(ref this._SummaryVMList, value);
                UpdateDisplayedActionVMList();
            }
        }
        private ObservableCollection<SummaryViewModel> _SummaryVMList;
        #endregion
        /// <summary>
        /// 選択された合計項目VM
        /// </summary>
        #region SelectedSummaryVM
        public SummaryViewModel SelectedSummaryVM
        {
            get { return this._SelectedSummaryVM; }
            set {
                SetProperty(ref this._SelectedSummaryVM, value);
                UpdateDisplayedActionVMList();
            }
        }
        private SummaryViewModel _SelectedSummaryVM = default(SummaryViewModel);
        #endregion
        #endregion

        /// <summary>
        /// <see cref="DisplayedActionVMList"/> を更新する
        /// </summary>
        private void UpdateDisplayedActionVMList()
        {
            if(this._SelectedSummaryVM == null || (BalanceKind)this._SelectedSummaryVM.BalanceKind == BalanceKind.Others) {
                this.DisplayedActionVMList = this._ActionVMList;
            }
            else {
                if(this._SelectedSummaryVM.ItemId != -1) {
                    this.DisplayedActionVMList = new ObservableCollection<ActionViewModel>(this._ActionVMList.Where((vm) => {
                        return vm.ItemId == this._SelectedSummaryVM.ItemId;
                    }));
                }
                else if (this._SelectedSummaryVM.CategoryId != -1) {
                    this.DisplayedActionVMList = new ObservableCollection<ActionViewModel>(this._ActionVMList.Where((vm) => {
                        return vm.CategoryId == this._SelectedSummaryVM.CategoryId;
                    }));
                }
                else if((BalanceKind)this._SelectedSummaryVM.BalanceKind != BalanceKind.Others) {
                    this.DisplayedActionVMList = new ObservableCollection<ActionViewModel>(this._ActionVMList.Where((vm) => {
                        return vm.BalanceKind == (BalanceKind)this._SelectedSummaryVM.BalanceKind;
                    }));
                }
                Debug.Assert(true);
            }
        }
        /// <summary>
        /// 統計値を更新する
        /// </summary>
        private void UpdateStatisticsValue()
        {
            int? sum = this.SelectedActionVMList.Count != 0 ? (int?)0 : null;
            foreach (ActionViewModel vm in this.SelectedActionVMList) {
                sum += vm.Income ?? 0 - vm.Outgo ?? 0;
            }
            this.SumValue = sum;
            this.Amount = this.SelectedActionVMList.Count((vm) => { return vm.Income != null || vm.Outgo != null; });
            this.AverageValue = this.SelectedActionVMList.Count != 0 ? (double?)sum / this.SelectedActionVMList.Count : null;
        }
        #endregion

        #region 月別一覧タブ
        #region プロパティ
        /// <summary>
        /// 表示年
        /// </summary>
        #region DisplayedYear
        public DateTime DisplayedYear
        {
            get { return this._DisplayedYear; }
            set {
                DateTime oldDisplayedYear = this._DisplayedYear;
                if (SetProperty(ref this._DisplayedYear, value)) {
                    if (!this.onUpdateDisplayedDate) {
                        int startMonth = Properties.Settings.Default.App_StartMonth;
                        int yearDiff = value.GetFirstDateOfFiscalYear(startMonth).Year - oldDisplayedYear.GetFirstDateOfFiscalYear(startMonth).Year;
                        this.onUpdateDisplayedDate = true;
                        if (this.DisplayedMonth != null) {
                            // 表示年の差分を表示月に反映する
                            this.DisplayedMonth = this.DisplayedMonth.Value.AddYears(yearDiff);
                            // 同年度中の未来の月の場合には、表示月を今日にする
                            if (this.DisplayedMonth > DateTime.Now &&
                               this.DisplayedMonth.Value.GetFirstDateOfFiscalYear(startMonth).Year == DateTime.Now.GetFirstDateOfFiscalYear(startMonth).Year) {
                                this.DisplayedMonth = DateTime.Now;
                            }
                        }
                        else {
                            this.DisplayedMonth = value.GetFirstDateOfFiscalYear(startMonth);
                            // 同年度中の月の場合には、表示月を今日にする
                            if (this.DisplayedMonth.Value.GetFirstDateOfFiscalYear(startMonth).Year == DateTime.Now.GetFirstDateOfFiscalYear(startMonth).Year) {
                                this.DisplayedMonth = DateTime.Now;
                            }
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
        public ObservableCollection<string> DisplayedMonths
        {
            get { return this._DisplayedMonths; }
            set { SetProperty(ref this._DisplayedMonths, value); }
        }
        private ObservableCollection<string> _DisplayedMonths = default(ObservableCollection<string>);
        #endregion

        /// <summary>
        /// 月別合計項目VMリスト
        /// </summary>
        #region MonthlySummaryVMList
        public ObservableCollection<SeriesViewModel> MonthlySummaryVMList
        {
            get { return this._MonthlySummaryVMList; }
            set { SetProperty(ref this._MonthlySummaryVMList, value); }
        }
        private ObservableCollection<SeriesViewModel> _MonthlySummaryVMList = default(ObservableCollection<SeriesViewModel>);
        #endregion
        #endregion
        #endregion

        #region 年別一覧タブ
        #region プロパティ
        /// <summary>
        /// 表示年リスト
        /// </summary>
        #region DisplayedYears
        public ObservableCollection<string> DisplayedYears
        {
            get { return this._DisplayedYears; }
            set { SetProperty(ref this._DisplayedYears, value); }
        }
        private ObservableCollection<string> _DisplayedYears = default(ObservableCollection<string>);
        #endregion

        /// <summary>
        /// 年別合計項目VMリスト
        /// </summary>
        #region YearlySummaryVMList
        public ObservableCollection<SeriesViewModel> YearlySummaryVMList
        {
            get { return this._YearlySummaryVMList; }
            set { SetProperty(ref this._YearlySummaryVMList, value); }
        }
        private ObservableCollection<SeriesViewModel> _YearlySummaryVMList = default(ObservableCollection<SeriesViewModel>);
        #endregion
        #endregion
        #endregion

        #region グラフタブ
        #region プロパティ
        /// <summary>
        /// グラフ種別辞書
        /// </summary>
        #region GraphKindDic
        public Dictionary<GraphKind, string> GraphKindDic { get; } = GraphKindStr;
        #endregion

        /// <summary>
        /// 選択されたグラフ種別
        /// </summary>
        #region SelectedGraphKind
        public GraphKind SelectedGraphKind
        {
            get { return this._SelectedGraphKind; }
            set { SetProperty(ref this._SelectedGraphKind, value); }
        }
        private GraphKind _SelectedGraphKind = default(GraphKind);
        #endregion
        
        /// <summary>
        /// 全項目日別グラフプロットモデル
        /// </summary>
        #region WholeItemDailyGraphModel
        public PlotModel WholeItemDailyGraphModel
        {
            get { return this._WholeItemDailyGraphModel; }
            set { SetProperty(ref this._WholeItemDailyGraphModel, value); }
        }
        private PlotModel _WholeItemDailyGraphModel = new PlotModel() {
            Title = "日別グラフ",
            LegendOrientation = LegendOrientation.Horizontal,
            LegendPlacement = LegendPlacement.Outside,
            LegendPosition = LegendPosition.RightTop,
            LegendTitle = "凡例",
            LegendFontSize = 10.5
        };
        #endregion

        /// <summary>
        /// 選択項目日別グラフプロットモデル
        /// </summary>
        #region SelectedItemDailyGraphModel
        public PlotModel SelectedItemDailyGraphModel
        {
            get { return this._SelectedItemDailyGraphModel; }
            set { SetProperty(ref this._SelectedItemDailyGraphModel, value); }
        }
        private PlotModel _SelectedItemDailyGraphModel = new PlotModel() {
            Title = "個別グラフ",
            LegendOrientation = LegendOrientation.Horizontal,
            LegendPlacement = LegendPlacement.Outside,
            LegendPosition = LegendPosition.RightTop,
            LegendTitle = "凡例",
            LegendFontSize = 10.5
        };
        #endregion

        /// <summary>
        /// 全項目月別グラフプロットモデル
        /// </summary>
        #region WholeItemMonthlyGraphModel
        public PlotModel WholeItemMonthlyGraphModel
        {
            get { return this._WholeItemMonthlyGraphModel; }
            set { SetProperty(ref this._WholeItemMonthlyGraphModel, value); }
        }
        private PlotModel _WholeItemMonthlyGraphModel = new PlotModel() {
            Title = "月別グラフ",
            LegendOrientation = LegendOrientation.Horizontal,
            LegendPlacement = LegendPlacement.Outside,
            LegendPosition = LegendPosition.RightTop,
            LegendTitle = "凡例",
            LegendFontSize = 10.5
        };
        #endregion

        /// <summary>
        /// 選択項目月別グラフプロットモデル
        /// </summary>
        #region SelectedItemMonthlyGraphModel
        public PlotModel SelectedItemMonthlyGraphModel
        {
            get { return this._SelectedItemMonthlyGraphModel; }
            set { SetProperty(ref this._SelectedItemMonthlyGraphModel, value); }
        }
        private PlotModel _SelectedItemMonthlyGraphModel = new PlotModel() {
            Title = "個別グラフ",
            LegendOrientation = LegendOrientation.Horizontal,
            LegendPlacement = LegendPlacement.Outside,
            LegendPosition = LegendPosition.RightTop,
            LegendTitle = "凡例",
            LegendFontSize = 10.5
        };
        #endregion

        /// <summary>
        /// 全項目年別グラフプロットモデル
        /// </summary>
        #region WholeItemYearlyGraphModel
        public PlotModel WholeItemYearlyGraphModel
        {
            get { return this._WholeItemYearlyGraphModel; }
            set { SetProperty(ref this._WholeItemYearlyGraphModel, value); }
        }
        private PlotModel _WholeItemYearlyGraphModel = new PlotModel() {
            Title = "年別グラフ",
            LegendOrientation = LegendOrientation.Horizontal,
            LegendPlacement = LegendPlacement.Outside,
            LegendPosition = LegendPosition.RightTop,
            LegendTitle = "凡例",
            LegendFontSize = 10.5
        };
        #endregion

        /// <summary>
        /// 選択項目年別グラフプロットモデル
        /// </summary>
        #region SelectedItemYearlyGraphModel
        public PlotModel SelectedItemYearlyGraphModel
        {
            get { return this._SelectedItemYearlyGraphModel; }
            set { SetProperty(ref this._SelectedItemYearlyGraphModel, value); }
        }
        private PlotModel _SelectedItemYearlyGraphModel = new PlotModel() {
            Title = "個別グラフ",
            LegendOrientation = LegendOrientation.Horizontal,
            LegendPlacement = LegendPlacement.Outside,
            LegendPosition = LegendPosition.RightTop,
            LegendTitle = "凡例",
            LegendFontSize = 10.5
        };
        #endregion
        
        /// <summary>
        /// コントローラ
        /// </summary>
        #region Controller
        public PlotController Controller
        {
            get { return this._Controller; }
            set { SetProperty(ref this._Controller, value); }
        }
        private PlotController _Controller = new PlotController();
        #endregion
        #endregion
        #endregion

        /// <summary>
        /// デバッグビルドか
        /// </summary>
        public bool IsDebug
        {
            get {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// <see cref="MainWindowViewModel"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        public MainWindowViewModel()
        {
            this.SelectedActionVMList.CollectionChanged += (sender, e) => UpdateStatisticsValue();
            this.Controller.BindMouseEnter(PlotCommands.HoverPointsOnlyTrack);
        }
    }
}

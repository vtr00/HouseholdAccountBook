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
                    this.SelectedTab = (Tabs)value;
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
                if (this.SetProperty(ref this._SelectedBookVM, value) && value != null) { // SelectedBookVMがnullになることはない想定
                    this.SelectedBookChanged?.Invoke();
                }
            }
        }
        private BookViewModel _SelectedBookVM;
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
                DateTime lastDate = this.StartDate.GetFirstDateOfMonth().AddMonths(1).AddMilliseconds(-1);
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
                    this.EndDate = value.Value.GetFirstDateOfMonth().AddMonths(1).AddMilliseconds(-1);

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
        private DateTime _EndDate = DateTime.Now.GetFirstDateOfMonth().AddMonths(1).AddMilliseconds(-1);
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
                if (this.SetProperty(ref this._SelectedActionVMList, value)) {
                    this.SelectedActionVMList.CollectionChanged += (sender, e) => {
                        this.RaisePropertyChanged(nameof(this.SumValue));
                        this.RaisePropertyChanged(nameof(this.Amount));
                        this.RaisePropertyChanged(nameof(this.AverageValue));

                        this.RaisePropertyChanged(nameof(this.IsMatch));
                    };
                }
            }
        }
        private ObservableCollection<ActionViewModel> _SelectedActionVMList = default;
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
        /// 合計値
        /// </summary>
        #region SumValue
        public int? SumValue
        {
            get {
                int? sum = this.SelectedActionVMList.Count != 0 ? (int?)0 : null;
                foreach (ActionViewModel vm in this.SelectedActionVMList) {
                    sum += vm.Income ?? 0 - vm.Outgo ?? 0;
                }
                return sum;
            }
        }
        #endregion
        /// <summary>
        /// データの個数
        /// </summary>
        #region Amount
        public int Amount => this.SelectedActionVMList.Count((vm) => { return vm.Income != null || vm.Outgo != null; });
        #endregion
        /// <summary>
        /// 平均値
        /// </summary>
        #region AverageValue
        public double? AverageValue => this.SelectedActionVMList.Count != 0 ? (double?)this.SumValue / this.SelectedActionVMList.Count : null;
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
        private ObservableCollection<SummaryViewModel> _SummaryVMList;
        #endregion
        /// <summary>
        /// 選択された概要VM
        /// </summary>
        #region SelectedSummaryVM
        public SummaryViewModel SelectedSummaryVM
        {
            get => this._SelectedSummaryVM;
            set {
                this.SetProperty(ref this._SelectedSummaryVM, value);
                this.UpdateDisplayedActionVMList();
            }
        }
        private SummaryViewModel _SelectedSummaryVM = default;
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
            get => this._DisplayedMonths;
            set => this.SetProperty(ref this._DisplayedMonths, value);
        }
        private ObservableCollection<string> _DisplayedMonths = default;
        #endregion

        /// <summary>
        /// 月別概要VMリスト
        /// </summary>
        #region MonthlySummaryVMList
        public ObservableCollection<SeriesViewModel> MonthlySummaryVMList
        {
            get => this._MonthlySummaryVMList;
            set => this.SetProperty(ref this._MonthlySummaryVMList, value);
        }
        private ObservableCollection<SeriesViewModel> _MonthlySummaryVMList = default;
        #endregion
        #endregion

        #region 年別一覧タブ
        /// <summary>
        /// 表示年リスト
        /// </summary>
        #region DisplayedYears
        public ObservableCollection<string> DisplayedYears
        {
            get => this._DisplayedYears;
            set => this.SetProperty(ref this._DisplayedYears, value);
        }
        private ObservableCollection<string> _DisplayedYears = default;
        #endregion

        /// <summary>
        /// 年別概要VMリスト
        /// </summary>
        #region YearlySummaryVMList
        public ObservableCollection<SeriesViewModel> YearlySummaryVMList
        {
            get => this._YearlySummaryVMList;
            set => this.SetProperty(ref this._YearlySummaryVMList, value);
        }
        private ObservableCollection<SeriesViewModel> _YearlySummaryVMList = default;
        #endregion
        #endregion

        #region グラフタブ
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
            get => this._SelectedGraphKind;
            set {
                if (this.SetProperty(ref this._SelectedGraphKind, value)) {
                    this.SelectedGraphKindChanged?.Invoke();
                }
            }
        }
        private GraphKind _SelectedGraphKind = default;
        #endregion

        /// <summary>
        /// 全項目日別グラフプロットモデル
        /// </summary>
        #region WholeItemDailyGraphModel
        public PlotModel WholeItemDailyGraphModel
        {
            get => this._WholeItemDailyGraphModel;
            set => this.SetProperty(ref this._WholeItemDailyGraphModel, value);
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
            get => this._SelectedItemDailyGraphModel;
            set => this.SetProperty(ref this._SelectedItemDailyGraphModel, value);
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
            get => this._WholeItemMonthlyGraphModel;
            set => this.SetProperty(ref this._WholeItemMonthlyGraphModel, value);
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
            get => this._SelectedItemMonthlyGraphModel;
            set => this.SetProperty(ref this._SelectedItemMonthlyGraphModel, value);
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
            get => this._WholeItemYearlyGraphModel;
            set => this.SetProperty(ref this._WholeItemYearlyGraphModel, value);
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
            get => this._SelectedItemYearlyGraphModel;
            set => this.SetProperty(ref this._SelectedItemYearlyGraphModel, value);
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
            get => this._Controller;
            set => this.SetProperty(ref this._Controller, value);
        }
        private PlotController _Controller = new PlotController();
        #endregion
        #endregion

        /// <summary>
        /// デバッグビルドか
        /// </summary>
#if DEBUG
        public bool IsDebug => true;
#else
        public bool IsDebug => false;
#endif
        #endregion

        /// <summary>
        /// <see cref="MainWindowViewModel"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        public MainWindowViewModel()
        {
            this.SelectedActionVMList = new ObservableCollection<ActionViewModel>();
            this.Controller.BindMouseEnter(PlotCommands.HoverPointsOnlyTrack);
        }

        /// <summary>
        /// <see cref="DisplayedActionVMList"/> を更新する
        /// </summary>
        /// <remarks>表示されている項目のみ選択する</remarks>
        private void UpdateDisplayedActionVMList()
        {
            if (this._SelectedSummaryVM == null || (BalanceKind)this._SelectedSummaryVM.BalanceKind == BalanceKind.Others) {
                this.DisplayedActionVMList = this._ActionVMList;
            }
            else {
                if (this._SelectedSummaryVM.ItemId != -1) {
                    this.DisplayedActionVMList = new ObservableCollection<ActionViewModel>(this._ActionVMList.Where((vm) => {
                        return vm.ItemId == this._SelectedSummaryVM.ItemId;
                    }));
                    this.SelectedActionVMList = new ObservableCollection<ActionViewModel>(this.SelectedActionVMList.Where((vm) => {
                        return vm.ItemId == this._SelectedSummaryVM.ItemId;
                    }));
                }
                else if (this._SelectedSummaryVM.CategoryId != -1) {
                    this.DisplayedActionVMList = new ObservableCollection<ActionViewModel>(this._ActionVMList.Where((vm) => {
                        return vm.CategoryId == this._SelectedSummaryVM.CategoryId;
                    }));
                    this.SelectedActionVMList = new ObservableCollection<ActionViewModel>(this._SelectedActionVMList.Where((vm) => {
                        return vm.CategoryId == this._SelectedSummaryVM.CategoryId;
                    }));
                }
                else {
                    this.DisplayedActionVMList = new ObservableCollection<ActionViewModel>(this._ActionVMList.Where((vm) => {
                        return vm.BalanceKind == (BalanceKind)this._SelectedSummaryVM.BalanceKind;
                    }));
                    this.SelectedActionVMList = new ObservableCollection<ActionViewModel>(this._SelectedActionVMList.Where((vm) => {
                        return vm.BalanceKind == (BalanceKind)this._SelectedSummaryVM.BalanceKind;
                    }));
                }
            }
        }
    }
}

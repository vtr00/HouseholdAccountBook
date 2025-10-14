using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using HouseholdAccountBook.ViewModels.Windows;
using HouseholdAccountBook.Views.UserControls;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static HouseholdAccountBook.Views.UiConstants;

namespace HouseholdAccountBook.ViewModels.WindowsParts
{
    /// <summary>
    /// メインウィンドウ グラフタブVM
    /// </summary>
    public class MainWindowGraphTabViewModel : WindowPartViewModelBase
    {
        private MainWindowViewModel Parent { get; set; }

        private Tabs Tab { get; set; }

        #region イベント
        /// <summary>
        /// 系列選択変更時イベント
        /// </summary>
        public event EventHandler SelectedSeriesChanged = default;
        #endregion

        #region プロパティ
        /// <summary>
        /// グラフ系列VMリスト
        /// </summary>
        #region GraphSeriesVMList
        public ObservableCollection<SeriesViewModel> GraphSeriesVMList
        {
            get => this._GraphSeriesVMList;
            set => this.SetProperty(ref this._GraphSeriesVMList, value);
        }
        private ObservableCollection<SeriesViewModel> _GraphSeriesVMList = default;
        #endregion

        /// <summary>
        /// 選択されたグラフ系列VM
        /// </summary>
        #region SelectedGraphSeriesVM
        public SeriesViewModel SelectedGraphSeriesVM
        {
            get => this._SelectedGraphSeriesVM;
            set {
                SeriesViewModel oldVM = this._SelectedGraphSeriesVM;
                if (this.SetProperty(ref this._SelectedGraphSeriesVM, value)) {
                    this.Parent.SelectedBalanceKind = value?.BalanceKind;
                    this.Parent.SelectedCategoryId = value?.CategoryId;
                    this.Parent.SelectedItemId = value?.ItemId;

                    this.SelectedSeriesChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        private SeriesViewModel _SelectedGraphSeriesVM = default;
        #endregion

        /// <summary>
        /// グラフプロットモデル
        /// </summary>
        #region GraphPlotModel
        public PlotModel GraphPlotModel
        {
            get => this._GraphPlotModel;
            set => this.SetProperty(ref this._GraphPlotModel, value);
        }
        private PlotModel _GraphPlotModel = new() {
            Title = string.Empty,
            IsLegendVisible = false
        };
        #endregion

        /// <summary>
        /// 選択項目グラフプロットモデル
        /// </summary>
        #region SelectedGraphPlotModel
        public PlotModel SelectedGraphPlotModel
        {
            get => this._SelectedGraphPlotModel;
            set => this.SetProperty(ref this._SelectedGraphPlotModel, value);
        }
        private PlotModel _SelectedGraphPlotModel = new() {
            Title = Properties.Resources.GraphTitle_SeparetelyGraph,
            IsLegendVisible = false
        };
        #endregion

        /// <summary>
        /// グラフコントローラ
        /// </summary>
        #region Controller
        public PlotController Controller
        {
            get => this._Controller;
            set => this.SetProperty(ref this._Controller, value);
        }
        private PlotController _Controller = new();
        #endregion
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="parent">親VM</param>
        /// <param name="tab">タブ</param>
        public MainWindowGraphTabViewModel(MainWindowViewModel parent, Tabs tab)
        {
            using FuncLog funcLog = new(new { tab });

            this.Parent = parent;
            this.Tab = tab;

            this.GraphPlotModel.Title = tab switch {
                Tabs.DailyGraphTab => Properties.Resources.GraphTitle_DailyGraph,
                Tabs.MonthlyGraphTab => Properties.Resources.GraphTitle_MonthlyGraph,
                Tabs.YearlyGraphTab => Properties.Resources.GraphTitle_YearlyGraph,
                _ => "",
            };

            this.Controller.BindMouseEnter(PlotCommands.HoverPointsOnlyTrack);
        }

        public override async Task LoadAsync()
        {
            await this.LoadAsync(null, null);
        }

        /// <summary>
        /// グラフタブに表示するデータを読み込む
        /// </summary>
        /// <param name="categoryId">分類ID</param>
        /// <param name="itemId">項目ID</param>
        /// <returns></returns>
        public async Task LoadAsync(int? categoryId = null, int? itemId = null)
        {
            if (this.Parent.SelectedTab != this.Tab) return;

            using FuncLog funcLog = new(new { categoryId, itemId });

            this.InitializeGraphTabData();
            await this.UpdateGraphTabDataAsync(categoryId, itemId);
            this.UpdateSelectedGraph();
        }

        public override void AddEventHandlers()
        {
            // NOP
        }

        /// <summary>
        /// グラフタブに表示するデータを初期化する
        /// </summary>
        private void InitializeGraphTabData()
        {
            if (this.Parent.SelectedTab != this.Tab) return;

            using FuncLog funcLog = new();

            // 横軸 - 日付軸
            string unitX = this.Tab switch {
                Tabs.DailyGraphTab => Properties.Resources.Unit_Day,
                Tabs.MonthlyGraphTab => Properties.Resources.Unit_Month,
                Tabs.YearlyGraphTab => DateTimeExtensions.GetYearUnit(this.Parent.FiscalStartMonth),
                _ => throw new InvalidOperationException(),
            };
            DateTime start = this.Parent.DisplayedStart;
            DateTime end = this.Parent.DisplayedEnd;
            List<string> GetHorizontalLabels()
            {
                List<string> labels = [];
                switch (this.Tab) {
                    case Tabs.DailyGraphTab:
                        for (DateTime tmp = start; tmp <= end; tmp = tmp.AddDays(1)) {
                            labels.Add($"{tmp.Day}");
                        }
                        break;
                    case Tabs.MonthlyGraphTab:
                        for (DateTime tmp = start; tmp <= end; tmp = tmp.AddMonths(1)) {
                            labels.Add($"{tmp.Month}");
                        }
                        break;
                    case Tabs.YearlyGraphTab:
                        for (DateTime tmp = start; tmp <= end; tmp = tmp.AddYears(1)) {
                            labels.Add($"{tmp:yyyy}");
                        }
                        break;
                    default:
                        throw new InvalidOperationException();
                }
                return labels;
            }
            CategoryAxis GetHorizontalAxis()
            {
                CategoryAxis axis = new() {
                    Unit = unitX,
                    Position = AxisPosition.Bottom,
                    Key = "Category"
                };
                axis.Labels.AddRange(GetHorizontalLabels());
                return axis;
            }

            // 縦軸 - 線形軸
            string unitY = Properties.Resources.Unit_Money;
            LinearAxis GetVerticalAxis()
            {
                return new() {
                    Unit = unitY,
                    Position = AxisPosition.Left,
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Dot,
                    StringFormat = "#,0",
                    Key = "Value",
                    Title = GraphKind1Str[this.Parent.SelectedGraphKind1]
                };
            }

            // 全項目
            this.GraphPlotModel.Axes.Clear();
            this.GraphPlotModel.Series.Clear();
            this.GraphPlotModel.Axes.Add(GetHorizontalAxis());
            this.GraphPlotModel.Axes.Add(GetVerticalAxis());
            this.GraphPlotModel.InvalidatePlot(true);

            // 選択項目
            this.SelectedGraphPlotModel.Axes.Clear();
            this.SelectedGraphPlotModel.Series.Clear();
            this.SelectedGraphPlotModel.Axes.Add(GetHorizontalAxis());
            this.SelectedGraphPlotModel.Axes.Add(GetVerticalAxis());
            this.SelectedGraphPlotModel.InvalidatePlot(true);
        }

        /// <summary>
        /// グラフタブに表示するデータ(上部)を更新する
        /// </summary>
        /// <param name="categoryId">選択対象の分類ID</param>
        /// <param name="itemId">洗濯対象の項目ID</param>
        /// <returns></returns>
        private async Task UpdateGraphTabDataAsync(int? categoryId = null, int? itemId = null)
        {
            using FuncLog funcLog = new(new { categoryId, itemId });

            int? tmpCategoryId = categoryId ?? this.Parent.SelectedCategoryId ?? null;
            int? tmpItemId = itemId ?? this.Parent.SelectedItemId ?? null;
            Log.Vars(vars: new { tmpCategoryId, tmpItemId });

            // トラッカーのフォーマット文字列を取得する関数
            string GetTrackerFormatString(bool addItemName)
            {
                string unit_pre = DateTimeExtensions.GetYearPreUnit(this.Parent.FiscalStartMonth);
                string unit_post = DateTimeExtensions.GetYearPostUnit(this.Parent.FiscalStartMonth);
                return (addItemName ? "{0}\n" : "") + this.Tab switch {                             //{項目名}\n
                    Tabs.DailyGraphTab => "{Date:yyyy-MM-dd}: {Value:#,0}",                         //{日付}: {金額}
                    Tabs.MonthlyGraphTab => "{Date:yyyy-MM}: {Value:#,0}",                          //{月}: {金額}
                    Tabs.YearlyGraphTab => unit_pre + "{Date:yyyy}" + unit_post + ": {Value:#,0}",  //{単位}{年}{単位}: {金額}
                    _ => throw new InvalidOperationException(),
                };
            }

            var loader = new ViewModelLoader(this.dbHandlerFactory);
            switch (this.Parent.SelectedGraphKind1) {
                case GraphKind1.IncomeAndExpensesGraph: {
                    // グラフ表示データを取得する
                    async Task<ObservableCollection<SeriesViewModel>> GetSeriesVMList()
                    {
                        return this.Tab switch {
                            Tabs.DailyGraphTab => this.Parent.DisplayedTermKind switch {
                                TermKind.Monthly => await loader.LoadDailySeriesViewModelListWithinMonthAsync(this.Parent.SelectedBookVM?.Id, this.Parent.DisplayedMonth.Value),
                                TermKind.Selected => await loader.LoadDailySeriesViewModelListAsync(this.Parent.SelectedBookVM?.Id, this.Parent.StartDate, this.Parent.EndDate),
                                _ => throw new InvalidOperationException(),
                            },
                            Tabs.MonthlyGraphTab => await loader.LoadMonthlySeriesViewModelListWithinYearAsync(this.Parent.SelectedBookVM?.Id, this.Parent.DisplayedYear, this.Parent.FiscalStartMonth),
                            Tabs.YearlyGraphTab => await loader.LoadYearlySeriesViewModelListWithinDecadeAsync(this.Parent.SelectedBookVM?.Id, this.Parent.DisplayedStartYear, this.Parent.FiscalStartMonth),
                            _ => throw new InvalidOperationException(),
                        };
                    }
                    ObservableCollection<SeriesViewModel> tmpVMList = await GetSeriesVMList();
                    // グラフ表示データを設定用に絞り込む
                    switch (this.Parent.SelectedGraphKind2) {
                        case GraphKind2.CategoryGraph:
                            tmpVMList = new(tmpVMList.Where(vm => vm.CategoryId != -1 && vm.ItemId == -1));
                            break;
                        case GraphKind2.ItemGraph:
                            tmpVMList = new(tmpVMList.Where(vm => vm.ItemId != -1));
                            break;
                    }
                    this.GraphSeriesVMList = tmpVMList;

                    // グラフ表示データを設定する
                    this.GraphPlotModel.Series.Clear();
                    List<int> sumPlus = [.. Enumerable.Repeat(0, this.GraphSeriesVMList[0].Values.Count)]; // 単位ごとの合計収入(Y軸範囲の計算に使用)
                    List<int> sumMinus = [.. Enumerable.Repeat(0, this.GraphSeriesVMList[0].Values.Count)]; // 単位ごとの合計支出(Y軸範囲の計算に使用)
                    foreach (SeriesViewModel tmpVM in this.GraphSeriesVMList) {
                        CustomBarSeries wholeSeries = new() {
                            IsStacked = true,
                            Title = tmpVM.DisplayedName,
                            ItemsSource = tmpVM.Values.Zip(tmpVM.StartDates, (value, date) => new GraphDatumViewModel {
                                Value = value,
                                Date = date,
                                ItemId = tmpVM.ItemId,
                                CategoryId = tmpVM.CategoryId
                            }),
                            ValueField = "Value",
                            TrackerFormatString = GetTrackerFormatString(true),
                            XAxisKey = "Value",
                            YAxisKey = "Category"
                        };
                        // 全項目グラフの項目をマウスオーバーした時のイベントを登録する
                        wholeSeries.TrackerHitResultChanged += (sender, e) => {
                            if (e.Value == null) return;

                            GraphDatumViewModel datumVM = e.Value.Item as GraphDatumViewModel;
                            this.SelectedGraphSeriesVM = this.GraphSeriesVMList.FirstOrDefault(tmp => tmp.CategoryId == datumVM.CategoryId && tmp.ItemId == datumVM.ItemId);
                        };
                        this.GraphPlotModel.Series.Add(wholeSeries);

                        // 全項目の単位ごとの合計を計算する
                        for (int i = 0; i < tmpVM.Values.Count; ++i) {
                            if (tmpVM.Values[i] < 0) {
                                sumMinus[i] += tmpVM.Values[i];
                            }
                            else {
                                sumPlus[i] += tmpVM.Values[i];
                            }
                        }
                    }

                    foreach (Axis axis in this.GraphPlotModel.Axes) {
                        // Y軸の範囲を設定する
                        if (axis.Position == AxisPosition.Left) {
                            axis.SetAxisRange(sumMinus.Min(), sumPlus.Max(), 10, true);
                            break;
                        }
                    }
                    break;
                }
                case GraphKind1.BalanceGraph: {
                    // グラフ表示データを取得する
                    async Task<ObservableCollection<SeriesViewModel>> GetGraphSeriesVMList()
                    {
                        return this.Tab switch {
                            Tabs.DailyGraphTab => this.Parent.DisplayedTermKind switch {
                                TermKind.Monthly => await loader.LoadDailySeriesViewModelListWithinMonthAsync(this.Parent.SelectedBookVM?.Id, this.Parent.DisplayedMonth.Value),
                                TermKind.Selected => await loader.LoadDailySeriesViewModelListAsync(this.Parent.SelectedBookVM?.Id, this.Parent.StartDate, this.Parent.EndDate),
                                _ => throw new InvalidOperationException(),
                            },
                            Tabs.MonthlyGraphTab => await loader.LoadMonthlySeriesViewModelListWithinYearAsync(this.Parent.SelectedBookVM.Id, this.Parent.DisplayedYear, this.Parent.FiscalStartMonth),
                            Tabs.YearlyGraphTab => await loader.LoadYearlySeriesViewModelListWithinDecadeAsync(this.Parent.SelectedBookVM.Id, this.Parent.DisplayedStartYear, this.Parent.FiscalStartMonth),
                            _ => throw new InvalidOperationException(),
                        };
                    }
                    this.GraphSeriesVMList = await GetGraphSeriesVMList();

                    // グラフ表示データを設定する
                    this.GraphPlotModel.Series.Clear();
                    SeriesViewModel tmpVM = this.GraphSeriesVMList[0];
                    LineSeries series = new() {
                        Title = Properties.Resources.GraphKind1_BalanceGraph,
                        ItemsSource = tmpVM.Values.Select((value, index) => (value, index)).Zip(tmpVM.StartDates, (tmp, date) => new GraphDatumViewModel() {
                            Value = tmp.value,
                            Date = date,
                            Index = tmp.index
                        }),
                        TrackerFormatString = GetTrackerFormatString(false),
                        DataFieldX = "Index",
                        DataFieldY = "Value"
                    };
                    this.GraphPlotModel.Series.Add(series);

                    foreach (Axis axis in this.GraphPlotModel.Axes) {
                        // Y軸の範囲を設定する
                        if (axis.Position == AxisPosition.Left) {
                            axis.SetAxisRange(tmpVM.Values.Min(), tmpVM.Values.Max(), 10, true);
                            break;
                        }
                    }
                    break;
                }
            }
            this.GraphPlotModel.InvalidatePlot(true);

            this.SelectedGraphSeriesVM = this.GraphSeriesVMList.FirstOrDefault(vm => vm.CategoryId == tmpCategoryId && vm.ItemId == tmpItemId);
        }

        /// <summary>
        /// 選択項目グラフを更新する
        /// </summary>
        public void UpdateSelectedGraph()
        {
            if (this.Parent.SelectedTab != this.Tab) return;
            if (this.Parent.SelectedGraphKind1 != GraphKind1.IncomeAndExpensesGraph) return;

            using FuncLog funcLog = new();

            // トラッカーのフォーマット文字列を取得する関数
            string GetTrackerFormatString()
            {
                string unit_pre = DateTimeExtensions.GetYearPreUnit(this.Parent.FiscalStartMonth);
                string unit_post = DateTimeExtensions.GetYearPostUnit(this.Parent.FiscalStartMonth);
                return this.Tab switch {
                    Tabs.DailyGraphTab => "{Date:yyyy-MM-dd}: {Value:#,0}",                        //{日付}: {金額}
                    Tabs.MonthlyGraphTab => "{Date:yyyy-MM}: {Value:#,0}",                         //{月}: {金額}
                    Tabs.YearlyGraphTab => unit_pre + "{Date:yyyy}" + unit_post + ": {Value:#,0}", //{単位}{年}{単位}: {金額}
                    _ => throw new InvalidOperationException(),
                };
            }

            // グラフ表示データを設定する
            this.SelectedGraphPlotModel.Series.Clear();
            SeriesViewModel vm = this.SelectedGraphSeriesVM;
            if (vm != null) {
                CustomBarSeries selectedSeries = new() {
                    IsStacked = true,
                    Title = vm.DisplayedName,
                    FillColor = (this.GraphPlotModel.Series.FirstOrDefault(series => {
                        List<GraphDatumViewModel> datumVMList = [.. (series as CustomBarSeries).ItemsSource.Cast<GraphDatumViewModel>()];
                        return vm.CategoryId == datumVMList[0].CategoryId && vm.ItemId == datumVMList[0].ItemId;
                    }) as CustomBarSeries).ActualFillColor,
                    ItemsSource = vm.Values.Zip(vm.StartDates, (value, date) => new GraphDatumViewModel {
                        Value = value,
                        Date = date,
                        ItemId = vm.ItemId,
                        CategoryId = vm.CategoryId
                    }),
                    ValueField = "Value",
                    TrackerFormatString = GetTrackerFormatString(),
                    XAxisKey = "Value",
                    YAxisKey = "Category"
                };
                this.SelectedGraphPlotModel.Series.Add(selectedSeries);

                // Y軸の範囲を設定する
                foreach (Axis axis in this.SelectedGraphPlotModel.Axes) {
                    if (axis.Position == AxisPosition.Left) {
                        axis.SetAxisRange(vm.Values.Min(), vm.Values.Max(), 4, true);
                        break;
                    }
                }
            }
            this.SelectedGraphPlotModel.InvalidatePlot(true);
        }
    }
}

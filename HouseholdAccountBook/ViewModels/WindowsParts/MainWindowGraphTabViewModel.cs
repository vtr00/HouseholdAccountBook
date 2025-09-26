using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Models.Logger;
using HouseholdAccountBook.Models.Services;
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
        public event Action SelectedSeriesChanged = default;
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
                if (this.SetProperty(ref this._SelectedGraphSeriesVM, value)) {
                    this.Parent.SelectedBalanceKind = value?.BalanceKind;
                    this.Parent.SelectedCategoryId = value?.CategoryId;
                    this.Parent.SelectedItemId = value?.ItemId;

                    this.SelectedSeriesChanged?.Invoke();
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

        /// <summary>
        /// グラフタブを更新する
        /// </summary>
        /// <param name="categoryId">分類ID</param>
        /// <param name="itemId">項目ID</param>
        /// <returns></returns>
        public async Task UpdateGraphTabDataAsync(int? categoryId = null, int? itemId = null)
        {
            if (this.Parent.SelectedTab != this.Tab) return;

            switch (this.Tab) {
                case Tabs.DailyGraphTab:
                    this.InitializeDailyGraphTabData();
                    await this.UpdateDailyGraphTabDataAsync(categoryId, itemId);
                    if (this.Parent.SelectedGraphKind1 == GraphKind1.IncomeAndExpensesGraph) {
                        this.UpdateSelectedDailyGraph();
                    }
                    break;
                case Tabs.MonthlyGraphTab:
                    this.InitializeMonthlyGraphTabData();
                    await this.UpdateMonthlyGraphTabDataAsync(categoryId, itemId);
                    if (this.Parent.SelectedGraphKind1 == GraphKind1.IncomeAndExpensesGraph) {
                        this.UpdateSelectedMonthlyGraph();
                    }
                    break;
                case Tabs.YearlyGraphTab:
                    this.InitializeYearlyGraphTabData();
                    await this.UpdateYearlyGraphTabDataAsync(categoryId, itemId);
                    if (this.Parent.SelectedGraphKind1 == GraphKind1.IncomeAndExpensesGraph) {
                        this.UpdateSelectedYearlyGraph();
                    }
                    break;
            }
        }

        /// <summary>
        /// 個別グラフを更新する
        /// </summary>
        public void UpdateSelectedGraph()
        {
            if (this.Parent.SelectedTab != this.Tab) return;
            if (this.Parent.SelectedGraphKind1 != GraphKind1.IncomeAndExpensesGraph) return;

            switch (this.Tab) {
                case Tabs.DailyGraphTab:
                    this.UpdateSelectedDailyGraph();
                    break;
                case Tabs.MonthlyGraphTab:
                    this.UpdateSelectedMonthlyGraph();
                    break;
                case Tabs.YearlyGraphTab:
                    this.UpdateSelectedYearlyGraph();
                    break;
            }
        }

        #region 日別グラフタブ更新用の関数
        /// <summary>
        /// 日別グラフタブに表示するデータを初期化する
        /// </summary>
        private void InitializeDailyGraphTabData()
        {
            Log.Info();

            #region 全項目
            this.GraphPlotModel.Axes.Clear();
            this.GraphPlotModel.Series.Clear();

            // 横軸 - 日軸
            CategoryAxis horizontalAxis1 = new() {
                Unit = Properties.Resources.Unit_Day,
                Position = AxisPosition.Bottom,
                Key = "Category"
            };
            horizontalAxis1.Labels.Clear(); // 内部的にLabelsの値が共有されているのか、正常な表示にはこのコードが必要
            // 表示する日の文字列を作成する
            for (DateTime tmp = this.Parent.StartDate; tmp <= this.Parent.EndDate; tmp = tmp.AddDays(1)) {
                horizontalAxis1.Labels.Add(tmp.ToString("%d"));
            }
            this.GraphPlotModel.Axes.Add(horizontalAxis1);

            // 縦軸 - 線形軸
            LinearAxis verticalAxis1 = new() {
                Unit = Properties.Resources.Unit_Money,
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                StringFormat = "#,0",
                Key = "Value",
                Title = GraphKind1Str[this.Parent.SelectedGraphKind1]
            };
            this.GraphPlotModel.Axes.Add(verticalAxis1);

            this.GraphPlotModel.InvalidatePlot(true);
            #endregion

            #region 選択項目
            this.SelectedGraphPlotModel.Axes.Clear();
            this.SelectedGraphPlotModel.Series.Clear();

            // 横軸 - 日軸
            CategoryAxis horizontalAxis2 = new() {
                Unit = Properties.Resources.Unit_Day,
                Position = AxisPosition.Bottom,
                Key = "Category"
            };
            horizontalAxis2.Labels.Clear(); // 内部的にLabelsの値が共有されているのか、正常な表示にはこのコードが必要
            // 表示する日の文字列を作成する
            for (DateTime tmp = this.Parent.StartDate; tmp <= this.Parent.EndDate; tmp = tmp.AddDays(1)) {
                horizontalAxis2.Labels.Add(tmp.ToString("%d"));
            }
            this.SelectedGraphPlotModel.Axes.Add(horizontalAxis2);

            // 縦軸 - 線形軸
            LinearAxis verticalAxis2 = new() {
                Unit = Properties.Resources.Unit_Money,
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                StringFormat = "#,0",
                Key = "Value",
                Title = GraphKind1Str[this.Parent.SelectedGraphKind1]
            };
            this.SelectedGraphPlotModel.Axes.Add(verticalAxis2);

            this.SelectedGraphPlotModel.InvalidatePlot(true);
            #endregion
        }

        /// <summary>
        /// 日別グラフタブに表示するデータを更新する
        /// </summary>
        /// <param name="categoryId">選択対象の分類ID</param>
        /// <param name="itemId">選択対象の項目ID</param>
        private async Task UpdateDailyGraphTabDataAsync(int? categoryId = null, int? itemId = null)
        {
            Log.Info($"categoryId:{categoryId} itemId:{itemId}");

            int? tmpCategoryId = categoryId ?? this.Parent.SelectedCategoryId;
            int? tmpItemId = itemId ?? this.Parent.SelectedItemId ?? null;
            Log.Info($"tmpCategoryId:{tmpCategoryId} tmpItemId:{tmpItemId}");

            switch (this.Parent.SelectedGraphKind1) {
                case GraphKind1.IncomeAndExpensesGraph: {
                    List<int> sumPlus = Enumerable.Repeat(0, 31).ToList(); // 日ごとの合計収入(Y軸範囲の計算に使用)
                    List<int> sumMinus = Enumerable.Repeat(0, 31).ToList(); // 日ごとの合計支出(Y軸範囲の計算に使用)

                    // グラフ表示データを取得する
                    var loader = new ViewModelLoader(this.dbHandlerFactory);
                    ObservableCollection<SeriesViewModel> tmpVMList = null;
                    switch (this.Parent.DisplayedTermKind) {
                        case TermKind.Monthly:
                            tmpVMList = await loader.LoadDailySeriesViewModelListWithinMonthAsync(this.Parent.SelectedBookVM?.Id, this.Parent.DisplayedMonth.Value);
                            break;
                        case TermKind.Selected:
                            tmpVMList = await loader.LoadDailySeriesViewModelListAsync(this.Parent.SelectedBookVM?.Id, this.Parent.StartDate, this.Parent.EndDate);
                            break;
                    }
                    // グラフ表示データを設定用に絞り込む
                    switch (this.Parent.SelectedGraphKind2) {
                        case GraphKind2.CategoryGraph:
                            tmpVMList = new ObservableCollection<SeriesViewModel>(tmpVMList.Where((vm) => vm.CategoryId != -1 && vm.ItemId == -1));
                            break;
                        case GraphKind2.ItemGraph:
                            tmpVMList = new ObservableCollection<SeriesViewModel>(tmpVMList.Where((vm) => vm.ItemId != -1));
                            break;
                    }
                    this.GraphSeriesVMList = tmpVMList;

                    // グラフ表示データを設定する
                    this.GraphPlotModel.Series.Clear();
                    foreach (SeriesViewModel tmpVM in this.GraphSeriesVMList) {
                        CustomBarSeries wholeSeries = new() {
                            IsStacked = true,
                            Title = tmpVM.DisplayedName,
                            ItemsSource = tmpVM.Values.Select((value, index) => {
                                return new GraphDatumViewModel {
                                    Value = value,
                                    Number = index,
                                    ItemId = tmpVM.ItemId,
                                    CategoryId = tmpVM.CategoryId
                                };
                            }),
                            ValueField = "Value",
                            TrackerFormatString = "{0}\n{1}" + Properties.Resources.Unit_Day + ": {2:#,0}", //日付: 金額
                            XAxisKey = "Value",
                            YAxisKey = "Category"
                        };
                        // 全項目日別グラフの項目をマウスオーバーした時のイベントを登録する
                        wholeSeries.TrackerHitResultChanged += (sender, e) => {
                            if (e.Value == null) return;

                            GraphDatumViewModel datumVM = e.Value.Item as GraphDatumViewModel;
                            this.SelectedGraphSeriesVM = this.GraphSeriesVMList.FirstOrDefault((tmp) => tmp.CategoryId == datumVM.CategoryId && tmp.ItemId == datumVM.ItemId);
                        };

                        this.GraphPlotModel.Series.Add(wholeSeries);

                        // 全項目の日毎の合計を計算する
                        for (int i = 0; i < tmpVM.Values.Count; ++i) {
                            if (tmpVM.Values[i] < 0) {
                                sumMinus[i] += tmpVM.Values[i];
                            }
                            else {
                                sumPlus[i] += tmpVM.Values[i];
                            }
                        }
                    }

                    // Y軸の範囲を設定する
                    foreach (Axis axis in this.GraphPlotModel.Axes) {
                        if (axis.Position == AxisPosition.Left) {
                            axis.SetAxisRange(sumMinus.Min(), sumPlus.Max(), 10, true);
                            break;
                        }
                    }
                    break;
                }
                case GraphKind1.BalanceGraph: {
                    // グラフ表示データを取得する
                    var loader = new ViewModelLoader(this.dbHandlerFactory);
                    ObservableCollection<SeriesViewModel> seriesVMList = null;
                    switch (this.Parent.DisplayedTermKind) {
                        case TermKind.Monthly:
                            seriesVMList = await loader.LoadDailySeriesViewModelListWithinMonthAsync(this.Parent.SelectedBookVM?.Id, this.Parent.DisplayedMonth.Value);
                            break;
                        case TermKind.Selected:
                            seriesVMList = await loader.LoadDailySeriesViewModelListAsync(this.Parent.SelectedBookVM?.Id, this.Parent.StartDate, this.Parent.EndDate);
                            break;
                    }
                    this.GraphSeriesVMList = seriesVMList;

                    // グラフ表示データを設定する
                    this.GraphPlotModel.Series.Clear();
                    LineSeries series = new() {
                        Title = Properties.Resources.GraphKind1_BalanceGraph,
                        TrackerFormatString = "{2}" + Properties.Resources.Unit_Day + ": {4:#,0}" //日付: 金額
                    };
                    series.Points.AddRange(new List<int>(this.GraphSeriesVMList[0].Values).Select((value, index) => new DataPoint(index, value)));
                    this.GraphPlotModel.Series.Add(series);

                    // Y軸の範囲を設定する
                    foreach (Axis axis in this.GraphPlotModel.Axes) {
                        if (axis.Position == AxisPosition.Left) {
                            axis.SetAxisRange(series.Points.Min((value) => value.Y), series.Points.Max((value) => value.Y), 10, true);
                            break;
                        }
                    }
                    break;
                }
            }
            this.GraphPlotModel.InvalidatePlot(true);

            this.SelectedGraphSeriesVM = this.GraphSeriesVMList.FirstOrDefault((vm) => vm.CategoryId == tmpCategoryId && vm.ItemId == tmpItemId);
        }

        /// <summary>
        /// 選択項目日別グラフを更新する
        /// </summary>
        private void UpdateSelectedDailyGraph()
        {
            Log.Info();

            SeriesViewModel vm = this.SelectedGraphSeriesVM;

            // グラフ表示データを設定する
            this.SelectedGraphPlotModel.Series.Clear();
            if (vm != null) {
                CustomBarSeries slectedSeries = new() {
                    IsStacked = true,
                    Title = vm.DisplayedName,
                    FillColor = (this.GraphPlotModel.Series.FirstOrDefault((s) => {
                        List<GraphDatumViewModel> datumVMList = new((s as CustomBarSeries).ItemsSource.Cast<GraphDatumViewModel>());
                        return vm.CategoryId == datumVMList[0].CategoryId && vm.ItemId == datumVMList[0].ItemId;
                    }) as CustomBarSeries).ActualFillColor,
                    ItemsSource = vm.Values.Select((value, index) => {
                        return new GraphDatumViewModel {
                            Value = value,
                            Number = index,
                            ItemId = vm.ItemId,
                            CategoryId = vm.CategoryId
                        };
                    }),
                    ValueField = "Value",
                    TrackerFormatString = "{1}" + Properties.Resources.Unit_Day + ": {2:#,0}", //日付: 金額
                    XAxisKey = "Value",
                    YAxisKey = "Category"
                };

                this.SelectedGraphPlotModel.Series.Add(slectedSeries);

                foreach (Axis axis in this.SelectedGraphPlotModel.Axes) {
                    if (axis.Position == AxisPosition.Left) {
                        axis.SetAxisRange(vm.Values.Min(), vm.Values.Max(), 4, true);
                        break;
                    }
                }
            }

            this.SelectedGraphPlotModel.InvalidatePlot(true);
        }
        #endregion

        #region 月別グラフタブ更新用の関数
        /// <summary>
        /// 月別グラフタブに表示するデータを初期化する
        /// </summary>
        private void InitializeMonthlyGraphTabData()
        {
            Log.Info();

            int startMonth = this.Parent.FiscalStartMonth;

            #region 全項目
            this.GraphPlotModel.Axes.Clear();
            this.GraphPlotModel.Series.Clear();

            // 横軸 - 月軸
            CategoryAxis horizontalAxis1 = new() {
                Unit = Properties.Resources.Unit_Month,
                Position = AxisPosition.Bottom,
                Key = "Category"
            };
            horizontalAxis1.Labels.Clear(); // 内部的にLabelsの値が共有されているのか、正常な表示にはこのコードが必要
            // 表示する月の文字列を作成する
            for (int i = startMonth; i < startMonth + 12; ++i) {
                horizontalAxis1.Labels.Add($"{((i - 1) % 12) + 1}");
            }
            this.GraphPlotModel.Axes.Add(horizontalAxis1);

            // 縦軸 - 線形軸
            LinearAxis verticalAxis1 = new() {
                Unit = Properties.Resources.Unit_Money,
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                StringFormat = "#,0",
                Key = "Value",
                Title = GraphKind1Str[this.Parent.SelectedGraphKind1]
            };
            this.GraphPlotModel.Axes.Add(verticalAxis1);

            this.GraphPlotModel.InvalidatePlot(true);
            #endregion

            #region 選択項目
            this.SelectedGraphPlotModel.Axes.Clear();
            this.SelectedGraphPlotModel.Series.Clear();

            // 横軸 - 月軸
            CategoryAxis horizontalAxis2 = new() {
                Unit = Properties.Resources.Unit_Month,
                Position = AxisPosition.Bottom,
                Key = "Category"
            };
            horizontalAxis2.Labels.Clear(); // 内部的にLabelsの値が共有されているのか、正常な表示にはこのコードが必要
            // 表示する月の文字列を作成する
            for (int i = startMonth; i < startMonth + 12; ++i) {
                horizontalAxis2.Labels.Add($"{((i - 1) % 12) + 1}");
            }
            this.SelectedGraphPlotModel.Axes.Add(horizontalAxis2);

            // 縦軸 - 線形軸
            LinearAxis verticalAxis2 = new() {
                Unit = Properties.Resources.Unit_Money,
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                StringFormat = "#,0",
                Key = "Value",
                Title = GraphKind1Str[this.Parent.SelectedGraphKind1]
            };
            this.SelectedGraphPlotModel.Axes.Add(verticalAxis2);

            this.SelectedGraphPlotModel.InvalidatePlot(true);
            #endregion
        }

        /// <summary>
        /// 月別グラフタブに表示するデータを更新する
        /// </summary>
        /// <param name="categoryId">選択対象の分類ID</param>
        /// <param name="itemId">選択対象の項目ID</param>
        private async Task UpdateMonthlyGraphTabDataAsync(int? categoryId = null, int? itemId = null)
        {
            Log.Info($"categoryId:{categoryId} itemId:{itemId}");

            int? tmpCategoryId = categoryId ?? this.Parent.SelectedCategoryId;
            int? tmpItemId = itemId ?? this.Parent.SelectedItemId;
            Log.Info($"tmpCategoryId:{tmpCategoryId} tmpItemId:{tmpItemId}");

            int startMonth = this.Parent.FiscalStartMonth;

            switch (this.Parent.SelectedGraphKind1) {
                case GraphKind1.IncomeAndExpensesGraph: {
                    List<int> sumPlus = Enumerable.Repeat(0, 12).ToList(); // 月ごとの合計収入
                    List<int> sumMinus = Enumerable.Repeat(0, 12).ToList(); // 月ごとの合計支出

                    // グラフ表示データを取得する
                    var loader = new ViewModelLoader(this.dbHandlerFactory);
                    ObservableCollection<SeriesViewModel> tmpVMList = await loader.LoadMonthlySeriesViewModelListWithinYearAsync(this.Parent.SelectedBookVM?.Id, this.Parent.DisplayedYear, this.Parent.FiscalStartMonth);
                    // グラフ表示データを設定用に絞り込む
                    switch (this.Parent.SelectedGraphKind2) {
                        case GraphKind2.CategoryGraph:
                            tmpVMList = new ObservableCollection<SeriesViewModel>(tmpVMList.Where((vm) => vm.CategoryId != -1 && vm.ItemId == -1));
                            break;
                        case GraphKind2.ItemGraph:
                            tmpVMList = new ObservableCollection<SeriesViewModel>(tmpVMList.Where((vm) => vm.ItemId != -1));
                            break;
                    }
                    this.GraphSeriesVMList = tmpVMList;

                    // グラフ表示データを設定する
                    this.GraphPlotModel.Series.Clear();
                    foreach (SeriesViewModel tmpVM in this.GraphSeriesVMList) {
                        CustomBarSeries wholeSeries = new() {
                            IsStacked = true,
                            Title = tmpVM.DisplayedName,
                            ItemsSource = tmpVM.Values.Select((value, index) => new GraphDatumViewModel {
                                Value = value,
                                Number = index + startMonth,
                                ItemId = tmpVM.ItemId,
                                CategoryId = tmpVM.CategoryId
                            }),
                            ValueField = "Value",
                            TrackerFormatString = "{0}\n{1}" + Properties.Resources.Unit_Month + ": {2:#,0}", //月: 金額
                            XAxisKey = "Value",
                            YAxisKey = "Category"
                        };
                        // 全項目年間グラフの項目をマウスオーバーした時のイベントを登録する
                        wholeSeries.TrackerHitResultChanged += (sender, e) => {
                            if (e.Value == null) return;

                            GraphDatumViewModel datumVM = e.Value.Item as GraphDatumViewModel;
                            this.SelectedGraphSeriesVM = this.GraphSeriesVMList.FirstOrDefault((tmp) => tmp.CategoryId == datumVM.CategoryId && tmp.ItemId == datumVM.ItemId);
                        };
                        this.GraphPlotModel.Series.Add(wholeSeries);

                        // 全項目の月毎の合計を計算する
                        for (int i = 0; i < tmpVM.Values.Count; ++i) {
                            if (tmpVM.Values[i] < 0) {
                                sumMinus[i] += tmpVM.Values[i];
                            }
                            else {
                                sumPlus[i] += tmpVM.Values[i];
                            }
                        }
                    }

                    // Y軸の範囲を設定する
                    foreach (Axis axis in this.GraphPlotModel.Axes) {
                        if (axis.Position == AxisPosition.Left) {
                            axis.SetAxisRange(sumMinus.Min(), sumPlus.Max(), 10, true);
                            break;
                        }
                    }
                    break;
                }
                case GraphKind1.BalanceGraph: {
                    // グラフ表示データを取得する
                    var loader = new ViewModelLoader(this.dbHandlerFactory);
                    this.GraphSeriesVMList = await loader.LoadMonthlySeriesViewModelListWithinYearAsync(this.Parent.SelectedBookVM.Id, this.Parent.DisplayedYear, this.Parent.FiscalStartMonth);

                    // グラフ表示データを設定する
                    this.GraphPlotModel.Series.Clear();
                    LineSeries series = new() {
                        Title = Properties.Resources.GraphKind1_BalanceGraph,
                        TrackerFormatString = "{2}" + Properties.Resources.Unit_Month + ": {4:#,0}" //月: 金額
                    };
                    series.Points.AddRange(new List<int>(this.GraphSeriesVMList[0].Values).Select((value, index) => new DataPoint(index, value)));
                    this.GraphPlotModel.Series.Add(series);

                    // Y軸の範囲を設定する
                    foreach (Axis axis in this.GraphPlotModel.Axes) {
                        if (axis.Position == AxisPosition.Left) {
                            axis.SetAxisRange(series.Points.Min((value) => value.Y), series.Points.Max((value) => value.Y), 10, true);
                            break;
                        }
                    }
                    break;
                }
            }
            this.GraphPlotModel.InvalidatePlot(true);

            this.SelectedGraphSeriesVM = this.GraphSeriesVMList.FirstOrDefault((vm) => vm.CategoryId == tmpCategoryId && vm.ItemId == tmpItemId);
        }

        /// <summary>
        /// 選択項目月別グラフを更新する
        /// </summary>
        private void UpdateSelectedMonthlyGraph()
        {
            Log.Info();

            int startMonth = this.Parent.FiscalStartMonth;

            SeriesViewModel vm = this.SelectedGraphSeriesVM;

            // グラフ表示データを設定する
            this.SelectedGraphPlotModel.Series.Clear();
            if (vm != null) {
                CustomBarSeries selectedSeries = new() {
                    IsStacked = true,
                    Title = vm.DisplayedName,
                    FillColor = (this.GraphPlotModel.Series.FirstOrDefault((series) => {
                        List<GraphDatumViewModel> datumVMList = new((series as CustomBarSeries).ItemsSource.Cast<GraphDatumViewModel>());
                        return vm.CategoryId == datumVMList[0].CategoryId && vm.ItemId == datumVMList[0].ItemId;
                    }) as CustomBarSeries).ActualFillColor,
                    ItemsSource = vm.Values.Select((value, index) => new GraphDatumViewModel {
                        Value = value,
                        Number = index + startMonth,
                        ItemId = vm.ItemId,
                        CategoryId = vm.CategoryId
                    }),
                    ValueField = "Value",
                    TrackerFormatString = "{1}" + Properties.Resources.Unit_Month + ": {2:#,0}", //月: 金額
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
        #endregion

        #region 年別グラフタブ更新用の関数
        /// <summary>
        /// 年別グラフタブに表示するデータを初期化する
        /// </summary>
        private void InitializeYearlyGraphTabData()
        {
            Log.Info();

            int startYear = this.Parent.DisplayedStartYear.Year;

            #region 全項目
            this.GraphPlotModel.Axes.Clear();
            this.GraphPlotModel.Series.Clear();

            // 横軸 - 年軸
            CategoryAxis horizontalAxis1 = new() {
                Unit = this.Parent.FiscalStartMonth == 1 ? Properties.Resources.Unit_Year : Properties.Resources.Unit_FiscalYear,
                Position = AxisPosition.Bottom,
                Key = "Category"
            };
            horizontalAxis1.Labels.Clear(); // 内部的にLabelsの値が共有されているのか、正常な表示にはこのコードが必要
            // 表示する年の文字列を作成する
            for (int i = startYear; i < startYear + 10; ++i) {
                horizontalAxis1.Labels.Add(this.Parent.FiscalStartMonth == 1 ? $"{i}" : $"'{i}");
            }
            this.GraphPlotModel.Axes.Add(horizontalAxis1);

            // 縦軸 - 線形軸
            LinearAxis verticalAxis1 = new() {
                Unit = Properties.Resources.Unit_Money,
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                StringFormat = "#,0",
                Key = "Value",
                Title = GraphKind1Str[this.Parent.SelectedGraphKind1]
            };
            this.GraphPlotModel.Axes.Add(verticalAxis1);

            this.GraphPlotModel.InvalidatePlot(true);
            #endregion

            #region 選択項目
            this.SelectedGraphPlotModel.Axes.Clear();
            this.SelectedGraphPlotModel.Series.Clear();

            // 横軸 - 年軸
            CategoryAxis horizontalAxis2 = new() {
                Unit = this.Parent.FiscalStartMonth == 1 ? Properties.Resources.Unit_Year : Properties.Resources.Unit_FiscalYear,
                Position = AxisPosition.Bottom,
                Key = "Category"
            };
            horizontalAxis2.Labels.Clear(); // 内部的にLabelsの値が共有されているのか、正常な表示にはこのコードが必要
            // 表示する年の文字列を作成する
            for (int i = startYear; i < startYear + 10; ++i) {
                horizontalAxis2.Labels.Add(this.Parent.FiscalStartMonth == 1 ? $"{i}" : $"'{i}");
            }
            this.SelectedGraphPlotModel.Axes.Add(horizontalAxis2);

            // 縦軸 - 線形軸
            LinearAxis verticalAxis2 = new() {
                Unit = Properties.Resources.Unit_Money,
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                StringFormat = "#,0",
                Key = "Value",
                Title = GraphKind1Str[this.Parent.SelectedGraphKind1]
            };
            this.SelectedGraphPlotModel.Axes.Add(verticalAxis2);

            this.SelectedGraphPlotModel.InvalidatePlot(true);
            #endregion
        }

        /// <summary>
        /// 年別グラフタブに表示するデータを更新する
        /// </summary>
        /// <param name="categoryId">選択対象の分類ID</param>
        /// <param name="itemId">選択対象の項目ID</param>
        private async Task UpdateYearlyGraphTabDataAsync(int? categoryId = null, int? itemId = null)
        {
            Log.Info($"categoryId:{categoryId} itemId:{itemId}");

            int? tmpCategoryId = categoryId ?? this.Parent.SelectedCategoryId;
            int? tmpItemId = itemId ?? this.Parent.SelectedItemId;
            Log.Info($"tmpCategoryId:{tmpCategoryId} tmpItemId:{tmpItemId}");

            int startYear = this.Parent.DisplayedStartYear.Year;

            switch (this.Parent.SelectedGraphKind1) {
                case GraphKind1.IncomeAndExpensesGraph: {
                    List<int> sumPlus = Enumerable.Repeat(0, 10).ToList(); // 年ごとの合計収入
                    List<int> sumMinus = Enumerable.Repeat(0, 10).ToList(); // 年ごとの合計支出

                    // グラフ表示データを取得する
                    var loader = new ViewModelLoader(this.dbHandlerFactory);
                    ObservableCollection<SeriesViewModel> tmpVMList = await loader.LoadYearlySeriesViewModelListWithinDecadeAsync(this.Parent.SelectedBookVM?.Id, this.Parent.DisplayedStartYear, this.Parent.FiscalStartMonth);
                    // グラフ表示データを設定用に絞り込む
                    switch (this.Parent.SelectedGraphKind2) {
                        case GraphKind2.CategoryGraph:
                            tmpVMList = new ObservableCollection<SeriesViewModel>(tmpVMList.Where((vm) => vm.CategoryId != -1 && vm.ItemId == -1));
                            break;
                        case GraphKind2.ItemGraph:
                            tmpVMList = new ObservableCollection<SeriesViewModel>(tmpVMList.Where((vm) => vm.ItemId != -1));
                            break;
                    }
                    this.GraphSeriesVMList = tmpVMList;

                    // グラフ表示データを設定する
                    this.GraphPlotModel.Series.Clear();
                    foreach (SeriesViewModel tmpVM in this.GraphSeriesVMList) {
                        CustomBarSeries wholeSeries = new() {
                            IsStacked = true,
                            Title = tmpVM.DisplayedName,
                            ItemsSource = tmpVM.Values.Select((value, index) => new GraphDatumViewModel {
                                Value = value,
                                Number = index + startYear,
                                ItemId = tmpVM.ItemId,
                                CategoryId = tmpVM.CategoryId
                            }),
                            ValueField = "Value",
                            TrackerFormatString = "{0}\n{1}" + Properties.Resources.Unit_FiscalYear + ": {2:#,0}", //年度: 金額
                            XAxisKey = "Value",
                            YAxisKey = "Category"
                        };
                        // 全項目年間グラフの項目を選択した時のイベントを登録する
                        wholeSeries.TrackerHitResultChanged += (sender, e) => {
                            if (e.Value == null) return;

                            GraphDatumViewModel datumVM = e.Value.Item as GraphDatumViewModel;
                            this.SelectedGraphSeriesVM = this.GraphSeriesVMList.FirstOrDefault((tmp) => tmp.CategoryId == datumVM.CategoryId && tmp.ItemId == datumVM.ItemId);
                        };
                        this.GraphPlotModel.Series.Add(wholeSeries);

                        // 全項目の月毎の合計を計算する
                        for (int i = 0; i < tmpVM.Values.Count; ++i) {
                            if (tmpVM.Values[i] < 0) {
                                sumMinus[i] += tmpVM.Values[i];
                            }
                            else {
                                sumPlus[i] += tmpVM.Values[i];
                            }
                        }
                    }

                    // Y軸の範囲を設定する
                    foreach (Axis axis in this.GraphPlotModel.Axes) {
                        if (axis.Position == AxisPosition.Left) {
                            axis.SetAxisRange(sumMinus.Min(), sumPlus.Max(), 10, true);
                            break;
                        }
                    }
                    break;
                }
                case GraphKind1.BalanceGraph: {
                    // グラフ表示データを取得する
                    var loader = new ViewModelLoader(this.dbHandlerFactory);
                    this.GraphSeriesVMList = await loader.LoadYearlySeriesViewModelListWithinDecadeAsync(this.Parent.SelectedBookVM.Id, this.Parent.DisplayedStartYear, this.Parent.FiscalStartMonth);

                    // グラフ表示データを設定する
                    this.GraphPlotModel.Series.Clear();
                    LineSeries series = new() {
                        Title = Properties.Resources.GraphKind1_BalanceGraph,
                        TrackerFormatString = "{2}" + Properties.Resources.Unit_FiscalYear + ": {4:#,0}" //年度: 金額
                    };
                    series.Points.AddRange(new List<int>(this.GraphSeriesVMList[0].Values).Select((value, index) => new DataPoint(index, value)));
                    this.GraphPlotModel.Series.Add(series);

                    // Y軸の範囲を設定する
                    foreach (Axis axis in this.GraphPlotModel.Axes) {
                        if (axis.Position == AxisPosition.Left) {
                            axis.SetAxisRange(series.Points.Min((value) => value.Y), series.Points.Max((value) => value.Y), 10, true);
                            break;
                        }
                    }
                    break;
                }
            }
            this.GraphPlotModel.InvalidatePlot(true);

            this.SelectedGraphSeriesVM = this.GraphSeriesVMList.FirstOrDefault((vm) => vm.CategoryId == tmpCategoryId && vm.ItemId == tmpItemId);
        }

        /// <summary>
        /// 選択項目年別グラフを更新する
        /// </summary>
        private void UpdateSelectedYearlyGraph()
        {
            Log.Info();

            int startYear = this.Parent.DisplayedStartYear.Year;

            SeriesViewModel vm = this.SelectedGraphSeriesVM;

            // グラフ表示データを設定する
            this.SelectedGraphPlotModel.Series.Clear();
            if (vm != null) {
                CustomBarSeries selectedSeries = new() {
                    IsStacked = true,
                    Title = vm.DisplayedName,
                    FillColor = (this.GraphPlotModel.Series.FirstOrDefault((series) => {
                        List<GraphDatumViewModel> datumVMList = new((series as CustomBarSeries).ItemsSource.Cast<GraphDatumViewModel>());
                        return vm.CategoryId == datumVMList[0].CategoryId && vm.ItemId == datumVMList[0].ItemId;
                    }) as CustomBarSeries).ActualFillColor,
                    ItemsSource = vm.Values.Select((value, index) => new GraphDatumViewModel {
                        Value = value,
                        Number = index + startYear,
                        ItemId = vm.ItemId,
                        CategoryId = vm.CategoryId
                    }),
                    ValueField = "Value",
                    TrackerFormatString = "{1}" + Properties.Resources.Unit_FiscalYear + ": {2:#,0}", //年度: 金額
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
        #endregion
    }
}

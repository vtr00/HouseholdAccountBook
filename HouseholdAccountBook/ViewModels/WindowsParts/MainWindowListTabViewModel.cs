using HouseholdAccountBook.Infrastructure.CSV;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Infrastructure.Utilities.Args;
using HouseholdAccountBook.Infrastructure.Utilities.Args.RequestEventArgs;
using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using HouseholdAccountBook.ViewModels.Loaders;
using HouseholdAccountBook.ViewModels.Windows;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace HouseholdAccountBook.ViewModels.WindowsParts
{
    /// <summary>
    /// メインウィンドウ リストタブVM
    /// </summary>
    public class MainWindowListTabViewModel : WindowPartViewModelBase
    {
        private MainWindowViewModel Parent { get; init; }

        private Tabs Tab { get; init; }

        #region イベント
        /// <summary>
        /// 系列選択変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<(BalanceKind?, CategoryIdObj, ItemIdObj)?>> SelectedSeriesChanged;
        #endregion

        #region Bindingプロパティ
        /// <summary>
        /// 系列セレクタVM
        /// </summary>
        public SelectorViewModel<SeriesViewModel, (BalanceKind?, CategoryIdObj, ItemIdObj)?> SeriesSelectorVM => field ??= new(static vm => vm?.ToTuple(), this.mBusyService);
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="parent">親VM</param>
        /// <param name="tab">タブ</param>
        public MainWindowListTabViewModel(MainWindowViewModel parent, Tabs tab)
        {
            using FuncLog funcLog = new(new { tab });

            this.Parent = parent;
            this.Tab = tab;
        }

        public override void Initialize(BusyService busyService, DbHandlerFactory dbHandlerFactory)
        {
            base.Initialize(busyService, dbHandlerFactory);

            this.SeriesSelectorVM.SetLoader(async _ => {
                SeriesViewModelLoader loader = new(new(this.mDbHandlerFactory));
                return this.Tab switch {
                    Tabs.MonthlyListTab => await loader.LoadMonthlySeriesViewModelListWithinYearAsync(this.Parent.BookSelectorVM.SelectedKey, this.Parent.DisplayedYear, this.Parent.FiscalStartMonth),
                    Tabs.YearlyListTab => await loader.LoadYearlySeriesViewModelListWithinDecadeAsync(this.Parent.BookSelectorVM.SelectedKey, this.Parent.DisplayedStartYear, this.Parent.FiscalStartMonth),
                    _ => throw new NotImplementedException(),
                };
            }, mode: SelectorMode.FirstOrDefault);
        }

        public override async Task LoadAsync() => await this.LoadAsync(null, null, null);

        /// <summary>
        /// リストタブに表示するデータを読み込む
        /// </summary>
        /// <param name="balanceKind">収支種別</param>
        /// <param name="categoryId">分類ID</param>
        /// <param name="itemId">項目ID</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task LoadAsync(BalanceKind? balanceKind = null, CategoryIdObj categoryId = null, ItemIdObj itemId = null)
        {
            if (this.Parent.SelectedTab != this.Tab) { return; }

            using FuncLog funcLog = new(new { balanceKind, categoryId, itemId });
            using IDisposable disposable = this.mBusyService.Enter();

            // 指定がなければ、更新前のサマリーの選択を維持する
            BalanceKind? tmpBalanceKind = balanceKind ?? this.Parent.SelectedBalanceKind;
            CategoryIdObj tmpCategoryId = categoryId ?? this.Parent.SelectedCategoryId;
            ItemIdObj tmpItemId = itemId ?? this.Parent.SelectedItemId;
            Log.Vars(vars: new { tmpBalanceKind, tmpCategoryId, tmpItemId });

            await this.SeriesSelectorVM.LoadAsync((tmpBalanceKind, tmpCategoryId, tmpItemId));
        }

        public override void AddEventHandlers()
        {
#pragma warning disable IDE0022 // メソッドに式本体を使用する
            this.SeriesSelectorVM.SelectionChanged += (sender, e) => this.SelectedSeriesChanged?.Invoke(sender, e);
#pragma warning restore IDE0022 // メソッドに式本体を使用する
        }

        /// <summary>
        /// DataGridの情報をCSVファイルにエクスポートする
        /// </summary>
        /// <param name="rows">列表示値</param>
        public async Task ExportCSVFileAsync(IEnumerable<IEnumerable<string>> rows)
        {
            (string folderPath, string fileName) = PathUtil.GetSeparatedPath(UserSettingService.Instance.ExportCsvFilePath, App.GetCurrentDir());

            SaveFileDialogRequestEventArgs e = new() {
                InitialDirectory = folderPath,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = Properties.Resources.FileSelectFilter_CsvFile + "|*.csv"
            };
            if (this.SaveFileDialogRequest(e)) {
                // 選択したCSVファイルのパスを設定として保存する
                UserSettingService.Instance.ExportCsvFilePath = e.FileName;

                await CSVFileDao.SaveDataAsync(e.FileName, rows);
            }

            bool result = true;
            _ = result
                ? MessageBox.Show(Properties.Resources.Message_FinishToExport, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK)
                : MessageBox.Show(Properties.Resources.Message_FoultToExport, Properties.Resources.Title_Conformation, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        }
    }
}

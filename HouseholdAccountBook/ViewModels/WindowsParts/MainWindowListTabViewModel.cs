using HouseholdAccountBook.Infrastructure.CSV;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Infrastructure.Utilities.Args.RequestEventArgs;
using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using HouseholdAccountBook.ViewModels.Loaders;
using HouseholdAccountBook.ViewModels.Windows;
using HouseholdAccountBook.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace HouseholdAccountBook.ViewModels.WindowsParts
{
    /// <summary>
    /// メインウィンドウ リストタブVM
    /// </summary>
    /// <param name="parent">親VM</param>
    /// <param name="tab">タブ</param>
    public class MainWindowListTabViewModel : WindowPartViewModelBase
    {
        /// <summary>
        /// <see cref="SeriesViewModel"/> のキー情報
        /// </summary>
        /// <param name="BalanceKind">終始種別</param>
        /// <param name="CategoryId">分類ID</param>
        /// <param name="ItemId">項目ID</param>
        public record Keys(BalanceKind? BalanceKind, CategoryIdObj CategoryId, ItemIdObj ItemId) { }

        private MainWindowViewModel Parent { get; }

        private Tabs Tab { get; }

        #region イベント
        /// <summary>
        /// 系列選択変更時イベント
        /// </summary>
        public event EventHandler SelectedSeriesChanged;
        #endregion

        #region Bindingプロパティ
        /// <summary>
        /// 系列セレクタVM
        /// </summary>
        public SelectorViewModel<SeriesViewModel, Keys> SeriesSelectorVM { get; } = new(static vm => vm != null ? new(vm.Category.BalanceKind, vm.Category.Id, vm.Item.Id) : null);
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

            this.SeriesSelectorVM.SetLoader(async _ => {
                SeriesViewModelLoader loader = new(new(this.mDbHandlerFactory));
                return this.Tab switch {
                    Tabs.MonthlyListTab => await loader.LoadMonthlySeriesViewModelListWithinYearAsync(this.Parent.BookSelectorVM.SelectedKey, this.Parent.DisplayedYear, this.Parent.FiscalStartMonth),
                    Tabs.YearlyListTab => await loader.LoadYearlySeriesViewModelListWithinDecadeAsync(this.Parent.BookSelectorVM.SelectedKey, this.Parent.DisplayedStartYear, this.Parent.FiscalStartMonth),
                    _ => throw new NotImplementedException(),
                };
            });
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

            await this.SeriesSelectorVM.LoadAsync(new Keys(balanceKind, categoryId, itemId), SelectorMode.FirstOrDefault);
        }

        public override void Initialize(WaitCursorManagerFactory waitCursorManagerFactory, DbHandlerFactory dbHandlerFactory)
        {
            base.Initialize(waitCursorManagerFactory, dbHandlerFactory);

            this.SeriesSelectorVM.WaitCursorManagerFactory = waitCursorManagerFactory;
        }

        public override void AddEventHandlers()
        {
            this.SeriesSelectorVM.SelectionChanged += (sender, e) => {
                this.Parent.SelectedBalanceKind = e.NewValue?.BalanceKind;
                this.Parent.SelectedCategoryId = e.NewValue?.CategoryId;
                this.Parent.SelectedItemId = e.NewValue?.ItemId;

                this.SelectedSeriesChanged?.Invoke(sender, EventArgs.Empty);
            };
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

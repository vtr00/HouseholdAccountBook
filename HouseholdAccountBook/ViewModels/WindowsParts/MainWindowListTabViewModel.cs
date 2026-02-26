using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.Infrastructure;
using HouseholdAccountBook.Models.Infrastructure.Logger;
using HouseholdAccountBook.Models.Utilities.Args.RequestEventArgs;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using HouseholdAccountBook.ViewModels.Windows;
using HouseholdAccountBook.Views.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace HouseholdAccountBook.ViewModels.WindowsParts
{
    /// <summary>
    /// メインウィンドウ リストタブVM
    /// </summary>
    /// <param name="parent">親VM</param>
    /// <param name="tab">タブ</param>
    public class MainWindowListTabViewModel(MainWindowViewModel parent, Tabs tab) : WindowPartViewModelBase
    {
        private MainWindowViewModel Parent { get; } = parent;

        private Tabs Tab { get; } = tab;

        #region イベント
        /// <summary>
        /// 系列選択変更時イベント
        /// </summary>
        public event EventHandler SelectedSeriesChanged;
        #endregion

        #region プロパティ
        /// <summary>
        /// 系列VMリスト
        /// </summary>
        #region SeriesVMList
        public ObservableCollection<SeriesViewModel> SeriesVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 選択された系列VM
        /// </summary>
        #region SelectedSeriesVM
        public SeriesViewModel SelectedSeriesVM {
            get;
            set {
                var oldVM = field;
                if (this.SetProperty(ref field, value)) {
                    this.Parent.SelectedBalanceKind = value?.BalanceKind;
                    this.Parent.SelectedCategoryId = value?.Category.Id;
                    this.Parent.SelectedItemId = value?.Item.Id;

                    this.SelectedSeriesChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        #endregion
        #endregion

        public override async Task LoadAsync() => await this.LoadAsync(null, null, null);

        /// <summary>
        /// リストタブに表示するデータを読み込む
        /// </summary>
        /// <param name="balanceKind">収支種別</param>
        /// <param name="categoryId">分類ID</param>
        /// <param name="itemId">項目ID</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task LoadAsync(int? balanceKind = null, int? categoryId = null, int? itemId = null)
        {
            if (this.Parent.SelectedTab != this.Tab) { return; }

            using FuncLog funcLog = new(new { balanceKind, categoryId, itemId });

            int? tmpBalanceKind = balanceKind ?? this.Parent.SelectedBalanceKind;
            int? tmpCategoryId = categoryId ?? this.Parent.SelectedCategoryId;
            int? tmpItemId = itemId ?? this.Parent.SelectedItemId;
            Log.Vars(vars: new { tmpBalanceKind, tmpCategoryId, tmpItemId });

            ViewModelService service = new(this.mDbHandlerFactory);
            this.SeriesVMList = this.Tab switch {
                Tabs.MonthlyListTab => await service.LoadMonthlySeriesViewModelListWithinYearAsync(this.Parent.SelectedBookVM?.Id, this.Parent.DisplayedYear, this.Parent.FiscalStartMonth),
                Tabs.YearlyListTab => await service.LoadYearlySeriesViewModelListWithinDecadeAsync(this.Parent.SelectedBookVM?.Id, this.Parent.DisplayedStartYear, this.Parent.FiscalStartMonth),
                _ => throw new NotImplementedException(),
            };
            this.SelectedSeriesVM = this.SeriesVMList?.FirstOrDefault(vm => vm.BalanceKind == tmpBalanceKind && vm.Category.Id == tmpCategoryId && vm.Item.Id == tmpItemId);
        }

        public override void AddEventHandlers()
        {
            // NOP
        }

        /// <summary>
        /// DataGridの情報をCSVファイルにエクスポートする
        /// </summary>
        /// <param name="columnInfos">列情報</param>
        public async Task ExportCSVFileAsync(IEnumerable<DataGridExtensions.ColumnInfo> columnInfos)
        {
            Properties.Settings settings = Properties.Settings.Default;
            (string folderPath, string fileName) = PathUtil.GetSeparatedPath(settings.App_ExportCsvFilePath, App.GetCurrentDir());

            SaveFileDialogRequestEventArgs e = new() {
                InitialDirectory = folderPath,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = Properties.Resources.FileSelectFilter_CsvFile + "|*.csv"
            };
            if (this.SaveFileDialogRequest(e)) {
                // 選択したCSVファイルのパスを設定として保存する
                settings.App_ExportCsvFilePath = e.FileName;
                settings.Save();

                await CSVFileDao.SaveDataGridDataAsync(e.FileName, columnInfos, this.SeriesVMList);
            }

            bool result = true;
            _ = result
                ? MessageBox.Show(Properties.Resources.Message_FinishToExport, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK)
                : MessageBox.Show(Properties.Resources.Message_FoultToExport, Properties.Resources.Title_Conformation, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        }
    }
}

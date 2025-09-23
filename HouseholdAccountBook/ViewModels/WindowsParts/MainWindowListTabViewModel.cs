using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Models.Logger;
using HouseholdAccountBook.Models.Services;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using HouseholdAccountBook.ViewModels.Windows;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace HouseholdAccountBook.ViewModels.WindowsParts
{
    public class MainWindowListTabViewModel(MainWindowViewModel parent, Tabs tab) : WindowPartViewModelBase
    {
        private MainWindowViewModel Parent { get; } = parent;

        private Tabs Tab { get; } = tab;

        #region イベント
        /// <summary>
        /// 系列選択変更時イベント
        /// </summary>
        public event Action SelectedSeriesChanged = default;
        #endregion

        #region プロパティ
        /// <summary>
        /// 系列VMリスト
        /// </summary>
        #region SeriesVMList
        public ObservableCollection<SeriesViewModel> SeriesVMList
        {
            get => this._SeriesVMList;
            set => this.SetProperty(ref this._SeriesVMList, value);
        }
        private ObservableCollection<SeriesViewModel> _SeriesVMList = default;
        #endregion

        /// <summary>
        /// 選択された系列VM
        /// </summary>
        #region SelectedSeriesVM
        public SeriesViewModel SelectedSeriesVM
        {
            get => this._SelectedSeriesVM;
            set {
                if (this.SetProperty(ref this._SelectedSeriesVM, value)) {
                    this.Parent.SelectedBalanceKind = value?.BalanceKind;
                    this.Parent.SelectedCategoryId = value?.CategoryId;
                    this.Parent.SelectedItemId = value?.ItemId;

                    this.SelectedSeriesChanged?.Invoke();
                }
            }
        }
        private SeriesViewModel _SelectedSeriesVM = default;
        #endregion
        #endregion

        public async Task UpdateListTabDataAsync(int? balanceKind = null, int? categoryId = null, int? itemId = null)
        {
            if (this.Parent.SelectedTab != this.Tab) return;

            Log.Info($"balanceKind:{balanceKind} categoryId:{categoryId} itemId:{itemId}");

            int? tmpBalanceKind = balanceKind ?? this.Parent.SelectedBalanceKind;
            int? tmpCategoryId = categoryId ?? this.Parent.SelectedCategoryId;
            int? tmpItemId = itemId ?? this.Parent.SelectedItemId;
            Log.Info($"tmpBalanceKind:{tmpBalanceKind} tmpCategoryId:{tmpCategoryId} tmpItemId:{tmpItemId}");

            var loader = new ViewModelLoader(this.dbHandlerFactory);
            this.SeriesVMList = this.Tab switch {
                Tabs.MonthlyListTab => await loader.LoadMonthlySeriesViewModelListWithinYearAsync(this.Parent.SelectedBookVM?.Id, this.Parent.DisplayedYear, this.Parent.FiscalStartMonth),
                Tabs.YearlyListTab => await loader.LoadYearlySeriesViewModelListWithinDecadeAsync(this.Parent.SelectedBookVM?.Id, this.Parent.DisplayedStartYear, this.Parent.FiscalStartMonth),
                _ => throw new NotImplementedException(),
            };
            this.SelectedSeriesVM = this.SeriesVMList?.FirstOrDefault((vm) => vm.BalanceKind == tmpBalanceKind && vm.CategoryId == tmpCategoryId && vm.ItemId == tmpItemId);
        }
    }
}

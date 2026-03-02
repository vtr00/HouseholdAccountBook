using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities.Extensions;
using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Settings;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 設定VMローダー
    /// </summary>
    /// <param name="service">アプリサービス</param>
    public class SettingViewModelLoader(AppService service)
    {
        /// <summary>
        /// アプリサービス
        /// </summary>
        private readonly AppService mService = service;

        /// <summary>
        /// 帳簿設定VMを取得する
        /// </summary>
        /// <param name="bookId">表示対象の帳簿ID</param>
        /// <returns>帳簿設定VM</returns>
        public async Task<BookSettingViewModel> LoadBookSettingViewModelAsync(BookIdObj bookId)
        {
            using FuncLog funcLog = new(new { bookId });

            BookSettingViewModel vm = null;

            List<BookModel> bmList = await this.mService.LoadBookListAsync(Properties.Resources.ListName_None);

            BookModel bm = await this.mService.LoadBookAsync(bookId);

            vm = new BookSettingViewModel(bm) {
                DebitBookVMList = new ObservableCollection<BookModel>(bmList.Where(tmpVM => tmpVM.Id != bookId)),
                RelationVMList = [.. await this.mService.LoadRelationAsync(bookId)]
            };
            vm.SelectedDebitBookVM = vm.DebitBookVMList.FirstOrElementAtOrDefault(tmpVM => (int?)tmpVM.Id == bm.DebitBookId, 0);

            return vm;
        }

        /// <summary>
        /// 項目ツリーVMリストを取得する
        /// </summary>
        /// <returns>階層構造項目VMリスト</returns>
        public async Task<ObservableCollection<ItemTreeViewModel>> LoadItemTreeVMListAsync()
        {
            using FuncLog funcLog = new();

            ObservableCollection<ItemTreeViewModel> vmList = [
                new (BalanceKind.Income),
                new (BalanceKind.Expenses)
            ];

            foreach (ItemTreeViewModel balanceVM in vmList) {
                // 分類
                (await this.mService.LoadCategoryListAsync((BalanceKind)balanceVM.Id.Value))
                    .ForEach(cm => balanceVM.ChildrenVMList.Add(new ItemTreeViewModel(cm) { ParentVM = balanceVM, ChildrenVMList = [] }));

                foreach (ItemTreeViewModel categoryVM in balanceVM.ChildrenVMList) {
                    // 項目
                    (await this.mService.LoadItemListAsync(categoryVM.Id.Value))
                        .ForEach(im => categoryVM.ChildrenVMList.Add(new ItemTreeViewModel(im) { ParentVM = categoryVM }));
                }
            }

            return vmList;
        }

        /// <summary>
        /// 分類/項目設定VMを取得する
        /// </summary>
        /// <param name="kind">表示対象の階層種別</param>
        /// <param name="id">表示対象のID</param>
        /// <returns>分類/項目設定VM</returns>
        public async Task<ItemSettingViewModel> LoadItemSettingVMAsync(HierarchicalKind kind, IdObj id)
        {
            using FuncLog funcLog = new(new { kind, id });

            ItemSettingViewModel vm = null;

            switch (kind) {
                case HierarchicalKind.Balance: {
                    // 収支
                    vm = new();
                    break;
                }
                case HierarchicalKind.Category: {
                    // 分類
                    vm = new(await this.mService.LoadCategoryAsync(id.Value));
                    break;
                }
                case HierarchicalKind.Item: {
                    // 項目
                    vm = new(await this.mService.LoadItemAsync(id.Value));
                    break;
                }
            }

            return vm;
        }
    }
}

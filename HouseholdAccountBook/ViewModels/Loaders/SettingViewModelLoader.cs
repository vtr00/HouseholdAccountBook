using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Component;
using HouseholdAccountBook.ViewModels.Settings;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace HouseholdAccountBook.ViewModels.Loaders
{
    /// <summary>
    /// 設定VMローダー
    /// </summary>
    /// <param name="appService">アプリサービス</param>
    /// <param name="settingService">設定サービス</param>
    public class SettingViewModelLoader(AppCommonService appService, MasterSettingService settingService)
    {
        /// <summary>
        /// アプリサービス
        /// </summary>
        private readonly AppCommonService mAppService = appService;
        /// <summary>
        /// 設定サービス
        /// </summary>
        private readonly MasterSettingService mService = settingService;

        /// <summary>
        /// 帳簿設定VMを取得する
        /// </summary>
        /// <param name="accountId">表示対象の帳簿ID</param>
        /// <returns>帳簿設定VM</returns>
        public async Task<AccountSettingViewModel> LoadAccountSettingViewModelAsync(AccountIdObj accountId)
        {
            using FuncLog funcLog = new(new { accountId });

            AccountSettingViewModel vm = null;

            IEnumerable<AccountModel> bmList = await this.mAppService.LoadAccountListAsync(initialName: Properties.Resources.ListName_None);
            AccountModel am = await this.mService.LoadAccountAsync(accountId);

            vm = new(am);
            vm.AccountKindSelectorVM.SetLoader(() => UiConstants.AccountKindStr);
            await vm.AccountKindSelectorVM.LoadAsync(am.AccountKind);
            vm.DebitAccountSelectorVM.SetLoader(() => bmList.Where(tmpVM => tmpVM.Id != accountId));
            await vm.DebitAccountSelectorVM.LoadAsync(am.DebitAccountId);
            vm.TextEncodingSelectorVM.SetLoader(() => EncodingUtil.GetTextEncodingList());
            await vm.TextEncodingSelectorVM.LoadAsync(am.TextEncoding);
            vm.RelationSelectorVM.SetLoader(async () => (await this.mService.LoadRelationListAsync(accountId)).Select(model => new RelationViewModel(model)), mode: SelectorMode.FirstOrDefault);
            await vm.RelationSelectorVM.LoadAsync();

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
                new List<CategoryModel>(await this.mService.LoadCategoryListAsync(EnumUtil.SafeCastEnum(balanceVM.Id.Id, BalanceKind.Income)))
                    .ForEach(cm => balanceVM.ChildrenVMList.Add(new ItemTreeViewModel(cm) { ParentVM = balanceVM, ChildrenVMList = [] }));

                foreach (ItemTreeViewModel categoryVM in balanceVM.ChildrenVMList) {
                    // 項目
                    new List<ItemModel>(await this.mService.LoadItemListAsync(categoryVM.Id.Id))
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
                    CategoryIdObj categoryId = id.Id;
                    vm = new(await this.mService.LoadCategoryAsync(categoryId));
                    break;
                }
                case HierarchicalKind.Item: {
                    // 項目
                    ItemIdObj itemId = id.Id;
                    vm = new(await this.mService.LoadItemAsync(itemId));
                    vm.RelationSelectorVM.SetLoader(async () => (await this.mService.LoadRelationListAsync(itemId)).Select(model => new RelationViewModel(model)), mode: SelectorMode.FirstOrDefault);
                    await vm.RelationSelectorVM.LoadAsync();
                    vm.ShopSelectorVM.SetLoader(async () => await this.mAppService.LoadShopListAsync(itemId), mode: SelectorMode.FirstOrDefault);
                    await vm.ShopSelectorVM.LoadAsync();
                    vm.RemarkSelectorVM.SetLoader(async () => await this.mAppService.LoadRemarkListAsync(itemId), mode: SelectorMode.FirstOrDefault);
                    await vm.RemarkSelectorVM.LoadAsync();
                    break;
                }
            }

            return vm;
        }
    }
}

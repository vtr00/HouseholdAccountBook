using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities.Args;
using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using HouseholdAccountBook.ViewModels.Loaders;
using HouseholdAccountBook.ViewModels.Settings;
using HouseholdAccountBook.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels.WindowsParts
{
    /// <summary>
    /// 分類/項目設定タブVM
    /// </summary>
    public class SettingsWindowItemTabViewModel : WindowPartViewModelBase
    {
        #region フィールド
        /// <summary>
        /// 設定サービス
        /// </summary>
        private SettingService mSettingService;
        #endregion

        #region イベント
        /// <summary>
        /// 選択された項目ツリーVM変更時イベント
        /// </summary>
        public event EventHandler<EventArgs<ItemTreeViewModel>> SelectedItemTreeVMChanged;

        /// <summary>
        /// 更新必要時イベント
        /// </summary>
        public event EventHandler NeedToUpdateChanged;
        #endregion

        #region Bindingプロパティ
        /// <summary>
        /// 項目ツリーVMリスト
        /// </summary>
        public ObservableCollection<ItemTreeViewModel> ItemTreeVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        /// <summary>
        /// 選択された項目ツリーVM
        /// </summary>
        public ItemTreeViewModel SelectedItemTreeVM {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    this.SelectedItemTreeVMChanged?.Invoke(this, new EventArgs<ItemTreeViewModel>(value));
                }
            }
        }
        /// <summary>
        /// 表示された分類/項目設定VM
        /// </summary>
        public ItemSettingViewModel DisplayedItemSettingVM {
            get;
            set => this.SetProperty(ref field, value);
        }

        #region コマンド
        /// <summary>
        /// 分類追加コマンド
        /// </summary>
        public ICommand AddCategoryCommand => new AsyncRelayCommand(this.AddCategoryCommand_ExecuteAsync, this.AddCategoryCommand_CanExecute);
        /// <summary>
        /// 項目追加コマンド
        /// </summary>
        public ICommand AddItemCommand => new AsyncRelayCommand(this.AddItemCommand_ExecuteAsync, this.AddItemCommand_CanExecute);
        /// <summary>
        /// 分類/項目削除コマンド
        /// </summary>
        public ICommand DeleteItemCommand => new AsyncRelayCommand(this.DeleteItemCommand_ExecuteAsync, this.DeleteItemCommand_CanExecute);
        /// <summary>
        /// 分類/項目表示順上昇コマンド
        /// </summary>
        public ICommand RaiseItemSortOrderCommand => new AsyncRelayCommand(this.RaiseItemSortOrderCommand_ExecuteAsync, this.RaiseItemSortOrderCommand_CanExecute);
        /// <summary>
        /// 分類/項目表示順下降コマンド
        /// </summary>
        public ICommand DropItemSortOrderCommand => new AsyncRelayCommand(this.DropItemSortOrderCommand_ExecuteAsync, this.DropItemSortOrderCommand_CanExecute);
        /// <summary>
        /// 分類/項目情報保存コマンド
        /// </summary>
        public ICommand SaveItemInfoCommand => new AsyncRelayCommand(this.SaveItemInfoCommand_ExecuteAsync, this.SaveItemInfoCommand_CanExecute);
        /// <summary>
        /// 項目-帳簿関係変更コマンド
        /// </summary>
        public ICommand ChangeItemRelationCommand => new AsyncRelayCommand<object>(this.ChangeItemRelationCommand_ExecuteAsync);
        /// <summary>
        /// 店名削除コマンド
        /// </summary>
        public ICommand DeleteShopNameCommand => new AsyncRelayCommand(this.DeleteShopNameCommand_ExecuteAsync, this.DeleteShopNameCommand_CanExecute);
        /// <summary>
        /// 備考削除コマンド
        /// </summary>
        public ICommand DeleteRemarkCommand => new AsyncRelayCommand(this.DeleteRemarkCommand_ExecuteAsync, this.DeleteRemarkCommand_CanExecute);
        #endregion
        #endregion

        #region コマンドイベントハンドラ
        /// <summary>
        /// 分類追加コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool AddCategoryCommand_CanExecute() => this.SelectedItemTreeVM != null;

        /// <summary>
        /// 分類追加コマンド処理
        /// </summary>
        private async Task AddCategoryCommand_ExecuteAsync()
        {
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                ItemTreeViewModel vm = this.SelectedItemTreeVM;
                while (vm?.Kind != HierarchicalKind.Balance) {
                    vm = vm.ParentVM;
                }
                CategoryIdObj categoryId = await this.mSettingService.AddCategoryAsync((BalanceKind)vm.Id.Value);

                await this.LoadAsync(HierarchicalKind.Category, categoryId);
                this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 項目追加コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool AddItemCommand_CanExecute() => this.SelectedItemTreeVM != null && this.SelectedItemTreeVM.Kind != HierarchicalKind.Balance;

        /// <summary>
        /// 項目追加コマンド処理
        /// </summary>
        private async Task AddItemCommand_ExecuteAsync()
        {
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                ItemTreeViewModel vm = this.SelectedItemTreeVM;
                while (vm?.Kind != HierarchicalKind.Category) {
                    vm = vm.ParentVM;
                }

                ItemIdObj itemId = await this.mSettingService.AddItemAsync(vm.Id.Value);

                await this.LoadAsync(HierarchicalKind.Item, itemId);
                this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 分類/項目削除コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool DeleteItemCommand_CanExecute() => this.SelectedItemTreeVM != null && this.SelectedItemTreeVM.Kind != HierarchicalKind.Balance;

        /// <summary>
        /// 分類/項目削除コマンド処理
        /// </summary>
        private async Task DeleteItemCommand_ExecuteAsync()
        {
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                HierarchicalKind? kind = this.SelectedItemTreeVM?.Kind;
                IdObj id = this.SelectedItemTreeVM.Id;

                bool result = false;
                switch (kind) {
                    case HierarchicalKind.Category: {
                        result = await this.mSettingService.DeleteCategoryAsync((int)id);
                    }
                    break;
                    case HierarchicalKind.Item: {
                        result = await this.mSettingService.DeleteItemAsync((int)id);
                    }
                    break;
                }
                if (result) {
                    await this.LoadAsync(this.SelectedItemTreeVM.ParentVM?.Kind, this.SelectedItemTreeVM.ParentVM.Id);
                    this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
                }
                else {
                    switch (kind) {
                        case HierarchicalKind.Category: {
                            _ = MessageBox.Show(Properties.Resources.Message_CantDeleteBecauseActionItemExistsInCategory, Properties.Resources.Title_Error);
                            break;
                        }
                        case HierarchicalKind.Item: {
                            _ = MessageBox.Show(Properties.Resources.Message_CantDeleteBecauseActionItemExistsInItem, Properties.Resources.Title_Error);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 分類/項目表示順上昇コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool RaiseItemSortOrderCommand_CanExecute()
        {
            bool canExecute = this.SelectedItemTreeVM != null && this.SelectedItemTreeVM.ParentVM != null;
            if (canExecute) {
                // 同じ階層で、よりソート順序が上の分類/項目がある場合trueになる
                var parentVM = this.SelectedItemTreeVM.ParentVM;
                int index = parentVM.ChildrenVMList.IndexOf(this.SelectedItemTreeVM);
                canExecute = 0 < index;

                // 選択された対象が項目で分類内の最も上位にいる場合
                if (!canExecute && parentVM.ParentVM != null) {
                    // 項目の属する分類について、同じ階層内によりソート順序が上の分類がある場合trueになる
                    var grandparentVM = parentVM.ParentVM;
                    int index2 = grandparentVM.ChildrenVMList.IndexOf(parentVM);
                    canExecute = 0 < index2;
                }
            }
            return canExecute;
        }

        /// <summary>
        /// 分類/項目表示順上昇コマンド処理
        /// </summary>
        private async Task RaiseItemSortOrderCommand_ExecuteAsync()
        {
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                var parentVM = this.SelectedItemTreeVM.ParentVM;
                int index = parentVM.ChildrenVMList.IndexOf(this.SelectedItemTreeVM);
                IdObj changingId = parentVM.ChildrenVMList[index].Id; // 選択中の項目のID
                if (0 < index) {
                    IdObj changedId = parentVM.ChildrenVMList[index - 1].Id; // 入れ替え対象の項目のID

                    switch (this.SelectedItemTreeVM?.Kind) {
                        case HierarchicalKind.Category: {
                            await this.mSettingService.SwapCategorySortOrderAsync((int)changingId, (int)changedId);
                        }
                        break;
                        case HierarchicalKind.Item: {
                            await this.mSettingService.SwapItemSortOrderAsync((int)changingId, (int)changedId);
                        }
                        break;
                    }
                }
                else { // 分類を跨いで項目の表示順を変更するとき
                    Debug.Assert(this.SelectedItemTreeVM?.Kind == HierarchicalKind.Item);
                    var grandparentVM = parentVM.ParentVM;
                    int index2 = grandparentVM.ChildrenVMList.IndexOf(parentVM);
                    CategoryIdObj toCategoryId = new((int)grandparentVM.ChildrenVMList[index2 - 1].Id);

                    await this.mSettingService.UpdateItemSortOrderToMaximumAsync(toCategoryId, changingId.Value);
                }

                await this.LoadAsync(this.SelectedItemTreeVM?.Kind, this.SelectedItemTreeVM.Id);
                this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 分類/項目表示順下降コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool DropItemSortOrderCommand_CanExecute()
        {
            bool canExecute = this.SelectedItemTreeVM != null && this.SelectedItemTreeVM.ParentVM != null;
            if (canExecute) {
                // 同じ階層で、よりソート順序が下の分類/項目がある場合trueになる
                var parentVM = this.SelectedItemTreeVM.ParentVM;
                int index = parentVM.ChildrenVMList.IndexOf(this.SelectedItemTreeVM);
                canExecute = parentVM.ChildrenVMList.Count - 1 > index;

                // 選択された対象が項目で分類内の最も上位にいる場合
                if (!canExecute && parentVM.ParentVM != null) {
                    // 項目の属する分類について、同じ階層内によりソート順序が下の分類がある場合trueになる
                    var grandparentVM = parentVM.ParentVM;
                    int index2 = grandparentVM.ChildrenVMList.IndexOf(parentVM);
                    canExecute = grandparentVM.ChildrenVMList.Count - 1 > index2;
                }
            }
            return canExecute;
        }

        /// <summary>
        /// 分類/項目表示順下降コマンド処理
        /// </summary>
        private async Task DropItemSortOrderCommand_ExecuteAsync()
        {
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                var parentVM = this.SelectedItemTreeVM.ParentVM;
                int index = parentVM.ChildrenVMList.IndexOf(this.SelectedItemTreeVM);
                IdObj changingId = parentVM.ChildrenVMList[index].Id; // 選択中の項目のID
                if (parentVM.ChildrenVMList.Count - 1 > index) {
                    IdObj changedId = parentVM.ChildrenVMList[index + 1].Id; // 入れ替え対象の項目のID

                    switch (this.SelectedItemTreeVM?.Kind) {
                        case HierarchicalKind.Category: {
                            await this.mSettingService.SwapCategorySortOrderAsync((int)changingId, (int)changedId);
                        }
                        break;
                        case HierarchicalKind.Item: {
                            await this.mSettingService.SwapItemSortOrderAsync((int)changingId, (int)changedId);
                        }
                        break;
                    }
                }
                else { // 分類を跨いで項目の表示順を変更するとき
                    Debug.Assert(this.SelectedItemTreeVM?.Kind == HierarchicalKind.Item);
                    var grandparentVM = parentVM.ParentVM;
                    int index2 = grandparentVM.ChildrenVMList.IndexOf(parentVM);
                    CategoryIdObj toCategoryId = new((int)grandparentVM.ChildrenVMList[index2 + 1].Id);

                    await this.mSettingService.UpdateItemSortOrderToMinimumAsync(toCategoryId, changingId.Value);
                }

                await this.LoadAsync(this.SelectedItemTreeVM?.Kind, this.SelectedItemTreeVM.Id);
                this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 分類/項目情報保存コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool SaveItemInfoCommand_CanExecute()
        {
            return this.SelectedItemTreeVM != null && this.SelectedItemTreeVM.Kind != HierarchicalKind.Balance &&
                   !string.IsNullOrWhiteSpace(this.SelectedItemTreeVM.Name);
        }

        /// <summary>
        /// 分類/項目情報保存コマンド処理
        /// </summary>
        private async Task SaveItemInfoCommand_ExecuteAsync()
        {
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                ItemSettingViewModel vm = this.DisplayedItemSettingVM;

                switch (vm.Kind) {
                    case HierarchicalKind.Category: {
                        await this.mSettingService.SaveCategoryAsync(new(vm.Id.Value, vm.InputedName));
                    }
                    break;
                    case HierarchicalKind.Item: {
                        await this.mSettingService.SaveItemAsync(new(vm.Id.Value, vm.InputedName));
                    }
                    break;
                }

                await this.LoadAsync(vm.Kind, vm.Id);
                _ = MessageBox.Show(Properties.Resources.Message_FinishToSave, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information);
                this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 項目-帳簿関係変更コマンド処理
        /// </summary>
        /// <param name="viewModel">チェック対象に紐づくVM</param>
        private async Task ChangeItemRelationCommand_ExecuteAsync(object viewModel)
        {
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                Debug.Assert(this.DisplayedItemSettingVM.Kind == HierarchicalKind.Item);

                ItemSettingViewModel vm = this.DisplayedItemSettingVM;
                vm.RelationSelectorVM.SelectedItem = viewModel as RelationViewModel; // チェックボックスを変更しただけでは変更されないため、引数で受け取る

                if (await this.mSettingService.SaveBookItemRemationAsync(vm.RelationSelectorVM.SelectedKey, (int)vm.Id, vm.RelationSelectorVM.SelectedItem.IsRelated)) {
                    this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
                }
                else {
                    vm.RelationSelectorVM.SelectedItem.IsRelated = !vm.RelationSelectorVM.SelectedItem.IsRelated; // 選択前の状態に戻す
                    _ = MessageBox.Show(Properties.Resources.Message_CantDeleteBecauseActionItemExistsInItemWithinBook, Properties.Resources.Title_Error);
                }
            }
        }

        /// <summary>
        /// 店名削除コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool DeleteShopNameCommand_CanExecute() => this.DisplayedItemSettingVM?.ShopSelectorVM.SelectedItem != null;

        /// <summary>
        /// 店名削除コマンド処理
        /// </summary>
        private async Task DeleteShopNameCommand_ExecuteAsync()
        {
            if (MessageBox.Show(Properties.Resources.Message_DeleteNotification, Properties.Resources.Title_Information,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                    Debug.Assert(this.DisplayedItemSettingVM.Kind == HierarchicalKind.Item);

                    await this.mSettingService.DeleteShopAsync(this.DisplayedItemSettingVM.ShopSelectorVM.SelectedKey, this.DisplayedItemSettingVM.Id.Value);

                    SettingViewModelLoader loader = new(new(this.mDbHandlerFactory), new(this.mDbHandlerFactory));
                    this.DisplayedItemSettingVM = await loader.LoadItemSettingVMAsync(HierarchicalKind.Item, this.DisplayedItemSettingVM.Id);
                }
            }
        }

        /// <summary>
        /// 備考削除コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool DeleteRemarkCommand_CanExecute() => this.DisplayedItemSettingVM?.RemarkSelectorVM.SelectedItem != null;

        /// <summary>
        /// 備考削除コマンド処理
        /// </summary>
        private async Task DeleteRemarkCommand_ExecuteAsync()
        {
            if (MessageBox.Show(Properties.Resources.Message_DeleteNotification, Properties.Resources.Title_Information,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                    Debug.Assert(this.DisplayedItemSettingVM.Kind == HierarchicalKind.Item);

                    await this.mSettingService.DeleteRemarkAsync(this.DisplayedItemSettingVM.RemarkSelectorVM.SelectedKey, this.DisplayedItemSettingVM.Id.Value);

                    SettingViewModelLoader loader = new(new(this.mDbHandlerFactory), new(this.mDbHandlerFactory));
                    this.DisplayedItemSettingVM = await loader.LoadItemSettingVMAsync(HierarchicalKind.Item, this.DisplayedItemSettingVM.Id);
                }
            }
        }
        #endregion

        public override void Initialize(WaitCursorManagerFactory waitCursorManagerFactory, DbHandlerFactory dbHandlerFactory)
        {
            base.Initialize(waitCursorManagerFactory, dbHandlerFactory);

            this.mSettingService = new(this.mDbHandlerFactory);
        }

        public override async Task LoadAsync() => await this.LoadAsync(null, null);

        /// <summary>
        /// 項目設定タブに表示するデータを読み込む
        /// </summary>
        /// <param name="kind">選択対象の階層種別</param>
        /// <param name="id">選択対象のID</param>
        public async Task LoadAsync(HierarchicalKind? kind = null, IdObj id = null)
        {
            using FuncLog funcLog = new(new { kind, id });

            // InitializeComponent内で呼ばれる場合があるため、nullチェックを行う
            if (this.mDbHandlerFactory == null) { return; }

            // 指定がなければ現在選択中の項目を再選択する
            if (this.SelectedItemTreeVM != null && kind == null && id == null) {
                kind = this.SelectedItemTreeVM.Kind;
                id = this.SelectedItemTreeVM.Id;
            }

            SettingViewModelLoader loader = new(new(this.mDbHandlerFactory), new(this.mDbHandlerFactory));
            this.ItemTreeVMList = await loader.LoadItemTreeVMListAsync();

            // 選択する項目を探す
            ItemTreeViewModel selectedVM = null;
            if (kind != null && id != null) {
                // 収支から探す
                IEnumerable<ItemTreeViewModel> query = this.ItemTreeVMList.Where(vm => vm.Kind == kind && vm.Id == id);

                if (!query.Any()) {
                    // 分類から探す
                    foreach (ItemTreeViewModel tmpVM in this.ItemTreeVMList) {
                        query = tmpVM.ChildrenVMList.Where(vm => vm.Kind == kind && vm.Id == id);
                        if (query.Any()) { break; }
                    }
                }

                if (!query.Any()) {
                    // 項目から探す
                    foreach (ItemTreeViewModel tmpVM in this.ItemTreeVMList) {
                        foreach (ItemTreeViewModel tmpVM2 in tmpVM.ChildrenVMList) {
                            query = tmpVM2.ChildrenVMList.Where(vm => vm.Kind == kind && vm.Id == id);
                            if (query.Any()) { break; }
                        }
                        if (query.Any()) { break; }
                    }
                }

                selectedVM = query.FirstOrDefault();
            }

            // 何も選択されていないなら1番上の項目を選択する
            if (selectedVM == null && this.ItemTreeVMList.Any()) {
                selectedVM = this.ItemTreeVMList[0];
            }
            this.SelectedItemTreeVM = selectedVM;
        }

        public override void AddEventHandlers()
        {
            using FuncLog funcLog = new();

            this.SelectedItemTreeVMChanged += async (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.SelectedItemTreeVMChanged));

                if (e.Value != null) {
                    SettingViewModelLoader loader = new(new(this.mDbHandlerFactory), new(this.mDbHandlerFactory));
                    this.DisplayedItemSettingVM = await loader.LoadItemSettingVMAsync(e.Value.Kind, e.Value.Id);
                }
                else {
                    this.DisplayedItemSettingVM = null;
                }
            };
        }
    }
}

using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Models.Dao.Compositions;
using HouseholdAccountBook.Models.Dao.DbTable;
using HouseholdAccountBook.Models.DbHandler;
using HouseholdAccountBook.Models.DbHandler.Abstract;
using HouseholdAccountBook.Models.Dto.DbTable;
using HouseholdAccountBook.Models.Dto.Others;
using HouseholdAccountBook.Models.Logger;
using HouseholdAccountBook.Others;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using HouseholdAccountBook.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static HouseholdAccountBook.ViewModels.Settings.HierarchicalSettingViewModel;

namespace HouseholdAccountBook.ViewModels.WindowsParts
{
    /// <summary>
    /// 分類/項目設定タブVM
    /// </summary>
    public class SettingsWindowItemTabViewModel : WindowPartViewModelBase
    {
        #region イベント
        /// <summary>
        /// 選択された項目VM変更時イベント
        /// </summary>
        public event EventHandler<EventArgs<HierarchicalViewModel>> SelectedHierarchicalVMChanged;

        /// <summary>
        /// 更新必要時イベント
        /// </summary>
        public event EventHandler NeedToUpdateChanged;
        #endregion

        #region Bindingプロパティ
        /// <summary>
        /// 階層構造設定VMリスト
        /// </summary>
        #region HierarchicalVMList
        public ObservableCollection<HierarchicalViewModel> HierarchicalVMList
        {
            get => this._HierarchicalVMList;
            set => this.SetProperty(ref this._HierarchicalVMList, value);
        }
        private ObservableCollection<HierarchicalViewModel> _HierarchicalVMList = default;
        #endregion
        /// <summary>
        /// 選択された階層構造設定VM
        /// </summary>
        #region SelectedHierarchicalVM
        public HierarchicalViewModel SelectedHierarchicalVM
        {
            get => this._SelectedHierarchicalVM;
            set {
                if (this.SetProperty(ref this._SelectedHierarchicalVM, value)) {
                    this.SelectedHierarchicalVMChanged?.Invoke(this, new EventArgs<HierarchicalViewModel>(value));
                }
            }
        }
        private HierarchicalViewModel _SelectedHierarchicalVM = default;
        #endregion
        /// <summary>
        /// 表示された階層構造設定VM
        /// </summary>
        #region DisplayedHierarchicalSettingVM
        public HierarchicalSettingViewModel DisplayedHierarchicalSettingVM
        {
            get => this._DisplayedHierarchicalSettingVM;
            set => this.SetProperty(ref this._DisplayedHierarchicalSettingVM, value);
        }
        private HierarchicalSettingViewModel _DisplayedHierarchicalSettingVM = default;
        #endregion

        /// <summary>
        /// 分類追加コマンド
        /// </summary>
        public ICommand AddCategoryCommand => new RelayCommand(this.AddCategoryCommand_Executed, this.AddCategoryCommand_CanExecute);
        /// <summary>
        /// 項目追加コマンド
        /// </summary>
        public ICommand AddItemCommand => new RelayCommand(this.AddItemCommand_Executed, this.AddItemCommand_CanExecute);
        /// <summary>
        /// 分類/項目削除コマンド
        /// </summary>
        public ICommand DeleteItemCommand => new RelayCommand(this.DeleteItemCommand_Executed, this.DeleteItemCommand_CanExecute);
        /// <summary>
        /// 分類/項目表示順上昇コマンド
        /// </summary>
        public ICommand RaiseItemSortOrderCommand => new RelayCommand(this.RaiseItemSortOrderCommand_Executed, this.RaiseItemSortOrderCommand_CanExecute);
        /// <summary>
        /// 分類/項目表示順下降コマンド
        /// </summary>
        public ICommand DropItemSortOrderCommand => new RelayCommand(this.DropItemSortOrderCommand_Executed, this.DropItemSortOrderCommand_CanExecute);
        /// <summary>
        /// 分類/項目情報保存コマンド
        /// </summary>
        public ICommand SaveItemInfoCommand => new RelayCommand(this.SaveItemInfoCommand_Executed, this.SaveItemInfoCommand_CanExecute);
        /// <summary>
        /// 項目-帳簿関係変更コマンド
        /// </summary>
        public ICommand ChangeItemRelationCommand => new RelayCommand<object>(this.ChangeItemRelationCommand_Executed);
        /// <summary>
        /// 店名削除コマンド
        /// </summary>
        public ICommand DeleteShopNameCommand => new RelayCommand(this.DeleteShopNameCommand_Executed, this.DeleteShopNameCommand_CanExecute);
        /// <summary>
        /// 備考削除コマンド
        /// </summary>
        public ICommand DeleteRemarkCommand => new RelayCommand(this.DeleteRemarkCommand_Executed, this.DeleteRemarkCommand_CanExecute);
        #endregion

        #region コマンドイベントハンドラ
        /// <summary>
        /// 分類追加コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool AddCategoryCommand_CanExecute() => this.SelectedHierarchicalVM != null;

        /// <summary>
        /// 分類追加コマンド処理
        /// </summary>
        private async void AddCategoryCommand_Executed()
        {
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                HierarchicalViewModel vm = this.SelectedHierarchicalVM;
                while (GetHierarchicalKind(vm) != HierarchicalKind.Balance) {
                    vm = vm.ParentVM;
                }
                BalanceKind kind = (BalanceKind)vm.Id;

                int categoryId = -1;
                await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                    MstCategoryDao dao = new(dbHandler);
                    categoryId = await dao.InsertReturningIdAsync(new MstCategoryDto { BalanceKind = (int)kind });
                }
                await this.LoadAsync(HierarchicalKind.Category, categoryId);
                this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 項目追加コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool AddItemCommand_CanExecute() => this.SelectedHierarchicalVM != null && GetHierarchicalKind(this.SelectedHierarchicalVM) != HierarchicalKind.Balance;

        /// <summary>
        /// 項目追加コマンド処理
        /// </summary>
        private async void AddItemCommand_Executed()
        {
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                HierarchicalViewModel vm = this.SelectedHierarchicalVM;
                while (GetHierarchicalKind(vm) != HierarchicalKind.Category) {
                    vm = vm.ParentVM;
                }
                int categoryId = vm.Id;

                int itemId = -1;
                await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                    MstItemDao mstItemDao = new(dbHandler);
                    itemId = await mstItemDao.InsertReturningIdAsync(new MstItemDto { CategoryId = categoryId });
                }
                await this.LoadAsync(HierarchicalKind.Item, itemId);
                this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 分類/項目削除コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool DeleteItemCommand_CanExecute() => this.SelectedHierarchicalVM != null && GetHierarchicalKind(this.SelectedHierarchicalVM) != HierarchicalKind.Balance;

        /// <summary>
        /// 分類/項目削除コマンド処理
        /// </summary>
        private async void DeleteItemCommand_Executed()
        {
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                HierarchicalKind? kind = GetHierarchicalKind(this.SelectedHierarchicalVM);
                int id = this.SelectedHierarchicalVM.Id;

                await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                    switch (kind) {
                        case HierarchicalKind.Category: {
                            await dbHandler.ExecTransactionAsync(async () => {
                                HstActionWithHstItemDao hstActionWithHstItemDao = new(dbHandler);

                                var dtoList = await hstActionWithHstItemDao.FindByCategoryIdAsync(id);

                                if (dtoList.Any()) {
                                    _ = MessageBox.Show(Properties.Resources.Message_CantDeleteBecauseActionItemExistsInCategory, Properties.Resources.Title_Error);
                                    return;
                                }

                                MstCategoryDao mstCategoryDao = new(dbHandler);
                                _ = await mstCategoryDao.DeleteByIdAsync(id);
                            });
                        }
                        break;
                        case HierarchicalKind.Item: {
                            await dbHandler.ExecTransactionAsync(async () => {
                                HstActionDao hstActionDao = new(dbHandler);
                                var dtoList = await hstActionDao.FindByItemIdAsync(id);

                                if (dtoList.Any()) {
                                    _ = MessageBox.Show(Properties.Resources.Message_CantDeleteBecauseActionItemExistsInItem, Properties.Resources.Title_Error);
                                    return;
                                }

                                MstItemDao mstItemDao = new(dbHandler);
                                _ = await mstItemDao.DeleteByIdAsync(id);
                            });
                        }
                        break;
                    }
                }
                await this.LoadAsync(GetHierarchicalKind(this.SelectedHierarchicalVM.ParentVM), this.SelectedHierarchicalVM.ParentVM.Id);
                this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 分類/項目表示順上昇コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool RaiseItemSortOrderCommand_CanExecute()
        {
            bool canExecute = this.SelectedHierarchicalVM != null && this.SelectedHierarchicalVM.ParentVM != null;
            if (canExecute) {
                // 同じ階層で、よりソート順序が上の分類/項目がある場合trueになる
                var parentVM = this.SelectedHierarchicalVM.ParentVM;
                int index = parentVM.ChildrenVMList.IndexOf(this.SelectedHierarchicalVM);
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
        private async void RaiseItemSortOrderCommand_Executed()
        {
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                var parentVM = this.SelectedHierarchicalVM.ParentVM;
                int index = parentVM.ChildrenVMList.IndexOf(this.SelectedHierarchicalVM);
                int changingId = parentVM.ChildrenVMList[index].Id; // 選択中の項目のID
                if (0 < index) {
                    int changedId = parentVM.ChildrenVMList[index - 1].Id; // 入れ替え対象の項目のID

                    await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                        switch (GetHierarchicalKind(this.SelectedHierarchicalVM)) {
                            case HierarchicalKind.Category: {
                                MstCategoryDao mstCategoryDao = new(dbHandler);
                                _ = await mstCategoryDao.SwapSortOrderAsync(changingId, changedId);
                            }
                            break;
                            case HierarchicalKind.Item: {
                                MstItemDao mstItemDao = new(dbHandler);
                                _ = await mstItemDao.SwapSortOrderAsync(changingId, changedId);
                            }
                            break;
                        }
                    }
                }
                else { // 分類を跨いで項目の表示順を変更するとき
                    Debug.Assert(GetHierarchicalKind(this.SelectedHierarchicalVM) == HierarchicalKind.Item);
                    var grandparentVM = parentVM.ParentVM;
                    int index2 = grandparentVM.ChildrenVMList.IndexOf(parentVM);
                    int toCategoryId = grandparentVM.ChildrenVMList[index2 - 1].Id;

                    await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                        MstItemDao mstItemDao = new(dbHandler);
                        _ = await mstItemDao.UpdateSortOrderToMaximumAsync(toCategoryId, changingId);
                    }
                }

                await this.LoadAsync(GetHierarchicalKind(this.SelectedHierarchicalVM), this.SelectedHierarchicalVM.Id);
                this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 分類/項目表示順下降コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool DropItemSortOrderCommand_CanExecute()
        {
            bool canExecute = this.SelectedHierarchicalVM != null && this.SelectedHierarchicalVM.ParentVM != null;
            if (canExecute) {
                // 同じ階層で、よりソート順序が下の分類/項目がある場合trueになる
                var parentVM = this.SelectedHierarchicalVM.ParentVM;
                int index = parentVM.ChildrenVMList.IndexOf(this.SelectedHierarchicalVM);
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
        private async void DropItemSortOrderCommand_Executed()
        {
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                var parentVM = this.SelectedHierarchicalVM.ParentVM;
                int index = parentVM.ChildrenVMList.IndexOf(this.SelectedHierarchicalVM);
                int changingId = parentVM.ChildrenVMList[index].Id; // 選択中の項目のID
                if (parentVM.ChildrenVMList.Count - 1 > index) {
                    int changedId = parentVM.ChildrenVMList[index + 1].Id; // 入れ替え対象の項目のID

                    await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                        switch (GetHierarchicalKind(this.SelectedHierarchicalVM)) {
                            case HierarchicalKind.Category: {
                                MstCategoryDao mstCategoryDao = new(dbHandler);
                                _ = await mstCategoryDao.SwapSortOrderAsync(changingId, changedId);
                            }
                            break;
                            case HierarchicalKind.Item: {
                                MstItemDao mstItemDao = new(dbHandler);
                                _ = await mstItemDao.SwapSortOrderAsync(changingId, changedId);
                            }
                            break;
                        }
                    }
                }
                else { // 分類を跨いで項目の表示順を変更するとき
                    Debug.Assert(GetHierarchicalKind(this.SelectedHierarchicalVM) == HierarchicalKind.Item);
                    var grandparentVM = parentVM.ParentVM;
                    int index2 = grandparentVM.ChildrenVMList.IndexOf(parentVM);
                    int toCategoryId = grandparentVM.ChildrenVMList[index2 + 1].Id;

                    await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                        MstItemDao mstItemDao = new(dbHandler);
                        _ = await mstItemDao.UpdateSortOrderToMinimumAsync(toCategoryId, changingId);
                    }
                }

                await this.LoadAsync(GetHierarchicalKind(this.SelectedHierarchicalVM), this.SelectedHierarchicalVM.Id);
                this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 分類/項目情報保存コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool SaveItemInfoCommand_CanExecute()
        {
            return this.SelectedHierarchicalVM != null && GetHierarchicalKind(this.SelectedHierarchicalVM) != HierarchicalKind.Balance &&
                   !string.IsNullOrWhiteSpace(this.SelectedHierarchicalVM.Name);
        }

        /// <summary>
        /// 分類/項目情報保存コマンド処理
        /// </summary>
        private async void SaveItemInfoCommand_Executed()
        {
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                HierarchicalSettingViewModel vm = this.DisplayedHierarchicalSettingVM;

                await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                    switch (vm.Kind) {
                        case HierarchicalKind.Category: {
                            MstCategoryDao mstCategoryDao = new(dbHandler);
                            _ = await mstCategoryDao.UpdateSetableAsync(new MstCategoryDto { CategoryName = vm.Name, CategoryId = vm.Id });
                        }
                        break;
                        case HierarchicalKind.Item: {
                            MstItemDao mstItemDao = new(dbHandler);
                            _ = await mstItemDao.UpdateSetableAsync(new MstItemDto { ItemName = vm.Name, ItemId = vm.Id });
                        }
                        break;
                    }
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
        private async void ChangeItemRelationCommand_Executed(object viewModel)
        {
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                Debug.Assert(this.DisplayedHierarchicalSettingVM.Kind == HierarchicalKind.Item);

                HierarchicalSettingViewModel vm = this.DisplayedHierarchicalSettingVM;
                vm.SelectedRelationVM = viewModel as RelationViewModel; // チェックボックスを変更しただけでは変更されないため、引数で受け取る

                await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                    HstActionDao hstActionDao = new(dbHandler);
                    var actionDtoList = await hstActionDao.FindByBookIdAndItemIdAsync(vm.SelectedRelationVM.Id, vm.Id);

                    if (actionDtoList.Any()) {
                        vm.SelectedRelationVM.IsRelated = !vm.SelectedRelationVM.IsRelated; // 選択前の状態に戻す
                        _ = MessageBox.Show(Properties.Resources.Message_CantDeleteBecauseActionItemExistsInItemWithinBook, Properties.Resources.Title_Error);
                        return;
                    }

                    await dbHandler.ExecTransactionAsync(async () => {
                        RelBookItemDao relBookItemDao = new(dbHandler);
                        var dtoList = await relBookItemDao.UpsertAsync(new RelBookItemDto {
                            BookId = vm.SelectedRelationVM.Id,
                            ItemId = vm.Id,
                            DelFlg = vm.SelectedRelationVM.IsRelated ? 0 : 1
                        });
                    });
                }
                this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 店名削除コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool DeleteShopNameCommand_CanExecute() => this.DisplayedHierarchicalSettingVM?.SelectedShopVM != null;

        /// <summary>
        /// 店名削除コマンド処理
        /// </summary>
        private async void DeleteShopNameCommand_Executed()
        {
            if (MessageBox.Show(Properties.Resources.Message_DeleteNotification, Properties.Resources.Title_Information,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                    Debug.Assert(this.DisplayedHierarchicalSettingVM.Kind == HierarchicalKind.Item);

                    await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                        HstShopDao hstShopDao = new(dbHandler);
                        _ = await hstShopDao.DeleteAsync(new HstShopDto {
                            ShopName = this.DisplayedHierarchicalSettingVM.SelectedShopVM.Name,
                            ItemId = this.DisplayedHierarchicalSettingVM.Id
                        });

                        // 店舗名を更新する
                        this.DisplayedHierarchicalSettingVM.ShopVMList = await LoadShopViewModelListAsync(dbHandler, this.DisplayedHierarchicalSettingVM.Id);
                    }
                }
            }
        }

        /// <summary>
        /// 備考削除コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool DeleteRemarkCommand_CanExecute() => this.DisplayedHierarchicalSettingVM?.SelectedRemarkVM != null;

        /// <summary>
        /// 備考削除コマンド処理
        /// </summary>
        private async void DeleteRemarkCommand_Executed()
        {
            if (MessageBox.Show(Properties.Resources.Message_DeleteNotification, Properties.Resources.Title_Information,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                    Debug.Assert(this.DisplayedHierarchicalSettingVM.Kind == HierarchicalKind.Item);

                    await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                        HstRemarkDao hstRemarkDao = new(dbHandler);
                        _ = await hstRemarkDao.DeleteAsync(new HstRemarkDto {
                            Remark = this.DisplayedHierarchicalSettingVM.SelectedRemarkVM.Remark,
                            ItemId = this.DisplayedHierarchicalSettingVM.Id
                        });

                        // 備考欄の表示を更新する
                        this.DisplayedHierarchicalSettingVM.RemarkVMList = await LoadRemarkViewModelListAsync(dbHandler, this.DisplayedHierarchicalSettingVM.Id);
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// ViewModelの初期化を行う
        /// </summary>
        /// <param name="waitCursorManagerFactory">WaitCursorマネージャファクトリ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        public override void Initialize(WaitCursorManagerFactory waitCursorManagerFactory, DbHandlerFactory dbHandlerFactory)
        {
            base.Initialize(waitCursorManagerFactory, dbHandlerFactory);
        }

        public override async Task LoadAsync()
        {
            await this.LoadAsync(null, null);
        }

        /// <summary>
        /// 項目設定タブに表示するデータを更新する
        /// </summary>
        /// <param name="kind">選択対象の階層種別</param>
        /// <param name="id">選択対象のID</param>
        public async Task LoadAsync(HierarchicalKind? kind = null, int? id = null)
        {
            Log.Info();

            // InitializeComponent内で呼ばれる場合があるため、nullチェックを行う
            if (this.dbHandlerFactory == null) {
                return;
            }

            // 指定がなければ現在選択中の項目を再選択する
            if (this.SelectedHierarchicalVM != null && kind == null && id == null) {
                kind = GetHierarchicalKind(this.SelectedHierarchicalVM);
                id = this.SelectedHierarchicalVM.Id;
            }

            this.HierarchicalVMList = await LoadHierarchicalViewModelListAsync(this.dbHandlerFactory);

            // 選択する項目を探す
            HierarchicalViewModel selectedVM = null;
            if (kind != null && id != null) {
                // 収支から探す
                IEnumerable<HierarchicalViewModel> query = this.HierarchicalVMList.Where((vm) => GetHierarchicalKind(vm) == kind && vm.Id == id);

                if (!query.Any()) {
                    // 分類から探す
                    foreach (HierarchicalViewModel tmpVM in this.HierarchicalVMList) {
                        query = tmpVM.ChildrenVMList.Where((vm) => GetHierarchicalKind(vm) == kind && vm.Id == id);
                        if (query.Any()) { break; }
                    }
                }

                if (!query.Any()) {
                    // 項目から探す
                    foreach (HierarchicalViewModel tmpVM in this.HierarchicalVMList) {
                        foreach (HierarchicalViewModel tmpVM2 in tmpVM.ChildrenVMList) {
                            query = tmpVM2.ChildrenVMList.Where((vm) => GetHierarchicalKind(vm) == kind && vm.Id == id);
                            if (query.Any()) { break; }
                        }
                        if (query.Any()) { break; }
                    }
                }

                selectedVM = query.Any() ? query.First() : null;
            }

            // 何も選択されていないなら1番上の項目を選択する
            if (selectedVM == null && this.HierarchicalVMList.Any()) {
                selectedVM = this.HierarchicalVMList[0];
            }
            this.SelectedHierarchicalVM = selectedVM;

            this.AddEventHandlers();
        }

        protected override void AddEventHandlers()
        {
            this.SelectedHierarchicalVMChanged += async (sender, e) => {
                if (e.Value != null) {
                    await this.UpdateInputDataAsync(GetHierarchicalKind(e.Value).Value, e.Value.Id);
                }
                else {
                    this.DisplayedHierarchicalSettingVM = null;
                }
            };
        }

        /// <summary>
        /// 階層構造項目VMリストを取得する
        /// </summary>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <returns>階層構造項目VMリスト</returns>
        private static async Task<ObservableCollection<HierarchicalViewModel>> LoadHierarchicalViewModelListAsync(DbHandlerFactory dbHandlerFactory)
        {
            Log.Info();

            ObservableCollection<HierarchicalViewModel> vmList = [
                new () {
                    Depth = (int)HierarchicalKind.Balance,
                    Id = (int)BalanceKind.Income,
                    SortOrder = -1,
                    Name = Properties.Resources.BalanceKind_Income,
                    ParentVM = null,
                    ChildrenVMList = []
                },
                new () {
                    Depth = (int)HierarchicalKind.Balance,
                    Id = (int)BalanceKind.Expenses,
                    SortOrder = -1,
                    Name = Properties.Resources.BalanceKind_Expenses,
                    ParentVM = null,
                    ChildrenVMList = []
                }
            ];

            await using (DbHandlerBase dbHandler = await dbHandlerFactory.CreateAsync()) {
                foreach (HierarchicalViewModel vm in vmList) {
                    // 分類
                    MstCategoryDao mstCategoryDao = new(dbHandler);
                    var cDtoList = await mstCategoryDao.FindByBalanceKindAsync(vm.Id);

                    foreach (MstCategoryDto dto in cDtoList) {
                        vm.ChildrenVMList.Add(new HierarchicalViewModel() {
                            Depth = (int)HierarchicalKind.Category,
                            Id = dto.CategoryId,
                            SortOrder = dto.SortOrder,
                            Name = dto.CategoryName,
                            ParentVM = vm,
                            ChildrenVMList = []
                        });
                    }

                    // 項目
                    MstItemDao mstItemDao = new(dbHandler);
                    foreach (HierarchicalViewModel categoryVM in vm.ChildrenVMList) {
                        var iDtoList = await mstItemDao.FindByCategoryIdAsync(categoryVM.Id);

                        foreach (MstItemDto dto in iDtoList) {
                            categoryVM.ChildrenVMList.Add(new HierarchicalViewModel() {
                                Depth = (int)HierarchicalKind.Item,
                                Id = dto.ItemId,
                                SortOrder = dto.SortOrder,
                                Name = dto.ItemName,
                                ParentVM = categoryVM
                            });
                        }
                    }
                }
            }

            return vmList;
        }

        /// <summary>
        /// 項目設定タブの入力欄に表示するデータを更新する
        /// </summary>
        /// <param name="kind">表示対象の階層種別</param>
        /// <param name="id">表示対象のID</param>
        /// <returns></returns>
        private async Task UpdateInputDataAsync(HierarchicalKind kind, int id)
        {
            HierarchicalSettingViewModel vm = null;

            switch (kind) {
                case HierarchicalKind.Balance: {
                    vm = new HierarchicalSettingViewModel() {
                        Kind = HierarchicalKind.Balance,
                        Id = this.SelectedHierarchicalVM.Id,
                        Name = this.SelectedHierarchicalVM.Name
                    };
                    break;
                }
                case HierarchicalKind.Category: {
                    // 分類
                    await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                        MstCategoryDao mstCategoryDao = new(dbHandler);
                        var dto = await mstCategoryDao.FindByIdAsync(id);

                        vm = new HierarchicalSettingViewModel() {
                            Kind = HierarchicalKind.Category,
                            Id = id,
                            SortOrder = dto.SortOrder,
                            Name = dto.CategoryName
                        };
                    }
                    break;
                }
                case HierarchicalKind.Item: {
                    // 項目
                    await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                        MstItemDao mstItemDao = new(dbHandler);
                        var dto = await mstItemDao.FindByIdAsync(id);

                        vm = new HierarchicalSettingViewModel {
                            Kind = HierarchicalKind.Item,
                            Id = id,
                            SortOrder = dto.SortOrder,
                            Name = dto.ItemName,
                            RelationVMList = await LoadRelationViewModelListAsync(dbHandler, id),
                            ShopVMList = await LoadShopViewModelListAsync(dbHandler, id),
                            RemarkVMList = await LoadRemarkViewModelListAsync(dbHandler, id)
                        };
                    }
                    break;
                }
            }

            this.DisplayedHierarchicalSettingVM = vm;
        }

        /// <summary>
        /// 関連VMリスト(項目主体)を取得する
        /// </summary>
        /// <param name="dbHandler">DBハンドラ</param>
        /// <param name="itemId">項目ID</param>
        /// <returns>関連VMリスト</returns>
        private static async Task<ObservableCollection<RelationViewModel>> LoadRelationViewModelListAsync(DbHandlerBase dbHandler, int itemId)
        {
            BookRelFromItemInfoDao bookRelFromItemInfoDao = new(dbHandler);
            var dtoList = await bookRelFromItemInfoDao.FindByItemIdAsync(itemId);

            ObservableCollection<RelationViewModel> rvmList = [];
            foreach (BookRelFromItemInfoDto dto in dtoList) {
                RelationViewModel rvm = new() {
                    Id = dto.BookId,
                    Name = dto.BookName,
                    IsRelated = dto.IsRelated
                };
                rvmList.Add(rvm);
            }
            return rvmList;
        }

        /// <summary>
        /// 店舗VMリストを取得する
        /// </summary>
        /// <param name="dbHandler">DBハンドラ</param>
        /// <param name="itemId">項目ID</param>
        /// <returns>店舗VMリスト</returns>
        private static async Task<ObservableCollection<ShopViewModel>> LoadShopViewModelListAsync(DbHandlerBase dbHandler, int itemId)
        {
            ObservableCollection<ShopViewModel> svmList = [];
            ShopInfoDao shopInfoDao = new(dbHandler);
            var dtoList = await shopInfoDao.FindByItemIdAsync(itemId);

            foreach (ShopInfoDto dto in dtoList) {
                ShopViewModel svm = new() {
                    Name = dto.ShopName,
                    UsedCount = dto.Count,
                    UsedTime = dto.UsedTime
                };
                svmList.Add(svm);
            }
            return svmList;
        }

        /// <summary>
        /// 備考VMリストを取得する
        /// </summary>
        /// <param name="dbHandler">DBハンドラ</param>
        /// <param name="itemId">項目ID</param>
        /// <returns>備考VMリスト</returns>
        private static async Task<ObservableCollection<RemarkViewModel>> LoadRemarkViewModelListAsync(DbHandlerBase dbHandler, int itemId)
        {
            ObservableCollection<RemarkViewModel> rvmList = [];
            RemarkInfoDao remarkInfoDao = new(dbHandler);
            var dtoList = await remarkInfoDao.FindByItemIdAsync(itemId);

            foreach (RemarkInfoDto dto in dtoList) {
                RemarkViewModel rvm = new() {
                    Remark = dto.Remark,
                    UsedCount = dto.Count,
                    UsedTime = dto.UsedTime
                };
                rvmList.Add(rvm);
            }
            return rvmList;
        }
    }
}

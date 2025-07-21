using HouseholdAccountBook.Dao.Compositions;
using HouseholdAccountBook.Dao.DbTable;
using HouseholdAccountBook.DbHandler;
using HouseholdAccountBook.DbHandler.Abstract;
using HouseholdAccountBook.Dto.DbTable;
using HouseholdAccountBook.Dto.Others;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.ViewModels;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static HouseholdAccountBook.ConstValue.ConstValue;
using static HouseholdAccountBook.Extensions.FrameworkElementExtensions;
using static HouseholdAccountBook.ViewModels.HierarchicalSettingViewModel;

namespace HouseholdAccountBook.Windows
{
    /// <summary>
    /// SettingsWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingsWindow : Window
    {
        #region フィールド
        /// <summary>
        /// DBハンドラファクトリ
        /// </summary>
        private readonly DbHandlerFactory dbHandlerFactory;
        /// <summary>
        /// 前回選択していた設定タブ
        /// </summary>
        private SettingsTabs oldSelectedSettingsTab = SettingsTabs.ItemSettingsTab;
        /// <summary>
        /// 表示更新の必要があるか
        /// </summary>
        private bool needToUpdate = false;
        #endregion

        /// <summary>
        /// <see cref="SettingsWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        public SettingsWindow(DbHandlerFactory dbHandlerFactory)
        {
            this.dbHandlerFactory = dbHandlerFactory;

            this.InitializeComponent();
        }

        #region イベントハンドラ
        #region コマンド
        #region ウィンドウ
        /// <summary>
        /// ウィンドウを閉じる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region 項目設定の操作
        /// <summary>
        /// 分類を追加可能か判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddCategoryCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedHierarchicalVM != null;
        }

        /// <summary>
        /// 分類を追加する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddCategoryCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                HierarchicalViewModel vm = this.WVM.SelectedHierarchicalVM;
                while (HierarchicalSettingViewModel.GetHierarchicalKind(vm) != HierarchicalKind.Balance) {
                    vm = vm.ParentVM;
                }
                BalanceKind kind = (BalanceKind)vm.Id;

                int categoryId = -1;
                using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                    MstCategoryDao dao = new MstCategoryDao(dbHandler);
                    categoryId = await dao.InsertReturningIdAsync(new MstCategoryDto { BalanceKind = (int)kind });
                }
                await this.UpdateItemSettingsTabDataAsync(HierarchicalKind.Category, categoryId);
                this.needToUpdate = true;
            }
        }

        /// <summary>
        /// 項目を追加可能か判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedHierarchicalVM != null && HierarchicalSettingViewModel.GetHierarchicalKind(this.WVM.SelectedHierarchicalVM) != HierarchicalKind.Balance;
        }

        /// <summary>
        /// 項目を追加する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                HierarchicalViewModel vm = this.WVM.SelectedHierarchicalVM;
                while (HierarchicalSettingViewModel.GetHierarchicalKind(vm) != HierarchicalKind.Category) {
                    vm = vm.ParentVM;
                }
                int categoryId = vm.Id;

                int itemId = -1;
                using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                    MstItemDao mstItemDao = new MstItemDao(dbHandler);
                    _ = await mstItemDao.InsertReturningIdAsync(new MstItemDto { CategoryId = categoryId });
                }
                await this.UpdateItemSettingsTabDataAsync(HierarchicalKind.Item, itemId);
                this.needToUpdate = true;
            }
        }

        /// <summary>
        /// 分類/項目を削除可能か判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedHierarchicalVM != null && HierarchicalSettingViewModel.GetHierarchicalKind(this.WVM.SelectedHierarchicalVM) != HierarchicalKind.Balance;
        }

        /// <summary>
        /// 分類/項目を削除する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DeleteItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                HierarchicalKind? kind = HierarchicalSettingViewModel.GetHierarchicalKind(this.WVM.SelectedHierarchicalVM);
                int id = this.WVM.SelectedHierarchicalVM.Id;

                using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                    switch (kind) {
                        case HierarchicalKind.Category: {
                            await dbHandler.ExecTransactionAsync(async () => {
                                HstActionWithHstItemDao hstActionWithHstItemDao = new HstActionWithHstItemDao(dbHandler);

                                var dtoList = await hstActionWithHstItemDao.FindByCategoryIdAsync(id);

                                if (dtoList.Count() != 0) {
                                    MessageBox.Show(Properties.Resources.Message_CantDeleteBecauseActionItemExistsInCategory, Properties.Resources.Title_Error);
                                    return;
                                }

                                MstCategoryDao mstCategoryDao = new MstCategoryDao(dbHandler);
                                _ = await mstCategoryDao.DeleteByIdAsync(id);
                            });
                        }
                        break;
                        case HierarchicalKind.Item: {
                            await dbHandler.ExecTransactionAsync(async () => {
                                HstActionDao hstActionDao = new HstActionDao(dbHandler);
                                var dtoList = await hstActionDao.FindByItemIdAsync(id);

                                if (dtoList.Count() != 0) {
                                    MessageBox.Show(Properties.Resources.Message_CantDeleteBecauseActionItemExistsInItem, Properties.Resources.Title_Error);
                                    return;
                                }

                                MstItemDao mstItemDao = new MstItemDao(dbHandler);
                                _ = await mstItemDao.DeleteByIdAsync(id);
                            });
                        }
                        break;
                    }
                }
                await this.UpdateItemSettingsTabDataAsync(HierarchicalSettingViewModel.GetHierarchicalKind(this.WVM.SelectedHierarchicalVM.ParentVM), this.WVM.SelectedHierarchicalVM.ParentVM.Id);
                this.needToUpdate = true;
            }
        }

        /// <summary>
        /// 分類/項目の表示順を上げられるか判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RaiseItemSortOrderCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedHierarchicalVM != null && this.WVM.SelectedHierarchicalVM.ParentVM != null;
            if (e.CanExecute) {
                // 同じ階層で、よりソート順序が上の分類/項目がある場合trueになる
                var parentVM = this.WVM.SelectedHierarchicalVM.ParentVM;
                int index = parentVM.ChildrenVMList.IndexOf(this.WVM.SelectedHierarchicalVM);
                e.CanExecute = 0 < index;

                // 選択された対象が項目で分類内の最も上位にいる場合
                if (!e.CanExecute && parentVM.ParentVM != null) {
                    // 項目の属する分類について、同じ階層内によりソート順序が上の分類がある場合trueになる
                    var grandparentVM = parentVM.ParentVM;
                    int index2 = grandparentVM.ChildrenVMList.IndexOf(parentVM);
                    e.CanExecute = 0 < index2;
                }
            }
        }

        /// <summary>
        /// 分類/項目の表示順を上げる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RaiseItemSortOrderCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                var parentVM = this.WVM.SelectedHierarchicalVM.ParentVM;
                int index = parentVM.ChildrenVMList.IndexOf(this.WVM.SelectedHierarchicalVM);
                int changingId = parentVM.ChildrenVMList[index].Id; // 選択中の項目のID
                if (0 < index) {
                    int changedId = parentVM.ChildrenVMList[index - 1].Id; // 入れ替え対象の項目のID

                    using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                        switch (HierarchicalSettingViewModel.GetHierarchicalKind(this.WVM.SelectedHierarchicalVM)) {
                            case HierarchicalKind.Category: {
                                MstCategoryDao mstCategoryDao = new MstCategoryDao(dbHandler);
                                await mstCategoryDao.SwapSortOrderAsync(changingId, changedId);
                            }
                            break;
                            case HierarchicalKind.Item: {
                                MstItemDao mstItemDao = new MstItemDao(dbHandler);
                                await mstItemDao.SwapSortOrderAsync(changingId, changedId);
                            }
                            break;
                        }
                    }
                }
                else { // 分類を跨いで項目の表示順を変更するとき
                    Debug.Assert(HierarchicalSettingViewModel.GetHierarchicalKind(this.WVM.SelectedHierarchicalVM) == HierarchicalKind.Item);
                    var grandparentVM = parentVM.ParentVM;
                    int index2 = grandparentVM.ChildrenVMList.IndexOf(parentVM);
                    int toCategoryId = grandparentVM.ChildrenVMList[index2 - 1].Id;

                    using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                        MstItemDao mstItemDao = new MstItemDao(dbHandler);
                        _ = await mstItemDao.UpdateSortOrderToMaximumAsync(toCategoryId, changingId);
                    }
                }

                await this.UpdateItemSettingsTabDataAsync(HierarchicalSettingViewModel.GetHierarchicalKind(this.WVM.SelectedHierarchicalVM), this.WVM.SelectedHierarchicalVM.Id);
                this.needToUpdate = true;
            }
        }

        /// <summary>
        /// 分類/項目の表示順を下げられるか判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DropItemSortOrderCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedHierarchicalVM != null && this.WVM.SelectedHierarchicalVM.ParentVM != null;
            if (e.CanExecute) {
                // 同じ階層で、よりソート順序が下の分類/項目がある場合trueになる
                var parentVM = this.WVM.SelectedHierarchicalVM.ParentVM;
                int index = parentVM.ChildrenVMList.IndexOf(this.WVM.SelectedHierarchicalVM);
                e.CanExecute = parentVM.ChildrenVMList.Count - 1 > index;

                // 選択された対象が項目で分類内の最も上位にいる場合
                if (!e.CanExecute && parentVM.ParentVM != null) {
                    // 項目の属する分類について、同じ階層内によりソート順序が下の分類がある場合trueになる
                    var grandparentVM = parentVM.ParentVM;
                    int index2 = grandparentVM.ChildrenVMList.IndexOf(parentVM);
                    e.CanExecute = grandparentVM.ChildrenVMList.Count - 1 > index2;
                }
            }
        }

        /// <summary>
        /// 分類/項目の表示順を下げる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DropItemSortOrderCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                var parentVM = this.WVM.SelectedHierarchicalVM.ParentVM;
                int index = parentVM.ChildrenVMList.IndexOf(this.WVM.SelectedHierarchicalVM);
                int changingId = parentVM.ChildrenVMList[index].Id; // 選択中の項目のID
                if (parentVM.ChildrenVMList.Count - 1 > index) {
                    int changedId = parentVM.ChildrenVMList[index + 1].Id; // 入れ替え対象の項目のID

                    using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                        switch (HierarchicalSettingViewModel.GetHierarchicalKind(this.WVM.SelectedHierarchicalVM)) {
                            case HierarchicalKind.Category: {
                                MstCategoryDao mstCategoryDao = new MstCategoryDao(dbHandler);
                                await mstCategoryDao.SwapSortOrderAsync(changingId, changedId);
                            }
                            break;
                            case HierarchicalKind.Item: {
                                MstItemDao mstItemDao = new MstItemDao(dbHandler);
                                await mstItemDao.SwapSortOrderAsync(changingId, changedId);
                            }
                            break;
                        }
                    }
                }
                else { // 分類を跨いで項目の表示順を変更するとき
                    Debug.Assert(HierarchicalSettingViewModel.GetHierarchicalKind(this.WVM.SelectedHierarchicalVM) == HierarchicalKind.Item);
                    var grandparentVM = parentVM.ParentVM;
                    int index2 = grandparentVM.ChildrenVMList.IndexOf(parentVM);
                    int toCategoryId = grandparentVM.ChildrenVMList[index2 + 1].Id;

                    using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                        MstItemDao mstItemDao = new MstItemDao(dbHandler);
                        _ = await mstItemDao.UpdateSortOrderToMinimumAsync(toCategoryId, changingId);
                    }
                }

                await this.UpdateItemSettingsTabDataAsync(HierarchicalSettingViewModel.GetHierarchicalKind(this.WVM.SelectedHierarchicalVM), this.WVM.SelectedHierarchicalVM.Id);
                this.needToUpdate = true;
            }
        }

        /// <summary>
        /// 項目の情報を保存できるか判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveItemInfoCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedHierarchicalVM != null && HierarchicalSettingViewModel.GetHierarchicalKind(this.WVM.SelectedHierarchicalVM) != HierarchicalKind.Balance &&
                !string.IsNullOrWhiteSpace(this.WVM.SelectedHierarchicalVM.Name);
        }

        /// <summary>
        /// 項目の情報を保存する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SaveItemInfoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                HierarchicalSettingViewModel vm = this.WVM.DisplayedHierarchicalSettingVM;

                using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                    switch (vm.Kind) {
                        case HierarchicalKind.Category: {
                            MstCategoryDao mstCategoryDao = new MstCategoryDao(dbHandler);
                            _ = await mstCategoryDao.UpdateSetableAsync(new MstCategoryDto { CategoryName = vm.Name, CategoryId = vm.Id });
                        }
                        break;
                        case HierarchicalKind.Item: {
                            MstItemDao mstItemDao = new MstItemDao(dbHandler);
                            _ = await mstItemDao.UpdateSetableAsync(new MstItemDto { ItemName = vm.Name, ItemId = vm.Id });
                        }
                        break;
                    }
                }

                await this.UpdateItemSettingsTabDataAsync(vm.Kind, vm.Id);
                MessageBox.Show(Properties.Resources.Message_FinishToSave, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information);
                this.needToUpdate = true;
            }
        }

        /// <summary>
        /// 項目-帳簿の関係を変更する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ChangeItemRelationCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                Debug.Assert(this.WVM.DisplayedHierarchicalSettingVM.Kind == HierarchicalKind.Item);

                HierarchicalSettingViewModel vm = this.WVM.DisplayedHierarchicalSettingVM;
                vm.SelectedRelationVM = (e.OriginalSource as CheckBox)?.DataContext as RelationViewModel;

                using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                    HstActionDao hstActionDao = new HstActionDao(dbHandler);
                    var actionDtoList = await hstActionDao.FindByBookIdAndItemIdAsync(vm.SelectedRelationVM.Id, vm.Id);

                    if (actionDtoList.Count() != 0) {
                        vm.SelectedRelationVM.IsRelated = !vm.SelectedRelationVM.IsRelated; // 選択前の状態に戻す
                        MessageBox.Show(Properties.Resources.Message_CantDeleteBecauseActionItemExistsInItemWithinBook, Properties.Resources.Title_Error);
                        e.Handled = true;
                        return;
                    }

                    await dbHandler.ExecTransactionAsync(async () => {
                        RelBookItemDao relBookItemDao = new RelBookItemDao(dbHandler);
                        var dtoList = await relBookItemDao.UpsertAsync(new RelBookItemDto {
                            BookId = vm.SelectedRelationVM.Id,
                            ItemId = vm.Id,
                            DelFlg = vm.SelectedRelationVM.IsRelated ? 0 : 1
                        });
                    });
                }
                this.needToUpdate = true;
            }
        }

        /// <summary>
        /// 店名を削除できるか判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteShopNameCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.DisplayedHierarchicalSettingVM?.SelectedShopVM != null;
        }

        /// <summary>
        /// 店名を削除する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DeleteShopNameCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show(Properties.Resources.Message_DeleteNotification, Properties.Resources.Title_Information,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                    Debug.Assert(this.WVM.DisplayedHierarchicalSettingVM.Kind == HierarchicalKind.Item);

                    using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                        HstShopDao hstShopDao = new HstShopDao(dbHandler);
                        _ = await hstShopDao.DeleteAsync(new HstShopDto {
                            ShopName = this.WVM.DisplayedHierarchicalSettingVM.SelectedShopVM.Name,
                            ItemId = this.WVM.DisplayedHierarchicalSettingVM.Id
                        });

                        // 店舗名を更新する
                        this.WVM.DisplayedHierarchicalSettingVM.ShopVMList = await this.LoadShopViewModelListAsync(dbHandler, this.WVM.DisplayedHierarchicalSettingVM.Id);
                    }
                }
            }
        }

        /// <summary>
        /// 備考を削除できるか判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteRemarkCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.DisplayedHierarchicalSettingVM?.SelectedRemarkVM != null;
        }

        /// <summary>
        /// 備考を削除する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DeleteRemarkCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show(Properties.Resources.Message_DeleteNotification, Properties.Resources.Title_Information,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                    Debug.Assert(this.WVM.DisplayedHierarchicalSettingVM.Kind == HierarchicalKind.Item);

                    using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                        HstRemarkDao hstRemarkDao = new HstRemarkDao(dbHandler);
                        _ = await hstRemarkDao.DeleteAsync(new HstRemarkDto {
                            Remark = this.WVM.DisplayedHierarchicalSettingVM.SelectedRemarkVM.Remark,
                            ItemId = this.WVM.DisplayedHierarchicalSettingVM.Id
                        });

                        // 備考欄の表示を更新する
                        this.WVM.DisplayedHierarchicalSettingVM.RemarkVMList = await this.LoadRemarkViewModelListAsync(dbHandler, this.WVM.DisplayedHierarchicalSettingVM.Id);
                    }
                }
            }
        }
        #endregion

        #region 帳簿設定の操作
        /// <summary>
        /// 帳簿を追加する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddBookCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                int bookId = -1;
                using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                    MstBookDao mstBookDao = new MstBookDao(dbHandler);
                    bookId = await mstBookDao.InsertReturningIdAsync(new MstBookDto { });
                }

                await this.UpdateBookSettingTabDataAsync(bookId);
                this.needToUpdate = true;
            }
        }

        /// <summary>
        /// 帳簿を削除可能か判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteBookCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedBookVM != null;
        }

        /// <summary>
        /// 帳簿を削除する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DeleteBookCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                    await dbHandler.ExecTransactionAsync(async () => {
                        HstActionDao hstActionDao = new HstActionDao(dbHandler);
                        var dtoList = await hstActionDao.FindByBookIdAsync(this.WVM.SelectedBookVM.Id.Value);
                        if (dtoList.Count() != 0) {
                            MessageBox.Show(Properties.Resources.Message_CantDeleteBecauseActionItemExistsInBook, Properties.Resources.Title_Error);
                            return;
                        }

                        MstBookDao hstBookDao = new MstBookDao(dbHandler);
                        _ = await hstBookDao.DeleteByIdAsync(this.WVM.SelectedBookVM.Id.Value);
                    });
                }

                await this.UpdateBookSettingTabDataAsync();
                this.needToUpdate = true;
            }
        }

        /// <summary>
        /// 帳簿の表示順を上げられるか判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RaiseBookSortOrderCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.BookVMList != null;
            if (e.CanExecute) {
                int index = this.WVM.BookVMList.IndexOf(this.WVM.SelectedBookVM);
                e.CanExecute = index > 0;
            }
        }

        /// <summary>
        /// 帳簿の表示順を上げる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RaiseBookSortOrderCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                int index = this.WVM.BookVMList.IndexOf(this.WVM.SelectedBookVM);
                int changingId = this.WVM.BookVMList[index].Id.Value;
                int changedId = this.WVM.BookVMList[index - 1].Id.Value;

                using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                    MstBookDao mstBookDao = new MstBookDao(dbHandler);
                    await mstBookDao.SwapSortOrderAsync(changingId, changedId);
                }

                await this.UpdateBookSettingTabDataAsync(changingId);
                this.needToUpdate = true;
            }
        }

        /// <summary>
        /// 帳簿の表示順を下げられるか判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DropBookSortOrderCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.BookVMList != null;
            if (e.CanExecute) {
                int index = this.WVM.BookVMList.IndexOf(this.WVM.SelectedBookVM);
                e.CanExecute = index != -1 && index < this.WVM.BookVMList.Count - 1;
            }
        }

        /// <summary>
        /// 帳簿の表示順を下げる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DropBookSortOrderCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                int index = this.WVM.BookVMList.IndexOf(this.WVM.SelectedBookVM);
                int changingId = this.WVM.BookVMList[index].Id.Value;
                int changedId = this.WVM.BookVMList[index + 1].Id.Value;

                using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                    MstBookDao mstBookDao = new MstBookDao(dbHandler);
                    await mstBookDao.SwapSortOrderAsync(changingId, changedId);
                }

                await this.UpdateBookSettingTabDataAsync(changingId);
                this.needToUpdate = true;
            }
        }

        /// <summary>
        /// CSVフォルダパスを指定するダイアログを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CsvFolderPathDialogCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;

            string folderPath = Path.GetDirectoryName(settings.App_CsvFilePath);
            string fileName = string.Empty;
            if (this.WVM.DisplayedBookSettingVM.CsvFolderPath != null && this.WVM.DisplayedBookSettingVM.CsvFolderPath != string.Empty) {
                folderPath = Path.GetDirectoryName(this.WVM.DisplayedBookSettingVM.CsvFolderPath);
                fileName = Path.GetFileName(this.WVM.DisplayedBookSettingVM.CsvFolderPath);
            }

            CommonOpenFileDialog ofd = new CommonOpenFileDialog() {
                EnsureFileExists = true,
                IsFolderPicker = true,
                InitialDirectory = folderPath,
                DefaultFileName = fileName,
                Title = Properties.Resources.Title_CsvFolderSelection,
            };

            if (ofd.ShowDialog() == CommonFileDialogResult.Ok) {
                this.WVM.DisplayedBookSettingVM.CsvFolderPath = Path.Combine(ofd.InitialDirectory, ofd.FileName);
            }
        }

        /// <summary>
        /// 帳簿の情報を保存できるか判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveBookInfoCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedBookVM != null && !string.IsNullOrWhiteSpace(this.WVM.SelectedBookVM.Name);
        }

        /// <summary>
        /// 帳簿の情報を保存する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SaveBookInfoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                BookSettingViewModel vm = this.WVM.DisplayedBookSettingVM;
                using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                    MstBookDto.JsonDto jsonObj = new MstBookDto.JsonDto() {
                        StartDate = vm.StartDateExists ? (DateTime?)vm.StartDate : null,
                        EndDate = vm.EndDateExists ? (DateTime?)vm.EndDate : null,
                        CsvFolderPath = vm.CsvFolderPath != string.Empty ? vm.CsvFolderPath : null,
                        CsvActDateIndex = vm.ActDateIndex - 1,
                        CsvOutgoIndex = vm.ExpensesIndex - 1,
                        CsvItemNameIndex = vm.ItemNameIndex - 1
                    };
                    string jsonCode = JsonConvert.SerializeObject(jsonObj);

                    MstBookDao mstBookDao = new MstBookDao(dbHandler);
                    _ = await mstBookDao.UpdateSetableAsync(new MstBookDto {
                        BookName = vm.Name,
                        BookKind = (int)vm.SelectedBookKind,
                        InitialValue = vm.InitialValue,
                        DebitBookId = vm.SelectedDebitBookVM.Id == -1 ? null : vm.SelectedDebitBookVM.Id,
                        PayDay = vm.PayDay,
                        JsonCode = jsonCode,
                        BookId = vm.Id.Value
                    });
                }

                await this.UpdateBookSettingTabDataAsync(vm.Id);
                MessageBox.Show(Properties.Resources.Message_FinishToSave, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information);
                this.needToUpdate = true;
            }
        }

        /// <summary>
        /// 帳簿-項目の関係を変更する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ChangeBookRelationCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                BookSettingViewModel vm = this.WVM.DisplayedBookSettingVM;
                vm.SelectedRelationVM = (e.OriginalSource as CheckBox)?.DataContext as RelationViewModel;

                using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                    await dbHandler.ExecTransactionAsync(async () => {
                        HstActionDao hstActionDao = new HstActionDao(dbHandler);
                        var hstActionDtoList = await hstActionDao.FindByBookIdAndItemIdAsync(vm.Id.Value, vm.SelectedRelationVM.Id);

                        if (hstActionDtoList.Count() != 0) {
                            vm.SelectedRelationVM.IsRelated = !vm.SelectedRelationVM.IsRelated; // 選択前の状態に戻す
                            MessageBox.Show(Properties.Resources.Message_CantDeleteBecauseActionItemExistsInItemWithinBook, Properties.Resources.Title_Error);
                            e.Handled = true;
                            return;
                        }

                        RelBookItemDao relBookItemDao = new RelBookItemDao(dbHandler);
                        var dtoList = await relBookItemDao.UpsertAsync(new RelBookItemDto {
                            BookId = vm.Id.Value,
                            ItemId = vm.SelectedRelationVM.Id,
                            DelFlg = vm.SelectedRelationVM.IsRelated ? 0 : 1
                        });
                    });
                }
                this.needToUpdate = true;
            }
        }
        #endregion

        #region その他の設定の操作
        /// <summary>
        /// pg_dump.exeを指定するダイアログを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DumpExePathDialogCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string folderPath = string.Empty;
            string fileName = string.Empty;
            if (this.WVM.DumpExePath != string.Empty) {
                folderPath = Path.GetDirectoryName(this.WVM.DumpExePath);
                fileName = Path.GetFileName(this.WVM.DumpExePath);
            }

            OpenFileDialog ofd = new OpenFileDialog() {
                CheckFileExists = true,
                InitialDirectory = folderPath,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = "pg_dump.exe|pg_dump.exe"
            };

            if (ofd.ShowDialog() == true) {
                this.WVM.DumpExePath = Path.Combine(ofd.InitialDirectory, ofd.FileName);
            }
        }

        /// <summary>
        /// pg_restore.exeを指定するダイアログを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RestorePathDialogCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string folderPath = string.Empty;
            string fileName = string.Empty;
            if (this.WVM.RestoreExePath != string.Empty) {
                folderPath = Path.GetDirectoryName(this.WVM.RestoreExePath);
                fileName = Path.GetFileName(this.WVM.RestoreExePath);
            }

            OpenFileDialog ofd = new OpenFileDialog() {
                CheckFileExists = true,
                InitialDirectory = folderPath,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = "pg_restore.exe|pg_restore.exe"
            };

            if (ofd.ShowDialog() == true) {
                this.WVM.RestoreExePath = Path.Combine(ofd.InitialDirectory, ofd.FileName);
            }
        }

        /// <summary>
        /// データベース設定を行うために再起動する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RestartForDbSettingCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show(Properties.Resources.Message_RestartNotification, Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                Properties.Settings.Default.App_InitFlag = true;
                Properties.Settings.Default.Save();

                ((App)Application.Current).Restart();
            }
        }

        /// <summary>
        /// 言語設定を行うために再起動する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RestartForLanguageCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show(Properties.Resources.Message_RestartNotification, Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                Properties.Settings.Default.App_CultureName = this.WVM.SelectedCultureName;
                Properties.Settings.Default.Save();

                ((App)Application.Current).Restart();
            }
        }

        /// <summary>
        /// バックアップフォルダを指定するダイアログを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackUpFolderPathDialogCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string folderPath = Path.GetDirectoryName(Application.ResourceAssembly.Location);
            string fileName = string.Empty;
            if (this.WVM.BackUpFolderPath != string.Empty) {
                folderPath = Path.GetDirectoryName(this.WVM.BackUpFolderPath);
                fileName = Path.GetFileName(this.WVM.BackUpFolderPath);
            }

            CommonOpenFileDialog ofd = new CommonOpenFileDialog() {
                EnsureFileExists = true,
                IsFolderPicker = true,
                InitialDirectory = folderPath,
                DefaultFileName = fileName,
                Title = Properties.Resources.Title_BackupFolderSelection
            };

            if (ofd.ShowDialog() == CommonFileDialogResult.Ok) {
                if (Path.GetDirectoryName(Application.ResourceAssembly.Location).CompareTo(ofd.InitialDirectory) == 0) {
                    this.WVM.BackUpFolderPath = Path.GetFileName(ofd.FileName);
                }
                else {
                    this.WVM.BackUpFolderPath = Path.Combine(ofd.InitialDirectory, ofd.FileName);
                }
            }
        }

        /// <summary>
        /// ウィンドウ設定を再読込する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReloadWindowSettingCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.WVM.LoadWindowSetting();
        }

        /// <summary>
        /// ウィンドウ設定を初期化する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InitializeWindowSettingCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show(Properties.Resources.Message_RestartNotification, Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                Properties.Settings settings = Properties.Settings.Default;

                // メイン
                settings.MainWindow_Left = -1;
                settings.MainWindow_Top = -1;
                settings.MainWindow_Width = -1;
                settings.MainWindow_Height = -1;

                // 移動
                settings.MoveRegistrationWindow_Left = -1;
                settings.MoveRegistrationWindow_Top = -1;
                settings.MoveRegistrationWindow_Width = -1;
                settings.MoveRegistrationWindow_Height = -1;

                // 追加・変更
                settings.ActionRegistrationWindow_Left = -1;
                settings.ActionRegistrationWindow_Top = -1;
                settings.ActionRegistrationWindow_Width = -1;
                settings.ActionRegistrationWindow_Height = -1;

                // リスト追加
                settings.ActionListRegistrationWindow_Left = -1;
                settings.ActionListRegistrationWindow_Top = -1;
                settings.ActionListRegistrationWindow_Width = -1;
                settings.ActionListRegistrationWindow_Height = -1;

                // CSV比較
                settings.CsvComparisonWindow_Left = -1;
                settings.CsvComparisonWindow_Top = -1;
                settings.CsvComparisonWindow_Width = -1;
                settings.CsvComparisonWindow_Height = -1;

                // 期間選択
                settings.TermWindow_Left = -1;
                settings.TermWindow_Top = -1;
                // settings.TermWindow_Width = -1;
                // settings.TermWindow_Height = -1;

                // 設定
                settings.SettingsWindow_Left = -1;
                settings.SettingsWindow_Top = -1;
                settings.SettingsWindow_Width = -1;
                settings.SettingsWindow_Height = -1;

                settings.App_InitSizeFlag = true;
                settings.Save();

                ((App)Application.Current).Restart();
            }
        }
        #endregion
        #endregion

        #region ウィンドウ
        /// <summary>
        /// ウィンドウ読込完了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await this.UpdateSettingWindowDataAsync();

            this.RegisterEventHandlerToWVM();
        }

        /// <summary>
        /// ウィンドウ終了前
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsWindow_Closing(object sender, CancelEventArgs e)
        {
            this.DialogResult = this.needToUpdate || this.WVM.NeedToUpdate;
        }

        /// <summary>
        /// ウィンドウ終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsWindow_Closed(object sender, EventArgs e)
        {
            this.SaveWindowSetting();
        }
        #endregion

        /// <summary>
        /// 選択中の設定タブを変更した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SettingsTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.oldSelectedSettingsTab != this.WVM.SelectedTab) {
                Log.Info(this.WVM.SelectedTab.ToString());

                this.oldSelectedSettingsTab = this.WVM.SelectedTab;
                using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                    switch (this.WVM.SelectedTab) {
                        case SettingsTabs.ItemSettingsTab:
                            await this.UpdateItemSettingsTabDataAsync(HierarchicalSettingViewModel.GetHierarchicalKind(this.WVM.SelectedHierarchicalVM), this.WVM.SelectedHierarchicalVM?.Id);
                            break;
                        case SettingsTabs.BookSettingsTab:
                            await this.UpdateBookSettingTabDataAsync(this.WVM.SelectedBookVM?.Id);
                            break;
                        case SettingsTabs.OtherSettingsTab:
                            this.UpdateOtherSettingTabData();
                            break;
                    }
                }
            }
        }

        #region 項目設定操作
        /// <summary>
        /// 項目設定で一覧の選択を変更した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (this.shopDataGrid.Items.Count > 0 && VisualTreeHelper.GetChildrenCount(this.shopDataGrid) > 0) {
                if (VisualTreeHelper.GetChild(this.shopDataGrid, 0) is Decorator border) {
                    if (border.Child is ScrollViewer scroll) {
                        scroll.ScrollToTop();
                    }
                }
            }
            if (this.remarkDataGrid.Items.Count > 0 && VisualTreeHelper.GetChildrenCount(this.remarkDataGrid) > 0) {
                if (VisualTreeHelper.GetChild(this.remarkDataGrid, 0) is Decorator border) {
                    if (border.Child is ScrollViewer scroll) {
                        scroll.ScrollToTop();
                    }
                }
            }
        }
        #endregion
        #endregion

        #region 画面更新用の関数
        /// <summary>
        /// 設定ウィンドウに表示するデータを更新する
        /// </summary>
        /// <param name="kind">選択対象の階層種別</param>
        /// <param name="categoryOrItemId">選択対象のID</param>
        /// <param name="bookId">選択対象の帳簿ID</param>
        private async Task UpdateSettingWindowDataAsync(HierarchicalKind? kind = null, int? categoryOrItemId = null, int? bookId = null)
        {
            Log.Info();

            await this.UpdateItemSettingsTabDataAsync(kind, categoryOrItemId);
            await this.UpdateBookSettingTabDataAsync(bookId);
            this.UpdateOtherSettingTabData();
        }

        /// <summary>
        /// 項目設定タブに表示するデータを更新する
        /// </summary>
        /// <param name="kind">選択対象の階層種別</param>
        /// <param name="id">選択対象のID</param>
        private async Task UpdateItemSettingsTabDataAsync(HierarchicalKind? kind = null, int? id = null)
        {
            if (this.WVM.SelectedTab == SettingsTabs.ItemSettingsTab) {
                Log.Info();

                // 指定がなければ現在選択中の項目を再選択する
                if (this.WVM.SelectedHierarchicalVM != null && kind == null && id == null) {
                    kind = HierarchicalSettingViewModel.GetHierarchicalKind(this.WVM.SelectedHierarchicalVM);
                    id = this.WVM.SelectedHierarchicalVM.Id;
                }

                this.WVM.HierarchicalVMList = await this.LoadHierarchicalViewModelListAsync();

                // 選択する項目を探す
                HierarchicalViewModel selectedVM = null;
                if (kind != null && id != null) {
                    // 収支から探す
                    IEnumerable<HierarchicalViewModel> query = this.WVM.HierarchicalVMList.Where((vm) => { return HierarchicalSettingViewModel.GetHierarchicalKind(vm) == kind && vm.Id == id; });

                    if (query.Count() == 0) {
                        // 分類から探す
                        foreach (HierarchicalViewModel tmpVM in this.WVM.HierarchicalVMList) {
                            query = tmpVM.ChildrenVMList.Where((vm) => { return HierarchicalSettingViewModel.GetHierarchicalKind(vm) == kind && vm.Id == id; });
                            if (query.Count() != 0) { break; }
                        }
                    }

                    if (query.Count() == 0) {
                        // 項目から探す
                        foreach (HierarchicalViewModel tmpVM in this.WVM.HierarchicalVMList) {
                            foreach (HierarchicalViewModel tmpVM2 in tmpVM.ChildrenVMList) {
                                query = tmpVM2.ChildrenVMList.Where((vm) => { return HierarchicalSettingViewModel.GetHierarchicalKind(vm) == kind && vm.Id == id; });
                                if (query.Count() != 0) { break; }
                            }
                            if (query.Count() != 0) { break; }
                        }
                    }

                    selectedVM = query.Count() != 0 ? query.First() : null;
                }

                // 何も選択されていないなら1番上の項目を選択する
                if (selectedVM == null && this.WVM.HierarchicalVMList.Count != 0) {
                    selectedVM = this.WVM.HierarchicalVMList[0];
                }
                this.WVM.SelectedHierarchicalVM = selectedVM;
            }
        }

        /// <summary>
        /// 項目設定タブの入力欄に表示するデータを更新する
        /// </summary>
        /// <param name="kind">表示対象の階層種別</param>
        /// <param name="id">表示対象のID</param>
        /// <returns></returns>
        private async Task UpdateItemSettingsTabInputDataAsync(HierarchicalKind kind, int id)
        {
            HierarchicalSettingViewModel vm = null;

            switch (kind) {
                case HierarchicalKind.Balance: {
                    vm = new HierarchicalSettingViewModel() {
                        Kind = HierarchicalKind.Balance,
                        Id = this.WVM.SelectedHierarchicalVM.Id,
                        Name = this.WVM.SelectedHierarchicalVM.Name
                    };
                    break;
                }
                case HierarchicalKind.Category: {
                    // 分類
                    using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                        MstCategoryDao mstCategoryDao = new MstCategoryDao(dbHandler);
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
                    using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                        MstItemDao mstItemDao = new MstItemDao(dbHandler);
                        var dto = await mstItemDao.FindByIdAsync(id);

                        vm = new HierarchicalSettingViewModel {
                            Kind = HierarchicalKind.Item,
                            Id = id,
                            SortOrder = dto.SortOrder,
                            Name = dto.ItemName,
                            RelationVMList = await this.LoadRelationViewModelList1Async(dbHandler, id),
                            ShopVMList = await this.LoadShopViewModelListAsync(dbHandler, id),
                            RemarkVMList = await this.LoadRemarkViewModelListAsync(dbHandler, id)
                        };
                    }
                    break;
                }
            }

            this.WVM.DisplayedHierarchicalSettingVM = vm;
        }

        /// <summary>
        /// 帳簿設定タブに表示するデータを更新する
        /// </summary>
        /// <param name="bookId">選択対象の帳簿ID</param>
        private async Task UpdateBookSettingTabDataAsync(int? bookId = null)
        {
            if (this.WVM.SelectedTab == SettingsTabs.BookSettingsTab) {
                Log.Info();

                // 指定がなければ現在選択中の項目を再選択する
                if (this.WVM.SelectedBookVM != null && bookId == null) {
                    bookId = this.WVM.SelectedBookVM.Id;
                }

                this.WVM.BookVMList = await this.LoadBookViewModelListAsync();

                // 選択する項目を探す
                BookViewModel selectedVM = null;
                if (bookId != null) {
                    IEnumerable<BookViewModel> query = this.WVM.BookVMList.Where((vm) => { return vm.Id == bookId; });
                    selectedVM = query.Count() != 0 ? query.First() : null;
                }

                // 何も選択されていないなら1番上の項目を選択する
                if (selectedVM == null && this.WVM.BookVMList.Count != 0) {
                    selectedVM = this.WVM.BookVMList[0];
                }
                this.WVM.SelectedBookVM = selectedVM;
            }
        }

        /// <summary>
        /// 帳簿設定タブの入力欄に表示するデータを更新する
        /// </summary>
        /// <param name="bookId">表示対象の帳簿ID</param>
        /// <returns></returns>
        private async Task UpdateBookSettingTabInputDataAsync(int bookId)
        {
            BookSettingViewModel vm = null;

            ObservableCollection<BookViewModel> vmList = new ObservableCollection<BookViewModel>() {
                new BookViewModel(){ Id = -1, Name = Properties.Resources.ListName_None }
            };

            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                // 帳簿一覧を取得する(支払元選択用)
                MstBookDao mstBookDao = new MstBookDao(dbHandler);
                var dtoList = await mstBookDao.FindAllAsync();
                foreach (MstBookDto tmpDto in dtoList) {
                    vmList.Add(new BookViewModel() {
                        Id = tmpDto.BookId,
                        Name = tmpDto.BookName
                    });
                }

                // 帳簿一覧を取得する
                BookInfoDao bookInfoDao = new BookInfoDao(dbHandler);
                var dto = await bookInfoDao.FindByBookId(bookId);

                MstBookDto.JsonDto jsonObj = dto.JsonCode != null ? JsonConvert.DeserializeObject<MstBookDto.JsonDto>(dto.JsonCode) : null;

                vm = new BookSettingViewModel() {
                    Id = bookId,
                    SortOrder = dto.SortOrder,
                    Name = dto.BookName,
                    SelectedBookKind = (BookKind)dto.BookKind,
                    InitialValue = dto.InitialValue,
                    StartDateExists = jsonObj?.StartDate != null,
                    StartDate = jsonObj?.StartDate ?? dto.StartDate ?? DateTime.Today,
                    EndDateExists = jsonObj?.EndDate != null,
                    EndDate = jsonObj?.EndDate ?? dto.EndDate ?? DateTime.Today,
                    DebitBookVMList = new ObservableCollection<BookViewModel>(vmList.Where((tmpVM) => { return tmpVM.Id != bookId; })),
                    PayDay = dto.PayDay,
                    CsvFolderPath = jsonObj?.CsvFolderPath,
                    ActDateIndex = jsonObj?.CsvActDateIndex + 1,
                    ExpensesIndex = jsonObj?.CsvOutgoIndex + 1,
                    ItemNameIndex = jsonObj?.CsvItemNameIndex + 1
                };
                vm.SelectedDebitBookVM = vm.DebitBookVMList.FirstOrDefault((tmpVM) => { return tmpVM.Id == dto.DebitBookId; }) ?? vm.DebitBookVMList[0];

                vm.RelationVMList = await this.LoadRelationViewModelList2Async(dbHandler, bookId);
            }

            this.WVM.DisplayedBookSettingVM = vm;
        }

        /// <summary>
        /// その他タブに表示するデータを更新する
        /// </summary>
        private void UpdateOtherSettingTabData()
        {
            if (this.WVM.SelectedTab == SettingsTabs.OtherSettingsTab) {
                Log.Info();

                this.WVM.LoadSettings();
            }
        }

        /// <summary>
        /// 階層構造項目VMリストを取得する
        /// </summary>
        /// <returns>階層構造項目VMリスト</returns>
        private async Task<ObservableCollection<HierarchicalViewModel>> LoadHierarchicalViewModelListAsync()
        {
            Log.Info();

            ObservableCollection<HierarchicalViewModel> vmList = new ObservableCollection<HierarchicalViewModel>();
            HierarchicalViewModel incomeVM = new HierarchicalViewModel() {
                Depth = (int)HierarchicalKind.Balance,
                Id = (int)BalanceKind.Income,
                SortOrder = -1,
                Name = Properties.Resources.BalanceKind_Income,
                ParentVM = null,
                ChildrenVMList = new ObservableCollection<HierarchicalViewModel>()
            };
            vmList.Add(incomeVM);

            HierarchicalViewModel expensesVM = new HierarchicalViewModel() {
                Depth = (int)HierarchicalKind.Balance,
                Id = (int)BalanceKind.Expenses,
                SortOrder = -1,
                Name = Properties.Resources.BalanceKind_Expenses,
                ParentVM = null,
                ChildrenVMList = new ObservableCollection<HierarchicalViewModel>()
            };
            vmList.Add(expensesVM);

            foreach (HierarchicalViewModel vm in vmList) {
                using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                    // 分類
                    MstCategoryDao mstCategoryDao = new MstCategoryDao(dbHandler);
                    var cDtoList = await mstCategoryDao.FindByBalanceKindAsync(vm.Id);

                    foreach (MstCategoryDto dto in cDtoList) {
                        vm.ChildrenVMList.Add(new HierarchicalViewModel() {
                            Depth = (int)HierarchicalKind.Category,
                            Id = dto.CategoryId,
                            SortOrder = dto.SortOrder,
                            Name = dto.CategoryName,
                            ParentVM = vm,
                            ChildrenVMList = new ObservableCollection<HierarchicalViewModel>()
                        });
                    }

                    // 項目
                    MstItemDao mstItemDao = new MstItemDao(dbHandler);
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
        /// 帳簿VMリストを取得する
        /// </summary>
        /// <returns>帳簿VMリスト</returns>
        private async Task<ObservableCollection<BookViewModel>> LoadBookViewModelListAsync()
        {
            Log.Info();

            ObservableCollection<BookViewModel> bookVMList = new ObservableCollection<BookViewModel>();

            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                // 帳簿一覧を取得する
                MstBookDao mstBookDao = new MstBookDao(dbHandler);
                var dtoList = await mstBookDao.FindAllAsync();
                foreach (MstBookDto dto in dtoList) {
                    bookVMList.Add(new BookViewModel() {
                        Id = dto.BookId,
                        Name = dto.BookName
                    });
                }
            }

            return bookVMList;
        }

        /// <summary>
        /// 関連VMリスト1(項目主体)を取得する
        /// </summary>
        /// <param name="dbHandler">DBハンドラ</param>
        /// <param name="itemId">項目ID</param>
        /// <returns>関連VMリスト</returns>
        private async Task<ObservableCollection<RelationViewModel>> LoadRelationViewModelList1Async(DbHandlerBase dbHandler, int itemId)
        {
            BookRelFromItemInfoDao bookRelFromItemInfoDao = new BookRelFromItemInfoDao(dbHandler);
            var dtoList = await bookRelFromItemInfoDao.FindByItemIdAsync(itemId);

            ObservableCollection<RelationViewModel> rvmList = new ObservableCollection<RelationViewModel>();
            foreach (BookRelFromItemInfoDto dto in dtoList) {
                RelationViewModel rvm = new RelationViewModel() {
                    Id = dto.BookId,
                    Name = dto.BookName,
                    IsRelated = dto.IsRelated
                };
                rvmList.Add(rvm);
            }
            return rvmList;
        }

        /// <summary>
        /// 関連VMリスト2(帳簿主体)を取得する
        /// </summary>
        /// <param name="dbHandler">DBハンドラ</param>
        /// <param name="bookId">帳簿ID</param>
        /// <returns>関連VMリスト</returns>
        private async Task<ObservableCollection<RelationViewModel>> LoadRelationViewModelList2Async(DbHandlerBase dbHandler, int bookId)
        {
            ItemRelFromBookInfoDao itemRelFromBookInfoDao = new ItemRelFromBookInfoDao(dbHandler);
            var dtoList = await itemRelFromBookInfoDao.FindByBookIdAsync(bookId);

            ObservableCollection<RelationViewModel> rvmList = new ObservableCollection<RelationViewModel>();
            foreach (ItemRelFromBookInfoDto dto in dtoList) {
                RelationViewModel rvm = new RelationViewModel() {
                    Id = dto.ItemId,
                    Name = $"{BalanceKindStr[(BalanceKind)dto.BalanceKind]} > {dto.CategoryName} > {dto.ItemName}",
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
        private async Task<ObservableCollection<ShopViewModel>> LoadShopViewModelListAsync(DbHandlerBase dbHandler, int itemId)
        {
            ObservableCollection<ShopViewModel> svmList = new ObservableCollection<ShopViewModel>();
            ShopInfoDao shopInfoDao = new ShopInfoDao(dbHandler);
            var dtoList = await shopInfoDao.FindByItemIdAsync(itemId);

            foreach (ShopInfoDto dto in dtoList) {
                ShopViewModel svm = new ShopViewModel() {
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
        private async Task<ObservableCollection<RemarkViewModel>> LoadRemarkViewModelListAsync(DbHandlerBase dbHandler, int itemId)
        {
            ObservableCollection<RemarkViewModel> rvmList = new ObservableCollection<RemarkViewModel>();
            RemarkInfoDao remarkInfoDao = new RemarkInfoDao(dbHandler);
            var dtoList = await remarkInfoDao.FindByItemIdAsync(itemId);

            foreach (RemarkInfoDto dto in dtoList) {
                RemarkViewModel rvm = new RemarkViewModel() {
                    Remark = dto.Remark,
                    UsedCount = dto.Count,
                    UsedTime = dto.UsedTime
                };
                rvmList.Add(rvm);
            }
            return rvmList;
        }
        #endregion

        #region 設定反映用の関数
        /// <summary>
        /// ウィンドウ設定を読み込む
        /// </summary>
        public void LoadWindowSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (settings.SettingsWindow_Height != -1 && settings.SettingsWindow_Width != -1) {
                this.Height = settings.SettingsWindow_Height;
                this.Width = settings.SettingsWindow_Width;
            }

            if (settings.App_IsPositionSaved && (-10 <= settings.SettingsWindow_Left && 0 <= settings.SettingsWindow_Top)) {
                this.Left = settings.SettingsWindow_Left;
                this.Top = settings.SettingsWindow_Top;
            }
            else {
                this.MoveOwnersCenter();
            }
        }

        /// <summary>
        /// ウィンドウ設定を保存する
        /// </summary>
        public void SaveWindowSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;
            if (!settings.App_InitSizeFlag && this.WindowState == WindowState.Normal) {
                if (settings.App_IsPositionSaved) {
                    settings.SettingsWindow_Left = this.Left;
                    settings.SettingsWindow_Top = this.Top;
                }
                settings.SettingsWindow_Height = this.Height;
                settings.SettingsWindow_Width = this.Width;
                settings.Save();
            }
        }
        #endregion

        /// <summary>
        /// イベントハンドラをWVMに登録する
        /// </summary>
        private void RegisterEventHandlerToWVM()
        {
            this.WVM.SelectedHierarchicalVMChanged += async (e) => {
                if (e.Value != null) {
                    await this.UpdateItemSettingsTabInputDataAsync(HierarchicalSettingViewModel.GetHierarchicalKind(e.Value).Value, e.Value.Id);
                }
                else {
                    this.WVM.DisplayedHierarchicalSettingVM = null;
                }
            };

            this.WVM.SelectedBookVMChanged += async (e) => {
                if (e.Value != null) {
                    await this.UpdateBookSettingTabInputDataAsync(e.Value.Id.Value);
                }
                else {
                    this.WVM.DisplayedBookSettingVM = null;
                }
            };
        }
    }
}

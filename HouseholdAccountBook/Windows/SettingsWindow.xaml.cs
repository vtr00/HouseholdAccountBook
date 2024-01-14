using HouseholdAccountBook.Dao;
using HouseholdAccountBook.Dto;
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
        /// DAOビルダ
        /// </summary>
        private readonly DaoBuilder builder;
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
        /// <param name="builder">DAOビルダ</param>
        public SettingsWindow(DaoBuilder builder)
        {
            this.builder = builder;

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
            e.CanExecute = this.WVM.SelectedItemVM != null && this.WVM.SelectedItemVM.Kind != HierarchicalKind.Item;
        }

        /// <summary>
        /// 分類を追加する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddCategoryCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            HierarchicalSettingViewModel vm = this.WVM.SelectedItemVM;
            while (vm.Kind != HierarchicalKind.Balance) {
                vm = vm.ParentVM;
            }
            BalanceKind kind = (BalanceKind)vm.Id;

            int categoryId = -1;
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader = await dao.ExecQueryAsync(@"
INSERT INTO mst_category (category_name, balance_kind, sort_order, del_flg, update_time, updater, insert_time, inserter)
VALUES ('(no name)', @{0}, (SELECT COALESCE(MAX(sort_order) + 1, 1) FROM mst_category), 0, 'now', @{1}, 'now', @{2})
RETURNING category_id;", (int)kind, Updater, Inserter);

                reader.ExecARow((record) => {
                    categoryId = record.ToInt("category_id");
                });
            }
            await this.UpdateItemSettingsTabDataAsync(HierarchicalKind.Category, categoryId);
            this.needToUpdate = true;
        }

        /// <summary>
        /// 項目を追加可能か判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedItemVM != null && this.WVM.SelectedItemVM.Kind != HierarchicalKind.Balance;
        }

        /// <summary>
        /// 項目を追加する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            HierarchicalSettingViewModel vm = this.WVM.SelectedItemVM;
            while (vm.Kind != HierarchicalKind.Category) {
                vm = vm.ParentVM;
            }
            int categoryId = vm.Id;

            int itemId = -1;
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader = await dao.ExecQueryAsync(@"
INSERT INTO mst_item (item_name, category_id, advance_flg, sort_order, del_flg, update_time, updater, insert_time, inserter)
VALUES ('(no name)', @{0}, 0, (SELECT COALESCE(MAX(sort_order) + 1, 1) FROM mst_item), 0, 'now', @{1}, 'now', @{2})
RETURNING item_id;", categoryId, Updater, Inserter);

                reader.ExecARow((record) => {
                    itemId = record.ToInt("item_id");
                });
            }
            await this.UpdateItemSettingsTabDataAsync(HierarchicalKind.Item, itemId);
            this.needToUpdate = true;
        }

        /// <summary>
        /// 分類/項目を削除可能か判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedItemVM != null && this.WVM.SelectedItemVM.Kind != HierarchicalKind.Balance;
        }

        /// <summary>
        /// 分類/項目を削除する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DeleteItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            HierarchicalKind kind = this.WVM.SelectedItemVM.Kind;
            int id = this.WVM.SelectedItemVM.Id;

            using (DaoBase dao = this.builder.Build()) {
                switch (kind) {
                    case HierarchicalKind.Category: {
                            await dao.ExecTransactionAsync(async () => {
                                DaoReader reader = await dao.ExecQueryAsync(@"
SELECT 1
FROM hst_action A
INNER JOIN (SELECT item_id FROM mst_item WHERE del_flg = 0 AND category_id = @{0}) I ON A.item_id = I.item_id
WHERE A.del_flg = 0;", id);

                                if (reader.Count != 0) {
                                    MessageBox.Show("分類内に帳簿項目が存在するので削除できません。"); // TODO
                                    return;
                                }

                                await dao.ExecNonQueryAsync(@"
UPDATE mst_category
SET del_flg = 1, update_time = 'now', updater = @{0}
WHERE category_id = @{1};", Updater, id);
                            });
                        }
                        break;
                    case HierarchicalKind.Item: {
                            await dao.ExecTransactionAsync(async () => {
                                DaoReader reader = await dao.ExecQueryAsync(@"
SELECT *
FROM hst_action
WHERE del_flg = 0 AND item_id = @{0};", id);

                                if (reader.Count != 0) {
                                    MessageBox.Show("項目内に帳簿項目が存在するので削除できません。"); // TODO
                                    return;
                                }

                                await dao.ExecNonQueryAsync(@"
UPDATE mst_item
SET del_flg = 1, update_time = 'now', updater = @{0}
WHERE item_id = @{1};", Updater, id);
                            });
                        }
                        break;
                }
            }
            await this.UpdateItemSettingsTabDataAsync(this.WVM.SelectedItemVM.ParentVM.Kind, this.WVM.SelectedItemVM.ParentVM.Id);
            this.needToUpdate = true;
        }

        /// <summary>
        /// 分類/項目の表示順を上げれるか判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RaiseItemSortOrderCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedItemVM != null && this.WVM.SelectedItemVM.ParentVM != null;
            if (e.CanExecute) {
                // 同じ階層で、よりソート順序が上の分類/項目がある場合trueになる
                var parentVM = this.WVM.SelectedItemVM.ParentVM;
                int index = parentVM.ChildrenVMList.IndexOf(this.WVM.SelectedItemVM);
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
            var parentVM = this.WVM.SelectedItemVM.ParentVM;
            int index = parentVM.ChildrenVMList.IndexOf(this.WVM.SelectedItemVM);
            int changingId = parentVM.ChildrenVMList[index].Id; // 選択中の項目のID
            if (0 < index) {
                int changedId = parentVM.ChildrenVMList[index - 1].Id; // 入れ替え対象の項目のID

                using (DaoBase dao = this.builder.Build()) {
                    switch (this.WVM.SelectedItemVM.Kind) {
                        case HierarchicalKind.Category: {
                                await dao.ExecTransactionAsync(async () => {
                                    DaoReader reader = await dao.ExecQueryAsync(@"
SELECT sort_order
FROM mst_category
WHERE category_id = @{0};", changedId);

                                    int tmpOrder = -1;
                                    reader.ExecARow((record) => {
                                        tmpOrder = record.ToInt("sort_order");
                                    });

                                    await dao.ExecNonQueryAsync(@"
UPDATE mst_category
SET sort_order = (SELECT sort_order FROM mst_category WHERE category_id = @{0}), update_time = 'now', updater = @{1}
WHERE category_id = @{2};", changingId, Updater, changedId);

                                    await dao.ExecNonQueryAsync(@"
UPDATE mst_category
SET sort_order = @{0}, update_time = 'now', updater = @{1}
WHERE category_id = @{2};", tmpOrder, Updater, changingId);
                                });
                            }
                            break;
                        case HierarchicalKind.Item: {
                                await dao.ExecTransactionAsync(async () => {
                                    DaoReader reader = await dao.ExecQueryAsync(@"
SELECT sort_order
FROM mst_item
WHERE item_id = @{0};", changedId);

                                    int tmpOrder = -1;
                                    reader.ExecARow((record) => {
                                        tmpOrder = record.ToInt("sort_order");
                                    });

                                    await dao.ExecNonQueryAsync(@"
UPDATE mst_item
SET sort_order = (SELECT sort_order FROM mst_item WHERE item_id = @{0}), update_time = 'now', updater = @{1}
WHERE item_id = @{2};", changingId, Updater, changedId);

                                    await dao.ExecNonQueryAsync(@"
UPDATE mst_item
SET sort_order = @{0}, update_time = 'now', updater = @{1}
WHERE item_id = @{2};", tmpOrder, Updater, changingId);
                                });
                            }
                            break;
                    }
                }
            }
            else { // 分類を跨いで項目の表示順を変更するとき
                Debug.Assert(this.WVM.SelectedItemVM.Kind == HierarchicalKind.Item);
                var grandparentVM = parentVM.ParentVM;
                int index2 = grandparentVM.ChildrenVMList.IndexOf(parentVM);
                int toCategoryId = grandparentVM.ChildrenVMList[index2 - 1].Id;

                using (DaoBase dao = this.builder.Build()) {
                    await dao.ExecNonQueryAsync(@"
UPDATE mst_item
SET category_id = @{0}, sort_order = (SELECT COALESCE(MAX(sort_order) + 1, 1) FROM mst_item), update_time = 'now', updater = @{1}
WHERE item_id = @{2};", toCategoryId, Updater, changingId);
                }
            }

            await this.UpdateItemSettingsTabDataAsync(this.WVM.SelectedItemVM.Kind, this.WVM.SelectedItemVM.Id);
            this.needToUpdate = true;
        }

        /// <summary>
        /// 分類/項目の表示順を下げれるか判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DropItemSortOrderCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedItemVM != null && this.WVM.SelectedItemVM.ParentVM != null;
            if (e.CanExecute) {
                // 同じ階層で、よりソート順序が下の分類/項目がある場合trueになる
                var parentVM = this.WVM.SelectedItemVM.ParentVM;
                int index = parentVM.ChildrenVMList.IndexOf(this.WVM.SelectedItemVM);
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
            var parentVM = this.WVM.SelectedItemVM.ParentVM;
            int index = parentVM.ChildrenVMList.IndexOf(this.WVM.SelectedItemVM);
            int changingId = parentVM.ChildrenVMList[index].Id; // 選択中の項目のID
            if (parentVM.ChildrenVMList.Count - 1 > index) {
                int changedId = parentVM.ChildrenVMList[index + 1].Id; // 入れ替え対象の項目のID

                using (DaoBase dao = this.builder.Build()) {
                    switch (this.WVM.SelectedItemVM.Kind) {
                        case HierarchicalKind.Category: {
                                await dao.ExecTransactionAsync(async () => {
                                    DaoReader reader = await dao.ExecQueryAsync(@"
SELECT sort_order
FROM mst_category
WHERE category_id = @{0};", changedId);

                                    int tmpOrder = -1;
                                    reader.ExecARow((record) => {
                                        tmpOrder = record.ToInt("sort_order");
                                    });

                                    await dao.ExecNonQueryAsync(@"
UPDATE mst_category
SET sort_order = (SELECT sort_order FROM mst_category WHERE category_id = @{0}), update_time = 'now', updater = @{1}
WHERE category_id = @{2};", changingId, Updater, changedId);

                                    await dao.ExecNonQueryAsync(@"
UPDATE mst_category
SET sort_order = @{0}, update_time = 'now', updater = @{1}
WHERE category_id = @{2};", tmpOrder, Updater, changingId);
                                });
                            }
                            break;
                        case HierarchicalKind.Item: {
                                await dao.ExecTransactionAsync(async () => {
                                    DaoReader reader = await dao.ExecQueryAsync(@"
SELECT sort_order
FROM mst_item
WHERE item_id = @{0};", changedId);

                                    int tmpOrder = -1;
                                    reader.ExecARow((record) => {
                                        tmpOrder = record.ToInt("sort_order");
                                    });

                                    await dao.ExecNonQueryAsync(@"
UPDATE mst_item
SET sort_order = (SELECT sort_order FROM mst_item WHERE item_id = @{0}), update_time = 'now', updater = @{1}
WHERE item_id = @{2};", changingId, Updater, changedId);

                                    await dao.ExecNonQueryAsync(@"
UPDATE mst_item
SET sort_order = @{0}, update_time = 'now', updater = @{1}
WHERE item_id = @{2};", tmpOrder, Updater, changingId);
                                });
                            }
                            break;
                    }
                }
            }
            else { // 分類を跨いで項目の表示順を変更するとき
                Debug.Assert(this.WVM.SelectedItemVM.Kind == HierarchicalKind.Item);
                var grandparentVM = parentVM.ParentVM;
                int index2 = grandparentVM.ChildrenVMList.IndexOf(parentVM);
                int toCategoryId = grandparentVM.ChildrenVMList[index2 + 1].Id;

                using (DaoBase dao = this.builder.Build()) {
                    await dao.ExecTransactionAsync(async () => {
                        // 種別IDを更新する
                        await dao.ExecNonQueryAsync(@"
UPDATE mst_item
SET category_id = @{0}, update_time = 'now', updater = @{1}
WHERE item_id = @{2};", toCategoryId, Updater, changingId);

                        // 移動対象のソート順を取得する
                        int tmpOrder = -1;
                        DaoReader reader = await dao.ExecQueryAsync(@"
SELECT sort_order
FROM mst_item
WHERE item_id = @{0};", changingId);
                        reader.ExecARow((record) => {
                            tmpOrder = record.ToInt("sort_order");
                        });
                        foreach (var vm in grandparentVM.ChildrenVMList[index2 + 1].ChildrenVMList) {
                            int changedId = vm.Id;

                            // ソート順が若ければ、移動対象のソート順を移動先のソート順に変更する
                            int num = await dao.ExecNonQueryAsync(@"
UPDATE mst_item
SET sort_order = (SELECT sort_order FROM mst_item WHERE item_id = @{0}), update_time = 'now', updater = @{1}
WHERE item_id = @{2} AND (SELECT sort_order FROM mst_item WHERE item_id = @{0}) < @{3};", changedId, Updater, changingId, tmpOrder);

                            if (num == 0) { break; }

                            // 移動先のソート順を移動対象のソート順に変更する
                            await dao.ExecNonQueryAsync(@"
UPDATE mst_item
SET sort_order = @{0}, update_time = 'now', updater = @{1}
WHERE item_id = @{2};", tmpOrder, Updater, changedId);
                            changingId = changedId;
                        }
                    });
                }
            }

            await this.UpdateItemSettingsTabDataAsync(this.WVM.SelectedItemVM.Kind, this.WVM.SelectedItemVM.Id);
            this.needToUpdate = true;
        }

        /// <summary>
        /// 項目の情報を保存できるか判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveItemInfoCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedItemVM != null && this.WVM.SelectedItemVM.Kind != HierarchicalKind.Balance &&
                !string.IsNullOrWhiteSpace(this.WVM.SelectedItemVM.Name);
        }

        /// <summary>
        /// 項目の情報を保存する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveItemInfoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            HierarchicalKind kind = this.WVM.SelectedItemVM.Kind;
            int id = this.WVM.SelectedItemVM.Id;

            using (DaoBase dao = this.builder.Build()) {
                switch (kind) {
                    case HierarchicalKind.Category: {
                            dao.ExecNonQueryAsync(@"
UPDATE mst_category
SET category_name = @{0}, update_time = 'now', updater = @{1}
WHERE category_id = @{2};", this.WVM.SelectedItemVM.Name, Updater, id);
                        }
                        break;
                    case HierarchicalKind.Item: {
                            dao.ExecNonQueryAsync(@"
UPDATE mst_item
SET item_name = @{0}, update_time = 'now', updater = @{1}
WHERE item_id = @{2};", this.WVM.SelectedItemVM.Name, Updater, id);
                        }
                        break;
                }
            }

            MessageBox.Show(MessageText.FinishToSave, MessageTitle.Information, MessageBoxButton.OK, MessageBoxImage.Information);
            this.needToUpdate = true;
        }

        /// <summary>
        /// 項目-帳簿の関係を変更する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ChangeItemRelationCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.Assert(this.WVM.SelectedItemVM.Kind == HierarchicalKind.Item);

            HierarchicalSettingViewModel vm = this.WVM.SelectedItemVM;
            vm.SelectedRelationVM = (e.OriginalSource as CheckBox)?.DataContext as RelationViewModel;
            vm.SelectedRelationVM.IsRelated = !vm.SelectedRelationVM.IsRelated; // 選択前の状態に戻す

            using (DaoBase dao = this.builder.Build()) {
                await dao.ExecTransactionAsync(async () => {
                    DaoReader reader = await dao.ExecQueryAsync(@"
SELECT *
FROM rel_book_item
WHERE item_id = @{0} AND book_id = @{1};", vm.Id, vm.SelectedRelationVM.Id);

                    if (reader.Count == 0) {
                        await dao.ExecNonQueryAsync(@"
INSERT INTO rel_book_item (book_id, item_id, del_flg, insert_time, inserter, update_time, updater)
VALUES (@{0}, @{1}, 0, 'now', @{2}, 'now', @{3});", vm.SelectedRelationVM.Id, vm.Id, Inserter, Updater);
                        vm.SelectedRelationVM.IsRelated = !vm.SelectedRelationVM.IsRelated;
                    }
                    else {
                        reader = await dao.ExecQueryAsync(@"
SELECT *
FROM hst_action
WHERE book_id = @{0} AND item_id = @{1} AND del_flg = 0;", vm.SelectedRelationVM.Id, vm.Id);

                        if (reader.Count != 0) {
                            MessageBox.Show("帳簿内の該当する項目に帳簿項目が存在するので削除できません。"); // TODO
                            e.Handled = true;
                            return;
                        }

                        vm.SelectedRelationVM.IsRelated = !vm.SelectedRelationVM.IsRelated;
                        await dao.ExecNonQueryAsync(@"
UPDATE rel_book_item
SET del_flg = @{0}, update_time = 'now', updater = @{1}
WHERE item_id = @{2} AND book_id = @{3};", vm.SelectedRelationVM.IsRelated ? 0 : 1, Updater, vm.Id, vm.SelectedRelationVM.Id);
                    }
                });
            }
            this.needToUpdate = true;
        }

        /// <summary>
        /// 店名を削除できるか判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteShopNameCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedItemVM?.SelectedShopVM != null;
        }

        /// <summary>
        /// 店名を削除する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DeleteShopNameCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show(MessageText.DeleteNotification, MessageTitle.Information, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                Debug.Assert(this.WVM.SelectedItemVM.Kind == HierarchicalKind.Item);

                using (DaoBase dao = this.builder.Build()) {
                    await dao.ExecQueryAsync(@"
UPDATE hst_shop SET del_flg = 1, update_time = 'now', updater = @{0}
WHERE shop_name = @{1} AND item_id = @{2};", Updater, this.WVM.SelectedItemVM.SelectedShopVM.Name, this.WVM.SelectedItemVM.Id);

                    // 店舗名を更新する
                    this.WVM.SelectedItemVM.ShopVMList = await this.LoadShopViewModelListAsync(dao, this.WVM.SelectedItemVM.Id);
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
            e.CanExecute = this.WVM.SelectedItemVM?.SelectedRemarkVM != null;
        }

        /// <summary>
        /// 備考を削除する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DeleteRemarkCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show(MessageText.DeleteNotification, MessageTitle.Information, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                Debug.Assert(this.WVM.SelectedItemVM.Kind == HierarchicalKind.Item);

                using (DaoBase dao = this.builder.Build()) {
                    await dao.ExecQueryAsync(@"
UPDATE hst_remark SET del_flg = 1, update_time = 'now', updater = @{0}
WHERE remark = @{1} AND item_id = @{2};", Updater, this.WVM.SelectedItemVM.SelectedRemarkVM.Remark, this.WVM.SelectedItemVM.Id);

                    // 備考欄の表示を更新する
                    this.WVM.SelectedItemVM.RemarkVMList = await this.LoadRemarkViewModelListAsync(dao, this.WVM.SelectedItemVM.Id);
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
            int bookId = -1;
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader = await dao.ExecQueryAsync(@"
INSERT INTO mst_book (book_name, book_kind, pay_day, initial_value, sort_order, del_flg, update_time, updater, insert_time, inserter)
VALUES ('(no name)', 0, null, 0, (SELECT COALESCE(MAX(sort_order) + 1, 1) FROM mst_book), 0, 'now', @{0}, 'now', @{1})
RETURNING book_id;", Updater, Inserter);

                reader.ExecARow((record) => {
                    bookId = record.ToInt("book_id");
                });
            }

            await this.UpdateBookSettingTabDataAsync(bookId);
            this.needToUpdate = true;
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
            using (DaoBase dao = this.builder.Build()) {
                await dao.ExecTransactionAsync(async () => {
                    DaoReader reader = await dao.ExecQueryAsync(@"
SELECT * FROM hst_action
WHERE book_id = @{0} AND del_flg = 0;", this.WVM.SelectedBookVM.Id);

                    if (reader.Count != 0) {
                        MessageBox.Show("帳簿内に帳簿項目が存在するので削除できません。"); // TODO
                        return;
                    }

                    await dao.ExecNonQueryAsync(@"
UPDATE mst_book
SET del_flg = 1, update_time = 'now', updater = @{0}
WHERE book_id = @{1};", Updater, this.WVM.SelectedBookVM.Id);
                });
            }

            await this.UpdateBookSettingTabDataAsync();
            this.needToUpdate = true;
        }

        /// <summary>
        /// 帳簿の表示順を上げれるか判定
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
            int index = this.WVM.BookVMList.IndexOf(this.WVM.SelectedBookVM);
            int changedId = this.WVM.BookVMList[index - 1].Id.Value;
            int changingId = this.WVM.BookVMList[index].Id.Value;

            using (DaoBase dao = this.builder.Build()) {
                await dao.ExecTransactionAsync(async () => {
                    DaoReader reader = await dao.ExecQueryAsync(@"
SELECT sort_order
FROM mst_book
WHERE book_id = @{0};", changedId);

                    int tmpOrder = -1;
                    reader.ExecARow((record) => {
                        tmpOrder = record.ToInt("sort_order");
                    });

                    await dao.ExecNonQueryAsync(@"
UPDATE mst_book
SET sort_order = (SELECT sort_order FROM mst_book WHERE book_id = @{0}), update_time = 'now', updater = @{1}
WHERE book_id = @{2};", changingId, Updater, changedId);

                    await dao.ExecNonQueryAsync(@"
UPDATE mst_book
SET sort_order = @{0}, update_time = 'now', updater = @{1}
WHERE book_id = @{2};", tmpOrder, Updater, changingId);
                });
            }

            await this.UpdateBookSettingTabDataAsync(changingId);
            this.needToUpdate = true;
        }

        /// <summary>
        /// 帳簿の表示順を下げれるか判定
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
            int index = this.WVM.BookVMList.IndexOf(this.WVM.SelectedBookVM);
            int changedId = this.WVM.BookVMList[index + 1].Id.Value;
            int changingId = this.WVM.BookVMList[index].Id.Value;

            using (DaoBase dao = this.builder.Build()) {
                await dao.ExecTransactionAsync(async () => {
                    DaoReader reader = await dao.ExecQueryAsync(@"
SELECT sort_order
FROM mst_book
WHERE book_id = @{0};", changedId);

                    int tmpOrder = -1;
                    reader.ExecARow((record) => {
                        tmpOrder = record.ToInt("sort_order");
                    });

                    await dao.ExecNonQueryAsync(@"
UPDATE mst_book
SET sort_order = (SELECT sort_order FROM mst_book WHERE book_id = @{0}), update_time = 'now', updater = @{1}
WHERE book_id = @{2};", changingId, Updater, changedId);

                    await dao.ExecNonQueryAsync(@"
UPDATE mst_book
SET sort_order = @{0}, update_time = 'now', updater = @{1}
WHERE book_id = @{2};", tmpOrder, Updater, changingId);
                });
            }

            await this.UpdateBookSettingTabDataAsync(changingId);
            this.needToUpdate = true;
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
        /// CSVフォルダパスを指定するダイアログを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CsvFolderPathDialogCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;

            string folderPath = Path.GetDirectoryName(settings.App_CsvFilePath);
            string fileName = string.Empty;
            if (this.WVM.SelectedBookVM.CsvFolderPath != null && this.WVM.SelectedBookVM.CsvFolderPath != string.Empty) {
                folderPath = Path.GetDirectoryName(this.WVM.SelectedBookVM.CsvFolderPath);
                fileName = Path.GetFileName(this.WVM.SelectedBookVM.CsvFolderPath);
            }

            CommonOpenFileDialog ofd = new CommonOpenFileDialog() {
                EnsureFileExists = true,
                IsFolderPicker = true,
                InitialDirectory = folderPath,
                DefaultFileName = fileName,
                Title = "CSVフォルダ選択"
            };

            if (ofd.ShowDialog() == CommonFileDialogResult.Ok) {
                this.WVM.SelectedBookVM.CsvFolderPath = Path.Combine(ofd.InitialDirectory, ofd.FileName);
            }
        }

        /// <summary>
        /// 帳簿の情報を保存する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SaveBookInfoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (DaoBase dao = this.builder.Build()) {
                BookSettingViewModel vm = this.WVM.SelectedBookVM;
                MstBookJsonObject jsonObj = new MstBookJsonObject() {
                    StartDate = vm.StartDateExists ? (DateTime?)vm.StartDate : null,
                    EndDate = vm.EndDateExists ? (DateTime?)vm.EndDate : null,
                    CsvFolderPath = vm.CsvFolderPath != string.Empty ? vm.CsvFolderPath : null,
                    CsvActDateIndex = vm.ActDateIndex,
                    CsvOutgoIndex = vm.OutgoIndex,
                    CsvItemNameIndex = vm.ItemNameIndex
                };
                string jsonCode = JsonConvert.SerializeObject(jsonObj);

                await dao.ExecNonQueryAsync(@"
UPDATE mst_book
SET book_name = @{0}, book_kind = @{1}, initial_value = @{2}, debit_book_id = @{3}, pay_day = @{4}, json_code = @{5}, update_time = 'now', updater = @{6}
WHERE book_id = @{7};", vm.Name, (int)vm.SelectedBookKind, vm.InitialValue, vm.SelectedDebitBookVM.Id == -1 ? null : vm.SelectedDebitBookVM.Id, vm.PayDay, jsonCode, Updater, vm.Id);
            }

            MessageBox.Show(MessageText.FinishToSave, MessageTitle.Information, MessageBoxButton.OK, MessageBoxImage.Information);
            this.needToUpdate = true;
        }

        /// <summary>
        /// 帳簿-項目の関係を変更する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ChangeBookRelationCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            BookSettingViewModel vm = this.WVM.SelectedBookVM;
            vm.SelectedRelationVM = (e.OriginalSource as CheckBox)?.DataContext as RelationViewModel;
            vm.SelectedRelationVM.IsRelated = !vm.SelectedRelationVM.IsRelated; // 選択前の状態に戻す

            using (DaoBase dao = this.builder.Build()) {
                await dao.ExecTransactionAsync(async () => {
                    DaoReader reader = await dao.ExecQueryAsync(@"
SELECT *
FROM rel_book_item
WHERE book_id = @{0} AND item_id = @{1};", vm.Id, vm.SelectedRelationVM.Id);

                    if (reader.Count == 0) {
                        await dao.ExecNonQueryAsync(@"
INSERT INTO rel_book_item (book_id, item_id, del_flg, insert_time, inserter, update_time, updater)
VALUES (@{0}, @{1}, 0, 'now', @{2}, 'now', @{3});", vm.Id, vm.SelectedRelationVM.Id, Inserter, Updater);
                        vm.SelectedRelationVM.IsRelated = !vm.SelectedRelationVM.IsRelated;
                    }
                    else {
                        reader = await dao.ExecQueryAsync(@"
SELECT *
FROM hst_action
WHERE book_id = @{0} AND item_id = @{1} AND del_flg = 0;", vm.Id, vm.SelectedRelationVM.Id);

                        if (reader.Count != 0) {
                            MessageBox.Show("帳簿内の該当する項目に帳簿項目が存在するので削除できません。"); // TODO
                            e.Handled = true;
                            return;
                        }

                        vm.SelectedRelationVM.IsRelated = !vm.SelectedRelationVM.IsRelated;
                        await dao.ExecNonQueryAsync(@"
UPDATE rel_book_item
SET del_flg = @{0}, update_time = 'now', updater = @{1}
WHERE book_id = @{2} AND item_id = @{3};", vm.SelectedRelationVM.IsRelated ? 0 : 1, Updater, vm.Id, vm.SelectedRelationVM.Id);
                    }
                });
            }
            this.needToUpdate = true;
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
                Title = "ファイル選択",
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
                Title = "ファイル選択",
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
            if (MessageBox.Show(MessageText.RestartNotification, this.Title, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                Properties.Settings.Default.App_InitFlag = true;
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
                Title = "バックアップフォルダ選択"
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
            if (MessageBox.Show(MessageText.RestartNotification, this.Title, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
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
                this.Cursor = Cursors.Wait;

                switch (this.WVM.SelectedTab) {
                    case SettingsTabs.ItemSettingsTab:
                        await this.UpdateItemSettingsTabDataAsync(this.WVM.SelectedItemVM?.Kind, this.WVM.SelectedItemVM?.Id);
                        break;
                    case SettingsTabs.BookSettingsTab:
                        await this.UpdateBookSettingTabDataAsync(this.WVM.SelectedBookVM?.Id);
                        break;
                    case SettingsTabs.OtherSettingsTab:
                        this.UpdateOtherSettingTabData();
                        break;
                }
                this.Cursor = null;
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
        /// <param name="id">選択対象のID</param>
        /// <param name="bookId">選択対象の帳簿ID</param>
        private async Task UpdateSettingWindowDataAsync(HierarchicalKind? kind = null, int? id = null, int? bookId = null)
        {
            Log.Info();

            await this.UpdateItemSettingsTabDataAsync(kind, id);
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

                this.WVM.HierachicalSettingVMList = await this.LoadItemViewModelListAsync();

                HierarchicalSettingViewModel selectedVM = null;
                if (kind != null && id != null) {
                    // 収支から探す
                    IEnumerable<HierarchicalSettingViewModel> query = this.WVM.HierachicalSettingVMList.Where((vm) => { return vm.Kind == kind && vm.Id == id; });
                    if (query.Count() == 0) {
                        // 分類から探す
                        foreach (HierarchicalSettingViewModel tmpVM in this.WVM.HierachicalSettingVMList) {
                            query = tmpVM.ChildrenVMList.Where((vm) => { return vm.Kind == kind && vm.Id == id; });
                            if (query.Count() != 0) { break; }
                        }

                        if (query.Count() == 0) {
                            // 項目から探す
                            foreach (HierarchicalSettingViewModel tmpVM in this.WVM.HierachicalSettingVMList) {
                                foreach (HierarchicalSettingViewModel tmpVM2 in tmpVM.ChildrenVMList) {
                                    query = tmpVM2.ChildrenVMList.Where((vm) => { return vm.Kind == kind && vm.Id == id; });
                                    if (query.Count() != 0) { break; }
                                }
                                if (query.Count() != 0) { break; }
                            }
                        }
                    }

                    selectedVM = query.Count() != 0 ? query.First() : null;
                }

                // 何も選択されていないなら1番上の項目を選択する
                if (this.WVM.SelectedItemVM == null && this.WVM.HierachicalSettingVMList.Count != 0) {
                    selectedVM = this.WVM.HierachicalSettingVMList[0];
                }
                this.WVM.SelectedItemVM = selectedVM;
            }
        }

        /// <summary>
        /// 帳簿設定タブに表示するデータを更新する
        /// </summary>
        /// <param name="bookId">選択対象の帳簿ID</param>
        private async Task UpdateBookSettingTabDataAsync(int? bookId = null)
        {
            if (this.WVM.SelectedTab == SettingsTabs.BookSettingsTab) {
                Log.Info();

                this.WVM.BookVMList = await this.LoadBookSettingViewModelListAsync();

                BookSettingViewModel tmpVM = null;
                if (bookId != null) {
                    IEnumerable<BookSettingViewModel> query = this.WVM.BookVMList.Where((vm) => { return vm.Id == bookId; });
                    tmpVM = query.Count() != 0 ? query.First() : null;
                }
                
                // 何も選択されていないなら1番上の項目を選択する
                if (tmpVM == null && this.WVM.BookVMList.Count != 0) {
                    tmpVM = this.WVM.BookVMList[0];
                }
                this.WVM.SelectedBookVM = tmpVM;
            }
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
        private async Task<ObservableCollection<HierarchicalSettingViewModel>> LoadItemViewModelListAsync()
        {
            Log.Info();

            ObservableCollection<HierarchicalSettingViewModel> vmList = new ObservableCollection<HierarchicalSettingViewModel>();
            HierarchicalSettingViewModel incomeVM = new HierarchicalSettingViewModel() {
                Kind = HierarchicalKind.Balance,
                Id = (int)BalanceKind.Income,
                SortOrder = -1,
                Name = "収入項目",
                ParentVM = null,
                RelationVMList = null,
                ChildrenVMList = new ObservableCollection<HierarchicalSettingViewModel>()
            };
            vmList.Add(incomeVM);

            HierarchicalSettingViewModel outgoVM = new HierarchicalSettingViewModel() {
                Kind = HierarchicalKind.Balance,
                Id = (int)BalanceKind.Outgo,
                SortOrder = -1,
                Name = "支出項目",
                ParentVM = null,
                RelationVMList = null,
                ChildrenVMList = new ObservableCollection<HierarchicalSettingViewModel>()
            };
            vmList.Add(outgoVM);

            foreach (HierarchicalSettingViewModel vm in vmList) {
                // 分類
                using (DaoBase dao = this.builder.Build()) {
                    DaoReader reader = await dao.ExecQueryAsync(@"
SELECT category_id, category_name, sort_order 
FROM mst_category
WHERE balance_kind = @{0} AND del_flg = 0 AND sort_order <> 0
ORDER BY sort_order;", vm.Id);

                    reader.ExecWholeRow((count, record) => {
                        int categoryId = record.ToInt("category_id");
                        int sortOrder = record.ToInt("sort_order");
                        string categoryName = record["category_name"];

                        vm.ChildrenVMList.Add(new HierarchicalSettingViewModel() {
                            Kind = HierarchicalKind.Category,
                            Id = categoryId,
                            SortOrder = sortOrder,
                            Name = categoryName,
                            ParentVM = vm,
                            RelationVMList = null,
                            ChildrenVMList = new ObservableCollection<HierarchicalSettingViewModel>()
                        });
                        return true;
                    });

                    foreach (HierarchicalSettingViewModel categocyVM in vm.ChildrenVMList) {
                        // 項目
                        reader = await dao.ExecQueryAsync(@"
SELECT item_id, item_name, sort_order
FROM mst_item
WHERE category_id = @{0} AND del_flg = 0
ORDER BY sort_order;", categocyVM.Id);

                        reader.ExecWholeRow((count, record) => {
                            int itemId = record.ToInt("item_id");
                            int sortOrder = record.ToInt("sort_order");
                            string itemName = record["item_name"];

                            categocyVM.ChildrenVMList.Add(new HierarchicalSettingViewModel() {
                                Kind = HierarchicalKind.Item,
                                Id = itemId,
                                SortOrder = sortOrder,
                                Name = itemName,
                                ParentVM = categocyVM,
                                RelationVMList = new ObservableCollection<RelationViewModel>(),
                                ShopVMList = new ObservableCollection<ShopViewModel>(),
                                RemarkVMList = new ObservableCollection<RemarkViewModel>()
                            });
                            return true;
                        });

                        foreach (HierarchicalSettingViewModel itemVM in categocyVM.ChildrenVMList) {
                            reader = await dao.ExecQueryAsync(@"
SELECT B.book_id AS book_id, B.book_name, RBI.book_id IS NULL AS is_not_related
FROM mst_book B
LEFT JOIN (SELECT book_id FROM rel_book_item WHERE del_flg = 0 AND item_id = @{0}) RBI ON RBI.book_id = B.book_id
WHERE del_flg = 0
ORDER BY B.sort_order;", itemVM.Id);

                            reader.ExecWholeRow((count2, record2) => {
                                int bookId = record2.ToInt("book_id");
                                string bookName = record2["book_name"];
                                bool isRelated = !record2.ToBoolean("is_not_related");

                                itemVM.RelationVMList.Add(new RelationViewModel() {
                                    Id = bookId,
                                    Name = bookName,
                                    IsRelated = isRelated
                                });
                                return true;
                            });

                            // 店舗名の一覧を取得する
                            itemVM.ShopVMList = await this.LoadShopViewModelListAsync(dao, itemVM.Id);
                            // 備考の一覧を取得する
                            itemVM.RemarkVMList = await this.LoadRemarkViewModelListAsync(dao, itemVM.Id);
                        }
                    }
                }
            }

            return vmList;
        }

        /// <summary>
        /// 帳簿VM(設定用)リストを取得する
        /// </summary>
        /// <returns>帳簿VM(設定用)リスト</returns>
        private async Task<ObservableCollection<BookSettingViewModel>> LoadBookSettingViewModelListAsync()
        {
            Log.Info();

            ObservableCollection<BookSettingViewModel> settingVMList = new ObservableCollection<BookSettingViewModel>();
            ObservableCollection<BookViewModel> vmList = new ObservableCollection<BookViewModel>() {
                new BookViewModel(){ Id = -1, Name = "なし" }
            };

            using (DaoBase dao = this.builder.Build()) {
                // 帳簿一覧を取得する(支払元選択用)
                DaoReader reader = await dao.ExecQueryAsync(@"
SELECT book_id, book_name
FROM mst_book
WHERE del_flg = 0
ORDER BY sort_order;");

                reader.ExecWholeRow((count, record) => {
                    int bookId = record.ToInt("book_id");
                    string bookName = record["book_name"];

                    vmList.Add(new BookViewModel() {
                        Id = bookId,
                        Name = bookName
                    });
                    return true;
                });

                // 帳簿一覧を取得する
                reader = await dao.ExecQueryAsync(@"
SELECT B.book_id, B.book_name, B.book_kind, B.debit_book_id, B.pay_day, B.initial_value, B.json_code, B.sort_order, MIN(A.act_time) AS start_date, MAX(A.act_time) AS end_date
FROM mst_book B
INNER JOIN hst_action A ON A.book_id = B.book_id AND A.del_flg = 0
WHERE B.del_flg = 0
GROUP BY B.book_id
ORDER BY B.sort_order;");

                reader.ExecWholeRow((count, record) => {
                    int bookId = record.ToInt("book_id");
                    int sortOrder = record.ToInt("sort_order");
                    string bookName = record["book_name"];
                    BookKind bookKind = (BookKind)record.ToInt("book_kind");
                    int initialValue = record.ToInt("initial_value");
                    int? debitBookId = record.ToNullableInt("debit_book_id");
                    int? payDay = record.ToNullableInt("pay_day");
                    DateTime startDate = record.ToDateTime("start_date");
                    DateTime endDate = record.ToDateTime("end_date");

                    string jsonCode = record["json_code"];
                    MstBookJsonObject jsonObj = JsonConvert.DeserializeObject<MstBookJsonObject>(jsonCode);

                    BookSettingViewModel tmpVM = new BookSettingViewModel() {
                        Id = bookId,
                        SortOrder = sortOrder,
                        Name = bookName,
                        SelectedBookKind = bookKind,
                        InitialValue = initialValue,
                        DebitBookVMList = new ObservableCollection<BookViewModel>(vmList.Where((vm) => { return vm.Id != bookId; })),
                        PayDay = payDay,
                        StartDateExists = jsonObj?.StartDate != null,
                        StartDate = jsonObj?.StartDate ?? startDate,
                        EndDateExists = jsonObj?.EndDate != null,
                        EndDate = jsonObj?.EndDate ?? endDate,
                        CsvFolderPath = jsonObj?.CsvFolderPath,
                        ActDateIndex = jsonObj?.CsvActDateIndex,
                        OutgoIndex = jsonObj?.CsvOutgoIndex,
                        ItemNameIndex = jsonObj?.CsvItemNameIndex,
                    };
                    tmpVM.SelectedDebitBookVM = tmpVM.DebitBookVMList.FirstOrDefault((vm) => { return vm.Id == debitBookId; }) ?? tmpVM.DebitBookVMList[0];

                    settingVMList.Add(tmpVM);
                    return true;
                });

                // 項目との関係の一覧を取得する(移動を除く)
                foreach (BookSettingViewModel vm in settingVMList) {
                    reader = await dao.ExecQueryAsync(@"
SELECT I.item_id AS item_id, I.item_name AS item_name, C.category_name AS category_name, RBI.item_id IS NULL AS is_not_related
FROM mst_item I
INNER JOIN (SELECT category_id, category_name FROM mst_category WHERE del_flg = 0) C ON C.category_id = I.category_id
LEFT JOIN (SELECT item_id FROM rel_book_item WHERE del_flg = 0 AND book_id = @{0}) RBI ON RBI.item_id = I.item_id
WHERE del_flg = 0 AND move_flg = 0
ORDER BY I.sort_order;", vm.Id);

                    vm.RelationVMList = new ObservableCollection<RelationViewModel>();
                    reader.ExecWholeRow((count, record) => {
                        int itemId = record.ToInt("item_id");
                        string name = string.Format(@"{0} > {1}", record["category_name"], record["item_name"]);
                        bool isRelated = !record.ToBoolean("is_not_related");

                        vm.RelationVMList.Add(new RelationViewModel() {
                            Id = itemId,
                            Name = name,
                            IsRelated = isRelated
                        });
                        return true;
                    });
                }
            }

            return settingVMList;
        }
        
        /// <summary>
        /// 店舗VMリストを取得する
        /// </summary>
        /// <param name="dao">DAO</param>
        /// <param name="itemId">項目ID</param>
        /// <returns></returns>
        private async Task<ObservableCollection<ShopViewModel>> LoadShopViewModelListAsync(DaoBase dao, int itemId)
        {
            ObservableCollection<ShopViewModel> svmList = new ObservableCollection<ShopViewModel>();
            DaoReader reader = await dao.ExecQueryAsync(@"
SELECT S.shop_name, COUNT(A.shop_name) AS shop_count, COALESCE(MAX(A.act_time), '1970-01-01') AS sort_time, COALESCE(MAX(A.act_time), null) AS used_time
FROM hst_shop S
LEFT OUTER JOIN (SELECT * FROM hst_action WHERE del_flg = 0) A ON A.shop_name = S.shop_name AND A.item_id = S.item_id
WHERE S.del_flg = 0 AND S.item_id = @{0}
GROUP BY S.shop_name
ORDER BY sort_time DESC, shop_count DESC;", itemId);
            reader.ExecWholeRow((count, record) => {
                ShopViewModel svm = new ShopViewModel() { Name = record["shop_name"], UsedCount = record.ToInt("shop_count"), UsedTime = record.ToNullableDateTime("used_time") };
                svmList.Add(svm);
                return true;
            });
            return svmList;
        }

        /// <summary>
        /// 備考VMリストを取得する
        /// </summary>
        /// <param name="dao">DAO</param>
        /// <param name="itemId">項目ID</param>
        /// <returns></returns>
        private async Task<ObservableCollection<RemarkViewModel>> LoadRemarkViewModelListAsync(DaoBase dao, int itemId)
        {
            ObservableCollection<RemarkViewModel> rvmList = new ObservableCollection<RemarkViewModel>();
            DaoReader reader = await dao.ExecQueryAsync(@"
SELECT R.remark, COUNT(A.remark) AS remark_count, COALESCE(MAX(A.act_time), '1970-01-01') AS sort_time, COALESCE(MAX(A.act_time), null) AS used_time
FROM hst_remark R
LEFT OUTER JOIN (SELECT * FROM hst_action WHERE del_flg = 0) A ON A.remark = R.remark AND A.item_id = R.item_id
WHERE R.del_flg = 0 AND R.item_id = @{0}
GROUP BY R.remark
ORDER BY sort_time DESC, remark_count DESC;", itemId);
            reader.ExecWholeRow((count, record) => {
                RemarkViewModel rvm = new RemarkViewModel() { Remark = record["remark"], UsedCount = record.ToInt("remark_count"), UsedTime = record.ToNullableDateTime("used_time") };
                rvmList.Add(rvm);
                return true;
            });
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
    }
}

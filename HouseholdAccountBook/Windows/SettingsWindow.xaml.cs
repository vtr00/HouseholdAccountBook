using HouseholdAccountBook.Dao;
using HouseholdAccountBook.Dto;
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static HouseholdAccountBook.ConstValue.ConstValue;
using static HouseholdAccountBook.ViewModels.HierarchicalItemViewModel;

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
        private DaoBuilder builder;
        /// <summary>
        /// 前回選択していた設定タブ
        /// </summary>
        private SettingsTabs oldSelectedSettingsTab = SettingsTabs.ItemSettingsTab;
        /// <summary>
        /// DBに保存されたか
        /// </summary>
        private bool IsSaved = false;
        #endregion

        /// <summary>
        /// <see cref="SettingsWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="builder">DAOビルダ</param>
        public SettingsWindow(DaoBuilder builder)
        {
            this.builder = builder;

            InitializeComponent();

            UpdateSettingWindowData();

            LoadSetting();
        }

        #region イベントハンドラ
        #region コマンド
        #region ウィンドウ
        /// <summary>
        /// ウィンドウを閉じる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region 項目設定の操作
        /// <summary>
        /// カテゴリを追加可能か判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddCategoryCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedItemVM != null && this.WVM.SelectedItemVM.Kind != HierarchicalKind.Item;
        }

        /// <summary>
        /// カテゴリを追加する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddCategoryCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            HierarchicalItemViewModel vm = this.WVM.SelectedItemVM;
            while (vm.Kind != HierarchicalKind.Balance) {
                vm = vm.ParentVM;
            }
            BalanceKind kind = (BalanceKind)vm.Id;

            int categoryId = -1;
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader = dao.ExecQuery(@"
INSERT INTO mst_category (category_name, balance_kind, sort_order, del_flg, update_time, updater, insert_time, inserter)
VALUES ('(no name)', @{0}, (SELECT COALESCE(MAX(sort_order) + 1, 1) FROM mst_category), 0, 'now', @{1}, 'now', @{2})
RETURNING category_id;", (int)kind, Updater, Inserter);

                reader.ExecARow((record) => {
                    categoryId = record.ToInt("category_id");
                });
            }
            UpdateItemSettingsTabData(HierarchicalKind.Category, categoryId);
            this.IsSaved = true;
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
        private void AddItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            HierarchicalItemViewModel vm = this.WVM.SelectedItemVM;
            while (vm.Kind != HierarchicalKind.Category) {
                vm = vm.ParentVM;
            }
            int categoryId = vm.Id;

            int itemId = -1;
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader = dao.ExecQuery(@"
INSERT INTO mst_item (item_name, category_id, advance_flg, sort_order, del_flg, update_time, updater, insert_time, inserter)
VALUES ('(no name)', @{0}, 0, (SELECT COALESCE(MAX(sort_order) + 1, 1) FROM mst_item), 0, 'now', @{1}, 'now', @{2})
RETURNING item_id;", categoryId, Updater, Inserter);

                reader.ExecARow((record) => {
                    itemId = record.ToInt("item_id");
                });
            }
            UpdateItemSettingsTabData(HierarchicalKind.Item, itemId);
            this.IsSaved = true;
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
        private void DeleteItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            HierarchicalKind kind = this.WVM.SelectedItemVM.Kind;
            int id = this.WVM.SelectedItemVM.Id;

            using (DaoBase dao = this.builder.Build()) {
                switch (kind) {
                    case HierarchicalKind.Category: {
                            dao.ExecTransaction(() => {
                                DaoReader reader = dao.ExecQuery(@"
SELECT 1
FROM hst_action A
INNER JOIN (SELECT item_id FROM mst_item WHERE del_flg = 0 AND category_id = @{0}) I ON A.item_id = I.item_id
WHERE A.del_flg = 0;", id);

                                if (reader.Count != 0) {
                                    MessageBox.Show("分類内に帳簿項目が存在するので削除できません。"); // TODO
                                    return;
                                }

                                dao.ExecNonQuery(@"
UPDATE mst_category
SET del_flg = 1, update_time = 'now', updater = @{0}
WHERE category_id = @{1};", Updater, id);
                            });
                        }
                        break;
                    case HierarchicalKind.Item: {
                            dao.ExecTransaction(() => {
                                DaoReader reader = dao.ExecQuery(@"
SELECT *
FROM hst_action
WHERE del_flg = 0 AND item_id = @{0};", id);

                                if (reader.Count != 0) {
                                    MessageBox.Show("項目内に帳簿項目が存在するので削除できません。"); // TODO
                                    return;
                                }

                                dao.ExecNonQuery(@"
UPDATE mst_item
SET del_flg = 1, update_time = 'now', updater = @{0}
WHERE item_id = @{1};", Updater, id);
                            });
                        }
                        break;
                }
            }
            UpdateItemSettingsTabData(this.WVM.SelectedItemVM.ParentVM.Kind, this.WVM.SelectedItemVM.ParentVM.Id);
            this.IsSaved = true;
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
        private void RaiseItemSortOrderCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var parentVM = this.WVM.SelectedItemVM.ParentVM;
            int index = parentVM.ChildrenVMList.IndexOf(this.WVM.SelectedItemVM);
            int changingId = parentVM.ChildrenVMList[index].Id;
            if (0 < index) {
                int changedId = parentVM.ChildrenVMList[index - 1].Id;

                using (DaoBase dao = this.builder.Build()) {
                    switch (this.WVM.SelectedItemVM.Kind) {
                        case HierarchicalKind.Category: {
                                dao.ExecTransaction(() => {
                                    DaoReader reader = dao.ExecQuery(@"
SELECT sort_order
FROM mst_category
WHERE category_id = @{0};", changedId);

                                    int tmpOrder = -1;
                                    reader.ExecARow((record) => {
                                        tmpOrder = record.ToInt("sort_order");
                                    });

                                    dao.ExecNonQuery(@"
UPDATE mst_category
SET sort_order = (SELECT sort_order FROM mst_category WHERE category_id = @{0}), update_time = 'now', updater = @{1}
WHERE category_id = @{2};", changingId, Updater, changedId);

                                    dao.ExecNonQuery(@"
UPDATE mst_category
SET sort_order = @{0}, update_time = 'now', updater = @{1}
WHERE category_id = @{2};", tmpOrder, Updater, changingId);
                                });
                            }
                            break;
                        case HierarchicalKind.Item: {
                                dao.ExecTransaction(() => {
                                    DaoReader reader = dao.ExecQuery(@"
SELECT sort_order
FROM mst_item
WHERE item_id = @{0};", changedId);

                                    int tmpOrder = -1;
                                    reader.ExecARow((record) => {
                                        tmpOrder = record.ToInt("sort_order");
                                    });

                                    dao.ExecNonQuery(@"
UPDATE mst_item
SET sort_order = (SELECT sort_order FROM mst_item WHERE item_id = @{0}), update_time = 'now', updater = @{1}
WHERE item_id = @{2};", changingId, Updater, changedId);

                                    dao.ExecNonQuery(@"
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
                    dao.ExecNonQuery(@"
UPDATE mst_item
SET category_id = @{0}, sort_order = (SELECT COALESCE(MAX(sort_order) + 1, 1) FROM mst_item), update_time = 'now', updater = @{1}
WHERE item_id = @{2};", toCategoryId, Updater, changingId);
                }
            }

            UpdateItemSettingsTabData(this.WVM.SelectedItemVM.Kind, this.WVM.SelectedItemVM.Id);
            this.IsSaved = true;
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
        private void DropItemSortOrderCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var parentVM = this.WVM.SelectedItemVM.ParentVM;
            int index = parentVM.ChildrenVMList.IndexOf(this.WVM.SelectedItemVM);
            int changingId = parentVM.ChildrenVMList[index].Id;
            if (parentVM.ChildrenVMList.Count - 1 > index) {
                int changedId = parentVM.ChildrenVMList[index + 1].Id;

                using (DaoBase dao = this.builder.Build()) {
                    switch (this.WVM.SelectedItemVM.Kind) {
                        case HierarchicalKind.Category: {
                                dao.ExecTransaction(() => {
                                    DaoReader reader = dao.ExecQuery(@"
SELECT sort_order
FROM mst_category
WHERE category_id = @{0};", changedId);

                                    int tmpOrder = -1;
                                    reader.ExecARow((record) => {
                                        tmpOrder = record.ToInt("sort_order");
                                    });

                                    dao.ExecNonQuery(@"
UPDATE mst_category
SET sort_order = (SELECT sort_order FROM mst_category WHERE category_id = @{0}), update_time = 'now', updater = @{1}
WHERE category_id = @{2};", changingId, Updater, changedId);

                                    dao.ExecNonQuery(@"
UPDATE mst_category
SET sort_order = @{0}, update_time = 'now', updater = @{1}
WHERE category_id = @{2};", tmpOrder, Updater, changingId);
                                });
                            }
                            break;
                        case HierarchicalKind.Item: {
                                dao.ExecTransaction(() => {
                                    DaoReader reader = dao.ExecQuery(@"
SELECT sort_order
FROM mst_item
WHERE item_id = @{0};", changedId);

                                    int tmpOrder = -1;
                                    reader.ExecARow((record) => {
                                        tmpOrder = record.ToInt("sort_order");
                                    });

                                    dao.ExecNonQuery(@"
UPDATE mst_item
SET sort_order = (SELECT sort_order FROM mst_item WHERE item_id = @{0}), update_time = 'now', updater = @{1}
WHERE item_id = @{2};", changingId, Updater, changedId);

                                    dao.ExecNonQuery(@"
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
                    dao.ExecTransaction(() => {
                        // 種別IDを更新する
                        dao.ExecNonQuery(@"
UPDATE mst_item
SET category_id = @{0}, update_time = 'now', updater = @{1}
WHERE item_id = @{2};", toCategoryId, Updater, changingId);

                        // 移動対象のソート順を取得する
                        int tmpOrder = -1;
                        DaoReader reader = dao.ExecQuery(@"
SELECT sort_order
FROM mst_item
WHERE item_id = @{0};", changingId);
                        reader.ExecARow((record) => {
                            tmpOrder = record.ToInt("sort_order");
                        });
                        foreach (var vm in grandparentVM.ChildrenVMList[index2 + 1].ChildrenVMList) {
                            int changedId = vm.Id;

                            // ソート順が若ければ、移動対象のソート順を移動先のソート順に変更する
                            int num = dao.ExecNonQuery(@"
UPDATE mst_item
SET sort_order = (SELECT sort_order FROM mst_item WHERE item_id = @{0}), update_time = 'now', updater = @{1}
WHERE item_id = @{2} AND (SELECT sort_order FROM mst_item WHERE item_id = @{0}) < @{3};", changedId, Updater, changingId, tmpOrder);

                            if (num == 0) { break; }

                            // 移動先のソート順を移動対象のソート順に変更する
                            dao.ExecNonQuery(@"
UPDATE mst_item
SET sort_order = @{0}, update_time = 'now', updater = @{1}
WHERE item_id = @{2};", tmpOrder, Updater, changedId);
                            changingId = changedId;
                        }
                    });
                }
            }

            UpdateItemSettingsTabData(this.WVM.SelectedItemVM.Kind, this.WVM.SelectedItemVM.Id);
            this.IsSaved = true;
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
                            dao.ExecNonQuery(@"
UPDATE mst_category
SET category_name = @{0}, update_time = 'now', updater = @{1}
WHERE category_id = @{2};", this.WVM.SelectedItemVM.Name, Updater, id);
                        }
                        break;
                    case HierarchicalKind.Item: {
                            dao.ExecNonQuery(@"
UPDATE mst_item
SET item_name = @{0}, update_time = 'now', updater = @{1}
WHERE item_id = @{2};", this.WVM.SelectedItemVM.Name, Updater, id);
                        }
                        break;
                }
            }

            MessageBox.Show(MessageText.FinishToSave, MessageTitle.Information, MessageBoxButton.OK, MessageBoxImage.Information);
            this.IsSaved = true;
        }

        /// <summary>
        /// 項目-帳簿の関係を変更する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeItemRelationCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.Assert(this.WVM.SelectedItemVM.Kind == HierarchicalKind.Item);

            HierarchicalItemViewModel vm = this.WVM.SelectedItemVM;
            vm.SelectedRelationVM = (e.OriginalSource as CheckBox)?.DataContext as RelationViewModel;
            vm.SelectedRelationVM.IsRelated = !vm.SelectedRelationVM.IsRelated; // 選択前の状態に戻す

            using (DaoBase dao = this.builder.Build()) {
                dao.ExecTransaction(() => {
                    DaoReader reader = dao.ExecQuery(@"
SELECT *
FROM rel_book_item
WHERE item_id = @{0} AND book_id = @{1};", vm.Id, vm.SelectedRelationVM.Id);

                    if (reader.Count == 0) {
                        dao.ExecNonQuery(@"
INSERT INTO rel_book_item (book_id, item_id, del_flg, insert_time, inserter, update_time, updater)
VALUES (@{0}, @{1}, 0, 'now', @{2}, 'now', @{3});", vm.SelectedRelationVM.Id, vm.Id, Inserter, Updater);
                        vm.SelectedRelationVM.IsRelated = !vm.SelectedRelationVM.IsRelated;
                    }
                    else {
                        reader = dao.ExecQuery(@"
SELECT *
FROM hst_action
WHERE book_id = @{0} AND item_id = @{1} AND del_flg = 0;", vm.SelectedRelationVM.Id, vm.Id);

                        if (reader.Count != 0) {
                            MessageBox.Show("帳簿内の該当する項目に帳簿項目が存在するので削除できません。"); // TODO
                            e.Handled = true;
                            return;
                        }

                        vm.SelectedRelationVM.IsRelated = !vm.SelectedRelationVM.IsRelated;
                        dao.ExecNonQuery(@"
UPDATE rel_book_item
SET del_flg = @{0}, update_time = 'now', updater = @{1}
WHERE item_id = @{2} AND book_id = @{3};", vm.SelectedRelationVM.IsRelated ? 0 : 1, Updater, vm.Id, vm.SelectedRelationVM.Id);
                    }
                });
            }
            this.IsSaved = true;
        }

        /// <summary>
        /// 店名を削除できるか判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteShopNameCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedItemVM?.SelectedShopName != null;
        }

        /// <summary>
        /// 店名を削除する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteShopNameCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show(MessageText.DeleteNotification, MessageTitle.Information, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                Debug.Assert(this.WVM.SelectedItemVM.Kind == HierarchicalKind.Item);
                using (DaoBase dao = this.builder.Build()) {
                    dao.ExecQuery(@"
UPDATE hst_shop SET del_flg = 1, update_time = 'now', updater = @{0}
WHERE shop_name = @{1} AND item_id = @{2};", Updater, this.WVM.SelectedItemVM.SelectedShopName, this.WVM.SelectedItemVM.Id);

                    // 店舗名を更新する
                    DaoReader reader = dao.ExecQuery(@"
SELECT shop_name
FROM hst_shop
WHERE del_flg = 0 AND item_id = @{0}
ORDER BY used_time DESC;", this.WVM.SelectedItemVM.Id);

                    this.WVM.SelectedItemVM.ShopNameList.Clear();
                    reader.ExecWholeRow((count, record) => {
                        string shopName = record["shop_name"];

                        this.WVM.SelectedItemVM.ShopNameList.Add(shopName);
                        return true;
                    });
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
            e.CanExecute = this.WVM.SelectedItemVM?.SelectedRemark != null;
        }

        /// <summary>
        /// 備考を削除する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteRemarkCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show(MessageText.DeleteNotification, MessageTitle.Information, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                Debug.Assert(this.WVM.SelectedItemVM.Kind == HierarchicalKind.Item);
                using (DaoBase dao = this.builder.Build()) {
                    dao.ExecQuery(@"
UPDATE hst_remark SET del_flg = 1, update_time = 'now', updater = @{0}
WHERE remark = @{1} AND item_id = @{2};", Updater, this.WVM.SelectedItemVM.SelectedRemark, this.WVM.SelectedItemVM.Id);

                    // 備考欄の表示を更新する
                    DaoReader reader = dao.ExecQuery(@"
SELECT remark
FROM hst_remark
WHERE del_flg = 0 AND item_id = @{0}
ORDER BY used_time DESC;", this.WVM.SelectedItemVM.Id);

                    this.WVM.SelectedItemVM.RemarkList.Clear();
                    reader.ExecWholeRow((count, record) => {
                        string remark = record["remark"];

                        this.WVM.SelectedItemVM.RemarkList.Add(remark);
                        return true;
                    });
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
        private void AddBookCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            int bookId = -1;
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader = dao.ExecQuery(@"
INSERT INTO mst_book (book_name, book_kind, pay_day, initial_value, sort_order, del_flg, update_time, updater, insert_time, inserter)
VALUES ('(no name)', 0, null, 0, (SELECT COALESCE(MAX(sort_order) + 1, 1) FROM mst_book), 0, 'now', @{0}, 'now', @{1})
RETURNING book_id;", Updater, Inserter);

                reader.ExecARow((record) => {
                    bookId = record.ToInt("book_id");
                });
            }

            UpdateBookSettingTabData(bookId);
            this.IsSaved = true;
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
        private void DeleteBookCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (DaoBase dao = this.builder.Build()) {
                dao.ExecTransaction(() => {
                    DaoReader reader = dao.ExecQuery(@"
SELECT * FROM hst_action
WHERE book_id = @{0} AND del_flg = 0;", this.WVM.SelectedBookVM.Id);

                    if (reader.Count != 0) {
                        MessageBox.Show("帳簿内に帳簿項目が存在するので削除できません。"); // TODO
                        return;
                    }

                    dao.ExecNonQuery(@"
UPDATE mst_book
SET del_flg = 1, update_time = 'now', updater = @{0}
WHERE book_id = @{1};", Updater, this.WVM.SelectedBookVM.Id);
                });
            }

            UpdateBookSettingTabData();
            this.IsSaved = true;
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
        private void RaiseBookSortOrderCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            int index = this.WVM.BookVMList.IndexOf(this.WVM.SelectedBookVM);
            int changedId = this.WVM.BookVMList[index - 1].Id.Value;
            int changingId = this.WVM.BookVMList[index].Id.Value;

            using (DaoBase dao = this.builder.Build()) {
                dao.ExecTransaction(() => {
                    DaoReader reader = dao.ExecQuery(@"
SELECT sort_order
FROM mst_book
WHERE book_id = @{0};", changedId);

                    int tmpOrder = -1;
                    reader.ExecARow((record) => {
                        tmpOrder = record.ToInt("sort_order");
                    });

                    dao.ExecNonQuery(@"
UPDATE mst_book
SET sort_order = (SELECT sort_order FROM mst_book WHERE book_id = @{0}), update_time = 'now', updater = @{1}
WHERE book_id = @{2};", changingId, Updater, changedId);

                    dao.ExecNonQuery(@"
UPDATE mst_book
SET sort_order = @{0}, update_time = 'now', updater = @{1}
WHERE book_id = @{2};", tmpOrder, Updater, changingId);
                });
            }

            UpdateBookSettingTabData(changingId);
            this.IsSaved = true;
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
        private void DropBookSortOrderCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            int index = this.WVM.BookVMList.IndexOf(this.WVM.SelectedBookVM);
            int changedId = this.WVM.BookVMList[index + 1].Id.Value;
            int changingId = this.WVM.BookVMList[index].Id.Value;

            using (DaoBase dao = this.builder.Build()) {
                dao.ExecTransaction(() => {
                    DaoReader reader = dao.ExecQuery(@"
SELECT sort_order
FROM mst_book
WHERE book_id = @{0};", changedId);

                    int tmpOrder = -1;
                    reader.ExecARow((record) => {
                        tmpOrder = record.ToInt("sort_order");
                    });

                    dao.ExecNonQuery(@"
UPDATE mst_book
SET sort_order = (SELECT sort_order FROM mst_book WHERE book_id = @{0}), update_time = 'now', updater = @{1}
WHERE book_id = @{2};", changingId, Updater, changedId);

                    dao.ExecNonQuery(@"
UPDATE mst_book
SET sort_order = @{0}, update_time = 'now', updater = @{1}
WHERE book_id = @{2};", tmpOrder, Updater, changingId);
                });
            }

            UpdateBookSettingTabData(changingId);
            this.IsSaved = true;
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
        private void SaveBookInfoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (DaoBase dao = this.builder.Build()) {
                BookSettingViewModel vm = this.WVM.SelectedBookVM;
                MstBookJsonObject jsonObj = new MstBookJsonObject() {
                    CsvActDateIndex = vm.ActDateIndex, CsvOutgoIndex = vm.OutgoIndex, CsvItemNameIndex = vm.ItemNameIndex
                };
                string jsonCode = JsonConvert.SerializeObject(jsonObj);

                dao.ExecNonQuery(@"
UPDATE mst_book
SET book_name = @{0}, book_kind = @{1}, initial_value = @{2}, debit_book_id = @{3}, pay_day = @{4}, json_code = @{5}, update_time = 'now', updater = @{6}
WHERE book_id = @{7};", vm.Name, (int)vm.SelectedBookKind, vm.InitialValue, vm.SelectedDebitBookVM.Id == -1 ? null : vm.SelectedDebitBookVM.Id, vm.PayDay, jsonCode, Updater, vm.Id);
            }

            MessageBox.Show(MessageText.FinishToSave, MessageTitle.Information, MessageBoxButton.OK, MessageBoxImage.Information);
            this.IsSaved = true;
        }

        /// <summary>
        /// 帳簿-項目の関係を変更する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeBookRelationCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            BookSettingViewModel vm = this.WVM.SelectedBookVM;
            vm.SelectedRelationVM = (e.OriginalSource as CheckBox)?.DataContext as RelationViewModel;
            vm.SelectedRelationVM.IsRelated = !vm.SelectedRelationVM.IsRelated; // 選択前の状態に戻す

            using (DaoBase dao = this.builder.Build()) {
                dao.ExecTransaction(() => {
                    DaoReader reader = dao.ExecQuery(@"
SELECT *
FROM rel_book_item
WHERE book_id = @{0} AND item_id = @{1};", vm.Id, vm.SelectedRelationVM.Id);

                    if (reader.Count == 0) {
                        dao.ExecNonQuery(@"
INSERT INTO rel_book_item (book_id, item_id, del_flg, insert_time, inserter, update_time, updater)
VALUES (@{0}, @{1}, 0, 'now', @{2}, 'now', @{3});", vm.Id, vm.SelectedRelationVM.Id, Inserter, Updater);
                        vm.SelectedRelationVM.IsRelated = !vm.SelectedRelationVM.IsRelated;
                    }
                    else {
                        reader = dao.ExecQuery(@"
SELECT *
FROM hst_action
WHERE book_id = @{0} AND item_id = @{1} AND del_flg = 0;", vm.Id, vm.SelectedRelationVM.Id);

                        if (reader.Count != 0) {
                            MessageBox.Show("帳簿内の該当する項目に帳簿項目が存在するので削除できません。"); // TODO
                            e.Handled = true;
                            return;
                        }

                        vm.SelectedRelationVM.IsRelated = !vm.SelectedRelationVM.IsRelated;
                        dao.ExecNonQuery(@"
UPDATE rel_book_item
SET del_flg = @{0}, update_time = 'now', updater = @{1}
WHERE book_id = @{2} AND item_id = @{3};", vm.SelectedRelationVM.IsRelated ? 0 : 1, Updater, vm.Id, vm.SelectedRelationVM.Id);
                    }
                });
            }
            this.IsSaved = true;
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
            string directory = string.Empty;
            string fileName = string.Empty;
            if (this.WVM.DumpExePath != string.Empty) {
                directory = Path.GetDirectoryName(this.WVM.DumpExePath);
                fileName = Path.GetFileName(this.WVM.DumpExePath);
            }

            OpenFileDialog ofd = new OpenFileDialog() {
                CheckFileExists = true,
                InitialDirectory = directory,
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
            string directory = string.Empty;
            string fileName = string.Empty;
            if (this.WVM.RestoreExePath != string.Empty) {
                directory = Path.GetDirectoryName(this.WVM.RestoreExePath);
                fileName = Path.GetFileName(this.WVM.RestoreExePath);
            }

            OpenFileDialog ofd = new OpenFileDialog() {
                CheckFileExists = true,
                InitialDirectory = directory,
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

                ((App)Application.Current).ReleaseMutex();
                Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// バックアップフォルダを指定するダイアログを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackUpFolderPathDialogCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string directory = Path.GetDirectoryName(Application.ResourceAssembly.Location);
            string fileName = string.Empty;
            if (this.WVM.BackUpFolderPath != string.Empty) {
                directory = Path.GetDirectoryName(this.WVM.BackUpFolderPath);
                fileName = Path.GetFileName(this.WVM.BackUpFolderPath);
            }

            CommonOpenFileDialog ofd = new CommonOpenFileDialog() {
                EnsureFileExists = true,
                IsFolderPicker = true,
                InitialDirectory = directory,
                DefaultFileName = fileName,
                Title = "バックアップフォルダ選択"
            };

            if (ofd.ShowDialog() == CommonFileDialogResult.Ok) {
                if (directory.CompareTo(ofd.InitialDirectory) == 0) {
                    this.WVM.BackUpFolderPath = Path.GetFileName(ofd.FileName);
                }
                else {
                    this.WVM.BackUpFolderPath = Path.Combine(ofd.InitialDirectory, ofd.FileName);
                }
            }
        }
        #endregion
        #endregion

        /// <summary>
        /// フォーム終了前
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsWindow_Closing(object sender, CancelEventArgs e)
        {
            this.DialogResult = this.IsSaved;
        }

        /// <summary>
        /// フォーム終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsWindow_Closed(object sender, EventArgs e)
        {
            SaveSetting();
        }

        /// <summary>
        /// 選択中の設定タブを変更した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.oldSelectedSettingsTab != this.WVM.SelectedTab) {
                Cursor cCursor = this.Cursor;
                this.Cursor = Cursors.Wait;

                switch (this.WVM.SelectedTab) {
                    case SettingsTabs.ItemSettingsTab:
                        UpdateItemSettingsTabData();
                        break;
                    case SettingsTabs.BookSettingsTab:
                        UpdateBookSettingTabData();
                        break;
                    case SettingsTabs.OtherSettingsTab:
                        UpdateOtherSettingTabData();
                        break;
                }
                this.Cursor = cCursor;
            }
            this.oldSelectedSettingsTab = this.WVM.SelectedTab;
        }

        #region 項目設定操作
        /// <summary>
        /// 項目設定で一覧の選択を変更した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (this.shopNameListBox.Items.Count > 0 && VisualTreeHelper.GetChildrenCount(this.shopNameListBox) > 0) {
                if (VisualTreeHelper.GetChild(this.shopNameListBox, 0) is Decorator border) {
                    if (border.Child is ScrollViewer scroll) {
                        scroll.ScrollToTop();
                    }
                }
            }
            if (this.remarkListBox.Items.Count > 0 && VisualTreeHelper.GetChildrenCount(this.remarkListBox) > 0) {
                if (VisualTreeHelper.GetChild(this.remarkListBox, 0) is Decorator border) {
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
        private void UpdateSettingWindowData(HierarchicalKind? kind = null, int? id = null, int? bookId = null)
        {
            UpdateItemSettingsTabData(kind, id);
            UpdateBookSettingTabData(bookId);
            UpdateOtherSettingTabData();
        }

        /// <summary>
        /// 項目設定タブに表示するデータを更新する
        /// </summary>
        /// <param name="kind">選択対象の階層種別</param>
        /// <param name="id">選択対象のID</param>
        private void UpdateItemSettingsTabData(HierarchicalKind? kind = null, int? id = null)
        {
            if (this.WVM.SelectedTab == SettingsTabs.ItemSettingsTab) {
                this.WVM.HierachicalItemVMList = LoadItemViewModelList();

                if (kind == null || id == null) {
                    this.WVM.SelectedItemVM = null;
                }
                else {
                    // 収支から探す
                    IEnumerable<HierarchicalItemViewModel> query = this.WVM.HierachicalItemVMList.Where((vm) => { return vm.Kind == kind && vm.Id == id; });
                    if (query.Count() == 0) {
                        // カテゴリから探す
                        foreach (HierarchicalItemViewModel tmpVM in this.WVM.HierachicalItemVMList) {
                            query = tmpVM.ChildrenVMList.Where((vm) => { return vm.Kind == kind && vm.Id == id; });
                            if (query.Count() != 0) { break; }
                        }

                        if (query.Count() == 0) {
                            // 項目から探す
                            foreach (HierarchicalItemViewModel tmpVM in this.WVM.HierachicalItemVMList) {
                                foreach (HierarchicalItemViewModel tmpVM2 in tmpVM.ChildrenVMList) {
                                    query = tmpVM2.ChildrenVMList.Where((vm) => { return vm.Kind == kind && vm.Id == id; });
                                    if (query.Count() != 0) { break; }
                                }
                                if (query.Count() != 0) { break; }
                            }
                        }
                    }

                    this.WVM.SelectedItemVM = query.Count() == 0 ? null : query.First();
                }
            }
        }

        /// <summary>
        /// 帳簿設定タブに表示するデータを更新する
        /// </summary>
        /// <param name="bookId">選択対象の帳簿ID</param>
        private void UpdateBookSettingTabData(int? bookId = null)
        {
            if (this.WVM.SelectedTab == SettingsTabs.BookSettingsTab) {
                this.WVM.BookVMList = LoadBookSettingViewModelList();

                if (bookId == null) {
                    this.WVM.SelectedBookVM = null;
                }
                else {
                    IEnumerable<BookSettingViewModel> query = this.WVM.BookVMList.Where((vm) => { return vm.Id == bookId; });
                    this.WVM.SelectedBookVM = query.Count() == 0 ? null : query.First();
                }
            }
        }

        /// <summary>
        /// その他タブに表示するデータを更新する
        /// </summary>
        private void UpdateOtherSettingTabData()
        {
            if(this.WVM.SelectedTab == SettingsTabs.OtherSettingsTab) {
                this.WVM.LoadSettings();
            }
        }

        /// <summary>
        /// 階層構造項目VMリストを取得する
        /// </summary>
        /// <returns>階層構造項目VMリスト</returns>
        private ObservableCollection<HierarchicalItemViewModel> LoadItemViewModelList()
        {
            ObservableCollection<HierarchicalItemViewModel> vmList = new ObservableCollection<HierarchicalItemViewModel>();
            HierarchicalItemViewModel incomeVM = new HierarchicalItemViewModel() {
                Kind = HierarchicalKind.Balance,
                Id = (int)BalanceKind.Income,
                Name = "収入項目",
                ParentVM = null,
                RelationVMList = null,
                ChildrenVMList = new ObservableCollection<HierarchicalItemViewModel>()
            };
            vmList.Add(incomeVM);

            HierarchicalItemViewModel outgoVM = new HierarchicalItemViewModel() {
                Kind = HierarchicalKind.Balance,
                Id = (int)BalanceKind.Outgo,
                Name = "支出項目",
                ParentVM = null,
                RelationVMList = null,
                ChildrenVMList = new ObservableCollection<HierarchicalItemViewModel>()
            };
            vmList.Add(outgoVM);

            foreach (HierarchicalItemViewModel vm in vmList) {
                using (DaoBase dao = this.builder.Build()) {
                    DaoReader reader = dao.ExecQuery(@"
SELECT category_id, category_name 
FROM mst_category
WHERE balance_kind = @{0} AND del_flg = 0 AND sort_order <> 0
ORDER BY sort_order;", vm.Id);

                    reader.ExecWholeRow((count, record) => {
                        int categoryId = record.ToInt("category_id");
                        string categoryName = record["category_name"];

                        vm.ChildrenVMList.Add(new HierarchicalItemViewModel() {
                            Kind = HierarchicalKind.Category,
                            Id = categoryId,
                            Name = categoryName,
                            ParentVM = vm,
                            RelationVMList = null,
                            ChildrenVMList = new ObservableCollection<HierarchicalItemViewModel>()
                        });
                        return true;
                    });

                    foreach (HierarchicalItemViewModel childVM in vm.ChildrenVMList) {
                        reader = dao.ExecQuery(@"
SELECT item_id, item_name
FROM mst_item
WHERE category_id = @{0} AND del_flg = 0
ORDER BY sort_order;", childVM.Id);

                        reader.ExecWholeRow((count, record) => {
                            int itemId = record.ToInt("item_id");
                            string itemName = record["item_name"];

                            childVM.ChildrenVMList.Add(new HierarchicalItemViewModel() {
                                Kind = HierarchicalKind.Item,
                                Id = itemId,
                                Name = itemName,
                                ParentVM = childVM,
                                RelationVMList = new ObservableCollection<RelationViewModel>(),
                                ShopNameList = new ObservableCollection<string>(),
                                RemarkList = new ObservableCollection<string>()
                            });
                            return true;
                        });

                        foreach (HierarchicalItemViewModel vm2 in childVM.ChildrenVMList) {
                            reader = dao.ExecQuery(@"
SELECT B.book_id AS BookId, B.book_name, RBI.book_id IS NULL AS IsNotRelated
FROM mst_book B
LEFT JOIN (SELECT book_id FROM rel_book_item WHERE del_flg = 0 AND item_id = @{0}) RBI ON RBI.book_id = B.book_id
WHERE del_flg = 0
ORDER BY B.sort_order;", vm2.Id);

                            reader.ExecWholeRow((count2, record2) => {
                                int bookId = record2.ToInt("BookId");
                                string bookName = record2["book_name"];
                                bool isRelated = !record2.ToBoolean("IsNotRelated");

                                vm2.RelationVMList.Add(new RelationViewModel() {
                                    Id = bookId,
                                    Name = bookName,
                                    IsRelated = isRelated
                                });
                                return true;
                            });

                            // 店舗名の一覧を取得する
                            reader = dao.ExecQuery(@"
SELECT shop_name
FROM hst_shop
WHERE del_flg = 0 AND item_id = @{0}
ORDER BY used_time DESC;", vm2.Id);

                            reader.ExecWholeRow((count2, record2) => {
                                string shopName = record2["shop_name"];
                                vm2.ShopNameList.Add(shopName);
                                return true;
                            });

                            // 備考の一覧を取得する
                            reader = dao.ExecQuery(@"
SELECT remark
FROM hst_remark
WHERE del_flg = 0 AND item_id = @{0}
ORDER BY used_time DESC;", vm2.Id);

                            reader.ExecWholeRow((count2, record2) => {
                                string remark = record2["remark"];
                                vm2.RemarkList.Add(remark);
                                return true;
                            });
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
        private ObservableCollection<BookSettingViewModel> LoadBookSettingViewModelList()
        {
            ObservableCollection<BookSettingViewModel> settingVMList = new ObservableCollection<BookSettingViewModel>();
            ObservableCollection<BookViewModel> vmList = new ObservableCollection<BookViewModel>() {
                new BookViewModel(){ Id = -1, Name = "なし" }
            };

            using (DaoBase dao = this.builder.Build()) {
                // 帳簿一覧を取得する(支払元選択用)
                DaoReader reader = dao.ExecQuery(@"
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
                reader = dao.ExecQuery(@"
SELECT book_id, book_name, book_kind, debit_book_id, pay_day, initial_value, json_code
FROM mst_book
WHERE del_flg = 0
ORDER BY sort_order;");

                reader.ExecWholeRow((count, record) => {
                    int bookId = record.ToInt("book_id");
                    string bookName = record["book_name"];
                    BookKind bookKind = (BookKind)record.ToInt("book_kind");
                    int initialValue = record.ToInt("initial_value");
                    int? debitBookId = record.ToNullableInt("debit_book_id");
                    int? payDay = record.ToNullableInt("pay_day");

                    string jsonCode = record["json_code"];
                    MstBookJsonObject jsonObj = JsonConvert.DeserializeObject<MstBookJsonObject>(jsonCode);

                    BookSettingViewModel tmpVM = new BookSettingViewModel() {
                        Id = bookId,
                        Name = bookName,
                        SelectedBookKind = bookKind,
                        InitialValue = initialValue,
                        DebitBookVMList = new ObservableCollection<BookViewModel>(vmList.Where((vm) => { return vm.Id != bookId; })),
                        PayDay = payDay,
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
                    reader = dao.ExecQuery(@"
SELECT I.item_id AS ItemId, I.item_name, C.category_name, RBI.item_id IS NULL AS IsNotRelated
FROM mst_item I
INNER JOIN (SELECT category_id, category_name FROM mst_category WHERE del_flg = 0) C ON C.category_id = I.category_id
LEFT JOIN (SELECT item_id FROM rel_book_item WHERE del_flg = 0 AND book_id = @{0}) RBI ON RBI.item_id = I.item_id
WHERE del_flg = 0 AND move_flg = 0
ORDER BY I.sort_order;", vm.Id);

                    vm.RelationVMList = new ObservableCollection<RelationViewModel>();
                    reader.ExecWholeRow((count, record) => {
                        int itemId = record.ToInt("ItemId");
                        string name = string.Format(@"{0} - {1}", record["category_name"], record["item_name"]);
                        bool isRelated = !record.ToBoolean("IsNotRelated");

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
        #endregion
        
        #region 設定反映用の関数
        /// <summary>
        /// 設定を読み込む
        /// </summary>
        private void LoadSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (settings.SettingsWindow_Left != -1) {
                this.Left = settings.SettingsWindow_Left;
            }
            if (settings.SettingsWindow_Top != -1) {
                this.Top = settings.SettingsWindow_Top;
            }
            if (settings.SettingsWindow_Width != -1) {
                this.Width = settings.SettingsWindow_Width;
            }
            if (settings.SettingsWindow_Height != -1) {
                this.Height = settings.SettingsWindow_Height;
            }
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        private void SaveSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;
            settings.SettingsWindow_Left = this.Left;
            settings.SettingsWindow_Top = this.Top;
            settings.SettingsWindow_Width = this.Width;
            settings.SettingsWindow_Height = this.Height;
            settings.Save();
        }
        #endregion
    }
}

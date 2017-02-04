using HouseholdAccountBook.Dao;
using HouseholdAccountBook.Extentions;
using HouseholdAccountBook.UserEventArgs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.Windows
{
    /// <summary>
    /// ActionRegistrationWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ActionRegistrationWindow : Window
    {
        #region フィールド
        /// <summary>
        /// DAOビルダ
        /// </summary>
        private DaoBuilder builder;
        /// <summary>
        /// 帳簿項目ID
        /// </summary>
        private int? actionId = null;
        /// <summary>
        /// グループID
        /// </summary>
        private int? groupId = null;
        #endregion

        /// <summary>
        /// 登録時のイベント
        /// </summary>
        public event EventHandler<RegistrateEventArgs> Registrated = null;

        /// <summary>
        /// 帳簿項目追加ウィンドウ
        /// </summary>
        /// <param name="builder">DAOビルダ</param>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="selectedDateTime">選択された日時</param>
        public ActionRegistrationWindow(DaoBuilder builder, int? bookId, DateTime? selectedDateTime)
        {
            InitializeComponent();
            Title = "追加";

            this.builder = builder;
            this.actionId = null;

            ObservableCollection<BookViewModel> bookVMList = new ObservableCollection<BookViewModel>();
            BookViewModel selectedBookVM = null;
            using (DaoBase dao = builder.Build()) {
                DaoReader reader = dao.ExecQuery("SELECT book_id, book_name FROM mst_book WHERE del_flg = 0;");
                reader.ExecWholeRow((count, record) => {
                    BookViewModel vm = new BookViewModel() { BookId = record.ToInt("book_id"), BookName = record["book_name"] };
                    bookVMList.Add(vm);
                    if(selectedBookVM == null || bookId == vm.BookId) {
                        selectedBookVM = vm;
                    }
                });
            }

            this.ActionRegistrationWindowVM.BookVMList = bookVMList;
            this.ActionRegistrationWindowVM.SelectedBookVM = selectedBookVM;
            this.ActionRegistrationWindowVM.SelectedDate = selectedDateTime != null ? selectedDateTime.Value : DateTime.Today;
            this.ActionRegistrationWindowVM.SelectedBalanceKindVM = this.ActionRegistrationWindowVM.BalanceKindVMList[1];

            UpdateCategoryList();
            UpdateItemList();
            UpdateShopList();
            UpdateRemarkList();

            LoadSetting();

            #region イベントハンドラの設定
            this.ActionRegistrationWindowVM.OnBookChanged += () => {
                UpdateCategoryList();
                UpdateItemList();
                UpdateShopList();
                UpdateRemarkList();
            };
            this.ActionRegistrationWindowVM.OnBalanceKindChanged += () => {
                UpdateCategoryList();
                UpdateItemList();
                UpdateShopList();
                UpdateRemarkList();
            };
            this.ActionRegistrationWindowVM.OnCategoryChanged += () => {
                UpdateItemList();
                UpdateShopList();
                UpdateRemarkList();
            };
            this.ActionRegistrationWindowVM.OnItemChanged += () => {
                UpdateShopList();
                UpdateRemarkList();
            };
            #endregion
        }

        /// <summary>
        /// 帳簿項目編集ウィンドウ
        /// </summary>
        /// <param name="builder">DAOビルダ</param>
        /// <param name="actionId">帳簿項目ID</param>
        public ActionRegistrationWindow(DaoBuilder builder, int actionId)
        {
            InitializeComponent();
            Title = "編集";

            this.builder = builder;
            this.actionId = actionId;
            
            ObservableCollection<BookViewModel> bookVMList = new ObservableCollection<BookViewModel>();
            BookViewModel selectedBookVM = null;
            int itemId = -1;
            DateTime actDate = DateTime.Now;
            int actValue = -1;
            string shopName = string.Empty;
            string remark = string.Empty;
            using(DaoBase dao = builder.Build()) {
                DaoReader reader = dao.ExecQuery(@"
SELECT book_id, item_id, act_time, act_value, group_id, shop_name, remark 
FROM hst_action 
WHERE del_flg = 0 AND action_id = @{0};", actionId);
                reader.ExecARow((record) => {
                    int bookId = record.ToInt("book_id");
                    itemId = record.ToInt("item_id");
                    actDate = DateTime.Parse(record["act_time"]);
                    actValue = record.ToInt("act_value");
                    groupId = record.ToNumerableInt("group_id");
                    shopName = record["shop_name"];
                    remark = record["remark"];

                    reader = dao.ExecQuery(@"
SELECT book_id, book_name FROM mst_book WHERE del_flg = 0;");
                    reader.ExecWholeRow((count2, record2) => {
                        BookViewModel vm = new BookViewModel() { BookId = record2.ToInt("book_id"), BookName = record2["book_name"] };
                        bookVMList.Add(vm);
                        if (selectedBookVM == null || bookId == vm.BookId) {
                            selectedBookVM = vm;
                        }
                    });
                });
            }
            this.ActionRegistrationWindowVM.BookVMList = bookVMList;
            this.ActionRegistrationWindowVM.SelectedBookVM = selectedBookVM;
            this.ActionRegistrationWindowVM.SelectedDate = actDate;
            int balanceKindIndex = Math.Sign(actValue) > 0 ? 0 : 1; // 収入 / 支出
            this.ActionRegistrationWindowVM.SelectedBalanceKindVM = this.ActionRegistrationWindowVM.BalanceKindVMList[balanceKindIndex];
            this.ActionRegistrationWindowVM.Value = Math.Abs(actValue);

            // 回数の表示
            int count = 1;
            if (groupId != null) {
                using (DaoBase dao = builder.Build()) {
                    DaoReader reader = dao.ExecQuery(@"
SELECT COUNT(action_id) count FROM hst_action 
WHERE del_flg = 0 AND group_id = @{0} AND act_time >= (SELECT act_time FROM hst_action WHERE action_id = @{1});", groupId, actionId);
                    reader.ExecARow((record) => {
                        count = record.ToInt("count");
                    });
                }
            }
            this.ActionRegistrationWindowVM.Count = count;

            UpdateCategoryList();
            UpdateItemList(itemId);
            UpdateShopList(shopName);
            UpdateRemarkList(remark);

            LoadSetting();

            #region イベントハンドラの設定
            this.ActionRegistrationWindowVM.OnBookChanged += () => {
                UpdateCategoryList();
                UpdateItemList();
                UpdateShopList();
                UpdateRemarkList();
            };
            this.ActionRegistrationWindowVM.OnBalanceKindChanged += () => {
                UpdateCategoryList();
                UpdateItemList();
                UpdateShopList();
                UpdateRemarkList();
            };
            this.ActionRegistrationWindowVM.OnCategoryChanged += () => {
                UpdateItemList();
                UpdateShopList();
                UpdateRemarkList();
            };
            this.ActionRegistrationWindowVM.OnItemChanged += () => {
                UpdateShopList();
                UpdateRemarkList();
            };
            #endregion
        }

        #region コマンド
        /// <summary>
        /// 続けて入力コマンド判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContinueToRegisterCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (actionId == null) && this.ActionRegistrationWindowVM.Value .HasValue;
        }

        /// <summary>
        /// 続けて入力コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContinueToRegisterCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // DB登録
            int? id = RegisterToDb();

            // MainWindow更新
            Registrated?.Invoke(this, new RegistrateEventArgs(id));

            // 表示クリア
            this.ActionRegistrationWindowVM.Value = null;
            this.ActionRegistrationWindowVM.Count = 1;
        }
        
        /// <summary>
        /// 登録コマンド判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegisterCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute =  this.ActionRegistrationWindowVM.Value.HasValue;
        }

        /// <summary>
        /// 登録コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegisterCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // DB登録
            int? id = RegisterToDb();

            // MainWindow更新
            Registrated?.Invoke(this, new RegistrateEventArgs(id));

            this.DialogResult = true;
            this.Close();
        }
        
        /// <summary>
        /// キャンセルコマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        #endregion

        #region イベントハンドラ
        /// <summary>
        /// 読込完了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActionRegistrationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 処理なし
        }

        /// <summary>
        /// フォーム終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActionRegistrationWindow_Closing(object sender, CancelEventArgs e)
        {
            SaveSetting();
        }

        /// <summary>
        /// キー押下時の処理(Shift+Enterキーで登録、Enterキーで登録(編集時)/続けて登録(追加時)、Escキーでキャンセル)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActionRegistrationWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key) {
                case Key.Enter:
                    if((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.None || actionId != null) {
                        RoutedCommand command = this.Resources["RegisterCommand"] as RoutedCommand;

                        if (command != null && command.CanExecute(null, sender as IInputElement)) {
                            command.Execute(null, sender as IInputElement);
                        }
                        e.Handled = true;
                    }
                    else {
                        RoutedCommand command = this.Resources["ContinueToRegisterCommand"] as RoutedCommand;

                        if (command != null && command.CanExecute(null, sender as IInputElement)) {
                            command.Execute(null, sender as IInputElement);
                        }
                        e.Handled = true;
                    }
                    break;
                case Key.Escape: {
                        RoutedCommand command = this.Resources["CancelCommand"] as RoutedCommand;

                        if (command != null && command.CanExecute(null, sender as IInputElement)) {
                            command.Execute(null, sender as IInputElement);
                        }
                        e.Handled = true;
                    }
                    
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region 画面更新用の関数
        /// <summary>
        /// カテゴリリストを更新する
        /// </summary>
        /// <param name="categoryId">選択対象のカテゴリ</param>
        private void UpdateCategoryList(int? categoryId = null)
        {
            ObservableCollection<CategoryViewModel> categoryVMList = new ObservableCollection<CategoryViewModel>() {
                new CategoryViewModel() { CategoryId = -1, CategoryName = "(指定なし)" }
            };
            CategoryViewModel selectedCategoryVM = categoryVMList[0];
            using (DaoBase dao = builder.Build()) {
                DaoReader reader = dao.ExecQuery(@"
SELECT category_id, category_name FROM mst_category C 
WHERE del_flg = 0 AND EXISTS (SELECT * FROM mst_item I WHERE I.category_id = C.category_id AND balance_kind = @{0} AND del_flg = 0 
  AND EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @{1} AND RBI.item_id = I.item_id)) 
ORDER BY sort_order;", (int)this.ActionRegistrationWindowVM.SelectedBalanceKindVM.BalanceKind, this.ActionRegistrationWindowVM.SelectedBookVM.BookId);
                reader.ExecWholeRow((count, record) => {
                    CategoryViewModel vm = new CategoryViewModel() { CategoryId = record.ToInt("category_id"), CategoryName = record["category_name"] };
                    categoryVMList.Add(vm);
                    if(vm.CategoryId == categoryId) {
                        selectedCategoryVM = vm;
                    }
                });
            }
            this.ActionRegistrationWindowVM.CategoryVMList = categoryVMList;
            this.ActionRegistrationWindowVM.SelectedCategoryVM = selectedCategoryVM;
        }

        /// <summary>
        /// 項目リストを更新する
        /// </summary>
        /// <param name="itemId">選択対象の項目</param>
        private void UpdateItemList(int? itemId = null)
        {
            ObservableCollection<ItemViewModel> itemVMList = new ObservableCollection<ItemViewModel>();
            ItemViewModel selectedItemVM = null;
            using (DaoBase dao = builder.Build()) {
                DaoReader reader;
                if (this.ActionRegistrationWindowVM.SelectedCategoryVM.CategoryId == -1) {
                    reader = dao.ExecQuery(@"
SELECT item_id, item_name FROM mst_item I 
WHERE del_flg = 0 AND EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @{0} AND RBI.item_id = I.item_id AND del_flg = 0)
  AND EXISTS (SELECT * FROM mst_category C WHERE C.category_id = I.category_id AND balance_kind = @{1} AND del_flg = 0)
ORDER BY sort_order;", this.ActionRegistrationWindowVM.SelectedBookVM.BookId, (int)this.ActionRegistrationWindowVM.SelectedBalanceKindVM.BalanceKind);
                }
                else {
                    reader = dao.ExecQuery(@"
SELECT item_id, item_name FROM mst_item I 
WHERE del_flg = 0 AND EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @{0} AND RBI.item_id = I.item_id AND del_flg = 0)
  AND category_id = @{1}
ORDER BY sort_order;", this.ActionRegistrationWindowVM.SelectedBookVM.BookId, (int)this.ActionRegistrationWindowVM.SelectedCategoryVM.CategoryId);
                }
                reader.ExecWholeRow((count, record) => {
                    ItemViewModel vm = new ItemViewModel() { ItemId = record.ToInt("item_id"), ItemName = record["item_name"] };
                    itemVMList.Add(vm);
                    if (selectedItemVM == null || vm.ItemId == itemId) {
                        selectedItemVM = vm;
                    }
                });
            }
            this.ActionRegistrationWindowVM.ItemVMList = itemVMList;
            this.ActionRegistrationWindowVM.SelectedItemVM = selectedItemVM;
        }

        /// <summary>
        /// 店舗リストを更新する
        /// </summary>
        /// <param name="shopName">選択対象の店舗名</param>
        private void UpdateShopList(string shopName = null)
        {
            ObservableCollection<string> shopNameVMList = new ObservableCollection<string>() {
                    string.Empty
            };
            string selectedShopName = shopName ?? this.ActionRegistrationWindowVM.SelectedShopName ?? shopNameVMList[0];
            using (DaoBase dao = builder.Build()) {
                DaoReader reader = dao.ExecQuery(@"
SELECT shop_name FROM hst_shop 
WHERE del_flg = 0 AND item_id = @{0} 
ORDER BY used_time DESC;", this.ActionRegistrationWindowVM.SelectedItemVM.ItemId);
                reader.ExecWholeRow((count, record) => {
                    string tmp = record["shop_name"];
                    shopNameVMList.Add(tmp);
                });
            }

            this.ActionRegistrationWindowVM.ShopNameList = shopNameVMList;
            this.ActionRegistrationWindowVM.SelectedShopName = selectedShopName;
        }

        /// <summary>
        /// 備考リストを更新する
        /// </summary>
        /// <param name="remark">選択対象の備考</param>
        private void UpdateRemarkList(string remark = null)
        {
            ObservableCollection<string> remarkVMList = new ObservableCollection<string>() {
                    string.Empty
            };
            string selectedRemark = remark ?? this.ActionRegistrationWindowVM.SelectedRemark ?? remarkVMList[0];
            using (DaoBase dao = builder.Build()) {
                DaoReader reader = dao.ExecQuery(@"
SELECT remark FROM hst_remark 
WHERE del_flg = 0 AND item_id = @{0} 
ORDER BY used_time DESC;", this.ActionRegistrationWindowVM.SelectedItemVM.ItemId);
                reader.ExecWholeRow((count, record) => {
                    string tmp = record["remark"];
                    remarkVMList.Add(tmp);
                });
            }
            
            this.ActionRegistrationWindowVM.RemarkList = remarkVMList;
            this.ActionRegistrationWindowVM.SelectedRemark = selectedRemark;
        }
        #endregion

        /// <summary>
        /// DBに登録する
        /// </summary>
        /// <returns>登録された帳簿項目ID</returns>
        private int? RegisterToDb()
        {
            if(!ActionRegistrationWindowVM.Value.HasValue || ActionRegistrationWindowVM.Value <= 0) {
                MessageBox.Show(this, Message.IllegalValue, MessageTitle.Exclamation, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return null;
            }

            BalanceKind balanceKind = ActionRegistrationWindowVM.SelectedBalanceKindVM.BalanceKind; // 収支種別
            int bookId = ActionRegistrationWindowVM.SelectedBookVM.BookId.Value; // 帳簿ID
            int itemId = ActionRegistrationWindowVM.SelectedItemVM.ItemId; // 帳簿項目ID
            DateTime actTime = ActionRegistrationWindowVM.SelectedDate; // 日付
            int actValue = (balanceKind == BalanceKind.Income ? 1 : -1) * ActionRegistrationWindowVM.Value.Value; // 値
            string shopName = ActionRegistrationWindowVM.SelectedShopName; // 店舗名
            int count = ActionRegistrationWindowVM.Count; // 繰返し回数
            string remark = ActionRegistrationWindowVM.SelectedRemark; // 備考

            int? resActionId = null;

            using(DaoBase dao = builder.Build()) {
                if (actionId == null) {
                    #region 帳簿項目を追加する
                    if (count == 1) { // 繰返し回数が1回(繰返しなし)
                        DaoReader reader = dao.ExecQuery(@"
INSERT INTO hst_action (book_id, item_id, act_time, act_value, shop_name, remark, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, @{2}, @{3}, @{4}, @{5}, 0, 'now', @{6}, 'now', @{7}) RETURNING action_id;",
                            bookId, itemId, actTime, actValue, shopName, remark, Updater, Inserter);

                        reader.ExecARow((record) => {
                            resActionId = record.ToInt("action_id");
                        });
                    }
                    else { // 繰返し回数が2回以上(繰返しあり)
                        dao.ExecTransaction(() => {
                            int tmpGroupId = -1;
                            // グループIDを取得する
                            DaoReader reader = dao.ExecQuery(@"
INSERT INTO hst_group (group_kind, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, 0, 'now', @{1}, 'now', @{2}) RETURNING group_id;", (int)GroupKind.Repeat, Updater, Inserter);
                            reader.ExecARow((record) => {
                                tmpGroupId = record.ToInt("group_id");
                            });

                            DateTime tmpActTime = actTime;
                            for (int i = 0; i < count; ++i) {
                                reader = dao.ExecQuery(@"
INSERT INTO hst_action (book_id, item_id, act_time, act_value, shop_name, group_id, remark, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, @{2}, @{3}, @{4}, @{5}, @{6}, 0, 'now', @{7}, 'now', @{8}) RETURNING action_id;",
                                    bookId, itemId, tmpActTime, actValue, shopName, tmpGroupId, remark, Updater, Inserter);
                                
                                // 繰り返しの最初の1回を選択するようにする
                                if (i == 0) {
                                    reader.ExecARow((record) => {
                                        resActionId = record.ToInt("action_id");
                                    });
                                }

                                tmpActTime = actTime.AddMonths(i + 1);
                            }
                        });
                    }
                    #endregion
                }
                else {
                    #region 帳簿項目を編集する
                    if (count == 1) {
                        #region 繰返し回数が1回
                        if (groupId == null) {
                            #region グループに属していない
                            dao.ExecNonQuery(@"
UPDATE hst_action
SET book_id = @{0}, item_id = @{1}, act_time = @{2}, act_value = @{3}, shop_name = @{4}, remark = @{5}, update_time = 'now', updater = @{6}
WHERE action_id = @{7};", bookId, itemId, actTime, actValue, shopName, remark, Updater, actionId);
                            #endregion
                        }
                        else {
                            #region グループに属している
                            dao.ExecTransaction(() => {
                                // 以降の繰返し分のレコードを削除する
                                dao.ExecNonQuery(@"
UPDATE hst_action
SET del_flg = 1, update_time = 'now', updater = @{0}
WHERE del_flg = 0 AND group_id = @{1} AND act_time > (SELECT act_time FROM hst_action WHERE action_id = @{2});", Updater, groupId, actionId);

                                // グループに属する項目の個数を調べる
                                DaoReader reader = dao.ExecQuery(@"
SELECT action_id FROM hst_action
WHERE del_flg = 0 AND group_id = @{0};", groupId);

                                if (reader.Count <= 1) {
                                    #region グループに属する項目が1項目以下
                                    // グループをクリアする
                                    dao.ExecNonQuery(@"
UPDATE hst_action
SET book_id = @{0}, item_id = @{1}, act_time = @{2}, act_value = @{3}, shop_name = @{4}, group_id = null, remark = @{5}, update_time = 'now', updater = @{6}
WHERE action_id = @{7};", bookId, itemId, actTime, actValue, shopName, remark, Updater, actionId);

                                    // グループを削除する
                                    dao.ExecNonQuery(@"
UPDATE hst_group
SET del_flg = 1, update_time = 'now', updater = @{0}
WHERE del_flg = 0 AND group_id = @{1};", Updater, groupId);
                                    #endregion
                                }
                                else {
                                    #region グループに属する項目が2項目以上
                                    dao.ExecNonQuery(@"
UPDATE hst_action
SET book_id = @{0}, item_id = @{1}, act_time = @{2}, act_value = @{3}, shop_name = @{4}, remark = @{5}, update_time = 'now', updater = @{6}
WHERE action_id = @{7};", bookId, itemId, actTime, actValue, shopName, remark, Updater, actionId);
                                    #endregion
                                }
                            });
                            #endregion
                        }
                        #endregion
                    }
                    else {
                        #region 繰返し回数が2回以上
                        dao.ExecTransaction(() => {
                            List<int> actionIdList = new List<int>();

                            DaoReader reader;
                            if (groupId == null) {
                                #region グループIDが未割当て
                                // グループIDを取得する
                                reader = dao.ExecQuery(@"
INSERT INTO hst_group (group_kind, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, 0, 'now', @{1}, 'now', @{2}) RETURNING group_id;", (int)GroupKind.Repeat, Updater, Inserter);
                                reader.ExecARow((record) => {
                                    groupId = record.ToInt("group_id");
                                });
                                actionIdList.Add(actionId.Value);
                                #endregion
                            }
                            else {
                                #region グループIDが割当て済
                                // 変更の対象となる帳簿項目を洗い出す
                                reader = dao.ExecQuery(@"
SELECT action_id FROM hst_action 
WHERE del_flg = 0 AND group_id = @{0} AND act_time >= (SELECT act_time FROM hst_action WHERE action_id = @{1})
ORDER BY act_time ASC;", groupId, actionId);
                                reader.ExecWholeRow((recCount, record) => {
                                    actionIdList.Add(record.ToInt("action_id"));
                                });
                                #endregion
                            }

                            DateTime tmpActTime = actTime;
                            for (int i = 0; i < actionIdList.Count; ++i) {
                                int targetActionId = actionIdList[i];

                                if (i < count) { // 繰返し回数の範囲内のレコードを更新する
                                    dao.ExecNonQuery(@"
UPDATE hst_action
SET book_id = @{0}, item_id = @{1}, act_time = @{2}, act_value = @{3}, shop_name = @{4}, group_id = @{5}, remark = @{6}, update_time = 'now', updater = @{7}
WHERE action_id = @{8};", bookId, itemId, tmpActTime, actValue, shopName, groupId, remark, Updater, targetActionId);
                                }
                                else { // 繰返し回数が帳簿項目数を下回っていた場合に、越えたレコードを削除する
                                    dao.ExecNonQuery(@"
UPDATE hst_action
SET del_flg = 1, update_time = 'now', updater = @{0}
WHERE action_id = @{1};", Updater, targetActionId);
                                }
                                tmpActTime = actTime.AddMonths(i + 1);
                            }
                            // 繰返し回数が帳簿項目数を越えていた場合に、新規レコードを追加する
                            for (int i = actionIdList.Count; i < count; ++i) {
                                dao.ExecNonQuery(@"
INSERT INTO hst_action (book_id, item_id, act_time, act_value, shop_name, group_id, remark, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, @{2}, @{3}, @{4}, @{5}, @{6}, 0, 'now', @{7}, 'now', @{8});", bookId, itemId, tmpActTime, actValue, shopName, groupId, remark, Updater, Inserter);

                                tmpActTime = actTime.AddMonths(i + 1);
                            }
                        });
                        #endregion
                    }

                    resActionId = actionId;
                    #endregion
                }

                if (shopName != string.Empty) {
                    #region 店舗を追加する
                    dao.ExecTransaction(() => {
                        DaoReader reader = dao.ExecQuery(@"
SELECT shop_name FROM hst_shop
WHERE item_id = @{0} AND shop_name = @{1};", itemId, shopName);

                        if (reader.Count == 0) {
                            dao.ExecNonQuery(@"
INSERT INTO hst_shop (item_id, shop_name, used_time, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, @{2}, 0, 'now', @{3}, 'now', @{4});", itemId, shopName, actTime, Updater, Inserter);
                        }
                        else {
                            dao.ExecNonQuery(@"
UPDATE hst_shop
SET used_time = @{0}, del_flg = 0, update_time = 'now', updater = @{1}
WHERE item_id = @{2} AND shop_name = @{3};", actTime, Updater, itemId, shopName);
                        }
                    });
                    #endregion
                }

                if (remark != string.Empty) {
                    #region 備考を追加する
                    dao.ExecTransaction(() => {
                        DaoReader reader = dao.ExecQuery(@"
SELECT remark FROM hst_remark
WHERE item_id = @{0} AND remark = @{1};", itemId, remark);

                        if (reader.Count == 0) {
                            dao.ExecNonQuery(@"
INSERT INTO hst_remark (item_id, remark, remark_kind, used_time, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, 0, @{2}, 0, 'now', @{3}, 'now', @{4});", itemId, remark, actTime, Updater, Inserter);
                        }
                        else {
                            dao.ExecNonQuery(@"
UPDATE hst_remark
SET used_time = @{0}, del_flg = 0, update_time = 'now', updater = @{1}
WHERE item_id = @{2} AND remark = @{3};", actTime, Updater, itemId, remark);
                        }
                    });
                    #endregion
                }
            }

            return resActionId;
        }

        #region 設定反映用の関数
        /// <summary>
        /// 設定を読み込む
        /// </summary>
        private void LoadSetting()
        {
            if (Properties.Settings.Default.ActionRegistrationWindow_Left != -1) {
                Left = Properties.Settings.Default.ActionRegistrationWindow_Left;
            }
            if (Properties.Settings.Default.ActionRegistrationWindow_Top != -1) {
                Top = Properties.Settings.Default.ActionRegistrationWindow_Top;
            }
            if (Properties.Settings.Default.ActionRegistrationWindow_Width != -1) {
                Width = Properties.Settings.Default.ActionRegistrationWindow_Width;
            }
            if (Properties.Settings.Default.ActionRegistrationWindow_Height != -1) {
                Height = Properties.Settings.Default.ActionRegistrationWindow_Height;
            }
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        private void SaveSetting()
        {
            if (this.WindowState == WindowState.Normal) {
                Properties.Settings.Default.ActionRegistrationWindow_Left = Left;
                Properties.Settings.Default.ActionRegistrationWindow_Top = Top;
                Properties.Settings.Default.ActionRegistrationWindow_Width = Width;
                Properties.Settings.Default.ActionRegistrationWindow_Height = Height;
                Properties.Settings.Default.Save();
            }
        }
        #endregion
    }

    #region ViewModel
    /// <summary>
    /// 帳簿VM
    /// </summary>
    public partial class BookViewModel { }

    /// <summary>
    /// 収支種別
    /// </summary>
    public partial class BalanceKindViewModel
    {
        /// <summary>
        /// 収支種別
        /// </summary>
        public BalanceKind BalanceKind { get; set; }
        /// <summary>
        /// 収支種別名
        /// </summary>
        public string BalanceKindName { get; set; }
    }

    /// <summary>
    /// カテゴリVM
    /// </summary>
    public partial class CategoryViewModel
    {
        /// <summary>
        /// カテゴリID
        /// </summary>
        public int CategoryId { get; set; }
        /// <summary>
        /// カテゴリ名
        /// </summary>
        public string CategoryName { get; set; }
    }

    /// <summary>
    /// 項目VM
    /// </summary>
    public partial class ItemViewModel
    {
        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; }
        /// <summary>
        /// 項目名
        /// </summary>
        public string ItemName { get; set; }
    }

    /// <summary>
    /// 帳簿項目登録ウィンドウVM
    /// </summary>
    public class ActionRegistrationWindowViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 変更時処理重複防止用フラグ
        /// </summary>
        private bool UpdateOnChanged = false;

        /// <summary>
        /// 帳簿変更時イベント
        /// </summary>
        public event Action OnBookChanged = default(Action);
        /// <summary>
        /// 収支変更時イベント
        /// </summary>
        public event Action OnBalanceKindChanged = default(Action);
        /// <summary>
        /// カテゴリ変更時イベント
        /// </summary>
        public event Action OnCategoryChanged = default(Action);
        /// <summary>
        /// 項目変更時イベント
        /// </summary>
        public event Action OnItemChanged = default(Action);

        /// <summary>
        /// 帳簿VMリスト
        /// </summary>
        #region BookVMList
        public ObservableCollection<BookViewModel> BookVMList
        {
            get { return _BookVMList; }
            set {
                if (_BookVMList != value) {
                    _BookVMList = value;
                    PropertyChanged?.Raise(this, _nameBookVMList);
                }
            }
        }
        private ObservableCollection<BookViewModel> _BookVMList = default(ObservableCollection<BookViewModel>);
        internal static readonly string _nameBookVMList = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.BookVMList);
        #endregion
        /// <summary>
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookViewModel SelectedBookVM
        {
            get { return _SelectedBookVM; }
            set {
                if (_SelectedBookVM != value) {
                    _SelectedBookVM = value;
                    PropertyChanged?.Raise(this, _nameSelectedBookVM);
                    
                    if (!UpdateOnChanged) {
                        UpdateOnChanged = true;
                        OnBookChanged?.Invoke();
                        UpdateOnChanged = false;
                    }
                }
            }
        }

        private BookViewModel _SelectedBookVM = default(BookViewModel);
        internal static readonly string _nameSelectedBookVM = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.SelectedBookVM);
        #endregion

        /// <summary>
        /// 選択された日時
        /// </summary>
        #region SelectedDate
        public DateTime SelectedDate
        {
            get { return _SelectedDate; }
            set {
                if (_SelectedDate != value) {
                    _SelectedDate = value;
                    PropertyChanged?.Raise(this, _nameSelectedDate);
                }
            }
        }
        private DateTime _SelectedDate = default(DateTime);
        internal static readonly string _nameSelectedDate = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.SelectedDate);
        #endregion

        /// <summary>
        /// 収支VMリスト
        /// </summary>
        #region BalanceKindVMList
        public ObservableCollection<BalanceKindViewModel> BalanceKindVMList
        {
            get { return _BalanceKindVMList; }
            set {
                if (_BalanceKindVMList != value) {
                    _BalanceKindVMList = value;
                    PropertyChanged?.Raise(this, _nameBalanceKindVMList);
                }
            }
        }
        private ObservableCollection<BalanceKindViewModel> _BalanceKindVMList = new ObservableCollection<BalanceKindViewModel>() {
            new BalanceKindViewModel() { BalanceKind = BalanceKind.Income, BalanceKindName = BalanceStr[BalanceKind.Income]}, // 収入
            new BalanceKindViewModel() { BalanceKind = BalanceKind.Outgo, BalanceKindName = BalanceStr[BalanceKind.Outgo]}, // 支出
        };
        internal static readonly string _nameBalanceKindVMList = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.BalanceKindVMList);
        #endregion
        /// <summary>
        /// 選択された収支VM
        /// </summary>
        #region SelectedBalanceKindVM
        public BalanceKindViewModel SelectedBalanceKindVM
        {
            get { return _SelectedBalanceKindVM; }
            set {
                if (_SelectedBalanceKindVM != value) {
                    _SelectedBalanceKindVM = value;
                    PropertyChanged?.Raise(this, _nameSelectedBalanceKindVM);
                    
                    if (!UpdateOnChanged) {
                        UpdateOnChanged = true;
                        OnBalanceKindChanged?.Invoke();
                        UpdateOnChanged = false;
                    }
                }
            }
        }
        private BalanceKindViewModel _SelectedBalanceKindVM = default(BalanceKindViewModel);
        internal static readonly string _nameSelectedBalanceKindVM = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.SelectedBalanceKindVM);
        #endregion
        
        /// <summary>
        /// カテゴリVMリスト
        /// </summary>
        #region CategoryVMList
        public ObservableCollection<CategoryViewModel> CategoryVMList
        {
            get { return _CategoryVMList; }
            set {
                if (_CategoryVMList != value) {
                    _CategoryVMList = value;
                    PropertyChanged?.Raise(this, _nameCategoryVMList);
                }
            }
        }
        private ObservableCollection<CategoryViewModel> _CategoryVMList = default(ObservableCollection<CategoryViewModel>);
        internal static readonly string _nameCategoryVMList = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.CategoryVMList);
        #endregion
        /// <summary>
        /// 選択されたカテゴリVM
        /// </summary>
        #region SelectedCategoryVM
        public CategoryViewModel SelectedCategoryVM
        {
            get { return _SelectedCategoryVM; }
            set {
                if (_SelectedCategoryVM != value) {
                    _SelectedCategoryVM = value;
                    PropertyChanged?.Raise(this, _nameSelectedCategoryVM);
                    
                    if (!UpdateOnChanged) {
                        UpdateOnChanged = true;
                        OnCategoryChanged?.Invoke();
                        UpdateOnChanged = false;
                    }
                }
            }
        }
        private CategoryViewModel _SelectedCategoryVM = default(CategoryViewModel);
        internal static readonly string _nameSelectedCategoryVM = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.SelectedCategoryVM);
        #endregion

        /// <summary>
        /// 項目VMリスト
        /// </summary>
        #region ItemVMList
        public ObservableCollection<ItemViewModel> ItemVMList
        {
            get { return _ItemVMList; }
            set {
                if (_ItemVMList != value) {
                    _ItemVMList = value;
                    PropertyChanged?.Raise(this, _nameItemVMList);
                }
            }
        }
        private ObservableCollection<ItemViewModel> _ItemVMList = default(ObservableCollection<ItemViewModel>);
        internal static readonly string _nameItemVMList = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.ItemVMList);
        #endregion
        /// <summary>
        /// 選択された項目VM
        /// </summary>
        #region SelectedItemVM
        public ItemViewModel SelectedItemVM
        {
            get { return _SelectedItemVM; }
            set {
                if (_SelectedItemVM != value) {
                    _SelectedItemVM = value;
                    PropertyChanged?.Raise(this, _nameSelectedItemVM);
                    
                    if (!UpdateOnChanged) {
                        UpdateOnChanged = true;
                        OnItemChanged?.Invoke();
                        UpdateOnChanged = false;
                    }
                }
            }
        }
        private ItemViewModel _SelectedItemVM = default(ItemViewModel);
        internal static readonly string _nameSelectedItemVM = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.SelectedItemVM);
        #endregion
        
        /// <summary>
        /// 金額
        /// </summary>
        #region Value
        public int? Value
        {
            get { return _Value; }
            set {
                if (_Value != value) {
                    _Value = value;
                    PropertyChanged?.Raise(this, _nameValue);
                }
            }
        }
        private int? _Value = null;
        internal static readonly string _nameValue = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.Value);
        #endregion

        /// <summary>
        /// 回数
        /// </summary>
        #region Count
        public int Count
        {
            get { return _Count; }
            set {
                if (_Count != value) {
                    _Count = value;
                    PropertyChanged?.Raise(this, _nameCount);
                }
            }
        }
        private int _Count = 1;
        internal static readonly string _nameCount = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.Count);
        #endregion

        /// <summary>
        /// 店舗名リスト
        /// </summary>
        #region ShopNameList
        public ObservableCollection<String> ShopNameList
        {
            get { return _ShopNameList; }
            set {
                if (_ShopNameList != value) {
                    _ShopNameList = value;
                    PropertyChanged?.Raise(this, _nameShopNameList);
                }
            }
        }
        private ObservableCollection<String> _ShopNameList = default(ObservableCollection<String>);
        internal static readonly string _nameShopNameList = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.ShopNameList);
        #endregion
        /// <summary>
        /// 選択された店舗名
        /// </summary>
        #region SelectedShopName
        public String SelectedShopName
        {
            get { return _SelectedShopName; }
            set {
                if (_SelectedShopName != value) {
                    _SelectedShopName = value;
                    PropertyChanged?.Raise(this, _nameSelectedShopName);
                }
            }
        }
        private String _SelectedShopName = default(String);
        internal static readonly string _nameSelectedShopName = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.SelectedShopName);
        #endregion

        /// <summary>
        /// 備考リスト
        /// </summary>
        #region RemarkList
        public ObservableCollection<String> RemarkList
        {
            get { return _RemarkList; }
            set {
                if (_RemarkList != value) {
                    _RemarkList = value;
                    PropertyChanged?.Raise(this, _nameRemarkList);
                }
            }
        }
        private ObservableCollection<String> _RemarkList = default(ObservableCollection<String>);
        internal static readonly string _nameRemarkList = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.RemarkList);
        #endregion
        /// <summary>
        /// 選択された備考
        /// </summary>
        #region SelectedRemark
        public String SelectedRemark
        {
            get { return _SelectedRemark; }
            set {
                if (_SelectedRemark != value) {
                    _SelectedRemark = value;
                    PropertyChanged?.Raise(this, _nameSelectedRemark);
                }
            }
        }
        private String _SelectedRemark = default(String);
        internal static readonly string _nameSelectedRemark = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.SelectedRemark);
        #endregion
        
        /// <summary>
        /// プロパティ変更イベントハンドラ
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
    #endregion
}

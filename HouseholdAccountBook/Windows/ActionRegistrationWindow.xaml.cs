using HouseholdAccountBook.Dao;
using HouseholdAccountBook.ViewModels;
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
                    BookViewModel vm = new BookViewModel() { Id = record.ToInt("book_id"), Name = record["book_name"] };
                    bookVMList.Add(vm);
                    if(selectedBookVM == null || bookId == vm.Id) {
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
                    groupId = record.ToNullableInt("group_id");
                    shopName = record["shop_name"];
                    remark = record["remark"];

                    reader = dao.ExecQuery(@"
SELECT book_id, book_name FROM mst_book WHERE del_flg = 0;");
                    reader.ExecWholeRow((count2, record2) => {
                        BookViewModel vm = new BookViewModel() { Id = record2.ToInt("book_id"), Name = record2["book_name"] };
                        bookVMList.Add(vm);
                        if (selectedBookVM == null || bookId == vm.Id) {
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
        /// 今日コマンド判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TodayCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ActionRegistrationWindowVM.SelectedDate != DateTime.Today;
        }

        /// <summary>
        /// 今日コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TodayCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.ActionRegistrationWindowVM.SelectedDate = DateTime.Today;
        }

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
                new CategoryViewModel() { Id = -1, Name = "(指定なし)" }
            };
            CategoryViewModel selectedCategoryVM = categoryVMList[0];
            using (DaoBase dao = builder.Build()) {
                DaoReader reader = dao.ExecQuery(@"
SELECT category_id, category_name FROM mst_category C 
WHERE del_flg = 0 AND EXISTS (SELECT * FROM mst_item I WHERE I.category_id = C.category_id AND balance_kind = @{0} AND del_flg = 0 
  AND EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @{1} AND RBI.item_id = I.item_id)) 
ORDER BY sort_order;", (int)this.ActionRegistrationWindowVM.SelectedBalanceKindVM.BalanceKind, this.ActionRegistrationWindowVM.SelectedBookVM.Id);
                reader.ExecWholeRow((count, record) => {
                    CategoryViewModel vm = new CategoryViewModel() { Id = record.ToInt("category_id"), Name = record["category_name"] };
                    categoryVMList.Add(vm);
                    if(vm.Id == categoryId) {
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
                if (this.ActionRegistrationWindowVM.SelectedCategoryVM.Id == -1) {
                    reader = dao.ExecQuery(@"
SELECT item_id, item_name FROM mst_item I 
WHERE del_flg = 0 AND EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @{0} AND RBI.item_id = I.item_id AND del_flg = 0)
  AND EXISTS (SELECT * FROM mst_category C WHERE C.category_id = I.category_id AND balance_kind = @{1} AND del_flg = 0)
ORDER BY sort_order;", this.ActionRegistrationWindowVM.SelectedBookVM.Id, (int)this.ActionRegistrationWindowVM.SelectedBalanceKindVM.BalanceKind);
                }
                else {
                    reader = dao.ExecQuery(@"
SELECT item_id, item_name FROM mst_item I 
WHERE del_flg = 0 AND EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @{0} AND RBI.item_id = I.item_id AND del_flg = 0)
  AND category_id = @{1}
ORDER BY sort_order;", this.ActionRegistrationWindowVM.SelectedBookVM.Id, (int)this.ActionRegistrationWindowVM.SelectedCategoryVM.Id);
                }
                reader.ExecWholeRow((count, record) => {
                    ItemViewModel vm = new ItemViewModel() { Id = record.ToInt("item_id"), Name = record["item_name"] };
                    itemVMList.Add(vm);
                    if (selectedItemVM == null || vm.Id == itemId) {
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
ORDER BY used_time DESC;", this.ActionRegistrationWindowVM.SelectedItemVM.Id);
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
ORDER BY used_time DESC;", this.ActionRegistrationWindowVM.SelectedItemVM.Id);
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
            int bookId = ActionRegistrationWindowVM.SelectedBookVM.Id.Value; // 帳簿ID
            int itemId = ActionRegistrationWindowVM.SelectedItemVM.Id; // 帳簿項目ID
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
            Properties.Settings settings = Properties.Settings.Default;

            if (settings.ActionRegistrationWindow_Left != -1) {
                Left = settings.ActionRegistrationWindow_Left;
            }
            if (settings.ActionRegistrationWindow_Top != -1) {
                Top = settings.ActionRegistrationWindow_Top;
            }
            if (settings.ActionRegistrationWindow_Width != -1) {
                Width = settings.ActionRegistrationWindow_Width;
            }
            if (settings.ActionRegistrationWindow_Height != -1) {
                Height = settings.ActionRegistrationWindow_Height;
            }
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        private void SaveSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (this.WindowState == WindowState.Normal) {
                settings.ActionRegistrationWindow_Left = Left;
                settings.ActionRegistrationWindow_Top = Top;
                settings.ActionRegistrationWindow_Width = Width;
                settings.ActionRegistrationWindow_Height = Height;
                settings.Save();
            }
        }
        #endregion
    }
}

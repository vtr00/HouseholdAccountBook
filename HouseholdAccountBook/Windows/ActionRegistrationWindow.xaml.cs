using HouseholdAccountBook.Dao;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.UserEventArgs;
using HouseholdAccountBook.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
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
        private readonly DaoBuilder builder;
        /// <summary>
        /// <see cref="MainWindow"/>で選択された帳簿ID
        /// </summary>
        private readonly int? selectedBookId;
        /// <summary>
        /// <see cref="MainWindow"/>で選択された日付
        /// </summary>
        private readonly DateTime? selectedDateTime;
        /// <summary>
        /// <see cref="CsvComparisonWindow"/>で選択されたCSVレコード
        /// </summary>
        private readonly CsvComparisonViewModel.CsvRecord selectedRecord;
        /// <summary>
        /// <see cref="MainWindow"/>で選択された帳簿項目ID
        /// </summary>
        private readonly int? selectedActionId;
        /// <summary>
        /// グループID
        /// </summary>
        private int? groupId = null;
        #endregion

        #region イベント
        /// <summary>
        /// 登録時のイベント
        /// </summary>
        public event EventHandler<EventArgs<List<int>>> Registrated = null;
        #endregion

        /// <summary>
        /// 帳簿項目の新規登録のために <see cref="ActionRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="builder">DAOビルダ</param>
        /// <param name="selectedBookId">選択された帳簿ID</param>
        /// <param name="selectedDateTime">選択された日時</param>
        public ActionRegistrationWindow(DaoBuilder builder, int? selectedBookId, DateTime? selectedDateTime)
        {
            this.builder = builder;
            this.selectedBookId = selectedBookId;
            this.selectedDateTime = selectedDateTime;
            this.selectedActionId = null;

            this.InitializeComponent();
            this.LoadSetting();

            this.WVM.RegMode = RegistrationMode.Add;
        }

        /// <summary>
        /// 帳簿項目の新規登録のために <see cref="ActionRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="builder">DAOビルダ</param>
        /// <param name="selectedBookId">選択された帳簿ID</param>
        /// <param name="selectedRecord">選択されたCSVレコード</param>
        public ActionRegistrationWindow(DaoBuilder builder, int selectedBookId, CsvComparisonViewModel.CsvRecord selectedRecord)
        {
            this.builder = builder;
            this.selectedBookId = selectedBookId;
            this.selectedDateTime = null;
            this.selectedRecord = selectedRecord;
            this.selectedActionId = null;

            this.InitializeComponent();
            this.LoadSetting();

            this.WVM.RegMode = RegistrationMode.Add;
        }

        /// <summary>
        /// 帳簿項目の編集(複製)のために <see cref="ActionRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="builder">DAOビルダ</param>
        /// <param name="selectedActionId">帳簿項目ID</param>
        /// <param name="mode">登録モード</param>
        public ActionRegistrationWindow(DaoBuilder builder, int selectedActionId, RegistrationMode mode = RegistrationMode.Edit)
        {
            this.builder = builder;
            this.selectedBookId = null;
            this.selectedDateTime = null;
            switch (mode) {
                case RegistrationMode.Edit:
                case RegistrationMode.Copy:
                    this.selectedActionId = selectedActionId;
                    break;
            }

            this.InitializeComponent();
            this.LoadSetting();

            this.WVM.RegMode = mode;
        }

        #region イベントハンドラ
        #region コマンド
        /// <summary>
        /// 今日コマンド判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TodayCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedDate != DateTime.Today;
        }

        /// <summary>
        /// 今日コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TodayCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.WVM.SelectedDate = DateTime.Today;
        }

        /// <summary>
        /// 続けて入力コマンド判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContinueToRegisterCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.RegMode != RegistrationMode.Edit && this.WVM.Value.HasValue;
        }

        /// <summary>
        /// 続けて入力コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ContinueToRegisterCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!this.WVM.Value.HasValue || this.WVM.Value <= 0) {
                MessageBox.Show(this, MessageText.IllegalValue, MessageTitle.Exclamation, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            // DB登録
            int? id = await this.RegisterToDbAsync();

            // MainWindow更新
            List<int> value = id != null ? new List<int>() { id.Value } : new List<int>();
            Registrated?.Invoke(this, new EventArgs<List<int>>(value));

            // 表示クリア
            this.WVM.Value = null;
            this.WVM.Count = 1;
        }

        /// <summary>
        /// 登録コマンド判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegisterCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.Value.HasValue;
        }

        /// <summary>
        /// 登録コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RegisterCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!this.WVM.Value.HasValue || this.WVM.Value <= 0) {
                MessageBox.Show(this, MessageText.IllegalValue, MessageTitle.Exclamation, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            // DB登録
            int? id = await this.RegisterToDbAsync();

            // MainWindow更新
            List<int> value = id != null ? new List<int>() { id.Value } : new List<int>();
            Registrated?.Invoke(this, new EventArgs<List<int>>(value));

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

        /// <summary>
        /// フォーム読込完了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ActionRegistrationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ObservableCollection<BookViewModel> bookVMList = new ObservableCollection<BookViewModel>();
            int? bookId = null;
            BookViewModel selectedBookVM = null;
            DateTime actDate = DateTime.Now;
            BalanceKind balanceKind = BalanceKind.Outgo;

            int? itemId = null;
            int? actValue = null;
            bool isMatch = false;
            string shopName = null;
            string remark = null;

            switch (this.WVM.RegMode) {
                case RegistrationMode.Add: {
                        bookId = this.selectedBookId;
                        balanceKind = BalanceKind.Outgo;
                        if (this.selectedRecord == null) {
                            actDate = this.selectedDateTime != null ? this.selectedDateTime.Value : DateTime.Today;
                        }
                        else {
                            actDate = this.selectedRecord.Date;
                            actValue = this.selectedRecord.Value;
                        }
                    }
                    break;
                case RegistrationMode.Edit:
                case RegistrationMode.Copy: {
                        using (DaoBase dao = this.builder.Build()) {
                            DaoReader reader = await dao.ExecQueryAsync(@"
SELECT book_id, item_id, act_time, act_value, group_id, shop_name, remark, is_match
FROM hst_action 
WHERE del_flg = 0 AND action_id = @{0};", this.selectedActionId);
                            reader.ExecARow((record) => {
                                bookId = record.ToInt("book_id");
                                itemId = record.ToInt("item_id");
                                actDate = record.ToDateTime("act_time");
                                actValue = record.ToInt("act_value");
                                this.groupId = record.ToNullableInt("group_id");
                                shopName = record["shop_name"];
                                remark = record["remark"];
                                isMatch = record.ToInt("is_match") == 1;
                            });
                        }
                        balanceKind = Math.Sign(actValue.Value) > 0 ? BalanceKind.Income : BalanceKind.Outgo; // 収入 / 支出

                        // 回数の表示
                        int count = 1;
                        if (this.groupId != null) {
                            using (DaoBase dao = this.builder.Build()) {
                                DaoReader reader = await dao.ExecQueryAsync(@"
SELECT COUNT(action_id) count FROM hst_action 
WHERE del_flg = 0 AND group_id = @{0} AND act_time >= (SELECT act_time FROM hst_action WHERE action_id = @{1});", this.groupId, this.selectedActionId);
                                reader.ExecARow((record) => {
                                    count = record.ToInt("count");
                                });
                            }
                        }
                        this.WVM.Count = count;
                    }
                    break;
            }

            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader = await dao.ExecQueryAsync(@"
SELECT book_id, book_name FROM mst_book WHERE del_flg = 0 ORDER BY sort_order;");
                reader.ExecWholeRow((count, record) => {
                    BookViewModel vm = new BookViewModel() { Id = record.ToInt("book_id"), Name = record["book_name"] };
                    bookVMList.Add(vm);
                    if (selectedBookVM == null || bookId == vm.Id) {
                        selectedBookVM = vm;
                    }
                    return true;
                });
            }

            this.WVM.BookVMList = bookVMList;
            this.WVM.SelectedBookVM = selectedBookVM;
            this.WVM.SelectedBalanceKind = balanceKind;
            this.WVM.SelectedDate = actDate;

            this.WVM.Value = actValue.HasValue ? Math.Abs(actValue.Value) : (int?)null;
            this.WVM.IsMatch = isMatch;

            await this.UpdateCategoryListAsync();
            await this.UpdateItemListAsync(itemId);
            await this.UpdateShopListAsync(shopName);
            await this.UpdateRemarkListAsync(remark);

            #region イベントハンドラの設定
            this.WVM.BookChanged += async () => {
                await this.UpdateCategoryListAsync();
                await this.UpdateItemListAsync();
                await this.UpdateShopListAsync();
                await this.UpdateRemarkListAsync();
            };
            this.WVM.BalanceKindChanged += async () => {
                await this.UpdateCategoryListAsync();
                await this.UpdateItemListAsync();
                await this.UpdateShopListAsync();
                await this.UpdateRemarkListAsync();
            };
            this.WVM.CategoryChanged += async () => {
                await this.UpdateItemListAsync();
                await this.UpdateShopListAsync();
                await this.UpdateRemarkListAsync();
            };
            this.WVM.ItemChanged += async () => {
                await this.UpdateShopListAsync();
                await this.UpdateRemarkListAsync();
            };
            #endregion
        }

        /// <summary>
        /// フォーム終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActionRegistrationWindow_Closed(object sender, EventArgs e)
        {
            this.SaveSetting();
        }
        #endregion

        #region 画面更新用の関数
        /// <summary>
        /// カテゴリリストを更新する
        /// </summary>
        /// <param name="categoryId">選択対象のカテゴリ</param>
        private async Task UpdateCategoryListAsync(int? categoryId = null)
        {
            ObservableCollection<CategoryViewModel> categoryVMList = new ObservableCollection<CategoryViewModel>() {
                new CategoryViewModel() { Id = -1, Name = "(指定なし)" }
            };
            int? tmpCategoryId = categoryId ?? this.WVM.SelectedCategoryVM?.Id ?? categoryVMList[0].Id;
            CategoryViewModel selectedCategoryVM = categoryVMList[0];
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader = await dao.ExecQueryAsync(@"
SELECT category_id, category_name FROM mst_category C 
WHERE del_flg = 0 AND EXISTS (SELECT * FROM mst_item I WHERE I.category_id = C.category_id AND balance_kind = @{0} AND del_flg = 0 
  AND EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @{1} AND RBI.item_id = I.item_id)) 
ORDER BY sort_order;", (int)this.WVM.SelectedBalanceKind, this.WVM.SelectedBookVM.Id);
                reader.ExecWholeRow((count, record) => {
                    CategoryViewModel vm = new CategoryViewModel() { Id = record.ToInt("category_id"), Name = record["category_name"] };
                    categoryVMList.Add(vm);
                    if (vm.Id == categoryId) {
                        selectedCategoryVM = vm;
                    }
                    return true;
                });
            }
            this.WVM.CategoryVMList = categoryVMList;
            this.WVM.SelectedCategoryVM = selectedCategoryVM;
        }

        /// <summary>
        /// 項目リストを更新する
        /// </summary>
        /// <param name="itemId">選択対象の項目</param>
        private async Task UpdateItemListAsync(int? itemId = null)
        {
            if (this.WVM.SelectedCategoryVM == null) return;

            ObservableCollection<ItemViewModel> itemVMList = new ObservableCollection<ItemViewModel>();
            int? tmpItemId = itemId ?? this.WVM.SelectedItemVM?.Id;
            ItemViewModel selectedItemVM = null;
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader;
                if (this.WVM.SelectedCategoryVM.Id == -1) {
                    reader = await dao.ExecQueryAsync(@"
SELECT item_id, item_name FROM mst_item I 
WHERE del_flg = 0 AND EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @{0} AND RBI.item_id = I.item_id AND del_flg = 0)
  AND EXISTS (SELECT * FROM mst_category C WHERE C.category_id = I.category_id AND balance_kind = @{1} AND del_flg = 0)
ORDER BY sort_order;", this.WVM.SelectedBookVM.Id, (int)this.WVM.SelectedBalanceKind);
                }
                else {
                    reader = await dao.ExecQueryAsync(@"
SELECT item_id, item_name FROM mst_item I 
WHERE del_flg = 0 AND EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @{0} AND RBI.item_id = I.item_id AND del_flg = 0)
  AND category_id = @{1}
ORDER BY sort_order;", this.WVM.SelectedBookVM.Id, (int)this.WVM.SelectedCategoryVM.Id);
                }
                reader.ExecWholeRow((count, record) => {
                    ItemViewModel vm = new ItemViewModel() { Id = record.ToInt("item_id"), Name = record["item_name"] };
                    itemVMList.Add(vm);
                    if (selectedItemVM == null || vm.Id == tmpItemId) {
                        selectedItemVM = vm;
                    }
                    return true;
                });
            }
            this.WVM.ItemVMList = itemVMList;
            this.WVM.SelectedItemVM = selectedItemVM;
        }

        /// <summary>
        /// 店舗リストを更新する
        /// </summary>
        /// <param name="shopName">選択対象の店舗名</param>
        private async Task UpdateShopListAsync(string shopName = null)
        {
            if (this.WVM.SelectedItemVM == null) return;

            ObservableCollection<string> shopNameVMList = new ObservableCollection<string>() {
                string.Empty
            };
            string selectedShopName = shopName ?? this.WVM.SelectedShopName ?? shopNameVMList[0];
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader = await dao.ExecQueryAsync(@"
SELECT shop_name FROM hst_shop 
WHERE del_flg = 0 AND item_id = @{0} 
ORDER BY used_time DESC;", this.WVM.SelectedItemVM.Id);
                reader.ExecWholeRow((count, record) => {
                    string tmp = record["shop_name"];
                    shopNameVMList.Add(tmp);
                    return true;
                });
            }

            this.WVM.ShopNameList = shopNameVMList;
            this.WVM.SelectedShopName = selectedShopName;
        }

        /// <summary>
        /// 備考リストを更新する
        /// </summary>
        /// <param name="remark">選択対象の備考</param>
        private async Task UpdateRemarkListAsync(string remark = null)
        {
            if (this.WVM.SelectedItemVM == null) return;

            ObservableCollection<string> remarkVMList = new ObservableCollection<string>() {
                    string.Empty
            };
            string selectedRemark = remark ?? this.WVM.SelectedRemark ?? remarkVMList[0];
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader = await dao.ExecQueryAsync(@"
SELECT remark FROM hst_remark 
WHERE del_flg = 0 AND item_id = @{0} 
ORDER BY used_time DESC;", this.WVM.SelectedItemVM.Id);
                reader.ExecWholeRow((count, record) => {
                    string tmp = record["remark"];
                    remarkVMList.Add(tmp);
                    return true;
                });
            }

            this.WVM.RemarkList = remarkVMList;
            this.WVM.SelectedRemark = selectedRemark;
        }
        #endregion

        /// <summary>
        /// DBに登録する
        /// </summary>
        /// <returns>登録された帳簿項目ID</returns>
        private async Task<int?> RegisterToDbAsync()
        {
            BalanceKind balanceKind = this.WVM.SelectedBalanceKind; // 収支種別
            int bookId = this.WVM.SelectedBookVM.Id.Value; // 帳簿ID
            int itemId = this.WVM.SelectedItemVM.Id; // 帳簿項目ID
            DateTime actTime = this.WVM.SelectedDate; // 入力日付
            int actValue = (balanceKind == BalanceKind.Income ? 1 : -1) * this.WVM.Value.Value; // 値
            string shopName = this.WVM.SelectedShopName; // 店舗名
            string remark = this.WVM.SelectedRemark; // 備考
            int count = this.WVM.Count; // 繰返し回数
            bool isLink = this.WVM.IsLink;
            int isMatch = this.WVM.IsMatch == true ? 1 : 0;
            HolidaySettingKind holidaySettingKind = this.WVM.SelectedHolidaySettingKind;

            int? resActionId = null;

            // 休日設定を考慮した日付を取得する関数
            DateTime getDateTimeWithHolidaySettingKind(DateTime tmpDateTime)
            {
                switch (holidaySettingKind) {
                    case HolidaySettingKind.BeforeHoliday:
                        while (tmpDateTime.IsNationalHoliday() || tmpDateTime.DayOfWeek == DayOfWeek.Saturday || tmpDateTime.DayOfWeek == DayOfWeek.Sunday) {
                            tmpDateTime = tmpDateTime.AddDays(-1);
                        }
                        break;
                    case HolidaySettingKind.AfterHoliday:
                        while (tmpDateTime.IsNationalHoliday() || tmpDateTime.DayOfWeek == DayOfWeek.Saturday || tmpDateTime.DayOfWeek == DayOfWeek.Sunday) {
                            tmpDateTime = tmpDateTime.AddDays(1);
                        }
                        break;
                }
                return tmpDateTime;
            }

            using (DaoBase dao = this.builder.Build()) {
                switch (this.WVM.RegMode) {
                    case RegistrationMode.Add:
                    case RegistrationMode.Copy: {
                            #region 帳簿項目を追加する
                            if (count == 1) { // 繰返し回数が1回(繰返しなし)
                                DaoReader reader = await dao.ExecQueryAsync(@"
    INSERT INTO hst_action (book_id, item_id, act_time, act_value, shop_name, remark, is_match, del_flg, update_time, updater, insert_time, inserter)
    VALUES (@{0}, @{1}, @{2}, @{3}, @{4}, @{5}, 0, 0, 'now', @{6}, 'now', @{7}) RETURNING action_id;",
                                    bookId, itemId, actTime, actValue, shopName, remark, Updater, Inserter);

                                reader.ExecARow((record) => {
                                    resActionId = record.ToInt("action_id");
                                });
                            }
                            else { // 繰返し回数が2回以上(繰返しあり)
                                await dao.ExecTransactionAsync(async () => {
                                    int tmpGroupId = -1;
                                    // グループIDを取得する
                                    DaoReader reader = await dao.ExecQueryAsync(@"
    INSERT INTO hst_group (group_kind, del_flg, update_time, updater, insert_time, inserter)
    VALUES (@{0}, 0, 'now', @{1}, 'now', @{2}) RETURNING group_id;", (int)GroupKind.Repeat, Updater, Inserter);
                                    reader.ExecARow((record) => {
                                        tmpGroupId = record.ToInt("group_id");
                                    });

                                    DateTime tmpActTime = getDateTimeWithHolidaySettingKind(actTime); // 登録日付
                                    for (int i = 0; i < count; ++i) {
                                        reader = await dao.ExecQueryAsync(@"
    INSERT INTO hst_action (book_id, item_id, act_time, act_value, shop_name, group_id, remark, is_match, del_flg, update_time, updater, insert_time, inserter)
    VALUES (@{0}, @{1}, @{2}, @{3}, @{4}, @{5}, @{6}, 0, 0, 'now', @{7}, 'now', @{8}) RETURNING action_id;",
                                            bookId, itemId, tmpActTime, actValue, shopName, tmpGroupId, remark, Updater, Inserter);

                                        // 繰り返しの最初の1回を選択するようにする
                                        if (i == 0) {
                                            reader.ExecARow((record) => {
                                                resActionId = record.ToInt("action_id");
                                            });
                                        }

                                        tmpActTime = getDateTimeWithHolidaySettingKind(actTime.AddMonths(i + 1));
                                    }
                                });
                            }
                            #endregion
                        }
                        break;
                    case RegistrationMode.Edit: {
                            #region 帳簿項目を編集する
                            if (count == 1) {
                                #region 繰返し回数が1回
                                if (this.groupId == null) {
                                    #region グループに属していない
                                    await dao.ExecNonQueryAsync(@"
UPDATE hst_action
SET book_id = @{0}, item_id = @{1}, act_time = @{2}, act_value = @{3}, shop_name = @{4}, remark = @{5}, is_match = @{6}, update_time = 'now', updater = @{7}
WHERE action_id = @{8};", bookId, itemId, actTime, actValue, shopName, remark, isMatch, Updater, this.selectedActionId);
                                    #endregion
                                }
                                else {
                                    #region グループに属している
                                    await dao.ExecTransactionAsync(async () => {
                                        // この帳簿項目以降の繰返し分のレコードを削除する
                                        await dao.ExecNonQueryAsync(@"
UPDATE hst_action
SET del_flg = 1, update_time = 'now', updater = @{0}
WHERE del_flg = 0 AND group_id = @{1} AND act_time > (SELECT act_time FROM hst_action WHERE action_id = @{2});", Updater, this.groupId, this.selectedActionId);

                                        // グループに属する項目の個数を調べる
                                        DaoReader reader = await dao.ExecQueryAsync(@"
SELECT action_id FROM hst_action
WHERE del_flg = 0 AND group_id = @{0};", this.groupId);

                                        if (reader.Count <= 1) {
                                            #region グループに属する項目が1項目以下
                                            // この帳簿項目のグループIDをクリアする
                                            await dao.ExecNonQueryAsync(@"
UPDATE hst_action
SET book_id = @{0}, item_id = @{1}, act_time = @{2}, act_value = @{3}, shop_name = @{4}, group_id = null, remark = @{5}, is_match = @{6}, update_time = 'now', updater = @{7}
WHERE action_id = @{8};", bookId, itemId, actTime, actValue, shopName, remark, isMatch, Updater, this.selectedActionId);

                                            // グループを削除する
                                            await dao.ExecNonQueryAsync(@"
UPDATE hst_group
SET del_flg = 1, update_time = 'now', updater = @{0}
WHERE del_flg = 0 AND group_id = @{1};", Updater, this.groupId);
                                            #endregion
                                        }
                                        else {
                                            #region グループに属する項目が2項目以上
                                            // この帳簿項目のグループIDをクリアせずに残す
                                            await dao.ExecNonQueryAsync(@"
UPDATE hst_action
SET book_id = @{0}, item_id = @{1}, act_time = @{2}, act_value = @{3}, shop_name = @{4}, remark = @{5}, is_match = @{6}, update_time = 'now', updater = @{7}
WHERE action_id = @{8};", bookId, itemId, actTime, actValue, shopName, remark, isMatch, Updater, this.selectedActionId);
                                            #endregion
                                        }
                                    });
                                    #endregion
                                }
                                #endregion
                            }
                            else {
                                #region 繰返し回数が2回以上
                                await dao.ExecTransactionAsync(async () => {
                                    List<int> actionIdList = new List<int>();

                                    DaoReader reader;
                                    if (this.groupId == null) {
                                        #region グループIDが未割当て
                                        // グループIDを取得する
                                        reader = await dao.ExecQueryAsync(@"
INSERT INTO hst_group (group_kind, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, 0, 'now', @{1}, 'now', @{2}) RETURNING group_id;", (int)GroupKind.Repeat, Updater, Inserter);
                                        reader.ExecARow((record) => {
                                            this.groupId = record.ToInt("group_id");
                                        });
                                        actionIdList.Add(this.selectedActionId.Value);
                                        #endregion
                                    }
                                    else {
                                        #region グループIDが割当て済
                                        // 変更の対象となる帳簿項目を洗い出す
                                        reader = await dao.ExecQueryAsync(@"
SELECT action_id FROM hst_action 
WHERE del_flg = 0 AND group_id = @{0} AND act_time >= (SELECT act_time FROM hst_action WHERE action_id = @{1})
ORDER BY act_time ASC;", this.groupId, this.selectedActionId);
                                        reader.ExecWholeRow((recCount, record) => {
                                            actionIdList.Add(record.ToInt("action_id"));
                                            return true;
                                        });
                                        #endregion
                                    }

                                    DateTime tmpActTime = getDateTimeWithHolidaySettingKind(actTime);

                                    // この帳簿項目にだけis_matchを反映する
                                    Debug.Assert(actionIdList[0] == this.selectedActionId);
                                    await dao.ExecNonQueryAsync(@"
UPDATE hst_action
SET book_id = @{0}, item_id = @{1}, act_time = @{2}, act_value = @{3}, shop_name = @{4}, group_id = @{5}, remark = @{6}, is_match = @{7}, update_time = 'now', updater = @{8}
WHERE action_id = @{9};", bookId, itemId, tmpActTime, actValue, shopName, this.groupId, remark, isMatch, Updater, this.selectedActionId);

                                    tmpActTime = getDateTimeWithHolidaySettingKind(actTime.AddMonths(1));
                                    for (int i = 1; i < actionIdList.Count; ++i) {
                                        int targetActionId = actionIdList[i];

                                        if (i < count) { // 繰返し回数の範囲内のレコードを更新する
                                            // 連動して編集時のみ変更する
                                            if (isLink) {
                                                await dao.ExecNonQueryAsync(@"
UPDATE hst_action
SET book_id = @{0}, item_id = @{1}, act_time = @{2}, act_value = @{3}, shop_name = @{4}, group_id = @{5}, remark = @{6}, update_time = 'now', updater = @{7}
WHERE action_id = @{8};", bookId, itemId, tmpActTime, actValue, shopName, this.groupId, remark, Updater, targetActionId);
                                            }
                                        }
                                        else { // 繰返し回数が帳簿項目数を下回っていた場合に、越えたレコードを削除する
                                            await dao.ExecNonQueryAsync(@"
UPDATE hst_action
SET del_flg = 1, update_time = 'now', updater = @{0}
WHERE action_id = @{1};", Updater, targetActionId);
                                        }

                                        tmpActTime = getDateTimeWithHolidaySettingKind(actTime.AddMonths(i + 1));
                                    }

                                    // 繰返し回数が帳簿項目数を越えていた場合に、新規レコードを追加する
                                    for (int i = actionIdList.Count; i < count; ++i) {
                                        await dao.ExecNonQueryAsync(@"
INSERT INTO hst_action (book_id, item_id, act_time, act_value, shop_name, group_id, remark, is_match, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, @{2}, @{3}, @{4}, @{5}, @{6}, 0, 0, 'now', @{7}, 'now', @{8});", bookId, itemId, tmpActTime, actValue, shopName, this.groupId, remark, Updater, Inserter);

                                        tmpActTime = getDateTimeWithHolidaySettingKind(actTime.AddMonths(i + 1));
                                    }
                                });
                                #endregion
                            }

                            resActionId = this.selectedActionId;
                            #endregion
                        }
                        break;
                }

                if (shopName != string.Empty) {
                    #region 店舗を追加する
                    await dao.ExecTransactionAsync(async () => {
                        DaoReader reader = await dao.ExecQueryAsync(@"
SELECT shop_name FROM hst_shop
WHERE item_id = @{0} AND shop_name = @{1};", itemId, shopName);

                        if (reader.Count == 0) {
                            await dao.ExecNonQueryAsync(@"
INSERT INTO hst_shop (item_id, shop_name, used_time, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, @{2}, 0, 'now', @{3}, 'now', @{4});", itemId, shopName, actTime, Updater, Inserter);
                        }
                        else {
                            await dao.ExecNonQueryAsync(@"
UPDATE hst_shop
SET used_time = @{0}, del_flg = 0, update_time = 'now', updater = @{1}
WHERE item_id = @{2} AND shop_name = @{3} AND used_time < @{0};", actTime, Updater, itemId, shopName);
                        }
                    });
                    #endregion
                }

                if (remark != string.Empty) {
                    #region 備考を追加する
                    await dao.ExecTransactionAsync(async () => {
                        DaoReader reader = await dao.ExecQueryAsync(@"
SELECT remark FROM hst_remark
WHERE item_id = @{0} AND remark = @{1};", itemId, remark);

                        if (reader.Count == 0) {
                            await dao.ExecNonQueryAsync(@"
INSERT INTO hst_remark (item_id, remark, remark_kind, used_time, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, 0, @{2}, 0, 'now', @{3}, 'now', @{4});", itemId, remark, actTime, Updater, Inserter);
                        }
                        else {
                            await dao.ExecNonQueryAsync(@"
UPDATE hst_remark
SET used_time = @{0}, del_flg = 0, update_time = 'now', updater = @{1}
WHERE item_id = @{2} AND remark = @{3} AND used_time < @{0};", actTime, Updater, itemId, remark);
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

            if (0 <= settings.ActionRegistrationWindow_Left) {
                this.Left = settings.ActionRegistrationWindow_Left;
            }
            if (0 <= settings.ActionRegistrationWindow_Top) {
                this.Top = settings.ActionRegistrationWindow_Top;
            }
            if (settings.ActionRegistrationWindow_Width != -1) {
                this.Width = settings.ActionRegistrationWindow_Width;
            }
            if (settings.ActionRegistrationWindow_Height != -1) {
                this.Height = settings.ActionRegistrationWindow_Height;
            }
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        private void SaveSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (this.WindowState == WindowState.Normal) {
                settings.ActionRegistrationWindow_Left = this.Left;
                settings.ActionRegistrationWindow_Top = this.Top;
                settings.ActionRegistrationWindow_Width = this.Width;
                settings.ActionRegistrationWindow_Height = this.Height;
                settings.Save();
            }
        }
        #endregion
    }
}

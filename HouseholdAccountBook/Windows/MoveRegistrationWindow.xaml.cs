using HouseholdAccountBook.Dao;
using HouseholdAccountBook.ViewModels;
using HouseholdAccountBook.UserEventArgs;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using static HouseholdAccountBook.ConstValue.ConstValue;
using System.Linq;
using HouseholdAccountBook.Extentions;

namespace HouseholdAccountBook.Windows
{
    /// <summary>
    /// MoveRegistrationWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MoveRegistrationWindow : Window
    {
        #region フィールド
        /// <summary>
        /// DAOビルダ
        /// </summary>
        private DaoBuilder builder;
        /// <summary>
        /// 選択されていた帳簿ID
        /// </summary>
        private int? selectedBookId = null;
        /// <summary>
        /// 移動元帳簿項目ID
        /// </summary>
        private int? fromActionId = null;
        /// <summary>
        /// 移動先帳簿項目ID
        /// </summary>
        private int? toActionId = null;
        /// <summary>
        /// 手数料帳簿項目ID
        /// </summary>
        private int? commissionActionId = null;
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
        /// 帳簿項目追加ウィンドウ(移動)
        /// </summary>
        /// <param name="builder">DAOビルダ</param>
        /// <param name="selectedBookId">帳簿ID</param>
        /// <param name="selectedDateTime">選択された日時</param>
        public MoveRegistrationWindow(DaoBuilder builder, int? selectedBookId, DateTime? selectedDateTime)
        {
            InitializeComponent();
            Title = "移動(追加)";

            this.builder = builder;
            this.selectedBookId = selectedBookId;
            this.fromActionId = null;
            this.toActionId = null;
            this.commissionActionId = null;
            this.groupId = null;

            int? debitBookId = null;
            int? payDay = null;
            ObservableCollection<BookViewModel> bookVMList = new ObservableCollection<BookViewModel>();
            BookViewModel selectedBookVM = null;
            using (DaoBase dao = builder.Build()) {
                DaoReader reader = dao.ExecQuery("SELECT book_id, book_name, book_kind, debit_book_id, pay_day FROM mst_book WHERE del_flg = 0;");
                reader.ExecWholeRow((count, record) => {
                    BookViewModel vm = new BookViewModel() { Id = record.ToInt("book_id"), Name = record["book_name"] };
                    bookVMList.Add(vm);

                    if (selectedBookVM == null || selectedBookId == vm.Id) {
                        selectedBookVM = vm;

                        if(record.ToInt("book_kind") == (int)BookKind.CreditCard) {
                            debitBookId = record.ToNullableInt("debit_book_id");
                            payDay = record.ToNullableInt("pay_day");
                        }
                    }
                    return true;
                });
            }

            this.MoveRegistrationWindowVM.BookVMList = bookVMList;
            this.MoveRegistrationWindowVM.SelectedDate = selectedDateTime != null ? (payDay != null ? selectedDateTime.Value.GetDateInMonth(payDay.Value) : selectedDateTime.Value) : DateTime.Today;
            this.MoveRegistrationWindowVM.SelectedFromBookVM = debitBookId != null ? bookVMList.FirstOrDefault((vm)=> { return vm.Id == debitBookId; }) : selectedBookVM;
            this.MoveRegistrationWindowVM.SelectedToBookVM = selectedBookVM;
            this.MoveRegistrationWindowVM.SelectedCommissionKind = CommissionKind.FromBook;

            UpdateItemList();
            UpdateRemarkList();

            LoadSetting();

            #region イベントハンドラの設定
            this.MoveRegistrationWindowVM.OnFromBookChanged += () => {
                UpdateItemList();
                UpdateRemarkList();
            };
            this.MoveRegistrationWindowVM.OnToBookChanged += () => {
                UpdateItemList();
                UpdateRemarkList();
            };
            this.MoveRegistrationWindowVM.OnCommissionKindChanged += () => {
                UpdateItemList();
                UpdateRemarkList();
            };
            this.MoveRegistrationWindowVM.OnItemChanged += () => {
                UpdateRemarkList();
            };
            #endregion
        }

        /// <summary>
        /// 帳簿項目編集ウィンドウ(移動)
        /// </summary>
        /// <param name="builder">DAOビルダ</param>
        /// <param name="selectedBookId">帳簿ID</param>
        /// <param name="groupId">グループID</param>
        public MoveRegistrationWindow(DaoBuilder builder, int? selectedBookId, int groupId)
        {
            InitializeComponent();
            Title = "移動(編集)";

            this.builder = builder;
            this.selectedBookId = selectedBookId;
            this.groupId = groupId;

            ObservableCollection<BookViewModel> bookVMList = new ObservableCollection<BookViewModel>();
            int fromBookId = -1;
            BookViewModel selectedFromBookVM = null;
            int toBookId = -1;
            BookViewModel selectedToBookVM = null;
            DateTime actDate = DateTime.Now;
            int moveValue = -1;
            CommissionKind commissionKind = CommissionKind.FromBook;
            int? commissionItemId = null;
            int commissionValue = 0;
            string commissionRemark = null;
            using (DaoBase dao = builder.Build()) {
                DaoReader reader = dao.ExecQuery(@"
SELECT A.book_id, A.action_id, A.item_id, A.act_time, A.act_value, A.remark, I.move_flg
FROM hst_action A
INNER JOIN (SELECT * FROM mst_item WHERE del_flg = 0) I ON I.item_id = A.item_id
WHERE A.del_flg = 0 AND A.group_id = @{0}
ORDER BY move_flg DESC;", groupId);
                reader.ExecWholeRow((count, record) => {
                    int bookId = record.ToInt("book_id");
                    int actionId = record.ToInt("action_id");
                    int itemId = record.ToInt("item_id");
                    actDate = DateTime.Parse(record["act_time"]);
                    int actValue = record.ToInt("act_value");
                    int moveFlg = record.ToInt("move_flg");
                    string remark = record["remark"];
                    
                    if (moveFlg == 1) {
                        if(actValue < 0) {
                            fromBookId = bookId;
                            fromActionId = actionId;
                        }
                        else {
                            toBookId = bookId;
                            toActionId = actionId;
                            moveValue = actValue;
                        }
                    }
                    else {
                        if (bookId == fromBookId) { // 移動元負担
                            commissionKind = CommissionKind.FromBook;
                        }
                        else if (bookId == toBookId) { // 移動先負担
                            commissionKind = CommissionKind.ToBook;
                        }
                        commissionActionId = actionId;
                        commissionItemId = itemId;
                        commissionValue = Math.Abs(actValue);
                        commissionRemark = remark;
                    }
                    return true;
                });

                reader = dao.ExecQuery(@"
SELECT book_id, book_name FROM mst_book WHERE del_flg = 0;");
                reader.ExecWholeRow((count, record) => {
                    BookViewModel vm = new BookViewModel() { Id = record.ToInt("book_id"), Name = record["book_name"] };
                    bookVMList.Add(vm);
                    if (selectedFromBookVM == null || fromBookId == vm.Id) {
                        selectedFromBookVM = vm;
                    }
                    if (selectedToBookVM == null || toBookId == vm.Id) {
                        selectedToBookVM = vm;
                    }
                    return true;
                });
            }
            this.MoveRegistrationWindowVM.SelectedDate = actDate;
            this.MoveRegistrationWindowVM.BookVMList = bookVMList;
            this.MoveRegistrationWindowVM.SelectedFromBookVM = selectedFromBookVM;
            this.MoveRegistrationWindowVM.SelectedToBookVM = selectedToBookVM;
            this.MoveRegistrationWindowVM.Value = moveValue;
            this.MoveRegistrationWindowVM.SelectedCommissionKind = commissionKind;
            this.MoveRegistrationWindowVM.Commission = commissionValue;

            UpdateItemList(commissionItemId);
            UpdateRemarkList(commissionRemark);

            LoadSetting();

            #region イベントハンドラの設定
            this.MoveRegistrationWindowVM.OnFromBookChanged += () => {
                UpdateItemList();
                UpdateRemarkList();
            };
            this.MoveRegistrationWindowVM.OnToBookChanged += () => {
                UpdateItemList();
                UpdateRemarkList();
            };
            this.MoveRegistrationWindowVM.OnCommissionKindChanged += () => {
                UpdateItemList();
                UpdateRemarkList();
            };
            this.MoveRegistrationWindowVM.OnItemChanged += () => {
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
            e.CanExecute = this.MoveRegistrationWindowVM.SelectedDate != DateTime.Today;
        }

        /// <summary>
        /// 今日コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TodayCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.MoveRegistrationWindowVM.SelectedDate = DateTime.Today;
        }

        /// <summary>
        /// 登録コマンド判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegisterCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.MoveRegistrationWindowVM.Value.HasValue;
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
        private void MoveRegistrationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 処理なし
        }

        /// <summary>
        /// フォーム終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveRegistrationWindow_Closing(object sender, CancelEventArgs e)
        {
            SaveSetting();
        }
        #endregion

        #region 画面更新用の関数
        /// <summary>
        /// 手数料項目リストを更新する
        /// </summary>
        /// <param name="itemId">選択対象の項目</param>
        private void UpdateItemList(int? itemId = null)
        {
            ObservableCollection<ItemViewModel> itemVMList = new ObservableCollection<ItemViewModel>();
            ItemViewModel selectedItemVM = null;
            using (DaoBase dao = builder.Build()) {
                int bookId = -1;
                switch (this.MoveRegistrationWindowVM.SelectedCommissionKind) {
                    case CommissionKind.FromBook:
                        bookId = this.MoveRegistrationWindowVM.SelectedFromBookVM.Id.Value;
                        break;
                    case CommissionKind.ToBook:
                        bookId = this.MoveRegistrationWindowVM.SelectedToBookVM.Id.Value;
                        break;
                }
                DaoReader reader = dao.ExecQuery(@"
SELECT item_id, item_name FROM mst_item I 
WHERE del_flg = 0 AND EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @{0} AND RBI.item_id = I.item_id AND del_flg = 0)
  AND EXISTS (SELECT * FROM mst_category C WHERE C.category_id = I.category_id AND balance_kind = @{1} AND del_flg = 0)
ORDER BY sort_order;", bookId, (int)BalanceKind.Outgo);

                reader.ExecWholeRow((count, record) => {
                    ItemViewModel vm = new ItemViewModel() { Id = record.ToInt("item_id"), Name = record["item_name"] };
                    itemVMList.Add(vm);
                    if (selectedItemVM == null || vm.Id == itemId) {
                        selectedItemVM = vm;
                    }
                    return true;
                });
            }
            this.MoveRegistrationWindowVM.ItemVMList = itemVMList;
            this.MoveRegistrationWindowVM.SelectedItemVM = selectedItemVM;
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
            string selectedRemark = remark ?? this.MoveRegistrationWindowVM.SelectedRemark ?? remarkVMList[0];
            using (DaoBase dao = builder.Build()) {
                DaoReader reader = dao.ExecQuery(@"
SELECT remark FROM hst_remark 
WHERE del_flg = 0 AND item_id = @{0} 
ORDER BY used_time DESC;", this.MoveRegistrationWindowVM.SelectedItemVM.Id);
                reader.ExecWholeRow((count, record) => {
                    string tmp = record["remark"];
                    remarkVMList.Add(tmp);
                    return true;
                });
            }

            this.MoveRegistrationWindowVM.RemarkList = remarkVMList;
            this.MoveRegistrationWindowVM.SelectedRemark = selectedRemark;
        }
        #endregion

        /// <summary>
        /// DBに登録する
        /// </summary>
        /// <returns>登録された帳簿項目ID</returns>
        private int? RegisterToDb()
        {
            if(this.MoveRegistrationWindowVM.SelectedFromBookVM == this.MoveRegistrationWindowVM.SelectedToBookVM) {
                MessageBox.Show(this, MessageText.IllegalSameBook, MessageTitle.Exclamation, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return null;
            }
            if(!this.MoveRegistrationWindowVM.Value.HasValue && this.MoveRegistrationWindowVM.Value <= 0) {
                MessageBox.Show(this, MessageText.IllegalValue, MessageTitle.Exclamation, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return null;
            }

            DateTime actTime = this.MoveRegistrationWindowVM.SelectedDate;
            int fromBookId = this.MoveRegistrationWindowVM.SelectedFromBookVM.Id.Value;
            int toBookId = this.MoveRegistrationWindowVM.SelectedToBookVM.Id.Value;
            int actValue = this.MoveRegistrationWindowVM.Value.Value;
            CommissionKind commissionKind = this.MoveRegistrationWindowVM.SelectedCommissionKind;
            int commissionItemId = this.MoveRegistrationWindowVM.SelectedItemVM.Id;
            int commission = this.MoveRegistrationWindowVM.Commission ?? 0;
            string remark = this.MoveRegistrationWindowVM.SelectedRemark;

            int? resActionId = null;

            int tmpGroupId = -1; // ローカル用
            using (DaoBase dao = builder.Build()) {
                dao.ExecTransaction(() => {
                    if (groupId == null) { // 追加
                        #region 帳簿項目を追加する
                        // グループIDを取得する
                        DaoReader reader = dao.ExecQuery(@"
INSERT INTO hst_group (group_kind, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, 0, 'now', @{1}, 'now', @{2}) RETURNING group_id;", (int)GroupKind.Move, Updater, Inserter);
                        reader.ExecARow((record) => {
                            tmpGroupId = record.ToInt("group_id");
                        });

                        reader = dao.ExecQuery(@"
-- 移動元
INSERT INTO hst_action (book_id, item_id, act_time, act_value, group_id, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, (
  SELECT item_id FROM mst_item I 
  INNER JOIN (SELECT * FROM mst_category WHERE balance_kind = @{6}) C ON C.category_id = I.category_id
  WHERE move_flg = 1
), @{1}, @{2}, @{3}, 0, 'now', @{4}, 'now', @{5}) RETURNING action_id;",
                            fromBookId, actTime, - actValue, tmpGroupId, Updater, Inserter, (int)BalanceKind.Outgo);
                        if(selectedBookId == fromBookId) {
                            reader.ExecARow((record) => {
                                resActionId = record.ToInt("action_id");
                            });
                        }

                        reader = dao.ExecQuery(@"
-- 移動先
INSERT INTO hst_action (book_id, item_id, act_time, act_value, group_id, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, (
  SELECT item_id FROM mst_item I
  INNER JOIN (SELECT * FROM mst_category WHERE balance_kind = @{6}) C ON C.category_id = I.category_id
  WHERE move_flg = 1
), @{1}, @{2}, @{3}, 0, 'now', @{4}, 'now', @{5}) RETURNING action_id;",
                            toBookId, actTime, actValue, tmpGroupId, Updater, Inserter, (int)BalanceKind.Income);
                        if (selectedBookId == toBookId) {
                            reader.ExecARow((record) => {
                                resActionId = record.ToInt("action_id");
                            });
                        }
                        #endregion
                    }
                    else { // 編集
                        #region 帳簿項目を編集する
                        tmpGroupId = groupId.Value;
                        dao.ExecNonQuery(@"
-- 移動元
UPDATE hst_action
SET book_id = @{0}, act_time = @{1}, act_value = @{2}, update_time = 'now', updater = @{3}
WHERE action_id = @{4};", fromBookId, actTime, - actValue, Updater, fromActionId);
                        if(selectedBookId == fromBookId) {
                            resActionId = fromActionId;
                        }

                        dao.ExecNonQuery(@"
-- 移動先
UPDATE hst_action
SET book_id = @{0}, act_time = @{1}, act_value = @{2}, update_time = 'now', updater = @{3}
WHERE action_id = @{4};", toBookId, actTime, actValue, Updater, toActionId);
                        if(selectedBookId == toBookId) {
                            resActionId = toActionId;
                        }
                        #endregion
                    }

                    if (commission != 0) {
                        #region 手数料あり
                        int bookId = -1;
                        switch (commissionKind) {
                            case CommissionKind.FromBook:
                                bookId = fromBookId;
                                break;
                            case CommissionKind.ToBook:
                                bookId = toBookId;
                                break;
                        }
                        if (commissionActionId != null) {
                            dao.ExecNonQuery(@"
UPDATE hst_action
SET book_id = @{0}, item_id = @{1}, act_time = @{2}, act_value = @{3}, remark = @{4}, update_time = 'now', updater = @{5}
WHERE action_id = @{6};", bookId, commissionItemId, actTime, - commission, remark, Updater, commissionActionId);
                        }
                        else {
                            dao.ExecNonQuery(@"
INSERT INTO hst_action (book_id, item_id, act_time, act_value, group_id, remark, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, @{2}, @{3}, @{4}, @{5}, 0, 'now', @{6}, 'now', @{7});", bookId, commissionItemId, actTime, - commission, tmpGroupId, remark, Updater, Inserter);
                        }
                        #endregion
                    }
                    else {
                        #region 手数料なし
                        if (commissionActionId != null) {
                            dao.ExecNonQuery(@"
UPDATE hst_action
SET del_flg = 1, update_time = 'now', updater = @{0}
WHERE action_id = @{1};", Updater, commissionActionId);
                        }
                        #endregion
                    }
                });

                if (remark != string.Empty) {
                    #region 備考を追加する
                    DaoReader reader = dao.ExecQuery(@"
SELECT remark FROM hst_remark
WHERE item_id = @{0} AND remark = @{1};", commissionItemId, remark);

                    if (reader.Count == 0) {
                        dao.ExecNonQuery(@"
INSERT INTO hst_remark (item_id, remark, remark_kind, used_time, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, 0, @{2}, 0, 'now', @{3}, 'now', @{4});", commissionItemId, remark, actTime, Updater, Inserter);
                    }
                    else {
                        dao.ExecNonQuery(@"
UPDATE hst_remark
SET used_time = @{0}, del_flg = 0, update_time = 'now', updater = @{1}
WHERE item_id = @{2} AND remark = @{3} AND used_time < @{0};", actTime, Updater, commissionItemId, remark);
                    }
                    #endregion
                }

                return resActionId;
            }
        }

        #region 設定反映用の関数
        /// <summary>
        /// 設定を読み込む
        /// </summary>
        private void LoadSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (settings.MoveRegistrationWindow_Left != -1) {
                Left = settings.MoveRegistrationWindow_Left;
            }
            if (settings.MoveRegistrationWindow_Top != -1) {
                Top = settings.MoveRegistrationWindow_Top;
            }
            if (settings.MoveRegistrationWindow_Width != -1) {
                Width = settings.MoveRegistrationWindow_Width;
            }
            if (settings.MoveRegistrationWindow_Height != -1) {
                Height = settings.MoveRegistrationWindow_Height;
            }
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        private void SaveSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (this.WindowState == WindowState.Normal) {
                settings.MoveRegistrationWindow_Left = Left;
                settings.MoveRegistrationWindow_Top = Top;
                settings.MoveRegistrationWindow_Width = Width;
                settings.MoveRegistrationWindow_Height = Height;
                settings.Save();
            }
        }
        #endregion
    }
}

﻿using HouseholdAccountBook.Dao;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.UserEventArgs;
using HouseholdAccountBook.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static HouseholdAccountBook.ConstValue.ConstValue;

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
        private readonly DaoBuilder builder;
        /// <summary>
        /// 選択されていた帳簿ID
        /// </summary>
        private readonly int? selectedBookId = null;
        /// <summary>
        /// 選択された日付
        /// </summary>
        private readonly DateTime? selectedDateTime = null;
        /// <summary>
        /// 選択された帳簿項目のグループID
        /// </summary>
        private readonly int? groupId = null;
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
        #endregion

        #region イベント
        /// <summary>
        /// 登録時のイベント
        /// </summary>
        public event EventHandler<EventArgs<List<int>>> Registrated = null;
        #endregion

        /// <summary>
        /// 帳簿項目(移動)の新規登録のために <see cref="MoveRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="builder">DAOビルダ</param>
        /// <param name="selectedBookId">帳簿ID</param>
        /// <param name="selectedDateTime">選択された日時</param>
        public MoveRegistrationWindow(DaoBuilder builder, int? selectedBookId, DateTime? selectedDateTime)
        {
            this.builder = builder;
            this.selectedBookId = selectedBookId;
            this.selectedDateTime = selectedDateTime;
            this.groupId = null;

            this.InitializeComponent();
            this.LoadWindowSetting();

            this.WVM.RegMode = RegistrationMode.Add;
        }

        /// <summary>
        /// 帳簿項目(移動)の編集(複製)のために <see cref="MoveRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="builder">DAOビルダ</param>
        /// <param name="selectedBookId">選択された帳簿ID</param>
        /// <param name="groupId">グループID</param>
        /// <param name="mode">登録モード</param>
        public MoveRegistrationWindow(DaoBuilder builder, int? selectedBookId, int groupId, RegistrationMode mode = RegistrationMode.Edit)
        {
            this.builder = builder;
            this.selectedBookId = selectedBookId;
            this.selectedDateTime = null;
            switch (mode) {
                case RegistrationMode.Edit:
                    this.groupId = groupId;
                    break;
                case RegistrationMode.Copy:
                    this.groupId = null;
                    break;
            }

            this.InitializeComponent();
            this.LoadWindowSetting();

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
            if ((string)e.Parameter == "1") {
                e.CanExecute = this.WVM.MovedDate != DateTime.Today;
            }
            else {
                e.CanExecute = this.WVM.MovingDate != DateTime.Today;
            }
        }

        /// <summary>
        /// 今日コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TodayCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.WVM.MovedDate = DateTime.Today;
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
            if (this.WVM.MovedBookVM == this.WVM.MovingBookVM) {
                MessageBox.Show(this, MessageText.IllegalSameBook, MessageTitle.Exclamation, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            if (!this.WVM.Value.HasValue && this.WVM.Value <= 0) {
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
        private async void MoveRegistrationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ObservableCollection<BookViewModel> bookVMList = new ObservableCollection<BookViewModel>();
            int? movedBookId = this.selectedBookId;
            int? movingBookId = this.selectedBookId;
            BookViewModel movedBookVM = null;
            BookViewModel movingBookVM = null;
            int? commissionItemId = null;
            string commissionRemark = null;

            switch (this.WVM.RegMode) {
                case RegistrationMode.Edit:
                case RegistrationMode.Copy: {
                        DateTime movedDate = DateTime.Now;
                        DateTime movingDate = DateTime.Now;
                        CommissionKind commissionKind = CommissionKind.FromBook;
                        int moveValue = -1;
                        int commissionValue = 0;

                        using (DaoBase dao = this.builder.Build()) {
                            DaoReader reader = await dao.ExecQueryAsync(@"
SELECT A.book_id, A.action_id, A.item_id, A.act_time, A.act_value, A.remark, I.move_flg
FROM hst_action A
INNER JOIN (SELECT * FROM mst_item WHERE del_flg = 0) I ON I.item_id = A.item_id
WHERE A.del_flg = 0 AND A.group_id = @{0}
ORDER BY move_flg DESC;", this.groupId);

                            reader.ExecWholeRow((count, record) => {
                                int bookId = record.ToInt("book_id");
                                DateTime dateTime = record.ToDateTime("act_time");
                                int actionId = record.ToInt("action_id");
                                int itemId = record.ToInt("item_id");
                                int actValue = record.ToInt("act_value");
                                int moveFlg = record.ToInt("move_flg");
                                string remark = record["remark"];

                                if (moveFlg == 1) {
                                    if (actValue < 0) {
                                        movedBookId = bookId;
                                        movedDate = dateTime;
                                        this.fromActionId = actionId;
                                    }
                                    else {
                                        movingBookId = bookId;
                                        movingDate = dateTime;
                                        this.toActionId = actionId;
                                        moveValue = actValue;
                                    }
                                }
                                else { // 手数料
                                    if (bookId == movedBookId) { // 移動元負担
                                        commissionKind = CommissionKind.FromBook;
                                    }
                                    else if (bookId == movingBookId) { // 移動先負担
                                        commissionKind = CommissionKind.ToBook;
                                    }
                                    this.commissionActionId = actionId;
                                    commissionItemId = itemId;
                                    commissionValue = Math.Abs(actValue);
                                    commissionRemark = remark;
                                }
                                return true;
                            });
                        }

                        this.WVM.IsLink = (movedDate == movingDate);
                        this.WVM.MovedDate = movedDate;
                        this.WVM.MovingDate = movingDate;
                        this.WVM.SelectedCommissionKind = commissionKind;
                        this.WVM.Value = moveValue;
                        this.WVM.Commission = commissionValue;
                    }
                    break;
            }

            int? debitBookId = null;
            int? payDay = null;
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader = await dao.ExecQueryAsync(@"
SELECT book_id, book_name, book_kind, debit_book_id, pay_day FROM mst_book WHERE del_flg = 0 ORDER BY sort_order;");
                reader.ExecWholeRow((count, record) => {
                    BookViewModel vm = new BookViewModel() { Id = record.ToInt("book_id"), Name = record["book_name"] };
                    bookVMList.Add(vm);

                    if (movedBookVM == null || movedBookId == vm.Id) {
                        movedBookVM = vm;

                        switch (this.WVM.RegMode) {
                            case RegistrationMode.Add: {
                                    if (record.ToInt("book_kind") == (int)BookKind.CreditCard) {
                                        debitBookId = record.ToNullableInt("debit_book_id");
                                        payDay = record.ToNullableInt("pay_day");
                                    }
                                }
                                break;
                        };
                    }
                    if (movingBookVM == null || movingBookId == vm.Id) {
                        movingBookVM = vm;
                    }
                    return true;
                });
            }

            this.WVM.BookVMList = bookVMList;
            this.WVM.MovingBookVM = movingBookVM;
            this.WVM.MovedBookVM = movedBookVM;

            switch (this.WVM.RegMode) {
                case RegistrationMode.Add: {
                        if (debitBookId != null) {
                            this.WVM.MovedBookVM = bookVMList.FirstOrDefault((vm) => { return vm.Id == debitBookId; });
                        }
                        this.WVM.MovedDate = this.selectedDateTime == null ? DateTime.Today : (
                            payDay != null ? this.selectedDateTime.Value.GetDateInMonth(payDay.Value) : this.selectedDateTime.Value.GetFirstDateOfMonth());
                        this.WVM.IsLink = true;
                        this.WVM.SelectedCommissionKind = CommissionKind.FromBook;
                    }
                    break;
            };

            await this.UpdateItemListAsync(commissionItemId);
            await this.UpdateRemarkListAsync(commissionRemark);

            #region イベントハンドラの設定
            this.WVM.FromBookChanged += async () => {
                await this.UpdateItemListAsync();
                await this.UpdateRemarkListAsync();
            };
            this.WVM.ToBookChanged += async () => {
                await this.UpdateItemListAsync();
                await this.UpdateRemarkListAsync();
            };
            this.WVM.CommissionKindChanged += async () => {
                await this.UpdateItemListAsync();
                await this.UpdateRemarkListAsync();
            };
            this.WVM.ItemChanged += async () => {
                await this.UpdateRemarkListAsync();
            };
            #endregion
        }

        /// <summary>
        /// フォーム終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveRegistrationWindow_Closed(object sender, EventArgs e)
        {
            this.SaveWindowSetting();
        }
        #endregion

        #region 画面更新用の関数
        /// <summary>
        /// 手数料項目リストを更新する
        /// </summary>
        /// <param name="itemId">選択対象の項目</param>
        private async Task UpdateItemListAsync(int? itemId = null)
        {
            ObservableCollection<ItemViewModel> itemVMList = new ObservableCollection<ItemViewModel>();
            ItemViewModel selectedItemVM = null;
            using (DaoBase dao = this.builder.Build()) {
                int bookId = -1;
                switch (this.WVM.SelectedCommissionKind) {
                    case CommissionKind.FromBook:
                        bookId = this.WVM.MovedBookVM.Id.Value;
                        break;
                    case CommissionKind.ToBook:
                        bookId = this.WVM.MovingBookVM.Id.Value;
                        break;
                }
                DaoReader reader = await dao.ExecQueryAsync(@"
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
            this.WVM.ItemVMList = itemVMList;
            this.WVM.SelectedItemVM = selectedItemVM;
        }

        /// <summary>
        /// 備考リストを更新する
        /// </summary>
        /// <param name="remark">選択対象の備考</param>
        private async Task UpdateRemarkListAsync(string remark = null)
        {
            if (this.WVM?.SelectedItemVM?.Id == null) return;

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
            DateTime movedTime = this.WVM.MovedDate;
            DateTime movingTime = this.WVM.MovingDate;
            int movedBookId = this.WVM.MovedBookVM.Id.Value;
            int movingBookId = this.WVM.MovingBookVM.Id.Value;
            int actValue = this.WVM.Value.Value;
            CommissionKind commissionKind = this.WVM.SelectedCommissionKind;
            int commissionItemId = this.WVM.SelectedItemVM.Id;
            int commission = this.WVM.Commission ?? 0;
            string remark = this.WVM.SelectedRemark;

            int? resActionId = null;

            int tmpGroupId = -1; // ローカル用
            using (DaoBase dao = this.builder.Build()) {
                await dao.ExecTransactionAsync(async () => {
                    if (this.groupId == null) { // 追加
                        #region 帳簿項目を追加する
                        // グループIDを取得する
                        DaoReader reader = await dao.ExecQueryAsync(@"
INSERT INTO hst_group (group_kind, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, 0, 'now', @{1}, 'now', @{2}) RETURNING group_id;", (int)GroupKind.Move, Updater, Inserter);
                        reader.ExecARow((record) => {
                            tmpGroupId = record.ToInt("group_id");
                        });

                        reader = await dao.ExecQueryAsync(@"
-- 移動元
INSERT INTO hst_action (book_id, item_id, act_time, act_value, group_id, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, (
  SELECT item_id FROM mst_item I 
  INNER JOIN (SELECT * FROM mst_category WHERE balance_kind = @{6}) C ON C.category_id = I.category_id
  WHERE move_flg = 1
), @{1}, @{2}, @{3}, 0, 'now', @{4}, 'now', @{5}) RETURNING action_id;",
                            movedBookId, movedTime, -actValue, tmpGroupId, Updater, Inserter, (int)BalanceKind.Outgo);
                        if (this.selectedBookId == movedBookId) {
                            reader.ExecARow((record) => {
                                resActionId = record.ToInt("action_id");
                            });
                        }

                        reader = await dao.ExecQueryAsync(@"
-- 移動先
INSERT INTO hst_action (book_id, item_id, act_time, act_value, group_id, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, (
  SELECT item_id FROM mst_item I
  INNER JOIN (SELECT * FROM mst_category WHERE balance_kind = @{6}) C ON C.category_id = I.category_id
  WHERE move_flg = 1
), @{1}, @{2}, @{3}, 0, 'now', @{4}, 'now', @{5}) RETURNING action_id;",
                            movingBookId, movingTime, actValue, tmpGroupId, Updater, Inserter, (int)BalanceKind.Income);
                        if (this.selectedBookId == movingBookId) {
                            reader.ExecARow((record) => {
                                resActionId = record.ToInt("action_id");
                            });
                        }
                        #endregion
                    }
                    else { // 編集
                        #region 帳簿項目を編集する
                        tmpGroupId = this.groupId.Value;
                        await dao.ExecNonQueryAsync(@"
-- 移動元
UPDATE hst_action
SET book_id = @{0}, act_time = @{1}, act_value = @{2}, update_time = 'now', updater = @{3}
WHERE action_id = @{4};", movedBookId, movedTime, -actValue, Updater, this.fromActionId);
                        if (this.selectedBookId == movedBookId) {
                            resActionId = this.fromActionId;
                        }

                        await dao.ExecNonQueryAsync(@"
-- 移動先
UPDATE hst_action
SET book_id = @{0}, act_time = @{1}, act_value = @{2}, update_time = 'now', updater = @{3}
WHERE action_id = @{4};", movingBookId, movingTime, actValue, Updater, this.toActionId);
                        if (this.selectedBookId == movingBookId) {
                            resActionId = this.toActionId;
                        }
                        #endregion
                    }

                    if (commission != 0) {
                        #region 手数料あり
                        int bookId = -1;
                        DateTime actTime = DateTime.Now;
                        switch (commissionKind) {
                            case CommissionKind.FromBook:
                                bookId = movedBookId;
                                actTime = movedTime;
                                break;
                            case CommissionKind.ToBook:
                                bookId = movingBookId;
                                actTime = movingTime;
                                break;
                        }
                        if (this.commissionActionId != null) {
                            await dao.ExecNonQueryAsync(@"
UPDATE hst_action
SET book_id = @{0}, item_id = @{1}, act_time = @{2}, act_value = @{3}, remark = @{4}, update_time = 'now', updater = @{5}
WHERE action_id = @{6};", bookId, commissionItemId, actTime, -commission, remark, Updater, this.commissionActionId);
                        }
                        else {
                            await dao.ExecNonQueryAsync(@"
INSERT INTO hst_action (book_id, item_id, act_time, act_value, group_id, remark, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, @{2}, @{3}, @{4}, @{5}, 0, 'now', @{6}, 'now', @{7});", bookId, commissionItemId, actTime, -commission, tmpGroupId, remark, Updater, Inserter);
                        }
                        #endregion
                    }
                    else {
                        #region 手数料なし
                        if (this.commissionActionId != null) {
                            await dao.ExecNonQueryAsync(@"
UPDATE hst_action
SET del_flg = 1, update_time = 'now', updater = @{0}
WHERE action_id = @{1};", Updater, this.commissionActionId);
                        }
                        #endregion
                    }
                });

                if (remark != string.Empty) {
                    #region 備考を追加する
                    DaoReader reader = await dao.ExecQueryAsync(@"
SELECT remark FROM hst_remark
WHERE item_id = @{0} AND remark = @{1};", commissionItemId, remark);

                    if (reader.Count == 0) {
                        await dao.ExecNonQueryAsync(@"
INSERT INTO hst_remark (item_id, remark, remark_kind, used_time, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, 0, @{2}, 0, 'now', @{3}, 'now', @{4});", commissionItemId, remark, movedTime > movingTime ? movedTime : movingTime, Updater, Inserter);
                    }
                    else {
                        await dao.ExecNonQueryAsync(@"
UPDATE hst_remark
SET used_time = @{0}, del_flg = 0, update_time = 'now', updater = @{1}
WHERE item_id = @{2} AND remark = @{3} AND used_time < @{0};", movedTime > movingTime ? movedTime : movingTime, Updater, commissionItemId, remark);
                    }
                    #endregion
                }

                return resActionId;
            }
        }

        #region 設定反映用の関数
        /// <summary>
        /// ウィンドウ設定を読み込む
        /// </summary>
        private void LoadWindowSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (0 <= settings.MoveRegistrationWindow_Left) {
                this.Left = settings.MoveRegistrationWindow_Left;
            }
            if (0 <= settings.MoveRegistrationWindow_Top) {
                this.Top = settings.MoveRegistrationWindow_Top;
            }
            if (settings.MoveRegistrationWindow_Width != -1) {
                this.Width = settings.MoveRegistrationWindow_Width;
            }
            if (settings.MoveRegistrationWindow_Height != -1) {
                this.Height = settings.MoveRegistrationWindow_Height;
            }
        }

        /// <summary>
        /// ウィンドウ設定を保存する
        /// </summary>
        private void SaveWindowSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (this.WindowState == WindowState.Normal) {
                settings.MoveRegistrationWindow_Left = this.Left;
                settings.MoveRegistrationWindow_Top = this.Top;
                settings.MoveRegistrationWindow_Width = this.Width;
                settings.MoveRegistrationWindow_Height = this.Height;
                settings.Save();
            }
        }
        #endregion
    }
}

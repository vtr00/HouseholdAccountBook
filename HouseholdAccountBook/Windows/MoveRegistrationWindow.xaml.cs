using HouseholdAccountBook.DbHandler;
using HouseholdAccountBook.DbHandler.Abstract;
using HouseholdAccountBook.Dto.DbTable;
using HouseholdAccountBook.Dto.Others;
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
using static HouseholdAccountBook.Extensions.FrameworkElementExtensions;

namespace HouseholdAccountBook.Windows
{
    /// <summary>
    /// MoveRegistrationWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MoveRegistrationWindow : Window
    {
        #region フィールド
        /// <summary>
        /// DBハンドラファクトリ
        /// </summary>
        private readonly DbHandlerFactory dbHandlerFactory;
        /// <summary>
        /// <see cref="MainWindow"/>で選択された帳簿ID
        /// </summary>
        private readonly int? selectedBookId = null;
        /// <summary>
        /// <see cref="MainWindow"/>で選択された表示年月
        /// </summary>
        private readonly DateTime? selectedMonth = null;
        /// <summary>
        /// <see cref="MainWindow"/>で選択された帳簿項目の日付
        /// </summary>
        private readonly DateTime? selectedDate = null;
        /// <summary>
        /// <see cref="MainWindow"/>で選択された帳簿項目のグループID
        /// </summary>
        private readonly int? selectedGroupId = null;
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
        /// <summary>
        /// 移動元変更時のイベント
        /// </summary>
        public event EventHandler<EventArgs<int?>> FromBookChanged = null;
        /// <summary>
        /// 移動元日時変更時のイベント
        /// </summary>
        public event EventHandler<EventArgs<DateTime>> FromDateChanged = null;
        /// <summary>
        /// 移動先変更時のイベント
        /// </summary>
        public event EventHandler<EventArgs<int?>> ToBookChanged = null;
        /// <summary>
        /// 移動先日時変更時のイベント
        /// </summary>
        public event EventHandler<EventArgs<DateTime>> ToDateChanged = null;
        #endregion

        #region コンストラクタ
        /// <summary>
        /// 帳簿項目(移動)の新規登録のために <see cref="MoveRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="selectedBookId">帳簿ID</param>
        /// <param name="selectedMonth">選択された年月</param>
        /// <param name="selectedDate">選択された日付</param>
        public MoveRegistrationWindow(DbHandlerFactory dbHandlerFactory, int? selectedBookId, DateTime? selectedMonth, DateTime? selectedDate)
        {
            this.dbHandlerFactory = dbHandlerFactory;
            this.selectedBookId = selectedBookId;
            this.selectedMonth = selectedMonth;
            this.selectedDate = selectedDate;
            this.selectedGroupId = null;

            this.InitializeComponent();

            this.WVM.RegMode = RegistrationKind.Add;
        }

        /// <summary>
        /// 帳簿項目(移動)の編集(複製)のために <see cref="MoveRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="selectedBookId">選択された帳簿ID</param>
        /// <param name="selectedGroupId">グループID</param>
        /// <param name="mode">登録モード</param>
        public MoveRegistrationWindow(DbHandlerFactory dbHandlerFactory, int? selectedBookId, int selectedGroupId, RegistrationKind mode = RegistrationKind.Edit)
        {
            this.dbHandlerFactory = dbHandlerFactory;
            this.selectedBookId = selectedBookId;
            this.selectedMonth = null;
            this.selectedDate = null;
            this.selectedGroupId = selectedGroupId;

            this.InitializeComponent();

            this.WVM.RegMode = mode;
        }
        #endregion

        #region イベントハンドラ
        #region コマンド
        /// <summary>
        /// 今日コマンド操作可能判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TodayCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((string)e.Parameter == "1") {
                e.CanExecute = this.WVM.FromDate != DateTime.Today;
            }
            else {
                e.CanExecute = this.WVM.ToDate != DateTime.Today;
            }
        }

        /// <summary>
        /// 今日コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TodayCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.WVM.FromDate = DateTime.Today;
        }

        /// <summary>
        /// 登録コマンド操作可能判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegisterCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.Value.HasValue && this.WVM.SelectedFromBookVM != this.WVM.SelectedToBookVM;
        }

        /// <summary>
        /// 登録コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RegisterCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // DB登録
            List<int> idList = null;
            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                idList = await this.RegisterToDbAsync();
            }

            // MainWindow更新
            Registrated?.Invoke(this, new EventArgs<List<int>>(idList ?? new List<int>()));

            try {
                this.DialogResult = true;
            }
            catch (InvalidOperationException) { }
            this.Close();
        }

        /// <summary>
        /// キャンセルコマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try {
                this.DialogResult = false;
            }
            catch (InvalidOperationException) { }
            this.Close();
        }
        #endregion

        #region ウィンドウ
        /// <summary>
        /// ウィンドウ読込完了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MoveRegistrationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            int? fromBookId = this.selectedBookId;
            int? toBookId = this.selectedBookId;
            int? commissionItemId = null;
            string commissionRemark = null;

            // DBから値を読み込む
            switch (this.WVM.RegMode) {
                case RegistrationKind.Add:
                    // 追加時の日時、金額は帳簿リスト更新時に取得する
                    break;
                case RegistrationKind.Edit:
                case RegistrationKind.Copy: {
                    DateTime fromDate = DateTime.Now;
                    DateTime toDate = DateTime.Now;
                    CommissionKind commissionKind = CommissionKind.MoveFrom;
                    int moveValue = -1;
                    int commissionValue = 0;

                    using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                        var dtoList = await dbHandler.QueryAsync<MoveActionInfoDto>(@"
SELECT A.book_id, A.action_id, A.item_id, A.act_time, A.act_value, A.remark, I.move_flg
FROM hst_action A
INNER JOIN (SELECT * FROM mst_item WHERE del_flg = 0) I ON I.item_id = A.item_id
WHERE A.del_flg = 0 AND A.group_id = @GroupId
ORDER BY I.move_flg DESC;",
new { GroupId = this.selectedGroupId });

                        foreach (MoveActionInfoDto dto in dtoList) {
                            if (dto.MoveFlg == 1) {
                                if (dto.ActValue < 0) {
                                    fromBookId = dto.BookId;
                                    fromDate = dto.ActTime;
                                    this.fromActionId = dto.ActionId;
                                }
                                else {
                                    toBookId = dto.BookId;
                                    toDate = dto.ActTime;
                                    this.toActionId = dto.ActionId;
                                    moveValue = dto.ActValue;
                                }
                            }
                            else { // 手数料
                                if (dto.BookId == fromBookId) { // 移動元負担
                                    commissionKind = CommissionKind.MoveFrom;
                                }
                                else if (dto.BookId == toBookId) { // 移動先負担
                                    commissionKind = CommissionKind.MoveTo;
                                }
                                this.commissionActionId = dto.ActionId;
                                commissionItemId = dto.ItemId;
                                commissionValue = Math.Abs(dto.ActValue);
                                commissionRemark = dto.Remark;
                            }
                        }
                    }

                    // WVMに値を設定する
                    if (this.WVM.RegMode == RegistrationKind.Edit) {
                        this.WVM.FromId = this.fromActionId;
                        this.WVM.ToId = this.toActionId;
                        this.WVM.GroupId = this.selectedGroupId;
                        this.WVM.CommissionId = this.commissionActionId;
                    }
                    this.WVM.IsLink = (fromDate == toDate);
                    this.WVM.FromDate = fromDate;
                    this.WVM.ToDate = toDate;
                    this.WVM.SelectedCommissionKind = commissionKind;
                    this.WVM.Value = moveValue;
                    this.WVM.Commission = commissionValue;
                }
                break;
            }

            // リストを更新する
            await this.UpdateBookListAsync(fromBookId, toBookId);
            await this.UpdateItemListAsync(commissionItemId);
            await this.UpdateRemarkListAsync(commissionRemark);

            // イベントハンドラを登録する
            this.RegisterEventHandlerToWVM();
        }

        /// <summary>
        /// ウィンドウ終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveRegistrationWindow_Closed(object sender, EventArgs e)
        {
            this.SaveWindowSetting();
        }
        #endregion
        #endregion

        #region 画面更新用の関数
        /// <summary>
        /// 帳簿リストを更新する
        /// </summary>
        /// <param name="fromBookId">移動元帳簿ID</param>
        /// <param name="toBookId">移動先帳簿ID</param>
        /// <returns></returns>
        private async Task UpdateBookListAsync(int? fromBookId = null, int? toBookId = null)
        {
            ObservableCollection<BookViewModel> bookVMList = new ObservableCollection<BookViewModel>();
            BookViewModel fromBookVM = null;
            BookViewModel toBookVM = null;

            int? debitBookId = null;
            int? payDay = null;
            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                var dtoList = await dbHandler.QueryAsync<MstBookDto>(@"
SELECT book_id, book_name, book_kind, debit_book_id, pay_day
FROM mst_book
WHERE del_flg = 0
ORDER BY sort_order;");
                foreach (MstBookDto dto in dtoList) {
                    BookViewModel vm = new BookViewModel() { Id = dto.BookId, Name = dto.BookName };
                    bookVMList.Add(vm);
                    if (fromBookVM == null || fromBookId == vm.Id) {
                        fromBookVM = vm;
                        switch (this.WVM.RegMode) {
                            case RegistrationKind.Add: {
                                if (dto.BookKind == (int)BookKind.CreditCard) {
                                    debitBookId = dto.DebitBookId;
                                    payDay = dto.PayDay;
                                }
                            }
                            break;
                        };
                    }
                    if (toBookVM == null || toBookId == vm.Id) {
                        toBookVM = vm;
                    }
                }
            }

            this.WVM.BookVMList = bookVMList;
            this.WVM.SelectedFromBookVM = fromBookVM;
            this.WVM.SelectedToBookVM = toBookVM;

            switch (this.WVM.RegMode) {
                case RegistrationKind.Add: {
                    if (debitBookId != null) {
                        this.WVM.SelectedFromBookVM = bookVMList.FirstOrDefault((vm) => { return vm.Id == debitBookId; });
                    }
                    this.WVM.FromDate = this.selectedDate ?? ((this.selectedMonth == null || this.selectedMonth?.Month == DateTime.Today.Month) ? DateTime.Today : this.selectedMonth.Value);
                    if (payDay != null) {
                        this.WVM.FromDate = this.WVM.FromDate.GetDateInMonth(payDay.Value);
                    }
                    this.WVM.IsLink = true;
                    this.WVM.SelectedCommissionKind = CommissionKind.MoveFrom;
                }
                break;
            };

        }

        /// <summary>
        /// 手数料項目リストを更新する
        /// </summary>
        /// <param name="itemId">選択対象の項目</param>
        /// <returns></returns>
        private async Task UpdateItemListAsync(int? itemId = null)
        {
            ObservableCollection<ItemViewModel> itemVMList = new ObservableCollection<ItemViewModel>();
            ItemViewModel selectedItemVM = null;

            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                int bookId = -1;
                switch (this.WVM.SelectedCommissionKind) {
                    case CommissionKind.MoveFrom:
                        bookId = this.WVM.SelectedFromBookVM.Id.Value;
                        break;
                    case CommissionKind.MoveTo:
                        bookId = this.WVM.SelectedToBookVM.Id.Value;
                        break;
                }
                var dtoList = await dbHandler.QueryAsync<CategoryItemInfoDto>(@"
SELECT I.item_id, I.item_name, C.category_name
FROM mst_item I
INNER JOIN (SELECT * FROM mst_category WHERE del_flg = 0) C ON C.category_id = I.category_id
WHERE I.del_flg = 0 AND C.balance_kind = @BalanceKind AND
      EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @BookId AND RBI.item_id = I.item_id AND del_flg = 0)
ORDER BY C.sort_order, I.sort_order;",
new { BalanceKind = (int)BalanceKind.Expenses, BookId = bookId });

                foreach (CategoryItemInfoDto dto in dtoList) {
                    ItemViewModel vm = new ItemViewModel() {
                        Id = dto.ItemId,
                        Name = dto.ItemName,
                        CategoryName = dto.CategoryName
                    };
                    itemVMList.Add(vm);
                    if (selectedItemVM == null || vm.Id == itemId) {
                        selectedItemVM = vm;
                    }
                }
            }
            this.WVM.ItemVMList = itemVMList;
            this.WVM.SelectedItemVM = selectedItemVM;
        }

        /// <summary>
        /// 備考リストを更新する
        /// </summary>
        /// <param name="remark">選択対象の備考</param>
        /// <returns></returns>
        private async Task UpdateRemarkListAsync(string remark = null)
        {
            if (this.WVM?.SelectedItemVM?.Id == null) return;

            ObservableCollection<RemarkViewModel> remarkVMList = new ObservableCollection<RemarkViewModel>() {
                new RemarkViewModel(){ Remark = string.Empty }
            };
            string selectedRemark = remark ?? this.WVM.SelectedRemark ?? remarkVMList[0].Remark;
            RemarkViewModel selectedRemarkVM = remarkVMList[0]; // UNUSED
            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                var dtoList = await dbHandler.QueryAsync<RemarkInfoDto>(@"
SELECT R.remark, COUNT(A.remark) AS count, COALESCE(MAX(A.act_time), '1970-01-01') AS sort_time, COALESCE(MAX(A.act_time), null) AS used_time
FROM hst_remark R
LEFT OUTER JOIN (SELECT * FROM hst_action WHERE del_flg = 0) A ON A.remark = R.remark AND A.item_id = R.item_id
WHERE R.del_flg = 0 AND R.item_id = @ItemId
GROUP BY R.remark
ORDER BY sort_time DESC, remark_count DESC;",
new { ItemId = this.WVM.SelectedItemVM.Id });
                foreach (RemarkInfoDto dto in dtoList) {
                    RemarkViewModel rvm = new RemarkViewModel() {
                        Remark = dto.Remark,
                        UsedCount = dto.Count,
                        UsedTime = dto.UsedTime
                    };
                    remarkVMList.Add(rvm);
                    if (selectedRemarkVM == null || rvm.Remark == selectedRemark) {
                        selectedRemarkVM = rvm;
                    }
                }
            }

            this.WVM.RemarkVMList = remarkVMList;
            this.WVM.SelectedRemark = selectedRemark;
        }
        #endregion

        /// <summary>
        /// WVMのイベントハンドラに登録する
        /// </summary>
        private void RegisterEventHandlerToWVM()
        {
            this.WVM.FromBookChanged += async (fromBookId) => {
                using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                    await this.UpdateItemListAsync();
                    await this.UpdateRemarkListAsync();
                }
                this.FromBookChanged?.Invoke(this, new EventArgs<int?>(fromBookId));
            };
            this.WVM.FromDateChanged += (fromDate) => {
                this.FromDateChanged?.Invoke(this, new EventArgs<DateTime>(fromDate));
            };
            this.WVM.ToBookChanged += async (toBookId) => {
                using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                    await this.UpdateItemListAsync();
                    await this.UpdateRemarkListAsync();
                }
                this.ToBookChanged?.Invoke(this, new EventArgs<int?>(toBookId));
            };
            this.WVM.ToDateChanged += (toDate) => {
                this.ToDateChanged?.Invoke(this, new EventArgs<DateTime>(toDate));
            };
            this.WVM.CommissionKindChanged += async (_) => {
                using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                    await this.UpdateItemListAsync();
                    await this.UpdateRemarkListAsync();
                }
            };
            this.WVM.ItemChanged += async (_) => {
                using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                    await this.UpdateRemarkListAsync();
                }
            };
        }

        /// <summary>
        /// DBに登録する
        /// </summary>
        /// <returns>登録された帳簿項目ID</returns>
        private async Task<List<int>> RegisterToDbAsync()
        {
            List<int> resActionIdList = new List<int>();

            DateTime fromDate = this.WVM.FromDate;
            DateTime toDate = this.WVM.ToDate;
            int fromBookId = this.WVM.SelectedFromBookVM.Id.Value;
            int toBookId = this.WVM.SelectedToBookVM.Id.Value;
            int actValue = this.WVM.Value.Value;
            CommissionKind commissionKind = this.WVM.SelectedCommissionKind;
            int commissionItemId = this.WVM.SelectedItemVM.Id;
            int commission = this.WVM.Commission ?? 0;
            string remark = this.WVM.SelectedRemark;

            int tmpGroupId = -1; // ローカル用
            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                await dbHandler.ExecTransactionAsync(async () => {
                    switch (this.WVM.RegMode) {
                        case RegistrationKind.Add:
                        case RegistrationKind.Copy: {
                            #region 帳簿項目を追加する
                            // グループIDを取得する
                            var dto = await dbHandler.QuerySingleAsync<ReturningDto>(@"
INSERT INTO hst_group (group_kind, del_flg, update_time, updater, insert_time, inserter)
VALUES (@GroupKind, 0, 'now', @Updater, 'now', @Inserter)
RETURNING group_id AS Id;",
new HstGroupDto { GroupKind = (int)GroupKind.Move });
                            tmpGroupId = dto.Id;

                            dto = await dbHandler.QuerySingleAsync<ReturningDto>(@"
-- 移動元
INSERT INTO hst_action (book_id, item_id, act_time, act_value, group_id, del_flg, update_time, updater, insert_time, inserter)
VALUES (@BookId, (
  SELECT item_id FROM mst_item I 
  INNER JOIN (SELECT * FROM mst_category WHERE balance_kind = @BalanceKind) C ON C.category_id = I.category_id
  WHERE move_flg = 1
), @ActTime, @ActValue, @GroupId, 0, 'now', @Updater, 'now', @Inserter)
RETURNING action_id AS Id;",
new { BookId = fromBookId, ActTime = fromDate, ActValue = -actValue, GroupId = tmpGroupId, Updater, Inserter, BalanceKind = (int)BalanceKind.Expenses });
                            if (this.selectedBookId == fromBookId) {
                                resActionIdList.Add(dto.Id);
                            }

                            dto = await dbHandler.QuerySingleAsync<ReturningDto>(@"
-- 移動先
INSERT INTO hst_action (book_id, item_id, act_time, act_value, group_id, del_flg, update_time, updater, insert_time, inserter)
VALUES (@BookId, (
  SELECT item_id FROM mst_item I
  INNER JOIN (SELECT * FROM mst_category WHERE balance_kind = @BalanceKind) C ON C.category_id = I.category_id
  WHERE move_flg = 1
), @ActTime, @ActValue, @GroupId, 0, 'now', @Updater, 'now', @Inserter)
RETURNING action_id AS Id;",
new { BookId = toBookId, ActTime = toDate, ActValue = actValue, GroupId = tmpGroupId, Updater, Inserter, BalanceKind = (int)BalanceKind.Income });
                            if (this.selectedBookId == toBookId) {
                                resActionIdList.Add(dto.Id);
                            }
                            #endregion
                            break;
                        }
                        case RegistrationKind.Edit: {
                            #region 帳簿項目を変更する
                            tmpGroupId = this.selectedGroupId.Value;
                            await dbHandler.ExecuteAsync(@"
-- 移動元
UPDATE hst_action
SET book_id = @BookId, act_time = @ActTime, act_value = @ActValue, update_time = 'now', updater = @Updater
WHERE action_id = @ActionId;",
new HstActionDto { BookId = fromBookId, ActTime = fromDate, ActValue = -actValue, ActionId = this.fromActionId.Value });
                            if (this.selectedBookId == fromBookId) {
                                resActionIdList.Add(this.fromActionId.Value);
                            }

                            await dbHandler.ExecuteAsync(@"
-- 移動先
UPDATE hst_action
SET book_id = @BookId, act_time = @ActTime, act_value = @ActValue, update_time = 'now', updater = @Updater
WHERE action_id = @ActionId;",
new HstActionDto { BookId = toBookId, ActTime = toDate, ActValue = actValue, ActionId = this.toActionId.Value });
                            if (this.selectedBookId == toBookId) {
                                resActionIdList.Add(this.toActionId.Value);
                            }
                            #endregion
                            break;
                        }
                    }

                    if (commission != 0) {
                        #region 手数料あり
                        int bookId = -1;
                        DateTime actTime = DateTime.Now;
                        switch (commissionKind) {
                            case CommissionKind.MoveFrom:
                                bookId = fromBookId;
                                actTime = fromDate;
                                break;
                            case CommissionKind.MoveTo:
                                bookId = toBookId;
                                actTime = toDate;
                                break;
                        }

                        if (this.commissionActionId != null) {
                            await dbHandler.ExecuteAsync(@"
UPDATE hst_action
SET book_id = @BookId, item_id = @ItemId, act_time = @ActTime, act_value = @ActValue, remark = @Remark, update_time = 'now', updater = @Updater
WHERE action_id = @ActionId;",
new HstActionDto { BookId = bookId, ItemId = commissionItemId, ActTime = actTime, ActValue = -commission, Remark = remark, ActionId = this.commissionActionId.Value });
                            resActionIdList.Add(this.commissionActionId.Value);
                        }
                        else {
                            var dto = await dbHandler.QuerySingleAsync<ReturningDto>(@"
INSERT INTO hst_action (book_id, item_id, act_time, act_value, group_id, remark, del_flg, update_time, updater, insert_time, inserter)
VALUES (@BookId, @ItemId, @ActTime, @ActValue, @GroupId, @Remark, 0, 'now', @Updater, 'now', @Inserter)
RETURNING action_id AS Id;",
new HstActionDto { BookId = bookId, ItemId = commissionItemId, ActTime = actTime, ActValue = -commission, GroupId = tmpGroupId, Remark = remark });
                            resActionIdList.Add(dto.Id);
                        }
                        #endregion
                    }
                    else {
                        #region 手数料なし
                        if (this.commissionActionId != null) {
                            await dbHandler.ExecuteAsync(@"
UPDATE hst_action
SET del_flg = 1, update_time = 'now', updater = @Updater
WHERE action_id = @ActionId;",
new HstActionDto { ActionId = this.commissionActionId.Value });
                        }
                        #endregion
                    }
                });

                if (remark != string.Empty) {
                    #region 備考を追加する
                    var dtoList = await dbHandler.QueryAsync<HstRemarkDto>(@"
SELECT remark FROM hst_remark
WHERE item_id = @ItemId AND remark = @Remark;",
new HstRemarkDto { ItemId = commissionItemId, Remark = remark });

                    if (dtoList.Count() == 0) {
                        await dbHandler.ExecuteAsync(@"
INSERT INTO hst_remark (item_id, remark, remark_kind, used_time, del_flg, update_time, updater, insert_time, inserter)
VALUES (@ItemId, @Remark, 0, @UsedTime, 0, 'now', @Updater, 'now', @Inserter);",
new HstRemarkDto { ItemId = commissionItemId, Remark = remark, UsedTime = fromDate > toDate ? fromDate : toDate });
                    }
                    else {
                        await dbHandler.ExecuteAsync(@"
UPDATE hst_remark
SET used_time = @UsedTime, del_flg = 0, update_time = 'now', updater = @Updater
WHERE item_id = @ItemId AND remark = @Remark AND used_time < @UsedTime;",
new HstRemarkDto { UsedTime = fromDate > toDate ? fromDate : toDate, ItemId = commissionItemId, Remark = remark });
                    }
                    #endregion
                }

                return resActionIdList;
            }
        }

        #region 設定反映用の関数
        /// <summary>
        /// ウィンドウ設定を読み込む
        /// </summary>
        public void LoadWindowSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (settings.MoveRegistrationWindow_Width != -1 && settings.MoveRegistrationWindow_Height != -1) {
                this.Width = settings.MoveRegistrationWindow_Width;
                this.Height = settings.MoveRegistrationWindow_Height;
            }

            if (settings.App_IsPositionSaved && (-10 <= settings.MoveRegistrationWindow_Left && 0 <= settings.MoveRegistrationWindow_Top)) {
                this.Left = settings.MoveRegistrationWindow_Left;
                this.Top = settings.MoveRegistrationWindow_Top;
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

            if (this.WindowState == WindowState.Normal) {
                if (settings.App_IsPositionSaved) {
                    settings.MoveRegistrationWindow_Left = this.Left;
                    settings.MoveRegistrationWindow_Top = this.Top;
                }
                settings.MoveRegistrationWindow_Width = this.Width;
                settings.MoveRegistrationWindow_Height = this.Height;
                settings.Save();
            }
        }
        #endregion
    }
}

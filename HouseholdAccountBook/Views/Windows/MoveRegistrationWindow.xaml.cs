using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Models.Dao.Compositions;
using HouseholdAccountBook.Models.Dao.DbTable;
using HouseholdAccountBook.Models.DbHandler;
using HouseholdAccountBook.Models.DbHandler.Abstract;
using HouseholdAccountBook.Models.Dto.DbTable;
using HouseholdAccountBook.Models.Dto.Others;
using HouseholdAccountBook.Others;
using HouseholdAccountBook.ViewModels.Component;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static HouseholdAccountBook.Extensions.FrameworkElementExtensions;
using static HouseholdAccountBook.Models.DbConstants;
using static HouseholdAccountBook.ViewModels.LogicConstants;

namespace HouseholdAccountBook.Views.Windows
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
            e.CanExecute = (string)e.Parameter == "1" ? this.WVM.FromDate != DateTime.Today : this.WVM.ToDate != DateTime.Today;
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
            Registrated?.Invoke(this, new EventArgs<List<int>>(idList ?? []));

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
                    int? commissionValue = null;

                    await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                        MoveActionInfoDao moveActionInfoDao = new(dbHandler);
                        var dtoList = await moveActionInfoDao.GetAllAsync(this.selectedGroupId.Value);
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
                    this.WVM.IsLink = fromDate == toDate;
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
            ObservableCollection<BookViewModel> bookVMList = [];
            BookViewModel fromBookVM = null;
            BookViewModel toBookVM = null;

            int? debitBookId = null;
            int? payDay = null;
            await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                MstBookDao mstBookDao = new(dbHandler);
                var dtoList = await mstBookDao.FindAllAsync();
                foreach (var dto in dtoList) {
                    BookViewModel vm = new() { Id = dto.BookId, Name = dto.BookName };
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
                        }
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
            }
        }

        /// <summary>
        /// 手数料項目リストを更新する
        /// </summary>
        /// <param name="itemId">選択対象の項目</param>
        /// <returns></returns>
        private async Task UpdateItemListAsync(int? itemId = null)
        {
            ObservableCollection<ItemViewModel> itemVMList = [];
            ItemViewModel selectedItemVM = null;

            await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                int bookId = -1;
                switch (this.WVM.SelectedCommissionKind) {
                    case CommissionKind.MoveFrom:
                        bookId = this.WVM.SelectedFromBookVM.Id.Value;
                        break;
                    case CommissionKind.MoveTo:
                        bookId = this.WVM.SelectedToBookVM.Id.Value;
                        break;
                }

                CategoryItemInfoDao categoryItemInfoDao = new(dbHandler);
                var dtoList = await categoryItemInfoDao.FindByBookIdAndBalanceKindAsync(bookId, (int)BalanceKind.Expenses);
                foreach (CategoryItemInfoDto dto in dtoList) {
                    ItemViewModel vm = new() {
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

            ObservableCollection<RemarkViewModel> remarkVMList = [
                new RemarkViewModel(){ Remark = string.Empty }
            ];
            string selectedRemark = remark ?? this.WVM.SelectedRemark ?? remarkVMList[0].Remark;
            RemarkViewModel selectedRemarkVM = remarkVMList[0]; // UNUSED
            await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                RemarkInfoDao remarkInfoDao = new(dbHandler);
                var dtoList = await remarkInfoDao.FindByItemIdAsync(this.WVM.SelectedItemVM.Id);
                foreach (RemarkInfoDto dto in dtoList) {
                    RemarkViewModel rvm = new() {
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
            List<int> resActionIdList = [];

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
            await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                await dbHandler.ExecTransactionAsync(async () => {
                    switch (this.WVM.RegMode) {
                        case RegistrationKind.Add:
                        case RegistrationKind.Copy: {
                            #region 帳簿項目を追加する
                            // グループIDを取得する
                            HstGroupDao hstGroupDao = new(dbHandler);
                            tmpGroupId = await hstGroupDao.InsertReturningIdAsync(new HstGroupDto { GroupKind = (int)GroupKind.Move });

                            HstActionDao hstActionDao = new(dbHandler);
                            int id = await hstActionDao.InsertMoveActionReturningIdAsync(new HstActionDto {
                                BookId = fromBookId,
                                ActTime = fromDate,
                                ActValue = -actValue,
                                GroupId = tmpGroupId
                            }, (int)BalanceKind.Expenses);
                            if (this.selectedBookId == fromBookId) {
                                resActionIdList.Add(id);
                            }

                            id = await hstActionDao.InsertMoveActionReturningIdAsync(new HstActionDto {
                                BookId = toBookId,
                                ActTime = toDate,
                                ActValue = actValue,
                                GroupId = tmpGroupId
                            }, (int)BalanceKind.Income);
                            if (this.selectedBookId == toBookId) {
                                resActionIdList.Add(id);
                            }
                            #endregion
                            break;
                        }
                        case RegistrationKind.Edit: {
                            #region 帳簿項目を変更する
                            tmpGroupId = this.selectedGroupId.Value;

                            HstActionDao hstActionDao = new(dbHandler);
                            _ = await hstActionDao.UpdateMoveActionAsync(new HstActionDto {
                                BookId = fromBookId,
                                ActTime = fromDate,
                                ActValue = -actValue,
                                ActionId = this.fromActionId.Value
                            });
                            if (this.selectedBookId == fromBookId) {
                                resActionIdList.Add(this.fromActionId.Value);
                            }

                            _ = await hstActionDao.UpdateMoveActionAsync(new HstActionDto {
                                BookId = toBookId,
                                ActTime = toDate,
                                ActValue = actValue,
                                ActionId = this.toActionId.Value
                            });
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
                            HstActionDao hstActionDao = new(dbHandler);
                            _ = await hstActionDao.UpdateAsync(new HstActionDto {
                                BookId = bookId,
                                ItemId = commissionItemId,
                                ActTime = actTime,
                                ActValue = -commission,
                                Remark = remark,
                                ActionId = this.commissionActionId.Value
                            });
                            resActionIdList.Add(this.commissionActionId.Value);
                        }
                        else {
                            HstActionDao hstActionDao = new(dbHandler);
                            int id = await hstActionDao.InsertReturningIdAsync(new HstActionDto {
                                BookId = bookId,
                                ItemId = commissionItemId,
                                ActTime = actTime,
                                ActValue = -commission,
                                GroupId = tmpGroupId,
                                Remark = remark
                            });
                            resActionIdList.Add(id);
                        }

                        if (remark != string.Empty) {
                            // 備考を追加する
                            HstRemarkDao hstRemarkDao = new(dbHandler);
                            _ = await hstRemarkDao.UpsertAsync(new HstRemarkDto {
                                ItemId = commissionItemId,
                                Remark = remark,
                                UsedTime = actTime
                            });
                        }
                        #endregion
                    }
                    else {
                        #region 手数料なし
                        if (this.commissionActionId != null) {
                            HstActionDao hstActionDao = new(dbHandler);
                            _ = await hstActionDao.DeleteByIdAsync(this.commissionActionId.Value);
                        }
                        #endregion
                    }
                });

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

            if (settings.App_IsPositionSaved && -10 <= settings.MoveRegistrationWindow_Left && 0 <= settings.MoveRegistrationWindow_Top) {
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

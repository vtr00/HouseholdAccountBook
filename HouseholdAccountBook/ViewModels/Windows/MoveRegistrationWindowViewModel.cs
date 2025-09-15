using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Models.Dao.Compositions;
using HouseholdAccountBook.Models.Dao.DbTable;
using HouseholdAccountBook.Models.DbHandler.Abstract;
using HouseholdAccountBook.Models.Dto.DbTable;
using HouseholdAccountBook.Models.Dto.Others;
using HouseholdAccountBook.Others;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static HouseholdAccountBook.Views.UiConstants;

namespace HouseholdAccountBook.ViewModels.Windows
{
    /// <summary>
    /// 帳簿項目登録ウィンドウ(移動)VM
    /// </summary>
    public class MoveRegistrationWindowViewModel : WindowViewModelBase
    {
        #region フィールド
        /// <summary>
        /// 変更時処理重複防止用フラグ
        /// </summary>
        private bool isUpdateOnChanged = false;
        #endregion

        #region イベント
        /// <summary>
        /// 移動元帳簿変更時イベント
        /// </summary>
        public event EventHandler<EventArgs<int?>> FromBookChanged;
        /// <summary>
        /// 移動先帳簿変更時イベント
        /// </summary>
        public event EventHandler<EventArgs<int?>> ToBookChanged;
        /// <summary>
        /// 手数料種別変更時イベント
        /// </summary>
        public event EventHandler<EventArgs<CommissionKind>> CommissionKindChanged;
        /// <summary>
        /// 項目変更時イベント
        /// </summary>
        public event EventHandler<EventArgs<int?>> ItemChanged;

        /// <summary>
        /// 登録時のイベント
        /// </summary>
        public event EventHandler<EventArgs<List<int>>> Registrated;
        #endregion

        #region Bindingプロパティ
        /// <summary>
        /// 登録種別
        /// </summary>
        #region RegKind
        public RegistrationKind RegKind
        {
            get => this._RegKind;
            set => this.SetProperty(ref this._RegKind, value);
        }
        private RegistrationKind _RegKind = default;
        #endregion

        /// <summary>
        /// 移動元帳簿項目ID
        /// </summary>
        #region FromId
        public int? FromId
        {
            get => this._FromId;
            set => this.SetProperty(ref this._FromId, value);
        }
        private int? _FromId = default;
        #endregion
        /// <summary>
        /// 移動先帳簿項目ID
        /// </summary>
        #region ToId
        public int? ToId
        {
            get => this._ToId;
            set => this.SetProperty(ref this._ToId, value);
        }
        private int? _ToId = default;
        #endregion
        /// <summary>
        /// グループID
        /// </summary>
        #region GroupId
        public int? GroupId
        {
            get => this._GroupId;
            set => this.SetProperty(ref this._GroupId, value);
        }
        private int? _GroupId = default;
        #endregion

        /// <summary>
        /// 帳簿VMリスト
        /// </summary>
        #region BookVMList
        public ObservableCollection<BookViewModel> BookVMList
        {
            get => this._BookVMList;
            set => this.SetProperty(ref this._BookVMList, value);
        }
        private ObservableCollection<BookViewModel> _BookVMList = default;
        #endregion
        /// <summary>
        /// 選択された移動元帳簿VM
        /// </summary>
        #region SelectedFromBookVM
        public BookViewModel SelectedFromBookVM
        {
            get => this._SelectedFromBookVM;
            set {
                if (this.SetProperty(ref this._SelectedFromBookVM, value)) {
                    if (!this.isUpdateOnChanged) {
                        this.isUpdateOnChanged = true;
                        this.FromBookChanged?.Invoke(this, new EventArgs<int?>(value.Id));
                        this.isUpdateOnChanged = false;
                    }
                }
            }
        }
        private BookViewModel _SelectedFromBookVM = default;
        #endregion
        /// <summary>
        /// 選択された移動先帳簿VM
        /// </summary>
        #region SelectedToBookVM
        public BookViewModel SelectedToBookVM
        {
            get => this._SelectedToBookVM;
            set {
                if (this.SetProperty(ref this._SelectedToBookVM, value)) {
                    if (!this.isUpdateOnChanged) {
                        this.isUpdateOnChanged = true;
                        this.ToBookChanged?.Invoke(this, new EventArgs<int?>(value.Id));
                        this.isUpdateOnChanged = false;
                    }
                }
            }
        }
        private BookViewModel _SelectedToBookVM = default;
        #endregion

        /// <summary>
        /// 日付(移動元)
        /// </summary>
        #region FromDate
        public DateTime FromDate
        {
            get => this._FromDate;
            set {
                if (this.SetProperty(ref this._FromDate, value)) {
                    CommandManager.InvalidateRequerySuggested();

                    if (this.ToDate < value || this.IsLink) {
                        this.ToDate = value;
                    }
                }
            }
        }
        private DateTime _FromDate = DateTime.Today;
        #endregion
        /// <summary>
        /// 日付(移動先)
        /// </summary>
        #region ToDate
        public DateTime ToDate
        {
            get => this._ToDate;
            set {
                if (this.SetProperty(ref this._ToDate, value)) {
                    CommandManager.InvalidateRequerySuggested();

                    if (value < this.FromDate) {
                        this.FromDate = value;
                        this.IsLink = true;
                    }
                }
            }
        }
        private DateTime _ToDate = DateTime.Today;
        #endregion
        /// <summary>
        /// 移動先日時が移動元日時に連動して編集
        /// </summary>
        #region IsLink
        public bool IsLink
        {
            get => this._IsLink;
            set {
                if (this.SetProperty(ref this._IsLink, value)) {
                    if (value) {
                        this.ToDate = this.FromDate;
                    }
                }
            }
        }
        private bool _IsLink = true;
        #endregion

        /// <summary>
        /// 金額
        /// </summary>
        #region Value
        public int? Value
        {
            get => this._Value;
            set {
                if (this.SetProperty(ref this._Value, value)) {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        private int? _Value = null;
        #endregion

        /// <summary>
        /// 手数料ID
        /// </summary>
        #region CommissionId
        public int? CommissionId
        {
            get => this._CommissionId;
            set => this.SetProperty(ref this._CommissionId, value);
        }
        private int? _CommissionId = default;
        #endregion

        /// <summary>
        /// 手数料種別辞書
        /// </summary>
        #region CommissionKindDic
        public Dictionary<CommissionKind, string> CommissionKindDic { get; } = CommissionKindStr;
        #endregion
        /// <summary>
        /// 選択された手数料種別
        /// </summary>
        #region SelectedCommissionKind
        public CommissionKind SelectedCommissionKind
        {
            get => this._SelectedCommissionKind;
            set {
                if (this.SetProperty(ref this._SelectedCommissionKind, value)) {
                    if (!this.isUpdateOnChanged) {
                        this.isUpdateOnChanged = true;
                        CommissionKindChanged?.Invoke(this, new EventArgs<CommissionKind>(value));
                        this.isUpdateOnChanged = false;
                    }
                }
            }
        }
        private CommissionKind _SelectedCommissionKind = default;
        #endregion

        /// <summary>
        /// 手数料項目VMリスト
        /// </summary>
        #region ItemVMList
        public ObservableCollection<ItemViewModel> ItemVMList
        {
            get => this._ItemVMList;
            set => this.SetProperty(ref this._ItemVMList, value);
        }
        private ObservableCollection<ItemViewModel> _ItemVMList = default;
        #endregion
        /// <summary>
        /// 選択された手数料項目VM
        /// </summary>
        #region SelectedItemVM
        public ItemViewModel SelectedItemVM
        {
            get => this._SelectedItemVM;
            set {
                if (this.SetProperty(ref this._SelectedItemVM, value)) {
                    if (!this.isUpdateOnChanged) {
                        this.isUpdateOnChanged = true;
                        ItemChanged?.Invoke(this, new EventArgs<int?>(value?.Id));
                        this.isUpdateOnChanged = false;
                    }
                }
            }
        }
        private ItemViewModel _SelectedItemVM = default;
        #endregion

        /// <summary>
        /// 手数料
        /// </summary>
        #region Commission
        public int? Commission
        {
            get => this._Commission;
            set {
                if (this.SetProperty(ref this._Commission, value)) {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        private int? _Commission = null;
        #endregion

        /// <summary>
        /// 備考VMリスト
        /// </summary>
        #region RemarkVMList
        public ObservableCollection<RemarkViewModel> RemarkVMList
        {
            get => this._RemarkVMList;
            set => this.SetProperty(ref this._RemarkVMList, value);
        }
        private ObservableCollection<RemarkViewModel> _RemarkVMList = default;
        #endregion
        /// <summary>
        /// 選択された備考
        /// </summary>
        #region SelectedRemark
        public string SelectedRemark
        {
            get => this._SelectedRemark;
            set => this.SetProperty(ref this._SelectedRemark, value);
        }
        private string _SelectedRemark = default;
        #endregion

        public ICommand TodayCommand => new RelayCommand(() => { this.FromDate = DateTime.Today; }, () => { return this.FromDate != DateTime.Today; });
        #endregion

        protected override bool OKCommand_CanExecute()
        {
            return this.Value.HasValue && this.SelectedFromBookVM != this.SelectedToBookVM;
        }
        protected override async void OKCommand_Executed()
        {
            // DB登録
            List<int> idList = null;
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                idList = await this.RegisterMoveInfoAsync();
            }
            this.Registrated?.Invoke(this, new EventArgs<List<int>>(idList));

            base.OKCommand_Executed();
        }

        #region ウィンドウ設定プロパティ
        public override Rect WindowRectSetting
        {
            set {
                Properties.Settings settings = Properties.Settings.Default;

                if (settings.App_IsPositionSaved) {
                    settings.MoveRegistrationWindow_Left = value.Left;
                    settings.MoveRegistrationWindow_Top = value.Top;
                }

                settings.MoveRegistrationWindow_Width = value.Width;
                settings.MoveRegistrationWindow_Height = value.Height;
                settings.Save();
            }
        }

        public override Size? WindowSizeSetting
        {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return WindowSizeSettingImpl(settings.MoveRegistrationWindow_Width, settings.MoveRegistrationWindow_Height);
            }
        }

        public override Point? WindowPointSetting
        {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return WindowPointSettingImpl(settings.MoveRegistrationWindow_Left, settings.MoveRegistrationWindow_Top, settings.App_IsPositionSaved);
            }
        }
        #endregion

        /// <summary>
        /// DBから読み込む
        /// </summary>
        /// <param name="selectedBookId">選択された帳簿ID</param>
        /// <param name="selectedGroupId">選択された帳簿項目のグループID</param>
        /// <param name="selectedMonth">選択された年月</param>
        /// <param name="selectedDate">選択された日付</param>
        /// <returns></returns>
        public async Task LoadMoveInfoAsync(int? selectedBookId, int? selectedGroupId, DateTime? selectedMonth, DateTime? selectedDate)
        {
            int? fromBookId = selectedBookId;
            int? toBookId = selectedBookId;
            int? commissionItemId = null;
            string commissionRemark = null;

            // DBから値を読み込む
            switch (this.RegKind) {
                case RegistrationKind.Add:
                    this.FromDate = selectedDate ?? ((selectedMonth == null || selectedMonth?.Month == DateTime.Today.Month) ? DateTime.Today : selectedMonth.Value);
                    break;
                case RegistrationKind.Edit:
                case RegistrationKind.Copy: {
                    int? fromActionId = null;
                    DateTime fromDate = DateTime.Now;
                    int? toActionId = null;
                    DateTime toDate = DateTime.Now;
                    int moveValue = -1;
                    CommissionKind commissionKind = CommissionKind.MoveFrom;
                    int? commissionActionId = null;
                    int? commissionValue = null;

                    await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                        MoveActionInfoDao moveActionInfoDao = new(dbHandler);
                        var dtoList = await moveActionInfoDao.GetAllAsync(selectedGroupId.Value);
                        foreach (MoveActionInfoDto dto in dtoList) {
                            if (dto.MoveFlg == 1) {
                                if (dto.ActValue < 0) {
                                    fromBookId = dto.BookId;
                                    fromDate = dto.ActTime;
                                    fromActionId = dto.ActionId;
                                }
                                else {
                                    toBookId = dto.BookId;
                                    toDate = dto.ActTime;
                                    toActionId = dto.ActionId;
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
                                commissionActionId = dto.ActionId;
                                commissionItemId = dto.ItemId;
                                commissionValue = Math.Abs(dto.ActValue);
                                commissionRemark = dto.Remark;
                            }
                        }
                    }

                    // WVMに値を設定する
                    if (this.RegKind == RegistrationKind.Edit) {
                        this.FromId = fromActionId;
                        this.ToId = toActionId;
                        this.GroupId = selectedGroupId;
                        this.CommissionId = commissionActionId;
                    }
                    this.IsLink = fromDate == toDate;
                    this.FromDate = fromDate;
                    this.ToDate = toDate;
                    this.SelectedCommissionKind = commissionKind;
                    this.Value = moveValue;
                    this.Commission = commissionValue;
                }
                break;
            }

            // リストを更新する
            await this.UpdateBookListAsync(fromBookId, toBookId);
            await this.UpdateItemListAsync(commissionItemId);
            await this.UpdateRemarkListAsync(commissionRemark);

            this.AddEventHandlers();
        }

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
                        switch (this.RegKind) {
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

            this.BookVMList = bookVMList;
            this.SelectedFromBookVM = fromBookVM;
            this.SelectedToBookVM = toBookVM;

            switch (this.RegKind) {
                case RegistrationKind.Add: {
                    if (debitBookId != null) {
                        this.SelectedFromBookVM = bookVMList.FirstOrDefault((vm) => { return vm.Id == debitBookId; });
                    }
                    if (payDay != null) {
                        this.FromDate = this.FromDate.GetDateInMonth(payDay.Value);
                    }
                    this.IsLink = true;
                    this.SelectedCommissionKind = CommissionKind.MoveFrom;
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
                switch (this.SelectedCommissionKind) {
                    case CommissionKind.MoveFrom:
                        bookId = this.SelectedFromBookVM.Id.Value;
                        break;
                    case CommissionKind.MoveTo:
                        bookId = this.SelectedToBookVM.Id.Value;
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
            this.ItemVMList = itemVMList;
            this.SelectedItemVM = selectedItemVM;
        }

        /// <summary>
        /// 備考リストを更新する
        /// </summary>
        /// <param name="remark">選択対象の備考</param>
        /// <returns></returns>
        private async Task UpdateRemarkListAsync(string remark = null)
        {
            if (this?.SelectedItemVM?.Id == null) return;

            ObservableCollection<RemarkViewModel> remarkVMList = [
                new RemarkViewModel(){ Remark = string.Empty }
            ];
            string selectedRemark = remark ?? this.SelectedRemark ?? remarkVMList[0].Remark;
            RemarkViewModel selectedRemarkVM = remarkVMList[0]; // UNUSED
            await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                RemarkInfoDao remarkInfoDao = new(dbHandler);
                var dtoList = await remarkInfoDao.FindByItemIdAsync(this.SelectedItemVM.Id);
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

            this.RemarkVMList = remarkVMList;
            this.SelectedRemark = selectedRemark;
        }

        /// <summary>
        /// イベントハンドラを登録する
        /// </summary>
        private void AddEventHandlers()
        {
            this.FromBookChanged += async (_, _) => {
                using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                    await this.UpdateItemListAsync();
                    await this.UpdateRemarkListAsync();
                }
            };
            this.ToBookChanged += async (_, _) => {
                using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                    await this.UpdateItemListAsync();
                    await this.UpdateRemarkListAsync();
                }
            };
            this.CommissionKindChanged += async (_, _) => {
                using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                    await this.UpdateItemListAsync();
                    await this.UpdateRemarkListAsync();
                }
            };
            this.ItemChanged += async (_, _) => {
                using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                    await this.UpdateRemarkListAsync();
                }
            };
        }

        /// <summary>
        /// DBに登録する
        /// </summary>
        /// <returns>登録された帳簿項目IDリスト</returns>
        private async Task<List<int>> RegisterMoveInfoAsync()
        {
            List<int> resActionIdList = [];

            DateTime fromDate = this.FromDate;
            int fromId = this.FromId ?? -1;
            DateTime toDate = this.ToDate;
            int toId = this.ToId ?? -1;
            int fromBookId = this.SelectedFromBookVM.Id.Value;
            int toBookId = this.SelectedToBookVM.Id.Value;
            int actValue = this.Value.Value;
            CommissionKind commissionKind = this.SelectedCommissionKind;
            int commissionItemId = this.SelectedItemVM.Id;
            int commissionId = this.CommissionId ?? -1;
            int commission = this.Commission ?? 0;
            string remark = this.SelectedRemark;

            int tmpGroupId = -1; // ローカル用
            await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                await dbHandler.ExecTransactionAsync(async () => {
                    switch (this.RegKind) {
                        case RegistrationKind.Add:
                        case RegistrationKind.Copy: {
                            #region 帳簿項目を追加する
                            // グループIDを取得する
                            HstGroupDao hstGroupDao = new(dbHandler);
                            tmpGroupId = await hstGroupDao.InsertReturningIdAsync(new HstGroupDto { GroupKind = (int)GroupKind.Move });

                            HstActionDao hstActionDao = new(dbHandler);
                            fromId = await hstActionDao.InsertMoveActionReturningIdAsync(new HstActionDto {
                                BookId = fromBookId,
                                ActTime = fromDate,
                                ActValue = -actValue,
                                GroupId = tmpGroupId
                            }, (int)BalanceKind.Expenses);

                            toId = await hstActionDao.InsertMoveActionReturningIdAsync(new HstActionDto {
                                BookId = toBookId,
                                ActTime = toDate,
                                ActValue = actValue,
                                GroupId = tmpGroupId
                            }, (int)BalanceKind.Income);
                            #endregion
                            break;
                        }
                        case RegistrationKind.Edit: {
                            #region 帳簿項目を変更する
                            tmpGroupId = this.GroupId.Value;

                            HstActionDao hstActionDao = new(dbHandler);
                            _ = await hstActionDao.UpdateMoveActionAsync(new HstActionDto {
                                BookId = fromBookId,
                                ActTime = fromDate,
                                ActValue = -actValue,
                                ActionId = fromId
                            });

                            _ = await hstActionDao.UpdateMoveActionAsync(new HstActionDto {
                                BookId = toBookId,
                                ActTime = toDate,
                                ActValue = actValue,
                                ActionId = toId
                            });
                            #endregion
                            break;
                        }
                    }
                    resActionIdList.Add(fromId);
                    resActionIdList.Add(toId);

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

                        if (commissionId != -1) {
                            // 手数料が登録済のとき更新する
                            HstActionDao hstActionDao = new(dbHandler);
                            _ = await hstActionDao.UpdateAsync(new HstActionDto {
                                BookId = bookId,
                                ItemId = commissionItemId,
                                ActTime = actTime,
                                ActValue = -commission,
                                Remark = remark,
                                ActionId = commissionId
                            });
                        }
                        else {
                            // 手数料が未登録のとき追加する
                            HstActionDao hstActionDao = new(dbHandler);
                            commissionId = await hstActionDao.InsertReturningIdAsync(new HstActionDto {
                                BookId = bookId,
                                ItemId = commissionItemId,
                                ActTime = actTime,
                                ActValue = -commission,
                                GroupId = tmpGroupId,
                                Remark = remark
                            });
                        }
                        resActionIdList.Add(commissionId);

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
                        if (this.CommissionId != null) {
                            HstActionDao hstActionDao = new(dbHandler);
                            _ = await hstActionDao.DeleteByIdAsync(commissionId);
                        }
                        #endregion
                    }
                });

                return resActionIdList;
            }
        }
    }
}

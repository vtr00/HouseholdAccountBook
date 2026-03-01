using HouseholdAccountBook.Models;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static HouseholdAccountBook.ViewModels.UiConstants;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities.Args;
using HouseholdAccountBook.Infrastructure.Utilities.Extensions;
using HouseholdAccountBook.Infrastructure.DB.DbDao.Compositions;
using HouseholdAccountBook.Infrastructure.DB.DbDao.DbTable;
using HouseholdAccountBook.Infrastructure.DB.DbDto.Others;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Infrastructure.DB.DbDto.DbTable;

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
        private bool mIsUpdateOnChanged;
        #endregion

        #region イベント
        /// <summary>
        /// 移動元帳簿変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<BookIdObj>> FromBookChanged;
        /// <summary>
        /// 移動先帳簿変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<BookIdObj>> ToBookChanged;
        /// <summary>
        /// 手数料種別変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<CommissionKind>> CommissionKindChanged;
        /// <summary>
        /// 項目変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<ItemIdObj>> ItemChanged;

        /// <summary>
        /// 登録時イベント
        /// </summary>
        public event EventHandler<EventArgs<List<ActionIdObj>>> Registrated;
        #endregion

        #region Bindingプロパティ
        /// <summary>
        /// 登録種別
        /// </summary>
        #region RegKind
        public RegistrationKind RegKind {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 移動元帳簿項目ID
        /// </summary>
        #region FromActionId
        public ActionIdObj FromActionId {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 移動先帳簿項目ID
        /// </summary>
        #region ToActionId
        public ActionIdObj ToActionId {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// グループID
        /// </summary>
        #region GroupId
        public GroupIdObj GroupId {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 帳簿VMリスト
        /// </summary>
        #region BookVMList
        public ObservableCollection<BookModel> BookVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 選択された移動元帳簿VM
        /// </summary>
        #region SelectedFromBookVM
        public BookModel SelectedFromBookVM {
            get;
            set {
                var oldValue = field;
                if (this.SetProperty(ref field, value)) {
                    if (!this.mIsUpdateOnChanged) {
                        this.mIsUpdateOnChanged = true;
                        this.FromBookChanged?.Invoke(this, new() { OldValue = oldValue?.Id, NewValue = value?.Id });
                        this.mIsUpdateOnChanged = false;
                    }
                }
            }
        }
        #endregion
        /// <summary>
        /// 選択された移動先帳簿VM
        /// </summary>
        #region SelectedToBookVM
        public BookModel SelectedToBookVM {
            get;
            set {
                var oldValue = field;
                if (this.SetProperty(ref field, value)) {
                    if (!this.mIsUpdateOnChanged) {
                        this.mIsUpdateOnChanged = true;
                        this.ToBookChanged?.Invoke(this, new() { OldValue = oldValue?.Id, NewValue = value?.Id });
                        this.mIsUpdateOnChanged = false;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 選択された日付(移動元)
        /// </summary>
        #region SelectedFromDate
        public DateTime SelectedFromDate {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    CommandManager.InvalidateRequerySuggested();

                    if (this.SelectedToDate < value || this.IsLink) {
                        this.SelectedToDate = value;
                    }
                }
            }
        } = DateTime.Today;
        #endregion
        /// <summary>
        /// 選択された日付(移動先)
        /// </summary>
        #region SelectedToDate
        public DateTime SelectedToDate {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    CommandManager.InvalidateRequerySuggested();

                    if (value < this.SelectedFromDate) {
                        this.SelectedFromDate = value;
                        this.IsLink = true;
                    }
                }
            }
        } = DateTime.Today;
        #endregion
        /// <summary>
        /// 移動先日時が移動元日時に連動して編集
        /// </summary>
        #region IsLink
        public bool IsLink {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    if (value) {
                        this.SelectedToDate = this.SelectedFromDate;
                    }
                }
            }
        } = true;
        #endregion

        /// <summary>
        /// 入力された金額
        /// </summary>
        #region InputedValue
        public decimal? InputedValue {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        #endregion

        /// <summary>
        /// 手数料の帳簿項目ID
        /// </summary>
        #region CommissionId
        public ActionIdObj CommissionId {
            get;
            set => this.SetProperty(ref field, value);
        }
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
        public CommissionKind SelectedCommissionKind {
            get;
            set {
                var oldValue = field;
                if (this.SetProperty(ref field, value)) {
                    if (!this.mIsUpdateOnChanged) {
                        this.mIsUpdateOnChanged = true;
                        CommissionKindChanged?.Invoke(this, new() { OldValue = oldValue, NewValue = value });
                        this.mIsUpdateOnChanged = false;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 手数料項目VMリスト
        /// </summary>
        #region ItemVMList
        public ObservableCollection<ItemModel> ItemVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 選択された手数料項目VM
        /// </summary>
        #region SelectedItemVM
        public ItemModel SelectedItemVM {
            get;
            set {
                var oldValue = field;
                if (this.SetProperty(ref field, value)) {
                    if (!this.mIsUpdateOnChanged) {
                        this.mIsUpdateOnChanged = true;
                        ItemChanged?.Invoke(this, new() { OldValue = oldValue?.Id, NewValue = value?.Id });
                        this.mIsUpdateOnChanged = false;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 入力された手数料
        /// </summary>
        #region InputedCommission
        public decimal? InputedCommission {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        #endregion

        /// <summary>
        /// 備考VMリスト
        /// </summary>
        #region RemarkVMList
        public ObservableCollection<RemarkModel> RemarkVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 選択された備考
        /// </summary>
        #region SelectedRemark
        public string SelectedRemark {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        #region コマンド
        /// <summary>
        /// 今日コマンド
        /// </summary>
        public ICommand TodayCommand => new RelayCommand(() => this.SelectedFromDate = DateTime.Today, () => this.SelectedFromDate != DateTime.Today);
        #endregion
        #endregion

        #region コマンドイベントハンドラ
        protected override bool OKCommand_CanExecute() => this.InputedValue is not null && this.SelectedFromBookVM != this.SelectedToBookVM;
        protected override async void OKCommand_Executed()
        {
            // DB登録
            List<ActionIdObj> idList = null;
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                idList = await this.SaveAsync();
            }
            this.Registrated?.Invoke(this, new EventArgs<List<ActionIdObj>>(idList));

            base.OKCommand_Executed();
        }
        #endregion

        #region ウィンドウ設定プロパティ
        protected override (double, double) WindowSizeSettingRaw {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return (settings.MoveRegistrationWindow_Width, settings.MoveRegistrationWindow_Height);
            }
            set {
                Properties.Settings settings = Properties.Settings.Default;
                settings.MoveRegistrationWindow_Width = value.Item1;
                settings.MoveRegistrationWindow_Height = value.Item2;
                settings.Save();
            }
        }

        public override Point WindowPointSetting {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return new Point(settings.MoveRegistrationWindow_Left, settings.MoveRegistrationWindow_Top);
            }
            set {
                Properties.Settings settings = Properties.Settings.Default;
                settings.MoveRegistrationWindow_Left = value.X;
                settings.MoveRegistrationWindow_Top = value.Y;
                settings.Save();
            }
        }
        #endregion

        public override async Task LoadAsync() => await this.LoadAsync(null, null, null, null);

        public override void AddEventHandlers()
        {
            using FuncLog funcLog = new();

            this.FromBookChanged += async (sender, e) => {
                using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, methodName: nameof(this.FromBookChanged));

                using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                    await this.UpdateItemListAsync();
                }
            };
            this.ToBookChanged += async (sender, e) => {
                using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, methodName: nameof(this.ToBookChanged));

                using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                    await this.UpdateItemListAsync();
                }
            };
            this.CommissionKindChanged += async (sender, e) => {
                using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, methodName: nameof(this.CommissionKindChanged));

                using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                    await this.UpdateItemListAsync();
                }
            };
            this.ItemChanged += async (sender, e) => {
                using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, methodName: nameof(this.ItemChanged));

                using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                    await this.UpdateRemarkListAsync();
                }
            };
        }

        /// <summary>
        /// DBから読み込む
        /// </summary>
        /// <param name="initialBookId">追加時、初期選択する帳簿のID</param>
        /// <param name="initialMonth">追加時、初期選択する年月</param>
        /// <param name="initialDate">追加時、初期選択する日付</param>
        /// <param name="targetGroupId">複製/編集時、複製/編集対象の帳簿項目のグループID</param>
        /// <returns></returns>
        public async Task LoadAsync(BookIdObj initialBookId, DateOnly? initialMonth, DateOnly? initialDate, GroupIdObj targetGroupId)
        {
            using FuncLog funcLog = new(new { initialBookId, initialMonth, initialDate, targetGroupId });

            BookIdObj selectingFromBookId = null;
            BookIdObj selectingToBookId = null;
            ItemIdObj selectingCommissionItemId = null;
            string selectingCommissionRemark = null;

            switch (this.RegKind) {
                case RegistrationKind.Add:
                    selectingFromBookId = initialBookId;
                    selectingToBookId = initialBookId;

                    // WVMに値を設定する
                    this.SelectedFromDate = initialDate?.ToDateTime(TimeOnly.MinValue) ?? ((initialMonth == null || initialMonth?.Month == DateTime.Today.Month) ? DateTime.Today : initialMonth.Value.ToDateTime(TimeOnly.MinValue));

                    break;
                case RegistrationKind.Edit:
                case RegistrationKind.Copy: {
                    ActionIdObj fromActionId = null;
                    DateTime fromDate = DateTime.Now;
                    ActionIdObj toActionId = null;
                    DateTime toDate = DateTime.Now;
                    decimal moveValue = -1;
                    CommissionKind commissionKind = CommissionKind.MoveFrom;
                    ActionIdObj commissionActionId = null;
                    decimal? commissionValue = null;

                    // DBから値を読み込む
                    await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                        MoveActionInfoDao moveActionInfoDao = new(dbHandler);
                        var dtoList = (await moveActionInfoDao.GetAllAsync(targetGroupId.Value)).OrderBy(dto => dto.MoveFlg).Reverse(); // 手数料を後ろにするために並び替え

                        foreach (MoveActionInfoDto dto in dtoList) {
                            if (dto.MoveFlg == 1) {
                                if (dto.ActValue < 0) {
                                    selectingFromBookId = dto.BookId;
                                    fromDate = dto.ActTime;
                                    fromActionId = dto.ActionId;
                                }
                                else {
                                    selectingToBookId = dto.BookId;
                                    toDate = dto.ActTime;
                                    toActionId = dto.ActionId;
                                    moveValue = dto.ActValue;
                                }
                            }
                            else { // 手数料
                                if (dto.BookId == (int)selectingFromBookId) { // 移動元負担
                                    commissionKind = CommissionKind.MoveFrom;
                                }
                                else if (dto.BookId == (int)selectingToBookId) { // 移動先負担
                                    commissionKind = CommissionKind.MoveTo;
                                }
                                commissionActionId = dto.ActionId;
                                selectingCommissionItemId = dto.ItemId;
                                commissionValue = Math.Abs(dto.ActValue);
                                selectingCommissionRemark = dto.Remark;
                            }
                        }
                    }

                    // WVMに値を設定する
                    if (this.RegKind == RegistrationKind.Edit) {
                        this.FromActionId = fromActionId;
                        this.ToActionId = toActionId;
                        this.GroupId = targetGroupId;
                        this.CommissionId = commissionActionId;
                    }
                    this.IsLink = fromDate == toDate;
                    this.SelectedFromDate = fromDate;
                    this.SelectedToDate = toDate;
                    this.SelectedCommissionKind = commissionKind;
                    this.InputedValue = moveValue;
                    this.InputedCommission = commissionValue;

                    break;
                }
            }

            // リストを更新する
            await this.UpdateBookListAsync(selectingFromBookId, selectingToBookId);
            await this.UpdateItemListAsync(selectingCommissionItemId);
            await this.UpdateRemarkListAsync(selectingCommissionRemark);
        }

        /// <summary>
        /// 帳簿リストを更新する
        /// </summary>
        /// <param name="selectingFromBookId">選択対象の移動元帳簿ID</param>
        /// <param name="selectingToBookId">選択対象の移動先帳簿ID</param>
        /// <returns></returns>
        private async Task UpdateBookListAsync(BookIdObj selectingFromBookId = null, BookIdObj selectingToBookId = null)
        {
            using FuncLog funcLog = new(new { selectingFromBookId, selectingToBookId });

            AppService service = new(this.mDbHandlerFactory);
            this.BookVMList = [.. await service.LoadBookListAsync()];
            this.SelectedFromBookVM = this.BookVMList.FirstOrElementAtOrDefault(vm => vm.Id == selectingFromBookId, 0);
            this.SelectedToBookVM = this.BookVMList.FirstOrElementAtOrDefault(vm => vm.Id == selectingToBookId, 0);

            switch (this.RegKind) {
                case RegistrationKind.Add: {
                    if (this.SelectedToBookVM.BookKind == BookKind.CreditCard) {
                        if (this.SelectedToBookVM.DebitBookId != null) {
                            this.SelectedFromBookVM = this.BookVMList.FirstOrElementAtOrDefault(vm => vm.Id == this.SelectedToBookVM.DebitBookId, 0);
                        }
                        if (this.SelectedToBookVM.PayDay != null) {
                            this.SelectedFromDate = this.SelectedFromDate.GetDateInMonth(this.SelectedToBookVM.PayDay.Value);
                        }
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
        /// <param name="selectingItemId">選択対象の項目</param>
        /// <returns></returns>
        private async Task UpdateItemListAsync(ItemIdObj selectingItemId = null)
        {
            if (this.SelectedFromBookVM == null || this.SelectedToBookVM == null) { return; }

            using FuncLog funcLog = new(new { selectingItemId });

            BookIdObj bookId = this.SelectedCommissionKind switch {
                CommissionKind.MoveFrom => this.SelectedFromBookVM.Id,
                CommissionKind.MoveTo => this.SelectedToBookVM.Id,
                _ => throw new ArgumentException("SelectedComissionKind"),
            };
            AppService service = new(this.mDbHandlerFactory);
            ItemIdObj tmpItemId = selectingItemId ?? this.SelectedItemVM?.Id;
            this.ItemVMList = [.. await service.LoadItemListAsync(bookId, BalanceKind.Expenses, -1)];
            this.SelectedItemVM = this.ItemVMList.FirstOrElementAtOrDefault(vm => vm.Id == tmpItemId, 0);
        }

        /// <summary>
        /// 備考リストを更新する
        /// </summary>
        /// <param name="selectingRemark">選択対象の備考</param>
        /// <returns></returns>
        private async Task UpdateRemarkListAsync(string selectingRemark = null)
        {
            if (this.SelectedItemVM == null) { return; }

            using FuncLog funcLog = new(new { selectingRemark });

            AppService service = new(this.mDbHandlerFactory);
            string tmpRemark = selectingRemark ?? this.SelectedRemark;
            this.RemarkVMList = [.. await service.LoadRemarkListAsync(this.SelectedItemVM.Id)];
            this.SelectedRemark = this.RemarkVMList.FirstOrElementAtOrDefault(vm => vm.Remark == tmpRemark, 0).Remark;
        }

        /// <summary>
        /// DBに登録する
        /// </summary>
        /// <returns>登録された帳簿項目IDリスト</returns>
        protected override async Task<List<ActionIdObj>> SaveAsync()
        {
            using FuncLog funcLog = new();

            List<ActionIdObj> resActionIdList = [];

            DateTime fromDate = this.SelectedFromDate;
            ActionIdObj fromId = this.FromActionId ?? -1;
            DateTime toDate = this.SelectedToDate;
            ActionIdObj toId = this.ToActionId ?? -1;
            BookIdObj fromBookId = this.SelectedFromBookVM.Id;
            BookIdObj toBookId = this.SelectedToBookVM.Id;
            decimal? actValue = this.InputedValue;
            CommissionKind commissionKind = this.SelectedCommissionKind;
            ItemIdObj commissionItemId = this.SelectedItemVM.Id;
            ActionIdObj commissionId = this.CommissionId ?? -1;
            decimal commission = this.InputedCommission ?? 0;
            string remark = this.SelectedRemark;

            int tmpGroupId = -1; // ローカル用
            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
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
                                BookId = (int)fromBookId,
                                ActTime = fromDate,
                                ActValue = (int)-actValue,
                                GroupId = tmpGroupId
                            }, (int)BalanceKind.Expenses);

                            toId = await hstActionDao.InsertMoveActionReturningIdAsync(new HstActionDto {
                                BookId = (int)toBookId,
                                ActTime = toDate,
                                ActValue = (int)actValue,
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
                                BookId = (int)fromBookId,
                                ActTime = fromDate,
                                ActValue = (int)-actValue,
                                ActionId = (int)fromId
                            });

                            _ = await hstActionDao.UpdateMoveActionAsync(new HstActionDto {
                                BookId = (int)toBookId,
                                ActTime = toDate,
                                ActValue = (int)actValue,
                                ActionId = (int)toId
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
                                bookId = (int)fromBookId;
                                actTime = fromDate;
                                break;
                            case CommissionKind.MoveTo:
                                bookId = (int)toBookId;
                                actTime = toDate;
                                break;
                        }

                        if ((int)commissionId != -1) {
                            // 手数料が登録済のとき更新する
                            HstActionDao hstActionDao = new(dbHandler);
                            _ = await hstActionDao.UpdateAsync(new HstActionDto {
                                BookId = bookId,
                                ItemId = (int)commissionItemId,
                                ActTime = actTime,
                                ActValue = (int)-commission,
                                Remark = remark,
                                GroupId = tmpGroupId,
                                ActionId = (int)commissionId
                            });
                        }
                        else {
                            // 手数料が未登録のとき追加する
                            HstActionDao hstActionDao = new(dbHandler);
                            commissionId = await hstActionDao.InsertReturningIdAsync(new HstActionDto {
                                BookId = bookId,
                                ItemId = (int)commissionItemId,
                                ActTime = actTime,
                                ActValue = (int)-commission,
                                Remark = remark,
                                GroupId = tmpGroupId
                            });
                        }
                        resActionIdList.Add(commissionId);

                        if (remark != null && remark != string.Empty) {
                            // 備考を追加/更新する
                            HstRemarkDao hstRemarkDao = new(dbHandler);
                            _ = await hstRemarkDao.UpsertAsync(new HstRemarkDto {
                                ItemId = (int)commissionItemId,
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
                            _ = await hstActionDao.DeleteByIdAsync((int)commissionId);
                        }
                        #endregion
                    }
                });
            }

            return resActionIdList;
        }
    }
}

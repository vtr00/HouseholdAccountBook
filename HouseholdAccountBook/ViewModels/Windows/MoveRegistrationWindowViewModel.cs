using HouseholdAccountBook.Adapters.Dao.Compositions;
using HouseholdAccountBook.Adapters.Dao.DbTable;
using HouseholdAccountBook.Adapters.DbHandler.Abstract;
using HouseholdAccountBook.Adapters.Dto.DbTable;
using HouseholdAccountBook.Adapters.Dto.Others;
using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.Args;
using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Utilites;
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
        private bool mIsUpdateOnChanged;
        #endregion

        #region イベント
        /// <summary>
        /// 移動元帳簿変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<int?>> FromBookChanged;
        /// <summary>
        /// 移動先帳簿変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<int?>> ToBookChanged;
        /// <summary>
        /// 手数料種別変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<CommissionKind>> CommissionKindChanged;
        /// <summary>
        /// 項目変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<int?>> ItemChanged;

        /// <summary>
        /// 登録時イベント
        /// </summary>
        public event EventHandler<EventArgs<List<int>>> Registrated;
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
        #region FromId
        public int? FromId {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 移動先帳簿項目ID
        /// </summary>
        #region ToId
        public int? ToId {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// グループID
        /// </summary>
        #region GroupId
        public int? GroupId {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 帳簿VMリスト
        /// </summary>
        #region BookVMList
        public ObservableCollection<BookViewModel> BookVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 選択された移動元帳簿VM
        /// </summary>
        #region SelectedFromBookVM
        public BookViewModel SelectedFromBookVM {
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
        public BookViewModel SelectedToBookVM {
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
        /// 日付(移動元)
        /// </summary>
        #region FromDate
        public DateTime FromDate {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    CommandManager.InvalidateRequerySuggested();

                    if (this.ToDate < value || this.IsLink) {
                        this.ToDate = value;
                    }
                }
            }
        } = DateTime.Today;
        #endregion
        /// <summary>
        /// 日付(移動先)
        /// </summary>
        #region ToDate
        public DateTime ToDate {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    CommandManager.InvalidateRequerySuggested();

                    if (value < this.FromDate) {
                        this.FromDate = value;
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
                        this.ToDate = this.FromDate;
                    }
                }
            }
        } = true;
        #endregion

        /// <summary>
        /// 金額
        /// </summary>
        #region Value
        public int? Value {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        #endregion

        /// <summary>
        /// 手数料ID
        /// </summary>
        #region CommissionId
        public int? CommissionId {
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
        public ObservableCollection<ItemViewModel> ItemVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 選択された手数料項目VM
        /// </summary>
        #region SelectedItemVM
        public ItemViewModel SelectedItemVM {
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
        /// 手数料
        /// </summary>
        #region Commission
        public int? Commission {
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
        public ObservableCollection<RemarkViewModel> RemarkVMList {
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
        public ICommand TodayCommand => new RelayCommand(() => this.FromDate = DateTime.Today, () => this.FromDate != DateTime.Today);
        #endregion
        #endregion

        #region コマンドイベントハンドラ
        protected override bool OKCommand_CanExecute() => this.Value.HasValue && this.SelectedFromBookVM != this.SelectedToBookVM;
        protected override async void OKCommand_Executed()
        {
            // DB登録
            List<int> idList = null;
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                idList = await this.SaveAsync();
            }
            this.Registrated?.Invoke(this, new EventArgs<List<int>>(idList));

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
        public async Task LoadAsync(int? initialBookId, DateTime? initialMonth, DateTime? initialDate, int? targetGroupId)
        {
            using FuncLog funcLog = new(new { initialBookId, initialMonth, initialDate, targetGroupId });

            int? selectingFromBookId = null;
            int? selectingToBookId = null;
            int? selectingCommissionItemId = null;
            string selectingCommissionRemark = null;

            switch (this.RegKind) {
                case RegistrationKind.Add:
                    selectingFromBookId = initialBookId;
                    selectingToBookId = initialBookId;

                    // WVMに値を設定する
                    this.FromDate = initialDate ?? ((initialMonth == null || initialMonth?.Month == DateTime.Today.Month) ? DateTime.Today : initialMonth.Value);

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
                                if (dto.BookId == selectingFromBookId) { // 移動元負担
                                    commissionKind = CommissionKind.MoveFrom;
                                }
                                else if (dto.BookId == selectingToBookId) { // 移動先負担
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
                        this.FromId = fromActionId;
                        this.ToId = toActionId;
                        this.GroupId = targetGroupId;
                        this.CommissionId = commissionActionId;
                    }
                    this.IsLink = fromDate == toDate;
                    this.FromDate = fromDate;
                    this.ToDate = toDate;
                    this.SelectedCommissionKind = commissionKind;
                    this.Value = moveValue;
                    this.Commission = commissionValue;

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
        private async Task UpdateBookListAsync(int? selectingFromBookId = null, int? selectingToBookId = null)
        {
            using FuncLog funcLog = new(new { selectingFromBookId, selectingToBookId });

            ViewModelLoader loader = new(this.mDbHandlerFactory);
            this.BookVMList = await loader.LoadBookListAsync();
            this.SelectedFromBookVM = this.BookVMList.FirstOrElementAtOrDefault(vm => vm.Id == selectingFromBookId, 0);
            this.SelectedToBookVM = this.BookVMList.FirstOrElementAtOrDefault(vm => vm.Id == selectingToBookId, 0);

            switch (this.RegKind) {
                case RegistrationKind.Add: {
                    if (this.SelectedToBookVM.BookKind == BookKind.CreditCard) {
                        if (this.SelectedToBookVM.DebitBookId != null) {
                            this.SelectedFromBookVM = this.BookVMList.FirstOrElementAtOrDefault(vm => vm.Id == this.SelectedToBookVM.DebitBookId, 0);
                        }
                        if (this.SelectedToBookVM.PayDay != null) {
                            this.FromDate = this.FromDate.GetDateInMonth(this.SelectedToBookVM.PayDay.Value);
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
        private async Task UpdateItemListAsync(int? selectingItemId = null)
        {
            if (this.SelectedFromBookVM == null || this.SelectedToBookVM == null) { return; }

            using FuncLog funcLog = new(new { selectingItemId });

            int bookId = this.SelectedCommissionKind switch {
                CommissionKind.MoveFrom => this.SelectedFromBookVM.Id.Value,
                CommissionKind.MoveTo => this.SelectedToBookVM.Id.Value,
                _ => throw new ArgumentException("SelectedComissionKind"),
            };
            ViewModelLoader loader = new(this.mDbHandlerFactory);
            int? tmpItemId = selectingItemId ?? this.SelectedItemVM?.Id;
            this.ItemVMList = await loader.LoadItemListAsync(bookId, BalanceKind.Expenses, -1);
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

            ViewModelLoader loader = new(this.mDbHandlerFactory);
            string tmpRemark = selectingRemark ?? this.SelectedRemark;
            this.RemarkVMList = await loader.LoadRemarkListAsync(this.SelectedItemVM.Id);
            this.SelectedRemark = this.RemarkVMList.FirstOrElementAtOrDefault(vm => vm.Remark == tmpRemark, 0).Remark;
        }

        /// <summary>
        /// DBに登録する
        /// </summary>
        /// <returns>登録された帳簿項目IDリスト</returns>
        protected override async Task<List<int>> SaveAsync()
        {
            using FuncLog funcLog = new();

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
                                GroupId = tmpGroupId,
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
                                Remark = remark,
                                GroupId = tmpGroupId
                            });
                        }
                        resActionIdList.Add(commissionId);

                        if (remark != string.Empty) {
                            // 備考を追加/更新する
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
            }

            return resActionIdList;
        }
    }
}

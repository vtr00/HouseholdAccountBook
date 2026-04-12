using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities.Args;
using HouseholdAccountBook.Infrastructure.Utilities.Extensions;
using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static HouseholdAccountBook.ViewModels.UiConstants;

namespace HouseholdAccountBook.ViewModels.Windows
{
    /// <summary>
    /// 帳簿項目登録ウィンドウ(移動)VM
    /// </summary>
    public class MoveRegistrationWindowViewModel : WindowViewModelBase
    {
        #region フィールド
        /// <summary>
        /// アプリサービス
        /// </summary>
        private AppCommonService mAppService;
        /// <summary>
        /// 帳簿項目登録サービス
        /// </summary>
        private ActionRegService mService;
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
        public event EventHandler<EventArgs<IEnumerable<ActionIdObj>>> Registrated;
        #endregion

        #region Bindingプロパティ
        /// <summary>
        /// 登録種別
        /// </summary>
        public RegistrationKind RegKind {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 移動元帳簿項目ID
        /// </summary>
        public ActionIdObj FromActionId {
            get;
            set => this.SetProperty(ref field, value);
        }
        /// <summary>
        /// 移動先帳簿項目ID
        /// </summary>
        public ActionIdObj ToActionId {
            get;
            set => this.SetProperty(ref field, value);
        }
        /// <summary>
        /// グループID
        /// </summary>
        public GroupIdObj GroupId {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 移動元帳簿セレクタVM
        /// </summary>
        public SelectorViewModel<BookModel, BookIdObj> FromBookSelectorVM { get; } = new(static vm => vm?.Id);
        /// <summary>
        /// 移動先帳簿セレクタVM
        /// </summary>
        public SelectorViewModel<BookModel, BookIdObj> ToBookSelectorVM { get; } = new(static vm => vm?.Id);

        /// <summary>
        /// 選択された日付(移動元)
        /// </summary>
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
        /// <summary>
        /// 選択された日付(移動先)
        /// </summary>
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
        /// <summary>
        /// 移動先日時が移動元日時に連動して編集
        /// </summary>
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

        /// <summary>
        /// 入力された金額
        /// </summary>
        public decimal? InputedValue {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        /// <summary>
        /// 手数料の帳簿項目ID
        /// </summary>
        public ActionIdObj CommissionId {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 手数料種別セレクタVM
        /// </summary>
        public SelectorViewModel<KeyValuePair<CommissionKind, string>, CommissionKind> CommissionKindSelectorVM { get; } = new(static p => p.Key);

        /// <summary>
        /// 手数料項目セレクタVM
        /// </summary>
        public SelectorViewModel<ItemModel, ItemIdObj> ItemSelectorVM { get; } = new(static vm => vm?.Id);

        /// <summary>
        /// 入力された手数料
        /// </summary>
        public decimal? InputedCommission {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        /// <summary>
        /// 備考セレクタVM
        /// </summary>
        public SelectorViewModel<RemarkModel, string> RemarkSelectorVM { get; } = new(static vm => vm?.Remark);

        #region コマンド
        /// <summary>
        /// 今日コマンド
        /// </summary>
        public ICommand TodayCommand => field ??= new RelayCommand(() => this.SelectedFromDate = DateTime.Today, () => this.SelectedFromDate != DateTime.Today);
        /// <summary>
        /// OKコマンド
        /// </summary>
        public new ICommand OKCommand => field ??= new AsyncRelayCommand(this.OKCommand_ExecuteAsync, this.OKCommand_CanExecute);
        #endregion
        #endregion

        #region コマンドイベントハンドラ
        protected override bool OKCommand_CanExecute() => this.InputedValue is not null && this.FromBookSelectorVM?.SelectedKey != this.ToBookSelectorVM?.SelectedKey;
        protected async Task OKCommand_ExecuteAsync()
        {
            // DB登録
            IEnumerable<ActionIdObj> idList = null;
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                idList = await this.SaveAsync();
            }
            this.Registrated?.Invoke(this, new EventArgs<IEnumerable<ActionIdObj>>(idList));

            base.OKCommand_Execute();
        }
        #endregion

        #region ウィンドウ設定プロパティ
        protected override (double, double) WindowSizeSettingRaw {
            get => UserSettingService.Instance.MoveRegistrationWindowSize;
            set => UserSettingService.Instance.MoveRegistrationWindowSize = value;
        }

        public override Point WindowPointSetting {
            get => UserSettingService.Instance.MoveRegistrationWindowPoint;
            set => UserSettingService.Instance.MoveRegistrationWindowPoint = value;
        }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MoveRegistrationWindowViewModel()
        {
            using FuncLog funcLog = new();

            this.FromBookSelectorVM.SetLoader(async () => await this.mAppService.LoadBookListAsync());
            this.ToBookSelectorVM.SetLoader(async () => await this.mAppService.LoadBookListAsync());
            this.CommissionKindSelectorVM.SetLoader(() => CommissionKindStr);
            this.ItemSelectorVM.SetLoader(
                async () => {
                    BookIdObj bookId = this.CommissionKindSelectorVM.SelectedKey switch {
                        CommissionKind.MoveFrom => this.FromBookSelectorVM.SelectedKey,
                        CommissionKind.MoveTo => this.ToBookSelectorVM.SelectedKey,
                        _ => throw new NotSupportedException("SelectedComissionKind"),
                    };
                    return await this.mAppService.LoadItemListAsync(bookId, BalanceKind.Expenses, CategoryIdObj.System);
                },
                () => this.FromBookSelectorVM.SelectedKey != null && this.ToBookSelectorVM.SelectedKey != null);
            this.RemarkSelectorVM.SetLoader(
                async () => await this.mAppService.LoadRemarkListAsync(this.ItemSelectorVM.SelectedKey, true),
                () => this.ItemSelectorVM.SelectedKey != null, SelectorMode.Force);
        }

        public override void Initialize(WaitCursorManagerFactory waitCursorManagerFactory, DbHandlerFactory dbHandlerFactory)
        {
            base.Initialize(waitCursorManagerFactory, dbHandlerFactory);

            this.mAppService = new(this.mDbHandlerFactory);
            this.mService = new(this.mDbHandlerFactory);

            this.FromBookSelectorVM.WaitCursorManagerFactory = waitCursorManagerFactory;
            this.ToBookSelectorVM.WaitCursorManagerFactory = waitCursorManagerFactory;
            this.CommissionKindSelectorVM.WaitCursorManagerFactory = waitCursorManagerFactory;
            this.ItemSelectorVM.WaitCursorManagerFactory = waitCursorManagerFactory;
            this.RemarkSelectorVM.WaitCursorManagerFactory = waitCursorManagerFactory;
        }

        public override async Task LoadAsync() => await this.LoadAsync(null, null, null, null);

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
            CommissionKind selectingCommissionKind = default;
            string selectingCommissionRemark = null;

            switch (this.RegKind) {
                case RegistrationKind.Add:
                    selectingFromBookId = initialBookId;
                    selectingToBookId = initialBookId;
                    selectingCommissionKind = CommissionKind.MoveFrom;

                    // WVMに値を設定する
                    this.IsLink = true;
                    this.SelectedFromDate = initialDate?.ToDateTime(TimeOnly.MinValue) ?? ((initialMonth == null || initialMonth?.Month == DateTime.Today.Month) ? DateTime.Today : initialMonth.Value.ToDateTime(TimeOnly.MinValue));

                    break;
                case RegistrationKind.Edit:
                case RegistrationKind.Copy: {
                    // DBから値を読み込む
                    ActionModel fromAction;
                    ActionModel toAction;
                    ActionModel commissionAction;
                    (fromAction, toAction, commissionAction) = await this.mService.LoadMoveActionsAsync(targetGroupId);

                    // WVMに値を設定する
                    if (this.RegKind == RegistrationKind.Edit) {
                        this.FromActionId = fromAction.ActionId;
                        this.ToActionId = toAction.ActionId;
                        this.GroupId = targetGroupId;
                        this.CommissionId = commissionAction?.ActionId;
                    }
                    selectingFromBookId = fromAction.Book.Id;
                    selectingToBookId = toAction.Book.Id;
                    selectingCommissionKind = commissionAction?.Book.Id == toAction.Book.Id ? CommissionKind.MoveTo : CommissionKind.MoveFrom;
                    selectingCommissionItemId = commissionAction?.Item.Id;
                    selectingCommissionRemark = commissionAction?.Remark;

                    this.IsLink = fromAction.ActTime == toAction.ActTime;
                    this.SelectedFromDate = fromAction.ActTime;
                    this.SelectedToDate = toAction.ActTime;
                    this.InputedValue = toAction.Amount;
                    this.InputedCommission = commissionAction?.Expenses;

                    break;
                }
            }

            // リストを更新する
            await this.FromBookSelectorVM.LoadAsync(selectingFromBookId);
            await this.ToBookSelectorVM.LoadAsync(selectingToBookId);
            await this.CommissionKindSelectorVM.LoadAsync(selectingCommissionKind);
            await this.ItemSelectorVM.LoadAsync(selectingCommissionItemId);
            await this.RemarkSelectorVM.LoadAsync(selectingCommissionRemark);

            switch (this.RegKind) {
                case RegistrationKind.Add: {
                    if (this.ToBookSelectorVM.SelectedItem?.BookKind == BookKind.CreditCard) {
                        if (this.ToBookSelectorVM.SelectedItem.DebitBookId != null) {
                            this.FromBookSelectorVM.SelectedItem = this.FromBookSelectorVM.ItemList.FirstOrElementAtOrDefault(vm => vm.Id == this.ToBookSelectorVM.SelectedItem.DebitBookId, 0);
                        }
                        if (this.ToBookSelectorVM.SelectedItem?.PayDay != null) {
                            this.SelectedFromDate = this.SelectedFromDate.GetDateInMonth(this.ToBookSelectorVM.SelectedItem.PayDay.Value);
                        }
                    }
                }
                break;
            }
        }

        public override void AddEventHandlers()
        {
            using FuncLog funcLog = new();

            this.FromBookSelectorVM.SelectionChanged += (sender, e) => this.FromBookChanged?.Invoke(sender, e);
            this.FromBookSelectorVM.Children.Add(this.ItemSelectorVM);
            this.ToBookSelectorVM.SelectionChanged += (sender, e) => this.ToBookChanged?.Invoke(sender, e);
            this.ToBookSelectorVM.Children.Add(this.ItemSelectorVM);
            this.CommissionKindSelectorVM.SelectionChanged += (sender, e) => this.CommissionKindChanged?.Invoke(sender, e);
            this.CommissionKindSelectorVM.Children.Add(this.ItemSelectorVM);
            this.ItemSelectorVM.SelectionChanged += (sender, e) => this.ItemChanged?.Invoke(sender, e);
            this.ItemSelectorVM.Children.Add(this.RemarkSelectorVM);
        }

        /// <summary>
        /// DBに登録する
        /// </summary>
        /// <returns>登録された帳簿項目IDリスト</returns>
        protected override async Task<IEnumerable<ActionIdObj>> SaveAsync()
        {
            using FuncLog funcLog = new();

            ActionModel fromAction = new() {
                Base = new(this.FromActionId, this.SelectedFromDate, (decimal)-this.InputedValue),
                GroupId = this.GroupId,
                Book = new(this.FromBookSelectorVM.SelectedKey, string.Empty)
            };

            ActionModel toAction = new() {
                Base = new(this.ToActionId, this.SelectedToDate, (decimal)this.InputedValue),
                GroupId = this.GroupId,
                Book = new(this.ToBookSelectorVM.SelectedKey, string.Empty)
            };

            CommissionKind commissionKind = this.CommissionKindSelectorVM.SelectedKey;
            ActionModel commissionAction = new() {
                Base = new(this.CommissionId, commissionKind switch {
                    CommissionKind.MoveFrom => fromAction.ActTime,
                    CommissionKind.MoveTo => toAction.ActTime,
                    _ => throw new NotSupportedException("SelectedComissionKind")
                }, -this.InputedCommission ?? 0),
                GroupId = this.GroupId,
                Book = new(commissionKind switch {
                    CommissionKind.MoveFrom => fromAction.Book.Id,
                    CommissionKind.MoveTo => toAction.Book.Id,
                    _ => throw new NotSupportedException("SelectedComissionKind")
                }, string.Empty),
                Item = new(this.ItemSelectorVM.SelectedKey, string.Empty),
                Remark = this.RemarkSelectorVM.SelectedKey
            };

            IEnumerable<ActionIdObj> resActionIdList = await this.mService.SaveMoveActionsAsync(fromAction, toAction, commissionAction);

            if (commissionAction.Amount != 0) {
                if (!string.IsNullOrEmpty(commissionAction.Remark)) {
                    RemarkModel remark = new(commissionAction.Remark) { ItemId = commissionAction.Item.Id, CurrentActTime = commissionAction.ActTime };
                    await this.mService.SaveRemarkAsync(remark);
                }
            }

            return resActionIdList;
        }
    }
}

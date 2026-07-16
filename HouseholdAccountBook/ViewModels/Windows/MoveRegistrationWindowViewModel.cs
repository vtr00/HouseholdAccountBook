using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities.Extensions;
using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.Args;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;
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
        public event EventHandler<ChangedEventArgs<AccountIdObj>> SelectedFromAccountChanged;
        /// <summary>
        /// 移動先帳簿変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<AccountIdObj>> SelectedToAccountChanged;
        /// <summary>
        /// 手数料種別変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<CommissionKind>> SelectedCommissionKindChanged;
        /// <summary>
        /// 手数料項目変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<ItemIdObj>> SelectedCommissionItemChanged;

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
        /// グループID
        /// </summary>
        public GroupIdObj GroupId {
            get;
            set => this.SetProperty(ref field, value);
        }

        #region 移動元帳簿項目
        /// <summary>
        /// 移動元帳簿項目ID
        /// </summary>
        public ActionIdObj FromActionId {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 移動元帳簿セレクタVM
        /// </summary>
        public SelectorViewModel<AccountModel, AccountIdObj> FromAccountSelectorVM => field ??= new(static vm => vm?.Id, this.mBusyService);

        /// <summary>
        /// 選択された日付(移動元)
        /// </summary>
        public DateTime SelectedFromDate {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    CommandManager.InvalidateRequerySuggested();

                    if (this.SelectedToDate < value || this.IsDateLink) {
                        this.SelectedToDate = value;
                    }
                }
            }
        } = DateTime.Today;

        /// <summary>
        /// 選択された移動元アセットID
        /// </summary>
        public AssetIdObj SelectedFromAssetId => this.SelectedFromAccountAssetId; //TODO: 帳簿項目のアセットIDがSystemでなければ帳簿のアセットIDを採用する
        /// <summary>
        /// 選択された移動元帳簿アセットID
        /// </summary>
        public AssetIdObj SelectedFromAccountAssetId {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    this.RaisePropertyChanged(nameof(this.IsSameAsset));
                    this.RaisePropertyChanged(nameof(this.FromValueScale));
                    this.RaisePropertyChanged(nameof(this.InputedFromValueStr));

                    if (this.IsSameAsset) {
                        this.InputedToValue = this.InputedFromValue;
                    }
                }
            }
        }
        /// <summary>
        /// 入力された移動元金額
        /// </summary>
        public decimal? InputedFromValue {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    CommandManager.InvalidateRequerySuggested();
                    this.RaisePropertyChanged(nameof(this.InputedFromValueStr));

                    if (this.IsSameAsset) {
                        this.InputedToValue = field;
                    }
                }
            }
        }
        /// <summary>
        /// 移動元金額の小数点以下桁数
        /// </summary>
        public int FromValueScale => AssetService.Instance.GetAssetModel(this.SelectedFromAssetId).Scale;
        /// <summary>
        /// 入力された移動元金額(文字列)
        /// </summary>
        public string InputedFromValueStr => AssetService.Instance.ToAssetString(this.InputedFromValue, this.SelectedFromAssetId, UnitKind.MainUnit, UnitKind.MainUnit);
        #endregion

        #region 移動先帳簿項目
        /// <summary>
        /// 移動先帳簿項目ID
        /// </summary>
        public ActionIdObj ToActionId {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 移動先帳簿セレクタVM
        /// </summary>
        public SelectorViewModel<AccountModel, AccountIdObj> ToAccountSelectorVM => field ??= new(static vm => vm?.Id, this.mBusyService);

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
                        this.IsDateLink = true;
                    }
                }
            }
        } = DateTime.Today;
        /// <summary>
        /// 移動先日時が移動元日時に連動して編集
        /// </summary>
        public bool IsDateLink {
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
        /// 選択された移動先アセットID
        /// </summary>
        public AssetIdObj SelectedToAssetId => this.SelectedToAccountAssetId; //TODO: 帳簿項目のアセットIDがSystemでなければ帳簿のアセットIDを採用する
        /// <summary>
        /// 選択された移動先帳簿アセットID
        /// </summary>
        public AssetIdObj SelectedToAccountAssetId {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    this.RaisePropertyChanged(nameof(this.IsSameAsset));
                    this.RaisePropertyChanged(nameof(this.ToValueScale));
                    this.RaisePropertyChanged(nameof(this.InputedToValueStr));

                    if (this.IsSameAsset) {
                        this.InputedToValue = this.InputedFromValue;
                    }
                }
            }
        }

        public bool IsSameAsset => this.SelectedFromAssetId == this.SelectedToAssetId;
        /// <summary>
        /// 入力された移動先金額
        /// </summary>
        public decimal? InputedToValue {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    CommandManager.InvalidateRequerySuggested();
                    this.RaisePropertyChanged(nameof(this.InputedToValueStr));
                }
            }
        }
        /// <summary>
        /// 移動先金額の小数点以下桁数
        /// </summary>
        public int ToValueScale => AssetService.Instance.GetAssetModel(this.SelectedToAssetId).Scale;
        /// <summary>
        /// 入力された移動先金額(文字列)
        /// </summary>
        public string InputedToValueStr => AssetService.Instance.ToAssetString(this.InputedToValue, this.SelectedToAssetId, UnitKind.MainUnit, UnitKind.MainUnit);
        #endregion

        #region 手数料帳簿項目
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
        public SelectorViewModel<KeyValuePair<CommissionKind, string>, CommissionKind> CommissionKindSelectorVM => field ??= new(static p => p.Key);

        /// <summary>
        /// 手数料項目セレクタVM
        /// </summary>
        public SelectorViewModel<ItemModel, ItemIdObj> ItemSelectorVM => field ??= new(static vm => vm?.Id, this.mBusyService);

        /// <summary>
        /// 選択された手数料アセットID
        /// </summary>
        public AssetIdObj SelectedCommissionAssetId => this.SelectedCommissionAccountAssetId; //TODO: 帳簿項目のアセットIDがSystemでなければ帳簿のアセットIDを採用する
        /// <summary>
        /// 選択された手数料帳簿アセットID
        /// </summary>
        public AssetIdObj SelectedCommissionAccountAssetId {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    this.RaisePropertyChanged(nameof(this.CommissionScale));
                    this.RaisePropertyChanged(nameof(this.InputedCommissionStr));
                }
            }
        }
        /// <summary>
        /// 入力された手数料
        /// </summary>
        public decimal? InputedCommission {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    CommandManager.InvalidateRequerySuggested();
                    this.RaisePropertyChanged(nameof(this.InputedCommissionStr));
                }
            }
        }
        /// <summary>
        /// 手数料の小数点以下桁数
        /// </summary>
        public int CommissionScale => AssetService.Instance.GetAssetModel(this.SelectedCommissionAssetId).Scale;
        /// <summary>
        /// 入力された手数料(文字列)
        /// </summary>
        public string InputedCommissionStr => AssetService.Instance.ToAssetString(this.InputedCommission, this.SelectedCommissionAssetId, UnitKind.MainUnit, UnitKind.MainUnit);

        /// <summary>
        /// 備考セレクタVM
        /// </summary>
        public SelectorViewModel<RemarkModel, string> RemarkSelectorVM => field ??= new(static vm => vm?.Remark, this.mBusyService);
        #endregion
        #endregion

        #region コマンド
        /// <summary>
        /// 今日コマンド
        /// </summary>
        public ICommand TodayCommand => field ??= new RelayCommand(() => this.SelectedFromDate = DateTime.Today, () => this.SelectedFromDate != DateTime.Today);

        /// <summary>
        /// OKコマンド
        /// </summary>
        public new ICommand OKCommand => field ??= new AsyncRelayCommand(
            this.OKCommand_ExecuteAsync,
            () => this.InputedFromValue is not null && this.FromAccountSelectorVM?.SelectedKey != this.ToAccountSelectorVM?.SelectedKey, this.mBusyService);
        protected async Task OKCommand_ExecuteAsync()
        {
            // DB登録
            IEnumerable<ActionIdObj> idList = await this.SaveAsync();
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

        public override void Initialize(DbHandlerFactory dbHandlerFactory)
        {
            base.Initialize(dbHandlerFactory);

            this.mAppService = new(this.mDbHandlerFactory);
            this.mService = new(this.mDbHandlerFactory);

            this.FromAccountSelectorVM.SetLoader(async () => await this.mAppService.LoadAccountListAsync());
            this.ToAccountSelectorVM.SetLoader(async () => await this.mAppService.LoadAccountListAsync());
            this.CommissionKindSelectorVM.SetLoader(() => CommissionKindStr);
            this.ItemSelectorVM.SetLoader(
                async () => {
                    AccountIdObj accountId = this.CommissionKindSelectorVM.SelectedKey switch {
                        CommissionKind.MoveFrom => this.FromAccountSelectorVM.SelectedKey,
                        CommissionKind.MoveTo => this.ToAccountSelectorVM.SelectedKey,
                        _ => throw new NotSupportedException("SelectedCommissionKind"),
                    };
                    return await this.mAppService.LoadItemListAsync(accountId, BalanceKind.Expenses, CategoryIdObj.System);
                },
                () => this.FromAccountSelectorVM.SelectedKey != null && this.ToAccountSelectorVM.SelectedKey != null);
            this.RemarkSelectorVM.SetLoader(
                async () => await this.mAppService.LoadRemarkListAsync(this.ItemSelectorVM.SelectedKey, true),
                () => this.ItemSelectorVM.SelectedKey != null, SelectorMode.Force);
        }

        public override async Task LoadAsync() => await this.LoadAsync(null, null, null, null);

        /// <summary>
        /// DBから読み込む
        /// </summary>
        /// <param name="initialAccountId">追加時、初期選択する帳簿のID</param>
        /// <param name="initialMonth">追加時、初期選択する年月</param>
        /// <param name="initialDate">追加時、初期選択する日付</param>
        /// <param name="targetGroupId">複製/編集時、複製/編集対象の帳簿項目のグループID</param>
        /// <returns></returns>
        public async Task LoadAsync(AccountIdObj initialAccountId, DateOnly? initialMonth, DateOnly? initialDate, GroupIdObj targetGroupId)
        {
            using FuncLog funcLog = new(new { initialAccountId, initialMonth, initialDate, targetGroupId });
            using IDisposable disposable = this.mBusyService.Enter();

            AccountIdObj selectingFromAccountId = null;
            AccountIdObj selectingToAccountId = null;
            ItemIdObj selectingCommissionItemId = null;
            CommissionKind selectingCommissionKind = default;
            string selectingCommissionRemark = null;

            switch (this.RegKind) {
                case RegistrationKind.Add: {
                    selectingFromAccountId = initialAccountId;
                    selectingToAccountId = initialAccountId;
                    selectingCommissionKind = CommissionKind.MoveFrom;

                    // WVMに値を設定する
                    this.IsDateLink = true;
                    this.SelectedFromDate = initialDate?.ToDateTime(TimeOnly.MinValue) ?? ((initialMonth == null || initialMonth?.Month == DateTime.Today.Month) ? DateTime.Today : initialMonth.Value.ToDateTime(TimeOnly.MinValue));

                    break;
                }
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
                    selectingFromAccountId = fromAction.Account.Id;
                    selectingToAccountId = toAction.Account.Id;
                    selectingCommissionKind = commissionAction?.Account.Id == toAction.Account.Id ? CommissionKind.MoveTo : CommissionKind.MoveFrom;
                    selectingCommissionItemId = commissionAction?.Item.Id;
                    selectingCommissionRemark = commissionAction?.Remark?.Remark;

                    this.IsDateLink = fromAction.ActTime == toAction.ActTime;

                    this.SelectedFromDate = fromAction.ActTime;
                    this.InputedFromValue = fromAction.Expenses?.MainValue; // 移動元帳簿の支出

                    this.SelectedToDate = toAction.ActTime;
                    this.InputedToValue = toAction.Income?.MainValue; // 移動先帳簿の収入

                    this.InputedCommission = commissionAction?.Expenses?.MainValue;

                    break;
                }
            }

            // リストを更新する
            await this.FromAccountSelectorVM.LoadAsync(selectingFromAccountId);
            await this.ToAccountSelectorVM.LoadAsync(selectingToAccountId);
            await this.CommissionKindSelectorVM.LoadAsync(selectingCommissionKind);
            await this.ItemSelectorVM.LoadAsync(selectingCommissionItemId);
            await this.RemarkSelectorVM.LoadAsync(selectingCommissionRemark);

            switch (this.RegKind) {
                case RegistrationKind.Add: {
                    if (this.ToAccountSelectorVM.SelectedItem?.AccountKind == AccountKind.CreditCard) {
                        if (this.ToAccountSelectorVM.SelectedItem.DebitAccountId != null) {
                            this.FromAccountSelectorVM.SelectedItem = this.FromAccountSelectorVM.ItemList.FirstOrElementAtOrDefault(vm => vm.Id == this.ToAccountSelectorVM.SelectedItem.DebitAccountId, 0);
                        }
                        if (this.ToAccountSelectorVM.SelectedItem?.PayDay != null) {
                            this.SelectedFromDate = this.SelectedFromDate.GetDateInMonth(this.ToAccountSelectorVM.SelectedItem.PayDay.Value);
                        }
                    }
                }
                break;
            }

            // アセットIDを指定する
            this.SelectedFromAccountAssetId = this.FromAccountSelectorVM.SelectedItem?.AssetId ?? AssetIdObj.System;
            this.SelectedToAccountAssetId = this.ToAccountSelectorVM.SelectedItem?.AssetId ?? AssetIdObj.System;
            this.SelectedCommissionAccountAssetId = this.CommissionKindSelectorVM.SelectedKey switch {
                CommissionKind.MoveFrom => this.SelectedFromAccountAssetId,
                CommissionKind.MoveTo => this.SelectedToAccountAssetId,
                _ => throw new NotSupportedException("SelectedCommissionKind")
            } ?? AssetIdObj.System;
        }

        public override void AddEventHandlers()
        {
            using FuncLog funcLog = new();

            // 移動元帳簿変更時
            this.FromAccountSelectorVM.SelectionChanged += (sender, e) => {
                this.SelectedFromAccountAssetId = this.FromAccountSelectorVM.SelectedItem?.AssetId ?? AssetIdObj.System;

                if (this.CommissionKindSelectorVM.SelectedKey == CommissionKind.MoveFrom) {
                    this.SelectedCommissionAccountAssetId = this.FromAccountSelectorVM.SelectedItem?.AssetId;
                }

                this.SelectedFromAccountChanged?.Invoke(sender, e);
            };
            this.FromAccountSelectorVM.Children.Add(this.ItemSelectorVM);

            // 移動先帳簿変更時
            this.ToAccountSelectorVM.SelectionChanged += (sender, e) => {
                this.SelectedToAccountAssetId = this.ToAccountSelectorVM.SelectedItem?.AssetId ?? AssetIdObj.System;

                if (this.CommissionKindSelectorVM.SelectedKey == CommissionKind.MoveTo) {
                    this.SelectedCommissionAccountAssetId = this.ToAccountSelectorVM.SelectedItem?.AssetId;
                }

                this.SelectedToAccountChanged?.Invoke(sender, e);
            };
            this.ToAccountSelectorVM.Children.Add(this.ItemSelectorVM);

            // 手数料種別変更時
            this.CommissionKindSelectorVM.SelectionChanged += (sender, e) => {
                this.SelectedCommissionAccountAssetId = e.NewValue switch {
                    CommissionKind.MoveFrom => this.SelectedFromAccountAssetId,
                    CommissionKind.MoveTo => this.SelectedToAccountAssetId,
                    _ => throw new NotSupportedException("SelectedCommissionKind")
                } ?? AssetIdObj.System;

                this.SelectedCommissionKindChanged?.Invoke(sender, e);
            };
            this.CommissionKindSelectorVM.Children.Add(this.ItemSelectorVM);

            // 項目変更時
            this.ItemSelectorVM.SelectionChanged += (sender, e) => this.SelectedCommissionItemChanged?.Invoke(sender, e);
            this.ItemSelectorVM.Children.Add(this.RemarkSelectorVM);
        }

        /// <summary>
        /// DBに登録する
        /// </summary>
        /// <returns>登録された帳簿項目IDリスト</returns>
        protected override async Task<IEnumerable<ActionIdObj>> SaveAsync()
        {
            using FuncLog funcLog = new();

            // 移動元
            ActionModel fromAction = new() {
                Base = new(this.FromActionId, this.SelectedFromDate, new(-this.InputedFromValue.Value, this.SelectedFromAssetId)),
                AssetId = AssetIdObj.System, // TODO: 将来の拡張用(証券口座間の株の移動など)
                GroupId = this.GroupId,
                Account = new(this.FromAccountSelectorVM.SelectedKey, string.Empty)
            };

            // 移動先
            ActionModel toAction = new() {
                Base = new(this.ToActionId, this.SelectedToDate, new(this.InputedToValue.Value, this.SelectedToAssetId)),
                AssetId = AssetIdObj.System, // TODO: 将来の拡張用(証券口座間の株の移動など)
                GroupId = this.GroupId,
                Account = new(this.ToAccountSelectorVM.SelectedKey, string.Empty)
            };

            // 手数料
            CommissionKind commissionKind = this.CommissionKindSelectorVM.SelectedKey;
            DateTime commissionActTime = commissionKind switch {
                CommissionKind.MoveFrom => fromAction.ActTime,
                CommissionKind.MoveTo => toAction.ActTime,
                _ => throw new NotSupportedException("SelectedCommissionKind")
            };
            SelectorViewModel<AccountModel, AccountIdObj> commissionSelectorVM = commissionKind switch {
                CommissionKind.MoveFrom => this.FromAccountSelectorVM,
                CommissionKind.MoveTo => this.ToAccountSelectorVM,
                _ => throw new NotSupportedException("SelectedCommissionKind")
            };
            ActionModel commissionAction = new() {
                Base = new(this.CommissionId, commissionActTime, new(-this.InputedCommission ?? 0m, commissionSelectorVM.SelectedItem.AssetId)),
                AssetId = AssetIdObj.System, // 今のところ固定を想定
                GroupId = this.GroupId,
                Account = new(commissionKind switch {
                    CommissionKind.MoveFrom => fromAction.Account.Id,
                    CommissionKind.MoveTo => toAction.Account.Id,
                    _ => throw new NotSupportedException("SelectedCommissionKind")
                }, string.Empty),
                Item = new(this.ItemSelectorVM.SelectedKey, string.Empty),
                Remark = this.RemarkSelectorVM.SelectedKey
            };

            IEnumerable<ActionIdObj> resActionIdList = await this.mService.SaveMoveActionsAsync(fromAction, toAction, commissionAction);

            if (commissionAction.Amount.MainValue != 0m) {
                if (!string.IsNullOrEmpty(commissionAction.Remark)) {
                    RemarkModel remark = new(commissionAction.Remark) { ItemId = commissionAction.Item.Id, CurrentActTime = commissionAction.ActTime };
                    await this.mService.SaveRemarkAsync(remark);
                }
            }

            return resActionIdList;
        }
    }
}

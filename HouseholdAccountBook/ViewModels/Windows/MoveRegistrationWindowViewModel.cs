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
        public event EventHandler<ChangedEventArgs<AccountIdObj>> SelectedSrcAccountChanged;
        /// <summary>
        /// 移動先帳簿変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<AccountIdObj>> SelectedDstAccountChanged;
        /// <summary>
        /// 手数料種別変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<FeeKind>> SelectedFeeKindChanged;
        /// <summary>
        /// 手数料項目変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<ItemIdObj>> SelectedFeeItemChanged;

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
        public ActionIdObj SrcActionId {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 移動元帳簿セレクタVM
        /// </summary>
        public SelectorViewModel<AccountModel, AccountIdObj> SrcAccountSelectorVM => field ??= new(static vm => vm?.Id, this.mBusyService);

        /// <summary>
        /// 選択された日付(移動元)
        /// </summary>
        public DateTime SelectedSrcDate {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    CommandManager.InvalidateRequerySuggested();

                    if (this.SelectedDstDate < value || this.IsDateLink) {
                        this.SelectedDstDate = value;
                    }
                }
            }
        } = DateTime.Today;

        /// <summary>
        /// 選択された移動元アセットID
        /// </summary>
        public AssetIdObj SelectedSrcAssetId => this.SelectedSrcAccountAssetId; //TODO: 帳簿項目のアセットIDがSystemでなければ帳簿のアセットIDを採用する
        /// <summary>
        /// 選択された移動元帳簿アセットID
        /// </summary>
        public AssetIdObj SelectedSrcAccountAssetId {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    this.RaisePropertyChanged(nameof(this.IsSameAsset));
                    this.RaisePropertyChanged(nameof(this.SrcValueScale));
                    this.RaisePropertyChanged(nameof(this.InputedSrcValueStr));

                    if (this.IsSameAsset) {
                        this.InputedDstValue = this.InputedSrcValue;
                    }
                }
            }
        }
        /// <summary>
        /// 入力された移動元金額
        /// </summary>
        public decimal? InputedSrcValue {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    CommandManager.InvalidateRequerySuggested();
                    this.RaisePropertyChanged(nameof(this.InputedSrcValueStr));

                    if (this.IsSameAsset) {
                        this.InputedDstValue = field;
                    }
                }
            }
        }
        /// <summary>
        /// 移動元金額の小数点以下桁数
        /// </summary>
        public int SrcValueScale => AssetService.Instance.GetAssetModel(this.SelectedSrcAssetId).Scale;
        /// <summary>
        /// 入力された移動元金額(文字列)
        /// </summary>
        public string InputedSrcValueStr => AssetService.Instance.ToAssetString(this.InputedSrcValue, this.SelectedSrcAssetId, UnitKind.MainUnit, UnitKind.MainUnit);
        #endregion

        #region 移動先帳簿項目
        /// <summary>
        /// 移動先帳簿項目ID
        /// </summary>
        public ActionIdObj DstActionId {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 移動先帳簿セレクタVM
        /// </summary>
        public SelectorViewModel<AccountModel, AccountIdObj> DstAccountSelectorVM => field ??= new(static vm => vm?.Id, this.mBusyService);

        /// <summary>
        /// 選択された日付(移動先)
        /// </summary>
        public DateTime SelectedDstDate {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    CommandManager.InvalidateRequerySuggested();

                    if (value < this.SelectedSrcDate) {
                        this.SelectedSrcDate = value;
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
                        this.SelectedDstDate = this.SelectedSrcDate;
                    }
                }
            }
        } = true;

        /// <summary>
        /// 選択された移動先アセットID
        /// </summary>
        public AssetIdObj SelectedDstAssetId => this.SelectedDstAccountAssetId; //TODO: 帳簿項目のアセットIDがSystemでなければ帳簿のアセットIDを採用する
        /// <summary>
        /// 選択された移動先帳簿アセットID
        /// </summary>
        public AssetIdObj SelectedDstAccountAssetId {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    this.RaisePropertyChanged(nameof(this.IsSameAsset));
                    this.RaisePropertyChanged(nameof(this.DstValueScale));
                    this.RaisePropertyChanged(nameof(this.InputedDstValueStr));

                    if (this.IsSameAsset) {
                        this.InputedDstValue = this.InputedSrcValue;
                    }
                }
            }
        }

        public bool IsSameAsset => this.SelectedSrcAssetId == this.SelectedDstAssetId;
        /// <summary>
        /// 入力された移動先金額
        /// </summary>
        public decimal? InputedDstValue {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    CommandManager.InvalidateRequerySuggested();
                    this.RaisePropertyChanged(nameof(this.InputedDstValueStr));
                }
            }
        }
        /// <summary>
        /// 移動先金額の小数点以下桁数
        /// </summary>
        public int DstValueScale => AssetService.Instance.GetAssetModel(this.SelectedDstAssetId).Scale;
        /// <summary>
        /// 入力された移動先金額(文字列)
        /// </summary>
        public string InputedDstValueStr => AssetService.Instance.ToAssetString(this.InputedDstValue, this.SelectedDstAssetId, UnitKind.MainUnit, UnitKind.MainUnit);
        #endregion

        #region 手数料帳簿項目
        /// <summary>
        /// 手数料の帳簿項目ID
        /// </summary>
        public ActionIdObj FeeActionId {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 手数料種別セレクタVM
        /// </summary>
        public SelectorViewModel<KeyValuePair<FeeKind, string>, FeeKind> FeeKindSelectorVM => field ??= new(static p => p.Key);

        /// <summary>
        /// 手数料項目セレクタVM
        /// </summary>
        public SelectorViewModel<ItemModel, ItemIdObj> ItemSelectorVM => field ??= new(static vm => vm?.Id, this.mBusyService);

        /// <summary>
        /// 選択された手数料アセットID
        /// </summary>
        public AssetIdObj SelectedFeeAssetId => this.SelectedFeeAccountAssetId; //TODO: 帳簿項目のアセットIDがSystemでなければ帳簿のアセットIDを採用する
        /// <summary>
        /// 選択された手数料帳簿アセットID
        /// </summary>
        public AssetIdObj SelectedFeeAccountAssetId {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    this.RaisePropertyChanged(nameof(this.FeeScale));
                    this.RaisePropertyChanged(nameof(this.InputedFeeStr));
                }
            }
        }
        /// <summary>
        /// 入力された手数料
        /// </summary>
        public decimal? InputedFee {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    CommandManager.InvalidateRequerySuggested();
                    this.RaisePropertyChanged(nameof(this.InputedFeeStr));
                }
            }
        }
        /// <summary>
        /// 手数料の小数点以下桁数
        /// </summary>
        public int FeeScale => AssetService.Instance.GetAssetModel(this.SelectedFeeAssetId).Scale;
        /// <summary>
        /// 入力された手数料(文字列)
        /// </summary>
        public string InputedFeeStr => AssetService.Instance.ToAssetString(this.InputedFee, this.SelectedFeeAssetId, UnitKind.MainUnit, UnitKind.MainUnit);

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
        public ICommand TodayCommand => field ??= new RelayCommand(() => this.SelectedSrcDate = DateTime.Today, () => this.SelectedSrcDate != DateTime.Today);

        /// <summary>
        /// OKコマンド
        /// </summary>
        public new ICommand OKCommand => field ??= new AsyncRelayCommand(
            this.OKCommand_ExecuteAsync,
            () => this.InputedSrcValue is not null && this.SrcAccountSelectorVM?.SelectedKey != this.DstAccountSelectorVM?.SelectedKey, this.mBusyService);
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

            this.SrcAccountSelectorVM.SetLoader(async () => await this.mAppService.LoadAccountListAsync());
            this.DstAccountSelectorVM.SetLoader(async () => await this.mAppService.LoadAccountListAsync());
            this.FeeKindSelectorVM.SetLoader(() => FeeKindStr);
            this.ItemSelectorVM.SetLoader(
                async () => {
                    AccountIdObj accountId = this.FeeKindSelectorVM.SelectedKey switch {
                        FeeKind.Source => this.SrcAccountSelectorVM.SelectedKey,
                        FeeKind.Destination => this.DstAccountSelectorVM.SelectedKey,
                        _ => throw new NotSupportedException("SelectedFeeKind"),
                    };
                    return await this.mAppService.LoadItemListAsync(accountId, BalanceKind.Expenses, CategoryIdObj.System);
                },
                () => this.SrcAccountSelectorVM.SelectedKey != null && this.DstAccountSelectorVM.SelectedKey != null);
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

            AccountIdObj selectingSrcAccountId = null;
            AccountIdObj selectingDstAccountId = null;
            ItemIdObj selectingFeeItemId = null;
            FeeKind selectingFeeKind = default;
            string selectingFeeRemark = null;

            switch (this.RegKind) {
                case RegistrationKind.Add: {
                    selectingSrcAccountId = initialAccountId;
                    selectingDstAccountId = initialAccountId;
                    selectingFeeKind = FeeKind.Source;

                    // WVMに値を設定する
                    this.IsDateLink = true;
                    this.SelectedSrcDate = initialDate?.ToDateTime(TimeOnly.MinValue) ?? ((initialMonth == null || initialMonth?.Month == DateTime.Today.Month) ? DateTime.Today : initialMonth.Value.ToDateTime(TimeOnly.MinValue));

                    break;
                }
                case RegistrationKind.Edit:
                case RegistrationKind.Copy: {
                    // DBから値を読み込む
                    ActionModel srcAction;
                    ActionModel dstAction;
                    ActionModel feeAction;
                    (srcAction, dstAction, feeAction) = await this.mService.LoadMoveActionsAsync(targetGroupId);

                    // WVMに値を設定する
                    if (this.RegKind == RegistrationKind.Edit) {
                        this.SrcActionId = srcAction.ActionId;
                        this.DstActionId = dstAction.ActionId;
                        this.GroupId = targetGroupId;
                        this.FeeActionId = feeAction?.ActionId;
                    }
                    selectingSrcAccountId = srcAction.Account.Id;
                    selectingDstAccountId = dstAction.Account.Id;
                    selectingFeeKind = feeAction?.Account.Id == dstAction.Account.Id ? FeeKind.Destination : FeeKind.Source;
                    selectingFeeItemId = feeAction?.Item.Id;
                    selectingFeeRemark = feeAction?.Remark?.Remark;

                    this.IsDateLink = srcAction.ActTime == dstAction.ActTime;

                    this.SelectedSrcDate = srcAction.ActTime;
                    this.InputedSrcValue = srcAction.Expenses?.MainValue; // 移動元帳簿の支出

                    this.SelectedDstDate = dstAction.ActTime;
                    this.InputedDstValue = dstAction.Income?.MainValue; // 移動先帳簿の収入

                    this.InputedFee = feeAction?.Expenses?.MainValue;

                    break;
                }
            }

            // リストを更新する
            await this.SrcAccountSelectorVM.LoadAsync(selectingSrcAccountId);
            await this.DstAccountSelectorVM.LoadAsync(selectingDstAccountId);
            await this.FeeKindSelectorVM.LoadAsync(selectingFeeKind);
            await this.ItemSelectorVM.LoadAsync(selectingFeeItemId);
            await this.RemarkSelectorVM.LoadAsync(selectingFeeRemark);

            switch (this.RegKind) {
                case RegistrationKind.Add: {
                    if (this.DstAccountSelectorVM.SelectedItem?.AccountKind == AccountKind.CreditCard) {
                        if (this.DstAccountSelectorVM.SelectedItem.DebitAccountId != null) {
                            this.SrcAccountSelectorVM.SelectedItem = this.SrcAccountSelectorVM.ItemList.FirstOrElementAtOrDefault(vm => vm.Id == this.DstAccountSelectorVM.SelectedItem.DebitAccountId, 0);
                        }
                        if (this.DstAccountSelectorVM.SelectedItem?.PayDay != null) {
                            this.SelectedSrcDate = this.SelectedSrcDate.GetDateInMonth(this.DstAccountSelectorVM.SelectedItem.PayDay.Value);
                        }
                    }
                }
                break;
            }

            // アセットIDを指定する
            this.SelectedSrcAccountAssetId = this.SrcAccountSelectorVM.SelectedItem?.AssetId ?? AssetIdObj.System;
            this.SelectedDstAccountAssetId = this.DstAccountSelectorVM.SelectedItem?.AssetId ?? AssetIdObj.System;
            this.SelectedFeeAccountAssetId = this.FeeKindSelectorVM.SelectedKey switch {
                FeeKind.Source => this.SelectedSrcAccountAssetId,
                FeeKind.Destination => this.SelectedDstAccountAssetId,
                _ => throw new NotSupportedException("SelectedFeeKind")
            } ?? AssetIdObj.System;
        }

        public override void AddEventHandlers()
        {
            using FuncLog funcLog = new();

            // 移動元帳簿変更時
            this.SrcAccountSelectorVM.SelectionChanged += (sender, e) => {
                this.SelectedSrcAccountAssetId = this.SrcAccountSelectorVM.SelectedItem?.AssetId ?? AssetIdObj.System;

                if (this.FeeKindSelectorVM.SelectedKey == FeeKind.Source) {
                    this.SelectedFeeAccountAssetId = this.SrcAccountSelectorVM.SelectedItem?.AssetId;
                }

                this.SelectedSrcAccountChanged?.Invoke(sender, e);
            };
            this.SrcAccountSelectorVM.Children.Add(this.ItemSelectorVM);

            // 移動先帳簿変更時
            this.DstAccountSelectorVM.SelectionChanged += (sender, e) => {
                this.SelectedDstAccountAssetId = this.DstAccountSelectorVM.SelectedItem?.AssetId ?? AssetIdObj.System;

                if (this.FeeKindSelectorVM.SelectedKey == FeeKind.Destination) {
                    this.SelectedFeeAccountAssetId = this.DstAccountSelectorVM.SelectedItem?.AssetId;
                }

                this.SelectedDstAccountChanged?.Invoke(sender, e);
            };
            this.DstAccountSelectorVM.Children.Add(this.ItemSelectorVM);

            // 手数料種別変更時
            this.FeeKindSelectorVM.SelectionChanged += (sender, e) => {
                this.SelectedFeeAccountAssetId = e.NewValue switch {
                    FeeKind.Source => this.SelectedSrcAccountAssetId,
                    FeeKind.Destination => this.SelectedDstAccountAssetId,
                    _ => throw new NotSupportedException("SelectedFeeKind")
                } ?? AssetIdObj.System;

                this.SelectedFeeKindChanged?.Invoke(sender, e);
            };
            this.FeeKindSelectorVM.Children.Add(this.ItemSelectorVM);

            // 項目変更時
            this.ItemSelectorVM.SelectionChanged += (sender, e) => this.SelectedFeeItemChanged?.Invoke(sender, e);
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
            ActionModel srcAction = new() {
                Base = new(this.SrcActionId, this.SelectedSrcDate, new(-this.InputedSrcValue.Value, this.SelectedSrcAssetId)),
                AssetId = AssetIdObj.System, // TODO: 将来の拡張用(証券口座間の株の移動など)
                GroupId = this.GroupId,
                Account = new(this.SrcAccountSelectorVM.SelectedKey, string.Empty)
            };

            // 移動先
            ActionModel dstAction = new() {
                Base = new(this.DstActionId, this.SelectedDstDate, new(this.InputedDstValue.Value, this.SelectedDstAssetId)),
                AssetId = AssetIdObj.System, // TODO: 将来の拡張用(証券口座間の株の移動など)
                GroupId = this.GroupId,
                Account = new(this.DstAccountSelectorVM.SelectedKey, string.Empty)
            };

            // 手数料
            FeeKind feeKind = this.FeeKindSelectorVM.SelectedKey;
            DateTime feeActTime = feeKind switch {
                FeeKind.Source => srcAction.ActTime,
                FeeKind.Destination => dstAction.ActTime,
                _ => throw new NotSupportedException("SelectedFeeKind")
            };
            SelectorViewModel<AccountModel, AccountIdObj> feeSelectorVM = feeKind switch {
                FeeKind.Source => this.SrcAccountSelectorVM,
                FeeKind.Destination => this.DstAccountSelectorVM,
                _ => throw new NotSupportedException("SelectedFeeKind")
            };
            ActionModel feeAction = new() {
                Base = new(this.FeeActionId, feeActTime, new(-this.InputedFee ?? 0m, feeSelectorVM.SelectedItem.AssetId)),
                AssetId = AssetIdObj.System, // 今のところ固定を想定
                GroupId = this.GroupId,
                Account = new(feeKind switch {
                    FeeKind.Source => srcAction.Account.Id,
                    FeeKind.Destination => dstAction.Account.Id,
                    _ => throw new NotSupportedException("SelectedFeeKind")
                }, string.Empty),
                Item = new(this.ItemSelectorVM.SelectedKey, string.Empty),
                Remark = this.RemarkSelectorVM.SelectedKey
            };

            IEnumerable<ActionIdObj> resActionIdList = await this.mService.SaveMoveActionsAsync(srcAction, dstAction, feeAction);

            if (feeAction.Amount.MainValue != 0m) {
                if (!string.IsNullOrEmpty(feeAction.Remark)) {
                    RemarkModel remark = new(feeAction.Remark) { ItemId = feeAction.Item.Id, CurrentActTime = feeAction.ActTime };
                    await this.mService.SaveRemarkAsync(remark);
                }
            }

            return resActionIdList;
        }
    }
}

using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.Args;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static HouseholdAccountBook.ViewModels.UiConstants;

namespace HouseholdAccountBook.ViewModels.Windows
{
    /// <summary>
    /// 帳簿項目登録ウィンドウ(交換)VM
    /// </summary>
    public class ExchangeRegistrationWindowViewModel : WindowViewModelBase
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
        /// 帳簿変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<AccountIdObj>> SelectedAccountChanged;
        /// <summary>
        /// 変換元項目変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<ItemIdObj>> SelectedSrcItemChanged;
        /// <summary>
        /// 変換先項目変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<ItemIdObj>> SelectedDstItemChanged;
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

        /// <summary>
        /// 帳簿リストVM
        /// </summary>
        public SelectableViewModel<AccountModel, AccountIdObj> AccountListVM => field ??= new(static vm => vm?.Id, this.mBusyService);

        #region 変換元帳簿項目
        /// <summary>
        /// 変換元帳簿項目ID
        /// </summary>
        public ActionIdObj SrcActionId {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 選択された日付(変換元)
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
        /// 変換元項目リストVM
        /// </summary>
        public SelectableViewModel<ItemModel, ItemIdObj> SrcItemListVM => field ??= new(static vm => vm?.Id, this.mBusyService);

        /// <summary>
        /// 変換元アセットリストVM
        /// </summary>
        public SelectableViewModel<AssetModel, AssetIdObj> SrcAssetListVM => field ??= new(static vm => vm?.Id, this.mBusyService);
        private void RaiseSrcAssetChanged()
        {
            this.RaisePropertyChanged(nameof(this.SrcValueScale));
            this.RaisePropertyChanged(nameof(this.InputedSrcValueStr));
        }
        /// <summary>
        /// 入力された変換元金額
        /// </summary>
        public decimal? InputedSrcValue {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    CommandManager.InvalidateRequerySuggested();
                    this.RaisePropertyChanged(nameof(this.InputedSrcValueStr));
                }
            }
        }
        /// <summary>
        /// 変換元金額の小数点以下桁数
        /// </summary>
        public int SrcValueScale => AssetService.Instance.GetAssetModel(this.SrcAssetListVM.SelectedKey).Scale;
        /// <summary>
        /// 入力された変換元金額(文字列)
        /// </summary>
        public string InputedSrcValueStr => AssetService.Instance.ToAssetString(this.InputedSrcValue, this.SrcAssetListVM.SelectedKey, UnitKind.MainUnit, UnitKind.MainUnit);
        #endregion

        #region 変換先帳簿項目
        /// <summary>
        /// 変換先帳簿項目ID
        /// </summary>
        public ActionIdObj DstActionId {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 選択された日付(変換先)
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
        /// 変換先日時が変換元日時に連動して編集
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
        /// 変換先項目リストVM
        /// </summary>
        public SelectableViewModel<ItemModel, ItemIdObj> DstItemListVM => field ??= new(static vm => vm?.Id, this.mBusyService);

        /// <summary>
        /// 変換元アセットリストVM
        /// </summary>
        public SelectableViewModel<AssetModel, AssetIdObj> DstAssetListVM => field ??= new(static vm => vm?.Id, this.mBusyService);
        private void RaiseDstAssetChanged()
        {
            this.RaisePropertyChanged(nameof(this.DstValueScale));
            this.RaisePropertyChanged(nameof(this.InputedDstValueStr));
        }
        /// <summary>
        /// 入力された変換先金額
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
        /// 変換先金額の小数点以下桁数
        /// </summary>
        public int DstValueScale => AssetService.Instance.GetAssetModel(this.DstAssetListVM.SelectedKey).Scale;
        /// <summary>
        /// 入力された変換先金額(文字列)
        /// </summary>
        public string InputedDstValueStr => AssetService.Instance.ToAssetString(this.InputedDstValue, this.DstAssetListVM.SelectedKey, UnitKind.MainUnit, UnitKind.MainUnit);
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
        /// 手数料種別リストVM
        /// </summary>
        public SelectableViewModel<KeyValuePair<FeeKind, string>, FeeKind> FeeKindListVM => field ??= new(static p => p.Key);

        /// <summary>
        /// 手数料項目リストVM
        /// </summary>
        public SelectableViewModel<ItemModel, ItemIdObj> FeeItemListVM => field ??= new(static vm => vm?.Id, this.mBusyService);

        /// <summary>
        /// 変換元アセットリストVM
        /// </summary>
        public SelectableViewModel<AssetModel, AssetIdObj> FeeAssetListVM => field ??= new(static vm => vm?.Id, this.mBusyService);
        private void RaiseFeeAssetChanged()
        {
            this.RaisePropertyChanged(nameof(this.FeeScale));
            this.RaisePropertyChanged(nameof(this.InputedFeeStr));
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
        public int FeeScale => AssetService.Instance.GetAssetModel(this.FeeAssetListVM.SelectedKey).Scale;
        /// <summary>
        /// 入力された手数料(文字列)
        /// </summary>
        public string InputedFeeStr => AssetService.Instance.ToAssetString(this.InputedFee, this.FeeAssetListVM.SelectedKey, UnitKind.MainUnit, UnitKind.MainUnit);

        /// <summary>
        /// 備考リストVM
        /// </summary>
        public SelectableViewModel<RemarkModel, string> RemarkListVM => field ??= new(static vm => vm?.Remark, this.mBusyService);
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
            () => this.SrcItemListVM.SelectedKey != null && this.InputedSrcValue.HasValue && 0 < this.InputedSrcValue &&
                  this.DstItemListVM.SelectedKey != null && this.InputedDstValue.HasValue && 0 < this.InputedDstValue, this.mBusyService);
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
            get => UserSettingService.Instance.ExchangeRegistrationWindowSize;
            set => UserSettingService.Instance.ExchangeRegistrationWindowSize = value;
        }

        public override Point WindowPointSetting {
            get => UserSettingService.Instance.ExchangeRegistrationWindowPoint;
            set => UserSettingService.Instance.ExchangeRegistrationWindowPoint = value;
        }
        #endregion

        public override void Initialize(DbHandlerFactory dbHandlerFactory)
        {
            base.Initialize(dbHandlerFactory);

            this.mAppService = new(this.mDbHandlerFactory);
            this.mService = new(this.mDbHandlerFactory);

            this.AccountListVM.SetLoader(async () => await this.mAppService.LoadAccountListAsync());

            // 変換元(支出)
            this.SrcItemListVM.SetLoader(
                async () => await this.mAppService.LoadExchangeItemListAsync(this.AccountListVM.SelectedKey, BalanceKind.Expenses),
                () => this.AccountListVM.SelectedKey != null);
            this.SrcAssetListVM.SetLoader(() => AssetService.Instance.Assets);
            this.SrcAssetListVM.SetDefaultSelector(() => AssetService.Instance.GetValidAssetId(this.SrcItemListVM.SelectedItem?.AssetId, this.AccountListVM.SelectedItem?.AssetId));

            // 変換先(収入)
            this.DstItemListVM.SetLoader(
                async () => await this.mAppService.LoadExchangeItemListAsync(this.AccountListVM.SelectedKey, BalanceKind.Income),
                () => this.AccountListVM.SelectedKey != null);
            this.DstAssetListVM.SetLoader(() => AssetService.Instance.Assets);
            this.DstAssetListVM.SetDefaultSelector(() => AssetService.Instance.GetValidAssetId(this.DstItemListVM.SelectedItem?.AssetId, this.AccountListVM.SelectedItem?.AssetId));

            // 手数料(支出)
            this.FeeKindListVM.SetLoader(() => ExchangeFeeKindStr);
            this.FeeItemListVM.SetLoader(
                async () => await this.mAppService.LoadItemListAsync(this.AccountListVM.SelectedKey, BalanceKind.Expenses, CategoryIdObj.System),
                () => this.AccountListVM.SelectedKey != null);
            this.FeeAssetListVM.SetLoader(() => AssetService.Instance.Assets);
            this.FeeAssetListVM.SetDefaultSelector(() => AssetService.Instance.GetValidAssetId(this.FeeItemListVM.SelectedItem?.AssetId, this.AccountListVM.SelectedItem?.AssetId));
            this.RemarkListVM.SetLoader(
                async () => await this.mAppService.LoadRemarkListAsync(this.FeeItemListVM.SelectedKey, true),
                () => this.FeeItemListVM.SelectedKey != null, KeySelectionMode.Force);
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

            AccountIdObj selectingAccountId = null;
            ItemIdObj selectingSrcItemId = null;
            ItemIdObj selectingDstItemId = null;
            ItemIdObj selectingFeeItemId = null;
            FeeKind selectingFeeKind = default;
            string selectingFeeRemark = null;

            switch (this.RegKind) {
                case RegistrationKind.Add: {
                    selectingAccountId = initialAccountId;
                    selectingSrcItemId = ItemIdObj.System;
                    selectingDstItemId = ItemIdObj.System;
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
                    (srcAction, dstAction, feeAction) = await this.mService.LoadExchangeActionsAsync(targetGroupId);

                    // WVMに値を設定する
                    if (this.RegKind == RegistrationKind.Edit) {
                        this.SrcActionId = srcAction.ActionId;
                        this.DstActionId = dstAction.ActionId;
                        this.GroupId = targetGroupId;
                        this.FeeActionId = feeAction?.ActionId;
                    }
                    selectingAccountId = srcAction.Account.Id;
                    selectingSrcItemId = srcAction.Item.Id;
                    selectingDstItemId = dstAction.Item.Id;
                    selectingFeeKind = feeAction?.Account.Id == dstAction.Account.Id ? FeeKind.Destination : FeeKind.Source;
                    selectingFeeItemId = feeAction?.Item.Id;
                    selectingFeeRemark = feeAction?.Remark?.Remark;

                    this.IsDateLink = srcAction.ActTime == dstAction.ActTime;

                    this.SelectedSrcDate = srcAction.ActTime;
                    this.InputedSrcValue = srcAction.Expenses?.MainValue; // 変換元帳簿の支出

                    this.SelectedDstDate = dstAction.ActTime;
                    this.InputedDstValue = dstAction.Income?.MainValue; // 変換先帳簿の収入

                    this.InputedFee = feeAction?.Expenses?.MainValue;

                    break;
                }
            }

            // リストを更新する
            await this.AccountListVM.LoadAsync(selectingAccountId);
            await this.SrcItemListVM.LoadAsync(selectingSrcItemId);
            await this.DstItemListVM.LoadAsync(selectingDstItemId);
            await this.FeeKindListVM.LoadAsync(selectingFeeKind);
            await this.FeeItemListVM.LoadAsync(selectingFeeItemId);
            await this.RemarkListVM.LoadAsync(selectingFeeRemark);

            // アセットを更新する
            await this.SrcAssetListVM.LoadAsync();
            this.RaiseSrcAssetChanged();
            await this.DstAssetListVM.LoadAsync();
            this.RaiseDstAssetChanged();
            await this.FeeAssetListVM.LoadAsync();
            this.RaiseFeeAssetChanged();
        }

        public override void AddEventHandlers()
        {
            using FuncLog funcLog = new();

            // 帳簿変更時
            this.AccountListVM.SelectionChanged += (sender, e) => this.SelectedAccountChanged?.Invoke(sender, e);
            this.AccountListVM.Children.AddRange([this.SrcItemListVM, this.SrcAssetListVM, this.DstAssetListVM, this.FeeItemListVM]);

            // 変換元項目変更時
            this.SrcItemListVM.SelectionChanged += (sender, e) => this.SelectedSrcItemChanged?.Invoke(sender, e);
            this.SrcItemListVM.Children.AddRange([this.SrcAssetListVM, this.DstItemListVM]);

            // 変換元アセット変更時
            this.SrcAssetListVM.SelectionChanged += (sender, e) => this.RaiseSrcAssetChanged();

            // 変換先項目変更時
            this.DstItemListVM.SelectionChanged += (sender, e) => this.SelectedDstItemChanged?.Invoke(sender, e);
            this.DstItemListVM.Children.Add(this.DstAssetListVM);

            // 変換先アセット変更時
            this.DstAssetListVM.SelectionChanged += (sender, e) => this.RaiseDstAssetChanged();

            // 手数料種別変更時
            this.FeeKindListVM.SelectionChanged += (sender, e) => this.SelectedFeeKindChanged?.Invoke(sender, e);

            // 手数料項目変更時
            this.FeeItemListVM.SelectionChanged += (sender, e) => this.SelectedFeeItemChanged?.Invoke(sender, e);
            this.FeeItemListVM.Children.AddRange([this.FeeAssetListVM, this.RemarkListVM]);

            // 手数料アセット変更時
            this.FeeAssetListVM.SelectionChanged += (sender, e) => this.RaiseFeeAssetChanged();
        }

        /// <summary>
        /// DBに登録する
        /// </summary>
        /// <returns>登録された帳簿項目IDリスト</returns>
        protected override async Task<IEnumerable<ActionIdObj>> SaveAsync()
        {
            using FuncLog funcLog = new();

            // 変換元
            ActionModel srcAction = new() {
                Base = new(this.SrcActionId, this.SelectedSrcDate, new(-this.InputedSrcValue.Value, this.SrcAssetListVM.SelectedKey)),
                AssetId = AssetIdObj.System, // 固定
                GroupId = this.GroupId,
                Account = new(this.AccountListVM.SelectedKey, string.Empty),
                Item = new(this.SrcItemListVM.SelectedKey, this.SrcItemListVM.SelectedItem.Name)
            };

            // 変換先
            ActionModel dstAction = new() {
                Base = new(this.DstActionId, this.SelectedDstDate, new(this.InputedDstValue.Value, this.DstAssetListVM.SelectedKey)),
                AssetId = AssetIdObj.System, // 固定
                GroupId = this.GroupId,
                Account = new(this.AccountListVM.SelectedKey, string.Empty),
                Item = new(this.DstItemListVM.SelectedKey, this.DstItemListVM.SelectedItem.Name)
            };

            // 手数料
            FeeKind feeKind = this.FeeKindListVM.SelectedKey;
            DateTime feeActTime = feeKind switch {
                FeeKind.Source => srcAction.ActTime,
                FeeKind.Destination => dstAction.ActTime,
                _ => throw new NotSupportedException("SelectedFeeKind")
            };
            ActionModel feeAction = new() {
                Base = new(this.FeeActionId, feeActTime, new(-this.InputedFee ?? 0m, this.FeeAssetListVM.SelectedKey)),
                AssetId = AssetIdObj.System, // 今のところ固定を想定
                GroupId = this.GroupId,
                Account = new(this.AccountListVM.SelectedKey, string.Empty),
                Item = new(this.FeeItemListVM.SelectedKey, string.Empty),
                Remark = this.RemarkListVM.SelectedKey
            };

            IEnumerable<ActionIdObj> resActionIdList = await this.mService.SaveExchangeActionsAsync(srcAction, dstAction, feeAction);

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

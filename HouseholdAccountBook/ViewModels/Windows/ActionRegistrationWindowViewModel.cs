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
    /// 帳簿項目登録ウィンドウVM
    /// </summary>
    public class ActionRegistrationWindowViewModel : WindowViewModelBase
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
        /// 日時変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<DateTime>> SelectedDateChanged;
        /// <summary>
        /// 収支変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<BalanceKind>> SelectedBalanceKindChanged;
        /// <summary>
        /// 分類変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<CategoryIdObj>> SelectedCategoryChanged;
        /// <summary>
        /// 項目変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<ItemIdObj>> SelectedItemChanged;

        /// <summary>
        /// 登録時イベント
        /// </summary>
        public event EventHandler<EventArgs<IEnumerable<ActionIdObj>>> Registrated;
        #endregion

        #region Bindingプロパティ
        /// <summary>
        /// 登録モード
        /// </summary>
        public RegistrationKind RegKind {
            get;
            set => this.SetProperty(ref field, value);
        } = RegistrationKind.Add;

        /// <summary>
        /// 帳簿項目ID
        /// </summary>
        public ActionIdObj ActionId {
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
        /// CSV比較からの追加
        /// </summary>
        public bool AddedByCsvComparison {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 帳簿リストVM
        /// </summary>
        public SelectableViewModel<AccountModel, AccountIdObj> AccountListVM => field ??= new(static vm => vm?.Id, this.mBusyService);

        /// <summary>
        /// 選択された日時
        /// </summary>
        public DateTime SelectedDate {
            get;
            set {
                DateTime oldValue = field;
                if (this.SetProperty(ref field, value)) {
                    this.SelectedDateChanged?.Invoke(this, new() { OldValue = oldValue, NewValue = value });
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        } = DateTime.Now;

        /// <summary>
        /// 収支種別リストVM
        /// </summary>
        public SelectableViewModel<KeyValuePair<BalanceKind, string>, BalanceKind> BalanceKindListVM => field ??= new(static p => p.Key);

        /// <summary>
        /// 分類リストVM
        /// </summary>
        public SelectableViewModel<CategoryModel, CategoryIdObj> CategoryListVM => field ??= new(static vm => vm?.Id, this.mBusyService);

        /// <summary>
        /// 項目リストVM
        /// </summary>
        public SelectableViewModel<ItemModel, ItemIdObj> ItemListVM => field ??= new(static vm => vm?.Id, this.mBusyService);

        /// <summary>
        /// アセットリストVM
        /// </summary>
        public SelectableViewModel<AssetModel, AssetIdObj> AssetListVM => field ??= new(static vm => vm?.Id, this.mBusyService);
        private void RaiseAssetChanged()
        {
            this.RaisePropertyChanged(nameof(this.ValueScale));
            this.RaisePropertyChanged(nameof(this.InputedValueStr));
        }
        /// <summary>
        /// 入力された金額(主単位)
        /// </summary>
        public decimal? InputedValue {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    CommandManager.InvalidateRequerySuggested();
                    this.RaisePropertyChanged(nameof(this.InputedValueStr));
                }
            }
        }
        /// <summary>
        /// 金額の小数点以下桁数
        /// </summary>
        public int ValueScale => AssetService.Instance.GetAssetModel(this.AssetListVM.SelectedKey).Scale;
        /// <summary>
        /// 入力された金額(文字列)
        /// </summary>
        public string InputedValueStr => AssetService.Instance.ToAssetString(this.InputedValue, this.AssetListVM.SelectedKey, UnitKind.MainUnit, UnitKind.MainUnit);

        /// <summary>
        /// 店舗リストVM
        /// </summary>
        public SelectableViewModel<ShopModel, string> ShopListVM => field ??= new(static vm => vm?.Name, this.mBusyService);

        /// <summary>
        /// 備考リストVM
        /// </summary>
        public SelectableViewModel<RemarkModel, string> RemarkListVM => field ??= new(static vm => vm?.Remark, this.mBusyService);

        /// <summary>
        /// 入力された繰り返し回数
        /// </summary>
        public int InputedCount {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        } = 1;

        /// <summary>
        /// 同じグループIDを持つ帳簿項目を連動して編集
        /// </summary>
        public bool SelectedIfLink {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 休日設定リストVM
        /// </summary>
        public SelectableViewModel<KeyValuePair<HolidaySettingKind, string>, HolidaySettingKind> HolidaySettingListVM => field ??= new(static p => p.Key);

        /// <summary>
        /// 選択された一致フラグ
        /// </summary>
        public bool SelectedIfMatch {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        #region コマンド
        /// <summary>
        /// 今日コマンド
        /// </summary>
        public ICommand TodayCommand => field ??= new RelayCommand(() => this.SelectedDate = DateTime.Today, () => this.SelectedDate != DateTime.Today);

        /// <summary>
        /// 続けて入力コマンド
        /// </summary>
        public ICommand ContinueToOKCommand => field ??= new AsyncRelayCommand(
            this.ContinueToOKCommand_ExecuteAsync,
            () => this.ItemListVM.SelectedKey != null && this.InputedValue.HasValue && 0 < this.InputedValue && this.RegKind == RegistrationKind.Add, this.mBusyService);
        /// <summary>
        /// 続けて入力コマンド処理
        /// </summary>
        protected async Task ContinueToOKCommand_ExecuteAsync()
        {
            // DB登録
            ActionIdObj id = await this.SaveAsync();

            // 呼び出し元更新
            IEnumerable<ActionIdObj> value = id != null ? [id.Id] : [];
            this.Registrated?.Invoke(this, new EventArgs<IEnumerable<ActionIdObj>>(value));

            // 表示クリア
            this.InputedValue = null;
            this.InputedCount = 1;
        }

        /// <summary>
        /// OKコマンド
        /// </summary>
        public new ICommand OKCommand => field ??= new AsyncRelayCommand(
            this.OKCommand_ExecuteAsync,
            () => this.ItemListVM.SelectedKey != null && this.InputedValue.HasValue && 0 < this.InputedValue, this.mBusyService);
        protected async Task OKCommand_ExecuteAsync()
        {
            // DB登録
            ActionIdObj id = await this.SaveAsync();

            // 呼び出し元更新
            IEnumerable<ActionIdObj> value = id != null ? [id.Id] : [];
            this.Registrated?.Invoke(this, new EventArgs<IEnumerable<ActionIdObj>>(value));

            base.OKCommand_Execute();
        }
        #endregion

        #region ウィンドウ設定プロパティ
        protected override (double, double) WindowSizeSettingRaw {
            get => UserSettingService.Instance.ActionRegistrationWindowSize;
            set => UserSettingService.Instance.ActionRegistrationWindowSize = value;
        }

        public override Point WindowPointSetting {
            get => UserSettingService.Instance.ActionRegistrationWindowPoint;
            set => UserSettingService.Instance.ActionRegistrationWindowPoint = value;
        }
        #endregion

        public override void Initialize(DbHandlerFactory dbHandlerFactory)
        {
            base.Initialize(dbHandlerFactory);

            this.mAppService = new(this.mDbHandlerFactory);
            this.mService = new(this.mDbHandlerFactory);

            this.AccountListVM.SetLoader(async () => await this.mAppService.LoadAccountListAsync());
            this.BalanceKindListVM.SetLoader(() => BalanceKindStr);
            this.CategoryListVM.SetLoader(
                async () => await this.mAppService.LoadCategoryListAsync(this.AccountListVM.SelectedKey, this.BalanceKindListVM.SelectedKey, Properties.Resources.ListName_NoSpecification),
                () => this.AccountListVM.SelectedKey != null);
            this.ItemListVM.SetLoader(
                async () => await this.mAppService.LoadItemListAsync(this.AccountListVM.SelectedKey, this.BalanceKindListVM.SelectedKey, this.CategoryListVM.SelectedKey),
                () => this.AccountListVM.SelectedKey != null && this.CategoryListVM.SelectedKey != null);
            this.AssetListVM.SetLoader(() => AssetService.Instance.Assets);
            this.AssetListVM.SetDefaultSelector(() => AssetService.Instance.GetValidAssetId(this.ItemListVM.SelectedItem?.AssetId, this.AccountListVM.SelectedItem?.AssetId));
            this.ShopListVM.SetLoader(
                async () => await this.mAppService.LoadShopListAsync(this.ItemListVM.SelectedKey, true),
                () => this.ItemListVM.SelectedKey != null, KeySelectionMode.Force);
            this.RemarkListVM.SetLoader(
                async () => await this.mAppService.LoadRemarkListAsync(this.ItemListVM.SelectedKey, true),
                () => this.ItemListVM.SelectedKey != null, KeySelectionMode.Force);
            this.HolidaySettingListVM.SetLoader(() => HolidaySettingKindStr);
        }

        public override async Task LoadAsync() => await this.LoadAsync(null, null, null, null, null);

        /// <summary>
        /// DBから読み込む
        /// </summary>
        /// <param name="initialAccountId">追加時、初期選択する帳簿ID</param>
        /// <param name="initialMonth">追加時、初期選択する年月</param>
        /// <param name="initialDate">追加時、初期選択する日付</param>
        /// <param name="initialRecord">追加時、初期表示するCSVレコード</param>
        /// <param name="targetActionId">複製/編集時、複製/編集対象の帳簿項目のID</param>
        public async Task LoadAsync(AccountIdObj initialAccountId, DateOnly? initialMonth, DateOnly? initialDate, ActionCsvModel initialRecord, ActionIdObj targetActionId)
        {
            using FuncLog funcLog = new(new { initialAccountId, initialMonth, initialDate, initialRecord, targetActionId });
            using IDisposable disposable = this.mBusyService.Enter();

            AccountIdObj selectingAccountId = null;
            BalanceKind selectingBalanceKind = BalanceKind.Expenses;
            ItemIdObj selectingItemId = null;
            string selectingShopName = null;
            string selectingRemark = null;

            switch (this.RegKind) {
                case RegistrationKind.Add: {
                    selectingAccountId = initialAccountId ?? AccountIdObj.System;
                    selectingBalanceKind = BalanceKind.Expenses;
                    if (initialRecord == null) {
                        this.SelectedDate = initialDate?.ToDateTime(TimeOnly.MinValue) ?? ((initialMonth == null || initialMonth?.Month == DateTime.Today.Month) ? DateTime.Today : initialMonth.Value.ToDateTime(TimeOnly.MinValue));
                        this.InputedValue = null;
                    }
                    else {
                        this.SelectedDate = initialRecord.Date;
                        this.InputedValue = initialRecord.Value.MainValue;
                    }

                    break;
                }
                case RegistrationKind.Edit:
                case RegistrationKind.Copy: {
                    // DBから値を読み込む
                    ActionModel action = await this.mService.LoadActionAsync(targetActionId);
                    // 繰り返しの回数を読み込む
                    int count = await this.mService.LoadRepeatCountAsync(targetActionId);

                    // WVMに値を設定する
                    if (this.RegKind == RegistrationKind.Edit) {
                        this.ActionId = action.ActionId;
                        this.GroupId = action.GroupId;
                        this.SelectedIfMatch = action.IsMatch;
                    }
                    this.SelectedDate = action.ActTime;
                    this.InputedValue = Math.Abs(action.Amount.MainValue);
                    this.InputedCount = count;

                    selectingAccountId = action.Account.Id;
                    selectingBalanceKind = action.Category.BalanceKind;
                    selectingItemId = action.Item.Id;
                    selectingShopName = action.Shop.Name;
                    selectingRemark = action.Remark.Remark;

                    break;
                }
            }

            // リストを更新する
            await this.AccountListVM.LoadAsync(selectingAccountId);
            await this.BalanceKindListVM.LoadAsync(selectingBalanceKind);
            await this.CategoryListVM.LoadAsync();
            await this.ItemListVM.LoadAsync(selectingItemId);
            await this.ShopListVM.LoadAsync(selectingShopName);
            await this.RemarkListVM.LoadAsync(selectingRemark);
            await this.HolidaySettingListVM.LoadAsync(HolidaySettingKind.Nothing);

            // アセットを更新する
            await this.AssetListVM.LoadAsync();
            this.RaiseAssetChanged();
        }

        public override void AddEventHandlers()
        {
            using FuncLog funcLog = new();

            // 帳簿選択変更時
            this.AccountListVM.SelectionChanged += (sender, e) => this.SelectedAccountChanged?.Invoke(sender, e);
            this.AccountListVM.Children.AddRange([this.CategoryListVM, this.ItemListVM, this.AssetListVM]);

            // 収支種別選択変更時
            this.BalanceKindListVM.SelectionChanged += (sender, e) => this.SelectedBalanceKindChanged?.Invoke(sender, e);
            this.BalanceKindListVM.Children.AddRange([this.CategoryListVM, this.ItemListVM]);

            // 分類選択変更時
            this.CategoryListVM.SelectionChanged += (sender, e) => this.SelectedCategoryChanged?.Invoke(sender, e);
            this.CategoryListVM.Children.Add(this.ItemListVM);

            // 項目選択変更時
            this.ItemListVM.SelectionChanged += (sender, e) => this.SelectedItemChanged?.Invoke(sender, e);
            this.ItemListVM.Children.AddRange([this.AssetListVM, this.ShopListVM, this.RemarkListVM]);

            // アセット選択変更時
            this.AssetListVM.SelectionChanged += (sender, e) => this.RaiseAssetChanged();
        }

        /// <summary>
        /// DBに登録する
        /// </summary>
        /// <returns>登録された帳簿項目ID(先頭のみ)</returns>
        protected override async Task<ActionIdObj> SaveAsync()
        {
            using FuncLog funcLog = new();

            int sign = this.BalanceKindListVM.SelectedKey == BalanceKind.Income ? 1 : -1;
            ActionModel action = new() {
                Base = new(this.ActionId, this.SelectedDate, new(sign * this.InputedValue.Value, this.AccountListVM.SelectedItem.AssetId)),
                AssetId = AssetIdObj.System, // TODO: 将来の拡張用
                GroupId = this.GroupId,
                Account = new(this.AccountListVM.SelectedKey, string.Empty),
                Category = new(CategoryIdObj.System, string.Empty, this.BalanceKindListVM.SelectedKey),
                Item = new(this.ItemListVM.SelectedKey, string.Empty),
                Shop = new(this.ShopListVM.SelectedKey),
                Remark = new(this.RemarkListVM.SelectedKey),
                IsMatch = this.SelectedIfMatch
            };

            ActionIdObj resActionId = await this.mService.SaveActionAsync(action, this.InputedCount, this.SelectedIfLink, this.HolidaySettingListVM.SelectedKey);

            if (!string.IsNullOrEmpty(action.Shop)) {
                ShopModel shop = new(action.Shop) { ItemId = action.Item.Id, CurrentActTime = action.ActTime };
                await this.mService.SaveShopAsync(shop);
            }

            if (!string.IsNullOrEmpty(action.Remark)) {
                RemarkModel remark = new(action.Remark) { ItemId = action.Item.Id, CurrentActTime = action.ActTime };
                await this.mService.SaveRemarkAsync(remark);
            }

            return resActionId;
        }
    }
}

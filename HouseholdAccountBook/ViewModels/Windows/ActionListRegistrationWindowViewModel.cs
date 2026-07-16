using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.Args;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using HouseholdAccountBook.Views.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static HouseholdAccountBook.ViewModels.UiConstants;

namespace HouseholdAccountBook.ViewModels.Windows
{
    /// <summary>
    /// 帳簿項目リスト登録ウィンドウVM
    /// </summary>
    public class ActionListRegistrationWindowViewModel : WindowViewModelBase
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
        /// 登録種別
        /// </summary>
        public RegistrationKind RegKind {
            get;
            set {
                if (value == RegistrationKind.Copy) {
                    throw new NotSupportedException("登録種別にCopyは使用できません。");
                }
                _ = this.SetProperty(ref field, value);
            }
        }

        /// <summary>
        /// グループID
        /// </summary>
        public GroupIdObj GroupId {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 帳簿セレクタVM
        /// </summary>
        public SelectorViewModel<AccountModel, AccountIdObj> AccountSelectorVM => field ??= new(static vm => vm?.Id, this.mBusyService);

        /// <summary>
        /// 収支種別セレクタVM
        /// </summary>
        public SelectorViewModel<KeyValuePair<BalanceKind, string>, BalanceKind> BalanceKindSelectorVM => field ??= new(static p => p.Key);

        /// <summary>
        /// 分類セレクタVM
        /// </summary>
        public SelectorViewModel<CategoryModel, CategoryIdObj> CategorySelectorVM => field ??= new(static vm => vm?.Id, this.mBusyService);

        /// <summary>
        /// 項目セレクタVM
        /// </summary>
        public SelectorViewModel<ItemModel, ItemIdObj> ItemSelectorVM => field ??= new(static vm => vm?.Id, this.mBusyService);

        /// <summary>
        /// 入力された日付金額VMリスト
        /// </summary>
        public ObservableCollection<DateValueViewModel> InputedDateValueVMList {
            get;
            set => this.SetProperty(ref field, value);
        } = [];

        /// <summary>
        /// <see cref="NumericInputButton"/> の表示状態
        /// </summary>
        public bool IsOpenPopup {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 日付自動インクリメント
        /// </summary>
        public bool IsDateAutoIncrement {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 店舗セレクタVM
        /// </summary>
        public SelectorViewModel<ShopModel, string> ShopSelectorVM => field ??= new(static vm => vm?.Name, this.mBusyService);

        /// <summary>
        /// 備考セレクタVM
        /// </summary>
        public SelectorViewModel<RemarkModel, string> RemarkSelectorVM => field ??= new(static vm => vm?.Remark, this.mBusyService);

        /// <summary>
        /// 数値入力ボタンの入力値
        /// </summary>
        public int? InputedValue { get; set; }
        /// <summary>
        /// 数値入力ボタンの入力種別
        /// </summary>
        public NumericInputButton.InputKind InputedKind { get; set; }
        #endregion

        #region コマンド
        /// <summary>
        /// OKコマンド
        /// </summary>
        public new ICommand OKCommand => field ??= new AsyncRelayCommand(
            this.OKCommand_ExecuteAsync,
            () => this.ItemSelectorVM.SelectedKey != null && this.InputedDateValueVMList.Any(static vm => vm.InputedValue is not null and not 0), this.mBusyService);
        protected async Task OKCommand_ExecuteAsync()
        {
            // DB登録
            IEnumerable<ActionIdObj> idList = await this.SaveAsync();

            // MainWindow更新
            this.Registrated?.Invoke(this, new(idList));

            base.OKCommand_Execute();
        }
        #endregion

        #region ウィンドウ設定プロパティ
        protected override (double, double) WindowSizeSettingRaw {
            get => UserSettingService.Instance.ActionListRegistrationWindowSize;
            set => UserSettingService.Instance.ActionListRegistrationWindowSize = value;
        }

        public override Point WindowPointSetting {
            get => UserSettingService.Instance.ActionListRegistrationWindowPoint;
            set => UserSettingService.Instance.ActionListRegistrationWindowPoint = value;
        }
        #endregion

        public override void Initialize(DbHandlerFactory dbHandlerFactory)
        {
            base.Initialize(dbHandlerFactory);

            this.mAppService = new(this.mDbHandlerFactory);
            this.mService = new(this.mDbHandlerFactory);

            this.AccountSelectorVM.SetLoader(async () => await this.mAppService.LoadAccountListAsync());
            this.BalanceKindSelectorVM.SetLoader(() => BalanceKindStr);
            this.CategorySelectorVM.SetLoader(
                async () => await this.mAppService.LoadCategoryListAsync(this.AccountSelectorVM.SelectedKey, this.BalanceKindSelectorVM.SelectedKey, Properties.Resources.ListName_NoSpecification),
                () => this.AccountSelectorVM.SelectedKey != null);
            this.ItemSelectorVM.SetLoader(
                async () => await this.mAppService.LoadItemListAsync(this.AccountSelectorVM.SelectedKey, this.BalanceKindSelectorVM.SelectedKey, this.CategorySelectorVM.SelectedKey),
                () => this.AccountSelectorVM.SelectedKey != null && this.CategorySelectorVM.SelectedKey != null);
            this.ShopSelectorVM.SetLoader(
                async () => await this.mAppService.LoadShopListAsync(this.ItemSelectorVM.SelectedKey, true),
                () => this.ItemSelectorVM.SelectedKey != null, SelectorMode.Force);
            this.RemarkSelectorVM.SetLoader(
                async () => await this.mAppService.LoadRemarkListAsync(this.ItemSelectorVM.SelectedKey, true),
                () => this.ItemSelectorVM.SelectedKey != null, SelectorMode.Force);
        }

        public override async Task LoadAsync() => await this.LoadAsync(null, null, null, null, null);

        /// <summary>
        /// DBから読み込む
        /// </summary>
        /// <param name="initialAccountId">追加時、初期選択する帳簿のID</param>
        /// <param name="initialMonth">追加時、初期選択する年月</param>
        /// <param name="initialDate">追加時、初期選択する日付</param>
        /// <param name="initialRecordList">追加時、初期表示するCSVレコードリスト</param>
        /// <param name="targetGroupId">編集時、編集対象のグループID</param>
        public async Task LoadAsync(AccountIdObj initialAccountId, DateOnly? initialMonth, DateOnly? initialDate, IEnumerable<ActionCsvModel> initialRecordList, GroupIdObj targetGroupId)
        {
            using FuncLog funcLog = new(new { initialAccountId, initialMonth, initialDate, initialRecordList, targetGroupId });
            using IDisposable disposable = this.mBusyService.Enter();

            AccountIdObj selectingAccountId = null;
            BalanceKind selectingBalanceKind = BalanceKind.Expenses;
            ItemIdObj selectingItemId = null;
            string selectingShopName = null;
            string selectingRemark = null;

            switch (this.RegKind) {
                case RegistrationKind.Add: {
                    selectingAccountId = initialAccountId;

                    // WVMに値を設定する
                    if (initialRecordList == null) {
                        DateTime actDate = initialDate?.ToDateTime(TimeOnly.MinValue) ?? ((initialMonth == null || initialMonth?.Month == DateTime.Today.Month) ? DateTime.Today : initialMonth.Value.ToDateTime(TimeOnly.MinValue));
                        this.InputedDateValueVMList.Add(new DateValueViewModel() {
                            SelectedDate = actDate,
                            InputedValue = null
                        });
                    }
                    else {
                        foreach (ActionCsvModel record in initialRecordList) {
                            this.InputedDateValueVMList.Add(new DateValueViewModel() {
                                SelectedDate = record.Date,
                                InputedValue = record.Value.MainValue,
                                SelectedAccountAssetId = record.Value.AssetId // 一旦読込時のアセットIDを入れておく
                            });
                        }
                    }

                    break;
                }
                case RegistrationKind.Edit: {
                    List<DateValueViewModel> dateValueVMList = [];

                    // DBから値を読み込む
                    IEnumerable<ActionModel> actionList = await this.mService.LoadActionListAsync(targetGroupId);

                    foreach (ActionModel action in actionList) {
                        // 日付金額リストに追加
                        DateValueViewModel vm = new() {
                            ActionId = action.ActionId,
                            SelectedDate = action.ActTime,
                            InputedValue = Math.Abs(action.Amount.MainValue),
                            SelectedAccountAssetId = action.Amount.AssetId // 一旦読込時のアセットIDを入れておく
                        };

                        selectingAccountId = action.Account.Id;
                        selectingItemId = action.Item.Id;
                        selectingShopName = action.Shop.Name;
                        selectingRemark = action.Remark;

                        selectingBalanceKind = action.Category.BalanceKind;

                        dateValueVMList.Add(vm);
                    }

                    // WVMに値を設定する
                    this.GroupId = targetGroupId;
                    foreach (DateValueViewModel vm in dateValueVMList) {
                        this.InputedDateValueVMList.Add(vm);
                    }

                    break;
                }
                case RegistrationKind.Copy:
                    throw new NotSupportedException("登録種別にCopyは使用できません。");
            }

            // リストを更新する
            await this.AccountSelectorVM.LoadAsync(selectingAccountId);
            await this.BalanceKindSelectorVM.LoadAsync(selectingBalanceKind);
            await this.CategorySelectorVM.LoadAsync();
            await this.ItemSelectorVM.LoadAsync(selectingItemId);
            await this.ShopSelectorVM.LoadAsync(selectingShopName);
            await this.RemarkSelectorVM.LoadAsync(selectingRemark);

            // アセットIDを指定する
            foreach (DateValueViewModel item in this.InputedDateValueVMList) {
                item.SelectedAccountAssetId = this.AccountSelectorVM.SelectedItem.AssetId;
            }
        }

        public override void AddEventHandlers()
        {
            using FuncLog funcLog = new();

            // 帳簿変更時
            this.AccountSelectorVM.SelectionChanged += (sender, e) => {
                foreach (DateValueViewModel item in this.InputedDateValueVMList) {
                    item.SelectedAccountAssetId = this.AccountSelectorVM.SelectedItem.AssetId;
                }

                this.SelectedAccountChanged?.Invoke(sender, e);
            };
            this.AccountSelectorVM.Children.AddRange([this.CategorySelectorVM, this.ItemSelectorVM]);

            // 収支種別変更時
            this.BalanceKindSelectorVM.SelectionChanged += (sender, e) => this.SelectedBalanceKindChanged?.Invoke(sender, e);
            this.BalanceKindSelectorVM.Children.AddRange([this.CategorySelectorVM, this.ItemSelectorVM]);

            // 分類変更時
            this.CategorySelectorVM.SelectionChanged += (sender, e) => this.SelectedCategoryChanged?.Invoke(sender, e);
            this.CategorySelectorVM.Children.Add(this.ItemSelectorVM);

            // 項目変更時
            this.ItemSelectorVM.SelectionChanged += (sender, e) => this.SelectedItemChanged?.Invoke(sender, e);
            this.ItemSelectorVM.Children.AddRange([this.ShopSelectorVM, this.RemarkSelectorVM]);
        }

        /// <summary>
        /// DBに登録する
        /// </summary>
        /// <returns>登録された帳簿項目IDリスト</returns>
        protected override async Task<IEnumerable<ActionIdObj>> SaveAsync()
        {
            using FuncLog funcLog = new();

            List<ActionModel> actionList = [];
            BalanceKind balanceKind = this.BalanceKindSelectorVM.SelectedKey;
            ActionModel commonAction = new() {
                Base = new(null, DateTime.Now, new(0m, AssetIdObj.System)),
                AssetId = AssetIdObj.System, // TODO: 将来の拡張用
                GroupId = this.GroupId,
                Account = new(this.AccountSelectorVM.SelectedKey, string.Empty),
                Item = new(this.ItemSelectorVM.SelectedKey, string.Empty),
                Shop = this.ShopSelectorVM.SelectedKey,
                Remark = this.RemarkSelectorVM.SelectedKey
            };
            foreach (DateValueViewModel vm in this.InputedDateValueVMList) {
                if (vm.InputedValue is null or 0) { continue; }

                int sign = balanceKind == BalanceKind.Income ? 1 : -1;
                ActionBaseModel baseAction = new(vm.ActionId, vm.SelectedDate, new(sign * vm.InputedValue.Value, AssetIdObj.System));
                actionList.Add(commonAction.WithChanges(baseAction));
            }

            IEnumerable<ActionIdObj> resActionIdList = await this.mService.SaveActionListAsync(actionList);

            DateTime lastActTime = this.InputedDateValueVMList.Max(static tmp => tmp.SelectedDate);

            if (!string.IsNullOrEmpty(commonAction.Shop)) {
                ShopModel shop = new(commonAction.Shop) { ItemId = commonAction.Item.Id, CurrentActTime = commonAction.ActTime };
                await this.mService.SaveShopAsync(shop);
            }

            if (!string.IsNullOrEmpty(commonAction.Remark)) {
                RemarkModel remark = new(commonAction.Remark) { ItemId = commonAction.Item.Id, CurrentActTime = commonAction.ActTime };
                await this.mService.SaveRemarkAsync(remark);
            }

            return resActionIdList;
        }
    }
}

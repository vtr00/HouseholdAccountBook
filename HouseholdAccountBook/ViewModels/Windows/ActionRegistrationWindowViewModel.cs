using HouseholdAccountBook.Infrastructure.CSV;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities.Args;
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
        public event EventHandler<ChangedEventArgs<BookIdObj>> BookChanged;
        /// <summary>
        /// 日時変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<DateTime>> DateChanged;
        /// <summary>
        /// 収支変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<BalanceKind>> BalanceKindChanged;
        /// <summary>
        /// 分類変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<CategoryIdObj>> CategoryChanged;
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
        /// 帳簿セレクタVM
        /// </summary>
        public SelectorViewModel<BookModel, BookIdObj> BookSelectorVM { get; } = new(static vm => vm?.Id);

        /// <summary>
        /// 選択された日時
        /// </summary>
        public DateTime SelectedDate {
            get;
            set {
                var oldValue = field;
                if (this.SetProperty(ref field, value)) {
                    this.DateChanged?.Invoke(this, new() { OldValue = oldValue, NewValue = value });
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        } = DateTime.Now;

        /// <summary>
        /// 収支種別セレクタVM
        /// </summary>
        public SelectorViewModel<KeyValuePair<BalanceKind, string>, BalanceKind> BalanceKindSelectorVM { get; } = new(static p => p.Key);

        /// <summary>
        /// 分類セレクタVM
        /// </summary>
        public SelectorViewModel<CategoryModel, CategoryIdObj> CategorySelectorVM { get; } = new(static vm => vm?.Id);

        /// <summary>
        /// 項目セレクタVM
        /// </summary>
        public SelectorViewModel<ItemModel, ItemIdObj> ItemSelectorVM { get; } = new(static vm => vm?.Id);

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
        /// 店舗セレクタVM
        /// </summary>
        public SelectorViewModel<ShopModel, string> ShopSelectorVM { get; } = new(static vm => vm?.Name);

        /// <summary>
        /// 備考セレクタVM
        /// </summary>
        public SelectorViewModel<RemarkModel, string> RemarkSelectorVM { get; } = new(static vm => vm?.Remark);

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
        /// 休日設定セレクタVM
        /// </summary>
        public SelectorViewModel<KeyValuePair<HolidaySettingKind, string>, HolidaySettingKind> HolidaySettingSelectorVM { get; } = new(static p => p.Key);

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
        public ICommand ContinueToOKCommand => field ??= new AsyncRelayCommand(this.ContinueToOKCommand_ExecuteAsync, this.ContinueToOKCommand_CanExecute);
        /// <summary>
        /// 続けて入力コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        protected bool ContinueToOKCommand_CanExecute() => this.OKCommand_CanExecute() && this.RegKind == RegistrationKind.Add;
        /// <summary>
        /// 続けて入力コマンド処理
        /// </summary>
        protected async Task ContinueToOKCommand_ExecuteAsync()
        {
            // DB登録
            ActionIdObj id = null;
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                id = await this.SaveAsync();
            }

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
        public new ICommand OKCommand => field ??= new AsyncRelayCommand(this.OKCommand_ExecuteAsync, this.OKCommand_CanExecute);
        protected override bool OKCommand_CanExecute() => this.ItemSelectorVM.SelectedKey != null && this.InputedValue.HasValue && 0 < this.InputedValue;
        protected async Task OKCommand_ExecuteAsync()
        {
            // DB登録
            ActionIdObj id = null;
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                id = await this.SaveAsync();
            }

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

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ActionRegistrationWindowViewModel()
        {
            using FuncLog funcLog = new();

            this.BookSelectorVM.SetLoader(async () => await this.mAppService.LoadBookListAsync());
            this.BalanceKindSelectorVM.SetLoader(() => BalanceKindStr);
            this.CategorySelectorVM.SetLoader(
                async () => await this.mAppService.LoadCategoryListAsync(this.BookSelectorVM.SelectedKey, this.BalanceKindSelectorVM.SelectedKey, Properties.Resources.ListName_NoSpecification),
                () => this.BookSelectorVM.SelectedKey != null);
            this.ItemSelectorVM.SetLoader(
                async () => await this.mAppService.LoadItemListAsync(this.BookSelectorVM.SelectedKey, this.BalanceKindSelectorVM.SelectedKey, this.CategorySelectorVM.SelectedKey),
                () => this.BookSelectorVM.SelectedKey != null && this.CategorySelectorVM.SelectedKey != null);
            this.ShopSelectorVM.SetLoader(
                async () => await this.mAppService.LoadShopListAsync(this.ItemSelectorVM.SelectedKey, true),
                () => this.ItemSelectorVM.SelectedKey != null, SelectorMode.Force);
            this.RemarkSelectorVM.SetLoader(
                async () => await this.mAppService.LoadRemarkListAsync(this.ItemSelectorVM.SelectedKey, true),
                () => this.ItemSelectorVM.SelectedKey != null, SelectorMode.Force);
            this.HolidaySettingSelectorVM.SetLoader(() => HolidaySettingKindStr);
        }

        public override void Initialize(WaitCursorManagerFactory waitCursorManagerFactory, DbHandlerFactory dbHandlerFactory)
        {
            base.Initialize(waitCursorManagerFactory, dbHandlerFactory);

            this.mAppService = new(this.mDbHandlerFactory);
            this.mService = new(this.mDbHandlerFactory);

            this.BookSelectorVM.WaitCursorManagerFactory = waitCursorManagerFactory;
            this.BalanceKindSelectorVM.WaitCursorManagerFactory = waitCursorManagerFactory;
            this.CategorySelectorVM.WaitCursorManagerFactory = waitCursorManagerFactory;
            this.ItemSelectorVM.WaitCursorManagerFactory = waitCursorManagerFactory;
            this.ShopSelectorVM.WaitCursorManagerFactory = waitCursorManagerFactory;
            this.RemarkSelectorVM.WaitCursorManagerFactory = waitCursorManagerFactory;
            this.HolidaySettingSelectorVM.WaitCursorManagerFactory = waitCursorManagerFactory;
        }

        public override async Task LoadAsync() => await this.LoadAsync(null, null, null, null, null);

        /// <summary>
        /// DBから読み込む
        /// </summary>
        /// <param name="initialBookId">追加時、初期選択する帳簿ID</param>
        /// <param name="initialMonth">追加時、初期選択する年月</param>
        /// <param name="initialDate">追加時、初期選択する日付</param>
        /// <param name="initialRecord">追加時、初期表示するCSVレコード</param>
        /// <param name="targetActionId">複製/編集時、複製/編集対象の帳簿項目のID</param>
        public async Task LoadAsync(BookIdObj initialBookId, DateOnly? initialMonth, DateOnly? initialDate, ActionCsvDto initialRecord, ActionIdObj targetActionId)
        {
            using FuncLog funcLog = new(new { initialBookId, initialMonth, initialDate, initialRecord, targetActionId });

            BookIdObj selectingBookId = null;
            BalanceKind selectingBalanceKind = BalanceKind.Expenses;
            ItemIdObj selectingItemId = null;
            string selectingShopName = null;
            string selectingRemark = null;

            switch (this.RegKind) {
                case RegistrationKind.Add: {
                    selectingBookId = initialBookId ?? BookIdObj.System;
                    selectingBalanceKind = BalanceKind.Expenses;
                    if (initialRecord == null) {
                        this.SelectedDate = initialDate?.ToDateTime(TimeOnly.MinValue) ?? ((initialMonth == null || initialMonth?.Month == DateTime.Today.Month) ? DateTime.Today : initialMonth.Value.ToDateTime(TimeOnly.MinValue));
                    }
                    else {
                        this.SelectedDate = initialRecord.Date;
                        this.InputedValue = initialRecord.Value;
                    }

                    break;
                }
                case RegistrationKind.Edit:
                case RegistrationKind.Copy: {
                    // DBから値を読み込む
                    ActionModel action = await this.mService.LoadActionAsync(targetActionId);
                    // 繰り返しの回数を読み込む
                    int count = await this.mService.LoadRepeatCount(targetActionId);

                    // WVMに値を設定する
                    if (this.RegKind == RegistrationKind.Edit) {
                        this.ActionId = action.ActionId;
                        this.GroupId = action.GroupId;
                        this.SelectedIfMatch = action.IsMatch;
                    }
                    this.SelectedDate = action.ActTime;
                    this.InputedValue = Math.Abs(action.Amount);
                    this.InputedCount = count;

                    selectingBookId = action.Book.Id;
                    selectingBalanceKind = action.Category.BalanceKind;
                    selectingItemId = action.Item.Id;
                    selectingShopName = action.Shop.Name;
                    selectingRemark = action.Remark.Remark;

                    break;
                }
            }

            // リストを更新する
            await this.BookSelectorVM.LoadAsync(selectingBookId);
            await this.BalanceKindSelectorVM.LoadAsync(selectingBalanceKind);
            await this.CategorySelectorVM.LoadAsync();
            await this.ItemSelectorVM.LoadAsync(selectingItemId);
            await this.ShopSelectorVM.LoadAsync(selectingShopName);
            await this.RemarkSelectorVM.LoadAsync(selectingRemark);
            await this.HolidaySettingSelectorVM.LoadAsync(HolidaySettingKind.Nothing);
        }

        public override void AddEventHandlers()
        {
            using FuncLog funcLog = new();

            this.BookSelectorVM.SelectionChanged += (sender, e) => this.BookChanged?.Invoke(sender, e);
            this.BookSelectorVM.Children.Add(this.CategorySelectorVM);
            this.BalanceKindSelectorVM.SelectionChanged += (sender, e) => this.BalanceKindChanged?.Invoke(sender, e);
            this.BalanceKindSelectorVM.Children.AddRange([this.CategorySelectorVM, this.ItemSelectorVM]);
            this.CategorySelectorVM.SelectionChanged += (sender, e) => this.CategoryChanged?.Invoke(sender, e);
            this.CategorySelectorVM.Children.Add(this.ItemSelectorVM);
            this.ItemSelectorVM.SelectionChanged += (sender, e) => this.ItemChanged?.Invoke(sender, e);
            this.ItemSelectorVM.Children.AddRange([this.ShopSelectorVM, this.RemarkSelectorVM]);
        }

        /// <summary>
        /// DBに登録する
        /// </summary>
        /// <returns>登録された帳簿項目ID(先頭のみ)</returns>
        protected override async Task<ActionIdObj> SaveAsync()
        {
            using FuncLog funcLog = new();

            ActionModel action = new() {
                Base = new(this.ActionId, this.SelectedDate, (this.BalanceKindSelectorVM.SelectedKey == BalanceKind.Income ? 1 : -1) * this.InputedValue.Value),
                GroupId = this.GroupId,
                Book = new(this.BookSelectorVM.SelectedKey, string.Empty),
                Category = new(CategoryIdObj.System, string.Empty, this.BalanceKindSelectorVM.SelectedKey),
                Item = new(this.ItemSelectorVM.SelectedKey, string.Empty),
                Shop = new(this.ShopSelectorVM.SelectedKey),
                Remark = new(this.RemarkSelectorVM.SelectedKey),
                IsMatch = this.SelectedIfMatch
            };

            ActionIdObj resActionId = await this.mService.SaveActionAsync(action, this.InputedCount, this.SelectedIfLink, this.HolidaySettingSelectorVM.SelectedKey);

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

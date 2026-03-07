using HouseholdAccountBook.Infrastructure.CSV;
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
using System.Collections.ObjectModel;
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
        private AppService mAppService;
        /// <summary>
        /// 帳簿項目登録サービス
        /// </summary>
        private ActionRegService mService;

        /// <summary>
        /// 変更時処理重複防止用フラグ
        /// </summary>
        private bool mIsUpdateOnChanged;
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
        public event EventHandler<EventArgs<List<ActionIdObj>>> Registrated;
        #endregion

        #region Bindingプロパティ
        /// <summary>
        /// 登録モード
        /// </summary>
        #region RegKind
        public RegistrationKind RegKind {
            get;
            set => this.SetProperty(ref field, value);
        } = RegistrationKind.Add;
        #endregion

        /// <summary>
        /// 帳簿項目ID
        /// </summary>
        #region ActionId
        public ActionIdObj ActionId {
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
        /// CSV比較からの追加
        /// </summary>
        #region AddedByCsvComparison
        public bool AddedByCsvComparison {
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
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookModel SelectedBookVM {
            get;
            set {
                var oldValue = field;
                if (this.SetProperty(ref field, value)) {
                    if (!this.mIsUpdateOnChanged) {
                        this.mIsUpdateOnChanged = true;
                        this.BookChanged?.Invoke(this, new() { OldValue = oldValue?.Id, NewValue = value?.Id });
                        this.mIsUpdateOnChanged = false;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 選択された日時
        /// </summary>
        #region SelectedDate
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
        #endregion

        /// <summary>
        /// 収支種別辞書
        /// </summary>
        #region BalanceKindDic
        public Dictionary<BalanceKind, string> BalanceKindDic { get; } = BalanceKindStr;
        #endregion
        /// <summary>
        /// 選択された収支種別
        /// </summary>
        #region SelectedBalanceKind
        public BalanceKind SelectedBalanceKind {
            get;
            set {
                var oldValue = field;
                if (this.SetProperty(ref field, value)) {
                    if (!this.mIsUpdateOnChanged) {
                        this.mIsUpdateOnChanged = true;
                        this.BalanceKindChanged?.Invoke(this, new() { OldValue = oldValue, NewValue = value });
                        this.mIsUpdateOnChanged = false;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 分類VMリスト
        /// </summary>
        #region CategoryVMList
        public ObservableCollection<CategoryModel> CategoryVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 選択された分類VM
        /// </summary>
        #region SelectedCategoryVM
        public CategoryModel SelectedCategoryVM {
            get;
            set {
                var oldValue = field;
                if (this.SetProperty(ref field, value)) {
                    if (!this.mIsUpdateOnChanged) {
                        this.mIsUpdateOnChanged = true;
                        this.CategoryChanged?.Invoke(this, new() { OldValue = oldValue?.Id, NewValue = value?.Id });
                        this.mIsUpdateOnChanged = false;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 項目VMリスト
        /// </summary>
        #region ItemVMList
        public ObservableCollection<ItemModel> ItemVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 選択された項目VM
        /// </summary>
        #region SelectedItemVM
        public ItemModel SelectedItemVM {
            get;
            set {
                var oldValue = field;
                if (this.SetProperty(ref field, value)) {
                    if (!this.mIsUpdateOnChanged) {
                        this.mIsUpdateOnChanged = true;
                        this.ItemChanged?.Invoke(this, new() { OldValue = oldValue?.Id, NewValue = value?.Id });
                        this.mIsUpdateOnChanged = false;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 金額
        /// </summary>
        #region InputedValue
        public int? InputedValue {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        #endregion

        /// <summary>
        /// 店舗VMリスト
        /// </summary>
        #region ShopVMList
        public ObservableCollection<ShopModel> ShopVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 選択された店名
        /// </summary>
        #region SelectedShopName
        public string SelectedShopName {
            get;
            set => this.SetProperty(ref field, value);
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

        /// <summary>
        /// 入力された繰り返し回数
        /// </summary>
        #region InputedCount
        public int InputedCount {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        } = 1;
        #endregion

        /// <summary>
        /// 同じグループIDを持つ帳簿項目を連動して編集
        /// </summary>
        #region SelectedIfLink
        public bool SelectedIfLink {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 休日設定種別辞書
        /// </summary>
        #region HolidaySettingKindDic
        public Dictionary<HolidaySettingKind, string> HolidaySettingKindDic { get; } = HolidaySettingKindStr;
        #endregion
        /// <summary>
        /// 選択された休日設定種別
        /// </summary>
        #region SelectedHolidaySettingKind
        public HolidaySettingKind SelectedHolidaySettingKind {
            get;
            set => this.SetProperty(ref field, value);
        } = HolidaySettingKind.Nothing;
        #endregion

        /// <summary>
        /// 選択された一致フラグ
        /// </summary>
        #region SelectedIfMatch
        public bool SelectedIfMatch {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        #region コマンド
        /// <summary>
        /// 今日コマンド
        /// </summary>
        public ICommand TodayCommand => new RelayCommand(() => this.SelectedDate = DateTime.Today, () => this.SelectedDate != DateTime.Today);
        /// <summary>
        /// 続けて入力コマンド
        /// </summary>
        public ICommand ContinueToOKCommand => new RelayCommand(this.ContinueToOKCommand_Executed, this.ContinueToOKCommand_CanExecute);
        #endregion
        #endregion

        #region コマンドイベントハンドラ
        /// <summary>
        /// 続けて入力コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        protected bool ContinueToOKCommand_CanExecute() => this.OKCommand_CanExecute() && this.RegKind == RegistrationKind.Add;

        /// <summary>
        /// 続けて入力コマンド処理
        /// </summary>
        protected async void ContinueToOKCommand_Executed()
        {
            // DB登録
            ActionIdObj id = null;
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                id = await this.SaveAsync();
            }

            // 呼び出し元更新
            List<ActionIdObj> value = id != null ? [id.Value] : [];
            this.Registrated?.Invoke(this, new EventArgs<List<ActionIdObj>>(value));

            // 表示クリア
            this.InputedValue = null;
            this.InputedCount = 1;
        }

        protected override bool OKCommand_CanExecute() => this.SelectedItemVM != null && this.InputedValue.HasValue && 0 < this.InputedValue;
        protected override async void OKCommand_Executed()
        {
            // DB登録
            ActionIdObj id = null;
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                id = await this.SaveAsync();
            }

            // 呼び出し元更新
            List<ActionIdObj> value = id != null ? [id.Value] : [];
            this.Registrated?.Invoke(this, new EventArgs<List<ActionIdObj>>(value));

            base.OKCommand_Executed();
        }
        #endregion

        #region ウィンドウ設定プロパティ
        protected override (double, double) WindowSizeSettingRaw {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return (settings.ActionRegistrationWindow_Width, settings.ActionRegistrationWindow_Height);
            }
            set {
                Properties.Settings settings = Properties.Settings.Default;
                settings.ActionRegistrationWindow_Width = value.Item1;
                settings.ActionRegistrationWindow_Height = value.Item2;
                settings.Save();
            }
        }

        public override Point WindowPointSetting {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return new Point(settings.ActionRegistrationWindow_Left, settings.ActionRegistrationWindow_Top);
            }
            set {
                Properties.Settings settings = Properties.Settings.Default;
                settings.ActionRegistrationWindow_Left = value.X;
                settings.ActionRegistrationWindow_Top = value.Y;
                settings.Save();
            }
        }
        #endregion

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
                    selectingBookId = initialBookId ?? -1;
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
                    // 回数を読み込む
                    int count = await this.mService.LoadRepeatCount(targetActionId);

                    // WVMに値を設定する
                    if (this.RegKind == RegistrationKind.Edit) {
                        this.ActionId = action.ActionId;
                        this.GroupId = action.GroupId;
                        this.SelectedIfMatch = action.IsMatch;
                    }
                    this.SelectedDate = action.ActTime;
                    this.InputedValue = (int)action.Amount;
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
            await this.UpdateBookListAsync(selectingBookId);
            this.SelectedBalanceKind = selectingBalanceKind;
            await this.UpdateCategoryListAsync();
            await this.UpdateItemListAsync(selectingItemId);
            await this.UpdateShopListAsync(selectingShopName);
            await this.UpdateRemarkListAsync(selectingRemark);
        }

        /// <summary>
        /// 帳簿リストを更新する
        /// </summary>
        /// <param name="selectingBookId">選択対象の帳簿ID</param>
        /// <returns></returns>
        private async Task UpdateBookListAsync(BookIdObj selectingBookId = null)
        {
            using FuncLog funcLog = new(new { selectingBookId });

            ObservableCollection<BookModel> tmpBookVMList = [.. await this.mAppService.LoadBookListAsync()];
            this.SelectedBookVM = tmpBookVMList.FirstOrElementAtOrDefault(vm => vm.Id == selectingBookId, 0); // 先に選択しておく
            this.BookVMList = tmpBookVMList;
        }

        /// <summary>
        /// 分類リストを更新する
        /// </summary>
        /// <param name="selectingCategoryId">選択対象の分類ID</param>
        /// <returns></returns>
        private async Task UpdateCategoryListAsync(CategoryIdObj selectingCategoryId = null)
        {
            if (this.SelectedBookVM == null) { return; }

            using FuncLog funcLog = new(new { selectingCategoryId });

            CategoryIdObj tmpCategoryId = selectingCategoryId ?? this.SelectedCategoryVM?.Id;

            ObservableCollection<CategoryModel> tmpCategoryVMList = [.. await this.mAppService.LoadCategoryListAsync(this.SelectedBookVM.Id, this.SelectedBalanceKind, Properties.Resources.ListName_NoSpecification)];
            this.SelectedCategoryVM = tmpCategoryVMList.FirstOrElementAtOrDefault(vm => vm.Id == tmpCategoryId, 0); // 先に選択しておく
            this.CategoryVMList = tmpCategoryVMList;
        }

        /// <summary>
        /// 項目リストを更新する
        /// </summary>
        /// <param name="selectingItemId">選択対象の項目ID</param>
        /// <returns></returns>
        private async Task UpdateItemListAsync(ItemIdObj selectingItemId = null)
        {
            if (this.SelectedBookVM == null || this.SelectedCategoryVM == null) { return; }

            using FuncLog funcLog = new(new { selectingItemId });

            ItemIdObj tmpItemId = selectingItemId ?? this.SelectedItemVM?.Id;

            ObservableCollection<ItemModel> tmpItemVMList = [.. await this.mAppService.LoadItemListAsync(this.SelectedBookVM.Id, this.SelectedBalanceKind, this.SelectedCategoryVM.Id)];
            this.SelectedItemVM = tmpItemVMList.FirstOrElementAtOrDefault(vm => vm.Id == tmpItemId, 0); // 先に選択しておく
            this.ItemVMList = tmpItemVMList;
        }

        /// <summary>
        /// 店舗リストを更新する
        /// </summary>
        /// <param name="selectingShopName">選択対象の店舗名</param>
        /// <returns></returns>
        private async Task UpdateShopListAsync(string selectingShopName = null)
        {
            if (this.SelectedItemVM == null) { return; }

            using FuncLog funcLog = new(new { selectingShopName });

            string tmpShopName = selectingShopName ?? this.SelectedShopName;

            ObservableCollection<ShopModel> tmpShopVMList = [.. await this.mAppService.LoadShopListAsync(this.SelectedItemVM.Id, true)];
            this.SelectedShopName = tmpShopVMList.FirstOrElementAtOrDefault(vm => vm.Name == tmpShopName, 0).Name; // 先に選択しておく
            this.ShopVMList = tmpShopVMList;
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

            string tmpRemark = selectingRemark ?? this.SelectedRemark;

            ObservableCollection<RemarkModel> tmpRemarkVMList = [.. await this.mAppService.LoadRemarkListAsync(this.SelectedItemVM.Id, true)];
            this.SelectedRemark = tmpRemarkVMList.FirstOrElementAtOrDefault(vm => vm.Remark == tmpRemark, 0).Remark; // 先に選択しておく
            this.RemarkVMList = tmpRemarkVMList;
        }

        public override void Initialize(WaitCursorManagerFactory waitCursorManagerFactory, DbHandlerFactory dbHandlerFactory)
        {
            base.Initialize(waitCursorManagerFactory, dbHandlerFactory);

            this.mAppService = new(this.mDbHandlerFactory);
            this.mService = new(this.mDbHandlerFactory);
        }

        public override void AddEventHandlers()
        {
            using FuncLog funcLog = new();

            this.BookChanged += async (sender, e) => {
                using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, methodName: nameof(this.BookChanged));

                using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                    await this.UpdateCategoryListAsync();
                    await this.UpdateItemListAsync();
                }
            };
            this.BalanceKindChanged += async (sender, e) => {
                using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, methodName: nameof(this.BalanceKindChanged));

                using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                    await this.UpdateCategoryListAsync();
                    await this.UpdateItemListAsync();
                }
            };
            this.CategoryChanged += async (sender, e) => {
                using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, methodName: nameof(this.CategoryChanged));

                using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                    await this.UpdateItemListAsync();
                }
            };
            this.ItemChanged += async (sender, e) => {
                using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, methodName: nameof(this.ItemChanged));

                using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                    await this.UpdateShopListAsync();
                    await this.UpdateRemarkListAsync();
                }
            };
        }

        /// <summary>
        /// DBに登録する
        /// </summary>
        /// <returns>登録された帳簿項目ID(先頭のみ)</returns>
        protected override async Task<ActionIdObj> SaveAsync()
        {
            using FuncLog funcLog = new();

            ActionModel action = new() {
                Base = new(this.ActionId, this.SelectedDate, (this.SelectedBalanceKind == BalanceKind.Income ? 1 : -1) * this.InputedValue.Value),
                GroupId = this.GroupId,
                Book = new(this.SelectedBookVM.Id, string.Empty),
                Category = new(-1, string.Empty, this.SelectedBalanceKind),
                Item = new(this.SelectedItemVM.Id, string.Empty),
                Shop = new(this.SelectedShopName),
                Remark = new(this.SelectedRemark),
                IsMatch = this.SelectedIfMatch
            };

            ActionIdObj resActionId = await this.mService.SaveActionAsync(action, this.InputedCount, this.SelectedIfLink, this.SelectedHolidaySettingKind);

            if (action.Shop != null && action.Shop != string.Empty) {
                ShopModel shop = new(action.Shop) { ItemId = action.Item.Id, UsedTime = action.ActTime };
                await this.mService.SaveShopAsync(shop);
            }

            if (action.Remark != null && action.Remark != string.Empty) {
                RemarkModel remark = new(action.Remark) { ItemId = action.Item.Id, UsedTime = action.ActTime };
                await this.mService.SaveRemarkAsync(remark);
            }

            return resActionId;
        }
    }
}

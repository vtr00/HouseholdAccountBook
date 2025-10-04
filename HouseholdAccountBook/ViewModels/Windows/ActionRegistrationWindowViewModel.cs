using HouseholdAccountBook.Adapters.Dao.DbTable;
using HouseholdAccountBook.Adapters.DbHandler.Abstract;
using HouseholdAccountBook.Adapters.Dto.DbTable;
using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Others;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static HouseholdAccountBook.Views.UiConstants;

namespace HouseholdAccountBook.ViewModels.Windows
{
    /// <summary>
    /// 帳簿項目登録ウィンドウVM
    /// </summary>
    public class ActionRegistrationWindowViewModel : WindowViewModelBase
    {
        #region フィールド
        /// <summary>
        /// 変更時処理重複防止用フラグ
        /// </summary>
        private bool isUpdateOnChanged = false;
        #endregion

        #region イベント
        /// <summary>
        /// 帳簿変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<int?>> BookChanged = default;
        /// <summary>
        /// 日時変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<DateTime>> DateChanged = default;
        /// <summary>
        /// 収支変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<BalanceKind>> BalanceKindChanged = default;
        /// <summary>
        /// 分類変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<int?>> CategoryChanged = default;
        /// <summary>
        /// 項目変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<int?>> ItemChanged = default;

        /// <summary>
        /// 登録時イベント
        /// </summary>
        public event EventHandler<EventArgs<List<int>>> Registrated;
        #endregion

        #region Bindingプロパティ
        /// <summary>
        /// 登録モード
        /// </summary>
        #region RegKind
        public RegistrationKind RegKind
        {
            get => this._RegKind;
            set => this.SetProperty(ref this._RegKind, value);
        }
        private RegistrationKind _RegKind = RegistrationKind.Add;
        #endregion

        /// <summary>
        /// 帳簿項目ID
        /// </summary>
        #region ActionId
        public int? ActionId
        {
            get => this._ActionId;
            set => this.SetProperty(ref this._ActionId, value);
        }
        private int? _ActionId = default;
        #endregion
        /// <summary>
        /// グループID
        /// </summary>
        #region GroupId
        public int? GroupId
        {
            get => this._GroupId;
            set => this.SetProperty(ref this._GroupId, value);
        }
        private int? _GroupId = default;
        #endregion

        /// <summary>
        /// CSV比較からの追加
        /// </summary>
        #region AddedByCsvComparison
        public bool AddedByCsvComparison
        {
            get => this._AddedByCsvComparison;
            set => this.SetProperty(ref this._AddedByCsvComparison, value);
        }
        private bool _AddedByCsvComparison = default;
        #endregion

        /// <summary>
        /// 帳簿VMリスト
        /// </summary>
        #region BookVMList
        public ObservableCollection<BookViewModel> BookVMList
        {
            get => this._BookVMList;
            set => this.SetProperty(ref this._BookVMList, value);
        }
        private ObservableCollection<BookViewModel> _BookVMList = default;
        #endregion
        /// <summary>
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookViewModel SelectedBookVM
        {
            get => this._SelectedBookVM;
            set {
                var oldValue = this._SelectedBookVM;
                if (this.SetProperty(ref this._SelectedBookVM, value)) {
                    if (!this.isUpdateOnChanged) {
                        this.isUpdateOnChanged = true;
                        this.BookChanged?.Invoke(this, new() { OldValue = oldValue?.Id, NewValue = value?.Id });
                        this.isUpdateOnChanged = false;
                    }
                }
            }
        }
        private BookViewModel _SelectedBookVM = default;
        #endregion

        /// <summary>
        /// 選択された日時
        /// </summary>
        #region SelectedDate
        public DateTime SelectedDate
        {
            get => this._SelectedDate;
            set {
                var oldValue = this._SelectedDate;
                if (this.SetProperty(ref this._SelectedDate, value)) {
                    this.DateChanged?.Invoke(this, new() { OldValue = oldValue, NewValue = value });
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        private DateTime _SelectedDate = DateTime.Now;
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
        public BalanceKind SelectedBalanceKind
        {
            get => this._SelectedBalanceKind;
            set {
                var oldValue = this._SelectedBalanceKind;
                if (this.SetProperty(ref this._SelectedBalanceKind, value)) {
                    if (!this.isUpdateOnChanged) {
                        this.isUpdateOnChanged = true;
                        this.BalanceKindChanged?.Invoke(this, new() { OldValue = oldValue, NewValue = value });
                        this.isUpdateOnChanged = false;
                    }
                }
            }
        }
        private BalanceKind _SelectedBalanceKind = default;
        #endregion

        /// <summary>
        /// 分類VMリスト
        /// </summary>
        #region CategoryVMList
        public ObservableCollection<CategoryViewModel> CategoryVMList
        {
            get => this._CategoryVMList;
            set => this.SetProperty(ref this._CategoryVMList, value);
        }
        private ObservableCollection<CategoryViewModel> _CategoryVMList = default;
        #endregion
        /// <summary>
        /// 選択された分類VM
        /// </summary>
        #region SelectedCategoryVM
        public CategoryViewModel SelectedCategoryVM
        {
            get => this._SelectedCategoryVM;
            set {
                var oldValue = this._SelectedCategoryVM;
                if (this.SetProperty(ref this._SelectedCategoryVM, value)) {
                    if (!this.isUpdateOnChanged) {
                        this.isUpdateOnChanged = true;
                        this.CategoryChanged?.Invoke(this, new () { OldValue = oldValue?.Id, NewValue = value?.Id });
                        this.isUpdateOnChanged = false;
                    }
                }
            }
        }
        private CategoryViewModel _SelectedCategoryVM = default;
        #endregion

        /// <summary>
        /// 項目VMリスト
        /// </summary>
        #region ItemVMList
        public ObservableCollection<ItemViewModel> ItemVMList
        {
            get => this._ItemVMList;
            set => this.SetProperty(ref this._ItemVMList, value);
        }
        private ObservableCollection<ItemViewModel> _ItemVMList = default;
        #endregion
        /// <summary>
        /// 選択された項目VM
        /// </summary>
        #region SelectedItemVM
        public ItemViewModel SelectedItemVM
        {
            get => this._SelectedItemVM;
            set {
                var oldValue = this._SelectedItemVM;
                if (this.SetProperty(ref this._SelectedItemVM, value)) {
                    if (!this.isUpdateOnChanged) {
                        this.isUpdateOnChanged = true;
                        this.ItemChanged?.Invoke(this, new() { OldValue = oldValue?.Id, NewValue = value?.Id });
                        this.isUpdateOnChanged = false;
                    }
                }
            }
        }
        private ItemViewModel _SelectedItemVM = default;
        #endregion

        /// <summary>
        /// 金額
        /// </summary>
        #region Value
        public int? Value
        {
            get => this._Value;
            set {
                if (this.SetProperty(ref this._Value, value)) {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        private int? _Value = null;
        #endregion

        /// <summary>
        /// 店舗VMリスト
        /// </summary>
        #region ShopVMList
        public ObservableCollection<ShopViewModel> ShopVMList
        {
            get => this._ShopVMList;
            set => this.SetProperty(ref this._ShopVMList, value);
        }
        private ObservableCollection<ShopViewModel> _ShopVMList = default;
        #endregion
        /// <summary>
        /// 選択された店名
        /// </summary>
        #region SelectedShopName
        public string SelectedShopName
        {
            get => this._SelectedShopName;
            set => this.SetProperty(ref this._SelectedShopName, value);
        }
        private string _SelectedShopName = default;
        #endregion

        /// <summary>
        /// 備考VMリスト
        /// </summary>
        #region RemarkVMList
        public ObservableCollection<RemarkViewModel> RemarkVMList
        {
            get => this._RemarkVMList;
            set => this.SetProperty(ref this._RemarkVMList, value);
        }
        private ObservableCollection<RemarkViewModel> _RemarkVMList = default;
        #endregion
        /// <summary>
        /// 選択された備考
        /// </summary>
        #region SelectedRemark
        public string SelectedRemark
        {
            get => this._SelectedRemark;
            set => this.SetProperty(ref this._SelectedRemark, value);
        }
        private string _SelectedRemark = default;
        #endregion

        /// <summary>
        /// 回数
        /// </summary>
        #region Count
        public int Count
        {
            get => this._Count;
            set {
                if (this.SetProperty(ref this._Count, value)) {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        private int _Count = 1;
        #endregion

        /// <summary>
        /// 同じグループIDを持つ帳簿項目を連動して編集
        /// </summary>
        #region IsLink
        public bool IsLink
        {
            get => this._IsLink;
            set => this.SetProperty(ref this._IsLink, value);
        }
        private bool _IsLink = default;
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
        public HolidaySettingKind SelectedHolidaySettingKind
        {
            get => this._SelectedHolidaySettingKind;
            set => this.SetProperty(ref this._SelectedHolidaySettingKind, value);
        }
        private HolidaySettingKind _SelectedHolidaySettingKind = HolidaySettingKind.Nothing;
        #endregion

        /// <summary>
        /// 一致フラグ
        /// </summary>
        #region IsMatch
        public bool IsMatch
        {
            get => this._IsMatch;
            set => this.SetProperty(ref this._IsMatch, value);
        }
        private bool _IsMatch = default;
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
            int? id = null;
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                id = await this.SaveAsync();
            }

            // 呼び出し元更新
            List<int> value = id != null ? [id.Value] : [];
            this.Registrated?.Invoke(this, new EventArgs<List<int>>(value));

            // 表示クリア
            this.Value = null;
            this.Count = 1;
        }

        protected override bool OKCommand_CanExecute() => this.SelectedItemVM != null && this.Value.HasValue && 0 < this.Value;
        protected override async void OKCommand_Executed()
        {
            // DB登録
            int? id = null;
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                id = await this.SaveAsync();
            }

            // 呼び出し元更新
            List<int> value = id != null ? [id.Value] : [];
            this.Registrated?.Invoke(this, new EventArgs<List<int>>(value));

            base.OKCommand_Executed();
        }
        #endregion

        #region ウィンドウ設定プロパティ
        public override Size WindowSizeSetting
        {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return new Size(settings.ActionRegistrationWindow_Width, settings.ActionRegistrationWindow_Height);
            }
            set {
                Properties.Settings settings = Properties.Settings.Default;
                settings.ActionRegistrationWindow_Width = value.Width;
                settings.ActionRegistrationWindow_Height = value.Height;
                settings.Save();
            }
        }

        public override Point WindowPointSetting
        {
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

        public override async Task LoadAsync()
        {
            await this.LoadAsync(null, null, null, null, null);
        }

        /// <summary>
        /// DBから読み込む
        /// </summary>
        /// <param name="selectedBookId">選択された帳簿ID</param>
        /// <param name="selectedMonth">選択された年月</param>
        /// <param name="selectedDate">選択された日付</param>
        /// <param name="selectedRecord">選択されたCSVレコード</param>
        /// <param name="selectedActionId">選択された帳簿項目ID</param>
        public async Task LoadAsync(int? selectedBookId, DateTime? selectedMonth, DateTime? selectedDate, CsvViewModel selectedRecord, int? selectedActionId)
        {
            BalanceKind balanceKind = BalanceKind.Expenses;
            int? groupId = null;

            HstActionDto dto = new();
            switch (this.RegKind) {
                case RegistrationKind.Add: {
                    dto.BookId = selectedBookId ?? -1;
                    balanceKind = BalanceKind.Expenses;
                    if (selectedRecord == null) {
                        dto.ActTime = selectedDate ?? ((selectedMonth == null || selectedMonth?.Month == DateTime.Today.Month) ? DateTime.Today : selectedMonth.Value);
                    }
                    else {
                        dto.ActTime = selectedRecord.Date;
                        dto.ActValue = selectedRecord.Value;
                    }
                    break;
                }
                case RegistrationKind.Edit:
                case RegistrationKind.Copy: {
                    // DBから値を読み込む
                    await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                        HstActionDao hstActionDao = new(dbHandler);
                        dto = await hstActionDao.FindByIdAsync(selectedActionId.Value);
                    }
                    balanceKind = Math.Sign(dto.ActValue) > 0 ? BalanceKind.Income : BalanceKind.Expenses; // 収入 / 支出
                    groupId = dto.GroupId;

                    // 回数の表示
                    int count = 1;
                    if (groupId != null) {
                        await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                            HstActionDao hstActionDao = new(dbHandler);
                            var dtoList = await hstActionDao.FindInGroupAfterDateByIdAsync(selectedActionId.Value);
                            count = dtoList.Count();
                        }
                    }
                    this.Count = count;
                    break;
                }
            }

            // WVMに値を設定する
            if (this.RegKind == RegistrationKind.Edit) {
                this.ActionId = selectedActionId;
                this.GroupId = groupId;
            }
            this.SelectedBalanceKind = balanceKind;
            this.SelectedDate = dto.ActTime;
            this.Value = !(this.RegKind == RegistrationKind.Add && selectedRecord == null) ? Math.Abs(dto.ActValue) : (int?)null;
            this.IsMatch = dto.IsMatch == 1;

            // リストを更新する
            await this.UpdateBookListAsync(dto.BookId);
            await this.UpdateCategoryListAsync();
            await this.UpdateItemListAsync(dto.ItemId);
            await this.UpdateShopListAsync(dto.ShopName);
            await this.UpdateRemarkListAsync(dto.Remark);
        }

        /// <summary>
        /// 帳簿リストを更新する
        /// </summary>
        /// <param name="bookId">選択対象の帳簿ID</param>
        /// <returns></returns>
        private async Task UpdateBookListAsync(int? bookId = null)
        {
            ViewModelLoader loader = new(this.dbHandlerFactory);
            var tmpBookVMList = await loader.LoadBookListAsync();
            this.SelectedBookVM = tmpBookVMList.FirstOrElementAtOrDefault(vm => vm.Id == bookId, 0); // 先に選択しておく
            this.BookVMList = tmpBookVMList;
        }

        /// <summary>
        /// 分類リストを更新する
        /// </summary>
        /// <param name="categoryId">選択対象の分類ID</param>
        /// <returns></returns>
        private async Task UpdateCategoryListAsync(int? categoryId = null)
        {
            if (this.SelectedBookVM == null) return;

            ViewModelLoader loader = new(this.dbHandlerFactory);
            int? tmpCategoryId = categoryId ?? this.SelectedCategoryVM?.Id;
            var tmpCategoryVMList = await loader.LoadCategoryListAsync(this.SelectedBookVM.Id.Value, this.SelectedBalanceKind);
            this.SelectedCategoryVM = tmpCategoryVMList.FirstOrElementAtOrDefault(vm => vm.Id == tmpCategoryId, 0); // 先に選択しておく
            this.CategoryVMList = tmpCategoryVMList;
        }

        /// <summary>
        /// 項目リストを更新する
        /// </summary>
        /// <param name="itemId">選択対象の項目ID</param>
        /// <returns></returns>
        private async Task UpdateItemListAsync(int? itemId = null)
        {
            if (this.SelectedBookVM == null || this.SelectedCategoryVM == null) return;

            ViewModelLoader loader = new(this.dbHandlerFactory);
            int? tmpItemId = itemId ?? this.SelectedItemVM?.Id;
            var tmpItemVMList = await loader.LoadItemListAsync(this.SelectedBookVM.Id.Value, this.SelectedBalanceKind, this.SelectedCategoryVM.Id);
            this.SelectedItemVM = tmpItemVMList.FirstOrElementAtOrDefault(vm => vm.Id == tmpItemId, 0); // 先に選択しておく
            this.ItemVMList = tmpItemVMList;
        }

        /// <summary>
        /// 店舗リストを更新する
        /// </summary>
        /// <param name="shopName">選択対象の店舗名</param>
        /// <returns></returns>
        private async Task UpdateShopListAsync(string shopName = null)
        {
            if (this.SelectedItemVM == null) return;

            ViewModelLoader loader = new(this.dbHandlerFactory);
            string tmpShopName = shopName ?? this.SelectedShopName;
            var tmpShopVMList = await loader.LoadShopListAsync(this.SelectedItemVM.Id);
            this.SelectedShopName = tmpShopVMList.FirstOrElementAtOrDefault(vm => vm.Name == tmpShopName, 0).Name; // 先に選択しておく
            this.ShopVMList = tmpShopVMList;
        }

        /// <summary>
        /// 備考リストを更新する
        /// </summary>
        /// <param name="remark">選択対象の備考</param>
        /// <returns></returns>
        private async Task UpdateRemarkListAsync(string remark = null)
        {
            if (this.SelectedItemVM == null) return;

            ViewModelLoader loader = new(this.dbHandlerFactory);
            string tmpRemark = remark ?? this.SelectedRemark;
            var tmpRemarkVMList = await loader.LoadRemarkListAsync(this.SelectedItemVM.Id);
            this.SelectedRemark = tmpRemarkVMList.FirstOrElementAtOrDefault(vm => vm.Remark == tmpRemark, 0).Remark; // 先に選択しておく
            this.RemarkVMList = tmpRemarkVMList;
        }

        public override void AddEventHandlers()
        {
            this.BookChanged += async (sender, e) => {
                Log.Debug($"BookChanged old:{e.OldValue} new:{e.NewValue}");
                using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                    await this.UpdateCategoryListAsync();
                    await this.UpdateItemListAsync();
                }
            };
            this.BalanceKindChanged += async (sender, e) => {
                Log.Debug($"BalanceKindChanged old:{e.OldValue} new:{e.NewValue}");
                using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                    await this.UpdateCategoryListAsync();
                    await this.UpdateItemListAsync();
                }
            };
            this.CategoryChanged += async (sender, e) => {
                Log.Debug($"CategoryChanged old:{e.OldValue} new:{e.NewValue}");
                using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                    await this.UpdateItemListAsync();
                }
            };
            this.ItemChanged += async (sender, e) => {
                Log.Debug($"ItemChanged old:{e.OldValue} new:{e.NewValue}");
                using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                    await this.UpdateShopListAsync();
                    await this.UpdateRemarkListAsync();
                }
            };
        }

        /// <summary>
        /// DBに登録する
        /// </summary>
        /// <returns>登録された帳簿項目ID(先頭のみ)</returns>
        protected override async Task<int?> SaveAsync()
        {
            int actionId = this.ActionId ?? -1;
            int groupId = this.GroupId ?? -1;
            BalanceKind balanceKind = this.SelectedBalanceKind; // 収支種別
            int bookId = this.SelectedBookVM.Id.Value; // 帳簿ID
            int itemId = this.SelectedItemVM.Id; // 帳簿項目ID
            DateTime actTime = this.SelectedDate; // 入力日付
            int actValue = (balanceKind == BalanceKind.Income ? 1 : -1) * this.Value.Value; // 値
            string shopName = this.SelectedShopName; // 店舗名
            string remark = this.SelectedRemark; // 備考
            int count = this.Count; // 繰返し回数
            bool isLink = this.IsLink;
            int isMatch = this.IsMatch == true ? 1 : 0;
            HolidaySettingKind holidaySettingKind = this.SelectedHolidaySettingKind;

            int? resActionId = null;

            // 休日設定を考慮した日付を取得する関数
            DateTime getDateTimeWithHolidaySettingKind(DateTime tmpDateTime)
            {
                switch (holidaySettingKind) {
                    case HolidaySettingKind.BeforeHoliday:
                        while (tmpDateTime.IsNationalHoliday() || tmpDateTime.DayOfWeek == DayOfWeek.Saturday || tmpDateTime.DayOfWeek == DayOfWeek.Sunday) {
                            tmpDateTime = tmpDateTime.AddDays(-1);
                        }
                        break;
                    case HolidaySettingKind.AfterHoliday:
                        while (tmpDateTime.IsNationalHoliday() || tmpDateTime.DayOfWeek == DayOfWeek.Saturday || tmpDateTime.DayOfWeek == DayOfWeek.Sunday) {
                            tmpDateTime = tmpDateTime.AddDays(1);
                        }
                        break;
                }
                return tmpDateTime;
            }

            await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                await dbHandler.ExecTransactionAsync(async () => {
                    HstActionDao hstActionDao = new(dbHandler);
                    HstGroupDao hstGroupDao = new(dbHandler);

                    switch (this.RegKind) {
                        case RegistrationKind.Add:
                        case RegistrationKind.Copy: {
                            #region 帳簿項目を追加する
                            if (count == 1) { // 繰返し回数が1回(繰返しなし)
                                // 帳簿項目を追加する
                                resActionId = await hstActionDao.InsertReturningIdAsync(new HstActionDto {
                                    BookId = bookId,
                                    ItemId = itemId,
                                    ActTime = actTime,
                                    ActValue = actValue,
                                    ShopName = shopName,
                                    GroupId = null,
                                    Remark = remark,
                                    IsMatch = 0
                                });
                            }
                            else { // 繰返し回数が2回以上(繰返しあり)
                                // 新規グループIDを取得する
                                int tmpGroupId = await hstGroupDao.InsertReturningIdAsync(new HstGroupDto { GroupKind = (int)GroupKind.Repeat });

                                // 帳簿項目を追加する
                                DateTime tmpActTime = getDateTimeWithHolidaySettingKind(actTime); // 登録日付
                                for (int i = 0; i < count; ++i) {
                                    int id = await hstActionDao.InsertReturningIdAsync(new HstActionDto {
                                        BookId = bookId,
                                        ItemId = itemId,
                                        ActTime = tmpActTime,
                                        ActValue = actValue,
                                        ShopName = shopName,
                                        GroupId = tmpGroupId,
                                        Remark = remark
                                    });

                                    // 繰り返しの最初の1回を選択するようにする
                                    if (i == 0) {
                                        resActionId = id;
                                    }

                                    // 次の日付を取得する
                                    tmpActTime = getDateTimeWithHolidaySettingKind(actTime.AddMonths(i + 1));
                                }
                            }
                            #endregion
                            break;
                        }
                        case RegistrationKind.Edit: {
                            #region 帳簿項目を編集する
                            if (count == 1) {
                                #region 繰返し回数が1回
                                if (this.GroupId == null) {
                                    #region グループに属していない
                                    _ = await hstActionDao.UpdateAsync(new HstActionDto {
                                        BookId = bookId,
                                        ItemId = itemId,
                                        ActTime = actTime,
                                        ActValue = actValue,
                                        ShopName = shopName,
                                        GroupId = null,
                                        Remark = remark,
                                        IsMatch = isMatch,
                                        ActionId = actionId
                                    });
                                    #endregion
                                }
                                else {
                                    #region グループに属している
                                    // この帳簿項目以降の繰返し分のレコードを削除する
                                    _ = await hstActionDao.DeleteInGroupAfterDateByIdAsync(actionId, false);

                                    // グループに属する項目の個数を調べる
                                    var dtoList = await hstActionDao.FindByGroupIdAsync(groupId);

                                    if (dtoList.Count() <= 1) {
                                        #region グループに属する項目が1項目以下
                                        // この帳簿項目のグループIDをクリアする
                                        _ = await hstActionDao.UpdateAsync(new HstActionDto {
                                            BookId = bookId,
                                            ItemId = itemId,
                                            ActTime = actTime,
                                            ActValue = actValue,
                                            ShopName = shopName,
                                            GroupId = null,
                                            Remark = remark,
                                            IsMatch = isMatch,
                                            ActionId = actionId
                                        });

                                        // グループを削除する
                                        _ = await hstGroupDao.DeleteByIdAsync(groupId);
                                        #endregion
                                    }
                                    else {
                                        #region グループに属する項目が2項目以上
                                        // この帳簿項目のグループIDをクリアせずに残す(対象は過去分)
                                        _ = await hstActionDao.UpdateWithoutIsMatchAsync(new HstActionDto {
                                            BookId = bookId,
                                            ItemId = itemId,
                                            ActTime = actTime,
                                            ActValue = actValue,
                                            ShopName = shopName,
                                            GroupId = groupId,
                                            Remark = remark,
                                            IsMatch = 0, // 変更しない
                                            ActionId = actionId
                                        });
                                        #endregion
                                    }
                                    #endregion
                                }
                                #endregion
                            }
                            else {
                                #region 繰返し回数が2回以上
                                List<int> actionIdList = [];

                                if (this.GroupId == null) {
                                    #region グループIDが未割当て
                                    // グループIDを取得する
                                    groupId = await hstGroupDao.InsertReturningIdAsync(new HstGroupDto { GroupKind = (int)GroupKind.Repeat });
                                    actionIdList.Add(actionId);
                                    #endregion
                                }
                                else {
                                    #region グループIDが割当て済
                                    // 変更の対象となる帳簿項目を洗い出す
                                    var dtoList = await hstActionDao.FindInGroupAfterDateByIdAsync(actionId);
                                    dtoList.Select(dto => dto.ActionId).ToList().ForEach(actionIdList.Add);
                                    #endregion
                                }

                                DateTime tmpActTime = getDateTimeWithHolidaySettingKind(actTime);

                                // この帳簿項目にだけis_matchを反映する
                                Debug.Assert(actionIdList[0] == actionId);
                                _ = await hstActionDao.UpdateAsync(new HstActionDto {
                                    BookId = bookId,
                                    ItemId = itemId,
                                    ActTime = tmpActTime,
                                    ActValue = actValue,
                                    ShopName = shopName,
                                    GroupId = groupId,
                                    Remark = remark,
                                    IsMatch = isMatch,
                                    ActionId = actionId
                                });

                                tmpActTime = getDateTimeWithHolidaySettingKind(actTime.AddMonths(1)); // 登録日付
                                for (int i = 1; i < actionIdList.Count; ++i) {
                                    int targetActionId = actionIdList[i];

                                    if (i < count) { // 繰返し回数の範囲内のレコードを更新する
                                        // 連動して編集時のみ変更する
                                        if (isLink) {
                                            _ = await hstActionDao.UpdateWithoutIsMatchAsync(new HstActionDto {
                                                BookId = bookId,
                                                ItemId = itemId,
                                                ActTime = tmpActTime,
                                                ActValue = actValue,
                                                ShopName = shopName,
                                                GroupId = groupId,
                                                Remark = remark,
                                                IsMatch = 0, // 変更しない
                                                ActionId = targetActionId
                                            });
                                        }
                                    }
                                    else { // 繰返し回数が帳簿項目数を下回っていた場合に、越えたレコードを削除する
                                        _ = await hstActionDao.DeleteByIdAsync(targetActionId);
                                    }

                                    // 次の日付を取得する
                                    tmpActTime = getDateTimeWithHolidaySettingKind(actTime.AddMonths(i + 1));
                                }

                                // 繰返し回数が帳簿項目数を越えていた場合に、新規レコードを追加する
                                for (int i = actionIdList.Count; i < count; ++i) {
                                    _ = await hstActionDao.InsertReturningIdAsync(new HstActionDto {
                                        BookId = bookId,
                                        ItemId = itemId,
                                        ActTime = tmpActTime,
                                        ActValue = actValue,
                                        ShopName = shopName,
                                        GroupId = groupId,
                                        IsMatch = 0,
                                        Remark = remark
                                    });

                                    // 次の日付を取得する
                                    tmpActTime = getDateTimeWithHolidaySettingKind(actTime.AddMonths(i + 1));
                                }
                                #endregion
                            }

                            resActionId = actionId;
                            #endregion
                            break;
                        }
                    }
                });

                if (shopName != string.Empty) {
                    // 店舗を追加する
                    HstShopDao hstShopDao = new(dbHandler);
                    _ = await hstShopDao.UpsertAsync(new HstShopDto {
                        ItemId = itemId,
                        ShopName = shopName,
                        UsedTime = actTime
                    });
                }

                if (remark != string.Empty) {
                    // 備考を追加する
                    HstRemarkDao hstRemarkDao = new(dbHandler);
                    _ = await hstRemarkDao.UpsertAsync(new HstRemarkDto {
                        ItemId = itemId,
                        Remark = remark,
                        UsedTime = actTime
                    });
                }
            }

            return resActionId;
        }
    }
}

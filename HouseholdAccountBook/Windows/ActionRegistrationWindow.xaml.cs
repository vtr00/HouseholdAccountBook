using HouseholdAccountBook.Dao.Compositions;
using HouseholdAccountBook.Dao.DbTable;
using HouseholdAccountBook.DbHandler;
using HouseholdAccountBook.DbHandler.Abstract;
using HouseholdAccountBook.Dto.DbTable;
using HouseholdAccountBook.Dto.Others;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.UserEventArgs;
using HouseholdAccountBook.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static HouseholdAccountBook.Others.DbConstants;
using static HouseholdAccountBook.Others.LogicConstants;
using static HouseholdAccountBook.Extensions.FrameworkElementExtensions;

namespace HouseholdAccountBook.Windows
{
    /// <summary>
    /// ActionRegistrationWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ActionRegistrationWindow : Window
    {
        #region フィールド
        /// <summary>
        /// DBハンドラファクトリ
        /// </summary>
        private readonly DbHandlerFactory dbHandlerFactory;
        /// <summary>
        /// <see cref="MainWindow"/>で選択された帳簿ID
        /// </summary>
        private readonly int? selectedBookId;
        /// <summary>
        /// <see cref="MainWindow"/>で選択された表示年月
        /// </summary>
        private readonly DateTime? selectedMonth;
        /// <summary>
        /// <see cref="MainWindow"/>で選択された帳簿項目の日付
        /// </summary>
        private readonly DateTime? selectedDate;
        /// <summary>
        /// <see cref="CsvComparisonWindow"/>で選択されたCSVレコード
        /// </summary>
        private readonly CsvComparisonViewModel.CsvRecord selectedRecord;
        /// <summary>
        /// <see cref="MainWindow"/>で選択された帳簿項目ID
        /// </summary>
        private readonly int? selectedActionId;
        /// <summary>
        /// グループID
        /// </summary>
        private int? groupId = null;
        #endregion

        #region イベント
        /// <summary>
        /// 登録時のイベント
        /// </summary>
        public event EventHandler<EventArgs<List<int>>> Registrated = null;
        /// <summary>
        /// 帳簿変更時のイベント
        /// </summary>
        public event EventHandler<EventArgs<int?>> BookChanged = null;
        /// <summary>
        /// 日時変更時のイベント
        /// </summary>
        public event EventHandler<EventArgs<DateTime>> DateChanged = null;
        #endregion

        #region コンストラクタ
        /// <summary>
        /// 帳簿項目の新規登録のために <see cref="ActionRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="selectedBookId">選択された帳簿ID</param>
        /// <param name="selectedMonth">選択された年月</param>
        /// <param name="selectedDate">選択された日付</param>
        public ActionRegistrationWindow(DbHandlerFactory dbHandlerFactory, int? selectedBookId, DateTime? selectedMonth, DateTime? selectedDate)
        {
            this.dbHandlerFactory = dbHandlerFactory;
            this.selectedBookId = selectedBookId;
            this.selectedMonth = selectedMonth;
            this.selectedDate = selectedDate;
            this.selectedActionId = null;

            this.InitializeComponent();

            this.WVM.RegMode = RegistrationKind.Add;
        }

        /// <summary>
        /// 帳簿項目の新規登録のために <see cref="ActionRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="selectedBookId">選択された帳簿ID</param>
        /// <param name="selectedRecord">選択されたCSVレコード</param>
        public ActionRegistrationWindow(DbHandlerFactory dbHandlerFactory, int selectedBookId, CsvComparisonViewModel.CsvRecord selectedRecord)
        {
            this.dbHandlerFactory = dbHandlerFactory;
            this.selectedBookId = selectedBookId;
            this.selectedMonth = null;
            this.selectedDate = null;
            this.selectedRecord = selectedRecord;
            this.selectedActionId = null;

            this.InitializeComponent();

            this.WVM.RegMode = RegistrationKind.Add;
            this.WVM.AddedByCsvComparison = true;
        }

        /// <summary>
        /// 帳簿項目の編集(複製)のために <see cref="ActionRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="selectedActionId">帳簿項目ID</param>
        /// <param name="mode">登録モード</param>
        public ActionRegistrationWindow(DbHandlerFactory dbHandlerFactory, int selectedActionId, RegistrationKind mode = RegistrationKind.Edit)
        {
            this.dbHandlerFactory = dbHandlerFactory;
            this.selectedBookId = null;
            this.selectedMonth = null;
            this.selectedDate = null;
            switch (mode) {
                case RegistrationKind.Edit:
                case RegistrationKind.Copy:
                    this.selectedActionId = selectedActionId;
                    break;
            }

            this.InitializeComponent();

            this.WVM.RegMode = mode;
        }
        #endregion

        #region イベントハンドラ
        #region コマンド
        /// <summary>
        /// 今日コマンド操作可能判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TodayCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedDate != DateTime.Today;
        }

        /// <summary>
        /// 今日コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TodayCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.WVM.SelectedDate = DateTime.Today;
        }

        /// <summary>
        /// 続けて入力コマンド操作可能判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContinueToRegisterCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.RegMode != RegistrationKind.Edit && this.WVM.Value.HasValue && 0 < this.WVM.Value;
        }

        /// <summary>
        /// 続けて入力コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ContinueToRegisterCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // DB登録
            int? id = null;
            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                id = await this.RegisterToDbAsync();
            }

            // MainWindow更新
            List<int> value = id != null ? new List<int>() { id.Value } : new List<int>();
            Registrated?.Invoke(this, new EventArgs<List<int>>(value));

            // 表示クリア
            this.WVM.Value = null;
            this.WVM.Count = 1;
        }

        /// <summary>
        /// 登録コマンド操作可能判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegisterCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.Value.HasValue && 0 < this.WVM.Value;
        }

        /// <summary>
        /// 登録コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RegisterCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // DB登録
            int? id = null;
            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                id = await this.RegisterToDbAsync();
            }

            // MainWindow更新
            List<int> value = id != null ? new List<int>() { id.Value } : new List<int>();
            Registrated?.Invoke(this, new EventArgs<List<int>>(value));

            try {
                this.DialogResult = true;
            }
            catch (InvalidOperationException) { }
            this.Close();
        }

        /// <summary>
        /// キャンセルコマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try {
                this.DialogResult = false;
            }
            catch (InvalidOperationException) { }
            this.Close();
        }
        #endregion

        #region ウィンドウ
        /// <summary>
        /// ウィンドウ読込完了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ActionRegistrationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            BalanceKind balanceKind = BalanceKind.Expenses;

            HstActionDto dto = new HstActionDto();
            switch (this.WVM.RegMode) {
                case RegistrationKind.Add: {
                    dto.BookId = this.selectedBookId ?? -1;
                    balanceKind = BalanceKind.Expenses;
                    if (this.selectedRecord == null) {
                        dto.ActTime = this.selectedDate ?? ((this.selectedMonth == null || this.selectedMonth?.Month == DateTime.Today.Month) ? DateTime.Today : this.selectedMonth.Value);
                    }
                    else {
                        dto.ActTime = this.selectedRecord.Date;
                        dto.ActValue = this.selectedRecord.Value;
                    }
                    break;
                }
                case RegistrationKind.Edit:
                case RegistrationKind.Copy: {
                    // DBから値を読み込む
                    using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                        HstActionDao hstActionDao = new HstActionDao(dbHandler);
                        dto = await hstActionDao.FindByIdAsync(this.selectedActionId.Value);
                    }
                    balanceKind = Math.Sign(dto.ActValue) > 0 ? BalanceKind.Income : BalanceKind.Expenses; // 収入 / 支出
                    this.groupId = dto.GroupId;

                    // 回数の表示
                    int count = 1;
                    if (this.groupId != null) {
                        using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                            HstActionDao hstActionDao = new HstActionDao(dbHandler);
                            var dtoList = await hstActionDao.FindInGroupAfterDateByIdAsync(this.selectedActionId.Value);
                            count = dtoList.Count();
                        }
                    }
                    this.WVM.Count = count;
                    break;
                }
            }

            // WVMに値を設定する
            if (this.WVM.RegMode == RegistrationKind.Edit) {
                this.WVM.ActionId = this.selectedActionId;
                this.WVM.GroupId = this.groupId;
            }
            this.WVM.SelectedBalanceKind = balanceKind;
            this.WVM.SelectedDate = dto.ActTime;
            this.WVM.Value = !(this.WVM.RegMode == RegistrationKind.Add && this.selectedRecord == null) ? Math.Abs(dto.ActValue) : (int?)null;
            this.WVM.IsMatch = dto.IsMatch == 1;

            // リストを更新する
            await this.UpdateBookListAsync(dto.BookId);
            await this.UpdateCategoryListAsync();
            await this.UpdateItemListAsync(dto.ItemId);
            await this.UpdateShopListAsync(dto.ShopName);
            await this.UpdateRemarkListAsync(dto.Remark);

            // イベントハンドラを設定する
            this.RegisterEventHandlerToWVM();
        }

        /// <summary>
        /// ウィンドウ終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActionRegistrationWindow_Closed(object sender, EventArgs e)
        {
            this.SaveWindowSetting();
        }
        #endregion
        #endregion

        #region 画面更新用の関数
        /// <summary>
        /// 帳簿リストを更新する
        /// </summary>
        /// <param name="bookId">選択対象の帳簿ID</param>
        /// <returns></returns>
        private async Task UpdateBookListAsync(int? bookId = null)
        {
            // 帳簿を取得する
            ObservableCollection<BookViewModel> bookVMList = new ObservableCollection<BookViewModel>();
            BookViewModel selectedBookVM = null;
            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                MstBookDao mstBookDao = new MstBookDao(dbHandler);
                var dtoList = await mstBookDao.FindAllAsync();
                foreach (var dto in dtoList) {
                    BookViewModel vm = new BookViewModel() { Id = dto.BookId, Name = dto.BookName };
                    bookVMList.Add(vm);
                    if (selectedBookVM == null || bookId == vm.Id) {
                        selectedBookVM = vm;
                    }
                }
            }
            this.WVM.BookVMList = bookVMList;
            this.WVM.SelectedBookVM = selectedBookVM;
        }

        /// <summary>
        /// 分類リストを更新する
        /// </summary>
        /// <param name="categoryId">選択対象の分類ID</param>
        /// <returns></returns>
        private async Task UpdateCategoryListAsync(int? categoryId = null)
        {
            ObservableCollection<CategoryViewModel> categoryVMList = new ObservableCollection<CategoryViewModel>() {
                new CategoryViewModel() { Id = -1, Name = Properties.Resources.ListName_NoSpecification }
            };
            int? tmpCategoryId = categoryId ?? this.WVM.SelectedCategoryVM?.Id ?? categoryVMList[0].Id;
            CategoryViewModel selectedCategoryVM = categoryVMList[0];
            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                MstCategoryWithinBookDao mstCategoryWithinBookDao = new MstCategoryWithinBookDao(dbHandler);
                var dtoList = await mstCategoryWithinBookDao.FindByBookIdAndBalanceKindAsync(this.WVM.SelectedBookVM.Id.Value, (int)this.WVM.SelectedBalanceKind);
                foreach (var dto in dtoList) {
                    CategoryViewModel vm = new CategoryViewModel() { Id = dto.CategoryId, Name = dto.CategoryName };
                    categoryVMList.Add(vm);
                    if (selectedCategoryVM == null || vm.Id == tmpCategoryId) {
                        selectedCategoryVM = vm;
                    }
                }
            }
            this.WVM.CategoryVMList = categoryVMList;
            this.WVM.SelectedCategoryVM = selectedCategoryVM;
        }

        /// <summary>
        /// 項目リストを更新する
        /// </summary>
        /// <param name="itemId">選択対象の項目ID</param>
        /// <returns></returns>
        private async Task UpdateItemListAsync(int? itemId = null)
        {
            if (this.WVM.SelectedCategoryVM == null) return;

            ObservableCollection<ItemViewModel> itemVMList = new ObservableCollection<ItemViewModel>();
            int? tmpItemId = itemId ?? this.WVM.SelectedItemVM?.Id;
            ItemViewModel selectedItemVM = null;
            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                CategoryItemInfoDao categoryItemInfoDao = new CategoryItemInfoDao(dbHandler);
                var dtoList = this.WVM.SelectedCategoryVM.Id == -1
                    ? await categoryItemInfoDao.FindByBookIdAndBalanceKindAsync(this.WVM.SelectedBookVM.Id.Value, (int)this.WVM.SelectedBalanceKind)
                    : await categoryItemInfoDao.FindByBookIdAndCategoryIdAsync(this.WVM.SelectedBookVM.Id.Value, (int)this.WVM.SelectedCategoryVM.Id);
                foreach (CategoryItemInfoDto dto in dtoList) {
                    ItemViewModel vm = new ItemViewModel() {
                        Id = dto.ItemId,
                        Name = dto.ItemName,
                        CategoryName = this.WVM.SelectedCategoryVM.Id == -1 ? dto.CategoryName : ""
                    };
                    itemVMList.Add(vm);
                    if (selectedItemVM == null || vm.Id == tmpItemId) {
                        selectedItemVM = vm;
                    }
                }
            }
            this.WVM.ItemVMList = itemVMList;
            this.WVM.SelectedItemVM = selectedItemVM;
        }

        /// <summary>
        /// 店舗リストを更新する
        /// </summary>
        /// <param name="shopName">選択対象の店舗名</param>
        /// <returns></returns>
        private async Task UpdateShopListAsync(string shopName = null)
        {
            if (this.WVM.SelectedItemVM == null) return;

            ObservableCollection<ShopViewModel> shopNameVMList = new ObservableCollection<ShopViewModel>() {
                new ShopViewModel() { Name = String.Empty }
            };
            string selectedShopName = shopName ?? this.WVM.SelectedShopName ?? shopNameVMList[0].Name;
            ShopViewModel selectedShopVM = shopNameVMList[0]; // UNUSED
            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                ShopInfoDao shopInfoDao = new ShopInfoDao(dbHandler);
                var dtoList = await shopInfoDao.FindByItemIdAsync(this.WVM.SelectedItemVM.Id);
                foreach (ShopInfoDto dto in dtoList) {
                    ShopViewModel svm = new ShopViewModel() {
                        Name = dto.ShopName,
                        UsedCount = dto.Count,
                        UsedTime = dto.UsedTime
                    };
                    shopNameVMList.Add(svm);
                    if (selectedShopVM == null || svm.Name == selectedShopName) {
                        selectedShopVM = svm;
                    }
                }
            }

            this.WVM.ShopVMList = shopNameVMList;
            this.WVM.SelectedShopName = selectedShopName;
        }

        /// <summary>
        /// 備考リストを更新する
        /// </summary>
        /// <param name="remark">選択対象の備考</param>
        /// <returns></returns>
        private async Task UpdateRemarkListAsync(string remark = null)
        {
            if (this.WVM.SelectedItemVM == null) return;

            ObservableCollection<RemarkViewModel> remarkVMList = new ObservableCollection<RemarkViewModel>() {
                    new RemarkViewModel() { Remark = string.Empty }
            };
            string selectedRemark = remark ?? this.WVM.SelectedRemark ?? remarkVMList[0].Remark;
            RemarkViewModel selectedRemarkVM = remarkVMList[0]; // UNUSED
            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                RemarkInfoDao remarkInfoDao = new RemarkInfoDao(dbHandler);
                var dtoList = await remarkInfoDao.FindByItemIdAsync(this.WVM.SelectedItemVM.Id);
                foreach (RemarkInfoDto dto in dtoList) {
                    RemarkViewModel rvm = new RemarkViewModel() {
                        Remark = dto.Remark,
                        UsedCount = dto.Count,
                        UsedTime = dto.UsedTime
                    };
                    remarkVMList.Add(rvm);
                    if (selectedRemarkVM == null || rvm.Remark == selectedRemark) {
                        selectedRemarkVM = rvm;
                    }
                }
            }

            this.WVM.RemarkVMList = remarkVMList;
            this.WVM.SelectedRemark = selectedRemark;
        }
        #endregion

        /// <summary>
        /// イベントハンドラをWVMに登録する
        /// </summary>
        private void RegisterEventHandlerToWVM()
        {
            this.WVM.BookChanged += async (bookId) => {
                using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                    await this.UpdateCategoryListAsync();
                    await this.UpdateItemListAsync();
                    await this.UpdateShopListAsync();
                    await this.UpdateRemarkListAsync();
                }
                this.BookChanged?.Invoke(this, new EventArgs<int?>(bookId));
            };
            this.WVM.DateChanged += (date) => {
                this.DateChanged?.Invoke(this, new EventArgs<DateTime>(date));
            };
            this.WVM.BalanceKindChanged += async (_) => {
                using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                    await this.UpdateCategoryListAsync();
                    await this.UpdateItemListAsync();
                    await this.UpdateShopListAsync();
                    await this.UpdateRemarkListAsync();
                }
            };
            this.WVM.CategoryChanged += async (_) => {
                using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                    await this.UpdateItemListAsync();
                    await this.UpdateShopListAsync();
                    await this.UpdateRemarkListAsync();
                }
            };
            this.WVM.ItemChanged += async (_) => {
                using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                    await this.UpdateShopListAsync();
                    await this.UpdateRemarkListAsync();
                }
            };
        }

        /// <summary>
        /// DBに登録する
        /// </summary>
        /// <returns>登録された帳簿項目ID</returns>
        private async Task<int?> RegisterToDbAsync()
        {
            BalanceKind balanceKind = this.WVM.SelectedBalanceKind; // 収支種別
            int bookId = this.WVM.SelectedBookVM.Id.Value; // 帳簿ID
            int itemId = this.WVM.SelectedItemVM.Id; // 帳簿項目ID
            DateTime actTime = this.WVM.SelectedDate; // 入力日付
            int actValue = (balanceKind == BalanceKind.Income ? 1 : -1) * this.WVM.Value.Value; // 値
            string shopName = this.WVM.SelectedShopName; // 店舗名
            string remark = this.WVM.SelectedRemark; // 備考
            int count = this.WVM.Count; // 繰返し回数
            bool isLink = this.WVM.IsLink;
            int isMatch = this.WVM.IsMatch == true ? 1 : 0;
            HolidaySettingKind holidaySettingKind = this.WVM.SelectedHolidaySettingKind;

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

            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                await dbHandler.ExecTransactionAsync(async () => {
                    HstActionDao hstActionDao = new HstActionDao(dbHandler);
                    HstGroupDao hstGroupDao = new HstGroupDao(dbHandler);

                    switch (this.WVM.RegMode) {
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
                                if (this.groupId == null) {
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
                                        ActionId = this.selectedActionId.Value
                                    });
                                    #endregion
                                }
                                else {
                                    #region グループに属している
                                    // この帳簿項目以降の繰返し分のレコードを削除する
                                    _ = await hstActionDao.DeleteInGroupAfterDateByIdAsync(this.selectedActionId.Value, false);

                                    // グループに属する項目の個数を調べる
                                    var dtoList = await hstActionDao.FindByGroupIdAsync(this.groupId.Value);

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
                                            ActionId = this.selectedActionId.Value
                                        });

                                        // グループを削除する
                                        _ = await hstGroupDao.DeleteByIdAsync(this.groupId.Value);
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
                                            GroupId = this.groupId.Value,
                                            Remark = remark,
                                            IsMatch = 0, // 変更しない
                                            ActionId = this.selectedActionId.Value
                                        });
                                        #endregion
                                    }
                                    #endregion
                                }
                                #endregion
                            }
                            else {
                                #region 繰返し回数が2回以上
                                List<int> actionIdList = new List<int>();

                                if (this.groupId == null) {
                                    #region グループIDが未割当て
                                    // グループIDを取得する
                                    this.groupId = await hstGroupDao.InsertReturningIdAsync(new HstGroupDto { GroupKind = (int)GroupKind.Repeat });
                                    actionIdList.Add(this.selectedActionId.Value);
                                    #endregion
                                }
                                else {
                                    #region グループIDが割当て済
                                    // 変更の対象となる帳簿項目を洗い出す
                                    var dtoList = await hstActionDao.FindInGroupAfterDateByIdAsync(this.selectedActionId.Value);
                                    dtoList.Select(dto => dto.ActionId).ToList().ForEach(actionIdList.Add);
                                    #endregion
                                }

                                DateTime tmpActTime = getDateTimeWithHolidaySettingKind(actTime);

                                // この帳簿項目にだけis_matchを反映する
                                Debug.Assert(actionIdList[0] == this.selectedActionId);
                                _ = await hstActionDao.UpdateAsync(new HstActionDto {
                                    BookId = bookId,
                                    ItemId = itemId,
                                    ActTime = tmpActTime,
                                    ActValue = actValue,
                                    ShopName = shopName,
                                    GroupId = this.groupId,
                                    Remark = remark,
                                    IsMatch = isMatch,
                                    ActionId = this.selectedActionId.Value
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
                                                GroupId = this.groupId,
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
                                        GroupId = this.groupId,
                                        IsMatch = 0,
                                        Remark = remark
                                    });

                                    // 次の日付を取得する
                                    tmpActTime = getDateTimeWithHolidaySettingKind(actTime.AddMonths(i + 1));
                                }
                                #endregion
                            }

                            resActionId = this.selectedActionId;
                            #endregion
                            break;
                        }
                    }
                });

                if (shopName != string.Empty) {
                    // 店舗を追加する
                    HstShopDao hstShopDao = new HstShopDao(dbHandler);
                    _ = await hstShopDao.UpsertAsync(new HstShopDto {
                        ItemId = itemId,
                        ShopName = shopName,
                        UsedTime = actTime
                    });
                }

                if (remark != string.Empty) {
                    // 備考を追加する
                    HstRemarkDao hstRemarkDao = new HstRemarkDao(dbHandler);
                    _ = await hstRemarkDao.UpsertAsync(new HstRemarkDto {
                        ItemId = itemId,
                        Remark = remark,
                        UsedTime = actTime
                    });
                }
            }

            return resActionId;
        }

        #region 設定反映用の関数
        /// <summary>
        /// ウィンドウ設定を読み込む
        /// </summary>
        public void LoadWindowSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (settings.ActionRegistrationWindow_Width != -1 && settings.ActionRegistrationWindow_Height != -1) {
                this.Width = settings.ActionRegistrationWindow_Width;
                this.Height = settings.ActionRegistrationWindow_Height;
            }

            if (settings.App_IsPositionSaved && (-10 <= settings.ActionRegistrationWindow_Left && 0 <= settings.ActionRegistrationWindow_Top)) {
                this.Left = settings.ActionRegistrationWindow_Left;
                this.Top = settings.ActionRegistrationWindow_Top;
            }
            else {
                this.MoveOwnersCenter();
            }
        }

        /// <summary>
        /// ウィンドウ設定を保存する
        /// </summary>
        public void SaveWindowSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (this.WindowState == WindowState.Normal) {
                if (settings.App_IsPositionSaved) {
                    settings.ActionRegistrationWindow_Left = this.Left;
                    settings.ActionRegistrationWindow_Top = this.Top;
                }
                settings.ActionRegistrationWindow_Width = this.Width;
                settings.ActionRegistrationWindow_Height = this.Height;
                settings.Save();
            }
        }
        #endregion
    }
}

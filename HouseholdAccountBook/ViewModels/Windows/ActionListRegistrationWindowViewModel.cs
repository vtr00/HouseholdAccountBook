using HouseholdAccountBook.Adapters.Dao.DbTable;
using HouseholdAccountBook.Adapters.DbHandler.Abstract;
using HouseholdAccountBook.Adapters.Dto.DbTable;
using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Others;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using HouseholdAccountBook.Views.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static HouseholdAccountBook.Views.UiConstants;

namespace HouseholdAccountBook.ViewModels.Windows
{
    /// <summary>
    /// 帳簿項目リスト登録ウィンドウVM
    /// </summary>
    public class ActionListRegistrationWindowViewModel : WindowViewModelBase
    {
        #region フィールド
        /// <summary>
        /// 変更時処理重複防止用フラグ
        /// </summary>
        private bool mIsUpdateOnChanged;
        #endregion

        #region イベント
        /// <summary>
        /// 帳簿変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<int?>> BookChanged;
        /// <summary>
        /// 収支変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<BalanceKind>> BalanceKindChanged;
        /// <summary>
        /// 分類変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<int?>> CategoryChanged;
        /// <summary>
        /// 項目変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<int?>> ItemChanged;

        /// <summary>
        /// 登録時イベント
        /// </summary>
        public event EventHandler<EventArgs<List<int>>> Registrated;
        #endregion

        #region Bindingプロパティ
        /// <summary>
        /// 登録種別
        /// </summary>
        #region RegKind
        public RegistrationKind RegKind {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// グループID
        /// </summary>
        #region GroupId
        public int? GroupId {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 帳簿VMリスト
        /// </summary>
        #region BookVMList
        public ObservableCollection<BookViewModel> BookVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookViewModel SelectedBookVM {
            get;
            set {
                var oldVM = field;
                if (this.SetProperty(ref field, value)) {
                    if (!this.mIsUpdateOnChanged) {
                        this.mIsUpdateOnChanged = true;
                        this.BookChanged?.Invoke(this, new() { OldValue = oldVM?.Id, NewValue = value?.Id });
                        this.mIsUpdateOnChanged = false;
                    }
                }
            }
        }
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
        public ObservableCollection<CategoryViewModel> CategoryVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 選択された分類VM
        /// </summary>
        #region SelectedCategoryVM
        public CategoryViewModel SelectedCategoryVM {
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
        public ObservableCollection<ItemViewModel> ItemVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 選択された項目VM
        /// </summary>
        #region SelectedItemVM
        public ItemViewModel SelectedItemVM {
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
        /// 日付金額VMリスト
        /// </summary>
        #region DateValueVMList
        public ObservableCollection<DateValueViewModel> DateValueVMList {
            get;
            set => this.SetProperty(ref field, value);
        } = [];
        #endregion

        /// <summary>
        /// 編集中か
        /// </summary>
        #region IsEditing
        public bool IsEditing {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 日付自動インクリメント
        /// </summary>
        #region IsDateAutoIncrement
        public bool IsDateAutoIncrement {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 店舗VMリスト
        /// </summary>
        #region ShopNameList
        public ObservableCollection<ShopViewModel> ShopVMList {
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
        public ObservableCollection<RemarkViewModel> RemarkVMList {
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
        /// 数値入力ボタンの入力値
        /// </summary>
        public int? InputedValue { get; set; }
        /// <summary>
        /// 数値入力ボタンの入力種別
        /// </summary>
        public NumericInputButton.InputKind InputedKind { get; set; }
        #endregion

        #region コマンドイベントハンドラ
        protected override bool OKCommand_CanExecute() => this.SelectedItemVM != null && this.DateValueVMList.Any(static vm => vm.ActValue.HasValue);
        protected override async void OKCommand_Executed()
        {
            // DB登録
            List<int> idList = null;
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                idList = await this.SaveAsync();
            }

            // MainWindow更新
            this.Registrated?.Invoke(this, new(idList ?? []));

            base.OKCommand_Executed();
        }
        #endregion

        #region ウィンドウ設定プロパティ
        protected override (double, double) WindowSizeSettingRaw {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return (settings.ActionListRegistrationWindow_Width, settings.ActionListRegistrationWindow_Height);
            }
            set {
                Properties.Settings settings = Properties.Settings.Default;
                settings.ActionListRegistrationWindow_Width = value.Item1;
                settings.ActionListRegistrationWindow_Height = value.Item2;
                settings.Save();
            }
        }

        public override Point WindowPointSetting {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return new Point(settings.ActionListRegistrationWindow_Left, settings.ActionListRegistrationWindow_Top);
            }
            set {
                Properties.Settings settings = Properties.Settings.Default;
                settings.ActionListRegistrationWindow_Left = value.X;
                settings.ActionListRegistrationWindow_Top = value.Y;
                settings.Save();
            }
        }
        #endregion

        public override async Task LoadAsync() => await this.LoadAsync(null, null, null, null, null);

        /// <summary>
        /// DBから読み込む
        /// </summary>
        /// <param name="selectedBookId">選択された帳簿ID</param>
        /// <param name="selectedMonth">選択された年月</param>
        /// <param name="selectedDate">選択された日付</param>
        /// <param name="selectedRecordList">選択されたCSVレコードリスト</param>
        /// <param name="selectedGroupId">選択されたグループID</param>
        public async Task LoadAsync(int? selectedBookId, DateTime? selectedMonth, DateTime? selectedDate, List<CsvViewModel> selectedRecordList, int? selectedGroupId)
        {
            using FuncLog funcLog = new(new { selectedBookId, selectedMonth, selectedDate, selectedRecordList, selectedGroupId });

            int? bookId = null;
            BalanceKind balanceKind = BalanceKind.Expenses;

            int? itemId = null;
            string shopName = null;
            string remark = null;

            switch (this.RegKind) {
                case RegistrationKind.Add: {
                    bookId = selectedBookId;
                    balanceKind = BalanceKind.Expenses;
                    DateTime actDate = selectedDate ?? ((selectedMonth == null || selectedMonth?.Month == DateTime.Today.Month) ? DateTime.Today : selectedMonth.Value);

                    if (selectedRecordList == null) {
                        this.DateValueVMList.Add(new DateValueViewModel() { ActDate = actDate });
                    }
                    else {
                        foreach (CsvViewModel record in selectedRecordList) {
                            this.DateValueVMList.Add(new DateValueViewModel() { ActDate = record.Date, ActValue = record.Value });
                        }
                    }
                }
                break;
                case RegistrationKind.Edit:
                case RegistrationKind.Copy: {
                    // DBから値を読み込む
                    await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                        HstActionDao hstActionDao = new(dbHandler);
                        var dtoList = await hstActionDao.FindByGroupIdAsync(selectedGroupId.Value);
                        foreach (HstActionDto dto in dtoList) {
                            // 日付金額リストに追加
                            DateValueViewModel vm = new() {
                                ActionId = dto.ActionId,
                                ActDate = dto.ActTime,
                                ActValue = Math.Abs(dto.ActValue)
                            };

                            bookId = dto.BookId;
                            itemId = dto.ItemId;
                            shopName = dto.ShopName;
                            remark = dto.Remark;

                            balanceKind = Math.Sign(dto.ActValue) > 0 ? BalanceKind.Income : BalanceKind.Expenses; // 収入 / 支出

                            this.DateValueVMList.Add(vm);
                        }
                    }
                }
                break;
            }

            // WVMに値を設定する
            this.GroupId = this.RegKind == RegistrationKind.Edit ? selectedGroupId : null;
            this.SelectedBalanceKind = balanceKind;

            // リストを更新する
            await this.UpdateBookListAsync(bookId);
            await this.UpdateCategoryListAsync();
            await this.UpdateItemListAsync(itemId);
            await this.UpdateShopListAsync(shopName);
            await this.UpdateRemarkListAsync(remark);
        }

        /// <summary>
        /// 帳簿リストを更新する
        /// </summary>
        /// <param name="bookId">選択対象の帳簿ID</param>
        /// <returns></returns>
        private async Task UpdateBookListAsync(int? bookId = null)
        {
            using FuncLog funcLog = new(new { bookId });

            ViewModelLoader loader = new(this.dbHandlerFactory);
            int? tmpBookId = bookId ?? this.SelectedBookVM?.Id;
            this.BookVMList = await loader.LoadBookListAsync();
            this.SelectedBookVM = this.BookVMList.FirstOrElementAtOrDefault(vm => vm.Id == tmpBookId, 0);
        }

        /// <summary>
        /// 分類リストを更新する
        /// </summary>
        /// <param name="categoryId">選択対象の分類ID</param>
        /// <returns></returns>
        private async Task UpdateCategoryListAsync(int? categoryId = null)
        {
            if (this.SelectedBookVM == null) { return; }

            using FuncLog funcLog = new(new { categoryId });

            ViewModelLoader loader = new(this.dbHandlerFactory);
            int? tmpCategoryId = categoryId ?? this.SelectedCategoryVM?.Id;
            this.CategoryVMList = await loader.LoadCategoryListAsync(this.SelectedBookVM.Id.Value, this.SelectedBalanceKind);
            this.SelectedCategoryVM = this.CategoryVMList.FirstOrElementAtOrDefault(vm => vm.Id == tmpCategoryId, 0);
        }

        /// <summary>
        /// 項目リストを更新する
        /// </summary>
        /// <param name="itemId">選択対象の項目ID</param>
        /// <returns></returns>
        private async Task UpdateItemListAsync(int? itemId = null)
        {
            if (this.SelectedBookVM == null || this.SelectedCategoryVM == null) { return; }

            using FuncLog funcLog = new(new { itemId });

            ViewModelLoader loader = new(this.dbHandlerFactory);
            int? tmpItemId = itemId ?? this.SelectedItemVM?.Id;
            this.ItemVMList = await loader.LoadItemListAsync(this.SelectedBookVM.Id.Value, this.SelectedBalanceKind, this.SelectedCategoryVM.Id);
            this.SelectedItemVM = this.ItemVMList.FirstOrElementAtOrDefault(vm => vm.Id == tmpItemId, 0);
        }

        /// <summary>
        /// 店舗リストを更新する
        /// </summary>
        /// <param name="shopName">選択対象の店舗名</param>
        /// <returns></returns>
        private async Task UpdateShopListAsync(string shopName = null)
        {
            if (this.SelectedItemVM == null) { return; }

            using FuncLog funcLog = new(new { shopName });

            ViewModelLoader loader = new(this.dbHandlerFactory);
            string tmpShopName = shopName ?? this.SelectedShopName;
            this.ShopVMList = await loader.LoadShopListAsync(this.SelectedItemVM.Id); ;
            this.SelectedShopName = this.ShopVMList.FirstOrElementAtOrDefault(vm => vm.Name == tmpShopName, 0).Name;
        }

        /// <summary>
        /// 備考リストを更新する
        /// </summary>
        /// <param name="remark">選択対象の備考</param>
        /// <returns></returns>
        private async Task UpdateRemarkListAsync(string remark = null)
        {
            if (this.SelectedItemVM == null) { return; }

            using FuncLog funcLog = new(new { remark });

            ViewModelLoader loader = new(this.dbHandlerFactory);
            string tmpRemark = remark ?? this.SelectedRemark;
            this.RemarkVMList = await loader.LoadRemarkListAsync(this.SelectedItemVM.Id);
            this.SelectedRemark = this.RemarkVMList.FirstOrElementAtOrDefault(vm => vm.Remark == tmpRemark, 0).Remark;
        }

        public override void AddEventHandlers()
        {
            using FuncLog funcLog = new();

            this.BookChanged += async (sender, e) => {
                using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, methodName: nameof(this.BookChanged));

                using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                    await this.UpdateCategoryListAsync();
                    await this.UpdateItemListAsync();
                }
            };
            this.BalanceKindChanged += async (sender, e) => {
                using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, methodName: nameof(this.BalanceKindChanged));

                using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                    await this.UpdateCategoryListAsync();
                    await this.UpdateItemListAsync();
                }
            };
            this.CategoryChanged += async (sender, e) => {
                using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, methodName: nameof(this.CategoryChanged));

                using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                    await this.UpdateItemListAsync();
                }
            };
            this.ItemChanged += async (sender, e) => {
                using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, methodName: nameof(this.ItemChanged));

                using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                    await this.UpdateShopListAsync();
                    await this.UpdateRemarkListAsync();
                }
            };
        }

        /// <summary>
        /// DBに登録する
        /// </summary>
        /// <returns>登録された帳簿項目IDリスト</returns>
        protected override async Task<List<int>> SaveAsync()
        {
            using FuncLog funcLog = new();

            List<int> resActionIdList = [];

            BalanceKind balanceKind = this.SelectedBalanceKind; // 収支種別
            int bookId = this.SelectedBookVM.Id.Value;          // 帳簿ID
            int itemId = this.SelectedItemVM.Id;                // 帳簿項目ID
            int groupId = this.GroupId ?? -1;                   // グループID
            string shopName = this.SelectedShopName;            // 店舗名
            string remark = this.SelectedRemark;                // 備考

            DateTime lastActTime = this.DateValueVMList.Max(tmp => tmp.ActDate);
            await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                switch (this.RegKind) {
                    case RegistrationKind.Add: {
                        #region 帳簿項目を追加する
                        await dbHandler.ExecTransactionAsync(async () => {
                            // グループIDを取得する
                            HstGroupDao hstGroupDao = new(dbHandler);
                            int tmpGroupId = await hstGroupDao.InsertReturningIdAsync(new HstGroupDto { GroupKind = (int)GroupKind.ListReg });

                            HstActionDao hstActionDao = new(dbHandler);
                            foreach (DateValueViewModel vm in this.DateValueVMList) {
                                if (vm.ActValue.HasValue) {
                                    DateTime actTime = vm.ActDate; // 日付
                                    int actValue = (balanceKind == BalanceKind.Income ? 1 : -1) * vm.ActValue.Value; // 金額
                                    int id = await hstActionDao.InsertReturningIdAsync(new HstActionDto {
                                        BookId = bookId,
                                        ItemId = itemId,
                                        ActTime = actTime,
                                        ActValue = actValue,
                                        ShopName = shopName,
                                        GroupId = tmpGroupId,
                                        Remark = remark
                                    });
                                    resActionIdList.Add(id);
                                }
                            }
                        });
                        #endregion
                        break;
                    }
                    case RegistrationKind.Edit: {
                        #region 帳簿項目を編集する
                        await dbHandler.ExecTransactionAsync(async () => {
                            HstActionDao hstActionDao = new(dbHandler);
                            foreach (DateValueViewModel vm in this.DateValueVMList) {
                                if (vm.ActValue.HasValue) {
                                    int? actionId = vm.ActionId;
                                    DateTime actTime = vm.ActDate; // 日付
                                    int actValue = (balanceKind == BalanceKind.Income ? 1 : -1) * vm.ActValue.Value; // 金額

                                    if (actionId.HasValue) {
                                        _ = await hstActionDao.UpdateWithoutIsMatchAsync(new HstActionDto {
                                            BookId = bookId,
                                            ItemId = itemId,
                                            ActTime = actTime,
                                            ActValue = actValue,
                                            ShopName = shopName,
                                            Remark = remark,
                                            ActionId = actionId.Value
                                        });

                                        resActionIdList.Add(actionId.Value);
                                    }
                                    else {
                                        int id = await hstActionDao.InsertReturningIdAsync(new HstActionDto {
                                            BookId = bookId,
                                            ItemId = itemId,
                                            ActTime = actTime,
                                            ActValue = actValue,
                                            ShopName = shopName,
                                            GroupId = groupId,
                                            Remark = remark
                                        });
                                        resActionIdList.Add(id);
                                    }
                                }
                            }

                            var dtoList = await hstActionDao.FindByGroupIdAsync(this.GroupId.Value);
                            IEnumerable<int> expected = dtoList.Select(dto => dto.ActionId).Except(resActionIdList);
                            foreach (int actionId in expected) {
                                _ = await hstActionDao.DeleteByIdAsync(actionId);
                            }
                        });
                        #endregion
                        break;
                    }
                }

                if (shopName != string.Empty) {
                    // 店舗を追加する
                    HstShopDao hstShopDao = new(dbHandler);
                    _ = await hstShopDao.UpsertAsync(new HstShopDto {
                        ItemId = itemId,
                        ShopName = shopName,
                        UsedTime = lastActTime
                    });
                }

                if (remark != string.Empty) {
                    // 備考を追加する
                    HstRemarkDao hstRemarkDao = new(dbHandler);
                    _ = await hstRemarkDao.UpsertAsync(new HstRemarkDto {
                        ItemId = itemId,
                        Remark = remark,
                        UsedTime = lastActTime
                    });
                }
            }

            return resActionIdList;
        }
    }
}

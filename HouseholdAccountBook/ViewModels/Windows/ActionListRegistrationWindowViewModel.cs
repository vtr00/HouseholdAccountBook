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
            set {
                if (value == RegistrationKind.Copy) {
                    throw new NotSupportedException("登録種別にCopyは使用できません。");
                }
                _ = this.SetProperty(ref field, value);
            }
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
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
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
        /// <param name="initialBookId">追加時、初期選択する帳簿のID</param>
        /// <param name="initialMonth">追加時、初期選択する年月</param>
        /// <param name="initialDate">追加時、初期選択する日付</param>
        /// <param name="initialRecordList">追加時、初期表示するCSVレコードリスト</param>
        /// <param name="targetGroupId">編集時、編集対象のグループID</param>
        public async Task LoadAsync(int? initialBookId, DateTime? initialMonth, DateTime? initialDate, List<CsvViewModel> initialRecordList, int? targetGroupId)
        {
            using FuncLog funcLog = new(new { initialBookId, initialMonth, initialDate, initialRecordList, targetGroupId });

            int? selectingBookId = null;
            BalanceKind selectingBalanceKind = BalanceKind.Expenses;
            int? selectingItemId = null;
            string selectingShopName = null;
            string selectingRemark = null;

            switch (this.RegKind) {
                case RegistrationKind.Add: {
                    selectingBookId = initialBookId;

                    // WVMに値を設定する
                    if (initialRecordList == null) {
                        DateTime actDate = initialDate ?? ((initialMonth == null || initialMonth?.Month == DateTime.Today.Month) ? DateTime.Today : initialMonth.Value);
                        this.DateValueVMList.Add(new DateValueViewModel() { ActDate = actDate });
                    }
                    else {
                        foreach (CsvViewModel record in initialRecordList) {
                            this.DateValueVMList.Add(new DateValueViewModel() { ActDate = record.Date, ActValue = record.Value });
                        }
                    }

                    break;
                }
                case RegistrationKind.Edit: {
                    List<DateValueViewModel> dateValueVMList = [];

                    // DBから値を読み込む
                    await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                        HstActionDao hstActionDao = new(dbHandler);
                        var dtoList = await hstActionDao.FindByGroupIdAsync(targetGroupId.Value);

                        foreach (HstActionDto dto in dtoList) {
                            // 日付金額リストに追加
                            DateValueViewModel vm = new() {
                                ActionId = dto.ActionId,
                                ActDate = dto.ActTime,
                                ActValue = Math.Abs(dto.ActValue)
                            };

                            selectingBookId = dto.BookId;
                            selectingItemId = dto.ItemId;
                            selectingShopName = dto.ShopName;
                            selectingRemark = dto.Remark;

                            selectingBalanceKind = Math.Sign(dto.ActValue) > 0 ? BalanceKind.Income : BalanceKind.Expenses; // 収入 / 支出

                            dateValueVMList.Add(vm);
                        }
                    }

                    // WVMに値を設定する
                    this.GroupId = targetGroupId;
                    foreach (DateValueViewModel vm in dateValueVMList) {
                        this.DateValueVMList.Add(vm);
                    }

                    break;
                }
                case RegistrationKind.Copy:
                    throw new NotSupportedException("登録種別にCopyは使用できません。");
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
        private async Task UpdateBookListAsync(int? selectingBookId = null)
        {
            using FuncLog funcLog = new(new { selectingBookId });

            ViewModelLoader loader = new(this.mDbHandlerFactory);
            int? tmpBookId = selectingBookId ?? this.SelectedBookVM?.Id;
            this.BookVMList = await loader.LoadBookListAsync();
            this.SelectedBookVM = this.BookVMList.FirstOrElementAtOrDefault(vm => vm.Id == tmpBookId, 0);
        }

        /// <summary>
        /// 分類リストを更新する
        /// </summary>
        /// <param name="selectingCategoryId">選択対象の分類ID</param>
        /// <returns></returns>
        private async Task UpdateCategoryListAsync(int? selectingCategoryId = null)
        {
            if (this.SelectedBookVM == null) { return; }

            using FuncLog funcLog = new(new { selectingCategoryId });

            ViewModelLoader loader = new(this.mDbHandlerFactory);
            int? tmpCategoryId = selectingCategoryId ?? this.SelectedCategoryVM?.Id;
            this.CategoryVMList = await loader.LoadCategoryListAsync(this.SelectedBookVM.Id.Value, this.SelectedBalanceKind);
            this.SelectedCategoryVM = this.CategoryVMList.FirstOrElementAtOrDefault(vm => vm.Id == tmpCategoryId, 0);
        }

        /// <summary>
        /// 項目リストを更新する
        /// </summary>
        /// <param name="selectingItemId">選択対象の項目ID</param>
        /// <returns></returns>
        private async Task UpdateItemListAsync(int? selectingItemId = null)
        {
            if (this.SelectedBookVM == null || this.SelectedCategoryVM == null) { return; }

            using FuncLog funcLog = new(new { selectingItemId });

            ViewModelLoader loader = new(this.mDbHandlerFactory);
            int? tmpItemId = selectingItemId ?? this.SelectedItemVM?.Id;
            this.ItemVMList = await loader.LoadItemListAsync(this.SelectedBookVM.Id.Value, this.SelectedBalanceKind, this.SelectedCategoryVM.Id);
            this.SelectedItemVM = this.ItemVMList.FirstOrElementAtOrDefault(vm => vm.Id == tmpItemId, 0);
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

            ViewModelLoader loader = new(this.mDbHandlerFactory);
            string tmpShopName = selectingShopName ?? this.SelectedShopName;
            this.ShopVMList = await loader.LoadShopListAsync(this.SelectedItemVM.Id); ;
            this.SelectedShopName = this.ShopVMList.FirstOrElementAtOrDefault(vm => vm.Name == tmpShopName, 0).Name;
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

            ViewModelLoader loader = new(this.mDbHandlerFactory);
            string tmpRemark = selectingRemark ?? this.SelectedRemark;
            this.RemarkVMList = await loader.LoadRemarkListAsync(this.SelectedItemVM.Id);
            this.SelectedRemark = this.RemarkVMList.FirstOrElementAtOrDefault(vm => vm.Remark == tmpRemark, 0).Remark;
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
            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
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
                    // 店舗を追加/更新する
                    HstShopDao hstShopDao = new(dbHandler);
                    _ = await hstShopDao.UpsertAsync(new HstShopDto {
                        ItemId = itemId,
                        ShopName = shopName,
                        UsedTime = lastActTime
                    });
                }

                if (remark != string.Empty) {
                    // 備考を追加/更新する
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

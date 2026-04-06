using HouseholdAccountBook.Infrastructure.CSV;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities.Args;
using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using HouseholdAccountBook.Views;
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
        public event EventHandler<ChangedEventArgs<BookIdObj>> BookChanged;
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
        public SelectorViewModel<BookModel, BookIdObj> BookSelectorVM { get; } = new(static vm => vm?.Id);

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
        /// 入力された日付金額VMリスト
        /// </summary>
        public ObservableCollection<DateValueViewModel> InputedDateValueVMList {
            get;
            set => this.SetProperty(ref field, value);
        } = [];

        /// <summary>
        /// 編集中か
        /// </summary>
        public bool IsEditing {
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
        public SelectorViewModel<ShopModel, string> ShopSelectorVM { get; } = new(static vm => vm?.Name);

        /// <summary>
        /// 備考セレクタVM
        /// </summary>
        public SelectorViewModel<RemarkModel, string> RemarkSelectorVM { get; } = new(static vm => vm?.Remark);

        /// <summary>
        /// 数値入力ボタンの入力値
        /// </summary>
        public int? InputedValue { get; set; }
        /// <summary>
        /// 数値入力ボタンの入力種別
        /// </summary>
        public NumericInputButton.InputKind InputedKind { get; set; }

        #region コマンド
        /// <summary>
        /// OKコマンド
        /// </summary>
        public new ICommand OKCommand => new AsyncRelayCommand(this.OKCommand_ExecuteAsync, this.OKCommand_CanExecute);
        #endregion
        #endregion

        #region コマンドイベントハンドラ
        protected override bool OKCommand_CanExecute() => this.ItemSelectorVM.SelectedKey != null && this.InputedDateValueVMList.Any(static vm => vm.ActValue is not null and not 0);
        protected async Task OKCommand_ExecuteAsync()
        {
            // DB登録
            IEnumerable<ActionIdObj> idList = null;
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                idList = await this.SaveAsync();
            }

            // MainWindow更新
            this.Registrated?.Invoke(this, new(idList));

            base.OKCommand_Execute();
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

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ActionListRegistrationWindowViewModel()
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
                () => this.ItemSelectorVM.SelectedKey != null);
            this.RemarkSelectorVM.SetLoader(
                async () => await this.mAppService.LoadRemarkListAsync(this.ItemSelectorVM.SelectedKey, true),
                () => this.ItemSelectorVM.SelectedKey != null);
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
        }

        public override async Task LoadAsync() => await this.LoadAsync(null, null, null, null, null);

        /// <summary>
        /// DBから読み込む
        /// </summary>
        /// <param name="initialBookId">追加時、初期選択する帳簿のID</param>
        /// <param name="initialMonth">追加時、初期選択する年月</param>
        /// <param name="initialDate">追加時、初期選択する日付</param>
        /// <param name="initialRecordList">追加時、初期表示するCSVレコードリスト</param>
        /// <param name="targetGroupId">編集時、編集対象のグループID</param>
        public async Task LoadAsync(BookIdObj initialBookId, DateOnly? initialMonth, DateOnly? initialDate, IEnumerable<ActionCsvDto> initialRecordList, GroupIdObj targetGroupId)
        {
            using FuncLog funcLog = new(new { initialBookId, initialMonth, initialDate, initialRecordList, targetGroupId });

            BookIdObj selectingBookId = null;
            BalanceKind selectingBalanceKind = BalanceKind.Expenses;
            ItemIdObj selectingItemId = null;
            string selectingShopName = null;
            string selectingRemark = null;

            switch (this.RegKind) {
                case RegistrationKind.Add: {
                    selectingBookId = initialBookId;

                    // WVMに値を設定する
                    if (initialRecordList == null) {
                        DateTime actDate = initialDate?.ToDateTime(TimeOnly.MinValue) ?? ((initialMonth == null || initialMonth?.Month == DateTime.Today.Month) ? DateTime.Today : initialMonth.Value.ToDateTime(TimeOnly.MinValue));
                        this.InputedDateValueVMList.Add(new DateValueViewModel() { ActDate = actDate });
                    }
                    else {
                        foreach (ActionCsvDto record in initialRecordList) {
                            this.InputedDateValueVMList.Add(new DateValueViewModel() { ActDate = record.Date, ActValue = record.Value });
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
                            ActDate = action.ActTime,
                            ActValue = Math.Abs(action.Amount)
                        };

                        selectingBookId = action.Book.Id;
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
            await this.BookSelectorVM.LoadAsync(selectingBookId);
            await this.BalanceKindSelectorVM.LoadAsync(selectingBalanceKind);
            await this.CategorySelectorVM.LoadAsync();
            await this.ItemSelectorVM.LoadAsync(selectingItemId);
            await this.ShopSelectorVM.LoadAsync(selectingShopName);
            await this.RemarkSelectorVM.LoadAsync(selectingRemark);
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
        /// <returns>登録された帳簿項目IDリスト</returns>
        protected override async Task<IEnumerable<ActionIdObj>> SaveAsync()
        {
            using FuncLog funcLog = new();

            List<ActionModel> actionList = [];
            BalanceKind balanceKind = this.BalanceKindSelectorVM.SelectedKey;
            ActionModel commonAction = new() {
                Base = new(null, DateTime.Now, 0),
                GroupId = this.GroupId,
                Book = new(this.BookSelectorVM.SelectedKey, string.Empty),
                Item = new(this.ItemSelectorVM.SelectedKey, string.Empty),
                Shop = this.ShopSelectorVM.SelectedKey,
                Remark = this.RemarkSelectorVM.SelectedKey
            };
            foreach (DateValueViewModel vm in this.InputedDateValueVMList) {
                if (vm.ActValue is null or 0) { continue; }

                ActionBaseModel baseAction = new(vm.ActionId, vm.ActDate, (balanceKind == BalanceKind.Income ? 1 : -1) * vm.ActValue.Value);
                actionList.Add(commonAction.WithChanges(baseAction));
            }

            IEnumerable<ActionIdObj> resActionIdList = await this.mService.SaveActionListAsync(actionList);

            DateTime lastActTime = this.InputedDateValueVMList.Max(static tmp => tmp.ActDate);

            if (commonAction.Shop != null && commonAction.Shop != string.Empty) {
                ShopModel shop = new(commonAction.Shop) { ItemId = commonAction.Item.Id, CurrentActTime = commonAction.ActTime };
                await this.mService.SaveShopAsync(shop);
            }

            if (commonAction.Remark != null && commonAction.Remark != string.Empty) {
                RemarkModel remark = new(commonAction.Remark) { ItemId = commonAction.Item.Id, CurrentActTime = commonAction.ActTime };
                await this.mService.SaveRemarkAsync(remark);
            }

            return resActionIdList;
        }
    }
}

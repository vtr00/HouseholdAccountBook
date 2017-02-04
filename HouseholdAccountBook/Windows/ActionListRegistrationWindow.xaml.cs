using HouseholdAccountBook.Dao;
using HouseholdAccountBook.Extentions;
using HouseholdAccountBook.UserEventArgs;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.Windows
{
    /// <summary>
    /// ActionListRegistrationWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ActionListRegistrationWindow : Window
    {
        #region フィールド
        /// <summary>
        /// DAOビルダ
        /// </summary>
        private DaoBuilder builder;
        #endregion

        /// <summary>
        /// 登録時のイベント
        /// </summary>
        public event EventHandler<RegistrateEventArgs> Registrated = null;

        /// <summary>
        /// 帳簿項目まとめて追加ウィンドウ
        /// </summary>
        /// <param name="builder">DAOビルダ</param>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="selectedDateTime">選択された日時</param>
        public ActionListRegistrationWindow(DaoBuilder builder, int? bookId, DateTime? selectedDateTime)
        {
            InitializeComponent();

            this.builder = builder;

            ObservableCollection<BookViewModel> bookVMList = new ObservableCollection<BookViewModel>();
            BookViewModel selectedBookVM = null;
            using (DaoBase dao = builder.Build()) {
                DaoReader reader = dao.ExecQuery("SELECT book_id, book_name FROM mst_book WHERE del_flg = 0;");
                reader.ExecWholeRow((count, record) => {
                    BookViewModel vm = new BookViewModel() { BookId = record.ToInt("book_id"), BookName = record["book_name"] };
                    bookVMList.Add(vm);
                    if(selectedBookVM == null || bookId == vm.BookId) {
                        selectedBookVM = vm;
                    }
                });
            }

            this.ActionListRegistrationWindowVM.BookVMList = bookVMList;
            this.ActionListRegistrationWindowVM.SelectedBookVM = selectedBookVM;
            this.ActionListRegistrationWindowVM.SelectedBalanceKindVM = this.ActionListRegistrationWindowVM.BalanceKindVMList[1];

            UpdateCategoryList();
            UpdateItemList();
            UpdateShopList();
            UpdateRemarkList();

            LoadSetting();

            #region イベントハンドラの設定
            this.ActionListRegistrationWindowVM.OnBookChanged += () => {
                UpdateCategoryList();
                UpdateItemList();
                UpdateShopList();
                UpdateRemarkList();
            };
            this.ActionListRegistrationWindowVM.OnBalanceKindChanged += () => {
                UpdateCategoryList();
                UpdateItemList();
                UpdateShopList();
                UpdateRemarkList();
            };
            this.ActionListRegistrationWindowVM.OnCategoryChanged += () => {
                UpdateItemList();
                UpdateShopList();
                UpdateRemarkList();
            };
            this.ActionListRegistrationWindowVM.OnItemChanged += () => {
                UpdateShopList();
                UpdateRemarkList();
            };
            #endregion
        }

        #region コマンド
        /// <summary>
        /// 登録コマンド判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegisterCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ActionListRegistrationWindowVM.DateValueVMList.Count >= 1;
        }

        /// <summary>
        /// 登録コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegisterCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // DB登録
            int? id = RegisterToDb();

            // MainWindow更新
            Registrated?.Invoke(this, new RegistrateEventArgs(id));

            this.DialogResult = true;
            this.Close();
        }
        
        /// <summary>
        /// キャンセルコマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        #endregion

        #region イベントハンドラ
        /// <summary>
        /// 読込完了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActionListRegistrationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 処理なし
        }

        /// <summary>
        /// フォーム終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActionListRegistrationWindow_Closing(object sender, CancelEventArgs e)
        {
            SaveSetting();
        }

        /// <summary>
        /// キー押下時の処理(Escキーでキャンセル)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActionListRegistrationWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key) {
                case Key.Enter: {
                        RoutedCommand command = this.Resources["RegisterCommand"] as RoutedCommand;

                        if (command != null && command.CanExecute(null, sender as IInputElement)) {
                            command.Execute(null, sender as IInputElement);
                        }
                        e.Handled = true;
                    }
                    break;
                case Key.Escape: {
                        RoutedCommand command = this.Resources["CancelCommand"] as RoutedCommand;

                        if (command != null && command.CanExecute(null, sender as IInputElement)) {
                            command.Execute(null, sender as IInputElement);
                        }
                        e.Handled = true;
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 日付金額リスト追加時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            if(this.ActionListRegistrationWindowVM.DateValueVMList.Count > 0) {
                // リストに入力済の末尾のデータの日付を追加時に採用する
                DateValueViewModel lastVM = this.ActionListRegistrationWindowVM.DateValueVMList[this.ActionListRegistrationWindowVM.DateValueVMList.Count - 1];
                e.NewItem = new DateValueViewModel() { ActDate = lastVM.ActDate, ActValue = null };
            }
        }
        #endregion

        #region 画面更新用の関数
        /// <summary>
        /// カテゴリリストを更新する
        /// </summary>
        /// <param name="categoryId">選択対象のカテゴリ</param>
        private void UpdateCategoryList(int? categoryId = null)
        {
            ObservableCollection<CategoryViewModel> categoryVMList = new ObservableCollection<CategoryViewModel>() {
                new CategoryViewModel() { CategoryId = -1, CategoryName = "(指定なし)" }
            };
            CategoryViewModel selectedCategoryVM = categoryVMList[0];
            using (DaoBase dao = builder.Build()) {
                DaoReader reader = dao.ExecQuery(@"
SELECT category_id, category_name FROM mst_category C 
WHERE del_flg = 0 AND EXISTS (SELECT * FROM mst_item I WHERE I.category_id = C.category_id AND balance_kind = @{0} AND del_flg = 0 
  AND EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @{1} AND RBI.item_id = I.item_id)) 
ORDER BY sort_order;", (int)this.ActionListRegistrationWindowVM.SelectedBalanceKindVM.BalanceKind, this.ActionListRegistrationWindowVM.SelectedBookVM.BookId);
                reader.ExecWholeRow((count, record) => {
                    CategoryViewModel vm = new CategoryViewModel() { CategoryId = record.ToInt("category_id"), CategoryName = record["category_name"] };
                    categoryVMList.Add(vm);
                    if(vm.CategoryId == categoryId) {
                        selectedCategoryVM = vm;
                    }
                });
            }
            this.ActionListRegistrationWindowVM.CategoryVMList = categoryVMList;
            this.ActionListRegistrationWindowVM.SelectedCategoryVM = selectedCategoryVM;
        }

        /// <summary>
        /// 項目リストを更新する
        /// </summary>
        /// <param name="itemId">選択対象の項目</param>
        private void UpdateItemList(int? itemId = null)
        {
            ObservableCollection<ItemViewModel> itemVMList = new ObservableCollection<ItemViewModel>();
            ItemViewModel selectedItemVM = null;
            using (DaoBase dao = builder.Build()) {
                DaoReader reader;
                if (this.ActionListRegistrationWindowVM.SelectedCategoryVM.CategoryId == -1) {
                    reader = dao.ExecQuery(@"
SELECT item_id, item_name FROM mst_item I 
WHERE del_flg = 0 AND EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @{0} AND RBI.item_id = I.item_id AND del_flg = 0)
  AND EXISTS (SELECT * FROM mst_category C WHERE C.category_id = I.category_id AND balance_kind = @{1} AND del_flg = 0)
ORDER BY sort_order;", this.ActionListRegistrationWindowVM.SelectedBookVM.BookId, (int)this.ActionListRegistrationWindowVM.SelectedBalanceKindVM.BalanceKind);
                }
                else {
                    reader = dao.ExecQuery(@"
SELECT item_id, item_name FROM mst_item I 
WHERE del_flg = 0 AND EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @{0} AND RBI.item_id = I.item_id AND del_flg = 0)
  AND category_id = @{1}
ORDER BY sort_order;", this.ActionListRegistrationWindowVM.SelectedBookVM.BookId, (int)this.ActionListRegistrationWindowVM.SelectedCategoryVM.CategoryId);
                }
                reader.ExecWholeRow((count, record) => {
                    ItemViewModel vm = new ItemViewModel() { ItemId = record.ToInt("item_id"), ItemName = record["item_name"] };
                    itemVMList.Add(vm);
                    if (selectedItemVM == null || vm.ItemId == itemId) {
                        selectedItemVM = vm;
                    }
                });
            }
            this.ActionListRegistrationWindowVM.ItemVMList = itemVMList;
            this.ActionListRegistrationWindowVM.SelectedItemVM = selectedItemVM;
        }

        /// <summary>
        /// 店舗リストを更新する
        /// </summary>
        /// <param name="shopName">選択対象の店舗名</param>
        private void UpdateShopList(string shopName = null)
        {
            ObservableCollection<string> shopNameVMList = new ObservableCollection<string>() {
                    string.Empty
            };
            string selectedShopName = shopName ?? this.ActionListRegistrationWindowVM.SelectedShopName ?? shopNameVMList[0];
            using (DaoBase dao = builder.Build()) {
                DaoReader reader = dao.ExecQuery(@"
SELECT shop_name FROM hst_shop 
WHERE del_flg = 0 AND item_id = @{0} 
ORDER BY used_time DESC;", this.ActionListRegistrationWindowVM.SelectedItemVM.ItemId);
                reader.ExecWholeRow((count, record) => {
                    string tmp = record["shop_name"];
                    shopNameVMList.Add(tmp);
                });
            }

            this.ActionListRegistrationWindowVM.ShopNameList = shopNameVMList;
            this.ActionListRegistrationWindowVM.SelectedShopName = selectedShopName;
        }

        /// <summary>
        /// 備考リストを更新する
        /// </summary>
        /// <param name="remark">選択対象の備考</param>
        private void UpdateRemarkList(string remark = null)
        {
            ObservableCollection<string> remarkVMList = new ObservableCollection<string>() {
                    string.Empty
            };
            string selectedRemark = remark ?? this.ActionListRegistrationWindowVM.SelectedRemark ?? remarkVMList[0];
            using (DaoBase dao = builder.Build()) {
                DaoReader reader = dao.ExecQuery(@"
SELECT remark FROM hst_remark 
WHERE del_flg = 0 AND item_id = @{0} 
ORDER BY used_time DESC;", this.ActionListRegistrationWindowVM.SelectedItemVM.ItemId);
                reader.ExecWholeRow((count, record) => {
                    string tmp = record["remark"];
                    remarkVMList.Add(tmp);
                });
            }
            
            this.ActionListRegistrationWindowVM.RemarkList = remarkVMList;
            this.ActionListRegistrationWindowVM.SelectedRemark = selectedRemark;
        }
        #endregion

        /// <summary>
        /// DBに登録する
        /// </summary>
        /// <returns>登録された帳簿項目ID</returns>
        private int? RegisterToDb()
        {
            if(this.ActionListRegistrationWindowVM.DateValueVMList.Count < 1) {
                MessageBox.Show(this, Message.IllegalValue, MessageTitle.Exclamation, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return null;
            }

            BalanceKind balanceKind = this.ActionListRegistrationWindowVM.SelectedBalanceKindVM.BalanceKind; // 収支種別
            int bookId = this.ActionListRegistrationWindowVM.SelectedBookVM.BookId.Value; // 帳簿ID
            int itemId = this.ActionListRegistrationWindowVM.SelectedItemVM.ItemId; // 帳簿項目ID
            string shopName = this.ActionListRegistrationWindowVM.SelectedShopName; // 店舗名
            string remark = this.ActionListRegistrationWindowVM.SelectedRemark; // 備考

            DateTime lastActTime = this.ActionListRegistrationWindowVM.DateValueVMList[0].ActDate;
            using (DaoBase dao = builder.Build()) {
                #region 帳簿項目を追加する
                foreach(DateValueViewModel vm in this.ActionListRegistrationWindowVM.DateValueVMList) {
                    if (vm.ActValue.HasValue) {
                        DateTime actTime = vm.ActDate; // 日付
                        int actValue = (balanceKind == BalanceKind.Income ? 1 : -1) * vm.ActValue.Value; // 金額
                        DaoReader reader = dao.ExecQuery(@"
INSERT INTO hst_action (book_id, item_id, act_time, act_value, shop_name, remark, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, @{2}, @{3}, @{4}, @{5}, 0, 'now', @{6}, 'now', @{7}) RETURNING action_id;",
                            bookId, itemId, actTime, actValue, shopName, remark, Updater, Inserter);
                    }
                }
                #endregion

                if (shopName != string.Empty) {
                    #region 店舗を追加する
                    dao.ExecTransaction(() => {
                        DaoReader reader = dao.ExecQuery(@"
SELECT shop_name FROM hst_shop
WHERE item_id = @{0} AND shop_name = @{1};", itemId, shopName);

                        if (reader.Count == 0) {
                            dao.ExecNonQuery(@"
INSERT INTO hst_shop (item_id, shop_name, used_time, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, @{2}, 0, 'now', @{3}, 'now', @{4});", itemId, shopName, lastActTime, Updater, Inserter);
                        }
                        else {
                            dao.ExecNonQuery(@"
UPDATE hst_shop
SET used_time = @{0}, del_flg = 0, update_time = 'now', updater = @{1}
WHERE item_id = @{2} AND shop_name = @{3};", lastActTime, Updater, itemId, shopName);
                        }
                    });
                    #endregion
                }

                if (remark != string.Empty) {
                    #region 備考を追加する
                    dao.ExecTransaction(() => {
                        DaoReader reader = dao.ExecQuery(@"
SELECT remark FROM hst_remark
WHERE item_id = @{0} AND remark = @{1};", itemId, remark);

                        if (reader.Count == 0) {
                            dao.ExecNonQuery(@"
INSERT INTO hst_remark (item_id, remark, remark_kind, used_time, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, 0, @{2}, 0, 'now', @{3}, 'now', @{4});", itemId, remark, lastActTime, Updater, Inserter);
                        }
                        else {
                            dao.ExecNonQuery(@"
UPDATE hst_remark
SET used_time = @{0}, del_flg = 0, update_time = 'now', updater = @{1}
WHERE item_id = @{2} AND remark = @{3};", lastActTime, Updater, itemId, remark);
                        }
                    });
                    #endregion
                }
            }

            return null;
        }

        #region 設定反映用の関数
        /// <summary>
        /// 設定を読み込む
        /// </summary>
        private void LoadSetting()
        {
            if (Properties.Settings.Default.ActionListRegistrationWindow_Left != -1) {
                Left = Properties.Settings.Default.ActionListRegistrationWindow_Left;
            }
            if (Properties.Settings.Default.ActionListRegistrationWindow_Top != -1) {
                Top = Properties.Settings.Default.ActionListRegistrationWindow_Top;
            }
            if (Properties.Settings.Default.ActionListRegistrationWindow_Width != -1) {
                Width = Properties.Settings.Default.ActionListRegistrationWindow_Width;
            }
            if (Properties.Settings.Default.ActionListRegistrationWindow_Height != -1) {
                Height = Properties.Settings.Default.ActionListRegistrationWindow_Height;
            }
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        private void SaveSetting()
        {
            if (this.WindowState == WindowState.Normal) {
                Properties.Settings.Default.ActionListRegistrationWindow_Left = Left;
                Properties.Settings.Default.ActionListRegistrationWindow_Top = Top;
                Properties.Settings.Default.ActionListRegistrationWindow_Width = Width;
                Properties.Settings.Default.ActionListRegistrationWindow_Height = Height;
                Properties.Settings.Default.Save();
            }
        }
        #endregion
    }

    #region ViewModel
    /// <summary>
    /// 帳簿VM
    /// </summary>
    public partial class BookViewModel { }

    /// <summary>
    /// 収支種別
    /// </summary>
    public partial class BalanceKindViewModel { }

    /// <summary>
    /// カテゴリVM
    /// </summary>
    public partial class CategoryViewModel { }

    /// <summary>
    /// 項目VM
    /// </summary>
    public partial class ItemViewModel { }

    /// <summary>
    /// 日付金額VM
    /// </summary>
    public class DateValueViewModel
    {
        /// <summary>
        /// 日付
        /// </summary>
        public DateTime ActDate { get; set; } = DateTime.Now;
        /// <summary>
        /// 金額
        /// </summary>
        public int? ActValue { get; set; }
    }

    /// <summary>
    /// 帳簿項目リスト登録ウィンドウVM
    /// </summary>
    public class ActionListRegistrationWindowViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 変更時処理重複防止用フラグ
        /// </summary>
        private bool UpdateOnChanged = false;

        /// <summary>
        /// 帳簿変更時イベント
        /// </summary>
        public event Action OnBookChanged = default(Action);
        /// <summary>
        /// 収支変更時イベント
        /// </summary>
        public event Action OnBalanceKindChanged = default(Action);
        /// <summary>
        /// カテゴリ変更時イベント
        /// </summary>
        public event Action OnCategoryChanged = default(Action);
        /// <summary>
        /// 項目変更時イベント
        /// </summary>
        public event Action OnItemChanged = default(Action);

        /// <summary>
        /// 帳簿VMリスト
        /// </summary>
        #region BookVMList
        public ObservableCollection<BookViewModel> BookVMList
        {
            get { return _BookVMList; }
            set {
                if (_BookVMList != value) {
                    _BookVMList = value;
                    PropertyChanged?.Raise(this, _nameBookVMList);
                }
            }
        }
        private ObservableCollection<BookViewModel> _BookVMList = default(ObservableCollection<BookViewModel>);
        internal static readonly string _nameBookVMList = PropertyName<ActionListRegistrationWindowViewModel>.Get(x => x.BookVMList);
        #endregion
        /// <summary>
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookViewModel SelectedBookVM
        {
            get { return _SelectedBookVM; }
            set {
                if (_SelectedBookVM != value) {
                    _SelectedBookVM = value;
                    PropertyChanged?.Raise(this, _nameSelectedBookVM);
                    
                    if (!UpdateOnChanged) {
                        UpdateOnChanged = true;
                        OnBookChanged?.Invoke();
                        UpdateOnChanged = false;
                    }
                }
            }
        }

        private BookViewModel _SelectedBookVM = default(BookViewModel);
        internal static readonly string _nameSelectedBookVM = PropertyName<ActionListRegistrationWindowViewModel>.Get(x => x.SelectedBookVM);
        #endregion
        
        /// <summary>
        /// 収支VMリスト
        /// </summary>
        #region BalanceKindVMList
        public ObservableCollection<BalanceKindViewModel> BalanceKindVMList
        {
            get { return _BalanceKindVMList; }
            set {
                if (_BalanceKindVMList != value) {
                    _BalanceKindVMList = value;
                    PropertyChanged?.Raise(this, _nameBalanceKindVMList);
                }
            }
        }
        private ObservableCollection<BalanceKindViewModel> _BalanceKindVMList = new ObservableCollection<BalanceKindViewModel>() {
            new BalanceKindViewModel() { BalanceKind = BalanceKind.Income, BalanceKindName = BalanceStr[BalanceKind.Income]}, // 収入
            new BalanceKindViewModel() { BalanceKind = BalanceKind.Outgo, BalanceKindName = BalanceStr[BalanceKind.Outgo]}, // 支出
        };
        internal static readonly string _nameBalanceKindVMList = PropertyName<ActionListRegistrationWindowViewModel>.Get(x => x.BalanceKindVMList);
        #endregion
        /// <summary>
        /// 選択された収支VM
        /// </summary>
        #region SelectedBalanceKindVM
        public BalanceKindViewModel SelectedBalanceKindVM
        {
            get { return _SelectedBalanceKindVM; }
            set {
                if (_SelectedBalanceKindVM != value) {
                    _SelectedBalanceKindVM = value;
                    PropertyChanged?.Raise(this, _nameSelectedBalanceKindVM);
                    
                    if (!UpdateOnChanged) {
                        UpdateOnChanged = true;
                        OnBalanceKindChanged?.Invoke();
                        UpdateOnChanged = false;
                    }
                }
            }
        }
        private BalanceKindViewModel _SelectedBalanceKindVM = default(BalanceKindViewModel);
        internal static readonly string _nameSelectedBalanceKindVM = PropertyName<ActionListRegistrationWindowViewModel>.Get(x => x.SelectedBalanceKindVM);
        #endregion
        
        /// <summary>
        /// カテゴリVMリスト
        /// </summary>
        #region CategoryVMList
        public ObservableCollection<CategoryViewModel> CategoryVMList
        {
            get { return _CategoryVMList; }
            set {
                if (_CategoryVMList != value) {
                    _CategoryVMList = value;
                    PropertyChanged?.Raise(this, _nameCategoryVMList);
                }
            }
        }
        private ObservableCollection<CategoryViewModel> _CategoryVMList = default(ObservableCollection<CategoryViewModel>);
        internal static readonly string _nameCategoryVMList = PropertyName<ActionListRegistrationWindowViewModel>.Get(x => x.CategoryVMList);
        #endregion
        /// <summary>
        /// 選択されたカテゴリVM
        /// </summary>
        #region SelectedCategoryVM
        public CategoryViewModel SelectedCategoryVM
        {
            get { return _SelectedCategoryVM; }
            set {
                if (_SelectedCategoryVM != value) {
                    _SelectedCategoryVM = value;
                    PropertyChanged?.Raise(this, _nameSelectedCategoryVM);
                    
                    if (!UpdateOnChanged) {
                        UpdateOnChanged = true;
                        OnCategoryChanged?.Invoke();
                        UpdateOnChanged = false;
                    }
                }
            }
        }
        private CategoryViewModel _SelectedCategoryVM = default(CategoryViewModel);
        internal static readonly string _nameSelectedCategoryVM = PropertyName<ActionListRegistrationWindowViewModel>.Get(x => x.SelectedCategoryVM);
        #endregion

        /// <summary>
        /// 項目VMリスト
        /// </summary>
        #region ItemVMList
        public ObservableCollection<ItemViewModel> ItemVMList
        {
            get { return _ItemVMList; }
            set {
                if (_ItemVMList != value) {
                    _ItemVMList = value;
                    PropertyChanged?.Raise(this, _nameItemVMList);
                }
            }
        }
        private ObservableCollection<ItemViewModel> _ItemVMList = default(ObservableCollection<ItemViewModel>);
        internal static readonly string _nameItemVMList = PropertyName<ActionListRegistrationWindowViewModel>.Get(x => x.ItemVMList);
        #endregion
        /// <summary>
        /// 選択された項目VM
        /// </summary>
        #region SelectedItemVM
        public ItemViewModel SelectedItemVM
        {
            get { return _SelectedItemVM; }
            set {
                if (_SelectedItemVM != value) {
                    _SelectedItemVM = value;
                    PropertyChanged?.Raise(this, _nameSelectedItemVM);
                    
                    if (!UpdateOnChanged) {
                        UpdateOnChanged = true;
                        OnItemChanged?.Invoke();
                        UpdateOnChanged = false;
                    }
                }
            }
        }
        private ItemViewModel _SelectedItemVM = default(ItemViewModel);
        internal static readonly string _nameSelectedItemVM = PropertyName<ActionListRegistrationWindowViewModel>.Get(x => x.SelectedItemVM);
        #endregion

        /// <summary>
        /// 日付金額VMリスト
        /// </summary>
        #region DateValueVMList
        public ObservableCollection<DateValueViewModel> DateValueVMList
        {
            get { return _DateValueVMList; }
            set {
                if (_DateValueVMList != value) {
                    _DateValueVMList = value;
                    PropertyChanged.Raise(this, _nameDateValueVMList);
                }
            }
        }
        private ObservableCollection<DateValueViewModel> _DateValueVMList = new ObservableCollection<DateValueViewModel>();
        internal static readonly string _nameDateValueVMList = PropertyName<ActionListRegistrationWindowViewModel>.Get(x => x.DateValueVMList);
        #endregion
        
        /// <summary>
        /// 店舗名リスト
        /// </summary>
        #region ShopNameList
        public ObservableCollection<String> ShopNameList
        {
            get { return _ShopNameList; }
            set {
                if (_ShopNameList != value) {
                    _ShopNameList = value;
                    PropertyChanged?.Raise(this, _nameShopNameList);
                }
            }
        }
        private ObservableCollection<String> _ShopNameList = default(ObservableCollection<String>);
        internal static readonly string _nameShopNameList = PropertyName<ActionListRegistrationWindowViewModel>.Get(x => x.ShopNameList);
        #endregion
        /// <summary>
        /// 選択された店舗名
        /// </summary>
        #region SelectedShopName
        public String SelectedShopName
        {
            get { return _SelectedShopName; }
            set {
                if (_SelectedShopName != value) {
                    _SelectedShopName = value;
                    PropertyChanged?.Raise(this, _nameSelectedShopName);
                }
            }
        }
        private String _SelectedShopName = default(String);
        internal static readonly string _nameSelectedShopName = PropertyName<ActionListRegistrationWindowViewModel>.Get(x => x.SelectedShopName);
        #endregion

        /// <summary>
        /// 備考リスト
        /// </summary>
        #region RemarkList
        public ObservableCollection<String> RemarkList
        {
            get { return _RemarkList; }
            set {
                if (_RemarkList != value) {
                    _RemarkList = value;
                    PropertyChanged?.Raise(this, _nameRemarkList);
                }
            }
        }
        private ObservableCollection<String> _RemarkList = default(ObservableCollection<String>);
        internal static readonly string _nameRemarkList = PropertyName<ActionListRegistrationWindowViewModel>.Get(x => x.RemarkList);
        #endregion
        /// <summary>
        /// 選択された備考
        /// </summary>
        #region SelectedRemark
        public String SelectedRemark
        {
            get { return _SelectedRemark; }
            set {
                if (_SelectedRemark != value) {
                    _SelectedRemark = value;
                    PropertyChanged?.Raise(this, _nameSelectedRemark);
                }
            }
        }
        private String _SelectedRemark = default(String);
        internal static readonly string _nameSelectedRemark = PropertyName<ActionListRegistrationWindowViewModel>.Get(x => x.SelectedRemark);
        #endregion
        
        /// <summary>
        /// プロパティ変更イベントハンドラ
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
    #endregion
}

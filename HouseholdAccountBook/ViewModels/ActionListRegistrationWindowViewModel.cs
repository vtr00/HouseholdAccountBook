using HouseholdAccountBook.Extentions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.ViewModels
{
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

        #region 内部クラス定義
        /// <summary>
        /// 日付金額VM
        /// </summary>
        public class DateValueViewModel : INotifyPropertyChanged
        {
            /// <summary>
            /// 日付
            /// </summary>
            public DateTime ActDate { get; set; } = DateTime.Now;
            /// <summary>
            /// 金額
            /// </summary>
            #region ActValue
            public int? ActValue
            {
                get { return _ActValue; }
                set {
                    if (_ActValue != value) {
                        _ActValue = value;
                        PropertyChanged.Raise(this, _nameActValue);
                        CommandManager.InvalidateRequerySuggested();
                    }
                }
            }
            private int? _ActValue = default(int?);
            internal static readonly string _nameActValue = PropertyName<DateValueViewModel>.Get(x => x.ActValue);
            #endregion

            /// <summary>
            /// プロパティ変更イベントハンドラ
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;
        }
        #endregion
    }
}

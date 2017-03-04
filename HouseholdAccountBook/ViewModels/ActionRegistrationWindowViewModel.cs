using HouseholdAccountBook.Extentions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 帳簿項目登録ウィンドウVM
    /// </summary>
    public class ActionRegistrationWindowViewModel : INotifyPropertyChanged
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
        internal static readonly string _nameBookVMList = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.BookVMList);
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
        internal static readonly string _nameSelectedBookVM = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.SelectedBookVM);
        #endregion

        /// <summary>
        /// 選択された日時
        /// </summary>
        #region SelectedDate
        public DateTime SelectedDate
        {
            get { return _SelectedDate; }
            set {
                if (_SelectedDate != value) {
                    _SelectedDate = value;
                    PropertyChanged?.Raise(this, _nameSelectedDate);
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        private DateTime _SelectedDate = default(DateTime);
        internal static readonly string _nameSelectedDate = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.SelectedDate);
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
        internal static readonly string _nameBalanceKindVMList = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.BalanceKindVMList);
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
        internal static readonly string _nameSelectedBalanceKindVM = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.SelectedBalanceKindVM);
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
        internal static readonly string _nameCategoryVMList = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.CategoryVMList);
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
        internal static readonly string _nameSelectedCategoryVM = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.SelectedCategoryVM);
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
        internal static readonly string _nameItemVMList = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.ItemVMList);
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
        internal static readonly string _nameSelectedItemVM = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.SelectedItemVM);
        #endregion

        /// <summary>
        /// 金額
        /// </summary>
        #region Value
        public int? Value
        {
            get { return _Value; }
            set {
                if (_Value != value) {
                    _Value = value;
                    PropertyChanged?.Raise(this, _nameValue);
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        private int? _Value = null;
        internal static readonly string _nameValue = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.Value);
        #endregion

        /// <summary>
        /// 回数
        /// </summary>
        #region Count
        public int Count
        {
            get { return _Count; }
            set {
                if (_Count != value) {
                    _Count = value;
                    PropertyChanged?.Raise(this, _nameCount);
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        private int _Count = 1;
        internal static readonly string _nameCount = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.Count);
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
        internal static readonly string _nameShopNameList = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.ShopNameList);
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
        internal static readonly string _nameSelectedShopName = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.SelectedShopName);
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
        internal static readonly string _nameRemarkList = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.RemarkList);
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
        internal static readonly string _nameSelectedRemark = PropertyName<ActionRegistrationWindowViewModel>.Get(x => x.SelectedRemark);
        #endregion

        /// <summary>
        /// プロパティ変更イベントハンドラ
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}

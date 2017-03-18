using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 帳簿項目リスト登録ウィンドウVM
    /// </summary>
    public class ActionListRegistrationWindowViewModel : BindableBase
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
            set { SetProperty(ref _BookVMList, value); }
        }
        private ObservableCollection<BookViewModel> _BookVMList = default(ObservableCollection<BookViewModel>);
        #endregion
        /// <summary>
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookViewModel SelectedBookVM
        {
            get { return _SelectedBookVM; }
            set {
                if (SetProperty(ref _SelectedBookVM, value)) {
                    if (!UpdateOnChanged) {
                        UpdateOnChanged = true;
                        OnBookChanged?.Invoke();
                        UpdateOnChanged = false;
                    }
                }
            }
        }

        private BookViewModel _SelectedBookVM = default(BookViewModel);
        #endregion

        /// <summary>
        /// 収支VMリスト
        /// </summary>
        #region BalanceKindVMList
        public ObservableCollection<BalanceKindViewModel> BalanceKindVMList
        {
            get { return _BalanceKindVMList; }
            set { SetProperty(ref _BalanceKindVMList, value); }
        }
        private ObservableCollection<BalanceKindViewModel> _BalanceKindVMList = new ObservableCollection<BalanceKindViewModel>() {
            new BalanceKindViewModel() { BalanceKind = BalanceKind.Income, BalanceKindName = BalanceStr[BalanceKind.Income]}, // 収入
            new BalanceKindViewModel() { BalanceKind = BalanceKind.Outgo, BalanceKindName = BalanceStr[BalanceKind.Outgo]}, // 支出
        };
        #endregion
        /// <summary>
        /// 選択された収支VM
        /// </summary>
        #region SelectedBalanceKindVM
        public BalanceKindViewModel SelectedBalanceKindVM
        {
            get { return _SelectedBalanceKindVM; }
            set {
                if (SetProperty(ref _SelectedBalanceKindVM, value)) {
                    if (!UpdateOnChanged) {
                        UpdateOnChanged = true;
                        OnBalanceKindChanged?.Invoke();
                        UpdateOnChanged = false;
                    }
                }
            }
        }
        private BalanceKindViewModel _SelectedBalanceKindVM = default(BalanceKindViewModel);
        #endregion

        /// <summary>
        /// カテゴリVMリスト
        /// </summary>
        #region CategoryVMList
        public ObservableCollection<CategoryViewModel> CategoryVMList
        {
            get { return _CategoryVMList; }
            set { SetProperty(ref _CategoryVMList, value); }
        }
        private ObservableCollection<CategoryViewModel> _CategoryVMList = default(ObservableCollection<CategoryViewModel>);
        #endregion
        /// <summary>
        /// 選択されたカテゴリVM
        /// </summary>
        #region SelectedCategoryVM
        public CategoryViewModel SelectedCategoryVM
        {
            get { return _SelectedCategoryVM; }
            set {
                if (SetProperty(ref _SelectedCategoryVM, value)) {
                    if (!UpdateOnChanged) {
                        UpdateOnChanged = true;
                        OnCategoryChanged?.Invoke();
                        UpdateOnChanged = false;
                    }
                }
            }
        }
        private CategoryViewModel _SelectedCategoryVM = default(CategoryViewModel);
        #endregion

        /// <summary>
        /// 項目VMリスト
        /// </summary>
        #region ItemVMList
        public ObservableCollection<ItemViewModel> ItemVMList
        {
            get { return _ItemVMList; }
            set { SetProperty(ref _ItemVMList, value); }
        }
        private ObservableCollection<ItemViewModel> _ItemVMList = default(ObservableCollection<ItemViewModel>);
        #endregion
        /// <summary>
        /// 選択された項目VM
        /// </summary>
        #region SelectedItemVM
        public ItemViewModel SelectedItemVM
        {
            get { return _SelectedItemVM; }
            set {
                if (SetProperty(ref _SelectedItemVM, value)) {
                    if (!UpdateOnChanged) {
                        UpdateOnChanged = true;
                        OnItemChanged?.Invoke();
                        UpdateOnChanged = false;
                    }
                }
            }
        }
        private ItemViewModel _SelectedItemVM = default(ItemViewModel);
        #endregion

        /// <summary>
        /// 日付金額VMリスト
        /// </summary>
        #region DateValueVMList
        public ObservableCollection<DateValueViewModel> DateValueVMList
        {
            get { return _DateValueVMList; }
            set { SetProperty(ref _DateValueVMList, value); }
        }
        private ObservableCollection<DateValueViewModel> _DateValueVMList = new ObservableCollection<DateValueViewModel>();
        #endregion

        /// <summary>
        /// 店舗名リスト
        /// </summary>
        #region ShopNameList
        public ObservableCollection<String> ShopNameList
        {
            get { return _ShopNameList; }
            set { SetProperty(ref _ShopNameList, value); }
        }
        private ObservableCollection<String> _ShopNameList = default(ObservableCollection<String>);
        #endregion
        /// <summary>
        /// 選択された店舗名
        /// </summary>
        #region SelectedShopName
        public String SelectedShopName
        {
            get { return _SelectedShopName; }
            set { SetProperty(ref _SelectedShopName, value); }
        }
        private String _SelectedShopName = default(String);
        #endregion

        /// <summary>
        /// 備考リスト
        /// </summary>
        #region RemarkList
        public ObservableCollection<String> RemarkList
        {
            get { return _RemarkList; }
            set { SetProperty(ref _RemarkList, value); }
        }
        private ObservableCollection<String> _RemarkList = default(ObservableCollection<String>);
        #endregion
        /// <summary>
        /// 選択された備考
        /// </summary>
        #region SelectedRemark
        public String SelectedRemark
        {
            get { return _SelectedRemark; }
            set { SetProperty(ref _SelectedRemark, value); }
        }
        private String _SelectedRemark = default(String);
        #endregion
        
        #region 内部クラス定義
        /// <summary>
        /// 日付金額VM
        /// </summary>
        public class DateValueViewModel : BindableBase
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
                    if (SetProperty(ref _ActValue, value)) {
                        CommandManager.InvalidateRequerySuggested();
                    }
                }
            }
            private int? _ActValue = default(int?);
            #endregion
        }
        #endregion
    }
}

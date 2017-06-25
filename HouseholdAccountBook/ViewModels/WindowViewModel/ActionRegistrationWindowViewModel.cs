using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 帳簿項目登録ウィンドウVM
    /// </summary>
    public class ActionRegistrationWindowViewModel : BindableBase
    {
        #region フィールド
        /// <summary>
        /// 変更時処理重複防止用フラグ
        /// </summary>
        private bool UpdateOnChanged = false;
        #endregion

        #region イベントハンドラ
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
        #endregion

        #region プロパティ
        /// <summary>
        /// 登録モード
        /// </summary>
        #region RegMode
        public RegistrationMode RegMode
        {
            get { return _RegMode; }
            set { SetProperty(ref _RegMode, value); }
        }
        private RegistrationMode _RegMode = default(RegistrationMode);
        #endregion
        
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
        /// 選択された日時
        /// </summary>
        #region SelectedDate
        public DateTime SelectedDate
        {
            get { return _SelectedDate; }
            set {
                if (SetProperty(ref _SelectedDate, value)) {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        private DateTime _SelectedDate = default(DateTime);
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
            get { return _SelectedBalanceKind; }
            set {
                if (SetProperty(ref _SelectedBalanceKind, value)) {
                    if (!UpdateOnChanged) {
                        UpdateOnChanged = true;
                        OnBalanceKindChanged?.Invoke();
                        UpdateOnChanged = false;
                    }
                }
            }
        }
        private BalanceKind _SelectedBalanceKind = default(BalanceKind);
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
        /// 金額
        /// </summary>
        #region Value
        public int? Value
        {
            get { return _Value; }
            set {
                if (SetProperty(ref _Value, value)) {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        private int? _Value = null;
        #endregion

        /// <summary>
        /// 店舗名リスト
        /// </summary>
        #region ShopNameList
        public ObservableCollection<string> ShopNameList
        {
            get { return _ShopNameList; }
            set { SetProperty(ref _ShopNameList, value); }
        }
        private ObservableCollection<string> _ShopNameList = default(ObservableCollection<string>);
        #endregion
        /// <summary>
        /// 選択された店舗名
        /// </summary>
        #region SelectedShopName
        public string SelectedShopName
        {
            get { return _SelectedShopName; }
            set { SetProperty(ref _SelectedShopName, value); }
        }
        private string _SelectedShopName = default(string);
        #endregion

        /// <summary>
        /// 備考リスト
        /// </summary>
        #region RemarkList
        public ObservableCollection<string> RemarkList
        {
            get { return _RemarkList; }
            set { SetProperty(ref _RemarkList, value); }
        }
        private ObservableCollection<string> _RemarkList = default(ObservableCollection<string>);
        #endregion
        /// <summary>
        /// 選択された備考
        /// </summary>
        #region SelectedRemark
        public string SelectedRemark
        {
            get { return _SelectedRemark; }
            set { SetProperty(ref _SelectedRemark, value); }
        }
        private string _SelectedRemark = default(string);
        #endregion

        /// <summary>
        /// 回数
        /// </summary>
        #region Count
        public int Count
        {
            get { return _Count; }
            set {
                if (SetProperty(ref _Count, value)) {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        private int _Count = 1;
        #endregion
        /// <summary>
        /// 連動して編集
        /// </summary>
        #region IsLink
        public bool IsLink
        {
            get { return _IsLink; }
            set { SetProperty(ref _IsLink, value); }
        }
        private bool _IsLink = default(bool);
        #endregion
        
        /// <summary>
        /// 一致フラグ
        /// </summary>
        #region IsMatch
        public bool IsMatch
        {
            get { return _IsMatch; }
            set { SetProperty(ref _IsMatch, value); }
        }
        private bool _IsMatch = default(bool);
        #endregion
        #endregion
    }
}

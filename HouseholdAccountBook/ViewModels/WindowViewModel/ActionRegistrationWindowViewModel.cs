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
        private bool isUpdateOnChanged = false;
        #endregion

        #region イベント
        /// <summary>
        /// 帳簿変更時イベント
        /// </summary>
        public event Action BookChanged = default(Action);
        /// <summary>
        /// 収支変更時イベント
        /// </summary>
        public event Action BalanceKindChanged = default(Action);
        /// <summary>
        /// カテゴリ変更時イベント
        /// </summary>
        public event Action CategoryChanged = default(Action);
        /// <summary>
        /// 項目変更時イベント
        /// </summary>
        public event Action ItemChanged = default(Action);
        #endregion

        #region プロパティ
        /// <summary>
        /// 登録モード
        /// </summary>
        #region RegMode
        public RegistrationMode RegMode
        {
            get { return this._RegMode; }
            set { SetProperty(ref this._RegMode, value); }
        }
        private RegistrationMode _RegMode = default(RegistrationMode);
        #endregion
        
        /// <summary>
        /// 帳簿VMリスト
        /// </summary>
        #region BookVMList
        public ObservableCollection<BookViewModel> BookVMList
        {
            get { return this._BookVMList; }
            set { SetProperty(ref this._BookVMList, value); }
        }
        private ObservableCollection<BookViewModel> _BookVMList = default(ObservableCollection<BookViewModel>);
        #endregion
        /// <summary>
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookViewModel SelectedBookVM
        {
            get { return this._SelectedBookVM; }
            set {
                if (SetProperty(ref this._SelectedBookVM, value)) {
                    if (!this.isUpdateOnChanged) {
                        this.isUpdateOnChanged = true;
                        BookChanged?.Invoke();
                        this.isUpdateOnChanged = false;
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
            get { return this._SelectedDate; }
            set {
                if (SetProperty(ref this._SelectedDate, value)) {
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
            get { return this._SelectedBalanceKind; }
            set {
                if (SetProperty(ref this._SelectedBalanceKind, value)) {
                    if (!this.isUpdateOnChanged) {
                        this.isUpdateOnChanged = true;
                        BalanceKindChanged?.Invoke();
                        this.isUpdateOnChanged = false;
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
            get { return this._CategoryVMList; }
            set { SetProperty(ref this._CategoryVMList, value); }
        }
        private ObservableCollection<CategoryViewModel> _CategoryVMList = default(ObservableCollection<CategoryViewModel>);
        #endregion
        /// <summary>
        /// 選択されたカテゴリVM
        /// </summary>
        #region SelectedCategoryVM
        public CategoryViewModel SelectedCategoryVM
        {
            get { return this._SelectedCategoryVM; }
            set {
                if (SetProperty(ref this._SelectedCategoryVM, value)) {
                    if (!this.isUpdateOnChanged) {
                        this.isUpdateOnChanged = true;
                        CategoryChanged?.Invoke();
                        this.isUpdateOnChanged = false;
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
            get { return this._ItemVMList; }
            set { SetProperty(ref this._ItemVMList, value); }
        }
        private ObservableCollection<ItemViewModel> _ItemVMList = default(ObservableCollection<ItemViewModel>);
        #endregion
        /// <summary>
        /// 選択された項目VM
        /// </summary>
        #region SelectedItemVM
        public ItemViewModel SelectedItemVM
        {
            get { return this._SelectedItemVM; }
            set {
                if (SetProperty(ref this._SelectedItemVM, value)) {
                    if (!this.isUpdateOnChanged) {
                        this.isUpdateOnChanged = true;
                        ItemChanged?.Invoke();
                        this.isUpdateOnChanged = false;
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
            get { return this._Value; }
            set {
                if (SetProperty(ref this._Value, value)) {
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
            get { return this._ShopNameList; }
            set { SetProperty(ref this._ShopNameList, value); }
        }
        private ObservableCollection<string> _ShopNameList = default(ObservableCollection<string>);
        #endregion
        /// <summary>
        /// 選択された店舗名
        /// </summary>
        #region SelectedShopName
        public string SelectedShopName
        {
            get { return this._SelectedShopName; }
            set { SetProperty(ref this._SelectedShopName, value); }
        }
        private string _SelectedShopName = default(string);
        #endregion

        /// <summary>
        /// 備考リスト
        /// </summary>
        #region RemarkList
        public ObservableCollection<string> RemarkList
        {
            get { return this._RemarkList; }
            set { SetProperty(ref this._RemarkList, value); }
        }
        private ObservableCollection<string> _RemarkList = default(ObservableCollection<string>);
        #endregion
        /// <summary>
        /// 選択された備考
        /// </summary>
        #region SelectedRemark
        public string SelectedRemark
        {
            get { return this._SelectedRemark; }
            set { SetProperty(ref this._SelectedRemark, value); }
        }
        private string _SelectedRemark = default(string);
        #endregion

        /// <summary>
        /// 回数
        /// </summary>
        #region Count
        public int Count
        {
            get { return this._Count; }
            set {
                if (SetProperty(ref this._Count, value)) {
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
            get { return this._IsLink; }
            set { SetProperty(ref this._IsLink, value); }
        }
        private bool _IsLink = default(bool);
        #endregion
        /// <summary>
        /// 休日設定種別辞書
        /// </summary>
        #region HolidaySettingKindDic
        public Dictionary<HolidaySettingKind, string> HolidaySettingKindDic { get; } = HolidaySettingKindStr;
        #endregion
        /// <summary>
        /// 選択された祝別設定種別
        /// </summary>
        #region SelectedHolidaySettingKind
        public HolidaySettingKind SelectedHolidaySettingKind
        {
            get { return this._SelectedHolidaySettingKind; }
            set { SetProperty(ref this._SelectedHolidaySettingKind, value); }
        }
        private HolidaySettingKind _SelectedHolidaySettingKind = HolidaySettingKind.Nothing;
        #endregion

        /// <summary>
        /// 一致フラグ
        /// </summary>
        #region IsMatch
        public bool IsMatch
        {
            get { return this._IsMatch; }
            set { SetProperty(ref this._IsMatch, value); }
        }
        private bool _IsMatch = default(bool);
        #endregion
        #endregion
    }
}

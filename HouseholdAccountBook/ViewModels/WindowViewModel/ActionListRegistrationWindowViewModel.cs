using HouseholdAccountBook.UserControls;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 帳簿項目リスト登録ウィンドウVM
    /// </summary>
    public class ActionListRegistrationWindowViewModel : BindableBase
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
        }
        private RegistrationMode _RegMode = RegistrationMode.Add;
        #endregion

        /// <summary>
        /// 帳簿VMリスト
        /// </summary>
        #region BookVMList
        public ObservableCollection<BookViewModel> BookVMList
        {
            get { return this._BookVMList; }
            set { this.SetProperty(ref this._BookVMList, value); }
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
                if (this.SetProperty(ref this._SelectedBookVM, value)) {
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
                if (this.SetProperty(ref this._SelectedBalanceKind, value)) {
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
            set { this.SetProperty(ref this._CategoryVMList, value); }
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
                if (this.SetProperty(ref this._SelectedCategoryVM, value)) {
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
            set { this.SetProperty(ref this._ItemVMList, value); }
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
                if (this.SetProperty(ref this._SelectedItemVM, value)) {
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
        /// 日付金額VMリスト
        /// </summary>
        #region DateValueVMList
        public ObservableCollection<DateValueViewModel> DateValueVMList
        {
            get { return this._DateValueVMList; }
            set { this.SetProperty(ref this._DateValueVMList, value); }
        }
        private ObservableCollection<DateValueViewModel> _DateValueVMList = new ObservableCollection<DateValueViewModel>();
        #endregion

        /// <summary>
        /// 編集中か
        /// </summary>
        #region IsEditing
        public bool IsEditing
        {
            get { return this._IsEditing; }
            set { this.SetProperty(ref this._IsEditing, value); }
        }
        private bool _IsEditing = default(bool);
        #endregion
        
        /// <summary>
        /// 店舗名リスト
        /// </summary>
        #region ShopNameList
        public ObservableCollection<string> ShopNameList
        {
            get { return this._ShopNameList; }
            set { this.SetProperty(ref this._ShopNameList, value); }
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
            set { this.SetProperty(ref this._SelectedShopName, value); }
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
            set { this.SetProperty(ref this._RemarkList, value); }
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
            set { this.SetProperty(ref this._SelectedRemark, value); }
        }
        private string _SelectedRemark = default(string);
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
    }
}

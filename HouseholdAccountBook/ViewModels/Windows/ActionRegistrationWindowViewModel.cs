using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using static HouseholdAccountBook.Models.DbConstants;
using static HouseholdAccountBook.ViewModels.LogicConstants;
using static HouseholdAccountBook.Views.UiConstants;

namespace HouseholdAccountBook.ViewModels.Windows
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
        public event Action<int?> BookChanged = default;
        /// <summary>
        /// 日時変更時イベント
        /// </summary>
        public event Action<DateTime> DateChanged = default;
        /// <summary>
        /// 収支変更時イベント
        /// </summary>
        public event Action<BalanceKind> BalanceKindChanged = default;
        /// <summary>
        /// 分類変更時イベント
        /// </summary>
        public event Action<int?> CategoryChanged = default;
        /// <summary>
        /// 項目変更時イベント
        /// </summary>
        public event Action<int?> ItemChanged = default;
        #endregion

        #region プロパティ
        /// <summary>
        /// 帳簿項目ID
        /// </summary>
        #region ActionId
        public int? ActionId
        {
            get => this._ActionId;
            set => this.SetProperty(ref this._ActionId, value);
        }
        private int? _ActionId = default;
        #endregion
        /// <summary>
        /// グループID
        /// </summary>
        #region GroupId
        public int? GroupId
        {
            get => this._GroupId;
            set => this.SetProperty(ref this._GroupId, value);
        }
        private int? _GroupId = default;
        #endregion

        /// <summary>
        /// 登録モード
        /// </summary>
        #region RegMode
        public RegistrationKind RegMode
        {
            get => this._RegMode;
            set => this.SetProperty(ref this._RegMode, value);
        }
        private RegistrationKind _RegMode = RegistrationKind.Add;
        #endregion
        /// <summary>
        /// CSV比較からの追加
        /// </summary>
        #region AddedByCsvComparison
        public bool AddedByCsvComparison
        {
            get => this._AddedByCsvComparison;
            set => this.SetProperty(ref this._AddedByCsvComparison, value);
        }
        private bool _AddedByCsvComparison = default;
        #endregion

        /// <summary>
        /// 帳簿VMリスト
        /// </summary>
        #region BookVMList
        public ObservableCollection<BookViewModel> BookVMList
        {
            get => this._BookVMList;
            set => this.SetProperty(ref this._BookVMList, value);
        }
        private ObservableCollection<BookViewModel> _BookVMList = default;
        #endregion
        /// <summary>
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookViewModel SelectedBookVM
        {
            get => this._SelectedBookVM;
            set {
                if (this.SetProperty(ref this._SelectedBookVM, value)) {
                    if (!this.isUpdateOnChanged) {
                        this.isUpdateOnChanged = true;
                        this.BookChanged?.Invoke(value?.Id);
                        this.isUpdateOnChanged = false;
                    }
                }
            }
        }

        private BookViewModel _SelectedBookVM = default;
        #endregion

        /// <summary>
        /// 選択された日時
        /// </summary>
        #region SelectedDate
        public DateTime SelectedDate
        {
            get => this._SelectedDate;
            set {
                if (this.SetProperty(ref this._SelectedDate, value)) {
                    this.DateChanged?.Invoke(value);
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        private DateTime _SelectedDate = DateTime.Now;
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
            get => this._SelectedBalanceKind;
            set {
                if (this.SetProperty(ref this._SelectedBalanceKind, value)) {
                    if (!this.isUpdateOnChanged) {
                        this.isUpdateOnChanged = true;
                        this.BalanceKindChanged?.Invoke(value);
                        this.isUpdateOnChanged = false;
                    }
                }
            }
        }
        private BalanceKind _SelectedBalanceKind = default;
        #endregion

        /// <summary>
        /// 分類VMリスト
        /// </summary>
        #region CategoryVMList
        public ObservableCollection<CategoryViewModel> CategoryVMList
        {
            get => this._CategoryVMList;
            set => this.SetProperty(ref this._CategoryVMList, value);
        }
        private ObservableCollection<CategoryViewModel> _CategoryVMList = default;
        #endregion
        /// <summary>
        /// 選択された分類VM
        /// </summary>
        #region SelectedCategoryVM
        public CategoryViewModel SelectedCategoryVM
        {
            get => this._SelectedCategoryVM;
            set {
                if (this.SetProperty(ref this._SelectedCategoryVM, value)) {
                    if (!this.isUpdateOnChanged) {
                        this.isUpdateOnChanged = true;
                        this.CategoryChanged?.Invoke(value?.Id);
                        this.isUpdateOnChanged = false;
                    }
                }
            }
        }
        private CategoryViewModel _SelectedCategoryVM = default;
        #endregion

        /// <summary>
        /// 項目VMリスト
        /// </summary>
        #region ItemVMList
        public ObservableCollection<ItemViewModel> ItemVMList
        {
            get => this._ItemVMList;
            set => this.SetProperty(ref this._ItemVMList, value);
        }
        private ObservableCollection<ItemViewModel> _ItemVMList = default;
        #endregion
        /// <summary>
        /// 選択された項目VM
        /// </summary>
        #region SelectedItemVM
        public ItemViewModel SelectedItemVM
        {
            get => this._SelectedItemVM;
            set {
                if (this.SetProperty(ref this._SelectedItemVM, value)) {
                    if (!this.isUpdateOnChanged) {
                        this.isUpdateOnChanged = true;
                        this.ItemChanged?.Invoke(value?.Id);
                        this.isUpdateOnChanged = false;
                    }
                }
            }
        }
        private ItemViewModel _SelectedItemVM = default;
        #endregion

        /// <summary>
        /// 金額
        /// </summary>
        #region Value
        public int? Value
        {
            get => this._Value;
            set {
                if (this.SetProperty(ref this._Value, value)) {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        private int? _Value = null;
        #endregion

        /// <summary>
        /// 店舗VMリスト
        /// </summary>
        #region ShopVMList
        public ObservableCollection<ShopViewModel> ShopVMList
        {
            get => this._ShopVMList;
            set => this.SetProperty(ref this._ShopVMList, value);
        }
        private ObservableCollection<ShopViewModel> _ShopVMList = default;
        #endregion
        /// <summary>
        /// 選択された店名
        /// </summary>
        #region SelectedShopName
        public string SelectedShopName
        {
            get => this._SelectedShopName;
            set => this.SetProperty(ref this._SelectedShopName, value);
        }
        private string _SelectedShopName = default;
        #endregion

        /// <summary>
        /// 備考VMリスト
        /// </summary>
        #region RemarkVMList
        public ObservableCollection<RemarkViewModel> RemarkVMList
        {
            get => this._RemarkVMList;
            set => this.SetProperty(ref this._RemarkVMList, value);
        }
        private ObservableCollection<RemarkViewModel> _RemarkVMList = default;
        #endregion
        /// <summary>
        /// 選択された備考
        /// </summary>
        #region SelectedRemark
        public string SelectedRemark
        {
            get => this._SelectedRemark;
            set => this.SetProperty(ref this._SelectedRemark, value);
        }
        private string _SelectedRemark = default;
        #endregion

        /// <summary>
        /// 回数
        /// </summary>
        #region Count
        public int Count
        {
            get => this._Count;
            set {
                if (this.SetProperty(ref this._Count, value)) {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        private int _Count = 1;
        #endregion

        /// <summary>
        /// 同じグループIDを持つ帳簿項目を連動して編集
        /// </summary>
        #region IsLink
        public bool IsLink
        {
            get => this._IsLink;
            set => this.SetProperty(ref this._IsLink, value);
        }
        private bool _IsLink = default;
        #endregion

        /// <summary>
        /// 休日設定種別辞書
        /// </summary>
        #region HolidaySettingKindDic
        public Dictionary<HolidaySettingKind, string> HolidaySettingKindDic { get; } = HolidaySettingKindStr;
        #endregion
        /// <summary>
        /// 選択された休日設定種別
        /// </summary>
        #region SelectedHolidaySettingKind
        public HolidaySettingKind SelectedHolidaySettingKind
        {
            get => this._SelectedHolidaySettingKind;
            set => this.SetProperty(ref this._SelectedHolidaySettingKind, value);
        }
        private HolidaySettingKind _SelectedHolidaySettingKind = HolidaySettingKind.Nothing;
        #endregion

        /// <summary>
        /// 一致フラグ
        /// </summary>
        #region IsMatch
        public bool IsMatch
        {
            get => this._IsMatch;
            set => this.SetProperty(ref this._IsMatch, value);
        }
        private bool _IsMatch = default;
        #endregion
        #endregion
    }
}

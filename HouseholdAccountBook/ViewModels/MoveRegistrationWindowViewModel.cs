using HouseholdAccountBook.Extentions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 帳簿項目登録ウィンドウ(移動)VM
    /// </summary>
    public class MoveRegistrationWindowViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 変更時処理重複防止用フラグ
        /// </summary>
        private bool UpdateOnChanged = false;

        /// <summary>
        /// 移動元帳簿変更時イベント
        /// </summary>
        public event Action OnFromBookChanged = default(Action);
        /// <summary>
        /// 移動先帳簿変更時イベント
        /// </summary>
        public event Action OnToBookChanged = default(Action);
        /// <summary>
        /// 手数料種別変更時イベント
        /// </summary>
        public event Action OnCommissionKindChanged = default(Action);
        /// <summary>
        /// 項目変更時イベント
        /// </summary>
        public event Action OnItemChanged = default(Action);

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
                }
            }
        }
        private DateTime _SelectedDate = default(DateTime);
        internal static readonly string _nameSelectedDate = PropertyName<MoveRegistrationWindowViewModel>.Get(x => x.SelectedDate);
        #endregion

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
        internal static readonly string _nameBookVMList = PropertyName<MoveRegistrationWindowViewModel>.Get(x => x.BookVMList);
        #endregion
        /// <summary>
        /// 選択された移動元帳簿VM
        /// </summary>
        #region SelectedFromBookVM
        public BookViewModel SelectedFromBookVM
        {
            get { return _SelectedFromBookVM; }
            set {
                if (_SelectedFromBookVM != value) {
                    _SelectedFromBookVM = value;
                    PropertyChanged?.Raise(this, _nameSelectedFromBookVM);

                    if (!UpdateOnChanged) {
                        UpdateOnChanged = true;
                        OnFromBookChanged?.Invoke();
                        UpdateOnChanged = false;
                    }
                }
            }
        }
        private BookViewModel _SelectedFromBookVM = default(BookViewModel);
        internal static readonly string _nameSelectedFromBookVM = PropertyName<MoveRegistrationWindowViewModel>.Get(x => x.SelectedFromBookVM);
        #endregion
        /// <summary>
        /// 選択された移動先帳簿VM
        /// </summary>
        #region SelectedToBookVM
        public BookViewModel SelectedToBookVM
        {
            get { return _SelectedToBookVM; }
            set {
                if (_SelectedToBookVM != value) {
                    _SelectedToBookVM = value;
                    PropertyChanged?.Raise(this, _nameSelectedToBookVM);

                    if (!UpdateOnChanged) {
                        UpdateOnChanged = true;
                        OnToBookChanged?.Invoke();
                        UpdateOnChanged = false;
                    }
                }
            }
        }
        private BookViewModel _SelectedToBookVM = default(BookViewModel);
        internal static readonly string _nameSelectedToBookVM = PropertyName<MoveRegistrationWindowViewModel>.Get(x => x.SelectedToBookVM);
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
                }
            }
        }
        private int? _Value = null;
        internal static readonly string _nameValue = PropertyName<MoveRegistrationWindowViewModel>.Get(x => x.Value);
        #endregion

        /// <summary>
        /// 手数料種別VMリスト
        /// </summary>
        #region CommissionKindVMList
        public ObservableCollection<CommissionKindViewModel> CommissionKindVMList
        {
            get { return _CommissionKindVMList; }
            set {
                if (_CommissionKindVMList != value) {
                    _CommissionKindVMList = value;
                    PropertyChanged?.Raise(this, _nameCommissionKindVMList);
                }
            }
        }
        private ObservableCollection<CommissionKindViewModel> _CommissionKindVMList = new ObservableCollection<CommissionKindViewModel> {
            new CommissionKindViewModel() { CommissionKind = CommissionKind.FromBook, CommissionKindName = CommissionStr[CommissionKind.FromBook] },
            new CommissionKindViewModel() { CommissionKind = CommissionKind.ToBook, CommissionKindName = CommissionStr[CommissionKind.ToBook] }
        };
        internal static readonly string _nameCommissionKindVMList = PropertyName<MoveRegistrationWindowViewModel>.Get(x => x.CommissionKindVMList);
        #endregion
        /// <summary>
        /// 選択された手数料種別VM
        /// </summary>
        #region SelectedCommissionKindVM
        public CommissionKindViewModel SelectedCommissionKindVM
        {
            get { return _SelectedCommissionKindVM; }
            set {
                if (_SelectedCommissionKindVM != value) {
                    _SelectedCommissionKindVM = value;
                    PropertyChanged?.Raise(this, _nameSelectedCommissionKindVM);

                    if (!UpdateOnChanged) {
                        UpdateOnChanged = true;
                        OnCommissionKindChanged?.Invoke();
                        UpdateOnChanged = false;
                    }
                }
            }
        }
        private CommissionKindViewModel _SelectedCommissionKindVM = default(CommissionKindViewModel);
        internal static readonly string _nameSelectedCommissionKindVM = PropertyName<MoveRegistrationWindowViewModel>.Get(x => x.SelectedCommissionKindVM);
        #endregion

        /// <summary>
        /// 手数料項目VMリスト
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
        internal static readonly string _nameItemVMList = PropertyName<MoveRegistrationWindowViewModel>.Get(x => x.ItemVMList);
        #endregion
        /// <summary>
        /// 選択された手数料項目VM
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
        internal static readonly string _nameSelectedItemVM = PropertyName<MoveRegistrationWindowViewModel>.Get(x => x.SelectedItemVM);
        #endregion

        /// <summary>
        /// 手数料
        /// </summary>
        #region Commission
        public int? Commission
        {
            get { return _Commission; }
            set {
                if (_Commission != value) {
                    _Commission = value;
                    PropertyChanged?.Raise(this, _nameCommission);
                }
            }
        }
        private int? _Commission = null;
        internal static readonly string _nameCommission = PropertyName<MoveRegistrationWindowViewModel>.Get(x => x.Commission);
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
        internal static readonly string _nameRemarkList = PropertyName<MoveRegistrationWindowViewModel>.Get(x => x.RemarkList);
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
        internal static readonly string _nameSelectedRemark = PropertyName<MoveRegistrationWindowViewModel>.Get(x => x.SelectedRemark);
        #endregion

        /// <summary>
        /// プロパティ変更イベントハンドラ
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}

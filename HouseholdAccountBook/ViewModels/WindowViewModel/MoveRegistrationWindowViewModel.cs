using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 帳簿項目登録ウィンドウ(移動)VM
    /// </summary>
    public class MoveRegistrationWindowViewModel : BindableBase
    {
        #region フィールド
        /// <summary>
        /// 変更時処理重複防止用フラグ
        /// </summary>
        private bool isUpdateOnChanged = false;
        #endregion

        #region イベント
        /// <summary>
        /// 移動元帳簿変更時イベント
        /// </summary>
        public event Action FromBookChanged = default(Action);
        /// <summary>
        /// 移動先帳簿変更時イベント
        /// </summary>
        public event Action ToBookChanged = default(Action);
        /// <summary>
        /// 手数料種別変更時イベント
        /// </summary>
        public event Action CommissionKindChanged = default(Action);
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
        /// 選択された移動元帳簿VM
        /// </summary>
        #region SelectedFromBookVM
        public BookViewModel SelectedFromBookVM
        {
            get { return this._SelectedFromBookVM; }
            set {
                if (SetProperty(ref this._SelectedFromBookVM, value)) {
                    if (!this.isUpdateOnChanged) {
                        this.isUpdateOnChanged = true;
                        FromBookChanged?.Invoke();
                        this.isUpdateOnChanged = false;
                    }
                }
            }
        }
        private BookViewModel _SelectedFromBookVM = default(BookViewModel);
        #endregion
        /// <summary>
        /// 選択された移動先帳簿VM
        /// </summary>
        #region SelectedToBookVM
        public BookViewModel SelectedToBookVM
        {
            get { return this._SelectedToBookVM; }
            set {
                if (SetProperty(ref this._SelectedToBookVM, value)) {
                    if (!this.isUpdateOnChanged) {
                        this.isUpdateOnChanged = true;
                        ToBookChanged?.Invoke();
                        this.isUpdateOnChanged = false;
                    }
                }
            }
        }
        private BookViewModel _SelectedToBookVM = default(BookViewModel);
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
        /// 手数料種別辞書
        /// </summary>
        #region CommissionKindDic
        public Dictionary<CommissionKind, string> CommissionKindDic { get; } = CommissionKindStr;
        #endregion
        /// <summary>
        /// 選択された手数料種別
        /// </summary>
        #region SelectedCommissionKind
        public CommissionKind SelectedCommissionKind
        {
            get { return this._SelectedCommissionKind; }
            set {
                if (SetProperty(ref this._SelectedCommissionKind, value)) {
                    if (!this.isUpdateOnChanged) {
                        this.isUpdateOnChanged = true;
                        CommissionKindChanged?.Invoke();
                        this.isUpdateOnChanged = false;
                    }
                }
            }
        }
        private CommissionKind _SelectedCommissionKind = default(CommissionKind);
        #endregion

        /// <summary>
        /// 手数料項目VMリスト
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
        /// 選択された手数料項目VM
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
        /// 手数料
        /// </summary>
        #region Commission
        public int? Commission
        {
            get { return this._Commission; }
            set {
                if (SetProperty(ref this._Commission, value)) {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        private int? _Commission = null;
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
        #endregion
    }
}

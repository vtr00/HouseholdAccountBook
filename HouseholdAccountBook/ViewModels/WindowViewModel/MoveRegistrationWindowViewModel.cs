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
        public event Action FromBookChanged = default;
        /// <summary>
        /// 移動先帳簿変更時イベント
        /// </summary>
        public event Action ToBookChanged = default;
        /// <summary>
        /// 手数料種別変更時イベント
        /// </summary>
        public event Action CommissionKindChanged = default;
        /// <summary>
        /// 項目変更時イベント
        /// </summary>
        public event Action ItemChanged = default;
        #endregion

        #region プロパティ
        /// <summary>
        /// 登録モード
        /// </summary>
        #region RegMode
        public RegistrationMode RegMode
        {
            get => this._RegMode;
            set => this.SetProperty(ref this._RegMode, value);
        }
        private RegistrationMode _RegMode = default;
        #endregion

        /// <summary>
        /// 日付(移動元)
        /// </summary>
        #region MovedDate
        public DateTime MovedDate
        {
            get => this._MovedDate;
            set {
                if (this.SetProperty(ref this._MovedDate, value)) {
                    CommandManager.InvalidateRequerySuggested();

                    if (this.MovingDate < value || this.IsLink) {
                        this.MovingDate = value;
                    }
                }
            }
        }
        private DateTime _MovedDate = DateTime.Today;
        #endregion
        /// <summary>
        /// 日付(移動先)
        /// </summary>
        #region MovingDate
        public DateTime MovingDate
        {
            get => this._MovingDate;
            set {
                if (this.SetProperty(ref this._MovingDate, value)) {
                    CommandManager.InvalidateRequerySuggested();

                    if (value < this.MovedDate) {
                        this.MovedDate = value;
                    }
                }
            }
        }
        private DateTime _MovingDate = DateTime.Today;
        #endregion
        /// <summary>
        /// 連動して編集
        /// </summary>
        #region IsLink
        public bool IsLink
        {
            get => this._IsLink;
            set {
                if (this.SetProperty(ref this._IsLink, value)) {
                    if (value) {
                        this.MovingDate = this.MovedDate;
                    }
                }
            }
        }
        private bool _IsLink = true;
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
        /// 選択された移動元帳簿VM
        /// </summary>
        #region MovedBookVM
        public BookViewModel MovedBookVM
        {
            get => this._MovedBookVM;
            set {
                if (this.SetProperty(ref this._MovedBookVM, value)) {
                    if (!this.isUpdateOnChanged) {
                        this.isUpdateOnChanged = true;
                        FromBookChanged?.Invoke();
                        this.isUpdateOnChanged = false;
                    }
                }
            }
        }
        private BookViewModel _MovedBookVM = default;
        #endregion
        /// <summary>
        /// 選択された移動先帳簿VM
        /// </summary>
        #region MovingBookVM
        public BookViewModel MovingBookVM
        {
            get => this._MovingBookVM;
            set {
                if (this.SetProperty(ref this._MovingBookVM, value)) {
                    if (!this.isUpdateOnChanged) {
                        this.isUpdateOnChanged = true;
                        ToBookChanged?.Invoke();
                        this.isUpdateOnChanged = false;
                    }
                }
            }
        }
        private BookViewModel _MovingBookVM = default;
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
            get => this._SelectedCommissionKind;
            set {
                if (this.SetProperty(ref this._SelectedCommissionKind, value)) {
                    if (!this.isUpdateOnChanged) {
                        this.isUpdateOnChanged = true;
                        CommissionKindChanged?.Invoke();
                        this.isUpdateOnChanged = false;
                    }
                }
            }
        }
        private CommissionKind _SelectedCommissionKind = default;
        #endregion

        /// <summary>
        /// 手数料項目VMリスト
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
        /// 選択された手数料項目VM
        /// </summary>
        #region SelectedItemVM
        public ItemViewModel SelectedItemVM
        {
            get => this._SelectedItemVM;
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
        private ItemViewModel _SelectedItemVM = default;
        #endregion

        /// <summary>
        /// 手数料
        /// </summary>
        #region Commission
        public int? Commission
        {
            get => this._Commission;
            set {
                if (this.SetProperty(ref this._Commission, value)) {
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
            get => this._RemarkList;
            set => this.SetProperty(ref this._RemarkList, value);
        }
        private ObservableCollection<string> _RemarkList = default;
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
        #endregion
    }
}

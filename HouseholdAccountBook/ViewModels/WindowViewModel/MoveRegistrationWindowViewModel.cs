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
        public event Action<int?> FromBookChanged = default;
        /// <summary>
        /// 移動元日時変更時イベント
        /// </summary>
        public event Action<DateTime> FromDateChanged = default;
        /// <summary>
        /// 移動先帳簿変更時イベント
        /// </summary>
        public event Action<int?> ToBookChanged = default;
        /// <summary>
        /// 移動先日時変更時イベント
        /// </summary>
        public event Action<DateTime> ToDateChanged = default;
        /// <summary>
        /// 手数料種別変更時イベント
        /// </summary>
        public event Action<CommissionKind> CommissionKindChanged = default;
        /// <summary>
        /// 項目変更時イベント
        /// </summary>
        public event Action<int?> ItemChanged = default;
        #endregion

        #region プロパティ
        /// <summary>
        /// 登録モード
        /// </summary>
        #region RegMode
        public RegistrationKind RegMode
        {
            get => this._RegMode;
            set => this.SetProperty(ref this._RegMode, value);
        }
        private RegistrationKind _RegMode = default;
        #endregion

        /// <summary>
        /// 移動元帳簿項目ID
        /// </summary>
        #region FromId
        public int? FromId
        {
            get => this._FromId;
            set => this.SetProperty(ref this._FromId, value);
        }
        private int? _FromId = default;
        #endregion
        /// <summary>
        /// 移動先帳簿項目ID
        /// </summary>
        #region ToId
        public int? ToId
        {
            get => this._ToId;
            set => this.SetProperty(ref this._ToId, value);
        }
        private int? _ToId = default;
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
        #region SelectedFromBookVM
        public BookViewModel SelectedFromBookVM
        {
            get => this._SelectedFromBookVM;
            set {
                if (this.SetProperty(ref this._SelectedFromBookVM, value)) {
                    if (!this.isUpdateOnChanged) {
                        this.isUpdateOnChanged = true;
                        this.FromBookChanged?.Invoke(value.Id);
                        this.isUpdateOnChanged = false;
                    }
                }
            }
        }
        private BookViewModel _SelectedFromBookVM = default;
        #endregion
        /// <summary>
        /// 選択された移動先帳簿VM
        /// </summary>
        #region SelectedToBookVM
        public BookViewModel SelectedToBookVM
        {
            get => this._SelectedToBookVM;
            set {
                if (this.SetProperty(ref this._SelectedToBookVM, value)) {
                    if (!this.isUpdateOnChanged) {
                        this.isUpdateOnChanged = true;
                        this.ToBookChanged?.Invoke(value.Id);
                        this.isUpdateOnChanged = false;
                    }
                }
            }
        }
        private BookViewModel _SelectedToBookVM = default;
        #endregion

        /// <summary>
        /// 日付(移動元)
        /// </summary>
        #region FromDate
        public DateTime FromDate
        {
            get => this._FromDate;
            set {
                if (this.SetProperty(ref this._FromDate, value)) {
                    this.FromDateChanged?.Invoke(value);
                    CommandManager.InvalidateRequerySuggested();

                    if (this.ToDate < value || this.IsLink) {
                        this.ToDate = value;
                    }
                }
            }
        }
        private DateTime _FromDate = DateTime.Today;
        #endregion
        /// <summary>
        /// 日付(移動先)
        /// </summary>
        #region ToDate
        public DateTime ToDate
        {
            get => this._ToDate;
            set {
                if (this.SetProperty(ref this._ToDate, value)) {
                    if (!this.IsLink) {
                        this.ToDateChanged?.Invoke(value);
                    }
                    CommandManager.InvalidateRequerySuggested();

                    if (value < this.FromDate) {
                        this.FromDate = value;
                        this.IsLink = true;
                    }
                }
            }
        }
        private DateTime _ToDate = DateTime.Today;
        #endregion
        /// <summary>
        /// 移動先日時が移動元日時に連動して編集
        /// </summary>
        #region IsLink
        public bool IsLink
        {
            get => this._IsLink;
            set {
                if (this.SetProperty(ref this._IsLink, value)) {
                    if (value) {
                        this.ToDate = this.FromDate;
                    }
                }
            }
        }
        private bool _IsLink = true;
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
        /// 手数料ID
        /// </summary>
        #region CommissionId
        public int? CommissionId
        {
            get => this._CommissionId;
            set => this.SetProperty(ref this._CommissionId, value);
        }
        private int? _CommissionId = default;
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
                        CommissionKindChanged?.Invoke(value);
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
                        ItemChanged?.Invoke(value?.Id);
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
        #endregion
    }
}

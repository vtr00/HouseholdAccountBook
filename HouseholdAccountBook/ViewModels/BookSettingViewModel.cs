using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 帳簿VM(設定用)
    /// </summary>
    public class BookSettingViewModel : BindableBase
    {
        #region プロパティ
        /// <summary>
        /// 帳簿ID
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// 帳簿名
        /// </summary>
        #region Name
        public string Name
        {
            get { return this._Name; }
            set { this.SetProperty(ref this._Name, value); }
        }
        private string _Name = default(string);
        #endregion

        /// <summary>
        /// 帳簿種別辞書
        /// </summary>
        public Dictionary<BookKind, string> BookKindDic { get; } = BookKindStr;
        /// <summary>
        /// 選択された帳簿種別
        /// </summary>
        #region SelectedBookKind
        public BookKind SelectedBookKind
        {
            get { return this._SelectedBookKind; }
            set {
                this.SetProperty(ref this._SelectedBookKind, value);
                this.UpdateNeedToPay();
                this.UpdateCsvDataExists();
            }
        }
        private BookKind _SelectedBookKind = BookKind.Uncategorized;
        #endregion

        /// <summary>
        /// 初期値
        /// </summary>
        #region InitialValue
        public int InitialValue
        {
            get { return this._InitialValue; }
            set { this.SetProperty(ref this._InitialValue, value); }
        }
        private int _InitialValue = 0;
        #endregion

        #region 支払情報
        /// <summary>
        /// 支払の必要があるか
        /// </summary>
        #region NeedToPay
        public bool NeedToPay
        {
            get { return this._NeedToPay; }
            private set { this.SetProperty(ref this._NeedToPay, value); }
        }
        private bool _NeedToPay = default(bool);
        #endregion

        /// <summary>
        /// 支払元帳簿VMリスト
        /// </summary>
        #region DebitBookVMList
        public ObservableCollection<BookViewModel> DebitBookVMList
        {
            get { return this._DebitBookVMList; }
            set { this.SetProperty(ref this._DebitBookVMList, value); }
        }
        private ObservableCollection<BookViewModel> _DebitBookVMList = default(ObservableCollection<BookViewModel>);
        #endregion
        /// <summary>
        /// 選択された支払元帳簿VM
        /// </summary>
        #region SelectedDebitBookVM
        public BookViewModel SelectedDebitBookVM
        {
            get { return this._SelectedDebitBookVM; }
            set { this.SetProperty(ref this._SelectedDebitBookVM, value); }
        }
        private BookViewModel _SelectedDebitBookVM = default(BookViewModel);
        #endregion

        /// <summary>
        /// 支払日
        /// </summary>
        #region PayDay
        public int? PayDay
        {
            get { return this._PayDay; }
            set { this.SetProperty(ref this._PayDay, value); }
        }
        private int? _PayDay = default(int?);
        #endregion
        #endregion

        #region CSV情報
        /// <summary>
        /// CSVデータがあるか
        /// </summary>
        #region CsvDataExists
        public bool CsvDataExists
        {
            get { return this._CsvDataExists; }
            private set { this.SetProperty(ref this._CsvDataExists, value); }
        }
        private bool _CsvDataExists = default(bool);
        #endregion
        
        /// <summary>
        /// 日付インデックス
        /// </summary>
        #region ActDateIndex
        public int? ActDateIndex
        {
            get { return this._ActDateIndex; }
            set { this.SetProperty(ref this._ActDateIndex, value); }
        }
        private int? _ActDateIndex = default(int?);
        #endregion

        /// <summary>
        /// 支出インデックス
        /// </summary>
        #region OutgoIndex
        public int? OutgoIndex
        {
            get { return this._OutgoIndex; }
            set { this.SetProperty(ref this._OutgoIndex, value); }
        }
        private int? _OutgoIndex = default(int?);
        #endregion

        /// <summary>
        /// 項目名インデックス
        /// </summary>
        #region ItemNameIndex
        public int? ItemNameIndex
        {
            get { return this._ItemNameIndex; }
            set { this.SetProperty(ref this._ItemNameIndex, value); }
        }
        private int? _ItemNameIndex = default(int?);
        #endregion
        #endregion

        /// <summary>
        /// 関係性VMリスト
        /// </summary>
        #region RelationVMList
        public ObservableCollection<RelationViewModel> RelationVMList
        {
            get { return this._RelationVMList; }
            set { this.SetProperty(ref this._RelationVMList, value); }
        }
        private ObservableCollection<RelationViewModel> _RelationVMList = default(ObservableCollection<RelationViewModel>);
        #endregion
        /// <summary>
        /// 選択された関係性VM
        /// </summary>
        #region SelectedRelationVM
        public RelationViewModel SelectedRelationVM
        {
            get { return this._SelectedRelationVM; }
            set { this.SetProperty(ref this._SelectedRelationVM, value); }
        }
        private RelationViewModel _SelectedRelationVM = default(RelationViewModel);
        #endregion
        #endregion

        /// <summary>
        /// 支払いの必要の有無を更新する
        /// </summary>
        private void UpdateNeedToPay()
        {
            this.NeedToPay = this.SelectedBookKind == BookKind.CreditCard;
        }

        /// <summary>
        /// CSVデータの有無を更新する
        /// </summary>
        private void UpdateCsvDataExists()
        {
            this.CsvDataExists = this.SelectedBookKind != BookKind.Wallet;
        }
    }
}

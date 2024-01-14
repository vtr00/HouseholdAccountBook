using Prism.Mvvm;
using System;
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
        /// ソート順
        /// </summary>
        #region SortOrder
        public int SortOrder
        {
            get => this._SortOrder;
            set => this.SetProperty(ref this._SortOrder, value);
        }
        private int _SortOrder = default;
        #endregion

        /// <summary>
        /// 帳簿名
        /// </summary>
        #region Name
        public string Name
        {
            get => this._Name;
            set => this.SetProperty(ref this._Name, value);
        }
        private string _Name = default;
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
            get => this._SelectedBookKind;
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
            get => this._InitialValue;
            set => this.SetProperty(ref this._InitialValue, value);
        }
        private int _InitialValue = 0;
        #endregion

        #region 期間情報
        /// <summary>
        /// 開始日の有無
        /// </summary>
        #region StartDateExists
        public bool StartDateExists
        {
            get => this._StartDateExists;
            set => this.SetProperty(ref this._StartDateExists, value);
        }
        private bool _StartDateExists = default;
        #endregion
        /// <summary>
        /// 開始日
        /// </summary>
        #region StartDate
        public DateTime StartDate
        {
            get => this._StartDate;
            set => this.SetProperty(ref this._StartDate, value);
        }
        private DateTime _StartDate = DateTime.Now;
        #endregion

        /// <summary>
        /// 終了日の有無
        /// </summary>
        #region EndDateExists
        public bool EndDateExists
        {
            get => this._EndDateExists;
            set => this.SetProperty(ref this._EndDateExists, value);
        }
        private bool _EndDateExists = default;
        #endregion
        /// <summary>
        /// 終了日
        /// </summary>
        #region EndDate
        public DateTime EndDate
        {
            get => this._EndDate;
            set => this.SetProperty(ref this._EndDate, value);
        }
        private DateTime _EndDate = DateTime.Now;
        #endregion
        #endregion

        #region 支払情報
        /// <summary>
        /// 支払の必要があるか
        /// </summary>
        #region NeedToPay
        public bool NeedToPay
        {
            get => this._NeedToPay;
            private set => this.SetProperty(ref this._NeedToPay, value);
        }
        private bool _NeedToPay = default;
        #endregion

        /// <summary>
        /// 支払元帳簿VMリスト
        /// </summary>
        #region DebitBookVMList
        public ObservableCollection<BookViewModel> DebitBookVMList
        {
            get => this._DebitBookVMList;
            set => this.SetProperty(ref this._DebitBookVMList, value);
        }
        private ObservableCollection<BookViewModel> _DebitBookVMList = default;
        #endregion
        /// <summary>
        /// 選択された支払元帳簿VM
        /// </summary>
        #region SelectedDebitBookVM
        public BookViewModel SelectedDebitBookVM
        {
            get => this._SelectedDebitBookVM;
            set => this.SetProperty(ref this._SelectedDebitBookVM, value);
        }
        private BookViewModel _SelectedDebitBookVM = default;
        #endregion

        /// <summary>
        /// 支払日
        /// </summary>
        #region PayDay
        public int? PayDay
        {
            get => this._PayDay;
            set => this.SetProperty(ref this._PayDay, value);
        }
        private int? _PayDay = default;
        #endregion
        #endregion

        #region CSV情報
        /// <summary>
        /// CSVデータがあるか
        /// </summary>
        #region CsvDataExists
        public bool CsvDataExists
        {
            get => this._CsvDataExists;
            private set => this.SetProperty(ref this._CsvDataExists, value);
        }
        private bool _CsvDataExists = default;
        #endregion

        /// <summary>
        /// CSVフォルダパス
        /// </summary>
        #region CsvFolderPath
        public string CsvFolderPath
        {
            get => this._CsvFolderPath;
            set => this.SetProperty(ref this._CsvFolderPath, value);
        }
        private string _CsvFolderPath = default;
        #endregion

        /// <summary>
        /// 日付インデックス
        /// </summary>
        #region ActDateIndex
        public int? ActDateIndex
        {
            get => this._ActDateIndex;
            set => this.SetProperty(ref this._ActDateIndex, value);
        }
        private int? _ActDateIndex = default;
        #endregion

        /// <summary>
        /// 支出インデックス
        /// </summary>
        #region OutgoIndex
        public int? OutgoIndex
        {
            get => this._OutgoIndex;
            set => this.SetProperty(ref this._OutgoIndex, value);
        }
        private int? _OutgoIndex = default;
        #endregion

        /// <summary>
        /// 項目名インデックス
        /// </summary>
        #region ItemNameIndex
        public int? ItemNameIndex
        {
            get => this._ItemNameIndex;
            set => this.SetProperty(ref this._ItemNameIndex, value);
        }
        private int? _ItemNameIndex = default;
        #endregion
        #endregion

        /// <summary>
        /// 関係性VMリスト
        /// </summary>
        #region RelationVMList
        public ObservableCollection<RelationViewModel> RelationVMList
        {
            get => this._RelationVMList;
            set => this.SetProperty(ref this._RelationVMList, value);
        }
        private ObservableCollection<RelationViewModel> _RelationVMList = default;
        #endregion
        /// <summary>
        /// 選択された関係性VM
        /// </summary>
        #region SelectedRelationVM
        public RelationViewModel SelectedRelationVM
        {
            get => this._SelectedRelationVM;
            set => this.SetProperty(ref this._SelectedRelationVM, value);
        }
        private RelationViewModel _SelectedRelationVM = default;
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

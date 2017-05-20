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
            get { return _Name; }
            set { SetProperty(ref _Name, value); }
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
            get { return _SelectedBookKind; }
            set {
                SetProperty(ref _SelectedBookKind, value);
                UpdateNeedToPay();
                UpdateCsvDataExists();
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
            get { return _InitialValue; }
            set { SetProperty(ref _InitialValue, value); }
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
            get { return _NeedToPay; }
            private set { SetProperty(ref _NeedToPay, value); }
        }
        private bool _NeedToPay = default(bool);
        #endregion

        /// <summary>
        /// 支払元帳簿VMリスト
        /// </summary>
        #region DebitBookVMList
        public ObservableCollection<BookViewModel> DebitBookVMList
        {
            get { return _DebitBookVMList; }
            set { SetProperty(ref _DebitBookVMList, value); }
        }
        private ObservableCollection<BookViewModel> _DebitBookVMList = default(ObservableCollection<BookViewModel>);
        #endregion
        /// <summary>
        /// 選択された支払元帳簿VM
        /// </summary>
        #region SelectedDebitBookVM
        public BookViewModel SelectedDebitBookVM
        {
            get { return _SelectedDebitBookVM; }
            set { SetProperty(ref _SelectedDebitBookVM, value); }
        }
        private BookViewModel _SelectedDebitBookVM = default(BookViewModel);
        #endregion

        /// <summary>
        /// 支払日
        /// </summary>
        #region PayDay
        public int? PayDay
        {
            get { return _PayDay; }
            set { SetProperty(ref _PayDay, value); }
        }
        private int? _PayDay = default(int?);
        #endregion

        /// <summary>
        /// 支払いの必要の有無を更新する
        /// </summary>
        private void UpdateNeedToPay()
        {
            NeedToPay = SelectedBookKind == BookKind.CreditCard;
        }
        #endregion

        #region CSV情報
        /// <summary>
        /// CSVデータがあるか
        /// </summary>
        #region CsvDataExists
        public bool CsvDataExists
        {
            get { return _CsvDataExists; }
            private set { SetProperty(ref _CsvDataExists, value); }
        }
        private bool _CsvDataExists = default(bool);
        #endregion
        
        /// <summary>
        /// 日付インデックス
        /// </summary>
        #region ActDateIndex
        public int? ActDateIndex
        {
            get { return _ActDateIndex; }
            set { SetProperty(ref _ActDateIndex, value); }
        }
        private int? _ActDateIndex = default(int?);
        #endregion

        /// <summary>
        /// 支出インデックス
        /// </summary>
        #region OutgoIndex
        public int? OutgoIndex
        {
            get { return _OutgoIndex; }
            set { SetProperty(ref _OutgoIndex, value); }
        }
        private int? _OutgoIndex = default(int?);
        #endregion

        /// <summary>
        /// 項目名インデックス
        /// </summary>
        #region ItemNameIndex
        public int? ItemNameIndex
        {
            get { return _ItemNameIndex; }
            set { SetProperty(ref _ItemNameIndex, value); }
        }
        private int? _ItemNameIndex = default(int?);
        #endregion

        /// <summary>
        /// CSVデータの有無を更新する
        /// </summary>
        private void UpdateCsvDataExists()
        {
            CsvDataExists = SelectedBookKind != BookKind.Wallet;
        }
        #endregion

        /// <summary>
        /// 関係性VMリスト
        /// </summary>
        #region RelationVMList
        public ObservableCollection<RelationViewModel> RelationVMList
        {
            get { return _RelationVMList; }
            set { SetProperty(ref _RelationVMList, value); }
        }
        private ObservableCollection<RelationViewModel> _RelationVMList = default(ObservableCollection<RelationViewModel>);
        #endregion
        /// <summary>
        /// 選択された関係性VM
        /// </summary>
        #region SelectedRelationVM
        public RelationViewModel SelectedRelationVM
        {
            get { return _SelectedRelationVM; }
            set { SetProperty(ref _SelectedRelationVM, value); }
        }
        private RelationViewModel _SelectedRelationVM = default(RelationViewModel);
        #endregion

    }
}

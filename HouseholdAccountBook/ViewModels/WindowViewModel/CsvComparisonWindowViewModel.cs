using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// CSV比較ウィンドウVM
    /// </summary>
    public class CsvComparisonWindowViewModel : BindableBase
    {
        /// <summary>
        /// 帳簿変更時のイベント
        /// </summary>
        public event Action<int?> BookChanged;

        #region プロパティ
        /// <summary>
        /// CSVファイル名
        /// </summary>
        #region CsvFileName
        public string CsvFileName
        {
            get => this._CsvFileName;
            set => this.SetProperty(ref this._CsvFileName, value);
        }
        private string _CsvFileName = default;
        #endregion

        /// <summary>
        /// 帳簿VMリスト
        /// </summary>
        #region BookVMList
        public ObservableCollection<BookComparisonViewModel> BookVMList
        {
            get => this._BookVMList;
            set => this.SetProperty(ref this._BookVMList, value);
        }
        private ObservableCollection<BookComparisonViewModel> _BookVMList = default;
        #endregion
        /// <summary>
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookComparisonViewModel SelectedBookVM
        {
            get => this._SelectedBookVM;
            set {
                if (this.SetProperty(ref this._SelectedBookVM, value)) {
                    this.BookChanged?.Invoke(value.Id);
                }
            }
        }
        private BookComparisonViewModel _SelectedBookVM = new BookComparisonViewModel() { };
        #endregion

        /// <summary>
        /// CSV比較VMリスト
        /// </summary>
        #region CsvComparisonVMList
        public ObservableCollection<CsvComparisonViewModel> CsvComparisonVMList
        {
            get => this._CsvComparisonVMList;
            set => this.SetProperty(ref this._CsvComparisonVMList, value);
        }
        private ObservableCollection<CsvComparisonViewModel> _CsvComparisonVMList = new ObservableCollection<CsvComparisonViewModel>();
        #endregion
        /// <summary>
        /// 選択されたCSV比較VM(先頭)
        /// </summary>
        #region SelectedCsvComparisonVM
        public CsvComparisonViewModel SelectedCsvComparisonVM
        {
            get => this._SelectedCsvComparisonVM;
            set => this.SetProperty(ref this._SelectedCsvComparisonVM, value);
        }
        private CsvComparisonViewModel _SelectedCsvComparisonVM = default;
        #endregion
        /// <summary>
        /// 選択されたCSV比較VMリスト
        /// </summary>
        #region SelectedCsvComparisonVMList
        public ObservableCollection<CsvComparisonViewModel> SelectedCsvComparisonVMList { get; } = new ObservableCollection<CsvComparisonViewModel>();
        #endregion

        /// <summary>
        /// 未チェック数
        /// </summary>
        /// <remarks>UI上に反映するには？</remarks>
        #region UncheckedNum
        public int? UncheckedNum
        {
            get {
                if (this.CsvComparisonVMList.Count == 0) return null;
                return this.CsvComparisonVMList.Count((tmp) => !tmp.IsMatch);
            }
        }
        #endregion
        /// <summary>
        /// 合計値
        /// </summary>
        #region SumValue
        public int? SumValue
        {
            get {
                int? sum = (this.SelectedCsvComparisonVMList.Count != 0) ? (int?)0 : null;
                foreach (CsvComparisonViewModel vm in this.SelectedCsvComparisonVMList) {
                    sum += vm.Record.Value;
                }
                return sum;
            }
        }
        #endregion
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CsvComparisonWindowViewModel()
        {
            this.SelectedCsvComparisonVMList.CollectionChanged += (sender, args) => {
                this.RaisePropertyChanged(nameof(this.SumValue));
            };
        }
    }
}

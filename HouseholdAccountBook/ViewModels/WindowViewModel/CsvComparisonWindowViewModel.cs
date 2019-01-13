using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// CSV比較ウィンドウVM
    /// </summary>
    public class CsvComparisonWindowViewModel : BindableBase
    {
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
        private string _CsvFileName = default(string);
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
        private ObservableCollection<BookComparisonViewModel> _BookVMList = default(ObservableCollection<BookComparisonViewModel>);
        #endregion
        /// <summary>
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookComparisonViewModel SelectedBookVM
        {
            get => this._SelectedBookVM;
            set => this.SetProperty(ref this._SelectedBookVM, value);
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
        /// 選択されたCSV比較VM
        /// </summary>
        #region SelectedCsvComparisonVM
        public CsvComparisonViewModel SelectedCsvComparisonVM
        {
            get => this._SelectedCsvComparisonVM;
            set => this.SetProperty(ref this._SelectedCsvComparisonVM, value);
        }
        private CsvComparisonViewModel _SelectedCsvComparisonVM = default(CsvComparisonViewModel);
        #endregion

        /// <summary>
        /// 合計値
        /// </summary>
        #region SumValue
        public int? SumValue
        {
            get => this._SumValue;
            set => this.SetProperty(ref this._SumValue, value);
        }
        private int? _SumValue = default(int?);
        #endregion
        
        /// <summary>
        /// デバッグビルドか
        /// </summary>
        #region IsDebug
#if DEBUG
        public bool IsDebug => true;
#else
        public bool IsDebug => false;
#endif
        #endregion
        #endregion
    }
}

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
            get { return this._CsvFileName; }
            set { SetProperty(ref this._CsvFileName, value); }
        }
        private string _CsvFileName = default(string);
        #endregion
        
        /// <summary>
        /// 帳簿VMリスト
        /// </summary>
        #region BookVMList
        public ObservableCollection<BookComparisonViewModel> BookVMList
        {
            get { return this._BookVMList; }
            set { SetProperty(ref this._BookVMList, value); }
        }
        private ObservableCollection<BookComparisonViewModel> _BookVMList = default(ObservableCollection<BookComparisonViewModel>);
        #endregion
        /// <summary>
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookComparisonViewModel SelectedBookVM
        {
            get { return this._SelectedBookVM; }
            set { SetProperty(ref this._SelectedBookVM, value); }
        }
        private BookComparisonViewModel _SelectedBookVM;
        #endregion

        /// <summary>
        /// CSV比較VMリスト
        /// </summary>
        #region CsvComparisonVMList
        public ObservableCollection<CsvComparisonViewModel> CsvComparisonVMList
        {
            get { return this._CsvComparisonVMList; }
            set { SetProperty(ref this._CsvComparisonVMList, value); }
        }
        private ObservableCollection<CsvComparisonViewModel> _CsvComparisonVMList = new ObservableCollection<CsvComparisonViewModel>();
        #endregion
        /// <summary>
        /// 選択されたCSV比較VM
        /// </summary>
        #region SelectedCsvComparisonVM
        public CsvComparisonViewModel SelectedCsvComparisonVM
        {
            get { return this._SelectedCsvComparisonVM; }
            set { SetProperty(ref this._SelectedCsvComparisonVM, value); }
        }
        private CsvComparisonViewModel _SelectedCsvComparisonVM = default(CsvComparisonViewModel);
        #endregion

        /// <summary>
        /// デバッグビルドか
        /// </summary>
        public bool IsDebug
        {
            get {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }
        #endregion
    }
}

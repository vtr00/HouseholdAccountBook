using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// CSV比較ウィンドウVM
    /// </summary>
    public class CsvComparisonWindowViewModel : BindableBase
    {
        /// <summary>
        /// 帳簿VMリスト
        /// </summary>
        #region BookVMList
        public ObservableCollection<BookViewModel> BookVMList
        {
            get { return _BookVMList; }
            set { SetProperty(ref _BookVMList, value); }
        }
        private ObservableCollection<BookViewModel> _BookVMList = default(ObservableCollection<BookViewModel>);
        #endregion
        /// <summary>
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookViewModel SelectedBookVM
        {
            get { return _SelectedBookVM; }
            set { SetProperty(ref _SelectedBookVM, value); }
        }
        private BookViewModel _SelectedBookVM;
        #endregion

        /// <summary>
        /// CSV比較VMリスト
        /// </summary>
        #region CsvComparisonVMList
        public ObservableCollection<CsvComparisonViewModel> CsvComparisonVMList
        {
            get { return _CsvComparisonVMList; }
            set { SetProperty(ref _CsvComparisonVMList, value); }
        }
        private ObservableCollection<CsvComparisonViewModel> _CsvComparisonVMList = default(ObservableCollection<CsvComparisonViewModel>);
        #endregion
        /// <summary>
        /// 選択されたCSV比較VM
        /// </summary>
        #region SelectedCsvComparisonVM
        public CsvComparisonViewModel SelectedCsvComparisonVM
        {
            get { return _SelectedCsvComparisonVM; }
            set { SetProperty(ref _SelectedCsvComparisonVM, value); }
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
    }
}

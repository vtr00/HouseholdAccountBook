using Prism.Mvvm;
using System.Collections.ObjectModel;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 設定VM
    /// </summary>
    public class SettingsViewModel : BindableBase
    {
        /// <summary>
        /// 選択された設定タブインデックス
        /// </summary>
        #region SelectedTabIndex
        public int SelectedTabIndex
        {
            get { return _SelectedTabIndex; }
            set {
                if (SetProperty(ref _SelectedTabIndex, value)) {
                    SelectedTab = (SettingsTab)value;
                }
            }
        }
        private int _SelectedTabIndex = default(int);
        #endregion
        /// <summary>
        /// 選択された設定タブ種別
        /// </summary>
        #region SelectedTab
        public SettingsTab SelectedTab
        {
            get { return _SelectedTab; }
            set {
                if (SetProperty(ref _SelectedTab, value)) {
                    SelectedTabIndex = (int)value;
                }
            }
        }
        private SettingsTab _SelectedTab = default(SettingsTab);
        #endregion
        
        #region 項目設定
        /// <summary>
        /// 階層構造項目VMリスト
        /// </summary>
        #region HierachicalItemVMList
        public ObservableCollection<HierarchicalItemViewModel> HierachicalItemVMList
        {
            get { return _HierachicalItemVMList; }
            set { SetProperty(ref _HierachicalItemVMList, value); }
        }
        private ObservableCollection<HierarchicalItemViewModel> _HierachicalItemVMList = default(ObservableCollection<HierarchicalItemViewModel>);
        #endregion
        /// <summary>
        /// 選択された項目VM
        /// </summary>
        /// <remarks>Not Binded</remarks>
        #region SelectedItemVM
        public HierarchicalItemViewModel SelectedItemVM
        {
            get { return _SelectedItemVM; }
            set { SetProperty(ref _SelectedItemVM, value); }
        }
        private HierarchicalItemViewModel _SelectedItemVM = default(HierarchicalItemViewModel);
        #endregion
        #endregion

        #region 帳簿設定
        /// <summary>
        /// 帳簿VMリスト
        /// </summary>
        #region BookVMList
        public ObservableCollection<BookSettingViewModel> BookVMList
        {
            get { return _BookVMList; }
            set { SetProperty(ref _BookVMList, value); }
        }
        private ObservableCollection<BookSettingViewModel> _BookVMList = default(ObservableCollection<BookSettingViewModel>);
        #endregion

        /// <summary>
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookSettingViewModel SelectedBookVM
        {
            get { return _SelectedBookVM; }
            set { SetProperty(ref _SelectedBookVM, value); }
        }
        private BookSettingViewModel _SelectedBookVM = default(BookSettingViewModel);
        #endregion
        #endregion
        
        #region その他

        #endregion
    }
}

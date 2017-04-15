using Prism.Mvvm;
using System.Collections.ObjectModel;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 設定ウィンドウVM
    /// </summary>
    public class SettingsWindowViewModel : BindableBase
    {
        /// <summary>
        /// 設定
        /// </summary>
        Properties.Settings settings;

        /// <summary>
        /// 設定を保存するか
        /// </summary>
        public bool WithSave { get; set; } = true;

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
        /// <summary>
        /// pg_dump.exeパス
        /// </summary>
        #region DumpExePath
        public string DumpExePath
        {
            get { return _DumpExePath; }
            set {
                if (SetProperty(ref _DumpExePath, value) && WithSave) {
                    settings.App_Postgres_DumpExePath = value;
                    settings.Save();
                }
            }
        }
        private string _DumpExePath = default(string);
        #endregion

        /// <summary>
        /// pg_restore.exeパス
        /// </summary>
        #region RestoreExePath
        public string RestoreExePath
        {
            get { return _RestoreExePath; }
            set {
                if (SetProperty(ref _RestoreExePath, value) && WithSave) {
                    settings.App_Postgres_RestoreExePath = value;
                    settings.Save();
                }
            }
        }
        private string _RestoreExePath = default(string);
        #endregion

        /// <summary>
        /// バックアップ数
        /// </summary>
        #region BackUpNum
        public int BackUpNum
        {
            get { return _BackUpNum; }
            set {
                if (SetProperty(ref _BackUpNum, value) && WithSave) {
                    settings.App_BackUpNum = value;
                    settings.Save();
                }
            }
        }
        private int _BackUpNum = default(int);
        #endregion

        /// <summary>
        /// バックアップ先フォルダ
        /// </summary>
        #region BackUpFolderPath
        public string BackUpFolderPath
        {
            get { return _BackUpFolderPath; }
            set {
                if (SetProperty(ref _BackUpFolderPath, value) && WithSave) {
                    settings.App_BackUpFolderPath = value;
                    settings.Save();
                }
            }
        }
        private string _BackUpFolderPath = default(string);
        #endregion

        /// <summary>
        /// 開始月
        /// </summary>
        #region StartMonth
        public int StartMonth
        {
            get { return _StartMonth; }
            set {
                if (SetProperty(ref _StartMonth, value) && WithSave) {
                    settings.App_StartMonth = value;
                    settings.Save();
                }
            }
        }
        private int _StartMonth = default(int);
        #endregion
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SettingsWindowViewModel()
        {
            settings = Properties.Settings.Default;
        }

        /// <summary>
        /// 設定を読み込む
        /// </summary>
        public void LoadSettings()
        {
            this.WithSave = false;
            DumpExePath = settings.App_Postgres_DumpExePath;
            RestoreExePath = settings.App_Postgres_RestoreExePath;
            BackUpNum = settings.App_BackUpNum;
            BackUpFolderPath = settings.App_BackUpFolderPath;
            StartMonth = settings.App_StartMonth;
            this.WithSave = true;
        }
    }
}

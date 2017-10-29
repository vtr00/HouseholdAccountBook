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
        private Properties.Settings settings;

        #region プロパティ
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
            get { return this._SelectedTabIndex; }
            set {
                if (SetProperty(ref this._SelectedTabIndex, value)) {
                    this.SelectedTab = (SettingsTabs)value;
                }
            }
        }
        private int _SelectedTabIndex = default(int);
        #endregion
        /// <summary>
        /// 選択された設定タブ種別
        /// </summary>
        #region SelectedTab
        public SettingsTabs SelectedTab
        {
            get { return this._SelectedTab; }
            set {
                if (SetProperty(ref this._SelectedTab, value)) {
                    this.SelectedTabIndex = (int)value;
                }
            }
        }
        private SettingsTabs _SelectedTab = default(SettingsTabs);
        #endregion
        
        #region 項目設定
        /// <summary>
        /// 階層構造項目VMリスト
        /// </summary>
        #region HierachicalItemVMList
        public ObservableCollection<HierarchicalItemViewModel> HierachicalItemVMList
        {
            get { return this._HierachicalItemVMList; }
            set { SetProperty(ref this._HierachicalItemVMList, value); }
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
            get { return this._SelectedItemVM; }
            set { SetProperty(ref this._SelectedItemVM, value); }
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
            get { return this._BookVMList; }
            set { SetProperty(ref this._BookVMList, value); }
        }
        private ObservableCollection<BookSettingViewModel> _BookVMList = default(ObservableCollection<BookSettingViewModel>);
        #endregion

        /// <summary>
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookSettingViewModel SelectedBookVM
        {
            get { return this._SelectedBookVM; }
            set { SetProperty(ref this._SelectedBookVM, value); }
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
            get { return this._DumpExePath; }
            set {
                if (SetProperty(ref this._DumpExePath, value) && this.WithSave) {
                    this.settings.App_Postgres_DumpExePath = value;
                    this.settings.Save();
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
            get { return this._RestoreExePath; }
            set {
                if (SetProperty(ref this._RestoreExePath, value) && this.WithSave) {
                    this.settings.App_Postgres_RestoreExePath = value;
                    this.settings.Save();
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
            get { return this._BackUpNum; }
            set {
                if (SetProperty(ref this._BackUpNum, value) && this.WithSave) {
                    this.settings.App_BackUpNum = value;
                    this.settings.Save();
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
            get { return this._BackUpFolderPath; }
            set {
                if (SetProperty(ref this._BackUpFolderPath, value) && this.WithSave) {
                    this.settings.App_BackUpFolderPath = value;
                    this.settings.Save();
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
            get { return this._StartMonth; }
            set {
                if (SetProperty(ref this._StartMonth, value) && this.WithSave) {
                    this.settings.App_StartMonth = value;
                    this.settings.Save();
                }
            }
        }
        private int _StartMonth = default(int);
        #endregion
        #endregion
        #endregion

        /// <summary>
        /// <see cref="SettingsWindowViewModel"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        public SettingsWindowViewModel()
        {
            this.settings = Properties.Settings.Default;
        }

        /// <summary>
        /// 設定を読み込む
        /// </summary>
        public void LoadSettings()
        {
            this.WithSave = false;
            this.DumpExePath = this.settings.App_Postgres_DumpExePath;
            this.RestoreExePath = this.settings.App_Postgres_RestoreExePath;
            this.BackUpNum = this.settings.App_BackUpNum;
            this.BackUpFolderPath = this.settings.App_BackUpFolderPath;
            this.StartMonth = this.settings.App_StartMonth;
            this.WithSave = true;
        }
    }
}

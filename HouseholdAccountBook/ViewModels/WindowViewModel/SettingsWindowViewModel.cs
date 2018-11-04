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
            get => this._SelectedTabIndex;
            set {
                if (this.SetProperty(ref this._SelectedTabIndex, value)) {
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
            get => this._SelectedTab;
            set {
                if (this.SetProperty(ref this._SelectedTab, value)) {
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
            get => this._HierachicalItemVMList;
            set => this.SetProperty(ref this._HierachicalItemVMList, value);
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
            get => this._SelectedItemVM;
            set => this.SetProperty(ref this._SelectedItemVM, value);
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
            get => this._BookVMList;
            set => this.SetProperty(ref this._BookVMList, value);
        }
        private ObservableCollection<BookSettingViewModel> _BookVMList = default(ObservableCollection<BookSettingViewModel>);
        #endregion

        /// <summary>
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookSettingViewModel SelectedBookVM
        {
            get => this._SelectedBookVM;
            set => this.SetProperty(ref this._SelectedBookVM, value);
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
            get => this._DumpExePath;
            set {
                if (this.SetProperty(ref this._DumpExePath, value) && this.WithSave) {
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
            get => this._RestoreExePath;
            set {
                if (this.SetProperty(ref this._RestoreExePath, value) && this.WithSave) {
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
            get => this._BackUpNum;
            set {
                if (this.SetProperty(ref this._BackUpNum, value) && this.WithSave) {
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
            get => this._BackUpFolderPath;
            set {
                if (this.SetProperty(ref this._BackUpFolderPath, value) && this.WithSave) {
                    this.settings.App_BackUpFolderPath = value;
                    this.settings.Save();
                }
            }
        }
        private string _BackUpFolderPath = default(string);
        #endregion

        /// <summary>
        /// メインウィンドウ最小化時バックアップフラグ
        /// </summary>
        #region BackUpFlagAtMinimizing
        public bool BackUpFlagAtMinimizing
        {
            get => this._BackUpFlagAtMinimizing;
            set {
                if (this.SetProperty(ref this._BackUpFlagAtMinimizing, value) && this.WithSave) {
                    this.settings.App_BackUpFlagAtMinimizing = value;
                    this.settings.Save();
                }
            }
        }
        private bool _BackUpFlagAtMinimizing = default(bool);
        #endregion

        /// <summary>
        /// メインウィンドウクローズ時バックアップフラグ
        /// </summary>
        #region BackUpFlagAtClosing
        public bool BackUpFlagAtClosing
        {
            get => this._BackUpFlagAtClosing;
            set {
                if (this.SetProperty(ref this._BackUpFlagAtClosing, value) && this.WithSave) {
                    this.settings.App_BackUpFlagAtClosing = value;
                    this.settings.Save();
                }
            }
        }
        private bool _BackUpFlagAtClosing = default(bool);
        #endregion

        /// <summary>
        /// 開始月
        /// </summary>
        #region StartMonth
        public int StartMonth
        {
            get => this._StartMonth;
            set {
                if (this.SetProperty(ref this._StartMonth, value) && this.WithSave) {
                    this.settings.App_StartMonth = value;
                    this.settings.Save();
                }
            }
        }
        private int _StartMonth = default(int);
        #endregion

        /// <summary>
        /// 国民の祝日CSV URI
        /// </summary>
        #region NationalHolidayCsvURI
        public string NationalHolidayCsvURI
        {
            get => this._NationalHolidayCsvURI;
            set {
                if(this.SetProperty(ref this._NationalHolidayCsvURI, value) && this.WithSave) {
                    this.settings.App_NationalHolidayCsv_Uri = value;
                    this.settings.Save();
                }
            }
        }
        private string _NationalHolidayCsvURI = default(string);
        #endregion

        /// <summary>
        /// 国民の祝日CSV 日付位置(0開始)
        /// </summary>
        #region NationalHolidayCsvDateIndex
        public int NationalHolidayCsvDateIndex
        {
            get => this._NationalHolidayCsvDateIndex;
            set {
                if (this.SetProperty(ref this._NationalHolidayCsvDateIndex, value) && this.WithSave) {
                    this.settings.App_NationalHolidayCsv_DateIndex = value;
                    this.settings.Save();
                }
            }
        }
        private int _NationalHolidayCsvDateIndex = default(int);
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
            this.BackUpFlagAtMinimizing = this.settings.App_BackUpFlagAtMinimizing;
            this.BackUpFlagAtClosing = this.settings.App_BackUpFlagAtClosing;
            this.StartMonth = this.settings.App_StartMonth;
            this.NationalHolidayCsvURI = this.settings.App_NationalHolidayCsv_Uri;
            this.NationalHolidayCsvDateIndex = this.settings.App_NationalHolidayCsv_DateIndex;
            this.WithSave = true;
        }
    }
}

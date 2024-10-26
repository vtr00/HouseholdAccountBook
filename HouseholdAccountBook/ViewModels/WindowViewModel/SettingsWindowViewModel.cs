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
        private readonly Properties.Settings settings;

        #region プロパティ
        /// <summary>
        /// 設定を保存するか
        /// </summary>
        public bool WithSave { get; set; } = true;
        /// <summary>
        /// 表示更新の必要があるか
        /// </summary>
        public bool NeedToUpdate { get; set; } = false;

        /// <summary>
        /// 選択された設定タブインデックス
        /// </summary>
        #region SelectedTabIndex
        public int SelectedTabIndex
        {
            get => this._SelectedTabIndex;
            set {
                if (this.SetProperty(ref this._SelectedTabIndex, value)) {
                    this._SelectedTab = (SettingsTabs)value;
                }
            }
        }
        private int _SelectedTabIndex = default;
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
                    this._SelectedTabIndex = (int)value;
                }
            }
        }
        private SettingsTabs _SelectedTab = default;
        #endregion

        #region 項目設定
        /// <summary>
        /// 階層構造設定VMリスト
        /// </summary>
        #region HierachicalSettingVMList
        public ObservableCollection<HierarchicalSettingViewModel> HierachicalSettingVMList
        {
            get => this._HierachicalSettingVMList;
            set => this.SetProperty(ref this._HierachicalSettingVMList, value);
        }
        private ObservableCollection<HierarchicalSettingViewModel> _HierachicalSettingVMList = default;
        #endregion
        /// <summary>
        /// 選択された項目VM
        /// </summary>
        /// <remarks>Not Binded</remarks>
        #region SelectedItemVM
        public HierarchicalSettingViewModel SelectedItemVM
        {
            get => this._SelectedItemVM;
            set => this.SetProperty(ref this._SelectedItemVM, value);
        }
        private HierarchicalSettingViewModel _SelectedItemVM = default;
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
            set {
                this._BookVMList.Clear();
                foreach (BookSettingViewModel vm in value) {
                    this._BookVMList.Add(vm);
                }
                this.RaisePropertyChanged(nameof(this.BookVMList));
            }
        }
        private readonly ObservableCollection<BookSettingViewModel> _BookVMList = new ObservableCollection<BookSettingViewModel>();
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
        private BookSettingViewModel _SelectedBookVM = default;
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
        private string _DumpExePath = default;
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
        private string _RestoreExePath = default;
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
        private int _BackUpNum = default;
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
        private string _BackUpFolderPath = default;
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
        private bool _BackUpFlagAtMinimizing = default;
        #endregion

        /// <summary>
        /// メインウィンドウ最小化時バックアップインターバル(分)
        /// </summary>
        #region BackUpIntervalAtMinimizing
        public int BackUpIntervalAtMinimizing
        {
            get => this._BackUpIntervalAtMinimizing;
            set {
                if (this.SetProperty(ref this._BackUpIntervalAtMinimizing, value) && this.WithSave) {
                    this.settings.App_BackUpIntervalMinAtMinimizing = value;
                    this.settings.Save();
                }
            }
        }
        private int _BackUpIntervalAtMinimizing = default;
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
        private bool _BackUpFlagAtClosing = default;
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
                    this.NeedToUpdate = true;
                }
            }
        }
        private int _StartMonth = default;
        #endregion

        /// <summary>
        /// 国民の祝日CSV URI
        /// </summary>
        #region NationalHolidayCsvURI
        public string NationalHolidayCsvURI
        {
            get => this._NationalHolidayCsvURI;
            set {
                if (this.SetProperty(ref this._NationalHolidayCsvURI, value) && this.WithSave) {
                    this.settings.App_NationalHolidayCsv_Uri = value;
                    this.settings.Save();
                }
            }
        }
        private string _NationalHolidayCsvURI = default;
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
        private int _NationalHolidayCsvDateIndex = default;
        #endregion

        /// <summary>
        /// デバッグモード
        /// </summary>
        #region DebugMode
        public bool DebugMode
        {
            get => this._DebugMode;
            set {
                if (this.SetProperty(ref this._DebugMode, value) && this.WithSave) {
                    this.settings.App_IsDebug = value;
                    this.settings.Save();

                    // リソースを更新して他ウィンドウの項目の表示/非表示を切り替える
                    App app = System.Windows.Application.Current as App;
                    app.RegisterSettingsToResource();
                }
            }
        }
        private bool _DebugMode = default;
        #endregion

        /// <summary>
        /// ウィンドウ位置を保存するか
        /// </summary>
        #region IsPositionSaved
        public bool IsPositionSaved
        {
            get => this._IsPositionSaved;
            set {
                if (this.SetProperty(ref this._IsPositionSaved, value) && this.WithSave) {
                    this.settings.App_IsPositionSaved = value;
                    this.settings.Save();
                }
            }
        }
        private bool _IsPositionSaved = default;
        #endregion

        /// <summary>
        /// ウィンドウ設定
        /// </summary>
        #region WindowSettingVMList
        public ObservableCollection<WindowSettingViewModel> WindowSettingVMList
        {
            get => this._WindowSettingVMList;
            set => this.SetProperty(ref this._WindowSettingVMList, value);
        }
        private ObservableCollection<WindowSettingViewModel> _WindowSettingVMList = default;
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
            this.BackUpIntervalAtMinimizing = this.settings.App_BackUpIntervalMinAtMinimizing;
            this.BackUpFlagAtClosing = this.settings.App_BackUpFlagAtClosing;
            this.StartMonth = this.settings.App_StartMonth;
            this.NationalHolidayCsvURI = this.settings.App_NationalHolidayCsv_Uri;
            this.NationalHolidayCsvDateIndex = this.settings.App_NationalHolidayCsv_DateIndex;
            this.IsPositionSaved = this.settings.App_IsPositionSaved;

            this.DebugMode = this.settings.App_IsDebug;

            this.WithSave = true;

            this.LoadWindowSetting();
        }

        /// <summary>
        /// ウィンドウ設定を読み込む
        /// </summary>
        public void LoadWindowSetting()
        {
            ObservableCollection<WindowSettingViewModel> list = new ObservableCollection<WindowSettingViewModel>() {
                new WindowSettingViewModel(){
                    Kind = "メイン",
                    Left = this.settings.MainWindow_Left, Top = this.settings.MainWindow_Top,
                    Width = this.settings.MainWindow_Width, Height = this.settings.MainWindow_Height },
                new WindowSettingViewModel(){
                    Kind = "移動",
                    Left = this.settings.MoveRegistrationWindow_Left, Top = this.settings.MoveRegistrationWindow_Top,
                    Width = this.settings.MoveRegistrationWindow_Width, Height = this.settings.MoveRegistrationWindow_Height },
                new WindowSettingViewModel(){
                    Kind = "追加・変更",
                    Left = this.settings.ActionRegistrationWindow_Left, Top = this.settings.ActionRegistrationWindow_Top,
                    Width = this.settings.ActionRegistrationWindow_Width, Height = this.settings.ActionRegistrationWindow_Height },
                new WindowSettingViewModel(){
                    Kind = "リスト追加",
                    Left = this.settings.ActionListRegistrationWindow_Left, Top = this.settings.ActionListRegistrationWindow_Top,
                    Width = this.settings.ActionListRegistrationWindow_Width, Height = this.settings.ActionListRegistrationWindow_Height },
                new WindowSettingViewModel(){
                    Kind = "CSV比較",
                    Left = this.settings.CsvComparisonWindow_Left, Top = this.settings.CsvComparisonWindow_Top,
                    Width = this.settings.CsvComparisonWindow_Width, Height = this.settings.CsvComparisonWindow_Height },
                new WindowSettingViewModel(){
                    Kind = "設定",
                    Left = this.settings.SettingsWindow_Left, Top = this.settings.SettingsWindow_Top,
                    Width = this.settings.SettingsWindow_Width, Height = this.settings.SettingsWindow_Height },
                new WindowSettingViewModel(){
                    Kind = "期間選択",
                    Left = this.settings.TermWindow_Left, Top = this.settings.TermWindow_Top,
                    Width = -1, Height = -1 }

            };
            this.WindowSettingVMList = list;
        }
    }
}

using HouseholdAccountBook.UserEventArgs;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static HouseholdAccountBook.Others.DbConstants;
using static HouseholdAccountBook.Others.UiConstants;

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

        #region フィールド
        /// <summary>
        /// 選択された項目VM変更時のイベント
        /// </summary>
        public event Action<EventArgs<HierarchicalViewModel>> SelectedHierarchicalVMChanged;
        /// <summary>
        /// 選択された帳簿VM変更時のイベント
        /// </summary>
        public event Action<EventArgs<BookViewModel>> SelectedBookVMChanged;
        #endregion

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
        #region HierarchicalVMList
        public ObservableCollection<HierarchicalViewModel> HierarchicalVMList
        {
            get => this._HierarchicalVMList;
            set => this.SetProperty(ref this._HierarchicalVMList, value);
        }
        private ObservableCollection<HierarchicalViewModel> _HierarchicalVMList = default;
        #endregion
        /// <summary>
        /// 選択された階層構造設定VM
        /// </summary>
        #region SelectedHierarchicalVM
        public HierarchicalViewModel SelectedHierarchicalVM
        {
            get => this._SelectedHierarchicalVM;
            set {
                if (this.SetProperty(ref this._SelectedHierarchicalVM, value)) {
                    this.SelectedHierarchicalVMChanged?.Invoke(new EventArgs<HierarchicalViewModel>(value));
                }
            }
        }
        private HierarchicalViewModel _SelectedHierarchicalVM = default;
        #endregion
        /// <summary>
        /// 表示された階層構造設定VM
        /// </summary>
        #region DisplayedHierarchicalSettingVM
        public HierarchicalSettingViewModel DisplayedHierarchicalSettingVM
        {
            get => this._DisplayedHierarchicalSettingVM;
            set => this.SetProperty(ref this._DisplayedHierarchicalSettingVM, value);
        }
        private HierarchicalSettingViewModel _DisplayedHierarchicalSettingVM = default;
        #endregion
        #endregion

        #region 帳簿設定
        /// <summary>
        /// 帳簿VMリスト
        /// </summary>
        #region BookVMList
        public ObservableCollection<BookViewModel> BookVMList
        {
            get => this._BookVMList;
            set {
                this._BookVMList.Clear();
                foreach (BookViewModel vm in value) {
                    this._BookVMList.Add(vm);
                }
                this.RaisePropertyChanged(nameof(this.BookVMList));
            }
        }
        private readonly ObservableCollection<BookViewModel> _BookVMList = [];
        #endregion
        /// <summary>
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookViewModel SelectedBookVM
        {
            get => this._SelectedBookVM;
            set {
                if (this.SetProperty(ref this._SelectedBookVM, value)) {
                    this.SelectedBookVMChanged?.Invoke(new EventArgs<BookViewModel>(value));
                }
            }
        }
        private BookViewModel _SelectedBookVM = default;
        #endregion
        /// <summary>
        /// 表示された帳簿設定VM
        /// </summary>
        #region DisplayedBookSettingVM
        public BookSettingViewModel DisplayedBookSettingVM
        {
            get => this._DisplayedBookSettingVM;
            set => this.SetProperty(ref this._DisplayedBookSettingVM, value);
        }
        private BookSettingViewModel _DisplayedBookSettingVM = default;
        #endregion
        #endregion

        #region その他
        #region データベース
        /// <summary>
        /// 選択されたDB種別
        /// </summary>
        #region SelectedDBKind
        public DBKind SelectedDBKind
        {
            get => this._SelectedDBKind;
            set {
                if (this.SetProperty(ref this._SelectedDBKind, value)) {
                    this.RaisePropertyChanged(nameof(this.IsPostgreSQL));
                }
            }
        }
        private DBKind _SelectedDBKind = default;
        #endregion

        /// <summary>
        /// IsPostgreSQL
        /// </summary>
        #region IsPostgreSQL
        public bool IsPostgreSQL => this.SelectedDBKind == DBKind.PostgreSQL;
        #endregion

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
        /// 記帳風月Accessプロバイダ名
        /// </summary>
        /// <remarks>動的に指定するため<see cref="ObservableCollection{T}"/>を用いる</remarks>
        public ObservableCollection<KeyValuePair<string, string>> KichoFugetsuProviderNameDic { get; } = [];
        /// <summary>
        /// 選択された記帳風月Accessプロバイダ名
        /// </summary>
        #region SelectedAccessProviderName
        public string SelectedKichoFugetsuProviderName
        {
            get => this._SelectedAccessProviderName;
            set {
                if (this.SetProperty(ref this._SelectedAccessProviderName, value) && this.WithSave) {
                    this.settings.App_Import_KichoFugetsu_Provider = value;
                    this.settings.Save();
                }
            }
        }
        private string _SelectedAccessProviderName = "Microsoft.ACE.OLEDB.16.0";
        #endregion
        #endregion

        #region 言語
        /// <summary>
        /// 言語種別辞書
        /// </summary>
        public Dictionary<string, string> CultureNameDic { get; } = CultureNameStr;
        /// <summary>
        /// 選択された言語種別
        /// </summary>
        #region SelectedCultureName
        public string SelectedCultureName
        {
            get => this._SelectedCultureName;
            set => this.SetProperty(ref this._SelectedCultureName, value);
        }
        private string _SelectedCultureName = "ja-JP";
        #endregion
        #endregion

        #region バックアップ
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
        /// パスワード入力方法
        /// </summary>
        #region PasswordInput
        public PostgresPasswordInput PasswordInput
        {
            get => this._PasswordInput;
            set {
                if (this.SetProperty(ref this._PasswordInput, value) && this.WithSave) {
                    this.settings.App_Postgres_Password_Input = (int)value;
                    this.settings.Save();
                }
            }
        }
        private PostgresPasswordInput _PasswordInput = PostgresPasswordInput.InputWindow;
        #endregion
        #endregion

        #region カレンダー
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
        /// 国民の祝日CSV 文字エンコーディング
        /// </summary>
        #region NatioalHolidayTextEncodingList
        public ObservableCollection<KeyValuePair<int, string>> NationalHolidayTextEncodingList
        {
            get => this._NationalHolidayTextEncodingList;
            set => this.SetProperty(ref this._NationalHolidayTextEncodingList, value);
        }
        private ObservableCollection<KeyValuePair<int, string>> _NationalHolidayTextEncodingList = default;
        #endregion
        /// <summary>
        /// 国民の祝日CSV 選択された文字エンコーディング
        /// </summary>
        #region SelectedNationalHolidayTextEncoding
        public int SelectedNationalHolidayTextEncoding
        {
            get => this._SelectedNationalHolidayTextEncoding;
            set {
                if (this.SetProperty(ref this._SelectedNationalHolidayTextEncoding, value)) {
                    this.settings.App_NationalHolidayCsv_TextEncoding = value;
                    this.settings.Save();
                }
            }
        }
        private int _SelectedNationalHolidayTextEncoding = default;
        #endregion

        /// <summary>
        /// 国民の祝日CSV 日付インデックス(1開始)
        /// </summary>
        #region NationalHolidayCsvDateIndex
        public int NationalHolidayCsvDateIndex
        {
            get => this._NationalHolidayCsvDateIndex;
            set {
                if (this.SetProperty(ref this._NationalHolidayCsvDateIndex, value) && this.WithSave) {
                    this.settings.App_NationalHolidayCsv_DateIndex = value - 1;
                    this.settings.Save();
                }
            }
        }
        private int _NationalHolidayCsvDateIndex = default;
        #endregion
        #endregion

        #region ウィンドウ
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
                    App.RegisterToResource();
                }
            }
        }
        private bool _DebugMode;
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
    }
}

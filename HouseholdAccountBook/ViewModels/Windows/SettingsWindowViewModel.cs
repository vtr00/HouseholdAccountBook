using HouseholdAccountBook.Others;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using HouseholdAccountBook.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static HouseholdAccountBook.Models.DbConstants;
using static HouseholdAccountBook.Views.UiConstants;

namespace HouseholdAccountBook.ViewModels.Windows
{
    /// <summary>
    /// 設定ウィンドウVM
    /// </summary>
    public class SettingsWindowViewModel : BindableBase
    {
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
            set => this.SetProperty(ref this._SelectedDBKind, value);
        }
        private DBKind _SelectedDBKind = default;
        #endregion

        /// <summary>
        /// PostgreSQL設定
        /// </summary>
        #region PostgreSQLDBSettingVM
        public PostgreSQLDBSettingViewModel PostgreSQLDBSettingVM
        {
            get => this._PostgreSQLDBSettingVM;
            set => this.SetProperty(ref this._PostgreSQLDBSettingVM, value);
        }
        private PostgreSQLDBSettingViewModel _PostgreSQLDBSettingVM = new();
        #endregion

        /// <summary>
        /// Access設定
        /// </summary>
        #region AccessSettingVM
        public OleDbSettingViewModel AccessSettingVM
        {
            get => this._AccessSettingVM;
            set => this.SetProperty(ref this._AccessSettingVM, value);
        }
        private OleDbSettingViewModel _AccessSettingVM = new();
        #endregion

        /// <summary>
        /// SQLite設定
        /// </summary>
        #region SQLiteSettingVM
        public FileDbSettingViewModel SQLiteSettingVM
        {
            get => this._SQLiteSettingVM;
            set => this.SetProperty(ref this._SQLiteSettingVM, value);
        }
        private FileDbSettingViewModel _SQLiteSettingVM = new();
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
            set => this.SetProperty(ref this._BackUpNum, value);
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
            set => this.SetProperty(ref this._BackUpFolderPath, value);
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
            set => this.SetProperty(ref this._BackUpFlagAtMinimizing, value);
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
            set => this.SetProperty(ref this._BackUpIntervalAtMinimizing, value);
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
            set => this.SetProperty(ref this._BackUpFlagAtClosing, value);
        }
        private bool _BackUpFlagAtClosing = default;
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
            set => this.SetProperty(ref this._StartMonth, value);
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
            set => this.SetProperty(ref this._NationalHolidayCsvURI, value);
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
            set => this.SetProperty(ref this._SelectedNationalHolidayTextEncoding, value);
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
            set => this.SetProperty(ref this._NationalHolidayCsvDateIndex, value);
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
            set => this.SetProperty(ref this._IsPositionSaved, value);
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
                if (this.SetProperty(ref this._DebugMode, value)) {
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
        public SettingsWindowViewModel() { }
    }
}

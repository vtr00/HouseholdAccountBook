using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Others.RequestEventArgs;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static HouseholdAccountBook.Extensions.EncodingExtensions;
using static HouseholdAccountBook.Views.UiConstants;

namespace HouseholdAccountBook.ViewModels.WindowsParts
{
    /// <summary>
    /// その他設定タブVM
    /// </summary>
    public class SettingsWindowOtherTabViewModel : WindowPartViewModelBase
    {
        #region イベント
        /// <summary>
        /// 更新必要時イベント
        /// </summary>
        public event EventHandler NeedToUpdateChanged;
        #endregion

        #region Bindingプロパティ
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

        #region デバッグ
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

        /// <summary>
        /// 操作ログ出力
        /// </summary>
        #region OutputOperationLog
        public bool OutputOperationLog
        {
            get => this._OutputOperationLog;
            set => this.SetProperty(ref this._OutputOperationLog, value);
        }
        private bool _OutputOperationLog = default;
        #endregion

        /// <summary>
        /// ウィンドウログ出力
        /// </summary>
        #region OutputWindowLog
        public bool OutputWindowLog
        {
            get => this._OutputWindowLog;
            set => this.SetProperty(ref this._OutputWindowLog, value);
        }
        private bool _OutputWindowLog = default;
        #endregion
        #endregion

        #region コマンド
        /// <summary>
        /// pg_dump.exe選択コマンド
        /// </summary>
        public ICommand SelectDumpExePathCommand => new RelayCommand(this.SelectDumpExePathCommand_Executed);
        /// <summary>
        /// pg_restore.exe選択コマンド
        /// </summary>
        public ICommand SelectRestoreExePathCommand => new RelayCommand(this.SelectRestoreExePathCommand_Executed);
        /// <summary>
        /// データベース設定コマンド
        /// </summary>
        public ICommand RestartForDbSettingCommand => new RelayCommand(this.RestartForDbSettingCommand_Executed);
        /// <summary>
        /// 言語設定適用コマンド
        /// </summary>
        public ICommand RestartForLanguageCommand => new RelayCommand(this.RestartForLanguageCommand_Executed);
        /// <summary>
        /// バックアップフォルダ選択コマンド
        /// </summary>
        public ICommand SelectBackUpFolderPathCommand => new RelayCommand(this.SelectBackUpFolderPathCommand_Executed);
        /// <summary>
        /// ウィンドウ設定再読込コマンド
        /// </summary>
        public ICommand ReloadWindowSettingCommand => new RelayCommand(this.ReloadWindowSettingCommand_Executed);
        /// <summary>
        /// ウィンドウ設定初期化コマンド
        /// </summary>
        public ICommand InitializeWindowSettingCommand => new RelayCommand(this.InitializeWindowSettingCommand_Executed);
        /// <summary>
        /// その他設定保存コマンド
        /// </summary>
        public ICommand SaveOtherSettingsCommand => new RelayCommand(this.SaveOtherSettingsCommand_Executed);
        #endregion
        #endregion

        #region コマンドイベントハンドラ
        /// <summary>
        /// pg_dump.exe選択コマンド処理
        /// </summary>
        private void SelectDumpExePathCommand_Executed()
        {
            (string folderPath, string fileName) = PathExtensions.GetSeparatedPath(this.PostgreSQLDBSettingVM.DumpExePath, App.GetCurrentDir());
            if (string.IsNullOrWhiteSpace(folderPath)) {
                folderPath = App.GetCurrentDir();
            }

            var e = new OpenFileDialogRequestEventArgs() {
                CheckFileExists = true,
                InitialDirectory = folderPath,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = "pg_dump.exe|pg_dump.exe"
            };
            if (this.OpenFileDialogRequest(e)) {
                this.PostgreSQLDBSettingVM.DumpExePath = PathExtensions.GetSmartPath(App.GetCurrentDir(), e.FileName);
            }
        }

        /// <summary>
        /// pg_restore.exe選択コマンド処理
        /// </summary>
        private void SelectRestoreExePathCommand_Executed()
        {
            (string folderPath, string fileName) = PathExtensions.GetSeparatedPath(this.PostgreSQLDBSettingVM.RestoreExePath, App.GetCurrentDir());
            if (string.IsNullOrWhiteSpace(folderPath)) {
                folderPath = App.GetCurrentDir();
            }

            var e = new OpenFileDialogRequestEventArgs() {
                CheckFileExists = true,
                InitialDirectory = folderPath,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = "pg_restore.exe|pg_restore.exe"
            };
            if (this.OpenFileDialogRequest(e)) {
                this.PostgreSQLDBSettingVM.RestoreExePath = PathExtensions.GetSmartPath(App.GetCurrentDir(), e.FileName);
            }
        }

        /// <summary>
        /// データベース設定コマンド処理
        /// </summary>
        private void RestartForDbSettingCommand_Executed()
        {
            if (MessageBox.Show(Properties.Resources.Message_RestartNotification, Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                Properties.Settings.Default.App_InitFlag = true;
                Properties.Settings.Default.Save();

                ((App)Application.Current).Restart();
            }
        }

        /// <summary>
        /// 言語設定適用コマンド処理
        /// </summary>
        private void RestartForLanguageCommand_Executed()
        {
            if (MessageBox.Show(Properties.Resources.Message_RestartNotification, Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                Properties.Settings.Default.App_CultureName = this.SelectedCultureName;
                Properties.Settings.Default.Save();

                ((App)Application.Current).Restart();
            }
        }

        /// <summary>
        /// バックアップフォルダ選択コマンド処理
        /// </summary>
        private void SelectBackUpFolderPathCommand_Executed()
        {
            string folderFullPath;
            if (string.IsNullOrWhiteSpace(this.BackUpFolderPath)) {
                folderFullPath = App.GetCurrentDir();
            }
            else {
                (string folderPath, string fileName) = PathExtensions.GetSeparatedPath(this.BackUpFolderPath, App.GetCurrentDir());
                folderFullPath = Path.Combine(folderPath, fileName);
            }

            var e = new OpenFolderDialogRequestEventArgs() {
                InitialDirectory = folderFullPath,
                Title = Properties.Resources.Title_BackupFolderSelection
            };
            if (this.OpenFolderDialogRequest(e)) {
                this.BackUpFolderPath = PathExtensions.GetSmartPath(App.GetCurrentDir(), e.FolderName);
            }
        }

        /// <summary>
        /// ウィンドウ設定再読込コマンド処理
        /// </summary>
        private void ReloadWindowSettingCommand_Executed()
        {
            this.WindowSettingVMList = LoadWindowSettings();
        }

        /// <summary>
        /// ウィンドウ設定初期化コマンド処理
        /// </summary>
        private void InitializeWindowSettingCommand_Executed()
        {
            if (MessageBox.Show(Properties.Resources.Message_RestartNotification, Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                Properties.Settings settings = Properties.Settings.Default;

                // メイン
                settings.MainWindow_Left = -1;
                settings.MainWindow_Top = -1;
                settings.MainWindow_Width = -1;
                settings.MainWindow_Height = -1;

                // 移動
                settings.MoveRegistrationWindow_Left = -1;
                settings.MoveRegistrationWindow_Top = -1;
                settings.MoveRegistrationWindow_Width = -1;
                settings.MoveRegistrationWindow_Height = -1;

                // 追加・変更
                settings.ActionRegistrationWindow_Left = -1;
                settings.ActionRegistrationWindow_Top = -1;
                settings.ActionRegistrationWindow_Width = -1;
                settings.ActionRegistrationWindow_Height = -1;

                // リスト追加
                settings.ActionListRegistrationWindow_Left = -1;
                settings.ActionListRegistrationWindow_Top = -1;
                settings.ActionListRegistrationWindow_Width = -1;
                settings.ActionListRegistrationWindow_Height = -1;

                // CSV比較
                settings.CsvComparisonWindow_Left = -1;
                settings.CsvComparisonWindow_Top = -1;
                settings.CsvComparisonWindow_Width = -1;
                settings.CsvComparisonWindow_Height = -1;

                // 設定
                settings.SettingsWindow_Left = -1;
                settings.SettingsWindow_Top = -1;
                settings.SettingsWindow_Width = -1;
                settings.SettingsWindow_Height = -1;

                // 期間選択
                settings.TermWindow_Left = -1;
                settings.TermWindow_Top = -1;
                // settings.TermWindow_Width = -1;
                // settings.TermWindow_Height = -1;

                // バージョン
                settings.VersionWindow_Left = -1;
                settings.VersionWindow_Top = -1;
                // settings.VersionWindow_Width = -1;
                // settings.VersionWindow_Height = -1;

                settings.App_InitSizeFlag = true;
                settings.Save();

                ((App)Application.Current).Restart();
            }
        }

        /// <summary>
        /// その他設定保存コマンド処理
        /// </summary>
        private void SaveOtherSettingsCommand_Executed()
        {
            this.Save();
            this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        public override Task LoadAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// その他設定を読み込む
        /// </summary>
        public void Load()
        {
            using FuncLog funcLog = new();

            Properties.Settings settings = Properties.Settings.Default;

            this.SelectedDBKind = (DBKind)settings.App_SelectedDBKind;

            // PostgreSQL
            this.PostgreSQLDBSettingVM.Load(null);

            // SQLite
            this.SQLiteSettingVM.Load();

            // Access(記帳風月)
            this.AccessSettingVM.LoadForKichoFugetsu();

            this.SelectedCultureName = settings.App_CultureName;

            this.BackUpNum = settings.App_BackUpNum;
            this.BackUpFolderPath = PathExtensions.GetSmartPath(App.GetCurrentDir(), settings.App_BackUpFolderPath);
            this.BackUpFlagAtMinimizing = settings.App_BackUpFlagAtMinimizing;
            this.BackUpIntervalAtMinimizing = settings.App_BackUpIntervalMinAtMinimizing;
            this.BackUpFlagAtClosing = settings.App_BackUpFlagAtClosing;

            this.StartMonth = settings.App_StartMonth;
            this.NationalHolidayCsvURI = settings.App_NationalHolidayCsv_Uri;
            this.NationalHolidayTextEncodingList = GetTextEncodingList();
            this.SelectedNationalHolidayTextEncoding = settings.App_NationalHolidayCsv_TextEncoding;
            this.NationalHolidayCsvDateIndex = settings.App_NationalHolidayCsv_DateIndex + 1;

            this.IsPositionSaved = settings.App_IsPositionSaved;

            this.DebugMode = settings.App_IsDebug;
            this.OutputOperationLog = settings.App_OutputFlag_OperationLog;
            this.OutputWindowLog = settings.App_OutputFlag_WindowLog;

            this.WindowSettingVMList = LoadWindowSettings();
        }

        public override void AddEventHandlers()
        {
            // NOP
        }

        /// <summary>
        /// 各ウィンドウの矩形領域設定を読み込む
        /// </summary>
        private static ObservableCollection<WindowSettingViewModel> LoadWindowSettings()
        {
            using FuncLog funcLog = new();

            Properties.Settings settings = Properties.Settings.Default;

            ObservableCollection<WindowSettingViewModel> list = [
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_DbSettingWindow,
                    Left = settings.DbSettingWindow_Left, Top = settings.DbSettingWindow_Top,
                    Width = settings.DbSettingWindow_Width, Height = settings.DbSettingWindow_Height
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_MainWindow,
                    Left = settings.MainWindow_Left, Top = settings.MainWindow_Top,
                    Width = settings.MainWindow_Width, Height = settings.MainWindow_Height
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_AddMoveWindow,
                    Left = settings.MoveRegistrationWindow_Left, Top = settings.MoveRegistrationWindow_Top,
                    Width = settings.MoveRegistrationWindow_Width, Height = settings.MoveRegistrationWindow_Height
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_AddWindow,
                    Left = settings.ActionRegistrationWindow_Left, Top = settings.ActionRegistrationWindow_Top,
                    Width = settings.ActionRegistrationWindow_Width, Height = settings.ActionRegistrationWindow_Height
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_AddListWindow,
                    Left = settings.ActionListRegistrationWindow_Left, Top = settings.ActionListRegistrationWindow_Top,
                    Width = settings.ActionListRegistrationWindow_Width, Height = settings.ActionListRegistrationWindow_Height
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_CsvComparisonWindow,
                    Left = settings.CsvComparisonWindow_Left, Top = settings.CsvComparisonWindow_Top,
                    Width = settings.CsvComparisonWindow_Width, Height = settings.CsvComparisonWindow_Height
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_SettingsWindow,
                    Left = settings.SettingsWindow_Left, Top = settings.SettingsWindow_Top,
                    Width = settings.SettingsWindow_Width, Height = settings.SettingsWindow_Height
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_TermSelectionWindow,
                    Left = settings.TermWindow_Left, Top = settings.TermWindow_Top,
                    Width = -1, Height = -1
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_VersionWindow,
                    Left = settings.VersionWindow_Left, Top = settings.VersionWindow_Top,
                    Width = -1, Height = -1
                }
            ];
            return list;
        }

        /// <summary>
        /// その他設定を保存する
        /// </summary>
        private void Save()
        {
            using FuncLog funcLog = new();

            Properties.Settings settings = Properties.Settings.Default;

            // PostgreSQL
            _ = this.PostgreSQLDBSettingVM.Save(null);

            // Access(記帳風月)
            _ = this.AccessSettingVM.SaveForKichoFugetsu();

            settings.App_CultureName = this.SelectedCultureName;

            settings.App_BackUpNum = this.BackUpNum;
            settings.App_BackUpFolderPath = Path.GetFullPath(this.BackUpFolderPath, App.GetCurrentDir());
            settings.App_BackUpFlagAtMinimizing = this.BackUpFlagAtMinimizing;
            settings.App_BackUpIntervalMinAtMinimizing = this.BackUpIntervalAtMinimizing;
            settings.App_BackUpFlagAtClosing = this.BackUpFlagAtClosing;

            settings.App_StartMonth = this.StartMonth;
            settings.App_NationalHolidayCsv_Uri = this.NationalHolidayCsvURI;
            settings.App_NationalHolidayCsv_TextEncoding = this.SelectedNationalHolidayTextEncoding;
            settings.App_NationalHolidayCsv_DateIndex = this.NationalHolidayCsvDateIndex - 1;

            settings.App_IsPositionSaved = this.IsPositionSaved;

            settings.App_IsDebug = this.DebugMode;
            settings.App_OutputFlag_WindowLog = this.OutputWindowLog;
            settings.App_OutputFlag_OperationLog = this.OutputOperationLog;

            settings.Save();
        }
    }
}

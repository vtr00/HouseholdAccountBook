using HouseholdAccountBook.Infrastructure;
using HouseholdAccountBook.Infrastructure.CSV;
using HouseholdAccountBook.Infrastructure.DB;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Models.DbHandlers;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.Views;
using System;
using System.Linq;
using System.Text;
using System.Windows;

namespace HouseholdAccountBook.Models.AppServices
{
    /// <summary>
    /// ユーザ設定サービス
    /// </summary>
    public class UserSettingService : SingletonBase<UserSettingService>
    {
        /// <summary>
        /// スタティックコンストラクタ
        /// </summary>
        static UserSettingService() => Register(static () => new UserSettingService());
        /// <summary>
        /// プライベートコンストラクタ
        /// </summary>
        private UserSettingService() { }

        /// <summary>
        /// 設定
        /// </summary>
        private readonly Properties.Settings mSettings = Properties.Settings.Default;

        #region アプリ全体
        /// <summary>
        /// 初期化フラグ
        /// </summary>
        public bool InitFlag {
            get => this.mSettings.App_InitFlag;
            set {
                this.mSettings.App_InitFlag = value;
                this.mSettings.Save();
            }
        }
        /// <summary>
        /// 選択カルチャ名
        /// </summary>
        public string SelectedCaltureName {
            get => this.mSettings.App_CultureName;
            set {
                this.mSettings.App_CultureName = value;
                this.mSettings.Save();
            }
        }
        /// <summary>
        /// 最後に起動したアプリのバージョン
        /// </summary>
        public Version LastAppVersion {
            get => Version.TryParse(this.mSettings.App_Version, out Version lastVersion) ? lastVersion : new Version(0, 0, 0, 0);
            set {
                this.mSettings.App_Version = value.ToString();
                this.mSettings.Save();
            }
        }
        /// <summary>
        /// 選択DB種別
        /// </summary>
        public DBKind SelectedDBKind {
            get => EnumUtil.SafeParseEnum(this.mSettings.App_SelectedDBKind, DBKind.SQLite);
            set {
                this.mSettings.App_SelectedDBKind = (int)value;
                this.mSettings.Save();
            }
        }
        /// <summary>
        /// 起動時に最新バージョンを確認するか
        /// </summary>
        public bool CheckLatestVersionAtAppLaunched {
            get => this.mSettings.App_LatestVersionNotifyAtAppLaunched;
            set {
                this.mSettings.App_LatestVersionNotifyAtAppLaunched = value;
                this.mSettings.Save();
            }
        }
        #endregion

        #region 初期表示
        /// <summary>
        /// 選択タブ
        /// </summary>
        public Tabs SelectedTab {
            get => EnumUtil.SafeParseEnum(this.mSettings.MainWindow_SelectedTabIndex, Tabs.BooksTab);
            set {
                this.mSettings.MainWindow_SelectedTabIndex = (int)value;
                this.mSettings.Save();
            }
        }
        /// <summary>
        /// 選択帳簿ID
        /// </summary>
        public BookIdObj SelectedBookId {
            get => this.mSettings.MainWindow_SelectedBookId == (int)BookIdObj.System ? BookIdObj.System : this.mSettings.MainWindow_SelectedBookId;
            set {
                this.mSettings.MainWindow_SelectedBookId = (int)value;
                this.mSettings.Save();
            }
        }
        /// <summary>
        /// 選択グラフ種別1
        /// </summary>
        public GraphKind1 SelectedGraphKind1 {
            get => EnumUtil.SafeParseEnum(this.mSettings.MainWindow_SelectedGraphKindIndex, GraphKind1.IncomeAndExpensesGraph);
            set {
                this.mSettings.MainWindow_SelectedGraphKindIndex = (int)value;
                this.mSettings.Save();
            }
        }
        /// <summary>
        /// 選択グラフ種別2
        /// </summary>
        public GraphKind2 SelectedGraphKind2 {
            get => EnumUtil.SafeParseEnum(this.mSettings.MainWindow_SelectedGraphKind2Index, GraphKind2.CategoryGraph);
            set {
                this.mSettings.MainWindow_SelectedGraphKind2Index = (int)value;
                this.mSettings.Save();
            }
        }
        #endregion

        #region 選択ダイアログ設定
        /// <summary>
        /// CSV比較ファイルパス
        /// </summary>
        public string CsvCompFile {
            get => this.mSettings.App_CsvFilePath;
            set {
                this.mSettings.App_CsvFilePath = value;
                this.mSettings.Save();
            }
        }
        /// <summary>
        /// 記帳風月ファイルパス
        /// </summary>
        public string KichoFugetsuFilePath {
            get => this.mSettings.App_Import_KichoFugetsu_FilePath;
            set {
                this.mSettings.App_Import_KichoFugetsu_FilePath = value;
                this.mSettings.Save();
            }
        }
        /// <summary>
        /// インポート時のカスタムフォーマットファイルパス
        /// </summary>
        public string ImportCustomFilePath {
            get => this.mSettings.App_Import_CustomFormat_FilePath;
            set {
                this.mSettings.App_Import_CustomFormat_FilePath = value;
                this.mSettings.Save();
            }
        }
        /// <summary>
        /// インポート時のSQLiteファイルパス
        /// </summary>
        public string ImportSQLiteFilePath {
            get => this.mSettings.App_Import_SQLite_FilePath;
            set {
                this.mSettings.App_Import_SQLite_FilePath = value;
                this.mSettings.Save();
            }
        }
        /// <summary>
        /// エクスポート時のカスタムフォーマットファイルパス
        /// </summary>
        public string ExportCustomFilePath {
            get => this.mSettings.App_Export_CustomFormat_FilePath;
            set {
                this.mSettings.App_Export_CustomFormat_FilePath = value;
                this.mSettings.Save();
            }
        }
        /// <summary>
        /// エクスポート時のSQLファイルパス
        /// </summary>
        public string ExportSQLFilePath {
            get => this.mSettings.App_Export_SQLFilePath;
            set {
                this.mSettings.App_Export_SQLFilePath = value;
                this.mSettings.Save();
            }
        }
        /// <summary>
        /// エクスポート時のCSVファイルパス
        /// </summary>
        public string ExportCsvFilePath {
            get => this.mSettings.App_ExportCsvFilePath;
            set {
                this.mSettings.App_ExportCsvFilePath = value;
                this.mSettings.Save();
            }
        }
        #endregion

        #region カレンダー設定
        /// <summary>
        /// 開始月
        /// </summary>
        public int FiscalStartMonth {
            get => this.mSettings.App_StartMonth;
            set {
                this.mSettings.App_StartMonth = value;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// 祝日CSV設定
        /// </summary>
        public CSVFileDao.HolidayCSVConfigulation HolidayCSVConfig {
            get => new() {
                Url = this.mSettings.App_NationalHolidayCsv_Uri,
                TextEncoding = Encoding.GetEncoding(this.mSettings.App_NationalHolidayCsv_TextEncoding),
                DateIndex = this.mSettings.App_NationalHolidayCsv_DateIndex
            };
            set {
                this.mSettings.App_NationalHolidayCsv_Uri = value.Url;
                this.mSettings.App_NationalHolidayCsv_TextEncoding = value.TextEncoding.CodePage;
                this.mSettings.App_NationalHolidayCsv_DateIndex = value.DateIndex;
                this.mSettings.Save();
            }
        }
        #endregion

        #region ログ設定
        /// <summary>
        /// 操作ログ設定
        /// </summary>
        public LogImpl.Configuration LogConfig {
            get => new() {
                OutputLogToFile = this.mSettings.App_OutputFlag_OperationLog,
                LogFileAmount = this.mSettings.App_OperationLogNum,
                OutputLogLevel = (Log.LogLevel)this.mSettings.App_OperationLogLevel
            };
            set {
                this.mSettings.App_OutputFlag_OperationLog = value.OutputLogToFile;
                this.mSettings.App_OperationLogNum = value.LogFileAmount;
                this.mSettings.App_OperationLogLevel = (int)value.OutputLogLevel;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// 例外ログ設定
        /// </summary>
        public ExceptionLog.Configuration ExceptionLogConfig {
            get => new() {
                LogFileAmount = this.mSettings.App_UnhandledExceptionLogNum
            };
            set {
                this.mSettings.App_UnhandledExceptionLogNum = value.LogFileAmount;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// ウィンドウログ設定
        /// </summary>
        public WindowLog.Configulation WindowLogConfig {
            get => new() {
                OutputLog = this.mSettings.App_OutputFlag_WindowLog,
                LogFileAmount = this.mSettings.App_WindowLogNum
            };
            set {
                this.mSettings.App_OutputFlag_WindowLog = value.OutputLog;
                this.mSettings.App_WindowLogNum = value.LogFileAmount;
                this.mSettings.Save();
            }
        }
        #endregion

        #region ウィンドウ設定
        /// <summary>
        /// ウィンドウ位置管理設定
        /// </summary>
        public WindowLocationManager.Configuration WindowLocationConfig {
            get => new() {
                IsPositionSaved = this.mSettings.App_IsPositionSaved
            };
            set {
                this.mSettings.App_IsPositionSaved = value.IsPositionSaved;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// メインウィンドウ位置
        /// </summary>
        public Point MainWindowPoint {
            get => new(this.mSettings.MainWindow_Left, this.mSettings.MainWindow_Top);
            set {
                this.mSettings.MainWindow_Left = value.X;
                this.mSettings.MainWindow_Top = value.Y;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// メインウィンドウサイズ
        /// </summary>
        public (double width, double height) MainWindowSize {
            get => (this.mSettings.MainWindow_Width, this.mSettings.MainWindow_Height);
            set {
                this.mSettings.MainWindow_Width = value.width;
                this.mSettings.MainWindow_Height = value.height;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// メインウィンドウ状態
        /// </summary>
        public int MainWindowState {
            get => this.mSettings.MainWindow_WindowState;
            set {
                this.mSettings.MainWindow_WindowState = value;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// 移動登録ウィンドウ位置
        /// </summary>
        public Point MoveRegistrationWindowPoint {
            get => new(this.mSettings.MoveRegistrationWindow_Left, this.mSettings.MoveRegistrationWindow_Top);
            set {
                this.mSettings.MoveRegistrationWindow_Left = value.X;
                this.mSettings.MoveRegistrationWindow_Top = value.Y;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// 移動登録ウィンドウサイズ
        /// </summary>
        public (double width, double height) MoveRegistrationWindowSize {
            get => (this.mSettings.MoveRegistrationWindow_Width, this.mSettings.MoveRegistrationWindow_Height);
            set {
                this.mSettings.MoveRegistrationWindow_Width = value.width;
                this.mSettings.MoveRegistrationWindow_Height = value.height;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// 帳簿項目登録ウィンドウ位置
        /// </summary>
        public Point ActionRegistrationWindowPoint {
            get => new(this.mSettings.ActionRegistrationWindow_Left, this.mSettings.ActionRegistrationWindow_Top);
            set {
                this.mSettings.ActionRegistrationWindow_Left = value.X;
                this.mSettings.ActionRegistrationWindow_Top = value.Y;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// 帳簿項目登録ウィンドウサイズ
        /// </summary>
        public (double width, double height) ActionRegistrationWindowSize {
            get => (this.mSettings.ActionRegistrationWindow_Width, this.mSettings.ActionRegistrationWindow_Height);
            set {
                this.mSettings.ActionRegistrationWindow_Width = value.width;
                this.mSettings.ActionRegistrationWindow_Height = value.height;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// 帳簿項目リスト登録ウィンドウ位置
        /// </summary>
        public Point ActionListRegistrationWindowPoint {
            get => new(this.mSettings.ActionListRegistrationWindow_Left, this.mSettings.ActionListRegistrationWindow_Top);
            set {
                this.mSettings.ActionListRegistrationWindow_Left = value.X;
                this.mSettings.ActionListRegistrationWindow_Top = value.Y;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// 帳簿項目リスト登録ウィンドウサイズ
        /// </summary>
        public (double width, double height) ActionListRegistrationWindowSize {
            get => (this.mSettings.ActionListRegistrationWindow_Width, this.mSettings.ActionListRegistrationWindow_Height);
            set {
                this.mSettings.ActionListRegistrationWindow_Width = value.width;
                this.mSettings.ActionListRegistrationWindow_Height = value.height;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// 設定ウィンドウ位置
        /// </summary>
        public Point SettingsWindowPoint {
            get => new(this.mSettings.SettingsWindow_Left, this.mSettings.SettingsWindow_Top);
            set {
                this.mSettings.SettingsWindow_Left = value.X;
                this.mSettings.SettingsWindow_Top = value.Y;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// 設定ウィンドウサイズ
        /// </summary>
        public (double width, double height) SettingsWindowSize {
            get => (this.mSettings.SettingsWindow_Width, this.mSettings.SettingsWindow_Height);
            set {
                this.mSettings.SettingsWindow_Width = value.width;
                this.mSettings.SettingsWindow_Height = value.height;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// CSV比較ウィンドウ位置
        /// </summary>
        public Point CsvComparisonWindowPoint {
            get => new(this.mSettings.CsvComparisonWindow_Left, this.mSettings.CsvComparisonWindow_Top);
            set {
                this.mSettings.CsvComparisonWindow_Left = value.X;
                this.mSettings.CsvComparisonWindow_Top = value.Y;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// CSV比較ウィンドウサイズ
        /// </summary>
        public (double width, double height) CsvComparisonWindowSize {
            get => (this.mSettings.CsvComparisonWindow_Width, this.mSettings.CsvComparisonWindow_Height);
            set {
                this.mSettings.CsvComparisonWindow_Width = value.width;
                this.mSettings.CsvComparisonWindow_Height = value.height;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// 期間ウィンドウ位置
        /// </summary>
        public Point TermWindowPoint {
            get => new(this.mSettings.TermWindow_Left, this.mSettings.TermWindow_Top);
            set {
                this.mSettings.TermWindow_Left = value.X;
                this.mSettings.TermWindow_Top = value.Y;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// DB設定ウィンドウ位置
        /// </summary>
        public Point DbSettingWindowPoint {
            get => new(this.mSettings.DbSettingWindow_Left, this.mSettings.DbSettingWindow_Top);
            set {
                this.mSettings.DbSettingWindow_Left = value.X;
                this.mSettings.DbSettingWindow_Top = value.Y;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// DB設定ウィンドウサイズ
        /// </summary>
        public (double width, double height) DbSettingWindowSize {
            get => (this.mSettings.DbSettingWindow_Width, this.mSettings.DbSettingWindow_Height);
            set {
                this.mSettings.DbSettingWindow_Width = value.width;
                this.mSettings.DbSettingWindow_Height = value.height;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// バージョンウィンドウ位置
        /// </summary>
        public Point VersionWindowPoint {
            get => new(this.mSettings.VersionWindow_Left, this.mSettings.VersionWindow_Top);
            set {
                this.mSettings.VersionWindow_Left = value.X;
                this.mSettings.VersionWindow_Top = value.Y;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// バージョンウィンドウサイズ
        /// </summary>
        public (double width, double height) VersionWindowSize {
            get => (this.mSettings.VersionWindow_Width, this.mSettings.VersionWindow_Height);
            set {
                this.mSettings.VersionWindow_Width = value.width;
                this.mSettings.VersionWindow_Height = value.height;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// ウィンドウ位置を初期化する
        /// </summary>
        public void InitializeWindowLocation()
        {
            // DB設定
            this.mSettings.DbSettingWindow_Left = -1;
            this.mSettings.DbSettingWindow_Top = -1;
            this.mSettings.DbSettingWindow_Width = -1;
            this.mSettings.DbSettingWindow_Height = -1;

            // メイン
            this.mSettings.MainWindow_Left = -1;
            this.mSettings.MainWindow_Top = -1;
            this.mSettings.MainWindow_Width = -1;
            this.mSettings.MainWindow_Height = -1;

            // 移動
            this.mSettings.MoveRegistrationWindow_Left = -1;
            this.mSettings.MoveRegistrationWindow_Top = -1;
            this.mSettings.MoveRegistrationWindow_Width = -1;
            this.mSettings.MoveRegistrationWindow_Height = -1;

            // 追加・変更
            this.mSettings.ActionRegistrationWindow_Left = -1;
            this.mSettings.ActionRegistrationWindow_Top = -1;
            this.mSettings.ActionRegistrationWindow_Width = -1;
            this.mSettings.ActionRegistrationWindow_Height = -1;

            // リスト追加
            this.mSettings.ActionListRegistrationWindow_Left = -1;
            this.mSettings.ActionListRegistrationWindow_Top = -1;
            this.mSettings.ActionListRegistrationWindow_Width = -1;
            this.mSettings.ActionListRegistrationWindow_Height = -1;

            // CSV比較
            this.mSettings.CsvComparisonWindow_Left = -1;
            this.mSettings.CsvComparisonWindow_Top = -1;
            this.mSettings.CsvComparisonWindow_Width = -1;
            this.mSettings.CsvComparisonWindow_Height = -1;

            // 設定
            this.mSettings.SettingsWindow_Left = -1;
            this.mSettings.SettingsWindow_Top = -1;
            this.mSettings.SettingsWindow_Width = -1;
            this.mSettings.SettingsWindow_Height = -1;

            // 期間選択
            this.mSettings.TermWindow_Left = -1;
            this.mSettings.TermWindow_Top = -1;
            // this.mSettings.TermWindow_Width = -1;
            // this.mSettings.TermWindow_Height = -1;

            // バージョン
            this.mSettings.VersionWindow_Left = -1;
            this.mSettings.VersionWindow_Top = -1;
            this.mSettings.VersionWindow_Width = -1;
            this.mSettings.VersionWindow_Height = -1;

            this.mSettings.Save();
        }
        #endregion

        #region DB接続設定
        /// <summary>
        /// PostgreSQL接続情報
        /// </summary>
        public NpgsqlDbHandler.ConnectInfo NpgsqlConnectInfo {
            get {
                NpgsqlDbHandler.ConnectInfo connectInfo = new() {
                    Host = this.mSettings.App_Postgres_Host,
                    Port = this.mSettings.App_Postgres_Port,
                    UserName = this.mSettings.App_Postgres_UserName,
                    Password = this.mSettings.App_Postgres_Password,
#if DEBUG
                    DatabaseName = this.mSettings.App_Postgres_DatabaseName_Debug,
#else
                    DatabaseName = this.mSettings.App_Postgres_DatabaseName,
#endif
                    Role = this.mSettings.App_Postgres_Role
                };
                if (this.mSettings.App_Postgres_Password == string.Empty) {
                    // 暗号化済パスワードを取得する
                    connectInfo.EncryptedPassword = this.mSettings.App_Postgres_EncryptedPassword;
                }
                else {
                    // 平文が保存されている場合は暗号化して保存し直す
                    this.mSettings.App_Postgres_EncryptedPassword = connectInfo.EncryptedPassword;
                    this.mSettings.App_Postgres_Password = string.Empty; // パスワードは保存しない
                    this.mSettings.Save();
                }
                return connectInfo;
            }
            set {
                this.mSettings.App_Postgres_Host = value.Host;
                this.mSettings.App_Postgres_Port = value.Port;
                this.mSettings.App_Postgres_UserName = value.UserName;
                this.mSettings.App_Postgres_Password = string.Empty; // 暗号化して保存するため空にする
                this.mSettings.App_Postgres_EncryptedPassword = value.EncryptedPassword;
                this.mSettings.App_Postgres_DatabaseName = value.DatabaseName;
                this.mSettings.App_Postgres_Role = value.Role;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// SQLite接続情報
        /// </summary>
        public SQLiteDbHandler.ConnectInfo SQLiteConnectInfo {
            get => new() {
                FilePath = this.mSettings.App_SQLite_DBFilePath
            };
            set {
                this.mSettings.App_SQLite_DBFilePath = value.FilePath;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// Access接続情報
        /// </summary>
        public OleDbHandler.ConnectInfo AccessConnectInfo {
            get => new(this.mSettings.App_Access_Provider) {
                DataSource = this.mSettings.App_Access_DBFilePath
            };
            set {
                this.mSettings.App_Access_Provider = value.Provider;
                this.mSettings.App_Access_DBFilePath = value.DataSource;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// 記帳風月接続情報
        /// </summary>
        /// <remarks><see cref="OleDbHandler.ConnectInfo.DataSource"/> はインポート時に指定する</remarks>
        public OleDbHandler.ConnectInfo KichoFugetsuConnectInfo {
            get => new(this.mSettings.App_Import_KichoFugetsu_Provider);
            set {
                this.mSettings.App_Import_KichoFugetsu_Provider = value.Provider;
                this.mSettings.Save();
            }
        }
        #endregion

        #region DBバックアップ設定
        /// <summary>
        /// 手動バックアップの日時
        /// </summary>
        public DateTime CurrentBackUpBySelf {
            get => this.mSettings.App_BackUpCurrentBySelf;
            set {
                this.mSettings.App_BackUpCurrentBySelf = value;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// 最小化時のバックアップの日時
        /// </summary>
        public DateTime CurrentBackUpAtMinimizing {
            get => this.mSettings.App_BackUpCurrentAtMinimizing;
            set {
                this.mSettings.App_BackUpCurrentAtMinimizing = value;
                this.mSettings.Save();
            }
        }
        /// <summary>
        /// 終了時のバックアップの日時
        /// </summary>
        public DateTime CurrentBackUpAtClosing {
            get => this.mSettings.App_BackUpCurrentAtClosing;
            set {
                this.mSettings.App_BackUpCurrentAtClosing = value;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// 最新のバックアップの日時
        /// </summary>
        public DateTime? CurrentBackUp {
            get {
                DateTime backUpCurrent = new[] { this.mSettings.App_BackUpCurrentAtMinimizing, this.mSettings.App_BackUpCurrentAtClosing, this.mSettings.App_BackUpCurrentBySelf }.Max();
                return backUpCurrent == DateTime.MinValue ? null : backUpCurrent;
            }
        }

        /// <summary>
        /// DBバックアップ設定
        /// </summary>
        public DbBackUpManager.Configuration DbBackupConfig {
            get => new() {
                Amount = this.mSettings.App_BackUpNum,
                TargetFolderPath = this.mSettings.App_BackUpFolderPath,
                ExecuteAtMinimizing = this.mSettings.App_BackUpFlagAtMinimizing,
                IntervalMinAtMinimizing = this.mSettings.App_BackUpIntervalMinAtMinimizing,
                NotifyAtMinimizing = this.mSettings.App_BackUpNotifyAtMinimizing,
                ExecuteAtClosing = this.mSettings.App_BackUpFlagAtClosing,
                Condition = (BackUpCondition)this.mSettings.App_BackUpCondition
            };
            set {
                this.mSettings.App_BackUpNum = value.Amount;
                this.mSettings.App_BackUpFolderPath = value.TargetFolderPath;
                this.mSettings.App_BackUpFlagAtMinimizing = value.ExecuteAtMinimizing;
                this.mSettings.App_BackUpIntervalMinAtMinimizing = value.IntervalMinAtMinimizing;
                this.mSettings.App_BackUpNotifyAtMinimizing = value.NotifyAtMinimizing;
                this.mSettings.App_BackUpFlagAtClosing = value.ExecuteAtClosing;
                this.mSettings.App_BackUpCondition = (int)value.Condition;
                this.mSettings.Save();
            }
        }

        /// <summary>
        /// PostgreSQLバックアップ設定
        /// </summary>
        public NpgsqlDbHandler.BackupConfiguration PostgreSQLBackupConfig {
            get => new() {
                DumpExePath = this.mSettings.App_Postgres_DumpExePath,
                RestoreExePath = this.mSettings.App_Postgres_RestoreExePath,
                PasswordInput = (PostgresPasswordInput)this.mSettings.App_Postgres_Password_Input
            };
            set {
                this.mSettings.App_Postgres_DumpExePath = value.DumpExePath;
                this.mSettings.App_Postgres_RestoreExePath = value.RestoreExePath;
                this.mSettings.App_Postgres_Password_Input = (int)value.PasswordInput;
                this.mSettings.Save();
            }
        }
        #endregion
    }
}

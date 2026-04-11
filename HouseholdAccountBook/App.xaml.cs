using HouseholdAccountBook.Infrastructure.DB;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.DbHandlers;
using HouseholdAccountBook.Views;
using HouseholdAccountBook.Views.Extensions;
using HouseholdAccountBook.Views.Windows;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MyResources = HouseholdAccountBook.Properties.Resources;

namespace HouseholdAccountBook
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        #region プロパティ
        /// <summary>
        /// 起動時刻
        /// </summary>
#pragma warning disable IDE0032 // 自動プロパティを使用する
        public DateTime StartupTime => this.mStartupTime;
        private readonly DateTime mStartupTime = DateTime.Now;
#pragma warning restore IDE0032 // 自動プロパティを使用する
        #endregion

        #region フィールド
#if !DEBUG
        /// <summary>
        /// 多重起動抑止用Mutex
        /// </summary>
        private static Mutex mMutex;
#endif
        #endregion

        /// <summary>
        /// <see cref="App"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        public App() { }

        /// <summary>
        /// アプリケーション開始時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void App_Startup(object sender, StartupEventArgs e)
        {
            // ログ関連の設定
            LogImpl.Instance.Config = UserSettingService.Instance.LogConfig;
            using FuncLog funcLog = new();
            ExceptionLog.Config = UserSettingService.Instance.ExceptionLogConfig;
            WindowLog.Config = UserSettingService.Instance.WindowLogConfig;
            WindowLocationManager.Config = UserSettingService.Instance.WindowLocationConfig;

            // shift-jis を使用するために必要
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Log.Debug($"App_InitFlag: {UserSettingService.Instance.InitFlag}");

            Log.Info($"Current Culture: {CultureInfo.CurrentCulture.Name}");
            Log.Info($"Application Culture: {UserSettingService.Instance.SelectedCaltureName}");
            // 言語の初期設定がない場合
            if (string.IsNullOrEmpty(UserSettingService.Instance.SelectedCaltureName)) {
                UserSettingService.Instance.SelectedCaltureName = CultureInfo.CurrentCulture.Name switch {
                    "ja-JP" => "ja-JP", // 日本語
                    _ => "en-001", // 英語
                };
            }
            CultureInfo cultureInfo = new(UserSettingService.Instance.SelectedCaltureName);

            MyResources.Culture = cultureInfo;
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            // 設定をリソースに登録する
            App.RegisterToResource();

#if !DEBUG
            // 多重起動を抑止する
            App.mMutex = new(false, this.GetType().Assembly.GetName().Name);
            if (!mMutex.WaitOne(TimeSpan.Zero, false)) {
                Process curProcess = Process.GetCurrentProcess();
                Process[] processList = Process.GetProcessesByName(curProcess.ProcessName);

                if (processList.Length >= 2) {
                    foreach (Process process in processList) {
                        if (process.Id != curProcess.Id) {
                            // 外部プロセスのアクティブ化したい(Win32を使わざるを得ない)
                        }
                    }
                }

                MessageBox.Show(MyResources.Message_DoNotDuplicateProcess, MyResources.Title_Warning);
                this.Shutdown();
                return;
            }
#endif

            // 前バージョンからのUpgradeを実行していないときはUpgradeを実施する
            Version assemblyVersion = GetAssemblyVersion();
            if (UserSettingService.Instance.LastAppVersion < assemblyVersion) {
                // Upgradeを実行する
                HouseholdAccountBook.Properties.Settings.Default.Upgrade();
            }

            DbHandlerFactory dbHandlerFactory = await this.GetDbHandlerFactory();
            if (dbHandlerFactory == null) {
                // DB接続設定がキャンセルされた場合はアプリケーションを終了する
                this.Shutdown();
                return;
            }

            // DBバックアップマネージャーを初期化する
            DbBackUpManager.Instance.DbHandlerFactory = dbHandlerFactory;
            DbBackUpManager.Instance.Config = UserSettingService.Instance.DbBackupConfig;
            DbBackUpManager.Instance.NpgsqlBackupConfig = UserSettingService.Instance.PostgreSQLBackupConfig;
            DbBackUpManager.Instance.BackUpCurrentAtMinimizing = UserSettingService.Instance.CurrentBackUpAtMinimizing;

            // DBのマイグレーションを実行する
            bool migrateResult = await DbUtil.UpMigrateAsync(dbHandlerFactory);
            if (!migrateResult) {
                MessageBox.Show(MyResources.Message_FoultToMigrateDb, MyResources.Title_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                this.Shutdown();
                return;
            }

            // 初期化フラグを解除する
            UserSettingService.Instance.InitFlag = false;

            // 休日リストを取得する
            if (!await HolidayService.Instance.DownloadHolidayListAsync(UserSettingService.Instance.HolidayCSVConfig)) {
                // 祝日取得失敗を通知する
                Log.Warning("Failed to get holiday list.");
                NotificationService.NotifyFailingToGetHolidayList();
            }
            if (UserSettingService.Instance.CheckLatestVersionAtAppLaunched) {
                // 最新バージョンを確認する
                await App.CheckLatestVersionAsync(false);
            }

            // メインウィンドウを開く
            MainWindow mw = new(dbHandlerFactory);
            this.MainWindow = mw;
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
            mw.Show();
        }

        /// <summary>
        /// ハンドルされない例外発生時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            using FuncLog funcLog = new();

            try {
                Log.Error("Unhandled Exception Occured.");

                e.Handled = true;
                Log.Error($"Unhandled Exception Message: {e.Exception.Message}");

                // 例外情報をファイルに保存する
                ExceptionLog log = new();
                log.Log(e.Exception);
                Log.Info($"Create Unhandled Exception Info File: {log.RelatedFilePath}");

                // ハンドルされない例外の発生を通知する
                string absoluteFilePath = Path.Combine(GetCurrentDir(), log.RelatedFilePath);
                NotificationService.NotifyUnhandledException(absoluteFilePath);
            }
            catch (Exception ex) {
                Log.Error($"Exception Occured in Unhandled Exception Handler: {ex.Message}");
                Current.Shutdown(1); // 例外処理中に例外が発生した場合は強制終了
            }
        }

        /// <summary>
        /// アプリケーション終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void App_Exit(object sender, ExitEventArgs e)
        {
            using FuncLog funcLog = new();

            // 多重起動防止用のMutexを解放する
            ReleaseMutex();
        }

        /// <summary>
        /// 設定をリソースに登録する
        /// </summary>
        public static void RegisterToResource()
        {
            using FuncLog funcLog = new();

            ResourceDictionary rd = Current.Resources;
            if (rd.Contains("Settings")) {
                rd["Settings"] = HouseholdAccountBook.Properties.Settings.Default;
            }
            if (rd.Contains("AppCulture")) {
                rd["AppCulture"] = MyResources.Culture;
            }
        }

        /// <summary>
        /// DBハンドラファクトリを取得する
        /// </summary>
        /// <returns>DBハンドラファクトリ</returns>
        /// <exception cref="NotSupportedException"></exception>
        private async Task<DbHandlerFactory> GetDbHandlerFactory()
        {
            using FuncLog funcLog = new();

            DbHandlerFactory dbHandlerFactory = null;
            bool isOpen = false;
            bool tryConnect = !UserSettingService.Instance.InitFlag; // 接続を試みるか
            string message = MyResources.Message_PleaseInputDbSetting;

            while (true) {
                // 接続する場合
                if (tryConnect) {
                    // 接続設定を読み込む
                    DbHandlerBase.ConnectInfoBase connInfo = GetDbConnectInfo();
                    dbHandlerFactory = new(connInfo);

                    // 接続を試行する
                    try {
                        await using (DbHandlerBase dbHandler = await dbHandlerFactory.CreateAsync()) {
                            isOpen = dbHandler.IsOpen;
                        }
                    }
                    catch (TimeoutException) { }

                    if (isOpen) {
                        // DB接続に成功した場合はループを抜ける
                        Log.Info("Database connection succeeded.");
                        break;
                    }
                    else {
                        // SQLiteの場合
                        if (connInfo is SQLiteDbHandler.ConnectInfo sqliteInfo) {
                            // DBファイルが存在しない場合は新規作成を試みる
                            if (!File.Exists(sqliteInfo.FilePath)) {
                                // SQLiteのテンプレートファイルをコピーして新規作成する
                                byte[] sqliteBinary = MyResources.SQLiteTemplateFile;
                                if (SQLiteDbHandler.CreateTemplateFile(sqliteInfo.FilePath, sqliteBinary)) {
                                    continue; // 作成に成功した場合は再度接続を試みる
                                }
                            }
                            else {
                                Log.Warning("Failed to connect to existing SQLite database file.");
                            }
                        }
                    }
                }

                // データベース接続を設定する
                DbSettingWindow dsw = new(null, message);
                this.MainWindow = dsw;
                // DB設定ダイアログ終了時に閉じないように設定する(明示的なシャットダウンが必要)
                this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                dsw.SetIsModal(true);
                if (dsw.ShowDialog() != true) {
                    // DB設定ダイアログでキャンセルされた場合はnullを返す
                    return null;
                }

                // 接続を試みる(SQLiteの場合はこの時点でファイルが作成済となっている)
                tryConnect = true;
                message = MyResources.Message_FoultToConnectDb;
            }

            return dbHandlerFactory;
        }

        /// <summary>
        /// DB接続情報を取得する
        /// </summary>
        /// <returns>DB接続情報</returns>
        /// <exception cref="NotSupportedException">サポート対象外のDBが設定されている場合</exception>
        public static DbHandlerBase.ConnectInfoBase GetDbConnectInfo()
        {
            using FuncLog funcLog = new();

            DbHandlerBase.ConnectInfoBase connInfo = null;
            switch (UserSettingService.Instance.SelectedDBKind) {
                case DBKind.PostgreSQL: {
                    connInfo = UserSettingService.Instance.NpgsqlConnectInfo;
                    break;
                }
                case DBKind.SQLite:
                    connInfo = UserSettingService.Instance.SQLiteConnectInfo;
                    break;
                case DBKind.Access:
                case DBKind.Undefined:
                default:
                    throw new NotSupportedException();
            }

            return connInfo;
        }

        /// <summary>
        /// 多重起動防止用のMutexを開放する
        /// </summary>
        public static void ReleaseMutex()
        {
            using FuncLog funcLog = new();

#if !DEBUG
            if (mMutex != null) {
                mMutex.ReleaseMutex();
                mMutex.Close();
                mMutex = null;
            }
#endif
        }

        /// <summary>
        /// アプリケーションを再起動する
        /// </summary>
        public void Restart(bool locationSaveEnabled = true)
        {
            using FuncLog funcLog = new();

            WindowLocationManager.SaveEnabled = locationSaveEnabled;

            string targetExe = GetCurrentExe();
            Log.Debug($"target: {targetExe}");

            ReleaseMutex();

            _ = Process.Start(targetExe);

            this.Exit -= this.App_Exit;
            this.Shutdown();
        }

        /// <summary>
        /// 実行ファイルのパスを取得する
        /// </summary>
        /// <returns>実行ファイルのパス</returns>
        public static string GetCurrentExe() => Environment.ProcessPath;

        /// <summary>
        /// 実行ファイルの存在するフォルダのパスを取得する
        /// </summary>
        /// <returns>実行ファイルの存在するフォルダのパス</returns>
        public static string GetCurrentDir() => Path.GetDirectoryName(GetCurrentExe());

        /// <summary>
        /// アセンブリバージョンを取得する
        /// </summary>
        /// <returns></returns>
        public static Version GetAssemblyVersion() => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

        /// <summary>
        /// 最新バージョンを確認する
        /// </summary>
        /// <param name="notifyCurrentIsLatest">現バージョンが最新でも通知するか</param>
        /// <returns></returns>
        public static async Task CheckLatestVersionAsync(bool notifyCurrentIsLatest)
        {
            // 最新バージョン情報を取得する
            if (!await AppVersionService.Instance.GetLatestInfoAsync()) {
                // 最新バージョン情報取得失敗を通知する
                Log.Warning("Failed to get letest info.");
                NotificationService.NotifyFailingToGetLatestVersionNumber();
            }
            else {
                Version latest = AppVersionService.Instance.GetLatestVersion();
                string latestHtmlUrl = AppVersionService.Instance.GetLatestHtmlUrl();
                Log.Info($"Latest: Ver. {latest}");

                if (App.GetAssemblyVersion() < latest) {
                    NotificationService.NotifyLatestVersionNumber(latest, latestHtmlUrl);
                }
                else if (notifyCurrentIsLatest) {
                    NotificationService.NotifyCurrentIsLatestVersion();
                }
            }
        }
    }
}

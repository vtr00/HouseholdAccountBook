﻿using HouseholdAccountBook.DbHandler;
using HouseholdAccountBook.DbHandler.Abstract;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Others;
using HouseholdAccountBook.Windows;
using Notification.Wpf;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

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
        public DateTime StartupTime => this._startupTime;
        private readonly DateTime _startupTime = DateTime.Now;
#pragma warning restore IDE0032 // 自動プロパティを使用する
        #endregion

        #region フィールド
#if !DEBUG
        /// <summary>
        /// 多重起動抑止用Mutex
        /// </summary>
        private static Mutex mutex = null;
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
#if DEBUG
            Trace.Listeners.Add(new ConsoleTraceListener());
#endif

            Log.Info("Application Startup");

            this.DispatcherUnhandledException += this.App_DispatcherUnhandledException;
            this.Exit += this.App_Exit;

            // shift-jis を使用するために必要
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Properties.Settings settings = HouseholdAccountBook.Properties.Settings.Default;

            Log.Info($"Current Culture: {CultureInfo.CurrentCulture.Name}");
            Log.Info($"Application Culture: {settings.App_CultureName}");
            // 言語の初期設定がない場合
            if (string.IsNullOrEmpty(settings.App_CultureName)) {
                settings.App_CultureName = CultureInfo.CurrentCulture.Name switch {
                    "ja-JP" => "ja-JP", // 日本語
                    _ => "en-001", // 英語
                };
                settings.Save();
            }
            CultureInfo cultureInfo = new(settings.App_CultureName);

            HouseholdAccountBook.Properties.Resources.Culture = cultureInfo;
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            // 設定をリソースに登録する
            App.RegisterToResource();

#if !DEBUG
            // 多重起動を抑止する
            App.mutex = new(false, this.GetType().Assembly.GetName().Name);
            if (!mutex.WaitOne(TimeSpan.Zero, false)) {
                Process curProcess = Process.GetCurrentProcess();
                Process[] processList = Process.GetProcessesByName(curProcess.ProcessName);

                if (processList.Length >= 2) {
                    foreach (Process process in processList) {
                        if (process.Id != curProcess.Id) {
                            // 外部プロセスのアクティブ化したい(Win32を使わざるを得ない)
                        }
                    }
                }

                MessageBox.Show(HouseholdAccountBook.Properties.Resources.Message_DoNotDuplicateProcess, HouseholdAccountBook.Properties.Resources.Title_Warning);
                this.Shutdown();
                return;
            }
#endif

            // 前バージョンからのUpgradeを実行していないときはUpgradeを実施する
            Version assemblyVersion = GetAssemblyVersion();
            if (!Version.TryParse(settings.App_Version, out Version preVersion) || preVersion < assemblyVersion) {
                // Upgradeを実行する
                settings.Upgrade();
            }

            // 初回起動時
            Log.Debug($"App_InitFlag: {settings.App_InitFlag}");
            if (settings.App_InitFlag) {
#if !DEBUG
                // リリースビルドの初回起動時デバッグモードはOFF
                settings.App_IsDebug = false;
#endif
            }

            // DB設定ダイアログ終了時に閉じないように設定する(明示的なシャットダウンが必要)
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            DbHandlerFactory dbHandlerFactory = null;
            bool isOpen = false;
            while (!isOpen) {
                // 初回起動時、またはDB接続に失敗した時
                if (settings.App_InitFlag || dbHandlerFactory is not null) {
                    // データベース接続を設定する
                    string message = dbHandlerFactory is not null
                        ? HouseholdAccountBook.Properties.Resources.Message_FoultToConnectDb
                        : HouseholdAccountBook.Properties.Resources.Message_PleaseInputDbSetting;
                    DbSettingWindow dsw = new(message);
                    bool? result = dsw.ShowDialog();

                    if (result != true) {
                        this.Shutdown();
                        return;
                    }
                }

                // 接続設定を読み込む
                DbHandlerBase.ConnectInfo connInfo = null;
                switch ((DbConstants.DBKind)settings.App_SelectedDBKind) {
                    case DbConstants.DBKind.PostgreSQL: {
                        NpgsqlDbHandler.ConnectInfo connectInfo = new() {
                            Host = settings.App_Postgres_Host,
                            Port = settings.App_Postgres_Port,
                            UserName = settings.App_Postgres_UserName,
                            Password = settings.App_Postgres_Password,
#if DEBUG
                            DatabaseName = settings.App_Postgres_DatabaseName_Debug,
#else
                            DatabaseName = settings.App_Postgres_DatabaseName,
#endif
                            Role = settings.App_Postgres_Role
                        };
                        if (settings.App_Postgres_Password == string.Empty) {
                            connectInfo.EncryptedPassword = settings.App_Postgres_EncryptedPassword;
                        }
                        else {
                            connectInfo.Password = settings.App_Postgres_Password;
                            settings.App_Postgres_EncryptedPassword = connectInfo.EncryptedPassword;
                            settings.App_Postgres_Password = string.Empty; // パスワードは保存しない
                            settings.Save();
                        }
                        connInfo = connectInfo;
                        break;
                    }
                    case DbConstants.DBKind.SQLite:
                        connInfo = new SQLiteDbHandler.ConnectInfo() {
                            FilePath = settings.App_SQLite_DBFilePath
                        };
                        break;
                    case DbConstants.DBKind.Access:
                    case DbConstants.DBKind.Undefined:
                    default:
                        throw new NotSupportedException();
                }
                dbHandlerFactory = new(connInfo);

                // 接続を試行する
                try {
                    await using (DbHandlerBase dbHandler = await dbHandlerFactory.CreateAsync()) {
                        isOpen = dbHandler.IsOpen;
                    }
                }
                catch (TimeoutException) { }
            }

            // 初回起動を解除する
            settings.App_InitFlag = false;
            settings.Save();

            // 休日リストを取得する
            await DateTimeExtensions.DownloadHolidayListAsync();

            // DBに接続できる場合だけメインウィンドウを開く
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
            try {
                Log.Error("Unhandled Exception Occured.");

                e.Handled = true;
                Log.Error($"Unhandled Exception Message: {e.Exception.Message}");

                // 例外情報をファイルに保存する
                ExceptionLog log = new();
                log.Log(e.Exception);
                Log.Info($"Create Unhandled Exception Info File: {log.RelatedFilePath}");

                // ハンドルされない例外の発生を通知する
                NotificationManager nm = new();
                NotificationContent nc = new() {
                    Title = Current.MainWindow?.Title ?? "",
                    Message = HouseholdAccountBook.Properties.Resources.Message_UnhandledExceptionOccurred,
                    Type = NotificationType.Warning
                };
                nm.Show(nc, expirationTime: new TimeSpan(0, 0, 10), onClick: () => {
                    string absoluteFilePath = Path.Combine(GetCurrentDir(), log.RelatedFilePath);
                    Log.Info($"Create Unhandled Exception Info Absolute File: {absoluteFilePath}");
                    _ = Process.Start(new ProcessStartInfo() {
                        FileName = absoluteFilePath,
                        UseShellExecute = true
                    });
                    if (Current.MainWindow == null) {
                        Current.Shutdown(1); // メインウィンドウがない場合は強制終了
                    }
                });
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
            Log.Info("Application End");

            ReleaseMutex();
            Log.Close();
        }

        /// <summary>
        /// 設定をリソースに登録する
        /// </summary>
        public static void RegisterToResource()
        {
            ResourceDictionary rd = Current.Resources;
            if (rd.Contains("Settings")) {
                rd["Settings"] = HouseholdAccountBook.Properties.Settings.Default;
            }
            if (rd.Contains("AppCulture")) {
                rd["AppCulture"] = HouseholdAccountBook.Properties.Resources.Culture;
            }
        }

        /// <summary>
        /// 多重起動防止用のMutexを開放する
        /// </summary>
        public static void ReleaseMutex()
        {
#if !DEBUG
            if (mutex != null) {
                mutex.ReleaseMutex();
                mutex.Close();
                mutex = null;
            }
#endif
        }

        /// <summary>
        /// アプリケーションを再起動する
        /// </summary>
        public void Restart()
        {
            Log.Info("Application Restart");

            string targetExe = GetCurrentExe();
            Log.Debug($"target: {targetExe}");

            App.ReleaseMutex();
            Log.Close();

            _ = Process.Start(targetExe);

            this.Exit -= this.App_Exit;
            this.Shutdown();
        }

        /// <summary>
        /// 実行ファイルのパスを取得する
        /// </summary>
        /// <returns>実行ファイルのパス</returns>
        public static string GetCurrentExe()
        {
            return Environment.ProcessPath;
        }

        /// <summary>
        /// 実行ファイルの存在するフォルダのパスを取得する
        /// </summary>
        /// <returns>実行ファイルの存在するフォルダのパス</returns>
        public static string GetCurrentDir()
        {
            return Path.GetDirectoryName(GetCurrentExe());
        }

        /// <summary>
        /// アセンブリバージョンを取得する
        /// </summary>
        /// <returns></returns>
        public static Version GetAssemblyVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        }
    }
}

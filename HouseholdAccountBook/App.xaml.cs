﻿using HouseholdAccountBook.DbHandler;
using HouseholdAccountBook.DbHandler.Abstract;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Windows;
using Newtonsoft.Json;
using Notifications.Wpf;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
        static public DateTime StartupTime = DateTime.Now;
        #endregion

        #region フィールド
        /// <summary>
        /// 接続情報
        /// </summary>
        private DbHandlerBase.ConnectInfo connectInfo = null;
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
            Debug.Listeners.Add(new ConsoleTraceListener());
#endif

            Log.Info("Application Startup");

            this.DispatcherUnhandledException += this.App_DispatcherUnhandledException;
            this.Exit += this.App_Exit;

            Properties.Settings settings = HouseholdAccountBook.Properties.Settings.Default;

            // 言語設定
            Log.Info($"Application Culture: {settings.App_CultureName}");
            System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo(settings.App_CultureName);

            HouseholdAccountBook.Properties.Resources.Culture = cultureInfo;
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

#if !DEBUG
            // 多重起動を抑止する
            App.mutex = new Mutex(false, this.GetType().Assembly.GetName().Name);
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
            Version assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            if (!Version.TryParse(settings.App_Version, out Version preVersion) || preVersion < assemblyVersion) {
                // Upgradeを実行する
                settings.Upgrade();
            }

            // 初回起動時
            Log.Debug("App_InitFlag: " + settings.App_InitFlag);
            if (settings.App_InitFlag) {
#if !DEBUG
                // リリースビルドの初回起動時デバッグモードはOFF
                settings.App_IsDebug = false;
#endif
                // DB設定ダイアログ終了時に閉じないように設定する(明示的なシャットダウンが必要)
                this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                // データベース接続を設定する
                DbSettingWindow dsw = new DbSettingWindow(HouseholdAccountBook.Properties.Resources.Message_PleaseInputDbSetting);
                bool? result = dsw.ShowDialog();

                if (result != true) {
                    this.connectInfo = null;
                    this.Shutdown();
                    return;
                }

                settings.App_InitFlag = false;
                settings.Save();
            }

            DbHandlerFactory dbHandlerFactory = null;
            while (true) {
                // 接続設定を読み込む
                this.connectInfo = new NpgsqlDbHandler.ConnectInfo() {
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
                dbHandlerFactory = new DbHandlerFactory(this.connectInfo);

                // 接続を試行する
                bool isOpen = false;
                try {
                    using (DbHandlerBase dbHandler = dbHandlerFactory.Create()) {
                        isOpen = dbHandler.IsOpen;
                    }
                }
                catch (TimeoutException) { }

                if (isOpen) {
                    break;
                }
                else {
                    // データベース接続を設定する
                    DbSettingWindow dsw = new DbSettingWindow(HouseholdAccountBook.Properties.Resources.Message_FoultToConnectDb);
                    bool? result = dsw.ShowDialog();

                    if (result != true) {
                        this.connectInfo = null;
                        this.Shutdown();
                        return;
                    }
                }
            }

            // 休日リストを取得する
            await DateTimeExtensions.DownloadHolidayListAsync();

            // 設定をリソースに登録する
            this.RegisterToResource();

            // DBに接続できる場合だけメインウィンドウを開く
            MainWindow mw = new MainWindow(dbHandlerFactory);
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

                // 例外情報をファイルに保存する
                ExceptionLog log = new ExceptionLog();
                log.Log(e.Exception);
                Log.Info("Create Unhandled Exception Info File:" + log.RelatedFilePath);

                // ハンドルされない例外の発生を通知する
                NotificationManager nm = new NotificationManager();
                NotificationContent nc = new NotificationContent() {
                    Title = Current.MainWindow?.Title ?? "",
                    Message = HouseholdAccountBook.Properties.Resources.Message_UnhandledExceptionOccurred,
                    Type = NotificationType.Warning
                };
                nm.Show(nc, expirationTime: new TimeSpan(0, 0, 10), onClick: () => {
                    string absoluteFilePath = Path.Combine(GetCurrentDir(), log.RelatedFilePath);
                    Log.Info("Create Unhandled Exception Info Absolute File:" + absoluteFilePath);
                    Process.Start(absoluteFilePath);
                });
            }
            catch (Exception) { }
        }

        /// <summary>
        /// アプリケーション終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void App_Exit(object sender, ExitEventArgs e)
        {
            Log.Info("Application End");

            this.ReleaseMutex();
            Log.Close();
        }

        /// <summary>
        /// 設定をリソースに登録する
        /// </summary>
        public void RegisterToResource()
        {
            ResourceDictionary rd = Current.Resources;
            if (rd.Contains("Settings")) {
                Properties.Settings settings = HouseholdAccountBook.Properties.Settings.Default;
                rd["Settings"] = settings;
            }
            if (rd.Contains("AppCulture")) {
                rd["AppCulture"] = HouseholdAccountBook.Properties.Resources.Culture;
            }
        }

        /// <summary>
        /// 多重起動防止用のMutexを開放する
        /// </summary>
        public void ReleaseMutex()
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

            this.ReleaseMutex();
            Log.Close();

            Process.Start(Application.ResourceAssembly.Location);

            this.Shutdown();
        }

        /// <summary>
        /// 実行ファイルの存在するフォルダのパスを取得する
        /// </summary>
        /// <returns>実行ファイルの存在するフォルダのパス</returns>
        public static string GetCurrentDir()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }
    }
}

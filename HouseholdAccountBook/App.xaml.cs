﻿using HouseholdAccountBook.Dao;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Notifications.Wpf;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        #region フィールド
        /// <summary>
        /// 接続情報
        /// </summary>
        private DaoNpgsql.ConnectInfo connectInfo = null;
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
        private void App_Startup(object sender, StartupEventArgs e)
        {
            Properties.Settings settings = HouseholdAccountBook.Properties.Settings.Default;

#if DEBUG
            Debug.Listeners.Add(new ConsoleTraceListener());
#endif

#if !DEBUG
            // 多重起動を抑止する
            App.mutex = new Mutex(false, this.GetType().Assembly.GetName().Name);
            if (!mutex.WaitOne(TimeSpan.Zero, false)) {
                Process curProcess = Process.GetCurrentProcess();
                Process[] processList = Process.GetProcessesByName(curProcess.ProcessName);

                if (processList.Length >= 2) {
                    foreach (Process process in processList) {
                        if (process.ProcessName != curProcess.ProcessName) {
                            // 外部プロセスのアクティブ化
                        }
                    }
                }

                MessageBox.Show("同時に複数起動することはできません。", MessageTitle.Exclamation);
                this.Shutdown();
                return;
            }
#endif

            this.DispatcherUnhandledException += this.App_DispatcherUnhandledException;
            this.Exit += this.App_Exit;

            // DB設定ダイアログ終了時に閉じないように設定する
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // 前バージョンからのUpgradeを実行していないときはUpgradeを実施する
            Version assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            if (!Version.TryParse(settings.App_Version, out Version preVersion) || preVersion < assemblyVersion) {
                // Upgradeを実行する
                settings.Upgrade();
            }

            // 初回起動時
            if (settings.App_InitFlag) {
                // データベース接続を設定する
                DbSettingWindow dsw = new DbSettingWindow("DB接続設定を入力してください。");
                bool? result = dsw.ShowDialog();

                if (result != true) {
                    this.connectInfo = null;
                    this.Shutdown();
                    return;
                }

                settings.App_InitFlag = false;
                settings.Save();
            }

            DaoBuilder builder = null;
            while (true) {
                // 接続設定を読み込む
                this.connectInfo = new DaoNpgsql.ConnectInfo() {
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
                builder = new DaoBuilder(this.connectInfo);

                // 接続を試行する
                bool isOpen = false;
                using (DaoBase dao = builder.Build()) {
                    isOpen = dao.IsOpen;
                }

                if (isOpen) {
                    break;
                }
                else {
                    // データベース接続を設定する
                    DbSettingWindow dsw = new DbSettingWindow("接続に失敗しました。接続設定を見直してください。");
                    bool? result = dsw.ShowDialog();

                    if (result != true) {
                        this.connectInfo = null;
                        this.Shutdown();
                        return;
                    }
                }
            }

            // 休日リストを取得する
            DateTimeExtensions.DownloadHolidayListAsync();

            // 接続できる場合だけメインウィンドウを開く
            MainWindow mw = new MainWindow(builder);
            this.MainWindow = mw;
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
            mw.Show();
        }

        /// <summary>
        /// ハンドルされない例外発生時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try {
                e.Handled = true;

                // 例外情報をファイルに保存する
                string jsonCode = JsonConvert.SerializeObject(e.Exception);
                using (FileStream fs = new FileStream(UnhandledExceptionInfoFileName, FileMode.Create)) {
                    using (StreamWriter sw = new StreamWriter(fs)) {
                        await sw.WriteLineAsync(jsonCode);
                    }
                }

                // ハンドルされない例外の発生を通知する
                NotificationManager nm = new NotificationManager();
                NotificationContent nc = new NotificationContent() {
                    Title = Current.MainWindow.Title,
                    Message = MessageText.UnhandledExceptionOccured,
                    Type = NotificationType.Warning
                };
                nm.Show(nc, expirationTime: new TimeSpan(0, 0, 10), onClick: () => {
                    Process.Start(UnhandledExceptionInfoFileName);
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
            this.ReleaseMutex();
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
    }
}
